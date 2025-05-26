# VRM Integration Guide for VR Interview System

## Overview

This document describes the integration of VRM (Virtual Reality Model) avatars into the VR Interview System. The system supports both standard Unity humanoid avatars and VRM avatars, with specialized components to handle the unique features of VRM models including blend shapes, expressions, and animations.

## VRM Avatar Components

The VR Interview System integrates VRM avatars through several specialized components:

1. **VRMAvatarAdapter**: Bridges the core avatar system with VRM-specific functionality
2. **VRMLipSync**: Manages lip synchronization specifically for VRM blend shapes
3. **VRMFacialExpressions**: Maps our custom expressions to VRM's standard blend shape presets
4. **VRMAvatarSetup**: Helper component to automatically configure VRM avatars

## VRMAvatarSetup Component

The `VRMAvatarSetup` component is designed to automatically configure VRM avatars with all necessary components:

```csharp
// Main functionality: automatically configures a VRM avatar
public void SetupVRMAvatar()
{
    // Find the VRM model if not assigned
    // Find required VRM components (BlendShapeProxy, Animator)
    // Find audio components
    // Set up VRM-specific components (LipSync, FacialExpressions)
    // Configure standard AvatarController to work with VRM
}
```

### Setup Process:

1. **Find the VRM model**: Locates the VRM avatar in the scene using VRMMeta component
2. **Locate required components**: Finds the VRMBlendShapeProxy and Animator
3. **Set up VRM-specific components**: Adds and configures VRMLipSync and VRMFacialExpressions
4. **Configure AvatarController**: Links the standard controller with the VRM-specific components

## Expression Mapping

The VR Interview System maps our custom expressions to VRM's standard blend shape presets using a dictionary:

```csharp
// From VRMFacialExpressions.cs
private Dictionary<FacialExpression, BlendShapePreset> expressionMap;

private void InitializeExpressionMap()
{
    expressionMap = new Dictionary<FacialExpression, BlendShapePreset>
    {
        { FacialExpression.Neutral, BlendShapePreset.Neutral },
        { FacialExpression.Happy, BlendShapePreset.Joy },
        { FacialExpression.Sad, BlendShapePreset.Sorrow },
        { FacialExpression.Angry, BlendShapePreset.Angry },
        { FacialExpression.Surprised, BlendShapePreset.Fun },
        { FacialExpression.Confused, BlendShapePreset.Sorrow }, // Reuse with lower intensity
        { FacialExpression.Thoughtful, BlendShapePreset.Sorrow }, // Reuse with lower intensity
        { FacialExpression.Interested, BlendShapePreset.Joy }, // Reuse with lower intensity
        { FacialExpression.Attentive, BlendShapePreset.Neutral }, // Slight modification
        { FacialExpression.Talking, BlendShapePreset.Neutral } // Handled by lip sync
    };
}
```

### Intensity Adjustments

For expressions that share the same VRM preset but differ in our system, we adjust the intensity:

```csharp
private float GetExpressionIntensity(FacialExpression expression)
{
    switch (expression)
    {
        case FacialExpression.Happy:
        case FacialExpression.Sad:
        case FacialExpression.Angry:
        case FacialExpression.Surprised:
            return expressionIntensity;
                
        case FacialExpression.Confused:
            return expressionIntensity * 0.7f;
                
        case FacialExpression.Thoughtful:
            return expressionIntensity * 0.5f;
                
        case FacialExpression.Interested:
            return expressionIntensity * 0.4f;
                
        case FacialExpression.Attentive:
            return expressionIntensity * 0.2f;
                
        default:
            return 0f;
    }
}
```

## Lip Sync Implementation

VRM avatars use a specialized lip sync system that utilizes the VRM blend shape presets:

```csharp
private void ApplyLipSyncToVRM(float value)
{
    if (blendShapeProxy == null) return;
    
    try
    {
        // Scale the value by the max mouth open amount
        float scaledValue = value * maxMouthOpenValue;
        
        if (useVisemeBlending)
        {
            // Advanced approach: blend between different visemes based on value
            if (value < 0.2f)
            {
                // Mouth mostly closed
                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), scaledValue * 5f);
                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), 0f);
            }
            else
            {
                // Mouth more open - transition to "A" viseme
                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), 
                    Mathf.Lerp(1f, 0f, (value - 0.2f) / 0.8f) * maxMouthOpenValue);
                
                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), 
                    Mathf.Lerp(0f, 1f, (value - 0.2f) / 0.8f) * maxMouthOpenValue);
            }
        }
        else
        {
            // Simple approach: just use the "A" blend shape
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), scaledValue);
        }
        
        // Apply the changes
        blendShapeProxy.Apply();
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error applying lip sync to VRM: {ex.Message}");
        _isLipSyncActive = false;
    }
}
```

## Setting Up a New VRM Avatar

To set up a new VRM avatar in the system:

1. **Import the VRM Model**:
   - Import the `.vrm` file into the Unity project
   - Ensure the model has the necessary blend shapes

2. **Add the VRM Components**:
   - Add the `VRMAvatarSetup` component to the root GameObject
   - Set references to the AudioSource and AudioPlayback components
   - Click the "Setup VRM Avatar" button or wait for Start() to execute

3. **Configure Animation Controller**:
   - Assign the InterviewerAnimator controller to the Animator component
   - Ensure the avatar has a proper humanoid rig setup

4. **Test Expressions**:
   - Play the scene and use AvatarAnimationTester to test expressions
   - Verify that lip sync works correctly when audio is played
   - Check that all facial expressions map correctly

## Common Issues and Solutions

1. **Missing Blend Shapes**:
   - VRM models may have different blend shape naming conventions
   - Solution: Use the VRM editor to map custom blend shapes to standard presets

2. **Animation Issues**:
   - VRM models might use different bone structures
   - Solution: Create avatar-specific animator controllers or use retargeting

3. **Performance Considerations**:
   - VRM models can be performance-intensive
   - Solution: Reduce blend shape updates for distant avatars, use LOD

4. **Initial Setup Failures**:
   - VRMAvatarSetup might not find all components
   - Solution: Manually assign components in the Inspector

## Advanced VRM Customization

For advanced customization of VRM avatars:

1. **Custom Blend Shape Mapping**:
   - Extend the `expressionMap` dictionary with new expressions
   - Create custom blend shape combinations for unique expressions

2. **Animation Blending**:
   - Use animation layers to blend between different animations
   - Create avatar masks to isolate specific body parts

3. **Performance Optimization**:
   - Use `ImmediatelySetValue` with `Apply` batching for better performance
   - Pre-compile expression mappings to avoid runtime dictionary lookups
   - Implement LOD for blend shapes when avatar is at a distance

## Conclusion

The VRM integration in the VR Interview System provides flexibility for using industry-standard VRM avatars. The specialized components handle the complexities of VRM expressions and animations, making it easy to integrate new avatars into the system.

For further customization, refer to the UniVRM documentation and the VRM specification.