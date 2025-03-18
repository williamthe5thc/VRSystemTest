using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the connection state and reconnection logic for the WebSocket client.
/// </summary>
public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private WebSocketClient webSocketClient;
    [SerializeField] private UIManager uiManager;
    
    [Header("Connection Settings")]
    [SerializeField] private float pingInterval = 5.0f;
    [SerializeField] private float connectionTimeoutSec = 10.0f;
    [SerializeField] private int maxReconnectAttempts = 3;
    [SerializeField] private float initialReconnectDelay = 1.0f;
    [SerializeField] private float maxReconnectDelay = 10.0f;
    
    // Connection state
    private bool _isConnected = false;
    private int _reconnectAttemptCount = 0;
    private float _lastPingTime = 0f;
    private float _lastPongTime = 0f;
    private bool _isPingPending = false;
    
    // Events
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnConnectionError;
    
    // Connection state properties
    public bool IsConnected => _isConnected;
    public int ReconnectAttemptCount => _reconnectAttemptCount;
    
    private void Start()
    {
        // Register for WebSocket events
        if (webSocketClient != null)
        {
            webSocketClient.OnConnected += HandleConnected;
            webSocketClient.OnDisconnected += HandleDisconnected;
            webSocketClient.OnError += HandleConnectionError;
            webSocketClient.OnMessageReceived += HandleMessage;
        }
        else
        {
            Debug.LogError("WebSocketClient not assigned to ConnectionManager!");
        }
        
        // Start ping coroutine
        StartCoroutine(PingCoroutine());
    }
    
    private void Update()
    {
        // Check for ping timeout
        if (_isPingPending && Time.time - _lastPingTime > connectionTimeoutSec)
        {
            Debug.LogWarning("Ping timeout detected!");
            _isPingPending = false;
            
            // Connection likely lost, attempt reconnect
            if (_isConnected)
            {
                _isConnected = false;
                AttemptReconnect();
            }
        }
    }
    
    /// <summary>
    /// Handles successful connection.
    /// </summary>
    private void HandleConnected()
    {
        Debug.Log("WebSocket connected");
        _isConnected = true;
        _reconnectAttemptCount = 0;
        
        // Send initial ping
        SendPing();
        
        // Send client capabilities (ADD THIS)
        SendClientCapabilities();
        
        // Update UI if available
        if (uiManager != null)
        {
            uiManager.UpdateConnectionStatus(true);
        }
        
        // Notify listeners
        OnConnected?.Invoke();
    }
    
    // Enhanced method to send capabilities
    private async void SendClientCapabilities()
    {
        if (webSocketClient == null || !webSocketClient.IsConnected)
        {
            return;
        }
        
        try
        {
            // Get session ID from SessionManager if available
            string sessionId = "";
            var sessionManager = FindObjectOfType<SessionManager>();
            if (sessionManager != null)
            {
                sessionId = sessionManager.GetSessionId();
                Debug.Log($"Using SessionManager's session ID: {sessionId}");
            }
            else
            {
                Debug.LogWarning("SessionManager not found, using empty session ID");
            }
            
            // Create capabilities
            var capabilities = new VRInterview.Network.ClientCapabilities();
            var message = new VRInterview.Network.ClientCapabilitiesMessage(sessionId, capabilities);
            
            // Serialize and send
            string jsonMessage = JsonUtility.ToJson(message);
            Debug.Log($"Sending client capabilities: {jsonMessage}");
            
            await webSocketClient.SendMessage(jsonMessage);
            Debug.Log("Client capabilities sent successfully");
            
            // Send a second time after a short delay to ensure server receives it
            // This is useful in case the first message was sent before session IDs were synchronized
            //await Task.Delay(2000); // 2 second delay
            
            // Check if session ID has changed
            if (sessionManager != null)
            {
                string updatedSessionId = sessionManager.GetSessionId();
                if (updatedSessionId != sessionId)
                {
                    Debug.Log($"Session ID changed to {updatedSessionId}, sending updated capabilities");
                    // Create a new message with updated session ID
                    var updatedMessage = new VRInterview.Network.ClientCapabilitiesMessage(updatedSessionId, capabilities);
                    string updatedJson = JsonUtility.ToJson(updatedMessage);
                    await webSocketClient.SendMessage(updatedJson);
                    Debug.Log("Updated client capabilities sent");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending client capabilities: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Handles disconnection.
    /// </summary>
    /// <param name="closeCode">The WebSocket close code.</param>
    private void HandleDisconnected(NativeWebSocket.WebSocketCloseCode closeCode)
    {
        Debug.Log($"WebSocket disconnected: {closeCode}");
        _isConnected = false;
        
        // Update UI if available
        if (uiManager != null)
        {
            uiManager.UpdateConnectionStatus(false);
        }
        
        // Attempt reconnect if not a normal closure
        if (closeCode != NativeWebSocket.WebSocketCloseCode.Normal)
        {
            AttemptReconnect();
        }
        
        // Notify listeners
        OnDisconnected?.Invoke();
    }
    
    /// <summary>
    /// Handles connection errors.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    private void HandleConnectionError(string errorMessage)
    {
        Debug.LogError($"Connection error: {errorMessage}");
        
        // Update UI if available
        if (uiManager != null)
        {
            uiManager.ShowError($"Connection error: {errorMessage}");
        }
        
        // Notify listeners
        OnConnectionError?.Invoke(errorMessage);
    }
    
    /// <summary>
    /// Handles incoming messages to detect pongs.
    /// </summary>
    /// <param name="message">The message from the server.</param>
    private void HandleMessage(string message)
    {
        // Check for pong message
        if (message.Contains("\"type\":\"pong\""))
        {
            _isPingPending = false;
            _lastPongTime = Time.time;
        }
    }
    
    /// <summary>
    /// Attempts to reconnect to the server with exponential backoff.
    /// </summary>
    private void AttemptReconnect()
    {
        // Check if max attempts reached
        if (_reconnectAttemptCount >= maxReconnectAttempts)
        {
            Debug.LogWarning($"Max reconnect attempts ({maxReconnectAttempts}) reached. Giving up.");
            
            // Show error to user
            if (uiManager != null)
            {
                uiManager.ShowError("Failed to connect to server after multiple attempts. Please check your network connection and try again.");
            }
            
            return;
        }
        
        // Increment attempt counter
        _reconnectAttemptCount++;
        
        // Calculate delay with exponential backoff
        float delay = initialReconnectDelay * Mathf.Pow(2, _reconnectAttemptCount - 1);
        delay = Mathf.Min(delay, maxReconnectDelay);
        
        Debug.Log($"Attempting to reconnect ({_reconnectAttemptCount}/{maxReconnectAttempts}) in {delay:F1} seconds...");
        
        // Show reconnection message
        if (uiManager != null)
        {
            uiManager.ShowMessage($"Connection lost. Reconnecting ({_reconnectAttemptCount}/{maxReconnectAttempts})...");
        }
        
        // Start reconnection coroutine
        StartCoroutine(ReconnectCoroutine(delay));
    }
    
    /// <summary>
    /// Coroutine for reconnecting after a delay.
    /// </summary>
    /// <param name="delay">The delay in seconds.</param>
    private IEnumerator ReconnectCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Attempt to reconnect
        if (webSocketClient != null)
        {
            _ = webSocketClient.Connect();
        }
    }
    
    /// <summary>
    /// Coroutine for sending periodic ping messages.
    /// </summary>
    private IEnumerator PingCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(pingInterval);
            
            if (_isConnected && webSocketClient != null && webSocketClient.IsConnected)
            {
                SendPing();
            }
        }
    }
    
    /// <summary>
    /// Sends a ping message to the server.
    /// </summary>
    private async void SendPing()
    {
        if (webSocketClient == null || !webSocketClient.IsConnected)
        {
            return;
        }
        
        try
        {
            // Create ping message
            var pingMessage = new PingMessage
            {
                type = "ping",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            };
            
            string jsonMessage = JsonUtility.ToJson(pingMessage);
            
            // Send ping
            await webSocketClient.SendMessage(jsonMessage);
            
            // Update ping state
            _lastPingTime = Time.time;
            _isPingPending = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending ping: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Data structure for ping messages.
    /// </summary>
    [Serializable]
    private class PingMessage
    {
        public string type;
        public double timestamp;
    }
}