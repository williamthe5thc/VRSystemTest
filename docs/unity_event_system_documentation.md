# VR Interview System - Event System Documentation

## Architecture Overview

The event system in the VR Interview System is based on a publisher-subscriber pattern that facilitates communication between components while maintaining loose coupling. Rather than tightly integrating components through direct references, the system uses C# events and delegates to allow components to communicate in a decoupled manner. This approach improves modularity, makes the code more maintainable, and allows for easier testing and extension.

## Key Concepts

### Publisher-Subscriber Pattern

The event system follows the publisher-subscriber (pub-sub) pattern with the following roles:

1. **Publishers**: Components that define and trigger events. They have no knowledge of who is listening.
2. **Subscribers**: Components that register for specific events and define how to respond.
3. **Events**: The messages or notifications that are passed from publishers to subscribers.

### Event Registration

Components register for events during initialization (typically in `Start()` or `Awake()`) and unregister when they're destroyed or disabled.

```csharp
private void Start()
{
    // Register for events
    webSocketClient.OnMessageReceived += ProcessMessage;
    webSocketClient.OnError += HandleConnectionError;
    
    sessionManager.OnStateChanged += HandleStateChange;
    sessionManager.OnSessionStarted += HandleSessionStart;
    sessionManager.OnSessionEnded += HandleSessionEnd;
    
    audioPlayback.OnPlaybackStarted += HandlePlaybackStarted;
    audioPlayback.OnPlaybackCompleted += HandlePlaybackCompleted;
}

private void OnDestroy()
{
    // Unregister to prevent memory leaks
    if (webSocketClient != null)
    {
        webSocketClient.OnMessageReceived -= ProcessMessage;
        webSocketClient.OnError -= HandleConnectionError;
    }
    
    if (sessionManager != null)
    {
        sessionManager.OnStateChanged -= HandleStateChange;
        sessionManager.OnSessionStarted -= HandleSessionStart;
        sessionManager.OnSessionEnded -= HandleSessionEnd;
    }
    
    if (audioPlayback != null)
    {
        audioPlayback.OnPlaybackStarted -= HandlePlaybackStarted;
        audioPlayback.OnPlaybackCompleted -= HandlePlaybackCompleted;
    }
}
```

## Event Definitions

The system defines events using C#'s built-in `Action` and `Action<T>` delegates:

### WebSocketClient Events

```csharp
public class WebSocketClient : MonoBehaviour
{
    // Event definitions
    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action<WebSocketCloseCode> OnDisconnected;
    public event Action<string> OnError;
    public event Action OnConnectionEstablished;
    
    // Event triggers
    private void HandleMessageReceived(string message)
    {
        OnMessageReceived?.Invoke(message);
    }
    
    private void HandleConnection()
    {
        OnConnected?.Invoke();
        OnConnectionEstablished?.Invoke();
    }
    
    private void HandleDisconnection(WebSocketCloseCode closeCode)
    {
        OnDisconnected?.Invoke(closeCode);
    }
    
    private void HandleError(string errorMessage)
    {
        OnError?.Invoke(errorMessage);
    }
}
```

### SessionManager Events

```csharp
public class SessionManager : MonoBehaviour
{
    // Event definitions
    public event Action<string, string> OnStateChanged;
    public event Action OnSessionStarted;
    public event Action OnSessionEnded;
    public event Action<string> OnConnectionError;
    public event Action OnResponseComplete;
    public event Action<string> OnSessionIdChanged;
    
    // Event triggers
    private void HandleStateUpdate(string previousState, string newState, Dictionary<string, object> metadata)
    {
        string oldState = _currentState;
        _currentState = newState;
        
        // Notify state change
        OnStateChanged?.Invoke(oldState, newState);
    }
    
    private void StartSession()
    {
        // Session startup logic...
        
        // Notify session start
        OnSessionStarted?.Invoke();
    }
    
    private void EndSession()
    {
        // Session cleanup logic...
        
        // Notify session end
        OnSessionEnded?.Invoke();
    }
    
    public void UpdateSessionId(string serverSessionId)
    {
        // Update session ID...
        
        // Notify session ID change
        OnSessionIdChanged?.Invoke(_sessionId);
    }
}
```

### MessageHandler Events

```csharp
public class MessageHandler : MonoBehaviour
{
    // Event definitions
    public event Action<string, string, Dictionary<string, object>> OnStateUpdate;
    public event Action<byte[]> OnAudioResponse;
    public event Action<string> OnError;
    public event Action<MessageHandler, string> MessageReceived;
    
    // Event triggers
    private void HandleStateUpdate(JObject messageObj)
    {
        string previousState = messageObj["previous"]?.ToString();
        string currentState = messageObj["current"]?.ToString();
        
        // Extract metadata, if any
        Dictionary<string, object> metadata = new Dictionary<string, object>();
        JObject metadataObj = messageObj["metadata"] as JObject;
        if (metadataObj != null)
        {
            foreach (var property in metadataObj.Properties())
            {
                metadata[property.Name] = property.Value.ToObject<object>();
            }
        }
        
        // Broadcast the state change
        OnStateUpdate?.Invoke(previousState, currentState, metadata);
    }
    
    private void HandleAudioResponse(JObject messageObj)
    {
        // Extract audio data
        string base64Audio = messageObj["data"]?.ToString();
        byte[] audioData = Convert.FromBase64String(base64Audio);
        
        // Broadcast the audio response
        OnAudioResponse?.Invoke(audioData);
    }
    
    private void HandleError(JObject messageObj)
    {
        string errorMessage = messageObj["message"]?.ToString();
        
        // Broadcast the error
        OnError?.Invoke(errorMessage);
    }
}
```

### Audio Events

```csharp
public class MicrophoneCapture : MonoBehaviour
{
    // Event definitions
    public event Action OnRecordingStarted;
    public event Action OnRecordingStopped;
    public event Action<float> OnAudioLevelChanged;
    
    // Event triggers
    public void StartRecording()
    {
        // Recording initialization logic...
        
        // Notify recording started
        OnRecordingStarted?.Invoke();
    }
    
    public void StopRecording()
    {
        // Recording cleanup logic...
        
        // Notify recording stopped
        OnRecordingStopped?.Invoke();
    }
    
    private void UpdateAudioLevel(float level)
    {
        // Notify audio level change
        OnAudioLevelChanged?.Invoke(level);
    }
}

public class AudioPlayback : MonoBehaviour
{
    // Event definitions
    public event Action OnPlaybackStarted;
    public event Action OnPlaybackCompleted;
    public event Action<float> OnPlaybackProgress;
    
    // Event triggers
    public void PlayAudioResponse(byte[] audioData)
    {
        // Playback initialization logic...
        
        // Notify playback started
        OnPlaybackStarted?.Invoke();
    }
    
    private void CompletePlayback()
    {
        // Notify playback completed
        OnPlaybackCompleted?.Invoke();
    }
    
    private void UpdateProgress(float progress)
    {
        // Notify playback progress
        OnPlaybackProgress?.Invoke(progress);
    }
}
```

### Avatar Events

```csharp
public class AvatarController : MonoBehaviour
{
    // Event definitions
    public event Action<string> OnStateChanged;
    
    // Event triggers
    public void SetIdleState()
    {
        if (_currentState == "IDLE") return;
        
        _currentState = "IDLE";
        
        // State change logic...
        
        // Notify state change
        OnStateChanged?.Invoke(_currentState);
    }
    
    public void SetSpeakingState()
    {
        if (_currentState == "RESPONDING") return;
        
        _currentState = "RESPONDING";
        
        // State change logic...
        
        // Notify state change
        OnStateChanged?.Invoke(_currentState);
    }
}
```

## Event Parameter Passing

The system passes different types of data through events depending on the event type:

### Simple Notifications

Some events don't need to pass any data and simply notify that something happened:

```csharp
public event Action OnSessionStarted;
public event Action OnPlaybackCompleted;
public event Action OnRecordingStarted;

// Trigger
OnSessionStarted?.Invoke();

// Handler
private void HandleSessionStart()
{
    Debug.Log("Session started!");
    // Perform actions...
}
```

### Single Parameter Events

Many events pass a single value:

```csharp
public event Action<string> OnMessageReceived;
public event Action<float> OnPlaybackProgress;
public event Action<float> OnAudioLevelChanged;

// Trigger
OnMessageReceived?.Invoke(message);
OnPlaybackProgress?.Invoke(normalizedTime);

// Handler
private void ProcessMessage(string message)
{
    Debug.Log($"Received message: {message}");
    // Process message...
}

private void UpdateProgressBar(float progress)
{
    // Update UI with progress
    progressBar.value = progress;
}
```

### Multiple Parameter Events

Some events need to pass multiple related values:

```csharp
public event Action<string, string> OnStateChanged;
public event Action<string, string, Dictionary<string, object>> OnStateUpdate;

// Trigger
OnStateChanged?.Invoke(oldState, newState);
OnStateUpdate?.Invoke(previousState, currentState, metadata);

// Handler
private void HandleStateChange(string previousState, string currentState)
{
    Debug.Log($"State changed from {previousState} to {currentState}");
    // Update UI, change behavior, etc.
}

private void HandleDetailedStateUpdate(string prev, string curr, Dictionary<string, object> meta)
{
    // Access metadata
    if (meta.TryGetValue("transcript", out object transcriptObj))
    {
        string transcript = transcriptObj.ToString();
        // Use transcript...
    }
}
```

## Event Flow Through System

The flow of events through the VR Interview System follows key conversation paths:

### User Speech Recording Flow

```
User speaks → MicrophoneCapture.OnRecordingStarted → UIManager updates UI
                    ↓
MicrophoneCapture records audio → OnAudioLevelChanged → UIManager updates audio level indicator
                    ↓
MicrophoneCapture.OnRecordingStopped → AudioProcessor processes audio
                    ↓
AudioProcessor sends to WebSocketClient → WebSocketClient sends to server
```

### Server Response Flow

```
WebSocketClient.OnMessageReceived → MessageHandler.ProcessMessage
                    ↓
MessageHandler determines message type and triggers specific events:
   ↓                     ↓                      ↓
OnStateUpdate     OnAudioResponse          OnError
   ↓                     ↓                      ↓
SessionManager   AudioPlayback.PlayAudio   UIManager.ShowError
updates state           ↓                     
   ↓            OnPlaybackStarted
   ↓                   ↓
UIManager      AvatarController.SetSpeakingState
updates UI             ↓
                 LipSync.StartLipSync
                        ↓
               AudioPlayback.OnPlaybackProgress → LipSync.UpdateLipSync
                        ↓
               AudioPlayback.OnPlaybackCompleted → AvatarController.SetAttentiveState
                        ↓
               SessionManager.NotifyPlaybackComplete → Server
```

### Session State Change Flow

```
Server sends state_update → WebSocketClient.OnMessageReceived
                    ↓
MessageHandler.ProcessMessage → MessageHandler.OnStateUpdate
                    ↓
SessionManager.HandleStateUpdate → SessionManager.OnStateChanged
                    ↓
┌────────────────────┬──────────────────┬────────────────────┐
↓                    ↓                  ↓                    ↓
UIManager         AvatarController   MicrophoneCapture    Other Components
updates UI         changes state      starts/stops        react to state
                                      based on state       
```

## Component Interactions

The event system facilitates the following key component interactions:

### Network to System Core

The WebSocketClient receives messages from the server and notifies the MessageHandler:

```csharp
// In MessageHandler
private void Start()
{
    if (webSocketClient != null)
    {
        webSocketClient.OnMessageReceived += ProcessMessage;
        webSocketClient.OnError += HandleConnectionError;
    }
}

public void ProcessMessage(string jsonMessage)
{
    // Parse the JSON message
    JObject messageObj = JObject.Parse(jsonMessage);
    string messageType = messageObj["type"]?.ToString();
    
    // Route based on message type and trigger appropriate events
    switch (messageType)
    {
        case "state_update":
            HandleStateUpdate(messageObj);
            break;
        
        case "audio_response":
            HandleAudioResponse(messageObj);
            break;
        
        case "error":
            HandleError(messageObj);
            break;
        
        // Additional message types...
    }
}
```

### System Core to UI

The SessionManager notifies the UIManager of state changes:

```csharp
// In UIManager
private void Start()
{
    if (sessionManager != null)
    {
        sessionManager.OnStateChanged += HandleStateChange;
        sessionManager.OnSessionStarted += HandleSessionStart;
        sessionManager.OnSessionEnded += HandleSessionEnd;
    }
}

private void HandleStateChange(string previousState, string currentState)
{
    // Update UI based on state
    UpdateStateDisplay(currentState);
    
    switch (currentState)
    {
        case "IDLE":
            SetProcessingPanelVisible(false);
            break;
        
        case "LISTENING":
            ShowUserTranscript("Listening...");
            break;
        
        case "PROCESSING":
            SetProcessingPanelVisible(true);
            ShowProcessingMessage("Processing your request...");
            break;
        
        case "RESPONDING":
            SetProcessingPanelVisible(false);
            break;
        
        case "WAITING":
            SetProcessingPanelVisible(false);
            break;
    }
}
```

### System Core to Avatar

The MessageHandler and AudioPlayback components notify the AvatarController of state changes and audio events:

```csharp
// In AvatarController
private void Start()
{
    // Subscribe to message handler events for state changes
    MessageHandler messageHandler = FindObjectOfType<MessageHandler>();
    if (messageHandler != null)
    {
        messageHandler.OnStateUpdate += HandleStateUpdate;
    }
    
    // Subscribe to audio playback events
    AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
    if (audioPlayback != null)
    {
        audioPlayback.OnPlaybackStarted += OnAudioPlaybackStarted;
        audioPlayback.OnPlaybackCompleted += OnAudioPlaybackCompleted;
        audioPlayback.OnPlaybackProgress += UpdateLipSync;
    }
}

private void HandleStateUpdate(string previous, string current, Dictionary<string, object> metadata)
{
    switch (current)
    {
        case "IDLE":
            SetIdleState();
            break;
        
        case "LISTENING":
            SetListeningState();
            break;
        
        case "PROCESSING":
            SetThinkingState();
            break;
        
        case "RESPONDING":
            SetSpeakingState();
            break;
        
        case "WAITING":
            SetAttentiveState();
            break;
        
        case "ERROR":
            SetConfusedState();
            break;
    }
}

public void OnAudioPlaybackStarted()
{
    // Ensure speaking state and lip sync
    if (_currentState != "RESPONDING")
    {
        SetSpeakingState();
    }
    
    if (lipSync != null)
    {
        lipSync.StartLipSync();
    }
}
```

## Custom Event Registration System

For more flexible event handling, the MessageHandler supports a custom registration system for message types:

```csharp
public class MessageHandler : MonoBehaviour
{
    // Dictionary to store message handlers
    private Dictionary<string, Action<string>> messageHandlers = new Dictionary<string, Action<string>>();
    
    public void RegisterMessageHandler(string messageType, Action<string> handler)
    {
        if (messageHandlers.ContainsKey(messageType))
        {
            messageHandlers[messageType] += handler;
        }
        else
        {
            messageHandlers[messageType] = handler;
        }
    }
    
    public void UnregisterMessageHandler(string messageType)
    {
        if (messageHandlers.ContainsKey(messageType))
        {
            messageHandlers.Remove(messageType);
        }
    }
    
    public void ProcessMessage(string jsonMessage)
    {
        try
        {
            // Parse the JSON message
            JObject messageObj = JObject.Parse(jsonMessage);
            string messageType = messageObj["type"]?.ToString();
            
            // Check if we have a registered handler for this message type
            if (!string.IsNullOrEmpty(messageType) && messageHandlers.ContainsKey(messageType))
            {
                messageHandlers[messageType]?.Invoke(jsonMessage);
                return;
            }
            
            // Default message routing...
        }
        catch (Exception e)
        {
            Debug.LogError($"Error handling message: {e.Message}");
        }
    }
}

// Usage example:
private void Start()
{
    messageHandler.RegisterMessageHandler("audio_packet", HandleAudioPacket);
    messageHandler.RegisterMessageHandler("system_message", HandleSystemMessage);
}

private void HandleAudioPacket(string jsonMessage)
{
    // Process audio packet message
}

private void HandleSystemMessage(string jsonMessage)
{
    // Process system message
}
```

## MonoBehaviour Message Routing

In addition to custom events, the system leverages Unity's MonoBehaviour message routing for lifecycle events:

```csharp
private void Awake()
{
    // Singleton setup and one-time initialization
}

private void Start()
{
    // Component initialization and event subscription
}

private void Update()
{
    // Per-frame updates like checking WebSocket messages
    if (_websocket != null)
    {
        _websocket.DispatchMessageQueue();
    }
}

private void OnDestroy()
{
    // Cleanup and event unsubscription
}

private void OnApplicationQuit()
{
    // Final cleanup before application exit
}
```

## Common Issues

### Event Registration Memory Leaks

Failure to unregister events can lead to memory leaks:

```csharp
// INCORRECT: Missing unregistration
private void Start()
{
    webSocketClient.OnMessageReceived += ProcessMessage;
}

// CORRECT: Proper cleanup in OnDestroy
private void OnDestroy()
{
    if (webSocketClient != null)
    {
        webSocketClient.OnMessageReceived -= ProcessMessage;
    }
}
```

### Missing Null Checks

Events can be null if no handlers are registered:

```csharp
// INCORRECT: No null check
private void TriggerEvent()
{
    OnStateChanged(oldState, newState); // Will throw NullReferenceException if no handlers
}

// CORRECT: Using null-conditional operator
private void TriggerEvent()
{
    OnStateChanged?.Invoke(oldState, newState);
}
```

### Race Conditions

Events firing in the wrong order can cause issues:

```csharp
// POTENTIAL ISSUE: Order-dependent events
public async void StartInterview()
{
    // Connection might not be established before sending message
    OnSessionStarted?.Invoke();
    await webSocketClient.SendMessage(startMessage);
}

// BETTER: Ensure connection before starting
public async Task<bool> StartInterview()
{
    if (!webSocketClient.IsConnected)
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
            OnConnectionError?.Invoke("Failed to connect to server.");
            return false;
        }
    }
    
    OnSessionStarted?.Invoke();
    await webSocketClient.SendMessage(startMessage);
    return true;
}
```

## Best Practices

### Event Naming Conventions

The system follows consistent naming conventions for events:

- Use `On` prefix for event definitions (`OnMessageReceived`, `OnStateChanged`)
- Use `Handle` prefix for event handlers (`HandleMessageReceived`, `HandleStateChange`)

### Lightweight Event Arguments

Keep event arguments lightweight to avoid performance issues:

```csharp
// GOOD: Lightweight event with specific data
public event Action<string, string> OnStateChanged;

// BAD: Passing entire large objects
public event Action<SessionManager, CompleteSessionState> OnStateChanged;
```

### Event Documentation

Document events clearly to help other developers:

```csharp
/// <summary>
/// Fired when the session state changes.
/// </summary>
/// <param name="previousState">The previous state name.</param>
/// <param name="currentState">The new state name.</param>
public event Action<string, string> OnStateChanged;
```

### Scoped Event Registration

Limit event registration scope to avoid unintended subscribers:

```csharp
// AVOID: Public event that anyone can subscribe to
public event Action<string> OnMessageReceived;

// BETTER: Internal event with public registration method
private event Action<string> OnMessageReceived;

public void AddMessageListener(Action<string> listener)
{
    OnMessageReceived += listener;
}

public void RemoveMessageListener(Action<string> listener)
{
    OnMessageReceived -= listener;
}
```

## Conclusion

The event system in the VR Interview System provides a flexible and maintainable way for components to communicate. By leveraging C# delegates and events, the system achieves loose coupling between components while enabling complex interactions. This architecture allows the various subsystems (UI, audio, avatar, networking) to work together seamlessly without tight dependencies.

The consistent use of event patterns throughout the codebase makes it easier to understand the flow of data and control through the application, and facilitates extension and debugging. By following the patterns and practices outlined in this document, developers can extend the system with new features while maintaining the clean architecture.
