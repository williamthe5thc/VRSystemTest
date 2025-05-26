# Avatar Animation System

This document provides detailed information about the avatar animation system used in the VR Interview System. It covers the animation state machine, facial expressions, gesture system, and the integration of both standard Unity avatars and VRM avatars.

## Overview

The avatar animation system is designed to provide natural and responsive animations for the interviewer avatar during the interview process. The system consists of three main components:

1. **Base State Machine**: Controls the overall body state of the avatar
2. **Facial Expression System**: Manages facial expressions via blend shapes
3. **Gesture System**: Controls hand and body gestures during speech

The system supports two avatar types:
- **Standard Unity Avatars**: Using the SunboxGames avatar package
- **VRM Avatars**: Using UniVRM integration for Japanese-style avatars

## Animation State Machine

The animation system uses a layered state machine defined in `InterviewerAnimator.controller`:

### Base Layer

The base layer controls the overall pose and state of the avatar:

- **Idle**: Default resting state
- **Listening**: Active attention state while user is speaking
- **Thinking**: Thoughtful pose during processing
- **Speaking**: Animation during response delivery
- **Attentive**: Alert waiting state after response
- **Confused**: Used during error states

Transitions between states are triggered by the AvatarController component using trigger parameters:
```csharp
// Example usage in AvatarController.cs
public void SetSpeakingState()
{
    if (animator != null)
    {
        animator.SetTrigger("Speaking");
    }
    
    if (facialExpressions != null)
    {
        facialExpressions.SetExpression(FacialExpression.Talking);
    }
}
```

### Gestures Layer

The gestures layer controls hand and arm animations that can play independently of the base state:

- **Empty**: Default state with no gesture
- **HandGesture1**: First gesture animation
- **HandGesture2**: Second gesture animation 
- **HandGesture3**: Third gesture animation

Gestures are triggered randomly or contextually by the GestureSystem component.

### Facial Layer

The facial layer is primarily controlled by blend shapes rather than animations, but provides a base state for blend shape modifications.

## Facial Expression System

Facial expressions are managed through two different implementations:

### Standard Unity Implementation (FacialExpressions.cs)

For standard avatars, facial expressions are managed through blend shapes:

```csharp
private Dictionary<int, float> GetExpressionBlendShapeValues(FacialExpression expression)
{
    Dictionary<int, float> values = new Dictionary<int, float>();
    
    switch (expression)
    {
        case FacialExpression.Neutral:
            values[smileBlendShapeIndex] = 0f;
            values[frownBlendShapeIndex] = 0f;
            break;
            
        case FacialExpression.Happy:
            values[smileBlendShapeIndex] = 100f;
            values[frownBlendShapeIndex] = 0f;
            break;
            
        // Additional expressions defined here
    }
    
    return values;
}
```

### VRM Implementation (VRMFacialExpressions.cs)

For VRM avatars, expressions are mapped to VRM blend shape presets:

```csharp
private void InitializeExpressionMap()
{
    // Maps between custom expressions and VRM blend shape presets
    expressionMap = new Dictionary<FacialExpression, BlendShapePreset>
    {
        { FacialExpression.Neutral, BlendShapePreset.Neutral },
        { FacialExpression.Happy, BlendShapePreset.Joy },
        { FacialExpression.Sad, BlendShapePreset.Sorrow },
        // Additional mappings defined here
    };
}
```

Expressions smoothly transition using coroutines:

```csharp
private IEnumerator TransitionToExpressionCoroutine(FacialExpression targetExpression)
{
    // Fade out previous expression
    // Fade in new expression
    // Apply the final values
}
```

## Lip Sync System

Lip synchronization is implemented in two ways:

### Standard Implementation (LipSync.cs)

For standard avatars, lip sync uses audio amplitude analysis:

```csharp
private void Update()
{
    if (useAmplitudeBasedLipSync && audioSource != null && audioSource.isPlaying)
    {
        // Get audio amplitude for lip sync
        audioSource.GetSpectrumData(_audioSamples, 0, FFTWindow.Rectangular);
        
        // Calculate mouth openness from spectrum data
        float sum = 0f;
        for (int i = 0; i < 64; i++)
        {
            sum += _audioSamples[i];
        }
        
        _targetLipSyncValue = Mathf.Clamp01(sum * lipSyncSensitivity * 100f);
    }
    else if (!useAmplitudeBasedLipSync && _isLipSyncActive)
    {
        // Use procedural lip sync when amplitude-based is not available
        _lipSyncTimer += Time.deltaTime * _lipSyncSpeed;
        
        if (_lipSyncTimer > 1f)
        {
            _lipSyncTimer -= 1f;
        }
        
        // Use animation curve for more natural movement
        _targetLipSyncValue = lipSyncCurve.Evaluate(_lipSyncTimer);
    }
    
    // Apply to blend shapes
    // ...
}
```

### VRM Implementation (VRMLipSync.cs)

For VRM avatars, lip sync is adapted to use VRM-specific blend shapes:

```csharp
private void ApplyLipSyncToVRM(float value)
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
```

## Gesture System

The gesture system triggers hand and arm animations during speech:

```csharp
private IEnumerator PerformRandomGestures()
{
    while (true)
    {
        // Don't perform random gestures during speaking or error states
        if (_currentState != "RESPONDING" && _currentState != "ERROR")
        {
            float waitTime = UnityEngine.Random.Range(randomGestureInterval * 0.5f, randomGestureInterval * 1.5f);
            yield return new WaitForSeconds(waitTime);
            
            // Perform a random gesture if not in critical state
            if (gestureSystem != null)
            {
                gestureSystem.PerformRandomGesture();
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }
    }
}
```

## VRM Avatar Setup

The VRMAvatarSetup.cs script handles automatic configuration of VRM avatars:

```csharp
public void SetupVRMAvatar()
{
    // Step 1: Find the VRM model if not assigned
    // Step 2: Find required VRM components
    // Step 3: Find audio components
    // Step 4: Set up VRM-specific components
    SetupVRMLipSync();
    SetupVRMFacialExpressions();
    SetupAvatarController();
}
```

## How to Use

### Setting Up a Standard Avatar

1. Import the avatar model (FBX format)
2. Configure the rig as "Humanoid" in the import settings
3. Add required blend shapes for facial expressions and lip sync
4. Add the AvatarController, LipSync, and FacialExpressions components
5. Assign the InterviewerAnimator controller to the Animator component

### Setting Up a VRM Avatar

1. Import the VRM model using UniVRM importer
2. Add the VRMAvatarSetup component to the avatar GameObject
3. Call the SetupVRMAvatar() method in Start or via the editor
4. The script will automatically configure the required components

### Testing Animations

Use the AvatarAnimationTester.cs component to test different states and expressions:

```csharp
// Add this component to the avatar
// Configure the references in the Inspector
// Use the buttons to test different states
```

## Animation Files

The following animation files are required:

1. **Base Animations**:
   - Idle.anim
   - Listening.anim
   - Thinking.anim
   - Speaking.anim
   - Attentive.anim
   - Confused.anim

2. **Gesture Animations**:
   - HandGesture1.anim
   - HandGesture2.anim
   - HandGesture3.anim

## Current Development Status

The animation system is currently under development with the following status:

- **Completed**:
  - Animation state machine architecture
  - Facial expression system for standard avatars
  - Basic lip sync implementation
  - VRM integration framework

- **In Progress**:
  - Enhanced lip sync with better audio analysis
  - Contextual gesture system
  - More natural idle animations
  - Expanded facial expression repertoire

- **Planned**:
  - Emotion-based procedural animation blending
  - Context-aware gesture selection
  - Improved finger animations for VRM models
  - Eye tracking and head movement system

## Next Steps for Animation Development

1. **Complete Animation Asset Creation**:
   - Create all required animation clips
   - Adjust timing and transitions
   - Test on both avatar types

2. **Enhance Lip Sync System**:
   - Improve audio amplitude analysis sensitivity
   - Add phoneme-based lip sync for English
   - Optimize for performance on Quest

3. **Expand Gesture Repertoire**:
   - Add more contextual gestures
   - Implement gesture selection based on speech content
   - Create gesture masks for better blending

4. **Test and Optimize VRM Integration**:
   - Ensure consistent behavior across avatar types
   - Optimize blend shape operations for performance
   - Implement LOD for blend shapes at distance