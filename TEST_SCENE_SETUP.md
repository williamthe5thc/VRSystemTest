# VR Interview System - Test Scene Setup Guide

This guide provides step-by-step instructions for creating a basic test scene to verify your VR Interview System functionality.

## Prerequisites

- Unity 2022.3 LTS or newer
- All required packages installed:
  - XR Interaction Toolkit
  - XR Plugin Management
  - Oculus XR Plugin
  - Input System
  - TextMeshPro
  - Newtonsoft.Json
  - NativeWebSocket

## Creating the Test Scene

1. **Create a New Scene**
   - Go to `File > New Scene`
   - Save it as `TestScene` in the `Assets/Scenes` folder

2. **Set up Required GameObjects**

   Add the following hierarchy:

   ```
   TestScene
   ├── Core
   │   ├── SceneInitializer
   │   ├── SessionManager
   │   ├── SettingsManager
   │
   ├── Network
   │   ├── WebSocketClient
   │   ├── MessageHandler
   │   ├── ConnectionManager
   │
   ├── Audio
   │   ├── MicrophoneCapture
   │   ├── AudioPlayback
   │   ├── AudioProcessor
   │
   ├── XRRig
   │   ├── Camera Offset
   │   │   └── Main Camera
   │   ├── LeftHand Controller
   │   └── RightHand Controller
   │
   ├── Avatar
   │   ├── InterviewerModel (placeholder cube for now)
   │
   └── UI
       ├── MainCanvas (World Space)
       │   ├── StatePanel
       │   ├── DebugPanel
       └── VRMenu (World Space)
   ```

3. **Add Components to GameObjects**

   - **SceneInitializer**: Add `SceneInitializer.cs`
   - **SessionManager**: Add `SessionManager.cs`
   - **SettingsManager**: Add `SettingsManager.cs`
   - **WebSocketClient**: Add `WebSocketClient.cs`
   - **MessageHandler**: Add `MessageHandler.cs`
   - **ConnectionManager**: Add `ConnectionManager.cs`
   - **MicrophoneCapture**: Add `MicrophoneCapture.cs`
   - **AudioPlayback**: Add `AudioPlayback.cs`
   - **AudioProcessor**: Add `AudioProcessor.cs`
   - **XRRig**: Add `VRRigSetup.cs`
   - **Avatar/InterviewerModel**: Add `AvatarController.cs`, `FacialExpressions.cs`, `GestureSystem.cs`, and `LipSync.cs`
   - **MainCanvas**: Add `UIManager.cs`
   - **VRMenu**: Add `VRInteractionUI.cs`

4. **Configure References**

   Configure these critical references:
   
   - In **SessionManager**:
     - WebSocketClient: Assign the WebSocketClient GameObject
     - MessageHandler: Assign the MessageHandler GameObject
     - UIManager: Assign the UIManager component on MainCanvas
     - AvatarController: Assign the AvatarController component on Avatar
   
   - In **WebSocketClient**:
     - Server URL: Set to the URL of your Python WebSocket server (e.g., "ws://localhost:8765")
   
   - In **MessageHandler**:
     - WebSocketClient: Assign the WebSocketClient GameObject
     - AudioPlayback: Assign the AudioPlayback GameObject
     - AvatarController: Assign the AvatarController component on Avatar
     - UIManager: Assign the UIManager component on MainCanvas
   
   - In **ConnectionManager**:
     - WebSocketClient: Assign the WebSocketClient GameObject
     - UIManager: Assign the UIManager component on MainCanvas
   
   - In **MicrophoneCapture**:
     - WebSocketClient: Assign the WebSocketClient GameObject
     - AudioProcessor: Assign the AudioProcessor GameObject
     - SessionManager: Assign the SessionManager GameObject
   
   - In **AudioPlayback**:
     - AudioProcessor: Assign the AudioProcessor GameObject
     - SessionManager: Assign the SessionManager GameObject
     - AvatarController: Assign the AvatarController component on Avatar

5. **Create Simple UI Elements**

   - **StatePanel**:
     - Add a Text (TMP) element for displaying the current state
     - Add an Image for the state icon
   
   - **DebugPanel**:
     - Add a Text (TMP) element for debug output
     - Add a Toggle button for showing/hiding debug info
   
   - **VRMenu**:
     - Add buttons for Start, Pause, Resume, and Exit

6. **Configure Avatar Placeholder**

   - Create a simple cube as a placeholder for the avatar
   - Apply a material with a different color for the front face
   - Position it at eye level about 1.5m in front of the XRRig

## Testing the Basic Functionality

1. **Enter Play Mode**
   - Enter Play Mode in the Unity Editor
   - Verify that the XR Rig initializes correctly
   - Verify that the WebSocketClient attempts to connect to the server

2. **Test Server Connection**
   - Start your Python WebSocket server
   - Verify that the WebSocketClient successfully connects
   - Check the Debug Panel for connection status messages

3. **Test State Transitions**
   - Use the Debug Panel to observe state changes
   - Test microphone input and verify it transitions to LISTENING state
   - Test audio playback by manually triggering it

4. **Create First Prefabs**
   - After verifying the basic functionality, create prefabs for:
     - Core (containing SessionManager, SettingsManager, etc.)
     - Network (containing WebSocketClient, MessageHandler, etc.)
     - Audio (containing MicrophoneCapture, AudioPlayback, etc.)
     - UI (containing MainCanvas, VRMenu, etc.)

## Troubleshooting

1. **Connection Issues**
   - Verify server URL
   - Check firewall settings
   - Verify server is running and accessible

2. **Audio Issues**
   - Verify microphone permissions
   - Check default microphone device
   - Test audio playback with a sample file

3. **UI Interaction Issues**
   - Verify XR Interaction Toolkit setup
   - Check ray interactor configuration
   - Verify canvas interaction settings
