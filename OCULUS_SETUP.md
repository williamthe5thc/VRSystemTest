# Oculus Quest Development Setup Guide

This guide provides step-by-step instructions for configuring Unity for Oculus Quest development.

## Prerequisites

Before you begin:
- Ensure you have Unity 2022.3 LTS or newer installed
- Have an Oculus Developer account (create one at developer.oculus.com)
- Have the Oculus mobile app installed on your phone
- Have your Oculus Quest headset ready and charged

## Unity Configuration

### 1. Install Required Packages

First, install all packages as described in the DEPENDENCIES.md file.

### 2. Configure Build Settings

1. Open **File > Build Settings**
2. Select **Android** as the platform
3. Click **Switch Platform** if it's not already selected
4. Set the following options:
   - Texture Compression: ASTC
   - Check "Development Build" during development
   - Check "Create symbols.zip" for better debugging

### 3. Configure Player Settings

1. Click **Player Settings** in the Build Settings window
2. Under **Other Settings**:
   - Set **Color Space** to "Linear" if possible (better visuals)
   - Set **Graphics APIs** to use Vulkan or OpenGLES3
   - Set **Minimum API Level** to Android 10.0 (API level 29) or higher
   - Set **Target API Level** to "Automatic (highest installed)"
   - Set **Scripting Backend** to IL2CPP
   - Set **Target Architectures** to ARM64 only (for best performance)
   - Set **API Compatibility Level** to .NET 4.x

3. Under **XR Settings**:
   - Check "Virtual Reality Supported" if present
   - Add "Oculus" to the Virtual Reality SDKs list if present

### 4. Configure XR Plugin Management

1. Go to **Edit > Project Settings > XR Plugin Management**
2. If prompted, click "Install XR Plugin Management"
3. Click the **Android** tab
4. Check **Oculus**
5. Click on the **Oculus** item that appears
6. Configure Oculus settings:
   - Stereo Rendering Mode: "Multi Pass" (more compatible) or "Single Pass Instanced" (better performance)
   - Check "Dash Support" and "V2 Signing"

### 5. Configure Input System

1. Go to **Edit > Project Settings > Input System Package** (only visible if the Input System package is installed)
2. If prompted, enable the new input system
3. Create an input action asset for XR controllers:
   - Right-click in the Project window 
   - Select **Create > Input Actions**
   - Name it "XRInputActions"
   - Configure it for XR controller input

### 6. Set Up Meta Account for Quest Development

1. Enable Developer Mode on your Quest:
   - Open the Oculus mobile app
   - Connect to your Quest headset
   - Go to Settings > Oculus Quest > More Settings > Developer Mode
   - Toggle Developer Mode ON

2. Install necessary drivers:
   - Install Oculus ADB Drivers from the Oculus website
   - Install Android Platform Tools (for ADB)

### 7. Building and Deploying

1. Connect your Quest to your computer with a USB cable
2. In Unity, go to **File > Build Settings**
3. Click **Build and Run**
4. When prompted, create a new folder for the build and name it
5. Wait for the build to complete and deploy to your Quest

## Performance Considerations

### Rendering Optimization

1. Limit draw calls to under 100 if possible
2. Use mobile-friendly shaders
3. Keep texture sizes at 1024x1024 or smaller when possible
4. Use occlusion culling
5. Implement LOD (Level of Detail) for complex objects

### CPU Optimization

1. Minimize Update() calls
2. Use object pooling instead of instantiating/destroying
3. Avoid expensive physics operations
4. Keep animation complexity in check

### Memory Management

1. Use texture compression
2. Consider streaming assets
3. Implement proper scene loading/unloading
4. Keep project within 1GB installed size for best performance

## Testing and Profiling

1. Use Unity Profiler to identify performance bottlenecks:
   - Connect to Quest via USB
   - Choose "Connect to Player" in Profiler window
   - Analyze CPU, GPU, and memory usage

2. Use Oculus Debug Tool:
   - Monitor performance metrics
   - Watch for dropped frames
   - Check thermal throttling

## Troubleshooting Common Issues

### Build Fails

- Make sure Developer Mode is enabled on Quest
- Verify USB debugging is allowed (Quest will prompt when connected)
- Check USB cable is data-capable (not charge-only)
- Try restarting the Quest and your computer

### Performance Issues

- Enable "Show Frame Rate" in Oculus settings to monitor performance
- Check for CPU bottlenecks using the profiler
- Reduce environment complexity
- Lower render scale if needed

### Tracking Issues

- Ensure adequate lighting in your playspace
- Clear guardian boundary and reset
- Check camera lenses are clean

## Next Steps

After setting up your development environment:
1. Follow the SCENE_SETUP.md guide to create your scenes
2. Create the necessary prefabs following PREFABS.md
3. Test on the Quest throughout development
