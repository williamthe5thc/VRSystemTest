# VR Interview System

## Project Location

- **Development Directory**: `D:\VRSystemTest\`
- **Unity Project**: The main Unity project is located in the root directory.
- **Documentation**: Detailed documentation files are in the `docs\` folder.

## Overview

The VR Interview System is a virtual reality application designed to simulate interview experiences with an AI-powered virtual interviewer. The system features real-time audio communication, lip-synced avatar animations, and natural conversation flow within a VR environment.

### Key Features

- Real-time speech-to-text and text-to-speech processing
- Animated virtual interviewer with lip sync and facial expressions
- WebSocket communication with a Python backend server
- Voice activity detection and audio streaming
- Responsive avatar with contextual gestures and expressions
- VR-optimized user interface
- Session management and state synchronization
- Detailed feedback and debugging tools

### Target Platforms

- Oculus Quest / Quest 2 / Quest Pro
- Other SteamVR compatible headsets

## Documentation

Comprehensive documentation is available in the `docs/` directory:

1. **[System Overview](docs/unity_system_overview.md)** - High-level architecture, component relationships, and system design
2. **[Communication Documentation](docs/unity_communication_documentation.md)** - WebSocket integration and messaging protocols
3. **[Audio Documentation](docs/unity_audio_documentation.md)** - Microphone capture, audio processing, and playback
4. **[UI Documentation](docs/unity_ui_documentation.md)** - User interface components and VR interaction patterns
5. **[Avatar Documentation](docs/unity_avatar_documentation.md)** - Avatar animation, lip sync, and gesture systems
6. **[Event System Documentation](docs/unity_event_system_documentation.md)** - Event architecture and message routing
7. **[Class Reference](docs/unity_class_reference.md)** - Detailed reference of all major classes

## Implementation Details

### Technologies Used

- **Unity Version**: 2022.2 or newer
- **Backend**: Python server with websockets
- **Audio**: Unity Microphone API for capture, AudioSource for playback
- **Networking**: NativeWebSocket for WebSocket communication
- **Serialization**: Newtonsoft.Json for message processing
- **Avatar Animation**: Unity Animator with blend shapes for facial expressions
- **UI**: TextMeshPro for text rendering, Unity UI system for interfaces

### Dependencies

- Newtonsoft.Json for JSON parsing
- NativeWebSocket for WebSocket communication
- TextMeshPro for UI text
- Optional: Oculus Integration SDK for Quest-specific features

## Common Problems and Solutions

### Connection Issues

1. **WebSocket Connection Failures**
   - **Symptoms**: Unable to connect to server, frequent disconnections
   - **Solution**: Verify server URL in settings, check network connectivity, ensure server is running

2. **Message Handling Errors**
   - **Symptoms**: Error logs about malformed JSON, missing fields
   - **Solution**: Ensure client and server are compatible versions, validate message structure

### Audio Problems

1. **Microphone Access Issues**
   - **Symptoms**: No microphone devices available, recording fails
   - **Solution**: Add microphone permissions to manifest, request permissions at runtime

2. **Audio Quality Issues**
   - **Symptoms**: Distortion in recorded audio
   - **Solution**: Adjust audio levels, implement dynamic gain control

### Performance Issues

1. **Frame Rate Drops**
   - **Symptoms**: Stuttering during audio processing or avatar animation
   - **Solution**: Implement threading for audio processing, use LOD for avatar at distance

2. **Memory Issues**
   - **Symptoms**: Growing memory usage, eventual crash
   - **Solution**: Properly dispose audio buffers, limit transcript history

## System Architecture

The VR Interview System follows a modular architecture with clear separation of concerns:

```
                  ┌─────────────────┐
                  │                 │
                  │   AppManager    │◄───────────┐
                  │                 │            │
                  └────────┬────────┘            │
                           │                     │
                           ▼                     │
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│                 │  │                 │  │                 │
│  WebSocketClient│◄─┤ SessionManager  │──┤  SettingsManager│
│                 │  │                 │  │                 │
└────────┬────────┘  └────────┬────────┘  └─────────────────┘
         │                    │
         │                    │
         ▼                    ▼
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│                 │  │                 │  │                 │
│  MessageHandler │──┤  AudioHandler   │──┤  UIManager      │
│                 │  │                 │  │                 │
└────────┬────────┘  └────────┬────────┘  └─────────────────┘
         │                    │
         │                    │
         ▼                    ▼
┌─────────────────┐  ┌─────────────────┐
│                 │  │                 │
│ AvatarController│  │ MicrophoneCapture
│                 │  │                 │
└─────────────────┘  └─────────────────┘
```

### Key Components:

1. **AppManager**: Main application controller and initialization
2. **SessionManager**: Handles interview session state and communication
3. **WebSocketClient**: Manages WebSocket connection to the server
4. **MessageHandler**: Processes and routes server messages
5. **AudioHandler**: Manages audio recording and playback
6. **AvatarController**: Controls the virtual interviewer's animations
7. **UIManager**: Manages UI elements and user feedback

## Data Flow

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

## Project Structure

```
VRSystemTest/
├── Assets/
│   ├── Scripts/
│   │   ├── Audio/                  # Audio recording and playback
│   │   ├── Avatar/                 # Avatar animation and control
│   │   ├── Core/                   # Core system components
│   │   ├── Environment/            # VR environment elements
│   │   ├── Network/                # WebSocket and communication
│   │   ├── Tests/                  # Testing scripts
│   │   └── UI/                     # User interface components
│   ├── Prefabs/                    # Reusable game objects
│   ├── Scenes/                     # Unity scenes
│   ├── Materials/                  # Material assets
│   ├── Models/                     # 3D models
│   ├── Animations/                 # Animation assets
│   └── Plugins/                    # Third-party plugins
├── Packages/                       # Unity package dependencies
├── ProjectSettings/                # Unity project settings
└── docs/                           # Documentation files
```

## Key Files and Their Purpose

### Core System Files

- `AppManager.cs`: Main application controller, initializes the system
- `SessionManager.cs`: Manages the interview session state and lifecycle
- `SceneInitializer.cs`: Handles Unity scene initialization and setup
- `SettingsManager.cs`: User preferences and application configuration

### Network Files

- `WebSocketClient.cs`: Manages WebSocket connection to the server
- `MessageHandler.cs`: Processes messages from the server
- `ClientProtocolHandler.cs`: Implements communication protocol
- `ClientCapabilities.cs`: Defines client capabilities for the server

### Audio Files

- `MicrophoneCapture.cs`: Records audio from microphone
- `AudioProcessor.cs`: Processes audio data for transmission
- `AudioPlayback.cs`: Plays audio responses from server
- `AudioStreamer.cs`: Handles streaming audio

### Avatar Files

- `AvatarController.cs`: Manages avatar state and animations
- `LipSync.cs`: Controls avatar lip movements during speech
- `FacialExpressions.cs`: Handles avatar facial expressions
- `GestureSystem.cs`: Controls avatar hand and body gestures

### UI Files

- `UIManager.cs`: Central controller for all UI elements
- `DebugDisplay.cs`: Shows debug information in-game
- `FeedbackSystem.cs`: Handles user feedback collection
- `VRInteractionUI.cs`: Manages VR-specific UI interactions

## Configuration

### Server Configuration

The server connection can be configured in the following ways:

1. **Runtime Settings**: Accessible via the in-app settings menu
2. **SettingsManager**: Initialize with different values in code
3. **PlayerPrefs**: Values stored between sessions

Key server settings:
- `ServerUrl`: WebSocket server URL (default: "ws://localhost:8765")
- `AutoConnect`: Whether to connect automatically on startup
- `ReconnectOnDisconnect`: Attempt automatic reconnection when connection lost
- `MaxReconnectAttempts`: Maximum number of reconnection attempts

### Audio Configuration

Audio settings can be adjusted in the following ways:

1. **Inspector**: Adjust values in the Unity Inspector
2. **Settings Menu**: Adjust via the in-app settings UI
3. **Code**: Modify programmatically via the AudioProcessor

Key audio settings:
- `SampleRate`: Audio sample rate (default: 16000)
- `Channels`: Audio channel count (mono/stereo)
- `VoiceDetectionThreshold`: Threshold for voice activity detection
- `SilenceTimeoutSec`: Time of silence before stopping recording

### Avatar Configuration

Avatar behavior can be configured in the following ways:

1. **Inspector**: Adjust values in the Unity Inspector
2. **Animation Assets**: Modify animation clips and transitions
3. **Blend Shapes**: Adjust facial expression configurations

Key avatar settings:
- `BlendDuration`: Duration for blending between facial expressions
- `RandomGestureInterval`: Frequency of random gestures
- `RandomBlinkInterval`: Frequency of blinking animations
- `AnimationSpeed`: General animation speed multiplier

---

## Getting Started

1. Clone the repository or unpack the project files to your local machine
2. Open the project in Unity 2022.2 or newer
3. Start the Python server component (separate repository)
4. Open the MainScene in Unity
5. Configure the ServerUrl in the SettingsManager component to point to your server
6. Press Play to test in editor or build for your target VR device

## License

[License details to be added by project owner]

## Contributors

[Contributor list to be added by project owner]
