# VR Interview System - Unity Client

A Unity-based VR client for an AI-powered interview practice system. This application runs on Oculus Quest devices and connects to a Python server for natural language processing and conversation management.

## Overview

The VR Interview System provides a realistic job interview practice environment in virtual reality. Users can practice their interview skills with an AI-powered interviewer that responds naturally to their answers through speech recognition and synthesis.

### Key Features

- **Realistic VR Environment**: Immersive interview setting with animated interviewer avatar
- **Natural Conversation**: Real-time speech recognition and response generation
- **Feedback System**: Visual feedback on conversation state and progress
- **Robust Error Handling**: Graceful recovery from connection issues and other errors

## Architecture

The Unity client follows a component-based architecture with several key modules:

1. **Core Communication**: WebSocket client, message handling, and session management
2. **User Interface**: UI managers, interaction controls, and feedback systems
3. **Audio Components**: Microphone capture, audio playback, and streaming
4. **Support Components**: Progress handling and session synchronization

See the technical documentation for detailed information about these components.

## Getting Started

### Prerequisites

- Unity 2022.3 LTS or newer
- Oculus Integration package
- Meta Quest Link (for development)
- Oculus Quest headset (for testing)

### Development Setup

1. Clone this repository
2. Open the project in Unity
3. Install the required packages from the Package Manager
4. Open the main scene from `Assets/Scenes/InterviewRoom.unity`
5. Configure the server connection settings in the WebSocketClient component

### Building for Oculus Quest

1. Switch platform to Android in Build Settings
2. Configure Oculus Quest-specific settings
3. Build and deploy to your Quest device

## Server Integration

This client connects to a Python-based server that handles:
- Speech-to-text processing
- Language model interaction
- Text-to-speech generation
- Conversation state management

See the server repository for setup instructions.

## Technical Documentation

For detailed technical information, refer to the following documentation:
- [Component Architecture](DEPENDENCIES.md)
- [Scene Setup Guide](SCENE_SETUP.md)
- [Prefab Documentation](PREFABS.md)
- [Project Status](PROJECT_STATUS.md)

## Contributing

When contributing to this project, please follow these guidelines:
- Follow Unity best practices for component design
- Use events for loose coupling between components
- Keep MonoBehaviours focused on specific responsibilities
- Include proper error handling and fallback mechanisms

## License

[Specify your license information here]
