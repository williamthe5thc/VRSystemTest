# VR Interview System - Communication Documentation

## Architecture Overview

The communication subsystem in the VR Interview System manages all interactions between the Unity client and the Python server. It is responsible for establishing and maintaining the WebSocket connection, sending and receiving messages, and ensuring the proper flow of data throughout the application.

## Key Classes/Components

### WebSocketClient

`WebSocketClient` is the core component responsible for managing the WebSocket connection to the server. It handles connection establishment, message sending/receiving, reconnection logic, and error handling.

```csharp
public class WebSocketClient : MonoBehaviour
{
    // Connection management
    public async Task Connect()
    // Sends a text message to the server with auto-queueing when disconnected
    public async Task<bool> SendMessage(string message, bool allowQueue = true)
    // Sends audio data to the server
    public async Task SendAudioData(byte[] audioData, string sessionId)
    // Closes the WebSocket connection
    public async Task Close(bool intentional = true)
    
    // Events
    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action<WebSocketCloseCode> OnDisconnected;
    public event Action<string> OnError;
}
```

### MessageHandler

The `MessageHandler` component parses incoming JSON messages from the server and routes them to the appropriate handlers based on message type. It serves as the dispatcher for all server communications.

```csharp
public class MessageHandler : MonoBehaviour
{
    // Message processing
    public void ProcessMessage(string jsonMessage)
    // Register custom handlers for specific message types
    public void RegisterMessageHandler(string messageType, Action<string> handler)
    // Remove registered handlers
    public void UnregisterMessageHandler(string messageType)
    
    // Events
    public event Action<string, string, Dictionary<string, object>> OnStateUpdate;
    public event Action<byte[]> OnAudioResponse;
    public event Action<string> OnError;
    public event Action<MessageHandler, string> MessageReceived;
}
```

### SessionManager

`SessionManager` tracks the current session state and manages the high-level communication protocol with the server. It handles session initialization, state transitions, and coordination between user interactions and server responses.

```csharp
public class SessionManager : MonoBehaviour
{
    // Session control
    public async Task StartSession()
    public async Task EndSession()
    public async Task ResetSession()
    
    // Server communication helpers
    private async Task SendControlMessage(string action)
    public void SendClientCapabilities()
    public void NotifyPlaybackComplete()
    
    // Events
    public event Action<string, string> OnStateChanged;
    public event Action OnSessionStarted;
    public event Action OnSessionEnded;
    public event Action<string> OnConnectionError;
}
```

### ClientCapabilities

`ClientCapabilities` defines the capabilities and features that the Unity client supports, which is communicated to the server during session initialization.

```csharp
[Serializable]
public class ClientCapabilities
{
    public bool supportsAudio;
    public bool supportsLipSync;
    public bool supportsFacialExpressions;
    public bool supportsGestures;
    public string[] supportedAudioFormats;
    public int sampleRate;
    public int channels;
    // Additional configuration parameters
}
```

## Message Format and Serialization

### JSON Message Structure

The system uses JSON for all message serialization. Messages follow a consistent structure:

```json
{
  "type": "message_type",
  "session_id": "unique_session_id",
  "timestamp": 1621234567.890,
  // Additional fields specific to the message type
}
```

### Common Message Types

1. **Control Messages**:
   ```json
   {
     "type": "control",
     "session_id": "abc123",
     "action": "start|end|reset|pause|resume",
     "timestamp": 1621234567.890
   }
   ```

2. **Audio Data Messages**:
   ```json
   {
     "type": "audio_data",
     "session_id": "abc123",
     "data": "base64_encoded_audio",
     "timestamp": 1621234567.890
   }
   ```

3. **State Update Messages**:
   ```json
   {
     "type": "state_update",
     "session_id": "abc123",
     "previous": "IDLE",
     "current": "LISTENING",
     "metadata": {
       "transcript": "User speech transcript",
       "response": "LLM response text"
     },
     "timestamp": 1621234567.890
   }
   ```

4. **Audio Response Messages**:
   ```json
   {
     "type": "audio_response",
     "session_id": "abc123",
     "data": "base64_encoded_audio",
     "text": "Text representation of the audio",
     "timestamp": 1621234567.890
   }
   ```

5. **Heartbeat Messages**:
   ```json
   {
     "type": "ping",
     "session_id": "abc123",
     "timestamp": 1621234567.890
   }
   ```

### Serialization Process

The system uses a combination of Unity's built-in `JsonUtility` and `Newtonsoft.Json` for JSON serialization and deserialization:

```csharp
// Serialization example
var message = new ControlMessage
{
    type = "control",
    session_id = _sessionId,
    action = action,
    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
};

string jsonMessage = JsonConvert.SerializeObject(message);
await webSocketClient.SendMessage(jsonMessage);

// Deserialization example
JObject messageObj = JObject.Parse(jsonMessage);
string messageType = messageObj["type"]?.ToString();
```

## Session ID Handling and Synchronization

### Session ID Generation

The system uses a dual session ID approach:

1. **Client-Generated ID**: Initially generated using `System.Guid.NewGuid()` when the session starts
2. **Server-Generated ID**: Received from the server during session initialization

```csharp
// Initial client-generated ID
_sessionId = System.Guid.NewGuid().ToString();

// Server ID handling
public void UpdateSessionId(string serverSessionId)
{
    if (!string.IsNullOrEmpty(serverSessionId))
    {
        string oldId = _sessionId;
        
        // Store mapping between old and new ID
        if (oldId != serverSessionId && !_sessionMappings.ContainsKey(oldId))
        {
            _sessionMappings[oldId] = serverSessionId;
        }
        
        _sessionId = serverSessionId;
        
        // Store in PlayerPrefs for persistence
        PlayerPrefs.SetString("CurrentSessionId", _sessionId);
        PlayerPrefs.Save();
        
        // Notify components about the change
        OnSessionIdChanged?.Invoke(_sessionId);
    }
}
```

### Session Synchronization

SessionSynchronizer ensures that the client and server maintain consistent session state:

1. The client connects and sends its capabilities
2. The server responds with a session_init message containing a server-generated ID
3. The client updates its session ID and maintains a mapping of old to new IDs
4. Subsequent messages use the server-generated ID

## WebSocket Connection Management

### Connection Lifecycle

```
┌─────────┐                ┌─────────┐
│  Client │                │  Server │
└────┬────┘                └────┬────┘
     │                          │
     │       Connect()          │
     ├─────────────────────────►│
     │                          │
     │      OnConnected         │
     │◄─────────────────────────┤
     │                          │
     │   SendClientCapabilities │
     ├─────────────────────────►│
     │                          │
     │      session_init        │
     │◄─────────────────────────┤
     │                          │
     │     StartSession()       │
     ├─────────────────────────►│
     │                          │
     │  Message Exchange...     │
     ├──────────┬───────────────┤
     │          │               │
     │     EndSession()         │
     ├─────────────────────────►│
     │                          │
     │       Close()            │
     ├─────────────────────────►│
     │                          │
     │     OnDisconnected       │
     │◄─────────────────────────┤
     │                          │
```

### Reconnection Logic

The system implements a robust reconnection system with exponential backoff:

```csharp
if (_isReconnecting)
{
    _reconnectTimer -= Time.deltaTime;
    
    if (_reconnectTimer <= 0f)
    {
        _isReconnecting = false;
        _reconnectAttempts++;
        
        if (_reconnectAttempts <= maxReconnectAttempts)
        {
            Connect().ContinueWith(task => {
                if (task.IsFaulted)
                {
                    _isReconnecting = true;
                    _reconnectTimer = reconnectDelaySeconds * Mathf.Pow(1.5f, _reconnectAttempts);
                }
            });
        }
    }
}
```

Key features:
- Configurable maximum reconnection attempts
- Exponential backoff between attempts
- Graceful failure handling
- UI feedback during reconnection

### Message Queuing

To handle temporary disconnections, the system includes a message queuing system:

```csharp
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
}
```

Messages are automatically redelivered when connection is restored:

```csharp
private async void ProcessMessageQueue()
{
    if (!IsConnected || _messageQueue.Count == 0)
        return;
    
    QueuedMessage message = _messageQueue.Dequeue();
    
    try
    {
        await _websocket.SendText(message.Content);
    }
    catch (Exception ex)
    {
        // Re-queue the message if it's recent
        TimeSpan age = DateTime.Now - message.Timestamp;
        if (age.TotalSeconds < 30)
        {
            // Create temporary queue with this message at front
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
```

## Error Handling and Reconnection Logic

### Error Types and Handling

The system handles several types of errors:

1. **Connection Errors**: Issues establishing or maintaining the WebSocket connection
2. **Message Parsing Errors**: Problems with malformed JSON messages
3. **Server Errors**: Explicit error messages sent by the server
4. **Session Errors**: Issues with session management

Each error type has specific handling:

```csharp
// Handle connection error
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

// Handle server error
private void HandleError(JObject messageObj)
{
    string errorMessage = messageObj["message"]?.ToString();
    Debug.LogError($"Server error: {errorMessage}");
    
    // Show error to user
    if (uiManager != null)
    {
        uiManager.ShowError(errorMessage);
    }
    
    // Broadcast the error
    OnError?.Invoke(errorMessage);
}
```

### Reconnection Strategy

The system uses a tiered approach to reconnection:

1. **Immediate Reconnection**: For transient errors or brief connection loss
2. **Delayed Reconnection**: With exponential backoff for persistent issues
3. **Manual Reconnection**: UI option for user-initiated reconnection after max attempts

```csharp
private IEnumerator AttemptReconnect()
{
    Debug.Log($"Attempting to reconnect... ({_reconnectAttemptCount}/{maxReconnectAttempts})");
    
    // Show reconnecting message
    if (uiManager != null)
    {
        uiManager.ShowMessage($"Connection lost. Reconnecting ({_reconnectAttemptCount}/{maxReconnectAttempts})...");
    }
    
    // Wait with exponential backoff
    float waitTime = reconnectDelaySeconds * Mathf.Pow(2, _reconnectAttemptCount - 1);
    yield return new WaitForSeconds(waitTime);
    
    // Try to reconnect
    if (webSocketClient != null)
    {
        _ = webSocketClient.Connect();
    }
}
```

## Heartbeat System

To maintain connection health and detect disconnections quickly, the system implements a heartbeat mechanism:

```csharp
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
            
            if (idleTime > (pingInterval / 2))
            {
                SendPingMessage();
                
                // Adaptive ping frequency based on state
                if (_currentState == "PROCESSING" || _currentState == "RESPONDING")
                {
                    currentInterval = pingInterval / 2;
                }
                else
                {
                    currentInterval = pingInterval;
                }
            }
            else
            {
                currentInterval = pingInterval;
            }
        }
    }
}
```

Key features:
- Adaptive ping frequency based on conversation state
- Activity-aware pinging (avoids unnecessary pings)
- Server responds with "pong" messages to confirm connection

## Usage Patterns

### Typical Communication Sequence

1. **Initialize Connection**:
   ```csharp
   await webSocketClient.Connect();
   ```

2. **Start Session**:
   ```csharp
   await sessionManager.StartSession();
   ```

3. **Listen for Messages**:
   ```csharp
   webSocketClient.OnMessageReceived += messageHandler.ProcessMessage;
   ```

4. **Send Audio Data**:
   ```csharp
   await webSocketClient.SendAudioData(audioData, sessionManager.SessionId);
   ```

5. **End Session**:
   ```csharp
   await sessionManager.EndSession();
   ```

6. **Close Connection**:
   ```csharp
   await webSocketClient.Close();
   ```

### Server Communication Examples

**Sending a Control Message**:
```csharp
var message = new ControlMessage
{
    type = "control",
    session_id = _sessionId,
    action = "start",
    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
};

string jsonMessage = JsonConvert.SerializeObject(message);
await webSocketClient.SendMessage(jsonMessage);
```

**Sending Audio Data**:
```csharp
var audioMessage = new AudioDataMessage
{
    type = "audio_data",
    session_id = sessionId,
    data = Convert.ToBase64String(audioData),
    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
};

string jsonMessage = JsonUtility.ToJson(audioMessage);
await webSocketClient.SendMessage(jsonMessage);
```

## Common Issues

### Known Issues and Solutions

1. **WebSocket Connection Failures**:
   - **Symptoms**: Unable to connect, frequent disconnections
   - **Causes**: Network issues, server unavailable, incorrect URL
   - **Solution**: Verify server URL in settings, check network connectivity, ensure server is running

2. **Message Handling Errors**:
   - **Symptoms**: Error logs about malformed JSON, missing fields
   - **Causes**: Version mismatch between client and server, incorrect message format
   - **Solution**: Ensure client and server are compatible versions, validate message structure

3. **Session Synchronization Issues**:
   - **Symptoms**: "Session not found" errors, inconsistent state
   - **Causes**: Session ID mismatch, session timeout on server
   - **Solution**: Restart session, verify session mapping logic

4. **Audio Data Transmission Problems**:
   - **Symptoms**: No audio received by server, corrupted audio responses
   - **Causes**: Format issues, buffer size problems, encoding errors
   - **Solution**: Verify audio format matches server expectations, check buffer handling

## VR-Specific Considerations

### Performance Optimization

- **Message Batching**: Combines multiple small messages to reduce overhead
- **Selective Updates**: Prioritizes critical messages during high load
- **Background Processing**: Handles network operations off the main thread

### Mobile VR Constraints

- **Battery Conservation**: Adaptive ping frequency to reduce power usage
- **Network Limitations**: Handles intermittent connectivity common in mobile scenarios
- **Platform-Specific Handling**: Special case code for Android WebSocket implementation

### Quest-Specific Implementations

```csharp
// Permission handling for Oculus Quest
#if PLATFORM_ANDROID
if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
{
    UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
}
#endif
```

## Conclusion

The communication subsystem is a critical component of the VR Interview System, providing reliable bidirectional communication with the Python server. It handles the complexities of WebSocket communication, message serialization, error recovery, and session management while maintaining a clean component-based architecture.
