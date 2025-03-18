# Server Integration Guide

This document outlines how the Unity client integrates with the Python server component of the VR Interview System.

## Overview

The VR Interview System consists of two main components:
1. **Unity Client**: Oculus Quest application that provides the VR interface
2. **Python Server**: Backend service that handles language processing and conversation management

These components communicate via WebSocket, exchanging JSON messages to coordinate the interview experience.

## Communication Protocol

### Client-Server Message Types

#### From Client to Server:
- **audio_data**: Raw audio from client microphone
  ```json
  {
    "type": "audio_data",
    "session_id": "client-session-123",
    "data": "base64-encoded-audio-data",
    "timestamp": 1647891234.567
  }
  ```

- **ping**: Heartbeat message to maintain connection
  ```json
  {
    "type": "ping",
    "session_id": "client-session-123",
    "timestamp": 1647891234.567
  }
  ```

- **control**: Session management commands
  ```json
  {
    "type": "control",
    "session_id": "client-session-123",
    "command": "end_session",
    "timestamp": 1647891234.567
  }
  ```

- **client_capabilities**: Client feature support information
  ```json
  {
    "type": "client_capabilities",
    "session_id": "client-session-123",
    "supports_streaming": true,
    "supported_formats": ["mp3", "wav"],
    "client_info": {
      "platform": "Oculus Quest 3",
      "version": "1.0.0"
    }
  }
  ```

- **streaming_status**: Streaming playback status updates
  ```json
  {
    "type": "streaming_status",
    "session_id": "client-session-123",
    "status": "playing",
    "position": 2.45
  }
  ```

- **playback_complete**: Notification when playback completes
  ```json
  {
    "type": "playback_complete",
    "session_id": "client-session-123",
    "timestamp": 1647891234.567
  }
  ```

#### From Server to Client:
- **state_update**: State transition notifications
  ```json
  {
    "type": "state_update",
    "previous_state": "LISTENING",
    "new_state": "PROCESSING",
    "message": "Processing your response",
    "timestamp": 1647891234.567
  }
  ```

- **audio_response**: Direct audio data
  ```json
  {
    "type": "audio_response",
    "data": "base64-encoded-audio-data",
    "format": "mp3",
    "timestamp": 1647891234.567
  }
  ```

- **audio_stream_url**: URL for streaming audio
  ```json
  {
    "type": "audio_stream_url",
    "url": "http://server:port/audio/response_123.mp3",
    "format": "mp3",
    "timestamp": 1647891234.567
  }
  ```

- **text_response**: Fallback text when audio fails
  ```json
  {
    "type": "text_response",
    "text": "I understand your perspective on team leadership...",
    "timestamp": 1647891234.567
  }
  ```

- **system_message**: System notifications
  ```json
  {
    "type": "system_message",
    "message": "Connection restored",
    "severity": "info",
    "timestamp": 1647891234.567
  }
  ```

- **error**: Error information
  ```json
  {
    "type": "error",
    "code": "audio_processing_failed",
    "message": "Failed to process audio input",
    "timestamp": 1647891234.567
  }
  ```

## Connection Flow

1. **Establish Connection**
   ```csharp
   // In WebSocketClient.cs
   public async Task Connect()
   {
       try
       {
           await _websocket.Connect(_serverUrl);
           
           // Generate client-side session ID
           string clientSessionId = System.Guid.NewGuid().ToString();
           _sessionManager.SetClientSessionId(clientSessionId);
           
           // Send capabilities
           await SendClientCapabilities(clientSessionId);
           
           // Trigger connected event
           OnConnected?.Invoke();
       }
       catch (Exception ex)
       {
           Debug.LogError($"Connection error: {ex.Message}");
           OnError?.Invoke(ex.Message);
       }
   }
   ```

2. **Session Initialization**
   ```csharp
   // In SessionManager.cs
   public void InitializeSession(string serverSessionId)
   {
       // Map client session ID to server session ID
       _serverSessionId = serverSessionId;
       _isSessionActive = true;
       
       // Clear conversation history
       _conversationHistory.Clear();
       
       // Trigger session started event
       OnSessionStarted?.Invoke(serverSessionId);
   }
   ```

## Conversation Flow

1. **User Speaks**
   - MicrophoneCapture records audio
   - Audio is sent to server via WebSocketClient
   - Server transitions to LISTENING state

2. **Audio Processing**
   - Server processes speech with STT
   - Server generates response with LLM
   - Server synthesizes speech with TTS
   - Server transitions through PROCESSING to RESPONDING

3. **AI Response**
   - Server sends audio response to client
   - Client plays audio and animates avatar
   - Client notifies server when playback completes
   - Server transitions to WAITING

## Error Handling

### Client-Side Error Handling

```csharp
// In WebSocketClient.cs
private async Task HandleConnectionError(Exception ex)
{
    Debug.LogError($"WebSocket error: {ex.Message}");
    
    // Notify error handler
    OnError?.Invoke(ex.Message);
    
    // Attempt reconnection
    if (_autoReconnect && !_isReconnecting)
    {
        _isReconnecting = true;
        await ReconnectWithBackoff();
    }
}

private async Task ReconnectWithBackoff()
{
    int attempt = 0;
    while (_autoReconnect && attempt < _maxReconnectAttempts)
    {
        attempt++;
        
        // Exponential backoff
        int waitTime = Math.Min(_baseReconnectDelayMs * (int)Math.Pow(2, attempt - 1), _maxReconnectDelayMs);
        Debug.Log($"Reconnect attempt {attempt} in {waitTime}ms");
        
        await Task.Delay(waitTime);
        
        try
        {
            await Connect();
            _isReconnecting = false;
            
            // Try to recover session
            if (_sessionManager.HasActiveSession)
            {
                await SendSessionRecovery(_sessionManager.GetClientSessionId());
            }
            
            Debug.Log("Reconnected successfully");
            return;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Reconnection attempt {attempt} failed: {ex.Message}");
        }
    }
    
    _isReconnecting = false;
    Debug.LogError("Failed to reconnect after maximum attempts");
}
```

### Handling Server-Side Errors

```csharp
// In MessageHandler.cs
private void HandleErrorMessage(string jsonMessage)
{
    try
    {
        ErrorMessage errorMsg = JsonUtility.FromJson<ErrorMessage>(jsonMessage);
        
        Debug.LogError($"Server error: {errorMsg.code} - {errorMsg.message}");
        
        // Show error to user
        _uiManager.ShowError(errorMsg.message);
        
        // Take action based on error code
        switch (errorMsg.code)
        {
            case "audio_processing_failed":
                _uiManager.PromptUserToRepeat();
                break;
                
            case "server_busy":
                StartCoroutine(RetryAfterDelay(3.0f));
                break;
                
            case "session_expired":
                _sessionManager.ResetSession();
                _websocketClient.Reconnect();
                break;
                
            default:
                // General error handling
                break;
        }
        
        // Notify error subscribers
        OnError?.Invoke(errorMsg.code, errorMsg.message);
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error parsing error message: {ex.Message}");
    }
}
```

## Audio Format Compatibility

The client and server need to agree on audio formats:

1. **Client-to-Server Audio**:
   - Format: 16-bit PCM WAV
   - Sample Rate: 16000 Hz
   - Channels: Mono
   - Encoding: Base64 for transmission

2. **Server-to-Client Audio**:
   - Primary Format: MP3 (streaming)
   - Fallback Format: WAV
   - Text fallback when audio fails

## Integration Testing

To test integration between client and server:

1. **Start Python Server**:
   ```bash
   cd D:\vr_interview_system
   python server.py
   ```

2. **Configure Unity Client**:
   - Set WebSocketClient.ServerUrl to "ws://localhost:8765"
   - Enable debug logging for WebSocket communication

3. **Test Connection**:
   - Use Unity Play mode to test connection
   - Verify WebSocket connection established
   - Check session initialization

4. **Test Conversation Flow**:
   - Test microphone capture
   - Verify audio transmission
   - Test state transitions
   - Verify response playback

## Troubleshooting

### Common Issues and Solutions

1. **Connection Refused**
   - Check server is running
   - Verify correct server URL and port
   - Check firewall settings

2. **Audio Transmission Issues**
   - Verify microphone permissions
   - Check audio encoding format
   - Validate Base64 encoding

3. **Message Parsing Errors**
   - Verify JSON structure matches expectations
   - Check for proper serialization/deserialization
   - Ensure proper UTF-8 encoding

4. **Session Synchronization Problems**
   - Implement robust session ID management
   - Handle reconnection with session recovery
   - Maintain session state on client
