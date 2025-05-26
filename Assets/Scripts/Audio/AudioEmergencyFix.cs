using UnityEngine;
using System.Collections;

/// <summary>
/// EMERGENCY FIX for audio issues in the VR Interview System
/// Attach this to the Audio gameobject
/// </summary>
public class AudioEmergencyFix : MonoBehaviour
{
    [Header("Audio System Information")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool createTestSoundOnStart = false;
    
    // Track existing components
    private AudioPlayback _audioPlayback;
    private AudioSource _audioSource;
    private AudioSource _dedicatedAudioSource;
    private AudioClip _testTone;
    
    private void Awake()
    {
        // Create dedicated audio source
        _dedicatedAudioSource = gameObject.AddComponent<AudioSource>();
        _dedicatedAudioSource.name = "EmergencyFixAudioSource";
        _dedicatedAudioSource.playOnAwake = false;
        _dedicatedAudioSource.volume = 1.0f;
        _dedicatedAudioSource.spatialBlend = 0.0f; // Force 2D audio
        _dedicatedAudioSource.priority = 0; // Highest priority
        
        // Create test tone
        _testTone = CreateTestTone();
    }
    
    private void Start()
    {
        Debug.Log("AudioEmergencyFix: Starting emergency audio system diagnostics");
        
        // Find components
        _audioPlayback = GetComponent<AudioPlayback>();
        _audioSource = GetComponent<AudioSource>();
        
        // Log system info
        if (showDebugInfo)
        {
            LogAudioSystemInfo();
        }
        
        // Auto-fix if enabled
        if (autoFixOnStart)
        {
            // Wait a moment for everything to initialize
            StartCoroutine(DelayedFix(0.5f));
        }
        
        // Create test sound if enabled
        if (createTestSoundOnStart)
        {
            StartCoroutine(DelayedPlayTestSound(1.0f));
        }
    }
    
    /// <summary>
    /// Plays a test tone using the dedicated audio source
    /// </summary>
    public void PlayTestSound()
    {
        // Stop any playing sound
        if (_dedicatedAudioSource.isPlaying)
        {
            _dedicatedAudioSource.Stop();
        }
        
        // Set up audio source
        _dedicatedAudioSource.clip = _testTone;
        _dedicatedAudioSource.volume = 1.0f;
        _dedicatedAudioSource.spatialBlend = 0.0f;
        _dedicatedAudioSource.loop = false;
        
        // Play the sound
        _dedicatedAudioSource.Play();
        
        Debug.Log($"AudioEmergencyFix: Playing test tone, duration {_testTone.length}s");
    }
    
    /// <summary>
    /// Attempts to fix all common audio issues
    /// </summary>
    public void FixAllIssues()
    {
        Debug.Log("AudioEmergencyFix: Applying emergency fixes");
        
        // Create/fix AudioSource
        FixAudioSource();
        
        // Fix AudioListener
        FixAudioListener();
        
        // Fix all audio settings
        FixAudioSettings();
        
        // Log results
        LogAudioSystemInfo();
        
        Debug.Log("AudioEmergencyFix: Emergency fixes applied");
    }
    
    private IEnumerator DelayedFix(float delay)
    {
        yield return new WaitForSeconds(delay);
        FixAllIssues();
    }
    
    private IEnumerator DelayedPlayTestSound(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayTestSound();
    }
    
    private void FixAudioSource()
    {
        // First check if we have an AudioSource
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            
            // Create if missing
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("AudioEmergencyFix: Created missing AudioSource");
            }
        }
        
        // Configure AudioSource for reliable playback
        _audioSource.spatialBlend = 0.0f; // 2D audio
        _audioSource.volume = 1.0f;
        _audioSource.mute = false;
        _audioSource.priority = 0; // Highest priority
        
        Debug.Log("AudioEmergencyFix: Configured AudioSource");
        
        // Try to connect to AudioPlayback if available
        if (_audioPlayback != null)
        {
            try
            {
                // Use reflection to set the audioSource field
                var field = _audioPlayback.GetType().GetField("audioSource", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic);
                    
                if (field != null)
                {
                    field.SetValue(_audioPlayback, _audioSource);
                    Debug.Log("AudioEmergencyFix: Connected AudioSource to AudioPlayback");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"AudioEmergencyFix: Error connecting AudioSource - {ex.Message}");
            }
        }
    }
    
    private void FixAudioListener()
    {
        // Find all AudioListeners
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        
        if (listeners.Length == 0)
        {
            // No listeners, add one to camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.gameObject.AddComponent<AudioListener>();
                Debug.Log("AudioEmergencyFix: Added AudioListener to main camera");
            }
            else
            {
                // No main camera, add to this object
                gameObject.AddComponent<AudioListener>();
                Debug.Log("AudioEmergencyFix: Added AudioListener to this object (no main camera)");
            }
        }
        else if (listeners.Length > 1)
        {
            // Too many listeners, disable all but one
            for (int i = 1; i < listeners.Length; i++)
            {
                listeners[i].enabled = false;
                Debug.Log($"AudioEmergencyFix: Disabled extra AudioListener on {listeners[i].gameObject.name}");
            }
        }
    }
    
    private void FixAudioSettings()
    {
        // Nothing to change, just logging current settings
        Debug.Log($"AudioEmergencyFix: Current sample rate: {AudioSettings.outputSampleRate}Hz");
        
        // Ensure no audio filter effects are causing issues
        AudioLowPassFilter lowPass = GetComponent<AudioLowPassFilter>();
        if (lowPass != null)
        {
            DestroyImmediate(lowPass);
            Debug.Log("AudioEmergencyFix: Removed AudioLowPassFilter");
        }
        
        AudioHighPassFilter highPass = GetComponent<AudioHighPassFilter>();
        if (highPass != null)
        {
            DestroyImmediate(highPass);
            Debug.Log("AudioEmergencyFix: Removed AudioHighPassFilter");
        }
    }
    
    private void LogAudioSystemInfo()
    {
        Debug.Log("=== AUDIO SYSTEM DIAGNOSTICS ===");
        Debug.Log($"AudioSettings.outputSampleRate: {AudioSettings.outputSampleRate}Hz");
        Debug.Log($"AudioSettings.speakerMode: {AudioSettings.speakerMode}");
        
        AudioConfiguration config = AudioSettings.GetConfiguration();
        Debug.Log($"DSP Buffer Size: {config.dspBufferSize}");
        
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        Debug.Log($"AudioListeners in scene: {listeners.Length}");
        
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        Debug.Log($"AudioSources in scene: {sources.Length}");
        
        AudioPlayback[] playbacks = FindObjectsOfType<AudioPlayback>();
        Debug.Log($"AudioPlayback components in scene: {playbacks.Length}");
        
        // Check our component's status
        if (_audioSource != null)
        {
            Debug.Log($"Our AudioSource status: Enabled={_audioSource.enabled}, Mute={_audioSource.mute}, Volume={_audioSource.volume}");
            Debug.Log($"Our AudioSource has clip: {_audioSource.clip != null}");
            Debug.Log($"Our AudioSource is playing: {_audioSource.isPlaying}");
        }
        else
        {
            Debug.Log("Our AudioSource is NULL");
        }
        
        if (_audioPlayback != null)
        {
            Debug.Log($"AudioPlayback component found: {_audioPlayback.name}");
            Debug.Log($"AudioPlayback IsPlaying property: {_audioPlayback.IsPlaying}");
        }
        else
        {
            Debug.Log("AudioPlayback component is NULL");
        }
        
        Debug.Log("=== END DIAGNOSTICS ===");
    }
    
    /// <summary>
    /// Creates a test tone for verifying audio output
    /// </summary>
    private AudioClip CreateTestTone()
    {
        int sampleRate = 48000; // Use high sample rate for compatibility
        float frequency = 440f;  // A4 note
        float duration = 1.0f;
        
        // Create the clip
        AudioClip clip = AudioClip.Create("EmergencyTestTone", (int)(sampleRate * duration), 1, sampleRate, false);
        
        // Generate sine wave data
        float[] samples = new float[(int)(sampleRate * duration)];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / sampleRate;
            
            // Apply fade in/out envelope
            float envelope = Mathf.Clamp01(Mathf.Min(t * 4, (duration - t) * 4));
            
            // Generate sine wave with envelope
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope;
        }
        
        // Set the data
        clip.SetData(samples, 0);
        
        return clip;
    }
}