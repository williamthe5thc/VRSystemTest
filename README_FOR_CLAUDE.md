# VR Interview System - Technical Guide for Claude (Unity Client)

## Project Location & Structure

- **Development Directory**: `D:\VRSystemTest\`
- **Unity Project Structure**:
  ```
  VRSystemTest/
  ├── Assets/
  │   ├── Animations/           # Avatar animations
  │   ├── Animator/             # Animation controllers
  │   ├── Materials/            # Material assets
  │   ├── Models/               # 3D model assets
  │   ├── Prefabs/              # Reusable GameObject prefabs
  │   ├── Scenes/               # Unity scenes
  │   ├── Scripts/              # C# scripts (main code)
  │   ├── SunboxGames/          # Third-party avatar assets
  │   ├── test Avatar./         # VRM test avatar assets
  │   ├── TextMesh Pro/         # Text rendering system
  │   ├── UnityJapanOffice/     # Environment assets 
  │   ├── XR/                   # XR device settings
  │   └── XRI/                  # XR Interaction Toolkit settings
  ├── Packages/                 # Unity packages
  ├── ProjectSettings/          # Unity project settings
  └── docs/                     # Documentation files
  ```
- **Documentation**: Detailed documentation is available in the `docs/` folder

> **Note**: For detailed information about the Python server implementation, please refer to `D:\vr_interview_system\README_FOR_CLAUDE.md`, which contains extensive details about the server architecture, audio processing pipeline, and LLM integration.

## System Overview

The VR Interview System is an AI-powered interview practice platform in virtual reality. The Unity client provides the VR interface, avatar animation, audio management, and user interaction, while communicating with a Python server that handles language processing and conversation management.

### Key Features

- Real-time speech-to-text and text-to-speech processing
- Animated virtual interviewer with lip sync and facial expressions
- WebSocket communication with a Python backend server
- Voice activity detection and audio streaming
- Responsive avatar with contextual gestures and expressions
- VR-optimized user interface
- Session management and state synchronization
- Detailed feedback and debugging tools

## Current Implementation Status

### Completed Components
1. **Core Communication Framework**
   - WebSocketClient with robust connection management
   - Enhanced error handling and reconnection logic
   - Session ID synchronization between client and server
   - Message validation and queue management

2. **Client Capability Reporting**
   - Enhanced detection of client capabilities (audio formats, streaming support)
   - Improved serialization for reliable capability reporting
   - Automatic capability resending when needed
   - Diagnostics for troubleshooting capability issues

3. **Session Management**
   - Session persistence across application restarts
   - Session state tracking and synchronization
   - Improved error recovery for connection interruptions
   - Session mappings between client and server IDs

4. **UI Improvements**
   - Enhanced transcript display with speaker differentiation
   - Visual status indicators for connection and processing states
   - Debugging displays for development
   - "Thinking" indicators during processing

5. **Audio Framework**
   - Audio capture with voice activity detection
   - Audio playback with proper format handling
   - Audio streaming framework for TTS responses
   - Synchronization with avatar animations

### In-Progress Components
1. **Audio Streaming System**
   - Advanced streaming from AllTalk TTS server
   - Fallback mechanisms for network issues
   - Format selection based on device capabilities
   - Direct audio download as streaming alternative

2. **Avatar Animation System**
   - Multi-avatar support (Unity standard and VRM formats)
   - Lip sync with audio amplitude analysis
   - Facial expression blending system with 10 distinct expressions
   - Random blinking and idle gesture system

3. **Progress Visualization**
   - Enhanced progress handling for LLM processing
   - Thinking updates during server processing
   - Improved transcript updates
   - Visual feedback for various system states

### Next Development Priorities
1. **Scene Development**
   - Complete environment scenes (Corporate, Casual offices)
   - Configure lighting and atmosphere
   - Position avatar and XR rig optimally
   - Set up spatial audio

2. **Prefab Creation**
   - Create core system prefabs (LoadingScreen, PersistentSystems)
   - Finalize avatar prefabs with multiple character options
   - Build UI element prefabs (TranscriptPanel already documented)
   - Create environment prefabs with office variations

3. **VR Interaction Refinement**
   - Controller-based UI interaction
   - Comfort adjustments for long sessions
   - Gaze-based alternative interactions
   - Teleportation and movement system

4. **Performance Optimization**
   - Profile and optimize for Quest hardware
   - Optimize avatar rendering and animations
   - Enhance audio streaming performance
   - Memory management for long sessions

## Technical Information

### Project Configuration

1. **Unity Setup**:
   - Unity 2022.3 LTS
   - High Definition Render Pipeline for enhanced visuals
   - Oculus XR Plugin (v4.0.0)
   - XR Interaction Toolkit (v2.4.3)
   - NativeWebSocket for WebSocket communication

2. **Avatar Systems**:
   - Dual support for standard Unity and VRM avatars
   - SunboxGames avatar assets (Male/Female base models)
   - VRM test avatars with standard blend shapes
   - Custom animation controllers for interviewer behaviors

3. **VR Configuration**:
   - Oculus Quest targeting (Android platform)
   - OpenXR and OculusXR loaders configured
   - Input System for controller interaction
   - XR Interaction Toolkit for VR UI

4. **Scene Organization**:
   - Bootstrap.unity - Initialization scene
   - MainMenu.unity - Configuration interface
   - Test scenes for development
   - Environment scenes under development:
     - Corporate.unity - Formal office environment
     - Casual.unity - Casual interview setting

## Core Components

### Communication Components

1. **WebSocketClient.cs** (Enhanced)
   - Handles WebSocket communication with the Python server
   - Includes robust connection management with auto-reconnection
   - Implements message queue for handling offline scenarios
   - Provides detailed error handling and logging
   - Performs message validation before sending
   - Manages resource cleanup properly

2. **MessageHandler.cs** (Enhanced)
   - Central message processing hub with improved routing
   - Handles different message types more efficiently
   - Includes message validation for better error detection
   - Provides registration system for message-specific handlers
   - Supports enhanced debugging and logging capabilities

3. **SessionManager.cs** (Enhanced)
   - Maintains session state with improved synchronization
   - Supports session ID mapping between client and server
   - Implements ping mechanism for connection maintenance
   - Stores conversation history for context preservation
   - Controls session lifecycle with robust error handling

4. **ClientCapabilities.cs** and **EnhancedClientCapabilities.cs**
   - Detects and reports device capabilities to the server
   - Handles audio format support detection
   - Manages streaming capability reporting
   - Ensures proper JSON serialization for reliable communication
   - Includes diagnostic information for troubleshooting

5. **ProgressHandler.cs** (New)
   - Processes progress updates from the server during LLM generation
   - Handles "thinking" updates for visual feedback
   - Manages transcript updates for both user and LLM text
   - Routes messages to appropriate UI components
   - Provides debugging information

### User Interface Components

1. **UIManager.cs**
   - Manages all UI elements and displays
   - Shows transcripts with clear speaker identification
   - Provides visual feedback about system states
   - Handles notifications and error displays
   - Controls processing indicators and progress visualization

2. **VRInteractionUI.cs**
   - Implements VR-specific interaction controls
   - Manages menu positioning for comfortable viewing
   - Handles VR controller input
   - Provides visual and haptic feedback

3. **FeedbackSystem.cs**
   - Collects user feedback about AI responses
   - Reports feedback to the server
   - Provides visualization of feedback options
   - Helps with system improvement

### Audio Components

1. **AudioPlayback.cs**
   - Plays audio responses from the server
   - Handles different audio formats
   - Manages audio playback queue
   - Notifies when playback completes
   - Synchronizes with avatar animations

2. **AudioProcessor.cs**
   - Processes recorded audio for transmission
   - Converts between audio formats
   - Optimizes audio data size
   - Handles audio quality management

3. **AudioStreamer.cs** (New)
   - Handles streaming audio from AllTalk TTS server
   - Manages streaming status reporting
   - Provides fallback mechanisms for network issues
   - Supports multiple audio formats
   - Includes direct audio download as an alternative

4. **MicrophoneCapture.cs**
   - Captures audio from VR headset microphone
   - Implements voice activity detection
   - Manages audio recording state
   - Provides level feedback during recording

### Avatar Components

1. **AvatarController.cs**
   - Manages avatar state and animations
   - Controls state transitions based on session state
   - Synchronizes with audio playback
   - Coordinates facial expressions and gestures
   - Handles random blinking and idle animations

2. **LipSync.cs**
   - Controls avatar lip movements during speech
   - Implements both amplitude-based and procedural lip sync
   - Uses animation curves for natural mouth movements
   - Provides smooth blending between lip positions
   - Adapts to different audio sources

3. **FacialExpressions.cs**
   - Handles 10 distinct facial expressions (neutral, happy, sad, etc.)
   - Implements smooth transitions between expressions
   - Controls eye blinking and micro-expressions
   - Manages blend shape weights for realistic faces
   - Provides random blinking for natural appearance

4. **GestureSystem.cs**
   - Controls avatar hand and body gestures
   - Synchronizes gestures with speech content
   - Manages random and contextual gestures
   - Provides natural body language

5. **VRMAvatarAdapter.cs** (VRM Support)
   - Bridges the core avatar system with VRM avatar models
   - Maps standard expressions to VRM blend shapes
   - Handles VRM-specific animation requirements
   - Provides compatibility with popular avatar formats
   - Simplifies integration of different avatar models

6. **VRMLipSync.cs** and **VRMFacialExpressions.cs** (VRM Support)
   - Specialized components for VRM avatar animation
   - Maps expressions to VRM BlendShapePresets
   - Manages VRM-specific blend shape controls
   - Provides seamless integration with the core system

## Assets and Resources

1. **Avatar Assets**
   - SunboxGames avatar package with male/female base models
   - VRM test avatars with standard blend shapes (A, O, Blink, etc.)
   - InterviewerAnimator with state transitions for interview behaviors

2. **Environment Assets**
   - UnityJapanOffice assets being integrated
   - Office environment models under development

3. **UI Elements**
   - TranscriptPanel prefab documented with README
   - Other UI elements in development

4. **XR Configuration**
   - Dual support for Oculus and OpenXR loaders
   - XR Interaction Toolkit integration for VR input

## Scene Structure Status

1. **Bootstrap Scene**: Functional
   - Application initialization
   - Settings loading
   - Scene transition

2. **MainMenu Scene**: In Development
   - UI layout created
   - Scene navigation
   - Configuration options

3. **Environment Scenes**: In Development
   - Basic structure defined
   - Avatar positioning
   - Lighting setup

## Development Roadmap

1. **Complete Environment Scenes**
   - Finalize the three office environment scenes
   - Set up proper lighting and atmosphere
   - Position avatar and XR rig optimally
   - Configure spatial audio

2. **Create Core Prefabs**
   - Implement LoadingScreen prefab
   - Create PersistentSystems prefab
   - Finish XRRig prefab configuration
   - Build Avatar prefab variants

3. **Optimize for Quest**
   - Set proper Android build settings
   - Configure shader variants for mobile
   - Optimize memory usage
   - Implement performance monitoring

4. **Test VR Functionality**
   - Test on actual Quest hardware
   - Verify WebSocket communication in VR
   - Test microphone and audio functionality
   - Validate UI readability and interaction

## Technical Recommendations

1. **VRM Avatar Optimization**
   - Consider using "ImmediatelySetValue" with "Apply" batching for better performance
   - Pre-compile expression mappings to avoid runtime dictionary lookups
   - Implement LOD for avatar blend shapes when at distance

2. **Audio Streaming Enhancement**
   - Consider implementing audio caching for frequently used responses
   - Add progressive loading indicators for long audio files
   - Implement bandwidth detection and quality adjustment

3. **WebSocket Performance**
   - Implement binary message protocol for audio data
   - Add compression for large JSON payloads
   - Consider binary serialization for performance-critical messages

4. **UI Optimization**
   - Use object pooling for transcript entries
   - Implement fade-in/out for UI elements to prevent VR discomfort
   - Consider gaze-based interaction as an alternative to controllers

## Key Prefab Setup Required

1. **TranscriptPanel Prefab**
   - Documented in UI/TranscriptPanel/README.txt
   - Requires proper TMPro text components
   - Needs dismiss button functionality

2. **Avatar Prefab**
   - Support for both standard Unity and VRM avatars
   - Configure with proper blend shapes and animations
   - Implement different variants for interviewer types

3. **LoadingScreen Prefab**
   - Canvas with progress bar
   - Loading message display
   - Scene transition handling

4. **PersistentSystems Prefab**
   - AppManager
   - SettingsManager
   - AudioListener

## Conclusion

The VR Interview System Unity client has made significant progress with a robust foundation for communication, audio handling, and avatar animation. The dual support for standard Unity and VRM avatars provides flexibility for character customization, while the enhanced communication layer ensures reliable connection with the Python server.

Current development focus should be on completing the environment scenes, creating the core prefabs, and testing the full system on actual Quest hardware. The project structure and architecture are well-designed, with clear separation of concerns and modular components.

By following the development roadmap and addressing the technical recommendations, the system will provide an immersive and effective interview practice experience in VR.

For detailed implementation information, refer to the documentation files in the `docs/` directory and the current development status in `DEVELOPMENT_CHECKLIST.md` and `PROJECT_STATUS.md`.