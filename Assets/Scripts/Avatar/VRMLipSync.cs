using System;
using System.Collections;
using UnityEngine;
using VRM;

/// <summary>
/// Lip synchronization component specifically for VRM avatars
/// </summary>
public class VRMLipSync : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    
    // Property accessors for external access
    public AudioSource AudioSource {
        get { return audioSource; }
        set { audioSource = value; }
    }
    [SerializeField] private VRMBlendShapeProxy blendShapeProxy;
    
    // Property accessors for external access
    public VRMBlendShapeProxy BlendShapeProxy {
        get { return blendShapeProxy; }
        set { blendShapeProxy = value; }
    }
    
    [Header("LipSync Settings")]
    [SerializeField] private float lipSyncSensitivity = 3.0f;
    [SerializeField] private float smoothingFactor = 0.5f;
    [SerializeField] private bool useAmplitudeBasedLipSync = true;
    [SerializeField] private AnimationCurve lipSyncCurve;
    
    [Header("VRM Specific")]
    [SerializeField] private float maxMouthOpenValue = 1.5f;
    [SerializeField] private bool useVisemeBlending = true;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private bool _isLipSyncActive = false;
    private float _currentLipSyncValue = 0f;
    private float _targetLipSyncValue = 0f;
    
    // For amplitude-based lip sync
    private float[] _audioSamples = new float[1024];
    
    private void Start()
    {
        InitializeComponents();
        
        // Initialize default lip sync curve if not set
        if (lipSyncCurve == null || lipSyncCurve.keys.Length == 0)
        {
            lipSyncCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.2f, 0.8f),
                new Keyframe(0.5f, 0.2f),
                new Keyframe(0.8f, 0.9f),
                new Keyframe(1.0f, 0.0f)
            );
        }
    }
    
    private void InitializeComponents()
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
                    Debug.LogError("VRMBlendShapeProxy not found. VRM lip sync will not work.");
                }
            }
        }
        
        // Find AudioSource if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = FindObjectOfType<AudioPlayback>()?.GetComponent<AudioSource>();
                
                if (audioSource == null)
                {
                    Debug.LogWarning("AudioSource not found for lip sync. Using procedural lip sync.");
                    useAmplitudeBasedLipSync = false;
                }
            }
        }
    }
    
    private void Update()
    {
        if (!_isLipSyncActive)
        {
            // When not active, gradually return to closed mouth
            _targetLipSyncValue = 0f;
        }
        else if (useAmplitudeBasedLipSync && audioSource != null && audioSource.isPlaying)
        {
            // Get audio amplitude for lip sync
            audioSource.GetSpectrumData(_audioSamples, 0, FFTWindow.Rectangular);
            
            // Calculate mouth openness from spectrum data
            float sum = 0f;
            for (int i = 0; i < 64; i++) // Use lower frequencies for speech
            {
                sum += _audioSamples[i];
            }
            
            // Apply higher sensitivity for more visible movement
            _targetLipSyncValue = Mathf.Clamp01(sum * lipSyncSensitivity * 100f);
            
            // Force some minimal mouth movement when audio is playing
            if (audioSource.isPlaying && _targetLipSyncValue < 0.05f)
            {
                _targetLipSyncValue = 0.1f + Mathf.Sin(Time.time * 10f) * 0.05f;
            }
        }
        else if (_isLipSyncActive)
        {
            // Use procedural lip sync when amplitude-based is not available
            float time = Time.time % 1.0f;
            _targetLipSyncValue = lipSyncCurve.Evaluate(time);
        }
        
        // Smooth the transition
        _currentLipSyncValue = Mathf.Lerp(_currentLipSyncValue, _targetLipSyncValue, 
            Time.deltaTime / smoothingFactor);
        
        // Apply to VRM blend shapes
        if (blendShapeProxy != null)
        {
            ApplyLipSyncToVRM(_currentLipSyncValue);
        }
    }
    
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
    
    /// <summary>
    /// Starts lip synchronization.
    /// </summary>
    public void StartLipSync()
    {
        _isLipSyncActive = true;
        
        if (debugMode)
        {
            Debug.Log("VRM lip sync started");
        }
    }
    
    /// <summary>
    /// Stops lip synchronization.
    /// </summary>
    public void StopLipSync()
    {
        _isLipSyncActive = false;
        
        if (debugMode)
        {
            Debug.Log("VRM lip sync stopped");
        }
        
        // Reset blend shapes
        if (blendShapeProxy != null)
        {
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), 0f);
            blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), 0f);
            blendShapeProxy.Apply();
        }
    }
    
    /// <summary>
    /// Updates the lip sync value manually.
    /// </summary>
    /// <param name="normalizedTime">Normalized time value (0-1) for procedural lip sync.</param>
    public void UpdateLipSyncValue(float normalizedTime)
    {
        float time = normalizedTime % 1.0f;
        _targetLipSyncValue = lipSyncCurve.Evaluate(time);
    }
}