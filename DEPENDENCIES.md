# VR Interview System - Dependency Installation Guide

This guide addresses the compilation errors in the VR Interview System project by installing the necessary packages.

## Required Packages

The project requires the following Unity packages:

1. **XR Interaction Toolkit** - For VR interactions
2. **XR Plugin Management** - For Oculus support
3. **Input System** - For controller input
4. **TextMeshPro** - For UI text elements
5. **Newtonsoft.Json** - For JSON parsing
6. **NativeWebSocket** - For WebSocket communication

## Installation Steps

### 1. Install Packages via Package Manager

1. Open the Unity Package Manager:
   - Go to **Window > Package Manager**

2. Install the XR Interaction Toolkit:
   - Click the "+" button in the top-left corner
   - Select "Add package by name..."
   - Enter `com.unity.xr.interaction.toolkit`
   - Click "Add"

3. Install XR Plugin Management:
   - Click the "+" button in the top-left corner
   - Select "Add package by name..."
   - Enter `com.unity.xr.management`
   - Click "Add"

4. Install Oculus XR Plugin:
   - Click the "+" button in the top-left corner
   - Select "Add package by name..."
   - Enter `com.unity.xr.oculus`
   - Click "Add"

5. Install Input System:
   - Click the "+" button in the top-left corner
   - Select "Add package by name..."
   - Enter `com.unity.inputsystem`
   - Click "Add"

6. Install TextMeshPro:
   - Click the "+" button in the top-left corner
   - Select "Add package by name..."
   - Enter `com.unity.textmeshpro`
   - Click "Add"

7. Install Newtonsoft.Json:
   - Click the "+" button in the top-left corner
   - Select "Add package by name..."
   - Enter `com.unity.nuget.newtonsoft-json`
   - Click "Add"

### 2. Install NativeWebSocket

The NativeWebSocket package needs to be installed from Git:

1. Click the "+" button in the top-left corner of the Package Manager
2. Select "Add package from git URL..."
3. Enter `https://github.com/endel/NativeWebSocket.git`
4. Click "Add"

### 3. Define Missing Enum Types

Create a new script to define the missing `FacialExpression` enum:

1. Create a new C# script in `Assets\Scripts\Avatar` called `FacialExpressionTypes.cs`
2. Replace its contents with:

```csharp
using UnityEngine;

/// <summary>
/// Defines facial expression types for the avatar.
/// </summary>
public enum FacialExpression
{
    Neutral,
    Happy,
    Sad,
    Surprised,
    Angry,
    Confused,
    Thoughtful,
    Interested,
    Attentive,
    Talking,
    Smiling,
    Frowning
}
```

### 4. Enable Input System Backend

After installing the Input System package, you need to enable it:

1. Go to **Edit > Project Settings > Player**
2. In the "Other Settings" section, find "Active Input Handling"
3. Change it from "Input Manager (Old)" to "Both" or "Input System Package (New)"
4. Unity will ask to restart - click "Yes"

### 5. Configure XR Plugin Management

1. Go to **Edit > Project Settings > XR Plugin Management**
2. Click "Install XR Plugin Management" if prompted
3. Under the Android tab:
   - Check the "Oculus" box
   - Configure settings as needed for Oculus Quest

### 6. Restart Unity

After installing all packages, restart Unity to ensure all changes take effect.

## Troubleshooting

If you continue to see errors after installing the packages:

### Missing WebSocketCloseCode Enum

If you still have an error with WebSocketCloseCode, add this definition:

1. Create a new C# script in `Assets\Scripts\Network` called `WebSocketCloseCodeDefinition.cs`
2. Replace its contents with:

```csharp
namespace NativeWebSocket
{
    public enum WebSocketCloseCode
    {
        Normal = 1000,
        Away = 1001,
        ProtocolError = 1002,
        UnsupportedData = 1003,
        Undefined = 1004,
        NoStatus = 1005,
        Abnormal = 1006,
        InvalidData = 1007,
        PolicyViolation = 1008,
        TooBig = 1009,
        MandatoryExtension = 1010,
        ServerError = 1011,
        TlsHandshakeFailure = 1015
    }
}
```

### Other Type Definitions

If you encounter errors with specific classes not being found, check:

1. That you have properly installed all packages
2. That your project's API compatibility level is set to .NET 4.x:
   - Go to **Edit > Project Settings > Player > Other Settings**
   - Set "API Compatibility Level" to ".NET 4.x"

## After Installation

Once all dependencies are installed, the compilation errors should be resolved, and you can proceed with setting up the scenes as described in the SCENE_SETUP.md file.
