using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles avatar gestures and animations.
/// </summary>
public class GestureSystem : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    [Header("Gesture Settings")]
    [SerializeField] private string[] gestureAnimationTriggers; // Array of animation trigger names
    [SerializeField] private float gestureCooldown = 2.0f;
    [SerializeField] private float gestureChance = 0.7f;
    [SerializeField] private bool useRandomGestures = true;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private bool _isGestureActive = false;
    private float _lastGestureTime = 0f;
    private int _lastGestureIndex = -1;
    private Dictionary<string, float> _gestureWeights = new Dictionary<string, float>();
    
    private void Start()
    {
        InitializeComponents();
        InitializeGestureWeights();
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
                Debug.LogError("Animator not found for GestureSystem!");
            }
        }
        
        // Set up default gesture triggers if empty
        if (gestureAnimationTriggers == null || gestureAnimationTriggers.Length == 0)
        {
            gestureAnimationTriggers = new string[]
            {
                "GestureNod",
                "GestureShakeHead",
                "GestureHandsUp",
                "GesturePointRight",
                "GesturePointLeft",
                "GestureThinking",
                "GestureWave"
            };
            
            Debug.Log("Using default gesture animation triggers");
        }
    }
    
    /// <summary>
    /// Initializes weights for each gesture.
    /// </summary>
    private void InitializeGestureWeights()
    {
        // Set initial weights
        foreach (string gesture in gestureAnimationTriggers)
        {
            _gestureWeights[gesture] = 1.0f;
        }
    }
    
    /// <summary>
    /// Performs a random gesture based on weights.
    /// </summary>
    public void PerformRandomGesture()
    {
        if (!useRandomGestures || animator == null || 
            gestureAnimationTriggers.Length == 0 || _isGestureActive)
        {
            return;
        }
        
        // Check cooldown
        if (Time.time - _lastGestureTime < gestureCooldown)
        {
            return;
        }
        
        // Random chance to perform gesture
        if (UnityEngine.Random.value > gestureChance)
        {
            return;
        }
        
        // Select a gesture based on weights
        string selectedGesture = SelectWeightedGesture();
        
        // Perform the gesture
        PerformGesture(selectedGesture);
    }
    
    /// <summary>
    /// Performs a specific gesture.
    /// </summary>
    /// <param name="gestureName">The gesture trigger name.</param>
    public void PerformGesture(string gestureName)
    {
        if (animator == null || string.IsNullOrEmpty(gestureName))
        {
            return;
        }
        
        try
        {
            // Trigger animation
            animator.SetTrigger(gestureName);
            
            // Update state
            _isGestureActive = true;
            _lastGestureTime = Time.time;
            
            // Find index for last gesture
            for (int i = 0; i < gestureAnimationTriggers.Length; i++)
            {
                if (gestureAnimationTriggers[i] == gestureName)
                {
                    _lastGestureIndex = i;
                    break;
                }
            }
            
            // Start cooldown coroutine
            StartCoroutine(GestureCooldownCoroutine());
            
            if (debugMode)
            {
                Debug.Log($"Performing gesture: {gestureName}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error performing gesture: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Performs a gesture by index.
    /// </summary>
    /// <param name="gestureIndex">The index of the gesture in the triggers array.</param>
    public void PerformGestureByIndex(int gestureIndex)
    {
        if (gestureIndex < 0 || gestureIndex >= gestureAnimationTriggers.Length)
        {
            Debug.LogError($"Invalid gesture index: {gestureIndex}");
            return;
        }
        
        PerformGesture(gestureAnimationTriggers[gestureIndex]);
    }
    
    /// <summary>
    /// Selects a gesture based on weights.
    /// </summary>
    /// <returns>The selected gesture trigger name.</returns>
    private string SelectWeightedGesture()
    {
        // Calculate total weight
        float totalWeight = 0f;
        foreach (var weight in _gestureWeights.Values)
        {
            totalWeight += weight;
        }
        
        // Select based on weight
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        
        foreach (var gesture in gestureAnimationTriggers)
        {
            cumulativeWeight += _gestureWeights[gesture];
            
            if (randomValue <= cumulativeWeight)
            {
                return gesture;
            }
        }
        
        // Fallback to random selection
        return gestureAnimationTriggers[UnityEngine.Random.Range(0, gestureAnimationTriggers.Length)];
    }
    
    /// <summary>
    /// Coroutine for gesture cooldown.
    /// </summary>
    private IEnumerator GestureCooldownCoroutine()
    {
        // Wait for estimated gesture duration
        yield return new WaitForSeconds(gestureCooldown * 0.5f);
        
        _isGestureActive = false;
    }
    
    /// <summary>
    /// Updates the weight for a specific gesture.
    /// </summary>
    /// <param name="gestureName">The gesture name.</param>
    /// <param name="weight">The new weight.</param>
    public void SetGestureWeight(string gestureName, float weight)
    {
        if (_gestureWeights.ContainsKey(gestureName))
        {
            _gestureWeights[gestureName] = Mathf.Max(0.1f, weight);
        }
    }
    
    /// <summary>
    /// Gets all available gesture names.
    /// </summary>
    /// <returns>Array of gesture names.</returns>
    public string[] GetAvailableGestures()
    {
        return gestureAnimationTriggers;
    }
    
    /// <summary>
    /// Gets the last performed gesture index.
    /// </summary>
    /// <returns>The last gesture index or -1 if none.</returns>
    public int GetLastGestureIndex()
    {
        return _lastGestureIndex;
    }
    
    /// <summary>
    /// Sets whether random gestures are enabled.
    /// </summary>
    /// <param name="enabled">Whether random gestures are enabled.</param>
    public void SetRandomGesturesEnabled(bool enabled)
    {
        useRandomGestures = enabled;
    }
    
    /// <summary>
    /// Sets the animator for gestures.
    /// </summary>
    /// <param name="newAnimator">The animator to use.</param>
    public void SetAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }
}