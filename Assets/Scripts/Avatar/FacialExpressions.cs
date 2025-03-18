using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles facial expressions for the avatar.
/// </summary>
public class FacialExpressions : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    
    [Header("Blend Shape Indices")]
    [SerializeField] private int blinkBlendShapeIndex = 0;
    [SerializeField] private int jawOpenBlendShapeIndex = 1;
    [SerializeField] private int smileBlendShapeIndex = 2;
    [SerializeField] private int frownBlendShapeIndex = 3;
    
    [Header("Expression Settings")]
    [SerializeField] private float expressionTransitionDuration = 0.5f;
    [SerializeField] private float randomBlinkInterval = 4f;
    
    [Header("Audio Visualization")]
    [SerializeField] private float minMouthOpenAmount = 10f;
    [SerializeField] private float maxMouthOpenAmount = 40f;
    [SerializeField] private float minMouthMoveDuration = 0.1f;
    [SerializeField] private float maxMouthMoveDuration = 0.3f;
    
    private FacialExpression _currentExpression = FacialExpression.Neutral;
    private Coroutine _blinkCoroutine;
    private Coroutine _randomBlinkCoroutine;
    private Coroutine _expressionCoroutine;
    private Coroutine _mouthCoroutine;
    private bool _isAnimatingMouth = false;
    
    private void Start()
    {
        // Start neutral expression
        SetExpression(FacialExpression.Neutral);
        
        // Start random blinking
        StartRandomBlinking();
    }
    
    /// <summary>
    /// Sets a facial expression.
    /// </summary>
    /// <param name="expression">The facial expression to set.</param>
    public void SetExpression(FacialExpression expression)
    {
        if (faceRenderer == null)
        {
            Debug.LogWarning("FacialExpressions: No SkinnedMeshRenderer assigned!");
            return;
        }
        
        // Stop any ongoing expression transition
        if (_expressionCoroutine != null)
        {
            StopCoroutine(_expressionCoroutine);
        }
        
        // Start new transition
        _expressionCoroutine = StartCoroutine(TransitionToExpressionCoroutine(expression, expressionTransitionDuration));
    }
    
    /// <summary>
    /// Triggers a blink animation.
    /// </summary>
    public void Blink()
    {
        if (faceRenderer == null) return;
        
        // Stop any ongoing blink
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
        }
        
        // Start new blink
        _blinkCoroutine = StartCoroutine(BlinkCoroutine(0.2f));
    }
    
    /// <summary>
    /// Starts random blinking.
    /// </summary>
    public void StartRandomBlinking()
    {
        if (faceRenderer == null) return;
        
        // Stop any ongoing random blinking
        if (_randomBlinkCoroutine != null)
        {
            StopCoroutine(_randomBlinkCoroutine);
        }
        
        // Start new random blinking
        _randomBlinkCoroutine = StartCoroutine(RandomBlinkCoroutine());
    }
    
    /// <summary>
    /// Stops random blinking.
    /// </summary>
    public void StopRandomBlinking()
    {
        if (_randomBlinkCoroutine != null)
        {
            StopCoroutine(_randomBlinkCoroutine);
            _randomBlinkCoroutine = null;
        }
    }
    
    /// <summary>
    /// Animates mouth movement for speaking visualization.
    /// </summary>
    public void AnimateMouthForSpeaking()
    {
        if (faceRenderer == null || _isAnimatingMouth) return;
        
        _isAnimatingMouth = true;
        StartCoroutine(MicrophoneVisualizationCoroutine());
    }
    
    /// <summary>
    /// Coroutine for random blinking.
    /// </summary>
    private IEnumerator RandomBlinkCoroutine()
    {
        while (true)
        {
            // Wait for random interval
            float waitTime = UnityEngine.Random.Range(randomBlinkInterval * 0.5f, randomBlinkInterval * 1.5f);
            yield return new WaitForSeconds(waitTime);
            
            // Blink
            Blink();
        }
    }
    
    /// <summary>
    /// Coroutine for blink animation.
    /// </summary>
    /// <param name="duration">Duration of the blink.</param>
    private IEnumerator BlinkCoroutine(float duration)
    {
        float elapsed = 0f;
        float startWeight = GetBlendShapeWeight(blinkBlendShapeIndex);
        
        // Open to close
        while (elapsed < duration * 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.3f);
            float weight = Mathf.Lerp(startWeight, 100f, t);
            SetBlendShapeWeight(blinkBlendShapeIndex, weight);
            yield return null;
        }
        
        // Hold closed
        yield return new WaitForSeconds(duration * 0.1f);
        
        // Close to open
        elapsed = 0f;
        while (elapsed < duration * 0.6f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.6f);
            float weight = Mathf.Lerp(100f, startWeight, t);
            SetBlendShapeWeight(blinkBlendShapeIndex, weight);
            yield return null;
        }
        
        // Ensure blend shape is back to original value
        SetBlendShapeWeight(blinkBlendShapeIndex, startWeight);
    }
    
    /// <summary>
    /// Coroutine for transitioning between expressions.
    /// </summary>
    /// <param name="targetExpression">The target expression.</param>
    /// <param name="transitionDuration">Duration of the transition.</param>
    private IEnumerator TransitionToExpressionCoroutine(FacialExpression targetExpression, float transitionDuration)
    {
        float elapsed = 0f;
        Dictionary<int, float> startValues = new Dictionary<int, float>();
        Dictionary<int, float> targetValues = GetExpressionBlendShapeValues(targetExpression);
        
        // Store starting values
        foreach (var kvp in targetValues)
        {
            startValues[kvp.Key] = GetBlendShapeWeight(kvp.Key);
        }
        
        // Animate the transition
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            
            // Apply interpolated values to all blend shapes
            foreach (var kvp in targetValues)
            {
                int blendShapeIndex = kvp.Key;
                float targetValue = kvp.Value;
                float startValue = startValues.ContainsKey(blendShapeIndex) ? startValues[blendShapeIndex] : 0f;
                float currentValue = Mathf.Lerp(startValue, targetValue, t);
                
                SetBlendShapeWeight(blendShapeIndex, currentValue);
            }
            
            yield return null;
        }
        
        // Ensure final values are set exactly
        foreach (var kvp in targetValues)
        {
            SetBlendShapeWeight(kvp.Key, kvp.Value);
        }
        
        // Update current expression
        _currentExpression = targetExpression;
    }
    
    /// <summary>
    /// Coroutine for mouth movement visualization.
    /// </summary>
    private IEnumerator MicrophoneVisualizationCoroutine()
    {
        float elapsed = 0f;
        float duration = UnityEngine.Random.Range(minMouthMoveDuration, maxMouthMoveDuration);
        float startWeight = GetBlendShapeWeight(jawOpenBlendShapeIndex);
        float targetWeight = UnityEngine.Random.Range(minMouthOpenAmount, maxMouthOpenAmount);
        
        // Open mouth
        while (elapsed < duration * 0.4f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.4f);
            float weight = Mathf.Lerp(startWeight, targetWeight, t);
            SetBlendShapeWeight(jawOpenBlendShapeIndex, weight);
            yield return null;
        }
        
        // Hold open briefly
        yield return new WaitForSeconds(duration * 0.2f);
        
        // Close mouth
        elapsed = 0f;
        while (elapsed < duration * 0.4f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.4f);
            float weight = Mathf.Lerp(targetWeight, startWeight, t);
            SetBlendShapeWeight(jawOpenBlendShapeIndex, weight);
            yield return null;
        }
        
        // Update state
        _isAnimatingMouth = false;
    }
    
    /// <summary>
    /// Gets blend shape weight.
    /// </summary>
    /// <param name="index">Blend shape index.</param>
    /// <returns>Blend shape weight.</returns>
    private float GetBlendShapeWeight(int index)
    {
        if (faceRenderer == null || index < 0 || index >= faceRenderer.sharedMesh.blendShapeCount)
        {
            return 0f;
        }
        
        return faceRenderer.GetBlendShapeWeight(index);
    }
    
    /// <summary>
    /// Sets blend shape weight.
    /// </summary>
    /// <param name="index">Blend shape index.</param>
    /// <param name="weight">Blend shape weight.</param>
    private void SetBlendShapeWeight(int index, float weight)
    {
        if (faceRenderer == null || index < 0 || index >= faceRenderer.sharedMesh.blendShapeCount)
        {
            return;
        }
        
        faceRenderer.SetBlendShapeWeight(index, weight);
    }
    
    /// <summary>
    /// Gets blend shape values for a facial expression.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns>Dictionary of blend shape indices and values.</returns>
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
                
            case FacialExpression.Sad:
                values[smileBlendShapeIndex] = 0f;
                values[frownBlendShapeIndex] = 100f;
                break;
                
            case FacialExpression.Surprised:
                values[jawOpenBlendShapeIndex] = 50f;
                break;
                
            case FacialExpression.Angry:
                values[frownBlendShapeIndex] = 80f;
                break;
                
            case FacialExpression.Confused:
                values[frownBlendShapeIndex] = 40f;
                break;
                
            case FacialExpression.Thoughtful:
                values[frownBlendShapeIndex] = 20f;
                break;
                
            case FacialExpression.Interested:
                values[smileBlendShapeIndex] = 20f;
                break;
                
            case FacialExpression.Attentive:
                values[smileBlendShapeIndex] = 10f;
                break;
                
            case FacialExpression.Talking:
                // This is handled by the AnimateMouthForSpeaking method
                break;
        }
        
        return values;
    }
}