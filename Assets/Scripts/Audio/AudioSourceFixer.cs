using UnityEngine;
using System.Collections;

/// <summary>
/// Fixes common audio source issues that affect lip sync
/// </summary>
public class AudioSourceFixer : MonoBehaviour
{
    [Header("Target Components")]
    [SerializeField] private AudioSource[] targetAudioSources;
    [SerializeField] private AudioPlayback audioPlayback;
    
    [Header("Settings")]
    [SerializeField] private bool fixOnAwake = true;
    [SerializeField] private bool fixOnStart = true;
    [SerializeField] private bool fixAfterDelay = true;
    [SerializeField] private float fixDelay = 2.0f;
    [SerializeField] private bool forceSpatialAudio = false;
    [SerializeField] private bool force2DAudio = true;
    [SerializeField] private float minimumVolume = 0.8f;
    [SerializeField] private bool logDebugInfo = true;
    
    private int _fixAttempts = 0;
    
    private void Awake()
    {
        if (fixOnAwake)
        {
            FixAudioSources();
        }
    }
    
    private void Start()
    {
        if (fixOnStart)
        {
            FixAudioSources();
        }
        
        if (fixAfterDelay)
        {
            StartCoroutine(ApplyFixesAfterDelay());
        }
    }
    
    private IEnumerator ApplyFixesAfterDelay()
    {
        yield return new WaitForSeconds(fixDelay);
        FixAudioSources();
        _fixAttempts++;
        
        if (logDebugInfo)
        {
            Debug.Log($"AudioSourceFixer: Applied fixes after delay (attempt {_fixAttempts})");
        }
    }
    
    /// <summary>
    /// Applies all fixes to configured audio sources
    /// </summary>
    public void FixAudioSources()
    {
        // Find audio sources if not assigned
        FindAudioSources();
        
        // Apply fixes to each audio source
        if (targetAudioSources != null && targetAudioSources.Length > 0)
        {
            foreach (var source in targetAudioSources)
            {
                if (source != null)
                {
                    ApplyFixesToAudioSource(source);
                }
            }
        }
        
        // Try to fix specific known audio problems
        FixAudioPlaybackIssues();
    }
    
    private void FindAudioSources()
    {
        // If no audio sources specified, try to find some
        if (targetAudioSources == null || targetAudioSources.Length == 0)
        {
            // First check AudioPlayback
            if (audioPlayback == null)
            {
                audioPlayback = FindObjectOfType<AudioPlayback>();
            }
            
            if (audioPlayback != null)
            {
                var source = audioPlayback.GetComponent<AudioSource>();
                if (source != null)
                {
                    targetAudioSources = new AudioSource[] { source };
                    
                    if (logDebugInfo)
                    {
                        Debug.Log("AudioSourceFixer: Found AudioSource on AudioPlayback");
                    }
                }
            }
            
            // If still not found, look for any in the scene
            if (targetAudioSources == null || targetAudioSources.Length == 0)
            {
                targetAudioSources = FindObjectsOfType<AudioSource>();
                
                if (logDebugInfo && targetAudioSources.Length > 0)
                {
                    Debug.Log($"AudioSourceFixer: Found {targetAudioSources.Length} AudioSources in scene");
                }
            }
        }
    }
    
    private void ApplyFixesToAudioSource(AudioSource source)
    {
        if (source == null) return;
        
        try
        {
            // Apply volume fix
            if (source.volume < minimumVolume)
            {
                source.volume = minimumVolume;
                
                if (logDebugInfo)
                {
                    Debug.Log($"AudioSourceFixer: Set volume to {minimumVolume} on {source.gameObject.name}");
                }
            }
            
            // Apply spatial audio setting
            if (forceSpatialAudio && source.spatialBlend < 1.0f)
            {
                source.spatialBlend = 1.0f;
                
                if (logDebugInfo)
                {
                    Debug.Log($"AudioSourceFixer: Forced spatial audio (3D) on {source.gameObject.name}");
                }
            }
            else if (force2DAudio && source.spatialBlend > 0.0f)
            {
                source.spatialBlend = 0.0f;
                
                if (logDebugInfo)
                {
                    Debug.Log($"AudioSourceFixer: Forced non-spatial audio (2D) on {source.gameObject.name}");
                }
            }
            
            // Fix other common issues
            if (source.mute)
            {
                source.mute = false;
                
                if (logDebugInfo)
                {
                    Debug.Log($"AudioSourceFixer: Unmuted {source.gameObject.name}");
                }
            }
            
            // Audio source should not play on awake
            if (source.playOnAwake)
            {
                source.playOnAwake = false;
                
                if (logDebugInfo)
                {
                    Debug.Log($"AudioSourceFixer: Disabled playOnAwake for {source.gameObject.name}");
                }
            }
            
            // Set priority to maximum
            if (source.priority > 0)
            {
                source.priority = 0;
                
                if (logDebugInfo)
                {
                    Debug.Log($"AudioSourceFixer: Set priority to maximum for {source.gameObject.name}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"AudioSourceFixer: Error fixing {source.gameObject.name}: {ex.Message}");
        }
    }
    
    private void FixAudioPlaybackIssues()
    {
        if (audioPlayback == null) return;
        
        try
        {
            // Try to fix field in AudioPlayback via reflection
            var audioSourceField = audioPlayback.GetType().GetField("audioSource", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (audioSourceField != null)
            {
                var source = audioPlayback.GetComponent<AudioSource>();
                if (source != null)
                {
                    audioSourceField.SetValue(audioPlayback, source);
                    
                    if (logDebugInfo)
                    {
                        Debug.Log("AudioSourceFixer: Connected AudioSource to AudioPlayback field");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"AudioSourceFixer: Error fixing AudioPlayback: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Creates a default audio source if none exists
    /// </summary>
    public AudioSource CreateDefaultAudioSource(GameObject targetObject)
    {
        if (targetObject == null)
        {
            targetObject = gameObject;
        }
        
        // Check if it already has an audio source
        AudioSource source = targetObject.GetComponent<AudioSource>();
        if (source != null)
        {
            return source;
        }
        
        // Create a new audio source
        source = targetObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.volume = minimumVolume;
        
        if (forceSpatialAudio)
        {
            source.spatialBlend = 1.0f;
        }
        else if (force2DAudio)
        {
            source.spatialBlend = 0.0f;
        }
        
        source.priority = 0;
        
        if (logDebugInfo)
        {
            Debug.Log($"AudioSourceFixer: Created new AudioSource on {targetObject.name}");
        }
        
        return source;
    }
}
