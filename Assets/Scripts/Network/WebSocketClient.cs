using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;
using System.Text;

/// <summary>
/// Handles WebSocket communication with the interview server.
/// </summary>
public class WebSocketClient : MonoBehaviour
{
    [SerializeField] private string serverUrl = "ws://192.168.68.100:8765";
    [SerializeField] private bool autoConnect = true;
    [SerializeField] private bool reconnectOnDisconnect = true;
    [SerializeField] private float reconnectDelay = 2f;
    [SerializeField] private TMPro.TextMeshProUGUI debugText;

    private WebSocket _websocket;
    private bool _isReconnecting = false;
    private float _reconnectTimer = 0f;
    private int _reconnectAttempts = 0;
    [SerializeField] private int maxReconnectAttempts = 5;
    [SerializeField] private float connectionTimeout = 10f;
    
    // Message queue for offline operation
    private Queue<QueuedMessage> _messageQueue = new Queue<QueuedMessage>();
    private const int MAX_QUEUED_MESSAGES = 10;
    
    // Event system for message handling
    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action<WebSocketCloseCode> OnDisconnected;
    public event Action<string> OnError;
    public event Action OnConnectionEstablished;
    public event Action<string> MessageReceived;
    
    // Connection state
    public bool IsConnected => _websocket != null && _websocket.State == WebSocketState.Open;
    private void UpdateDebugText(string message)
{
    if (debugText != null)
    {
        // Add timestamp
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string entry = $"[{timestamp}] {message}\n";
        
        // Keep only last N lines
        string[] lines = debugText.text.Split('\n');
        if (lines.Length > 20)
        {
            string newText = "";
            for (int i = 1; i < lines.Length; i++)
            {
                newText += lines[i] + "\n";
            }
            debugText.text = newText;
        }
        
        // Add new message
        debugText.text += entry;
    }
}
private void LogDebug(string message)
{
    Debug.Log(message);
    UpdateDebugText(message);
}

private void LogError(string message)
{
    Debug.LogError(message);
    UpdateDebugText($"ERROR: {message}");
}
   private void Start()
{
    // Get server URL from settings if available and not empty
    if (SettingsManager.Instance != null)
    {
        string settingsUrl = SettingsManager.Instance.GetSetting<string>("ServerUrl");
        if (!string.IsNullOrEmpty(settingsUrl))
        {
            serverUrl = settingsUrl;
        }
    }
    
    // Fallback to default if empty
    if (string.IsNullOrEmpty(serverUrl))
    {
        serverUrl = "ws://localhost:8765"; // Default fallback
        Debug.LogWarning("Using default server URL: " + serverUrl);
    }
    
    if (autoConnect)
    {
        _ = Connect();
    }
}
    
    /// <summary>
    /// Sends audio data to the server.
    /// </summary>
    /// <param name="audioData">The audio data to send.</param>
    /// <param name="sessionId">The current session ID.</param>
    public async Task SendAudioData(byte[] audioData, string sessionId)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("Cannot send audio: WebSocket not connected");
            await Connect();
            
            // If still not connected, abort
            if (!IsConnected)
            {
                Debug.LogError("Failed to connect to server. Audio not sent.");
                return;
            }
        }
        
        try
        {
            // Create audio data message
            var audioMessage = new AudioDataMessage
            {
                type = "audio_data",
                session_id = sessionId,
                data = Convert.ToBase64String(audioData),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            };
            
            // Convert to JSON and send
            string jsonMessage = JsonUtility.ToJson(audioMessage);
            await _websocket.SendText(jsonMessage);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending audio data: {ex.Message}");
            OnError?.Invoke(ex.Message);
        }
    }
    
    /// <summary>
    /// Connects to the WebSocket server.
    /// </summary>
    public async Task Connect()
    {
        // Skip if already connected or connecting
        if (_websocket != null && 
            (_websocket.State == WebSocketState.Open || 
             _websocket.State == WebSocketState.Connecting))
        {
            return;
        }
        
LogDebug($"Connecting to WebSocket server: {serverUrl}");        
        try
        {
            // Close any existing connection
            if (_websocket != null)
            {
                await _websocket.Close();
            }
            
            // Create new WebSocket connection
            _websocket = new WebSocket(serverUrl);
            
            // Set up event handlers
            _websocket.OnOpen += () => {
                Debug.Log("WebSocket connection opened");
                _isReconnecting = false;
                OnConnected?.Invoke();
                OnConnectionEstablished?.Invoke();
            };
            
            _websocket.OnMessage += (bytes) => {
                string message = Encoding.UTF8.GetString(bytes);
                OnMessageReceived?.Invoke(message);
                MessageReceived?.Invoke(message);
            };
            
            _websocket.OnClose += (closeCode) => {
                Debug.Log($"WebSocket connection closed: {closeCode}");
                OnDisconnected?.Invoke(closeCode);
                
                if (reconnectOnDisconnect && !_isReconnecting)
                {
                    _isReconnecting = true;
                    _reconnectTimer = reconnectDelay;
                }
            };
            
            _websocket.OnError += (errorMsg) => {
                Debug.LogError($"WebSocket error: {errorMsg}");
                OnError?.Invoke(errorMsg);
            };
            
            // Connect to server
            await _websocket.Connect();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error connecting to WebSocket server: {ex.Message}");
            OnError?.Invoke(ex.Message);
            
            if (reconnectOnDisconnect && !_isReconnecting)
            {
                _isReconnecting = true;
                _reconnectTimer = reconnectDelay;
            }
        }
    }
    
    /// <summary>
    /// Sends a text message to the server with auto-queueing when disconnected.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="allowQueue">Whether to allow queueing if disconnected</param>
    public new async Task<bool> SendMessage(string message, bool allowQueue = true)
    {
        if (!IsConnected)
        {
            LogDebug("Cannot send message: WebSocket not connected");
            
            if (allowQueue)
            {
                // Queue message for later delivery
                QueueMessage(message);
                
                // Try to reconnect
                _ = Connect();
                
                return false;
            }
            else
            {
                Debug.LogWarning("WebSocket not connected and queueing disabled. Message not sent.");
                return false;
            }
        }
        
        try
        {
            await _websocket.SendText(message);
            return true;
        }
        catch (Exception ex)
        {
            LogError($"Error sending message: {ex.Message}");
            OnError?.Invoke(ex.Message);
            
            if (allowQueue)
            {
                // Queue message for retry
                QueueMessage(message);
                LogDebug("Message queued for retry after send error");
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Sends audio data to the server using a non-async approach with callbacks.
    /// </summary>
    /// <param name="audioData">The audio data to send.</param>
    /// <param name="sessionId">The current session ID.</param>
    /// <param name="onComplete">Callback when the send operation completes successfully.</param>
    /// <param name="onError">Callback when the send operation fails with an error.</param>
    public void SendAudioDataNonAsync(byte[] audioData, string sessionId, Action onComplete = null, Action<string> onError = null)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("Cannot send audio: WebSocket not connected");
            onError?.Invoke("WebSocket not connected");
            return;
        }
        
        try
        {
            // Create audio data message
            var audioMessage = new AudioDataMessage
            {
                type = "audio_data",
                session_id = sessionId,
                data = Convert.ToBase64String(audioData),
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            };
            
            // Convert to JSON
            string jsonMessage = JsonUtility.ToJson(audioMessage);
            
            // Start a coroutine to send the data
            if (Application.isPlaying)
            {
                MonoBehaviour mb = FindObjectOfType<MonoBehaviour>();
                if (mb != null)
                {
                    mb.StartCoroutine(SendTextCoroutine(jsonMessage, onComplete, onError));
                }
                else
                {
                    Debug.LogError("No MonoBehaviour found to start coroutine");
                    onError?.Invoke("No MonoBehaviour found to start coroutine");
                }
            }
            else
            {
                Debug.LogError("Cannot send audio when not in play mode");
                onError?.Invoke("Cannot send audio when not in play mode");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error preparing audio data: {ex.Message}");
            onError?.Invoke(ex.Message);
        }
    }
    
    /// <summary>
    /// Coroutine to send text message to the server.
    /// </summary>
    private IEnumerator SendTextCoroutine(string message, Action onComplete = null, Action<string> onError = null)
    {
        // Create a task to send the text
        Task sendTask = null;
        bool taskCreated = false;
        Exception capturedException = null;
        
        try
        {
            sendTask = _websocket.SendText(message);
            taskCreated = true;
        }
        catch (Exception ex)
        {
            capturedException = ex;
        }
        
        // If task creation failed, report the error and exit
        if (!taskCreated || sendTask == null)
        {
            onError?.Invoke(capturedException?.Message ?? "Failed to create send task");
            yield break;
        }
        
        // Wait until the task completes
        while (!sendTask.IsCompleted)
        {
            yield return null;
        }
        
        // After the task completes, check its status
        if (sendTask.IsFaulted)
        {
            onError?.Invoke(sendTask.Exception?.Message ?? "Unknown error in send task");
        }
        else
        {
            onComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Closes the WebSocket connection.
    /// </summary>
    public async Task Close(bool intentional = true)
    {
        if (_websocket != null)
        {
            try
            {
                await _websocket.Close();
                LogDebug("WebSocket connection closed intentionally");
            }
            catch (Exception ex)
            {
                LogError($"Error closing WebSocket: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Queue a message for sending later if connection is lost.
    /// </summary>
    private void QueueMessage(string message)
    {
        if (_messageQueue.Count >= MAX_QUEUED_MESSAGES)
        {
            // Remove oldest message
            _messageQueue.Dequeue();
        }
        
        _messageQueue.Enqueue(new QueuedMessage {
            Content = message,
            Timestamp = DateTime.Now
        });
        
        LogDebug($"Message queued for later delivery. Queue size: {_messageQueue.Count}");
    }
    
    /// <summary>
    /// Process queued messages when connection is restored
    /// </summary>
    private async void ProcessMessageQueue()
    {
        if (!IsConnected || _messageQueue.Count == 0)
            return;
            
        LogDebug($"Processing queued messages. Count: {_messageQueue.Count}");
        
        // Send oldest message first
        QueuedMessage message = _messageQueue.Dequeue();
        
        try
        {
            await _websocket.SendText(message.Content);
            LogDebug("Sent queued message successfully");
        }
        catch (Exception ex)
        {
            LogError($"Failed to send queued message: {ex.Message}");
            // Re-queue the message at the front if it's recent (less than 30 seconds old)
            TimeSpan age = DateTime.Now - message.Timestamp;
            if (age.TotalSeconds < 30)
            {
                // Create a temporary queue with this message at the front
                Queue<QueuedMessage> tempQueue = new Queue<QueuedMessage>();
                tempQueue.Enqueue(message);
                
                // Add all other messages
                while (_messageQueue.Count > 0)
                {
                    tempQueue.Enqueue(_messageQueue.Dequeue());
                }
                
                // Replace the queue
                _messageQueue = tempQueue;
            }
        }
    }
    
    private void Update()
    {
        // Handle WebSocket message queue
        if (_websocket != null)
        {
            try {
                _websocket.DispatchMessageQueue();
            }
            catch (Exception ex) {
                Debug.LogError($"Error dispatching WebSocket messages: {ex.Message}");
                
                // If error occurs during dispatch, initiate reconnect
                if (reconnectOnDisconnect && !_isReconnecting) 
                {
                    LogDebug("Error during message dispatch, initiating reconnect");
                    _isReconnecting = true;
                    _reconnectTimer = reconnectDelay;
                }
            }
        }
        
        // Handle reconnection logic with improved state logging
        if (_isReconnecting)
        {
            _reconnectTimer -= Time.deltaTime;
            
            if (_reconnectTimer <= 0f)
            {
                LogDebug($"Attempting reconnection (attempt {_reconnectAttempts + 1}/{maxReconnectAttempts})");
                _isReconnecting = false;
                
                // Track attempt count
                _reconnectAttempts++;
                
                // Only continue reconnecting up to max attempts
                if (_reconnectAttempts <= maxReconnectAttempts)
                {
                    Connect().ContinueWith(task => {
                        if (task.IsFaulted && task.Exception != null)
                        {
                            LogError($"Reconnection attempt failed: {task.Exception.InnerException?.Message ?? "Unknown error"}");
                            
                            // Schedule another attempt with exponential backoff
                            _isReconnecting = true;
                            _reconnectTimer = reconnectDelay * Mathf.Pow(1.5f, _reconnectAttempts);
                        }
                    });
                }
                else
                {
                    LogError($"Maximum reconnection attempts ({maxReconnectAttempts}) reached");
                    OnError?.Invoke("Maximum reconnection attempts reached");
                    _reconnectAttempts = 0; // Reset for future reconnection cycles
                }
            }
        }
        
        // Process message queue if we're connected and have queued messages
        if (IsConnected && _messageQueue.Count > 0)
        {
            ProcessMessageQueue();
        }
    }
    
    private async void OnApplicationQuit()
    {
        await Close();
    }
    
    private async void OnDestroy()
    {
        await Close();
    }
    
    /// <summary>
    /// Structure for audio data messages.
    /// </summary>
    [Serializable]
    private class AudioDataMessage
    {
        public string type;
        public string session_id;
        public string data;  // Base64 encoded audio
        public double timestamp;
    }
    
    /// <summary>
    /// Structure for queued messages.
    /// </summary>
    private class QueuedMessage
    {
        public string Content;
        public DateTime Timestamp;
    }
}