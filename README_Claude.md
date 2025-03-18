# VR Interview System - Technical Guide for Claude

## Project Overview for AI Assistance

This document provides a comprehensive guide to the VR Interview System project structure to help Claude provide better technical assistance. This Unity/Oculus Quest project implements a VR interview practice system that connects to a Python WebSocket server.

## Repository Structure

The main project is located at `D:\VRSystemTest\` with the following structure:

### Key Directories
- `D:\VRSystemTest\Assets\Scripts\` - Contains all C# scripts organized by domain
- `D:\VRSystemTest\Assets\Scenes\` - Contains Unity scenes
- `D:\VRSystemTest\Assets\Prefabs\` - Contains reusable prefabs
- `D:\VRSystemTest\Assets\Models\` - Contains 3D models
- `D:\VRSystemTest\Assets\Materials\` - Contains materials and shaders
- `D:\VRSystemTest\Documentation\` - Contains additional documentation

### Documentation Files
Key markdown files in the root directory:
- `README.md` - General project overview
- `PROJECT_STATUS.md` - Current development status
- `DEPENDENCIES.md` - Required packages and installation
- `NEXT_STEPS.md` - Implementation roadmap
- `OCULUS_SETUP.md` - Oculus Quest setup instructions
- `SCENE_SETUP.md` - Scene setup instructions
- `PREFABS.md` - Prefab creation and usage

## Core Script Organization

The `Assets\Scripts\` directory is organized into logical domains:

### Core/ - Core System Scripts
- `AppManager.cs` - Main application controller
- `SessionManager.cs` - Interview session management
- `SettingsManager.cs` - User settings and preferences
- `VRRigSetup.cs` - VR camera rig setup
- `VRInputHandler.cs` - VR controller input
- `SceneInitializer.cs` - Scene initialization

### Network/ - Communication Scripts
- `WebSocketClient.cs` - WebSocket implementation
- `MessageHandler.cs` - Message parsing and routing
- `ConnectionManager.cs` - Connection state management
- `ClientCapabilities.cs` - Client feature reporting

### Audio/ - Audio System Scripts
- `MicrophoneCapture.cs` - Microphone input handling
- `AudioPlayback.cs` - Response audio playback
- `AudioProcessor.cs` - Audio conversion and formatting

### Avatar/ - Avatar Control Scripts
- `AvatarController.cs` - Main avatar control
- `LipSync.cs` - Lip synchronization with audio
- `FacialExpressions.cs` - Facial animation control
- `GestureSystem.cs` - Hand and body gestures

### Environment/ - Environment Scripts
- `EnvironmentManager.cs` - Environment management
- `InteractableItems.cs` - Interactive objects
- `LightingControl.cs` - Dynamic lighting

### UI/ - User Interface Scripts
- `UIManager.cs` - UI management
- `MenuController.cs` - Menu interactions
- `VRInteractionUI.cs` - VR-specific UI
- `FeedbackSystem.cs` - User feedback collection
- `DebugDisplay.cs` - Debugging display
- `LoadingScreen.cs` - Loading transition

## Key Technical Implementation Details

### State Machine
The system follows a state machine approach:
- **IDLE** - Initial state
- **LISTENING** - Capturing user audio
- **PROCESSING** - Server processing input
- **RESPONDING** - Avatar speaking response
- **WAITING** - Waiting for next user input

### WebSocket Protocol
The communication between client and server uses WebSocket with JSON messages:

#### Client to Server:
- `audio_data` - Base64-encoded audio
- `control` - Session control messages
- `ping` - Connection keepalive
- `playback_complete` - Notification when audio response finishes
- `feedback` - User feedback on responses

#### Server to Client:
- `state_update` - State machine transitions
- `audio_response` - Base64-encoded TTS audio
- `pong` - Response to ping
- `error` - Error notifications

### Audio Pipeline
- Client records 16kHz mono audio
- Audio is encoded and sent to server
- Server processes speech-to-text-to-speech
- Response audio is sent back to client
- Client plays audio through avatar with lip sync

### Core Code Dependencies
The key dependencies between scripts:
- `SessionManager` depends on `WebSocketClient` and `MessageHandler`
- `AudioPlayback` depends on `AudioProcessor`
- `AvatarController` depends on `LipSync` and `FacialExpressions`
- `MicrophoneCapture` depends on `SessionManager` and `WebSocketClient`

## Unity Scene Hierarchy (from screenshot)

The main test scene has the following hierarchy:
- Game
  - AppManager
  - SettingsManager
  - SessionManager
  - ConnectionManager
  - MessageHandler
  - WebSocketClient
  - Audio
    - MicrophoneCapture
    - AudioPlayback
    - AudioProcessor
    - AudioStreamer
  - XR Rig Setup
    - Camera Offset
      - Main Camera
      - Left Controller
      - Right Controller
  - Avatar
    - InterviewerModel
  - UI
    - MainCanvas
      - Text (TMP)
      - Image
      - State Panel
      - Debug Panel
      - Test Panel
      - Debug
    - Stop

## Key Implementation Files to Reference

When answering questions about specific functionality, refer to these key files:

1. **WebSocket Communication**: `WebSocketClient.cs` and `MessageHandler.cs`
2. **Session Management**: `SessionManager.cs`
3. **Audio System**: `MicrophoneCapture.cs` and `AudioPlayback.cs`
4. **Avatar Control**: `AvatarController.cs` and `LipSync.cs`
5. **UI Management**: `UIManager.cs`

## Common Technical Challenges

When assisting with this project, be aware of these common challenges:

1. **WebSocket Connection Issues**: Check `WebSocketClient.cs` for connection handling logic
2. **Audio Format Compatibility**: Look at `AudioProcessor.cs` for format conversion
3. **State Synchronization**: Refer to `SessionManager.cs` and `MessageHandler.cs`
4. **Oculus Quest Performance**: Consider Quest's mobile hardware limitations
5. **Avatar Animation and Lip Sync**: Check `LipSync.cs` and `FacialExpressions.cs`

## Server-Client Integration

The client expects to connect to a Python WebSocket server that:
1. Receives audio data and processes it with STT
2. Routes text to an LLM for response generation
3. Converts responses to audio via TTS
4. Manages the conversation state machine
5. Sends audio and state updates back to client

The server URL is configured in `WebSocketClient.cs` or via `SettingsManager`.

## Development Status

Based on `PROJECT_STATUS.md`, the project is in active development with:
- Core architecture implemented
- Working WebSocket communication
- Audio systems implemented
- Avatar control framework in place
- UI systems partially implemented
- Scene setup and environment building in progress
- Performance optimization pending

## Important Implementation Details

1. **Singleton Pattern**: Used for `AppManager`, `SessionManager`, and `SettingsManager`
2. **Event-based Communication**: Used throughout for loose coupling
3. **Async/Await Pattern**: Used for WebSocket and audio operations
4. **State Machine Pattern**: Core of the interaction flow
5. **Component-based Architecture**: Following Unity best practices

This information should provide Claude with a comprehensive understanding of the project's structure, key files, and technical implementation to better assist with questions and development tasks.