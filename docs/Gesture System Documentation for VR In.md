# Gesture System Documentation for VR Interview System

## Overview

The Gesture System provides natural hand and body gestures for the interviewer avatar during conversations. Gestures help convey emotions, emphasize points, and create a more realistic and engaging interview experience. This document describes how the gesture system is implemented, how to extend it, and best practices for creating effective avatar gestures.

## Implementation Structure

The gesture system is implemented through multiple components:

1. **Animation Layer Architecture**:
   - Main avatar animator controller has a dedicated "Gestures" layer
   - Gestures layer runs in parallel with the base animation layer
   - Gestures can be triggered independently of the avatar's base state

2. **GestureSystem Component**:
   - Manages gesture selection and timing
   - Triggers gesture animations through the Animator
   - Provides both random and contextual gestures

3. **Animator Controller Configuration**:
   - Separate state machine for gestures with transitions from any state
   - Auto-exit transitions after gestures complete
   - Parameters for triggering specific gestures

## Animator Controller Setup

The `InterviewerAnimator` controller includes a dedicated Gestures layer:

```
InterviewerAnimator
├── Base Layer (Idle, Listening, Thinking, Speaking, etc.)
├── Gestures Layer
│   ├── Empty (default state)
│   ├── HandGesture1
│   ├── HandGesture2
│   └── HandGesture3
└── Facial Layer
```

### Gesture Animation States

The gesture layer contains:

1. **Empty**: Default state with no gestures
2. **HandGesture1, HandGesture2, HandGesture3**: Specific gesture animations
3. **Transitions**: From any state to gesture states via trigger parameters
4. **Exit Transitions**: Auto-exit back to Empty after gesture completion

### Animation Parameters

The following animator parameters control gestures:

- `Gesture1`: Trigger for first gesture animation
- `Gesture2`: Trigger for second gesture animation
- `Gesture3`: Trigger for third gesture animation

## GestureSystem Component

The GestureSystem script controls when and which gestures are played:

```csharp
public class GestureSystem : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AvatarController avatarController;
    
    [Header("Gesture Settings")]
    [SerializeField] private float randomGestureInterval = 8f;
    [SerializeField] private float contextualGestureProbability = 0.3f;
    [SerializeField] private bool enableRandomGestures = true;
    
    private string _currentAvatarState = "IDLE";
    private Coroutine _randomGestureCoroutine;
    
    private void Start()
    {
        // Listen for avatar state changes
        if (avatarController != null)
        {
            avatarController.OnStateChanged += HandleAvatarStateChanged;
        }
        
        // Start random gesture coroutine
        if (enableRandomGestures)
        {
            _randomGestureCoroutine = StartCoroutine(PerformRandomGestures());
        }
    }
    
    // Perform a specific gesture by index (1-3)
    public void PerformGesture(int gestureIndex)
    {
        if (animator == null) return;
        
        // Don't perform gestures during certain states
        if (_currentAvatarState == "LISTENING" || _currentAvatarState == "ERROR")
        {
            return;
        }
        
        // Trigger the appropriate gesture animation
        switch (gestureIndex)
        {
            case 1:
                animator.SetTrigger("Gesture1");
                break;
            case 2:
                animator.SetTrigger("Gesture2");
                break;
            case 3:
                animator.SetTrigger("Gesture3");
                break;
        }
    }
    
    // Perform a random gesture
    public void PerformRandomGesture()
    {
        int randomGesture = UnityEngine.Random.Range(1, 4);
        PerformGesture(randomGesture);
    }
    
    // Coroutine for performing random gestures
    private IEnumerator PerformRandomGestures()
    {
        while (true)
        {
            // Wait for random interval
            float waitTime = UnityEngine.Random.Range(randomGestureInterval * 0.8f, 
                                                    randomGestureInterval * 1.2f);
            yield return new WaitForSeconds(waitTime);
            
            // Only perform random gestures during speaking or waiting states
            if (_currentAvatarState == "RESPONDING" || _currentAvatarState == "WAITING")
            {
                float rand = UnityEngine.Random.value;
                if (rand < 0.7f) // 70% chance to perform a random gesture
                {
                    PerformRandomGesture();
                }
            }
        }
    }
    
    // Handle avatar state changes to adjust gesture behavior
    private void HandleAvatarStateChanged(string newState)
    {
        _currentAvatarState = newState;
    }
}
```

## How Gestures Are Triggered

Gestures can be triggered in three ways:

1. **Random Gestures**:
   - Occur at random intervals during speaking and waiting states
   - Controlled by randomGestureInterval parameter
   - Disabled during listening and error states

2. **Contextual Gestures**:
   - Triggered based on the content of the response
   - Mapped to specific keywords or phrases
   - Called by the MessageHandler when detecting emphasis points

3. **Programmatic Gestures**:
   - Directly called via the PerformGesture method
   - Used for scripted interactions or specific responses
   - Can be integrated with other systems (e.g., emotion recognition)

## Adding New Gestures

To add a new gesture to the system:

1. **Create the Animation**:
   - Create a new animation clip for the gesture
   - Focus on upper body movement (arms, hands, shoulders)
   - Keep duration between 1-3 seconds
   - Add exit time for smooth transitions

2. **Update the Animator Controller**:
   - Add a new state to the Gestures layer
   - Set up a transition from Any State to the new gesture
   - Create a new trigger parameter (e.g., "Gesture4")
   - Configure an auto-exit transition back to Empty

3. **Update GestureSystem Script**:
   - Add handling for the new gesture in the PerformGesture method
   - Update the random gesture range if using random gestures
   - Add any contextual trigger logic if needed

Example for extending the gesture system:

```csharp
// Add to PerformGesture method
case 4:
    animator.SetTrigger("Gesture4");
    break;

// Update random gesture selection
int randomGesture = UnityEngine.Random.Range(1, 5); // Now includes 4 gestures
```

## Guidelines for Creating Effective Gestures

When creating gesture animations:

1. **Natural Movement**:
   - Base gestures on real human movements
   - Use reference videos or motion capture
   - Avoid robotic or exaggerated movements unless stylized

2. **Conversation Appropriate**:
   - Create gestures that would naturally occur in an interview
   - Include gestures like nodding, hand emphasis, thoughtful poses
   - Avoid distracting or overly dramatic gestures

3. **Technical Considerations**:
   - Keep animations short (1-3 seconds)
   - Use animation curves for natural easing
   - Add holding frames at the beginning/end for smoother transitions
   - Focus animation on key joints (shoulders, elbows, wrists, fingers)

4. **Context Appropriateness**:
   - Create different categories of gestures:
     - Neutral/Engaging: For general conversation
     - Emphasis: For important points
     - Thoughtful: For processing/thinking states
     - Excited/Positive: For encouraging moments

## Optimizing Gesture Performance

For optimal performance:

1. **Gesture Frequency Control**:
   - Adjust randomGestureInterval based on conversation flow
   - Reduce frequency during intensive processing
   - Balance between natural movement and distraction

2. **Animation Optimization**:
   - Use animation compression settings appropriate for VR
   - Animate only necessary bones
   - Consider using simplified gestures for distant viewing

3. **Conditional Execution**:
   - Only trigger gestures when the avatar is visible
   - Skip gestures during high-CPU operations
   - Disable random gestures during certain activities

## Troubleshooting

Common gesture system issues and solutions:

1. **Gestures Not Playing**:
   - Check animator parameter names match exactly
   - Verify layer weights are set correctly (should be 1)
   - Ensure transitions have proper conditions

2. **Animation Blending Issues**:
   - Adjust transition durations for smoother blending
   - Check for conflicting animation masks
   - Verify avatar configuration is correct

3. **Timing Problems**:
   - Adjust gesture frequency and duration
   - Check for coroutine issues if gestures stop working
   - Verify state transitions are working properly

## Future Enhancements

Planned improvements for the gesture system:

1. **Response Content Analysis**:
   - Deeper integration with response content
   - Automatic emphasis detection
   - Emotional context recognition

2. **Enhanced Gesture Variety**:
   - Expanded gesture library with categories
   - Personality-based gesture selection
   - Cultural adaptation of gestures

3. **User Feedback Integration**:
   - Learning from user engagement
   - Adapting gesture frequency to user preference
   - A/B testing of gesture effectiveness

## Conclusion

The gesture system is a critical component for creating a realistic, engaging interviewer avatar. By following these guidelines and understanding the implementation, you can enhance the system with new gestures and improve the overall interview experience.