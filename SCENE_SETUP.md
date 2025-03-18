# VR Interview System - Scene Setup Instructions

This guide provides step-by-step instructions for setting up the scenes required for the VR Interview System.

## Required Scenes

1. `MainMenu` - The entry point and configuration scene
2. `CorporateOffice` - Corporate interview environment
3. `StartupOffice` - Startup interview environment
4. `CasualOffice` - Casual interview environment

## Scene Setup Process

### MainMenu Scene

1. **Create the MainMenu Scene:**
   - In Unity, go to `File > New Scene`
   - Save it as `MainMenu` in the `Assets/Scenes` folder

2. **Set up the Scene Hierarchy:**
   ```
   MainMenu
   ├── SceneInitializer
   ├── AppManager
   ├── XRRig
   │   ├── Camera Offset
   │   │   └── Main Camera
   │   ├── LeftHand Controller
   │   └── RightHand Controller
   └── UI
       ├── MainMenuCanvas
       │   ├── MainMenuPanel
       │   │   ├── Title
       │   │   ├── StartButton
       │   │   ├── SettingsButton
       │   │   ├── InfoButton
       │   │   ├── ExitButton
       │   │   ├── EnvironmentDropdown
       │   │   └── AvatarDropdown
       │   ├── SettingsPanel
       │   │   ├── Title
       │   │   ├── VolumeSlider
       │   │   ├── MicToggle
       │   │   ├── ServerUrlInput
       │   │   └── BackButton
       │   └── InfoPanel
       │       ├── Title
       │       ├── InfoText
       │       └── BackButton
       └── ConnectionStatusCanvas
           └── ConnectionStatusPanel
               ├── StatusText
               └── StatusIcon
   ```

3. **Attach Scripts:**
   - `SceneInitializer` GameObject: Add `SceneInitializer.cs`
   - `AppManager` GameObject: Add `AppManager.cs` and `SettingsManager.cs`
   - `XRRig` GameObject: Add `VRRigSetup.cs`
   - `MainMenuCanvas` GameObject: Add `MenuController.cs`
   - `ConnectionStatusCanvas` GameObject: Add `UIManager.cs`

4. **Configure the Canvas:**
   - Set the `MainMenuCanvas` and `ConnectionStatusCanvas` to "World Space"
   - Position them appropriately in front of the camera
   - Set the render mode to "World Space"
   - Adjust scale to be readable in VR (around 0.001)

### Environment Scenes (CorporateOffice, StartupOffice, CasualOffice)

1. **Create the Environment Scenes:**
   - Create three new scenes and save them in `Assets/Scenes/Environments` as:
     - `CorporateOffice.unity`
     - `StartupOffice.unity`
     - `CasualOffice.unity`

2. **Set up the Scene Hierarchy (example for CorporateOffice):**
   ```
   CorporateOffice
   ├── SceneInitializer
   ├── SessionManager
   ├── NetworkManager
   │   ├── WebSocketClient
   │   └── MessageHandler
   ├── AudioManager
   │   ├── MicrophoneCapture
   │   └── AudioPlayback
   ├── XRRig
   │   ├── Camera Offset
   │   │   └── Main Camera
   │   ├── LeftHand Controller
   │   └── RightHand Controller
   ├── Environment
   │   ├── EnvironmentManager
   │   ├── Office (3D models)
   │   ├── Furniture (3D models)
   │   ├── Props (3D models)
   │   └── LightingControl
   ├── Avatar
   │   ├── AvatarController
   │   ├── Model
   │   ├── LipSync
   │   ├── FacialExpressions
   │   └── GestureSystem
   └── UI
       ├── MainCanvas
       │   ├── StatePanel
       │   ├── ErrorPanel
       │   └── DebugPanel
       ├── VRMenuCanvas
       │   ├── VRMenuPanel
       │   └── FeedbackPanel
       └── ConnectionStatusCanvas
           └── ConnectionStatusPanel
   ```

3. **Attach Scripts:**
   - `SceneInitializer` GameObject: Add `SceneInitializer.cs`
   - `SessionManager` GameObject: Add `SessionManager.cs`
   - `NetworkManager` GameObject: 
     - Add to `WebSocketClient` child: `WebSocketClient.cs`
     - Add to `MessageHandler` child: `MessageHandler.cs`
     - Add to parent object: `ConnectionManager.cs`
   - `AudioManager` GameObject:
     - Add to `MicrophoneCapture` child: `MicrophoneCapture.cs`
     - Add to `AudioPlayback` child: `AudioPlayback.cs` and `AudioProcessor.cs`
   - `Environment` GameObject:
     - Add to parent: `EnvironmentManager.cs`
     - Add to `LightingControl` child: `LightingControl.cs`
     - Add to any interactive objects: `InteractableItems.cs`
   - `Avatar` GameObject:
     - Add to parent: `AvatarController.cs`
     - Add to parent: `LipSync.cs`
     - Add to parent: `FacialExpressions.cs`
     - Add to parent: `GestureSystem.cs`
   - `UI` GameObject:
     - Add to `MainCanvas`: `UIManager.cs`
     - Add to `VRMenuCanvas`: `VRInteractionUI.cs`
     - Add to `FeedbackPanel`: `FeedbackSystem.cs`
     - Add to `DebugPanel`: `DebugDisplay.cs`

4. **Configure References:**
   - In the `SessionManager` component, drag the appropriate objects to set up references:
     - WebSocketClient
     - MessageHandler
     - UIManager
     - AvatarController
   - In the `WebSocketClient` component, set the server URL to match your Python backend
   - In the `UIManager` component, assign all the UI references
   - In the `AvatarController` component, set up references to the animator and models
   - In the `VRInteractionUI` component, set up references to the XR controllers
   - In the `FeedbackSystem` component, set up references to the UI elements and SessionManager

5. **Set Up Lighting:**
   - Add directional light for the main lighting
   - Set up reflection probes for realistic reflections
   - Configure ambient lighting based on the environment theme
   - For best performance, bake lighting for static objects

## XR Rig Setup

Each scene should include a properly configured XR Rig:

1. **Create the XR Rig:**
   - In the Hierarchy, right-click and select `XR > XR Origin (VR)`
   - This will create the XR Rig with camera and controllers

2. **Configure the XR Rig:**
   - Add the `VRRigSetup.cs` script to the XR Rig
   - Configure tracking origin mode (typically set to "Floor")
   - Set the camera height to an appropriate value (around 1.7m)

3. **Configure Input Actions:**
   - Set up the XR Controller components on the Left and Right Hand Controller objects
   - Assign the appropriate action references for grip, trigger, primary button, etc.

## UI Setup for VR

1. **World Space Canvas Setup:**
   - Set Canvas render mode to "World Space"
   - Scale canvas appropriately (usually around 0.001 for readable text in VR)
   - Position canvases at comfortable reading distances

2. **Interaction Setup:**
   - Add XR Interactable components to UI elements that need to be interactive
   - Set up ray interactors on the controllers for pointing at UI
   - Configure UI interaction with the XR Interaction Toolkit

3. **Feedback Panel Setup:**
   - Create visual feedback elements for showing system state
   - Add buttons for providing feedback after responses
   - Configure animations for smooth transitions

## Avatar Setup

1. **Import an Avatar Model:**
   - Import a humanoid avatar model (FBX format)
   - Configure the rig as "Humanoid" in the import settings
   - Set up appropriate animation avatar definition

2. **Set Up Animations:**
   - Create an Animator Controller for the avatar
   - Add animation states for:
     - Idle
     - Listening
     - Thinking
     - Speaking
     - Gesturing
   - Set up transitions between states

3. **Blend Shape Setup:**
   - Identify the blend shapes for lip-sync (if the model supports them)
   - Map them in the LipSync component
   - Test with sample audio to ensure correct mapping

## Build Settings

1. **Scene Management:**
   - Add all scenes to the build settings (File > Build Settings)
   - Ensure MainMenu scene is set as the first scene (index 0)

2. **Android Configuration:**
   - Switch platform to Android
   - Set Texture Compression to ASTC
   - Enable Multithreaded Rendering
   - Configure Package Name
   - Set Minimum API Level to Android 10 (API level 29)

3. **Oculus Quest Specific Settings:**
   - In XR Plugin Management, enable Oculus
   - Configure the Quest-specific performance settings
   - Set appropriate quality settings for the Quest hardware

## Testing

1. **In-Editor Testing:**
   - Use the XR Device Simulator to test basic functionality in the editor
   - Test scene transitions and UI interactions
   - Verify WebSocket connection to your server

2. **On-Device Testing:**
   - Build and deploy to Oculus Quest
   - Test in an appropriate play space
   - Verify all VR interactions work correctly
   - Test microphone input and audio output
   - Verify connection to the server works from the Quest

## Performance Optimization

1. **Graphics Optimization:**
   - Use mobile-friendly shaders
   - Reduce polygon count on models
   - Optimize texture sizes (1024x1024 or less recommended)
   - Implement LOD (Level of Detail) for complex objects

2. **CPU Optimization:**
   - Reduce update calls in scripts
   - Use object pooling where appropriate
   - Minimize physics calculations
   - Implement spatial partitioning for large environments

3. **Memory Optimization:**
   - Unload unused assets when changing scenes
   - Use asset bundles for environment variations
   - Implement streaming for large environments

## Troubleshooting

1. **Connection Issues:**
   - Verify server URL in WebSocketClient.cs
   - Ensure server and Quest are on the same network
   - Check firewall settings
   - Verify server is running and accessible

2. **Performance Issues:**
   - Use the Oculus Debug Tool to monitor performance
   - Look for CPU/GPU bottlenecks
   - Reduce environment complexity
   - Simplify avatar animations

3. **Audio Issues:**
   - Verify microphone permissions are granted
   - Check audio format compatibility with server
   - Test audio playback with sample files
