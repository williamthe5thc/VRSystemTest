# Unity Contribution Guidelines

This document outlines best practices and standards for contributing to the VR Interview System Unity client.

## Code Style Guidelines

### General C# Style
- Use camelCase for private/protected fields and variables
- Use PascalCase for methods, properties, classes, and public fields
- Use meaningful names that express intent
- Keep methods focused and concise (aim for < 50 lines)
- Add comments for complex logic, but prefer readable code over excessive comments
- Use proper XML documentation for public methods and classes

### Unity-Specific Style
- Use `[SerializeField] private` instead of `public` for inspector-visible fields
- Organize inspector fields with `[Header("Category")]` attributes
- Initialize references in Awake() or OnEnable() methods
- Prefer UnityEvents for connecting components when appropriate
- Use namespaces to organize code by feature or system
- Follow the execution order (Awake > OnEnable > Start > Update)

## Component Design

### Best Practices
- Follow single responsibility principle - each component should do one thing well
- Use composition over inheritance whenever possible
- Make components reusable and independent
- Use interfaces to define contracts between components
- Minimize references between components (use events for communication)
- Keep MonoBehaviours lean and focused

### Event-Based Communication
- Use C# events or UnityEvents for loose coupling
- Consider implementing a message bus for global events
- Always unsubscribe from events in OnDisable() or OnDestroy()
- Use consistent event naming conventions

## Performance Considerations

### Optimization Tips
- Use object pooling for frequently created/destroyed objects
- Minimize operations in Update() methods
- Use coroutines for operations that span multiple frames
- Consider frame rate implications in mobile VR (target 72+ FPS)
- Profile code regularly to identify bottlenecks
- Use caching to avoid repeated GetComponent calls
- Implement custom update methods for less-frequent updates

### Memory Management
- Avoid allocations during gameplay (especially in Update())
- Be cautious with string operations (they create garbage)
- Use StringBuilder for string concatenation
- Implement proper cleanup in OnDestroy()
- Be mindful of closures in lambda expressions
- Use custom object pools for frequently allocated objects

## VR-Specific Guidelines

### User Experience
- Always test in VR headset, not just in editor
- Be mindful of comfort and readability in VR
- Test all UI elements at various distances and angles
- Design for different user heights and positions
- Avoid rapid movement that could cause discomfort
- Implement accessible interaction patterns

### Oculus Quest Optimization
- Be aware of mobile hardware limitations
- Use mobile-optimized shaders
- Keep draw calls to a minimum
- Optimize textures and meshes for mobile VR
- Use level of detail (LOD) techniques when appropriate
- Monitor frame rate and performance in device debugger

## Testing

### Test Procedures
- Test all features in Unity Editor play mode
- Test on Oculus Quest hardware before submitting changes
- Verify connection to Python server works correctly
- Test error conditions and recovery mechanisms
- Verify UI readability and usability in VR
- Test microphone capture and audio playback

### Error Handling
- Implement comprehensive error detection
- Add user-friendly error messages
- Create fallback mechanisms for critical operations
- Add detailed logging for easier debugging
- Handle WebSocket connection issues gracefully

## Git Workflow

### Branch Strategy
- Main branch should always be stable
- Create feature branches for new features
- Use the naming convention: `feature/feature-name`
- Create bug fix branches for bug fixes
- Use the naming convention: `bugfix/bug-name`

### Commit Guidelines
- Write clear, concise commit messages
- Use present tense ("Add feature" not "Added feature")
- Reference issue numbers when applicable
- Keep commits focused on single changes
- Commit early and often

### Pull Request Process
1. Create a pull request from your feature branch to main
2. Ensure all tests pass
3. Provide clear description of changes
4. Request review from at least one team member
5. Address review comments
6. Squash commits if necessary before merging

## Project Structure

Maintain the following project structure:

```
Assets/
├── Scripts/
│   ├── Audio/              # Audio recording and playback
│   ├── Avatar/             # Avatar animation and control
│   ├── Core/               # Core systems and managers
│   ├── Network/            # WebSocket and message handling
│   ├── UI/                 # User interface components
│   └── Utils/              # Utility scripts
├── Prefabs/                # Reusable object prefabs
├── Scenes/                 # Unity scenes
├── Resources/              # Runtime resources
└── [ThirdParty]/           # Third-party assets (keep isolated)
```

## Documentation

### Code Documentation
- Document public interfaces with XML comments
- Include parameter descriptions and return values
- Document non-obvious implementations
- Add TODO comments for future improvements
- Update documentation when making significant changes

### Design Documentation
- Update relevant design documents when architecture changes
- Document component interactions and dependencies
- Maintain up-to-date message protocol documentation
- Keep README files current with setup instructions

## Additional Resources

- [Unity C# Coding Standards](https://unity.com/how-to/naming-and-code-style-tips-c-scripting-unity)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Unity Performance Best Practices](https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity.html)
- [Oculus Quest Developer Guidelines](https://developer.oculus.com/documentation/unity/unity-conf-settings/)
