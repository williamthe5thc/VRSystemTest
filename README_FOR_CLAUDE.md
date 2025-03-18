# VR Interview System - Technical Guide for Claude (Unity Client)

## System Overview

The VR Interview System is an AI-powered interview practice platform in virtual reality. The Unity client provides the VR interface, avatar animation, audio management, and user interaction, while communicating with a Python server that handles language processing and conversation management.

> **Note**: For detailed information about the Python server implementation, please refer to `D:\vr_interview_system\README_FOR_CLAUDE.md`, which contains extensive details about the server architecture, audio processing pipeline, and LLM integration.

## Client Architecture

The Unity client follows a component-based architecture with several key modules:

### Core Communication Components

1. **WebSocketClient.cs**
   - Handles WebSocket communication with the Python server
   - Sends audio data and receives server responses
   - Provides event system for notifying other components
   - Manages connection state and reconnection logic

2. **MessageHandler.cs**
   - Central message processing hub
   - Routes incoming server messages to appropriate components
   - Handles different message types (audio, text, state updates)
   - Provides registration system for message-specific handlers

3. **SessionManager.cs**
   - Manages session state and context
   - Handles session ID synchronization with server
   - Stores conversation history
   - Controls session lifecycle (start, pause, resume, end)

### User Interface Components

1. **UIManager.cs**
   - Manages all UI elements and displays
   - Shows transcripts and system status
   - Handles notifications and error displays
   - Provides visual feedback about conversation state

2. **VRInteractionUI.cs**
   - Implements VR-specific interaction controls
   - Manages menu positioning and activation
   - Handles VR controller input
   - Provides feedback mechanisms

### Audio Components

1. **AudioPlayback.cs**
   - Plays audio responses from the server
   - Manages audio playback queue
   - Controls audio settings and volume
   - Notifies when playback completes

2. **AudioStreamer.cs**
   - Handles streaming audio from server URLs
   - Downloads and processes audio files
   - Provides fallback mechanisms for failed streaming
   - Sends playback status updates to server

3. **MicrophoneCapture.cs**
   - Captures audio from VR headset microphone
   - Processes and encodes audio for transmission
   - Implements voice activity detection
   - Manages audio recording state

### Support Components

1. **ProgressHandler.cs**
   - Processes progress updates from server
   - Shows "thinking" indicators during processing
   - Updates progress displays and status messages
   - Handles transcript updates

2. **SessionSynchronizer.cs**
   - Ensures client-server session synchronization
   - Maintains session ID consistency
   - Provides client capabilities information to server
   - Handles reconnection session recovery

## Recent Enhancements

The Unity client has been recently improved with:

1. **Transcript Display System**
   - Shows both user speech and interviewer responses
   - Preserves user text while waiting for responses
   - Provides "Interviewer is thinking..." feedback
   - Updates response text when received

2. **Enhanced Error Handling**
   - Better handling of connection interruptions
   - Graceful fallbacks for audio playback issues
   - Improved error message display
   - Session recovery mechanisms

3. **Progress Visualization**
   - Displays progress updates during processing
   - Shows "thinking" messages during LLM generation
   - Provides status updates for long operations
   - Visual indicators for system state

4. **Session Synchronization**
   - Fixed client-server session ID mismatches
   - Implemented mapping between client and server IDs
   - Added reconnection logic with session preservation
   - Enhanced capability negotiation with server

## Event System

The Unity client uses an event-driven architecture for communication between components:

1. **WebSocketClient Events**
   - `OnMessageReceived`: Triggered when message arrives from server
   - `OnConnected`: Triggered when connection is established
   - `OnDisconnected`: Triggered when connection is lost
   - `OnError`: Triggered when connection error occurs

2. **MessageHandler Events**
   - `MessageReceived`: Triggered for all incoming messages
   - `OnStateUpdate`: Triggered for state change messages
   - `OnAudioResponse`: Triggered for audio response messages
   - `OnError`: Triggered for error messages

3. **SessionManager Events**
   - `OnSessionStarted`: Triggered when session begins
   - `OnSessionEnded`: Triggered when session ends
   - `OnStateChanged`: Triggered when conversation state changes
   - `OnResponseComplete`: Triggered when response playback finishes

## Implementation Details

### 1. Message Handling System

The message handling system uses a central `MessageHandler` that:
- Processes incoming WebSocket messages
- Parses message JSON and determines message type
- Routes messages to specific handlers based on type
- Provides a registration system for components to handle specific messages

```csharp
// Registering for specific message types
messageHandler.RegisterMessageHandler("progress_update", OnProgressUpdate);
messageHandler.RegisterMessageHandler("thinking_update", OnThinkingUpdate);
messageHandler.RegisterMessageHandler("transcript_update", OnTranscriptUpdate);
```

### 2. WebSocket Communication

The WebSocket client handles:
- Connection establishment and maintenance
- Message sending and receiving
- Error handling and reconnection
- Session ID management

```csharp
// Sending audio data to server
public async Task SendAudioData(byte[] audioData, string sessionId)
{
    // Create audio message
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
```

### 3. Transcript Display

The transcript display system:
- Shows user speech with "You:" prefix
- Shows AI responses with "Interviewer:" prefix
- Provides "Interviewer is thinking..." indicator during processing
- Updates only the appropriate section of the transcript

```csharp
// UIManager methods for transcript handling
public void ShowUserTranscript(string text)
{
    if (userTranscriptText != null && !string.IsNullOrEmpty(text))
        userTranscriptText.text = $"You: {text}";
}

public void ShowLLMResponse(string text)
{
    if (llmResponseText != null)
    {
        if (string.IsNullOrEmpty(text))
            llmResponseText.text = "Interviewer is thinking...";
        else
            llmResponseText.text = $"Interviewer: {text}";
    }
}
```

### 4. Progress Handling

The progress handling system:
- Processes progress updates from server
- Updates UI with progress information
- Shows thinking indicators during LLM processing
- Manages progress bar and status text

```csharp
// Progress handler methods
private void OnProgressUpdate(string jsonMessage)
{
    JObject data = JObject.Parse(jsonMessage);
    string message = data["message"]?.ToString() ?? "Processing...";
    
    // Update UI
    if (uiManager != null)
    {
        uiManager.UpdateStatus(message);
        uiManager.UpdateProgress(progress);
    }
}
```

## Common Issues and Solutions

### 1. Interface vs. MonoBehaviour Pattern

The system previously had issues with the MessageHandler being defined as an interface, which isn't compatible with Unity's component system. The solution was:

- Making MessageHandler a MonoBehaviour that implements an interface
- This allows both interface-based access and Unity's component-based access
- Enables FindObjectOfType<MessageHandler>() to work correctly

### 2. Event Signature Mismatches

Event signature mismatches caused various compilation errors. The solution was:
- Standardizing event signatures across components
- Ensuring publisher and subscriber signatures match exactly
- Using simple delegate types (Action<string>) instead of complex ones

### 3. Session ID Synchronization

Issues occurred when client and server had different session IDs. The fix involved:
- Accepting server-generated session IDs
- Implementing mapping between client and server IDs
- Properly handling ID synchronization during reconnections

## Development Guidelines

When helping with this project:

1. **Unity Best Practices**
   - Follow component-based design principles
   - Use events for loose coupling between components
   - Keep MonoBehaviours focused on specific responsibilities
   - Use coroutines for operations spanning multiple frames

2. **WebSocket Communication**
   - Handle asynchronous operations properly
   - Always check connection status before sending
   - Include proper error handling
   - Consider message size and frequency

3. **VR-Specific Considerations**
   - Optimize for performance on mobile VR hardware
   - Consider comfort and usability in VR interface design
   - Handle frame rate carefully, especially during audio processing
   - Test with actual VR hardware when possible

4. **Error Handling**
   - Implement graceful fallbacks
   - Provide clear feedback to users
   - Recover from connection interruptions
   - Log errors for debugging

## Key Areas Claude Can Help With

1. **Component Architecture**
   - Recommendations for component structure
   - Event-based communication patterns
   - Interface design and implementation

2. **WebSocket Communication**
   - Async/await patterns in Unity
   - Message handling optimization
   - Error recovery strategies

3. **UI Implementation**
   - Transcript display improvements
   - Progress visualization techniques
   - VR-friendly UI patterns

4. **Error Handling Strategies**
   - Robust error detection
   - Graceful fallback mechanisms
   - User-friendly error presentation

5. **Code Simplification**
   - Reducing redundancy
   - Improving maintainability
   - Enhancing readability

## Project Structure

The client project follows Unity's standard structure with scripts organized by functionality:

```
D:\VRSystemTest\
├── Assets/
│   ├── Scripts/
│   │   ├── Audio/              # Audio recording and playback
│   │   ├── Avatar/             # Avatar animation and control
│   │   ├── Core/               # Core systems and managers
│   │   ├── Network/            # WebSocket and message handling
│   │   ├── UI/                 # User interface components
│   │   └── Utils/              # Utility scripts
│   ├── Prefabs/                # Reusable object prefabs
│   ├── Scenes/                 # Unity scenes
│   └── Resources/              # Runtime resources
├── Packages/                   # Unity packages
└── ProjectSettings/            # Unity project settings
```

## Conclusion

The VR Interview System's Unity client provides a robust VR interface for the interview practice experience. By understanding the component structure, message handling system, and event flow, you can effectively contribute to maintaining and enhancing the system while ensuring compatibility with the Python server component.