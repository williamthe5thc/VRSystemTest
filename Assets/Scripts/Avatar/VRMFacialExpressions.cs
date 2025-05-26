using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;

/// <summary>
/// Manages facial expressions specifically for VRM avatars
/// </summary>
public class VRMFacialExpressions : MonoBehaviour
{
    [SerializeField] private VRMBlendShapeProxy blendShapeProxy;
    
    // Property accessor for external access
    public VRMBlendShapeProxy BlendShapeProxy {
        get { return blendShapeProxy; }
        set { blendShapeProxy = value; }
    }
    
    [Header("Expression Settings")]
    [SerializeField] private float expressionTransitionDuration = 0.5f;
    [SerializeField] private float expressionIntensity = 1.0f;
    [SerializeField] private float blinkDuration = 0.15f;
    [SerializeField] private float randomBlinkInterval = 4f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private FacialExpression _currentExpression = FacialExpression.Neutral;
    private Coroutine _expressionCoroutine;
    private Coroutine _blinkCoroutine;
    private Coroutine _randomBlinkCoroutine;
    
    // Map our custom expression types to VRM presets
    private Dictionary<FacialExpression, BlendShapePreset> expressionMap;
    
    private void Awake()
    {
        InitializeExpressionMap();
    }
    
    private void Start()
    {
        // Find VRMBlendShapeProxy if not assigned
        if (blendShapeProxy == null)
        {
            blendShapeProxy = GetComponent<VRMBlendShapeProxy>();
            
            if (blendShapeProxy == null)
            {
                blendShapeProxy = GetComponentInChildren<VRMBlendShapeProxy>();
                
                if (blendShapeProxy == null)
                {
                    Debug.LogError("VRMBlendShapeProxy not found. VRM facial expressions will not work.");
                    return;
                }
            }
        }
        
        // Start with neutral expression
        SetExpression(FacialExpression.Neutral);
        
        // Start random blinking
        StartRandomBlinking();
    }
    
    private void InitializeExpressionMap()
    {
        // Initialize mapping between our custom expressions and VRM blend shape presets
        expressionMap = new Dictionary<FacialExpression, BlendShapePreset>
        {
            { FacialExpression.Neutral, BlendShapePreset.Neutral },
            { FacialExpression.Happy, BlendShapePreset.Joy },
            { FacialExpression.Sad, BlendShapePreset.Sorrow },
            { FacialExpression.Angry, BlendShapePreset.Angry },
            { FacialExpression.Surprised, BlendShapePreset.Fun }, // Changed from Surprised to Fun
            { FacialExpression.Confused, BlendShapePreset.Sorrow }, // Reuse sorrow for confused
            { FacialExpression.Thoughtful, BlendShapePreset.Sorrow }, // Reuse with lower intensity
            { FacialExpression.Interested, BlendShapePreset.Joy }, // Reuse joy with lower intensity
            { FacialExpression.Attentive, BlendShapePreset.Neutral }, // Reuse neutral with slight modifications
            { FacialExpression.Talking, BlendShapePreset.Neutral } // Talking is handled by lip sync
        };
    }
    
    /// <summary>
    /// Sets a facial expression with proper VRM blend shape mapping
    /// </summary>
    public void SetExpression(FacialExpression expression)
    {
        if (blendShapeProxy == null) return;
        
        // Stop any ongoing expression transition
        if (_expressionCoroutine != null)
        {
            StopCoroutine(_expressionCoroutine);
        }
        
        // Start new transition
        _expressionCoroutine = StartCoroutine(TransitionToExpressionCoroutine(expression));
        
        if (debugMode)
        {
            Debug.Log($"Setting VRM expression: {expression}");
        }
    }
    
    /// <summary>
    /// Triggers a blink animation
    /// </summary>
    public void Blink()
    {
        if (blendShapeProxy == null) return;
        
        // Stop any ongoing blink
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
        }
        
        // Start new blink
        _blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }
    
    /// <summary>
    /// Starts random blinking
    /// </summary>
    public void StartRandomBlinking()
    {
        // Stop any ongoing random blinking
        if (_randomBlinkCoroutine != null)
        {
            StopCoroutine(_randomBlinkCoroutine);
        }
        
        // Start new random blinking
        _randomBlinkCoroutine = StartCoroutine(RandomBlinkCoroutine());
    }
    
    /// <summary>
    /// Stops random blinking
    /// </summary>
    public void StopRandomBlinking()
    {
        if (_randomBlinkCoroutine != null)
        {
            StopCoroutine(_randomBlinkCoroutine);
            _randomBlinkCoroutine = null;
        }
    }
    
    private IEnumerator TransitionToExpressionCoroutine(FacialExpression targetExpression)
    {
        if (blendShapeProxy == null) yield break;
        
        FacialExpression previousExpression = _currentExpression;
        _currentExpression = targetExpression;
        
        // Get the VRM blend shape presets for both expressions
        BlendShapePreset previousPreset = expressionMap.ContainsKey(previousExpression) ? 
            expressionMap[previousExpression] : BlendShapePreset.Neutral;
            
        BlendShapePreset targetPreset = expressionMap.ContainsKey(targetExpression) ? 
            expressionMap[targetExpression] : BlendShapePreset.Neutral;
        
        // Skip transition if the presets are the same
        if (previousPreset == targetPreset && previousPreset != BlendShapePreset.Neutral)
        {
            // Just apply the expression intensity based on the custom expression
            float intensity = GetExpressionIntensity(targetExpression);
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(targetPreset), intensity);
            blendShapeProxy.Apply();
            yield break;
        }
        
        // Get the target intensity
        float targetIntensity = GetExpressionIntensity(targetExpression);
        
        // Get the current value for the previous expression
        float previousValue = blendShapeProxy.GetValue(BlendShapeKey.CreateFromPreset(previousPreset));
        
        // Transition time
        float elapsed = 0f;
        
        // Fade out previous expression
        while (elapsed < expressionTransitionDuration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (expressionTransitionDuration / 2);
            
            // Fade out previous expression if it's not neutral
            if (previousPreset != BlendShapePreset.Neutral)
            {
                float value = Mathf.Lerp(previousValue, 0f, t);
                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(previousPreset), value);
            }
            
            blendShapeProxy.Apply();
            yield return null;
        }
        
        // Reset previous expression completely
        if (previousPreset != BlendShapePreset.Neutral)
        {
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(previousPreset), 0f);
        }
        
        // Fade in new expression
        elapsed = 0f;
        while (elapsed < expressionTransitionDuration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (expressionTransitionDuration / 2);
            
            // Fade in new expression if it's not neutral
            if (targetPreset != BlendShapePreset.Neutral)
            {
                float value = Mathf.Lerp(0f, targetIntensity, t);
                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(targetPreset), value);
            }
            
            blendShapeProxy.Apply();
            yield return null;
        }
        
        // Set final value
        if (targetPreset != BlendShapePreset.Neutral)
        {
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(targetPreset), targetIntensity);
        }
        
        blendShapeProxy.Apply();
    }
    
    private float GetExpressionIntensity(FacialExpression expression)
    {
        // Return different intensities based on the expression type
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
    
    private IEnumerator BlinkCoroutine()
    {
        if (blendShapeProxy == null) yield break;
        
        // Store current values of expressions that might be affected by blink
        var expressionKeys = new Dictionary<BlendShapePreset, float>();
        foreach (BlendShapePreset preset in System.Enum.GetValues(typeof(BlendShapePreset)))
        {
            var key = BlendShapeKey.CreateFromPreset(preset);
            expressionKeys[preset] = blendShapeProxy.GetValue(key);
        }
        
        // Create blink key
        var blinkKey = BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink);
        
        // Blink sequence: open -> closed -> open
        float elapsed = 0f;
        
        // Close eyes
        while (elapsed < blinkDuration * 0.4f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (blinkDuration * 0.4f);
            float value = Mathf.Lerp(0f, 1f, t);
            
            blendShapeProxy.ImmediatelySetValue(blinkKey, value);
            blendShapeProxy.Apply();
            
            yield return null;
        }
        
        // Hold closed briefly
        blendShapeProxy.ImmediatelySetValue(blinkKey, 1f);
        blendShapeProxy.Apply();
        yield return new WaitForSeconds(blinkDuration * 0.2f);
        
        // Open eyes
        elapsed = 0f;
        while (elapsed < blinkDuration * 0.4f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (blinkDuration * 0.4f);
            float value = Mathf.Lerp(1f, 0f, t);
            
            blendShapeProxy.ImmediatelySetValue(blinkKey, value);
            blendShapeProxy.Apply();
            
            yield return null;
        }
        
        // Ensure eyes are fully open
        blendShapeProxy.ImmediatelySetValue(blinkKey, 0f);
        blendShapeProxy.Apply();
    }
    
    private IEnumerator RandomBlinkCoroutine()
    {
        while (true)
        {
            // Random interval between blinks
            float waitTime = UnityEngine.Random.Range(randomBlinkInterval * 0.7f, randomBlinkInterval * 1.3f);
            yield return new WaitForSeconds(waitTime);
            
            // Don't blink during certain expressions
            if (_currentExpression != FacialExpression.Surprised && 
                _currentExpression != FacialExpression.Angry)
            {
                Blink();
            }
        }
    }
}