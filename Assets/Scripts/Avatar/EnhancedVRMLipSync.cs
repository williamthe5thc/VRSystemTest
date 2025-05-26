using System;
using System.Collections;
using UnityEngine;
using VRM;

namespace VRInterview
{
    /// <summary>
    /// Enhanced VRM lip synchronization component with better error handling and debugging
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class EnhancedVRMLipSync : MonoBehaviour
    {
        [Header("Required Components")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private VRMBlendShapeProxy _blendShapeProxy;
        
        [Header("LipSync Settings")]
        [SerializeField] private float lipSyncSensitivity = 5.0f;
        [SerializeField] private float smoothingFactor = 0.3f;
        [SerializeField] private bool useAmplitudeBasedLipSync = true;
        [SerializeField] private AnimationCurve lipSyncCurve;
        [SerializeField] private float maxMouthOpenValue = 1.5f;
        [SerializeField] private bool useVisemeBlending = true;
        
        [Header("Debug Settings")]
        [SerializeField] private bool debugMode = true;
        [SerializeField] private bool visualizeAudio = false;
        [SerializeField] private int _sampleDataLength = 1024;
        
        // Property accessors for external access
        public AudioSource AudioSource 
        {
            get { return _audioSource; }
            set { _audioSource = value; }
        }
        
        public VRMBlendShapeProxy BlendShapeProxy 
        {
            get { return _blendShapeProxy; }
            set { _blendShapeProxy = value; }
        }
        
        private bool _isLipSyncActive = false;
        private float _currentLipSyncValue = 0f;
        private float _targetLipSyncValue = 0f;
        private float[] _audioSamples;
        private float _lastUpdateTime = 0f;
        private int _errorCount = 0;
        private const int MAX_ERRORS = 10;
        
        private void Awake()
        {
            _audioSamples = new float[_sampleDataLength];
            
            // Initialize the lip sync curve if not set
            if (lipSyncCurve == null || lipSyncCurve.keys.Length == 0)
            {
                CreateDefaultLipSyncCurve();
            }
        }
        
        private void Start()
        {
            // Find required components if not assigned
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                    _audioSource.playOnAwake = false;
                    
                    if (debugMode)
                    {
                        Debug.Log("EnhancedVRMLipSync: Added AudioSource component");
                    }
                }
            }
            
            if (_blendShapeProxy == null)
            {
                _blendShapeProxy = GetComponent<VRMBlendShapeProxy>();
                if (_blendShapeProxy == null)
                {
                    _blendShapeProxy = GetComponentInChildren<VRMBlendShapeProxy>(true);
                    
                    if (_blendShapeProxy == null)
                    {
                        var parentObj = transform.parent ? transform.parent.gameObject : null;
                        if (parentObj != null)
                        {
                            _blendShapeProxy = parentObj.GetComponentInChildren<VRMBlendShapeProxy>(true);
                        }
                        
                        if (_blendShapeProxy == null)
                        {
                            _blendShapeProxy = FindObjectOfType<VRMBlendShapeProxy>();
                            
                            if (_blendShapeProxy == null)
                            {
                                Debug.LogError("EnhancedVRMLipSync: VRMBlendShapeProxy not found! Lip sync will not work.");
                                enabled = false;
                                return;
                            }
                        }
                    }
                    
                    if (debugMode)
                    {
                        Debug.Log($"EnhancedVRMLipSync: Found VRMBlendShapeProxy on {_blendShapeProxy.gameObject.name}");
                    }
                }
            }
            
            // Start with lip sync inactive
            _isLipSyncActive = false;
            
            if (debugMode)
            {
                Debug.Log($"EnhancedVRMLipSync initialized with AudioSource={_audioSource != null} BlendShapeProxy={_blendShapeProxy != null}");
            }
        }
        
        private void CreateDefaultLipSyncCurve()
        {
            lipSyncCurve = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(0.2f, 0.8f),
                new Keyframe(0.5f, 0.2f),
                new Keyframe(0.8f, 0.9f),
                new Keyframe(1.0f, 0.0f)
            );
            
            if (debugMode)
            {
                Debug.Log("EnhancedVRMLipSync: Created default lip sync curve");
            }
        }
        
        private void Update()
        {
            // Skip if missing required components
            if (_audioSource == null || _blendShapeProxy == null)
            {
                return;
            }
            
            try
            {
                // Limit update frequency to reduce overhead
                if (Time.time - _lastUpdateTime < 0.02f) // ~50 updates per second
                {
                    return;
                }
                _lastUpdateTime = Time.time;
                
                UpdateLipSyncTarget();
                SmoothLipSyncValue();
                ApplyLipSyncToBlendShapes();
                
                // Reset error count on successful update
                _errorCount = 0;
            }
            catch (Exception ex)
            {
                _errorCount++;
                
                if (_errorCount <= MAX_ERRORS)
                {
                    Debug.LogWarning($"EnhancedVRMLipSync: Error in Update ({_errorCount}/{MAX_ERRORS}): {ex.Message}");
                }
                
                if (_errorCount >= MAX_ERRORS)
                {
                    Debug.LogError($"EnhancedVRMLipSync: Too many errors, disabling component. Last error: {ex.Message}");
                    enabled = false;
                }
            }
        }
        
        private void UpdateLipSyncTarget()
        {
            if (!_isLipSyncActive)
            {
                // When not active, gradually return to closed mouth
                _targetLipSyncValue = 0f;
                return;
            }
            
            if (useAmplitudeBasedLipSync && _audioSource != null && _audioSource.isPlaying)
            {
                // Get audio amplitude for lip sync
                _audioSource.GetSpectrumData(_audioSamples, 0, FFTWindow.BlackmanHarris);
                
                // Calculate mouth openness from spectrum data
                float sum = 0f;
                for (int i = 0; i < 64; i++) // Use lower frequencies for speech
                {
                    sum += _audioSamples[i];
                }
                
                // Apply higher sensitivity for more visible movement
                _targetLipSyncValue = Mathf.Clamp01(sum * lipSyncSensitivity * 100f);
                
                if (visualizeAudio && debugMode)
                {
                    VisualizeAudioLevel(_targetLipSyncValue);
                }
                
                // Force some minimal mouth movement when audio is playing
                if (_audioSource.isPlaying && _targetLipSyncValue < 0.05f)
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
        }
        
        private void SmoothLipSyncValue()
        {
            // Smooth the transition with variable smoothing based on direction
            float smoothFactor = smoothingFactor;
            
            // Use faster smoothing when mouth is opening, slower when closing
            if (_targetLipSyncValue > _currentLipSyncValue)
            {
                smoothFactor *= 0.7f; // Open mouth faster
            }
            
            _currentLipSyncValue = Mathf.Lerp(_currentLipSyncValue, _targetLipSyncValue, 
                Time.deltaTime / smoothFactor);
        }
        
        private void ApplyLipSyncToBlendShapes()
        {
            if (_blendShapeProxy == null) return;
            
            try
            {
                // Scale the value by the max mouth open amount
                float scaledValue = _currentLipSyncValue * maxMouthOpenValue;
                
                if (useVisemeBlending)
                {
                    // Advanced approach: blend between different visemes based on value
                    if (_currentLipSyncValue < 0.2f)
                    {
                        // Mouth mostly closed - use O shape for low values
                        _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), scaledValue * 5f * 100f);
                        _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), 0f);
                    }
                    else
                    {
                        // Mouth more open - transition from O to A as value increases
                        _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), 
                            Mathf.Lerp(1f, 0f, (_currentLipSyncValue - 0.2f) / 0.8f) * maxMouthOpenValue * 100f);
                        
                        _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), 
                            Mathf.Lerp(0f, 1f, (_currentLipSyncValue - 0.2f) / 0.8f) * maxMouthOpenValue * 100f);
                    }
                }
                else
                {
                    // Simple approach: just use the "A" blend shape
                    // Note: Multiplying by 100 because VRM blend shapes use 0-100 range
                    _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), scaledValue * 100f);
                }
                
                // Apply the changes
                _blendShapeProxy.Apply();
            }
            catch (Exception ex)
            {
                Debug.LogError($"EnhancedVRMLipSync: Error applying lip sync to VRM: {ex.Message}");
                _isLipSyncActive = false;
            }
        }
        
        /// <summary>
        /// Starts lip synchronization.
        /// </summary>
        public void StartLipSync()
        {
            if (!enabled) return;
            
            _isLipSyncActive = true;
            
            if (debugMode)
            {
                Debug.Log("EnhancedVRMLipSync: Lip sync started");
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
                Debug.Log("EnhancedVRMLipSync: Lip sync stopped");
            }
            
            // Reset blend shapes
            if (_blendShapeProxy != null)
            {
                try
                {
                    _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), 0f);
                    _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), 0f);
                    _blendShapeProxy.Apply();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"EnhancedVRMLipSync: Error resetting blend shapes: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Updates the lip sync value manually.
        /// </summary>
        /// <param name="normalizedTime">Normalized time value (0-1) for procedural lip sync.</param>
        public void UpdateLipSyncValue(float normalizedTime)
        {
            if (!enabled || !_isLipSyncActive) return;
            
            float time = normalizedTime % 1.0f;
            _targetLipSyncValue = lipSyncCurve.Evaluate(time);
        }
        
        /// <summary>
        /// Visualizes audio level in the console (debug only).
        /// </summary>
        private void VisualizeAudioLevel(float value)
        {
            int barLength = Mathf.RoundToInt(value * 50);
            string bar = new string('|', barLength);
            Debug.Log($"Audio: [{bar}] {value:F3}");
        }
        
        // Called when script is enabled or after being disabled
        private void OnEnable()
        {
            _errorCount = 0;
            
            if (debugMode)
            {
                Debug.Log("EnhancedVRMLipSync: Component enabled");
            }
        }
        
        // Editor-only utility to manually check for components
        public void ScanForComponents()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            
            if (_blendShapeProxy == null)
            {
                _blendShapeProxy = GetComponent<VRMBlendShapeProxy>();
                if (_blendShapeProxy == null)
                {
                    _blendShapeProxy = GetComponentInChildren<VRMBlendShapeProxy>(true);
                    
                    if (_blendShapeProxy == null)
                    {
                        _blendShapeProxy = FindObjectOfType<VRMBlendShapeProxy>();
                    }
                }
            }
            
            Debug.Log($"EnhancedVRMLipSync component scan complete: AudioSource={_audioSource != null}, BlendShapeProxy={_blendShapeProxy != null}");
        }
    }
}
