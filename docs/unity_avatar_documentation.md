# VR Interview System - Avatar Documentation

## Architecture Overview

The avatar subsystem in the VR Interview System manages all aspects of the virtual interviewer's visual representation, animations, and expressiveness. It handles facial expressions, lip synchronization, body animations, and gesture systems to create a realistic and engaging interviewer presence. The avatar components are designed to work together to provide natural responses based on the conversation state and audio playback.

## Key Classes/Components

### AvatarController

`AvatarController` is the main component that orchestrates all avatar behaviors and states. It serves as the central hub for coordinating animations, expressions, and lip sync based on the interview state.

```csharp
public class AvatarController : MonoBehaviour
{
    // Avatar state control
    public void SetIdleState()
    public void SetListeningState()
    public void SetThinkingState()
    public void SetSpeakingState()
    public void SetAttentiveState()
    public void SetConfusedState()
    
    // Audio event handling
    public void OnAudioPlaybackStarted()
    public void OnAudioPlaybackCompleted()
    public void UpdateLipSync(float normalizedTime)
    
    // Animation control
    public void SetAnimationSpeed(float speed)
    public string GetCurrentState()
    
    // Events
    public event Action<string> OnStateChanged;
}
```

### LipSync

`LipSync` manages the avatar's mouth movements to match the audio being played, creating the illusion that the avatar is speaking.

```csharp
public class LipSync : MonoBehaviour
{
    // Core functionality
    public void StartLipSync()
    public void StopLipSync()
    public void UpdateLipSyncValue(float audioAmplitude)
    
    // Advanced lip sync features
    public void SetViseme(VisemeType viseme, float blend)
    public void ProcessAudioBuffer(float[] audioBuffer)
    public void SetVisemePreset(string presetName)
    
    // Tuning parameters
    public void SetSmoothingAmount(float amount)
    public void SetIntensity(float intensity)
}
```

### FacialExpressions

`FacialExpressions` controls the avatar's facial expressions to convey emotions and reactions during the interview.

```csharp
public class FacialExpressions : MonoBehaviour
{
    // Basic expressions
    public void SetExpression(FacialExpression expression)
    public void Blink()
    public void ClearExpressions()
    
    // Expression blending
    public void BlendToExpression(FacialExpression expression, float duration)
    public void AddExpressionLayer(FacialExpression expression, float weight)
    
    // Automatic expressions
    public void EnableRandomExpressions(bool enable)
    public void TriggerEmotionalResponse(EmotionType emotion, float intensity)
}
```

### GestureSystem

`GestureSystem` manages the avatar's hand and body gestures to add natural movement and emphasis during speech.

```csharp
public class GestureSystem : MonoBehaviour
{
    // Gesture triggering
    public void PerformGesture(GestureType gestureType)
    public void PerformRandomGesture()
    public void CancelGesture()
    
    // Gesture configuration
    public void SetGestureFrequency(float frequency)
    public void EnableGestureCategory(string category, bool enabled)
    
    // Gesture synchronization
    public void SyncGestureWithSpeech(string speechText)
    public void SetEmphasisPoints(List<float> normalizedTimes)
}
```

### FacialExpressionTypes

`FacialExpressionTypes` provides definitions for the various expressions the avatar can display.

```csharp
public enum FacialExpression
{
    Neutral,
    Interested,
    Thoughtful,
    Talking,
    Attentive,
    Confused,
    Smiling,
    Concerned,
    Surprised,
    Empathetic
}

public enum EmotionType
{
    Neutral,
    Joy,
    Sadness,
    Surprise,
    Fear,
    Anger,
    Disgust,
    Interest,
    Empathy
}
```

## Avatar Animation System

### Animation State Machine

The avatar uses a Unity Animator state machine to control its overall body animations:

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│             │     │             │     │             │
│    IDLE     │────►│  LISTENING  │────►│  THINKING   │
│             │     │             │     │             │
└──────┬──────┘     └─────────────┘     └──────┬──────┘
       │                                       │
       │                                       │
       │            ┌─────────────┐            │
       │            │             │            │
       └───────────►│  ATTENTIVE  │◄───────────┘
                    │             │
                    └──────┬──────┘
                           │
                           │
                    ┌──────▼──────┐
                    │             │
                    │  SPEAKING   │
                    │             │
                    └─────────────┘
```

Key animation states include:

1. **IDLE**: Default relaxed pose with subtle idle movements
2. **LISTENING**: Alert posture with attentive head positioning
3. **THINKING**: Contemplative pose with thoughtful expressions
4. **SPEAKING**: Active animation state with synchronized gestures
5. **ATTENTIVE**: Engaged waiting state between interactions
6. **CONFUSED**: Error state with appropriate confused body language

### Animation Implementation

```csharp
public void SetIdleState()
{
    if (_currentState == "IDLE") return;
    
    _currentState = "IDLE";
    
    if (animator != null)
    {
        animator.SetTrigger(idleTrigger);
    }
    
    if (facialExpressions != null)
    {
        facialExpressions.SetExpression(FacialExpression.Neutral);
    }
    
    if (lipSync != null)
    {
        lipSync.StopLipSync();
    }
    
    OnStateChanged?.Invoke(_currentState);
}

public void SetSpeakingState()
{
    if (_currentState == "RESPONDING") return;
    
    _currentState = "RESPONDING";
    
    if (animator != null)
    {
        animator.SetTrigger(speakingTrigger);
    }
    
    if (facialExpressions != null)
    {
        facialExpressions.SetExpression(FacialExpression.Talking);
    }
    
    if (lipSync != null)
    {
        lipSync.StartLipSync();
    }
    
    OnStateChanged?.Invoke(_currentState);
}
```

## Lip Sync Implementation

The lip sync system uses audio amplitude analysis to drive the avatar's mouth movements:

### Core Lip Sync Process

```csharp
public void StartLipSync()
{
    _isLipSyncActive = true;
    
    // Initialize mouth blend shapes to closed position
    UpdateMouthShape(0f);
    
    // Start automatic random mouth movements if no audio
    if (useRandomMovementsWhenIdle)
    {
        StartCoroutine(PerformRandomMouthMovements());
    }
}

public void UpdateLipSyncValue(float audioAmplitude)
{
    if (!_isLipSyncActive)
        return;
        
    // Apply smoothing to audio amplitude
    _currentAmplitude = Mathf.Lerp(_currentAmplitude, audioAmplitude, Time.deltaTime * smoothingSpeed);
    
    // Scale amplitude by intensity
    float scaledAmplitude = _currentAmplitude * intensity;
    
    // Clamp to valid range
    scaledAmplitude = Mathf.Clamp01(scaledAmplitude);
    
    // Update mouth shape
    UpdateMouthShape(scaledAmplitude);
}

private void UpdateMouthShape(float openAmount)
{
    if (skinnedMeshRenderer == null || mouthOpenBlendShapeIndex < 0)
        return;
        
    // Set blend shape weight
    skinnedMeshRenderer.SetBlendShapeWeight(mouthOpenBlendShapeIndex, openAmount * 100f);
    
    // Update related blend shapes if configured
    if (useMouthWidthBlendShape && mouthWidthBlendShapeIndex >= 0)
    {
        float widthAmount = openAmount * mouthWidthMultiplier;
        skinnedMeshRenderer.SetBlendShapeWeight(mouthWidthBlendShapeIndex, widthAmount * 100f);
    }
    
    if (useJawOpenBlendShape && jawOpenBlendShapeIndex >= 0)
    {
        float jawAmount = openAmount * jawOpenMultiplier;
        skinnedMeshRenderer.SetBlendShapeWeight(jawOpenBlendShapeIndex, jawAmount * 100f);
    }
}
```

### Advanced Viseme-Based Lip Sync

For more sophisticated lip sync, the system can use visemes (visual phonemes):

```csharp
public void ProcessAudioBuffer(float[] audioBuffer)
{
    if (!_isLipSyncActive || audioBuffer == null || audioBuffer.Length == 0)
        return;
        
    // Extract audio features for phoneme detection
    AudioFeatures features = ExtractAudioFeatures(audioBuffer);
    
    // Classify phonemes based on audio features
    Dictionary<VisemeType, float> visemeWeights = ClassifyVisemes(features);
    
    // Apply viseme blending
    ApplyVisemeWeights(visemeWeights);
}

private void ApplyVisemeWeights(Dictionary<VisemeType, float> visemeWeights)
{
    // Reset all blend shapes first
    foreach (int blendShapeIndex in visemeBlendShapeIndices.Values)
    {
        skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, 0f);
    }
    
    // Apply each viseme with its weight
    foreach (var visemePair in visemeWeights)
    {
        if (visemeBlendShapeIndices.TryGetValue(visemePair.Key, out int blendShapeIndex))
        {
            float weight = visemePair.Value * 100f;
            skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, weight);
        }
    }
}
```

## Facial Expressions

The facial expression system controls the avatar's emotional displays:

### Expression System Implementation

```csharp
public void SetExpression(FacialExpression expression)
{
    // Clear current expression
    ClearExpressions();
    
    // Set new expression
    _currentExpression = expression;
    
    // Get blend shape indices and weights for this expression
    Dictionary<int, float> blendShapeWeights = GetExpressionBlendShapes(expression);
    
    // Apply blend shapes
    foreach (var blendShapePair in blendShapeWeights)
    {
        skinnedMeshRenderer.SetBlendShapeWeight(blendShapePair.Key, blendShapePair.Value);
    }
    
    // Save last expression time for blink logic
    _lastExpressionChangeTime = Time.time;
}

public void Blink()
{
    if (skinnedMeshRenderer == null || eyeCloseBlendShapeIndex < 0)
        return;
        
    // Store current eye blend shape value
    float currentEyeCloseWeight = skinnedMeshRenderer.GetBlendShapeWeight(eyeCloseBlendShapeIndex);
    
    // Start blink coroutine
    StartCoroutine(PerformBlink(currentEyeCloseWeight));
}

private IEnumerator PerformBlink(float startWeight)
{
    // Close eyes quickly
    float blinkCloseDuration = 0.1f;
    float elapsedTime = 0f;
    
    while (elapsedTime < blinkCloseDuration)
    {
        float t = elapsedTime / blinkCloseDuration;
        float weight = Mathf.Lerp(startWeight, 100f, t);
        skinnedMeshRenderer.SetBlendShapeWeight(eyeCloseBlendShapeIndex, weight);
        
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    
    // Ensure fully closed
    skinnedMeshRenderer.SetBlendShapeWeight(eyeCloseBlendShapeIndex, 100f);
    
    // Hold blink briefly
    yield return new WaitForSeconds(0.05f);
    
    // Open eyes slightly slower
    float blinkOpenDuration = 0.15f;
    elapsedTime = 0f;
    
    while (elapsedTime < blinkOpenDuration)
    {
        float t = elapsedTime / blinkOpenDuration;
        float weight = Mathf.Lerp(100f, startWeight, t);
        skinnedMeshRenderer.SetBlendShapeWeight(eyeCloseBlendShapeIndex, weight);
        
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    
    // Restore original state
    skinnedMeshRenderer.SetBlendShapeWeight(eyeCloseBlendShapeIndex, startWeight);
}
```

### Expression Blending

The system supports blending between expressions for more natural transitions:

```csharp
public void BlendToExpression(FacialExpression targetExpression, float duration)
{
    if (duration <= 0f)
    {
        // Immediate change
        SetExpression(targetExpression);
        return;
    }
    
    // Start blend coroutine
    StopAllCoroutines();
    StartCoroutine(BlendExpressionCoroutine(_currentExpression, targetExpression, duration));
}

private IEnumerator BlendExpressionCoroutine(FacialExpression startExpression, FacialExpression targetExpression, float duration)
{
    // Get blend shape configurations for start and target expressions
    Dictionary<int, float> startWeights = GetExpressionBlendShapes(startExpression);
    Dictionary<int, float> targetWeights = GetExpressionBlendShapes(targetExpression);
    
    // Combine all blend shape indices used by either expression
    HashSet<int> allBlendShapeIndices = new HashSet<int>();
    foreach (int index in startWeights.Keys) allBlendShapeIndices.Add(index);
    foreach (int index in targetWeights.Keys) allBlendShapeIndices.Add(index);
    
    // Capture starting weights for all involved blend shapes
    Dictionary<int, float> currentWeights = new Dictionary<int, float>();
    foreach (int index in allBlendShapeIndices)
    {
        currentWeights[index] = skinnedMeshRenderer.GetBlendShapeWeight(index);
    }
    
    // Blend over time
    float elapsedTime = 0f;
    
    while (elapsedTime < duration)
    {
        float t = elapsedTime / duration;
        
        // Apply smooth interpolation
        t = Mathf.SmoothStep(0f, 1f, t);
        
        // Update all blend shapes
        foreach (int index in allBlendShapeIndices)
        {
            float startWeight = startWeights.ContainsKey(index) ? startWeights[index] : 0f;
            float targetWeight = targetWeights.ContainsKey(index) ? targetWeights[index] : 0f;
            
            float weight = Mathf.Lerp(startWeight, targetWeight, t);
            skinnedMeshRenderer.SetBlendShapeWeight(index, weight);
        }
        
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    
    // Ensure target expression is exactly applied
    foreach (var pair in targetWeights)
    {
        skinnedMeshRenderer.SetBlendShapeWeight(pair.Key, pair.Value);
    }
    
    _currentExpression = targetExpression;
}
```

## Gesture System

The gesture system controls the avatar's hand and body movements to enhance communication:

### Gesture Implementation

```csharp
public void PerformGesture(GestureType gestureType)
{
    // Cancel any current gesture
    CancelGesture();
    
    // Find animation clip for gesture
    AnimationClip gestureClip = GetGestureClip(gestureType);
    if (gestureClip == null)
        return;
    
    // Create gesture playable
    _gesturePlayable = animator.playableGraph.CreateAnimationClipPlayable(gestureClip);
    
    // Connect to animator and set parameters
    var playableOutput = AnimationPlayableOutput.Create(animator.playableGraph, "Gesture", animator);
    playableOutput.SetSourcePlayable(_gesturePlayable);
    
    // Set layer and weight
    _gesturePlayable.SetLayerAdditive(1);
    _gesturePlayable.SetLayerWeight(0.7f);
    
    // Play the gesture
    _gesturePlayable.Play();
    
    // Track current gesture
    _currentGestureType = gestureType;
    _currentGestureStartTime = Time.time;
    _currentGestureEndTime = Time.time + gestureClip.length;
    
    // Schedule completion callback
    StartCoroutine(CompleteGestureAfterDelay(gestureClip.length));
}

public void PerformRandomGesture()
{
    // Don't perform random gestures if we're in certain states
    if (_avatarController != null)
    {
        string state = _avatarController.GetCurrentState();
        if (state == "RESPONDING" || state == "ERROR")
            return;
    }
    
    // Select a random gesture from appropriate category
    List<GestureType> allowedGestures = new List<GestureType>();
    
    // Idle gestures
    allowedGestures.Add(GestureType.HandsRest);
    allowedGestures.Add(GestureType.ShiftWeight);
    allowedGestures.Add(GestureType.ArmCross);
    allowedGestures.Add(GestureType.HeadTilt);
    
    // Choose random gesture
    if (allowedGestures.Count > 0)
    {
        int randomIndex = UnityEngine.Random.Range(0, allowedGestures.Count);
        PerformGesture(allowedGestures[randomIndex]);
    }
}

private IEnumerator CompleteGestureAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    
    // Smooth transition back to base animation
    float blendDuration = 0.3f;
    float elapsedTime = 0f;
    
    while (elapsedTime < blendDuration && _gesturePlayable.IsValid())
    {
        float weight = Mathf.Lerp(0.7f, 0f, elapsedTime / blendDuration);
        _gesturePlayable.SetLayerWeight(weight);
        
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    
    // Clean up playable
    if (_gesturePlayable.IsValid())
    {
        _gesturePlayable.Destroy();
    }
    
    _currentGestureType = GestureType.None;
}
```

## VR-Specific Considerations

### Gaze Awareness

The avatar can respond to user gaze for more natural interaction:

```csharp
private void UpdateGazeAwareness()
{
    if (!enableGazeAwareness || Camera.main == null)
        return;
        
    // Get direction from avatar to user
    Vector3 directionToUser = Camera.main.transform.position - headTransform.position;
    directionToUser.y = 0f; // Ignore vertical difference
    directionToUser.Normalize();
    
    // Get avatar's forward direction
    Vector3 avatarForward = headTransform.forward;
    avatarForward.y = 0f;
    avatarForward.Normalize();
    
    // Calculate angle between forward and user direction
    float angle = Vector3.Angle(avatarForward, directionToUser);
    
    // Check if user is looking at avatar
    bool isUserLookingAtAvatar = IsUserLookingAtAvatar();
    
    // Update head look target
    if (isUserLookingAtAvatar && angle < maxHeadTurnAngle)
    {
        // Gradually turn head to face user
        Vector3 targetForward = Vector3.Slerp(
            headTransform.forward,
            directionToUser,
            Time.deltaTime * headTurnSpeed
        );
        
        // Apply rotation with constraints
        headTransform.rotation = Quaternion.LookRotation(targetForward, Vector3.up);
        
        // Apply constraints to avoid unnatural rotation
        LimitHeadRotation();
    }
    else
    {
        // Return to default forward orientation
        headTransform.localRotation = Quaternion.Slerp(
            headTransform.localRotation,
            Quaternion.identity,
            Time.deltaTime * headReturnSpeed
        );
    }
}
```

### Personal Space Awareness

The avatar respects personal space in VR for better comfort:

```csharp
private void UpdatePersonalSpaceAwareness()
{
    if (!enablePersonalSpaceAwareness || Camera.main == null)
        return;
        
    // Get distance to user
    float distanceToUser = Vector3.Distance(transform.position, Camera.main.transform.position);
    
    // Check if user is too close
    if (distanceToUser < minComfortableDistance)
    {
        // Calculate lean back amount based on how close the user is
        float leanAmount = Mathf.InverseLerp(minComfortableDistance, personalSpaceThreshold, distanceToUser);
        leanAmount = 1f - leanAmount; // Invert so closer = more lean
        
        // Apply lean back
        Vector3 leanDirection = (transform.position - Camera.main.transform.position).normalized;
        leanDirection.y = 0f; // Only lean back horizontally
        
        // Apply lean to spine transform
        if (spineTransform != null)
        {
            Quaternion targetRotation = Quaternion.Euler(leanAmount * maxLeanAngle, 0f, 0f);
            spineTransform.localRotation = Quaternion.Slerp(
                spineTransform.localRotation,
                targetRotation,
                Time.deltaTime * personalSpaceResponseSpeed
            );
        }
        
        // Possibly trigger a surprised or uncomfortable expression
        if (distanceToUser < personalSpaceThreshold && facialExpressions != null)
        {
            facialExpressions.SetExpression(FacialExpression.Surprised);
            _personalSpaceViolationTime += Time.deltaTime;
            
            // If violation continues, switch to uncomfortable
            if (_personalSpaceViolationTime > 1.5f)
            {
                facialExpressions.SetExpression(FacialExpression.Concerned);
            }
        }
    }
    else
    {
        // Reset lean and expression if user is at comfortable distance
        if (spineTransform != null)
        {
            spineTransform.localRotation = Quaternion.Slerp(
                spineTransform.localRotation,
                Quaternion.identity,
                Time.deltaTime * personalSpaceResponseSpeed
            );
        }
        
        // Reset personal space timer
        _personalSpaceViolationTime = 0f;
    }
}
```

### Performance Optimizations

The avatar system includes VR-specific performance optimizations:

```csharp
private void SetDetailLevel(DetailLevel level)
{
    // Skip if already at this detail level
    if (_currentDetailLevel == level)
        return;
        
    _currentDetailLevel = level;
    
    switch (level)
    {
        case DetailLevel.High:
            // Full blend shapes
            enableFacialBlendShapes = true;
            // Full gestures
            if (gestureSystem != null)
            {
                gestureSystem.SetGestureFrequency(1.0f);
            }
            // High quality materials
            SetMaterialQuality(MaterialQuality.High);
            break;
            
        case DetailLevel.Medium:
            // Limited blend shapes
            enableFacialBlendShapes = true;
            // Reduced gestures
            if (gestureSystem != null)
            {
                gestureSystem.SetGestureFrequency(0.5f);
            }
            // Medium quality materials
            SetMaterialQuality(MaterialQuality.Medium);
            break;
            
        case DetailLevel.Low:
            // Minimal blend shapes (just lip sync)
            enableFacialBlendShapes = false;
            // Minimal gestures
            if (gestureSystem != null)
            {
                gestureSystem.SetGestureFrequency(0.25f);
            }
            // Low quality materials
            SetMaterialQuality(MaterialQuality.Low);
            break;
    }
}
```

## Common Issues

### Known Issues and Solutions

1. **Lip Sync Delays**:
   - **Symptoms**: Audio playback and lip movements are out of sync
   - **Solution**: Adjust lip sync buffer size and smoothing parameters

2. **Expression Blending Issues**:
   - **Symptoms**: Unnatural facial transitions, expression flicker
   - **Solution**: Increase blend duration, ensure expression definitions don't conflict

3. **Gesture Conflicts**:
   - **Symptoms**: Avatar performs jerky or interrupted gestures
   - **Solution**: Implement better gesture prioritization and cancellation

4. **Performance Impacts**:
   - **Symptoms**: Frame rate drops during complex animations
   - **Solution**: Implement more aggressive LOD for animations and blend shapes

5. **Uncanny Valley Effects**:
   - **Symptoms**: Avatar feels unnatural or disturbing
   - **Solution**: Reduce micro-movements, ensure expressions are cohesive

## Usage Examples

### Setting Up Avatar States

```csharp
// Initialize the avatar controller with necessary components
private void InitializeAvatar()
{
    // Find required components if not assigned
    if (animator == null)
    {
        animator = GetComponent<Animator>();
    }
    
    if (lipSync == null)
    {
        lipSync = GetComponent<LipSync>();
    }
    
    if (facialExpressions == null)
    {
        facialExpressions = GetComponent<FacialExpressions>();
    }
    
    if (gestureSystem == null)
    {
        gestureSystem = GetComponent<GestureSystem>();
    }
    
    // Subscribe to audio events
    AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
    if (audioPlayback != null)
    {
        audioPlayback.OnPlaybackStarted += OnAudioPlaybackStarted;
        audioPlayback.OnPlaybackCompleted += OnAudioPlaybackCompleted;
        audioPlayback.OnPlaybackProgress += UpdateLipSync;
    }
    
    // Set initial state
    SetIdleState();
}
```

### Coordinating with Audio Playback

```csharp
// In AudioPlayback.cs
public void PlayAudioResponse(byte[] audioData)
{
    try
    {
        // Create audio clip from WAV data
        AudioClip clip = CreateAudioClipFromWAV(audioData);
        
        if (clip != null)
        {
            // Configure audio source
            _audioSource.clip = clip;
            _audioSource.Play();
            
            _isPlaying = true;
            _playbackPosition = 0f;
            
            // Notify avatar to start speaking animation
            OnPlaybackStarted?.Invoke();
            
            // Begin monitoring progress
            StartCoroutine(MonitorPlaybackProgress());
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error playing audio response: {ex.Message}");
    }
}

// In AvatarController.cs
public void OnAudioPlaybackStarted()
{
    // Ensure speaking state and lip sync
    if (_currentState != "RESPONDING")
    {
        SetSpeakingState();
    }
    
    if (lipSync != null)
    {
        lipSync.StartLipSync();
    }
}

public void OnAudioPlaybackCompleted()
{
    // Return to attentive state after speaking
    if (_currentState == "RESPONDING")
    {
        SetAttentiveState();
    }
    
    if (lipSync != null)
    {
        lipSync.StopLipSync();
    }
}
```

## Conclusion

The avatar subsystem is a critical component of the VR Interview System, providing a visual representation of the interviewer that can respond naturally to the conversation flow. Its modular design allows for easy customization and extension, while the various optimizations ensure good performance in VR environments.

The avatar system achieves realism through coordinated animation systems, including body animations, facial expressions, lip synchronization, and gesture systems. These visual cues complement the audio responses to create an engaging and natural conversation experience for the user.

By following the patterns described in this documentation, developers can extend the avatar capabilities, customize its appearance, and integrate it with new interview scenarios and interactions.
