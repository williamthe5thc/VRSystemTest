using UnityEngine;

/// <summary>
/// Patch for AudioPlayback to fix common playback issues
/// Attach this to your AudioPlayback GameObject
/// </summary>
public class AudioPlaybackFix : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public AudioPlayback audioPlayback; // Made public
    [SerializeField] public AudioSource audioSource;    // Made public
    [SerializeField] public AvatarController avatarController; // Added reference to AvatarController
    
    [Header("Fix Settings")]
    [SerializeField] public bool fixPlaybackIssues = true; // Made public
    [SerializeField] public bool forceNonSpatialAudio = true; // Made public
    [SerializeField] public float minimumVolume = 0.7f;
    [SerializeField] public bool fixOnAwake = true;
    [SerializeField] public float fixDelay = 0.5f; // Delay before applying fixes
    
    private void Awake()
    {
        if (fixOnAwake)
        {
            // Delayed fix to ensure all components are properly initialized
            Invoke("DelayedFix", fixDelay);
        }
    }
    
    private void Start()
    {
        // Find references if not assigned
        FindReferences();
        
        // Apply initial fixes
        ApplyAudioSourceFixes();
        
        // Subscribe to events
        SubscribeToEvents();
        
        Debug.Log("AudioPlaybackFix: Applied initial fixes and subscribed to events");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (audioPlayback != null)
        {
            audioPlayback.OnPlaybackStarted -= HandlePlaybackStarted;
            audioPlayback.OnPlaybackError -= HandlePlaybackError;
        }
    }
    
    private void FindReferences()
    {
        // Find AudioPlayback if not assigned
        if (audioPlayback == null)
        {
            audioPlayback = GetComponent<AudioPlayback>();
            if (audioPlayback == null)
            {
                audioPlayback = FindObjectOfType<AudioPlayback>();
            }
        }
        
        // Find AudioSource if not assigned
        if (audioSource == null && audioPlayback != null)
        {
            audioSource = audioPlayback.GetComponent<AudioSource>();
            
            // Create AudioSource if missing
            if (audioSource == null)
            {
                audioSource = audioPlayback.gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
                audioSource.volume = minimumVolume;
                Debug.Log("AudioPlaybackFix: Added missing AudioSource component");
            }
        }
        
        // Find AvatarController if not assigned
        if (avatarController == null)
        {
            avatarController = FindObjectOfType<AvatarController>();
        }
        
        // Connect avatar controller to audio playback if needed
        if (audioPlayback != null && avatarController != null)
        {
            // Try to set avatar controller via reflection if available
            try
            {
                var avatarField = audioPlayback.GetType().GetField("avatarController", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic);
                
                if (avatarField != null)
                {
                    var currentValue = avatarField.GetValue(audioPlayback);
                    if (currentValue == null)
                    {
                        avatarField.SetValue(audioPlayback, avatarController);
                        Debug.Log("AudioPlaybackFix: Connected AvatarController to AudioPlayback");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"AudioPlaybackFix: Could not connect AvatarController: {ex.Message}");
            }
        }
    }
    
    private void SubscribeToEvents()
    {
        if (audioPlayback != null)
        {
            // Remove existing handlers to prevent duplicates
            audioPlayback.OnPlaybackStarted -= HandlePlaybackStarted;
            audioPlayback.OnPlaybackError -= HandlePlaybackError;
            
            // Add event handlers
            audioPlayback.OnPlaybackStarted += HandlePlaybackStarted;
            audioPlayback.OnPlaybackError += HandlePlaybackError;
        }
    }
    
    private void DelayedFix()
    {
        FindReferences();
        ApplyAudioSourceFixes();
        FixLipSyncConnections();
        Debug.Log("AudioPlaybackFix: Applied delayed fixes");
    }
    
    private void ApplyAudioSourceFixes()
    {
        if (audioSource == null || !fixPlaybackIssues) return;
        
        // Ensure volume is audible
        if (audioSource.volume < minimumVolume)
        {
            audioSource.volume = minimumVolume;
            Debug.Log($"AudioPlaybackFix: Set volume to {minimumVolume}");
        }
        
        // Ensure audio is not spatial for reliable playback
        if (forceNonSpatialAudio && audioSource.spatialBlend > 0)
        {
            audioSource.spatialBlend = 0f;
            Debug.Log("AudioPlaybackFix: Forced audio to non-spatial (2D)");
        }
        
        // Ensure audio is not muted
        if (audioSource.mute)
        {
            audioSource.mute = false;
            Debug.Log("AudioPlaybackFix: Unmuted audio source");
        }
        
        // Ensure playOnAwake is off (let AudioPlayback control this)
        if (audioSource.playOnAwake)
        {
            audioSource.playOnAwake = false;
            Debug.Log("AudioPlaybackFix: Disabled playOnAwake");
        }
        
        // Set priority to high
        if (audioSource.priority > 128)
        {
            audioSource.priority = 0; // Highest priority
            Debug.Log("AudioPlaybackFix: Set audio priority to maximum");
        }
    }
    
    private void FixLipSyncConnections()
    {
        // Fix LipSync AudioSource connections
        LipSync[] lipSyncs = FindObjectsOfType<LipSync>(true);
        foreach (var lipSync in lipSyncs)
        {
            if (lipSync != null)
            {
                lipSync.SetAudioSource(audioSource);
                Debug.Log($"AudioPlaybackFix: Connected AudioSource to {lipSync.name}");
            }
        }
        
        // Also try to fix VRMLipSync if present
        try
        {
            var vrmLipSyncType = System.Type.GetType("VRMLipSync") ?? System.Type.GetType("VRM.VRMLipSync");
            if (vrmLipSyncType != null)
            {
                var vrmLipSyncs = FindObjectsOfType(vrmLipSyncType, true);
                foreach (var lipSync in vrmLipSyncs)
                {
                    var audioSourceField = vrmLipSyncType.GetField("AudioSource", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic);
                        
                    if (audioSourceField != null)
                    {
                        audioSourceField.SetValue(lipSync, audioSource);
                        Debug.Log($"AudioPlaybackFix: Connected AudioSource to VRMLipSync");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"AudioPlaybackFix: Could not fix VRMLipSync: {ex.Message}");
        }
    }
    
    private void HandlePlaybackStarted()
    {
        // Reapply fixes when playback starts
        if (fixPlaybackIssues)
        {
            ApplyAudioSourceFixes();
            
            // Force play if not already playing
            if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log("AudioPlaybackFix: Forced play on audio source");
            }
            
            // Connect to avatar if available
            if (avatarController != null)
            {
                avatarController.OnAudioPlaybackStarted();
                Debug.Log("AudioPlaybackFix: Notified AvatarController of playback start");
            }
        }
    }
    
    private void HandlePlaybackError(string errorMessage)
    {
        Debug.LogWarning($"AudioPlaybackFix: Attempting to recover from error: {errorMessage}");
        
        // Try emergency playback recovery
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Stop();
            audioSource.volume = 1.0f;
            audioSource.spatialBlend = 0.0f;
            audioSource.Play();
            Debug.Log("AudioPlaybackFix: Attempted emergency audio recovery");
        }
    }
    
    public void FixNow()
    {
        FindReferences();
        ApplyAudioSourceFixes();
        FixLipSyncConnections();
        
        if (audioSource != null && audioSource.clip != null)
        {
            // Stop and restart playback
            audioSource.Stop();
            audioSource.Play();
            Debug.Log("AudioPlaybackFix: Manually restarted audio playback");
        }
    }
}