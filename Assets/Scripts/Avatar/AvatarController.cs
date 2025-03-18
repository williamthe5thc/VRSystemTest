using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Main controller for the interviewer avatar.
/// Manages animations, lip sync, and expressions.
/// </summary>
public class AvatarController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private LipSync lipSync;
    [SerializeField] private FacialExpressions facialExpressions;
    [SerializeField] private GestureSystem gestureSystem;
    
    [Header("Animation Parameters")]
    [SerializeField] private string idleTrigger = "Idle";
    [SerializeField] private string listeningTrigger = "Listening";
    [SerializeField] private string thinkingTrigger = "Thinking";
    [SerializeField] private string speakingTrigger = "Speaking";
    [SerializeField] private string attentiveTrigger = "Attentive";
    [SerializeField] private string confusedTrigger = "Confused";
    
    [Header("Animation Settings")]
    [SerializeField] private float blendDuration = 0.5f;
    [SerializeField] private float randomGestureInterval = 8f;
    [SerializeField] private float randomBlinkInterval = 4f;
    [SerializeField] private float animationSpeed = 1.0f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private string _currentState = "IDLE";
    private Coroutine _randomGestureCoroutine;
    private Coroutine _randomBlinkCoroutine;
    
    // Events
    public event Action<string> OnStateChanged;
    
    private void Start()
    {
        // Check required components
        if (animator == null)
        {
            Debug.LogError("Animator not assigned to AvatarController!");
            return;
        }
        
        InitializeComponents();
        
        // Start random behaviors
        _randomGestureCoroutine = StartCoroutine(PerformRandomGestures());
        _randomBlinkCoroutine = StartCoroutine(PerformRandomBlinks());
        
        // Start in idle state
        SetIdleState();
    }
    
    /// <summary>
    /// Initializes required components if not assigned.
    /// </summary>
    private void InitializeComponents()
    {
        // Find animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            
            if (animator == null)
            {
                Debug.LogError("Animator component not found on avatar!");
            }
        }
        
        // Find or create lip sync if not assigned
        if (lipSync == null)
        {
            lipSync = GetComponent<LipSync>();
            
            if (lipSync == null && GetComponentInChildren<SkinnedMeshRenderer>() != null)
            {
                lipSync = gameObject.AddComponent<LipSync>();
                Debug.Log("Added LipSync component to avatar.");
            }
        }
        
        // Find or create facial expressions if not assigned
        if (facialExpressions == null)
        {
            facialExpressions = GetComponent<FacialExpressions>();
            
            if (facialExpressions == null && GetComponentInChildren<SkinnedMeshRenderer>() != null)
            {
                facialExpressions = gameObject.AddComponent<FacialExpressions>();
                Debug.Log("Added FacialExpressions component to avatar.");
            }
        }
        
        // Find or create gesture system if not assigned
        if (gestureSystem == null)
        {
            gestureSystem = GetComponent<GestureSystem>();
            
            if (gestureSystem == null)
            {
                gestureSystem = gameObject.AddComponent<GestureSystem>();
                Debug.Log("Added GestureSystem component to avatar.");
            }
        }
        
        // Set animation speed
        if (animator != null)
        {
            animator.speed = animationSpeed;
        }
    }
    
    #region State Control Methods
    
    /// <summary>
    /// Sets the avatar to idle state.
    /// </summary>
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
        
        if (debugMode)
        {
            Debug.Log("Avatar state: IDLE");
        }
    }
    
    /// <summary>
    /// Sets the avatar to listening state.
    /// </summary>
    public void SetListeningState()
    {
        if (_currentState == "LISTENING") return;
        
        _currentState = "LISTENING";
        
        if (animator != null)
        {
            animator.SetTrigger(listeningTrigger);
        }
        
        if (facialExpressions != null)
        {
            facialExpressions.SetExpression(FacialExpression.Interested);
        }
        
        if (lipSync != null)
        {
            lipSync.StopLipSync();
        }
        
        OnStateChanged?.Invoke(_currentState);
        
        if (debugMode)
        {
            Debug.Log("Avatar state: LISTENING");
        }
    }
    
    /// <summary>
    /// Sets the avatar to thinking state.
    /// </summary>
    public void SetThinkingState()
    {
        if (_currentState == "PROCESSING") return;
        
        _currentState = "PROCESSING";
        
        if (animator != null)
        {
            animator.SetTrigger(thinkingTrigger);
        }
        
        if (facialExpressions != null)
        {
            facialExpressions.SetExpression(FacialExpression.Thoughtful);
        }
        
        if (lipSync != null)
        {
            lipSync.StopLipSync();
        }
        
        OnStateChanged?.Invoke(_currentState);
        
        if (debugMode)
        {
            Debug.Log("Avatar state: PROCESSING");
        }
    }
    
    /// <summary>
    /// Sets the avatar to speaking state.
    /// </summary>
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
        
        if (debugMode)
        {
            Debug.Log("Avatar state: RESPONDING");
        }
    }
    
    /// <summary>
    /// Sets the avatar to attentive state.
    /// </summary>
    public void SetAttentiveState()
    {
        if (_currentState == "WAITING") return;
        
        _currentState = "WAITING";
        
        if (animator != null)
        {
            animator.SetTrigger(attentiveTrigger);
        }
        
        if (facialExpressions != null)
        {
            facialExpressions.SetExpression(FacialExpression.Attentive);
        }
        
        if (lipSync != null)
        {
            lipSync.StopLipSync();
        }
        
        OnStateChanged?.Invoke(_currentState);
        
        if (debugMode)
        {
            Debug.Log("Avatar state: WAITING");
        }
    }
    
    /// <summary>
    /// Sets the avatar to confused state.
    /// </summary>
    public void SetConfusedState()
    {
        if (_currentState == "ERROR") return;
        
        _currentState = "ERROR";
        
        if (animator != null)
        {
            animator.SetTrigger(confusedTrigger);
        }
        
        if (facialExpressions != null)
        {
            facialExpressions.SetExpression(FacialExpression.Confused);
        }
        
        if (lipSync != null)
        {
            lipSync.StopLipSync();
        }
        
        OnStateChanged?.Invoke(_currentState);
        
        if (debugMode)
        {
            Debug.Log("Avatar state: ERROR");
        }
    }
    
    #endregion
    
    #region Audio Event Handlers
    
    /// <summary>
    /// Called when audio playback starts.
    /// </summary>
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
    
    /// <summary>
    /// Called when audio playback completes.
    /// </summary>
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
    
    /// <summary>
    /// Updates lip sync during audio playback.
    /// </summary>
    /// <param name="normalizedTime">Normalized playback time (0-1).</param>
    public void UpdateLipSync(float normalizedTime)
    {
        if (lipSync != null)
        {
            lipSync.UpdateLipSyncValue(normalizedTime);
        }
    }
    
    #endregion
    
    #region Random Behaviors
    
    /// <summary>
    /// Coroutine for random gesture animations.
    /// </summary>
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
                    
                    if (debugMode)
                    {
                        Debug.Log("Performed random gesture");
                    }
                }
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }
    
    /// <summary>
    /// Coroutine for random blinking animations.
    /// </summary>
    private IEnumerator PerformRandomBlinks()
    {
        while (true)
        {
            float waitTime = UnityEngine.Random.Range(randomBlinkInterval * 0.5f, randomBlinkInterval * 1.5f);
            yield return new WaitForSeconds(waitTime);
            
            // Blink regardless of state
            if (facialExpressions != null)
            {
                facialExpressions.Blink();
                
                if (debugMode)
                {
                    Debug.Log("Performed blink");
                }
            }
        }
    }
    
    #endregion
    
    /// <summary>
    /// Sets the animation speed of the avatar.
    /// </summary>
    /// <param name="speed">The animation speed multiplier.</param>
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = Mathf.Clamp(speed, 0.5f, 2.0f);
        
        if (animator != null)
        {
            animator.speed = animationSpeed;
        }
    }
    
    /// <summary>
    /// Gets the current state of the avatar.
    /// </summary>
    /// <returns>The current avatar state.</returns>
    public string GetCurrentState()
    {
        return _currentState;
    }
}