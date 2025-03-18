using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles lip synchronization with audio for the avatar.
/// </summary>
public class LipSync : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    
    [Header("Blend Shape Indices")]
    [SerializeField] private int mouthOpenIndex = 0;
    [SerializeField] private int mouthCloseIndex = 1;
    [SerializeField] private int mouthWideIndex = 2;
    [SerializeField] private int mouthNarrowIndex = 3;
    
    [Header("LipSync Settings")]
    [SerializeField] private float lipSyncSensitivity = 1.0f;
    [SerializeField] private float smoothingFactor = 0.5f;
    [SerializeField] private bool useAmplitudeBasedLipSync = true;
    [SerializeField] private AnimationCurve lipSyncCurve;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool visualizeLipSync = false;
    
    private bool _isLipSyncActive = false;
    private float _currentLipSyncValue = 0f;
    private float _targetLipSyncValue = 0f;
    
    // For amplitude-based lip sync
    private float[] _audioSamples = new float[1024];
    
    // For procedural lip sync
    private float _lipSyncTimer = 0f;
    private float _lipSyncSpeed = 1f;
    
    private void Awake()
    {
        InitializeComponents();
        
        // Initialize lip sync curve if not set
        if (lipSyncCurve.keys.Length == 0)
        {
            // Create a simple curve for procedural lip sync
            lipSyncCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.1f, 0.8f),
                new Keyframe(0.3f, 0.2f),
                new Keyframe(0.5f, 0.7f),
                new Keyframe(0.7f, 0.3f),
                new Keyframe(0.9f, 0.6f),
                new Keyframe(1.0f, 0.0f)
            );
        }
    }
    
    /// <summary>
    /// Initializes required components if not assigned.
    /// </summary>
    private void InitializeComponents()
    {
        // Find SkinnedMeshRenderer if not assigned
        if (faceRenderer == null)
        {
            faceRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            
            if (faceRenderer == null)
            {
                Debug.LogError("SkinnedMeshRenderer not found for LipSync!");
            }
        }
        
        // Find AudioSource if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = GetComponentInParent<AudioSource>();
                
                if (audioSource == null)
                {
                    Debug.LogWarning("AudioSource not found for LipSync! Will use procedural lip sync.");
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
        
        // Smooth the transition
        _currentLipSyncValue = Mathf.Lerp(_currentLipSyncValue, _targetLipSyncValue, 
            Time.deltaTime / smoothingFactor);
        
        // Apply to blend shapes
        if (faceRenderer != null)
        {
            ApplyLipSyncToBlendShapes(_currentLipSyncValue);
        }
        
        // Visualize lip sync in debug mode
        if (debugMode && visualizeLipSync)
        {
            VisualizeLipSync(_currentLipSyncValue);
        }
    }
    
    /// <summary>
    /// Starts lip synchronization.
    /// </summary>
    public void StartLipSync()
    {
        _isLipSyncActive = true;
        _lipSyncTimer = 0f;
        
        if (debugMode)
        {
            Debug.Log("Lip sync started");
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
            Debug.Log("Lip sync stopped");
        }
    }
    
    /// <summary>
    /// Updates the lip sync value manually.
    /// </summary>
    /// <param name="normalizedTime">Normalized time value (0-1) for procedural lip sync.</param>
    public void UpdateLipSyncValue(float normalizedTime)
    {
        if (!useAmplitudeBasedLipSync && _isLipSyncActive)
        {
            _lipSyncTimer = Mathf.Clamp01(normalizedTime);
        }
    }
    
    /// <summary>
    /// Applies lip sync value to blend shapes.
    /// </summary>
    /// <param name="lipSyncValue">The lip sync value (0-1).</param>
    private void ApplyLipSyncToBlendShapes(float lipSyncValue)
    {
        if (faceRenderer == null) return;
        
        try
        {
            // Modify blend shapes based on lip sync value
            // This is a simplified mapping and should be customized for your avatar model
            
            // Mouth open/close (inversely related)
            float mouthOpen = lipSyncValue * 100f; 
            float mouthClose = (1f - lipSyncValue) * 30f; // Less extreme for closing
            
            // Mouth wide/narrow (correlated with openness but less intense)
            float mouthWide = lipSyncValue * 40f;
            float mouthNarrow = (1f - lipSyncValue) * 20f;
            
            // Apply blend shape weights if indices are valid
            if (mouthOpenIndex >= 0 && mouthOpenIndex < faceRenderer.sharedMesh.blendShapeCount)
            {
                faceRenderer.SetBlendShapeWeight(mouthOpenIndex, mouthOpen);
            }
            
            if (mouthCloseIndex >= 0 && mouthCloseIndex < faceRenderer.sharedMesh.blendShapeCount)
            {
                faceRenderer.SetBlendShapeWeight(mouthCloseIndex, mouthClose);
            }
            
            if (mouthWideIndex >= 0 && mouthWideIndex < faceRenderer.sharedMesh.blendShapeCount)
            {
                faceRenderer.SetBlendShapeWeight(mouthWideIndex, mouthWide);
            }
            
            if (mouthNarrowIndex >= 0 && mouthNarrowIndex < faceRenderer.sharedMesh.blendShapeCount)
            {
                faceRenderer.SetBlendShapeWeight(mouthNarrowIndex, mouthNarrow);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error applying lip sync to blend shapes: {ex.Message}");
            
            // Disable lip sync on error
            _isLipSyncActive = false;
        }
    }
    
    /// <summary>
    /// Sets the audio source for lip sync.
    /// </summary>
    /// <param name="source">The audio source to use.</param>
    public void SetAudioSource(AudioSource source)
    {
        audioSource = source;
    }
    
    /// <summary>
    /// Sets the face renderer for lip sync.
    /// </summary>
    /// <param name="renderer">The skinned mesh renderer to use.</param>
    public void SetFaceRenderer(SkinnedMeshRenderer renderer)
    {
        faceRenderer = renderer;
    }
    
    /// <summary>
    /// Sets the lip sync sensitivity.
    /// </summary>
    /// <param name="sensitivity">The sensitivity value.</param>
    public void SetSensitivity(float sensitivity)
    {
        lipSyncSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
    }
    
    /// <summary>
    /// Visualizes lip sync value in the console (debug only).
    /// </summary>
    /// <param name="value">The lip sync value (0-1).</param>
    private void VisualizeLipSync(float value)
    {
        int barLength = Mathf.RoundToInt(value * 50);
        string bar = new string('|', barLength);
        Debug.Log($"Lip Sync: [{bar}] {value:F3}");
    }
}