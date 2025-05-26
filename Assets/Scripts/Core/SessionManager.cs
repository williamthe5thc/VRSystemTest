using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using VRInterview.Network;
using VRInterview; // Add UIManager namespace

/// <summary>
/// Manages the interview session state and communication with the server.
/// </summary>
public class SessionManager : MonoBehaviour
{
    [SerializeField] private WebSocketClient webSocketClient;
    [SerializeField] private MessageHandler messageHandler;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AvatarController avatarController;
    
    [Header("Session Settings")]
    [SerializeField] private float pingInterval = 5.0f;
    [SerializeField] private float reconnectDelaySeconds = 2.0f;
    [SerializeField] private int maxReconnectAttempts = 3;
    
    // Session state
    private string _sessionId;
    private string _currentState = "IDLE";
    private bool _isSessionActive = false;
    private int _reconnectAttemptCount = 0;
    private Dictionary<string, object> _sessionMetadata = new Dictionary<string, object>();
    
    // Events for message handling
    public event Action<string> OnSessionStart;
    
    // Original events
    public event Action<string, string> OnStateChanged;
    public event Action OnSessionStarted;
    public event Action OnSessionEnded;
    public event Action<string> OnConnectionError;
    public event Action OnResponseComplete;
    
    // Transcript handling methods
    private List<string> userMessages = new List<string>();
    private List<string> assistantMessages = new List<string>();
    
    /// <summary>
    /// Add a user message to the transcript
    /// </summary>
    public void AddUserMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;
            
        userMessages.Add(message);
        Debug.Log($"Added user message: {message.Substring(0, Math.Min(30, message.Length))}...");
    }
    
    /// <summary>
    /// Add an assistant message to the transcript
    /// </summary>
    public void AddAssistantMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;
            
        assistantMessages.Add(message);
        Debug.Log($"Added assistant message: {message.Substring(0, Math.Min(30, message.Length))}...");
    }
    
    /// <summary>
    /// Get the most recent user message
    /// </summary>
    public string GetLatestUserMessage()
    {
        if (userMessages.Count == 0)
            return string.Empty;
            
        return userMessages[userMessages.Count - 1];
    }
    
    /// <summary>
    /// Get the most recent assistant message
    /// </summary>
    public string GetLatestAssistantMessage()
    {
        if (assistantMessages.Count == 0)
            return string.Empty;
            
        return assistantMessages[assistantMessages.Count - 1];
    }
    
    // Feedback
    private List<string> sessionFeedback = new List<string>();
    
    // Properties
    public string SessionId => _sessionId;
    public string CurrentState => _currentState;
    public bool IsSessionActive => _isSessionActive;
    public Dictionary<string, object> SessionMetadata => _sessionMetadata;
    
    // Static instance for global access
    private static SessionManager _instance;
    public static SessionManager Instance => _instance;
    
    private void Awake()
    {
        // Set up singleton instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // Try to load saved session ID first
        string savedSessionId = PlayerPrefs.GetString("CurrentSessionId", "");
        if (!string.IsNullOrEmpty(savedSessionId))
        {
            _sessionId = savedSessionId;
            Debug.Log($"Restored saved session ID: {_sessionId}");
        }
        else
        {
            // Generate a random session ID (will be replaced by server-provided ID when connected)
            _sessionId = System.Guid.NewGuid().ToString();
            Debug.Log($"Generated new session ID: {_sessionId}");
        }
    }
    
    // Dictionary to map client-generated IDs to server IDs
    private Dictionary<string, string> _sessionMappings = new Dictionary<string, string>();
    
    // Method to update session ID from server
    public void UpdateSessionId(string serverSessionId)
    {
        if (!string.IsNullOrEmpty(serverSessionId))
        {
            string oldId = _sessionId;
            
            // Store mapping between old and new ID if they're different
            if (oldId != serverSessionId && !_sessionMappings.ContainsKey(oldId))
            {
                _sessionMappings[oldId] = serverSessionId;
                Debug.Log($"Added session ID mapping: {oldId} → {serverSessionId}");
            }
            
            Debug.Log($"Updating session ID from {_sessionId} to server-provided {serverSessionId}");
            _sessionId = serverSessionId;
            
            // Store session ID in PlayerPrefs to maintain it across restarts
            PlayerPrefs.SetString("CurrentSessionId", _sessionId);
            PlayerPrefs.Save();
            
            // Notify any components that need to know about the session ID change
            OnSessionIdChanged?.Invoke(_sessionId);
        }
    }
    
    /// <summary>
    /// Get the server session ID for a client-generated ID
    /// </summary>
    /// <param name="clientId">Client-generated session ID</param>
    /// <returns>Server session ID if mapped, otherwise the input ID</returns>
    public string GetServerSessionId(string clientId)
    {
        if (_sessionMappings.TryGetValue(clientId, out string serverId))
        {
            return serverId;
        }
        return clientId; // Return original if no mapping exists
    }
    
    // Event for session ID changes
    public event Action<string> OnSessionIdChanged;
    
    private void Start()
    {
        // Register for message events
        if (messageHandler != null)
        {
            messageHandler.OnStateUpdate += HandleStateUpdate;
            messageHandler.OnError += HandleServerError;
        }
        else
        {
            Debug.LogError("MessageHandler not assigned to SessionManager!");
        }
        
        // Register for WebSocket events
        if (webSocketClient != null)
        {
            webSocketClient.OnConnected += HandleConnected;
            webSocketClient.OnDisconnected += HandleDisconnected;
            webSocketClient.OnError += HandleConnectionError;
        }
        else
        {
            Debug.LogError("WebSocketClient not assigned to SessionManager!");
        }
        
        // Start ping coroutine
        StartCoroutine(SendPingMessages());
    }
    
    /// <summary>
    /// Starts a new interview session.
    /// </summary>
    public async Task StartSession()
    {
        if (_isSessionActive)
        {
            Debug.LogWarning("Session already active. Call EndSession first to start a new one.");
            return;
        }
        
        // Make sure we're connected
        if (webSocketClient != null && !webSocketClient.IsConnected)
        {
            await webSocketClient.Connect();
            
            // Wait for connection to establish
            float connectionTimeout = 5.0f;
            float elapsed = 0f;
            
            while (!webSocketClient.IsConnected && elapsed < connectionTimeout)
            {
                await Task.Delay(100);
                elapsed += 0.1f;
            }
            
            if (!webSocketClient.IsConnected)
            {
                Debug.LogError("Failed to connect to server.");
                OnConnectionError?.Invoke("Failed to connect to server.");
                return;
            }
        }
        
        // Reset session
        _currentState = "IDLE";
        _isSessionActive = true;
        _sessionMetadata.Clear();
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateStateDisplay(_currentState);
        }
        
        // Notify session start
        OnSessionStarted?.Invoke();
        
        // Send session start message
        await SendControlMessage("start");
        
        Debug.Log($"Session started: {_sessionId}");
    }
    
    /// <summary>
    /// Ends the current interview session.
    /// </summary>
    public async Task EndSession()
    {
        if (!_isSessionActive)
        {
            return;
        }
        
        // Send session end message
        await SendControlMessage("end");
        
        // Clean up session
        _isSessionActive = false;
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateStateDisplay("IDLE");
        }
        
        // Notify session end
        OnSessionEnded?.Invoke();
        
        Debug.Log($"Session ended: {_sessionId}");
    }
    
    /// <summary>
    /// Resets the current interview session.
    /// </summary>
    public async Task ResetSession()
    {
        // Send reset message
        await SendControlMessage("reset");
        
        // Reset state
        _currentState = "IDLE";
        _sessionMetadata.Clear();
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateStateDisplay(_currentState);
        }
        
        // Reset avatar state
        if (avatarController != null)
        {
            avatarController.SetIdleState();
        }
        
        Debug.Log($"Session reset: {_sessionId}");
    }
    
    /// <summary>
    /// Sends a control message to the server.
    /// </summary>
    /// <param name="action">The control action.</param>
    private async Task SendControlMessage(string action)
    {
        if (webSocketClient == null || !webSocketClient.IsConnected)
        {
            Debug.LogWarning("Cannot send control message: WebSocket not connected.");
            return;
        }
        
        try
        {
            var message = new ControlMessage
            {
                type = "control",
                session_id = _sessionId,
                action = action,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            };
            
            string jsonMessage = JsonConvert.SerializeObject(message);
            await webSocketClient.SendMessage(jsonMessage);
            
            Debug.Log($"Sent control message: {action}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending control message: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Handles state update messages from the server.
    /// </summary>
    /// <param name="previousState">Previous state.</param>
    /// <param name="newState">New state.</param>
    /// <param name="metadata">State metadata.</param>
    private void HandleStateUpdate(string previousState, string newState, Dictionary<string, object> metadata)
    {
        string oldState = _currentState;
        _currentState = newState;
        
        // Update session metadata
        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                _sessionMetadata[kvp.Key] = kvp.Value;
            }
        }
        
        // Special handling for PROCESSING state
        if (newState == "PROCESSING" && uiManager != null)
        {
            // Display thinking message when entering processing state
            // without clearing user transcript
            uiManager.ClearTranscript();
            Debug.Log("Set 'thinking' message due to PROCESSING state");
        }
        
        // Notify state change
        OnStateChanged?.Invoke(oldState, newState);
        
        Debug.Log($"Session state changed: {oldState} -> {newState}");
    }
    
    /// <summary>
    /// Handles server error messages.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    private void HandleServerError(string errorMessage)
    {
        Debug.LogError($"Server error: {errorMessage}");
        
        // Show error in UI
        if (uiManager != null)
        {
            uiManager.ShowError(errorMessage);
        }
    }
    
    /// <summary>
    /// Handles successful WebSocket connection.
    /// </summary>
    private void HandleConnected()
    {
        Debug.Log("Connected to server");
        _reconnectAttemptCount = 0;
        
        // Show connection status
        if (uiManager != null)
        {
            uiManager.UpdateConnectionStatus(true);
        }
        
        // Send client capabilities first
        SendClientCapabilities();
    }
    
    /// <summary>
    /// Sends client capabilities to the server.
    /// </summary>
    public async void SendClientCapabilities()
    {
        if (webSocketClient == null || !webSocketClient.IsConnected)
        {
            Debug.LogWarning("Cannot send client capabilities: WebSocket not connected.");
            return;
        }
        
        try
        {
            // Create capabilities object
            ClientCapabilities capabilities = new ClientCapabilities();
            
            // Create message (constructor ensures type field is set)
            ClientCapabilitiesMessage message = new ClientCapabilitiesMessage(_sessionId, capabilities);
            
            // Convert to JSON using Newtonsoft.Json which handles serialization better
            string jsonMessage = JsonConvert.SerializeObject(message);
            
            // Verify the message has a type field
            if (!jsonMessage.Contains("\"type\":"))
            {
                Debug.LogWarning("Client capabilities missing type field - adding manually");
                // Fix by manually inserting right after the opening brace
                jsonMessage = jsonMessage.Insert(1, "\"type\":\"client_capabilities\",");
            }
            
            await webSocketClient.SendMessage(jsonMessage);
            
            Debug.Log("Sent client capabilities to server");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending client capabilities: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Handles WebSocket disconnection.
    /// </summary>
    /// <param name="closeCode">The WebSocket close code.</param>
    private void HandleDisconnected(NativeWebSocket.WebSocketCloseCode closeCode)
    {
        Debug.Log($"Disconnected from server: {closeCode}");
        
        // Show connection status
        if (uiManager != null)
        {
            uiManager.UpdateConnectionStatus(false);
        }
        
        // Try to reconnect if session was active
        if (_isSessionActive && _reconnectAttemptCount < maxReconnectAttempts)
        {
            _reconnectAttemptCount++;
            StartCoroutine(AttemptReconnect());
        }
        else if (_reconnectAttemptCount >= maxReconnectAttempts)
        {
            // Failed to reconnect
            _isSessionActive = false;
            OnSessionEnded?.Invoke();
            OnConnectionError?.Invoke("Failed to reconnect to server.");
            
            if (uiManager != null)
            {
                uiManager.ShowError("Failed to reconnect to server. Please check your network connection and try again.");
            }
        }
    }
    
    /// <summary>
    /// Handles connection errors.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    private void HandleConnectionError(string errorMessage)
    {
        Debug.LogError($"Connection error: {errorMessage}");
        OnConnectionError?.Invoke(errorMessage);
        
        // Show error in UI
        if (uiManager != null)
        {
            uiManager.ShowError($"Connection error: {errorMessage}");
        }
    }
    
    /// <summary>
    /// Coroutine for attempting reconnection.
    /// </summary>
    private IEnumerator AttemptReconnect()
    {
        Debug.Log($"Attempting to reconnect... ({_reconnectAttemptCount}/{maxReconnectAttempts})");
        
        // Show reconnecting message
        if (uiManager != null)
        {
            uiManager.ShowMessage($"Connection lost. Reconnecting ({_reconnectAttemptCount}/{maxReconnectAttempts})...");
        }
        
        // Wait before reconnecting (with exponential backoff)
        float waitTime = reconnectDelaySeconds * Mathf.Pow(2, _reconnectAttemptCount - 1);
        yield return new WaitForSeconds(waitTime);
        
        // Try to reconnect
        if (webSocketClient != null)
        {
            _ = webSocketClient.Connect();
        }
    }
    
    // Track the last activity time for optimizing ping frequency
    private float lastActivityTime;
    
    /// <summary>
    /// Update the activity timestamp when there's communication with the server
    /// </summary>
    public void UpdateActivityTimestamp()
    {
        lastActivityTime = Time.time;
    }
    
    /// <summary>
    /// Coroutine for sending periodic ping messages with adaptive intervals.
    /// </summary>
    private IEnumerator SendPingMessages()
    {
        // Initialize last activity time
        lastActivityTime = Time.time;
        
        // Start with the configured ping interval
        float currentInterval = pingInterval;
        
        while (true)
        {
            yield return new WaitForSeconds(currentInterval);
            
            if (webSocketClient != null && webSocketClient.IsConnected && _isSessionActive)
            {
                // Only send pings if there's been no recent activity
                float idleTime = Time.time - lastActivityTime;
                
                // If we've been inactive for at least half the ping interval
                if (idleTime > (pingInterval / 2))
                {
                    SendPingMessage();
                    
                    // If we're in an active state like PROCESSING or RESPONDING, use faster pings
                    if (_currentState == "PROCESSING" || _currentState == "RESPONDING")
                    {
                        // Use shorter interval during active processing
                        currentInterval = pingInterval / 2;
                    }
                    else
                    {
                        // Use normal interval during idle states
                        currentInterval = pingInterval;
                    }
                }
                else
                {
                    // Skip ping if there was recent activity
                    currentInterval = pingInterval;
                }
            }
        }
    }
    
    /// <summary>
    /// Sends a ping message to the server.
    /// </summary>
    private async void SendPingMessage()
    {
        try
        {
            var message = new PingMessage
            {
                type = "ping",
                session_id = _sessionId,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            };
            
            string jsonMessage = JsonConvert.SerializeObject(message);
            await webSocketClient.SendMessage(jsonMessage);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending ping message: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Notifies the server that audio playback is complete.
    /// </summary>
    public async void NotifyPlaybackComplete()
    {
        if (!_isSessionActive || webSocketClient == null || !webSocketClient.IsConnected)
        {
            return;
        }
        
        try
        {
            var message = new PlaybackCompleteMessage
            {
                type = "playback_complete",
                session_id = _sessionId,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            };
            
            string jsonMessage = JsonConvert.SerializeObject(message);
            await webSocketClient.SendMessage(jsonMessage);
            
            Debug.Log("Sent playback complete notification");
            
            // Trigger response complete event for feedback system
            OnResponseComplete?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending playback complete message: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets session metadata value.
    /// </summary>
    /// <typeparam name="T">Type of the metadata value.</typeparam>
    /// <param name="key">Metadata key.</param>
    /// <param name="defaultValue">Default value if not found.</param>
    /// <returns>Metadata value or default.</returns>
    public T GetMetadata<T>(string key, T defaultValue = default)
    {
        if (_sessionMetadata.TryGetValue(key, out object value))
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }
    
    /// <summary>
    /// Records user feedback about the interview response.
    /// </summary>
    /// <param name="feedbackType">Type of feedback (good, bad, neutral).</param>
    public async void RecordUserFeedback(string feedbackType)
    {
        // Add to local feedback list
        sessionFeedback.Add($"{_currentState}:{feedbackType}");
        
        // Send to server if connected
        if (_isSessionActive && webSocketClient != null && webSocketClient.IsConnected)
        {
            try
            {
                var message = new FeedbackMessage
                {
                    type = "feedback",
                    session_id = _sessionId,
                    feedback = feedbackType,
                    state = _currentState,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
                };
                
                string jsonMessage = JsonConvert.SerializeObject(message);
                await webSocketClient.SendMessage(jsonMessage);
                
                Debug.Log($"Sent user feedback: {feedbackType}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error sending feedback message: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Gets the current session ID.
    /// </summary>
    /// <returns>The session ID.</returns>
    public string GetSessionId()
    {
        return _sessionId;
    }
    
    /// <summary>
    /// Gets the WebSocketClient instance.
    /// </summary>
    /// <returns>The WebSocketClient instance.</returns>
    public WebSocketClient GetWebSocketClient()
    {
        return webSocketClient;
    }
    
    /// <summary>
    /// Pauses the current interview session.
    /// </summary>
    public async void PauseSession()
    {
        if (!_isSessionActive) return;
        
        // Send pause message to server
        await SendControlMessage("pause");
        
        Debug.Log("Session paused");
    }
    
    /// <summary>
    /// Resumes the current interview session.
    /// </summary>
    public async void ResumeSession()
    {
        if (!_isSessionActive) return;
        
        // Send resume message to server
        await SendControlMessage("resume");
        
        Debug.Log("Session resumed");
    }
    
    /// <summary>
    /// Message structures for various server communications.
    /// </summary>
    
    [Serializable]
    private class ControlMessage
    {
        public string type;
        public string session_id;
        public string action;
        public double timestamp;
    }
    
    [Serializable]
    private class PingMessage
    {
        public string type;
        public string session_id;
        public double timestamp;
    }
    
    [Serializable]
    private class PlaybackCompleteMessage
    {
        public string type;
        public string session_id;
        public double timestamp;
    }
    
    [Serializable]
    private class FeedbackMessage
    {
        public string type;
        public string session_id;
        public string feedback;
        public string state;
        public double timestamp;
    }
}