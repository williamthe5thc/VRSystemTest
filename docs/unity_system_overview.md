# VR Interview System - Unity Client Overview

## Architecture Overview

The VR Interview System is a virtual reality application designed to simulate an interview experience. The Unity client establishes a WebSocket connection to a Python server to handle audio recording, speech-to-text transcription, natural language processing, and text-to-speech conversion. The system features an animated avatar that responds to user interactions with appropriate animations, lip-syncing, and facial expressions.

## Component Relationships and Dependencies

The system follows a modular architecture with clear separation of concerns. Here's a high-level breakdown of the main components and their relationships:

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

### Core Dependencies:

1. **AppManager** → **SessionManager**: Controls session start/end
2. **SessionManager** → **WebSocketClient**: Manages server communication
3. **WebSocketClient** → **MessageHandler**: Routes incoming messages
4. **MessageHandler** → **AudioHandler**: Processes audio responses  
5. **MessageHandler** → **AvatarController**: Controls avatar state
6. **MessageHandler** → **UIManager**: Updates UI based on messages
7. **MicrophoneCapture** → **AudioProcessor**: Processes recorded audio
8. **MicrophoneCapture** → **WebSocketClient**: Sends audio to server
9. **AvatarController** → **LipSync**: Manages lip movement during speech

## Event Flow Diagrams

### Conversation Flow

```
┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐
│  IDLE   │────►│LISTENING│────►│PROCESSING─────►│RESPONDING│
└─────────┘     └─────────┘     └─────────┘     └─────────┘
     ▲                                               │
     │                                               │
     └───────────────────┬───────────────────────────┘
                         │
                     ┌─────────┐
                     │ WAITING │
                     └─────────┘
```

### Audio Recording Flow

```
User Speaks ──► MicrophoneCapture ──► AudioProcessor ──► WebSocketClient ──► Python Server
                      │                                         ▲
                      │                                         │
                      └─────────► SessionManager ───────────────┘
```

### Audio Response Flow

```
Python Server ──► WebSocketClient ──► MessageHandler ──► AudioPlayback ──► Unity Audio System
                                            │
                                            │
                                            ▼
                                     AvatarController
                                            │
                                            │
                                            ▼
                                         LipSync
```

## Startup and Initialization Sequence

1. **AppManager Initialization**:
   - Singleton setup
   - Load settings via SettingsManager
   - Request necessary permissions (microphone access)

2. **SessionManager Initialization**:
   - Generate or restore session ID
   - Subscribe to WebSocketClient events
   - Subscribe to MessageHandler events

3. **WebSocketClient Initialization**:
   - Configure server URL from settings
   - Establish WebSocket connection if autoConnect is enabled
   - Set up event handlers

4. **Component Registration**:
   - MessageHandler registers with WebSocketClient
   - AudioPlayback registers with MessageHandler
   - AvatarController initializes animation system

5. **UI Setup**:
   - Initialize panels
   - Set default states
   - Configure feedback mechanisms

## Scene Structure and Organization

The VR Interview System consists of several key GameObjects with specific responsibilities:

### Main Scene Hierarchy:

```
- VRSystemManager [AppManager]
  |
  ├── NetworkManager [WebSocketClient, SessionManager]
  |
  ├── AudioSystem [AudioProcessor, MicrophoneCapture, AudioPlayback]
  |
  ├── InterviewerAvatar [AvatarController, LipSync, FacialExpressions, GestureSystem]
  |
  ├── UI [UIManager, FeedbackSystem]
  |
  └── VRPlayer [VRInputHandler, VRRigSetup]
```

## Key Components

1. **Core System Components**:
   - AppManager: Main application controller
   - SessionManager: Manages interview session state
   - SettingsManager: Handles user preferences

2. **Network Components**:
   - WebSocketClient: Handles communication with server
   - MessageHandler: Parses and routes messages
   - ClientProtocolHandler: Implements communication protocol

3. **Audio Components**:
   - MicrophoneCapture: Records user audio
   - AudioProcessor: Formats audio for server
   - AudioPlayback: Plays server responses

4. **Avatar Components**:
   - AvatarController: Manages avatar state
   - LipSync: Animates mouth during speech
   - FacialExpressions: Controls avatar emotions
   - GestureSystem: Manages hand and body gestures

5. **UI Components**:
   - UIManager: Controls all UI elements
   - FeedbackSystem: Displays user feedback
   - TranscriptDisplay: Shows conversation history

## Performance Considerations

The system incorporates several optimizations for VR performance:

1. **Audio Processing**:
   - Audio is processed in chunks to avoid frame drops
   - Sample rate is fixed at 16KHz for server compatibility

2. **Network Optimization**:
   - Websocket connection maintains persistent connection
   - Reconnection logic with exponential backoff
   - Message queuing for offline operation

3. **Avatar Rendering**:
   - Simplified facial expressions for performance
   - Optimized animation state machine
   - Adaptive gesture system based on performance metrics

4. **VR-Specific Optimizations**:
   - Careful UI placement for comfort
   - Optimized for Quest hardware
   - Event-based architecture to maintain frame rate

## State Management

The system implements a state machine pattern for managing the interview flow:

1. **IDLE**: Initial state, waiting for interaction
2. **LISTENING**: Recording user's voice
3. **PROCESSING**: Sending audio to server and waiting for response
4. **RESPONDING**: Playing audio response with avatar animation
5. **WAITING**: Completed response, waiting for user input
6. **ERROR**: Error state with visual feedback

State transitions are triggered by:
- User actions (starting to speak)
- Server messages (state_update messages)
- System events (audio playback completion)

## Communication Patterns

The system uses a publisher-subscriber pattern for inter-component communication:

- Components expose events (OnStateChanged, OnMessageReceived)
- Other components subscribe to these events
- This decouples components and allows for modular architecture

## Special VR Considerations

1. **Comfort and Usability**:
   - Avatar positioned at comfortable distance
   - UI elements follow VR design best practices
   - Audio feedback to complement visual cues

2. **Quest-Specific Implementation**:
   - Microphone permission handling
   - Performance optimizations for mobile VR
   - Power management considerations

3. **Input Handling**:
   - VRInputHandler component for controller input
   - Gaze-based interaction for accessibility
   - Voice-activated controls

## Potential Improvements

Based on code analysis, potential areas for improvement include:

1. **Architecture Refinements**:
   - More consistent event system naming
   - Clearer error handling patterns
   - Better separation of VR-specific code

2. **Performance Enhancements**:
   - Further optimization of audio processing
   - More efficient avatar animation blending
   - Lazy initialization of components

3. **Feature Enhancements**:
   - Better offline mode support
   - Enhanced avatar expressions
   - More robust error recovery

## Conclusion

The VR Interview System Unity client represents a well-structured application designed for VR interaction. It effectively manages the complexities of real-time audio communication, avatar animation, and user interface within a VR environment while maintaining modularity and extensibility.
