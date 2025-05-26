using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Debug helper script for audio playback issues
/// </summary>
public class AudioPlaybackDebug : MonoBehaviour
{
    [Header("References")]
    // Changed to public to fix accessibility errors
    public AudioPlayback audioPlayback;
    public AudioSource audioSource;
    [SerializeField] private Button testSoundButton;
    
    [Header("Debug Settings")]
    [SerializeField] private bool debugEnabled = true;
    [SerializeField] private bool saveReceivedAudio = true;
    [SerializeField] private string debugFolder = "AudioDebug";
    
    private void Start()
    {
        // Find references if not assigned
        if (audioPlayback == null)
        {
            audioPlayback = FindObjectOfType<AudioPlayback>();
            if (audioPlayback == null)
            {
                Debug.LogError("AudioPlayback component not found");
            }
            else
            {
                Debug.Log("Found AudioPlayback component automatically");
            }
        }
        
        if (audioSource == null && audioPlayback != null)
        {
            audioSource = audioPlayback.GetComponent<AudioSource>();
            Debug.Log("Found AudioSource component from AudioPlayback");
        }
        
        // Set up button callback if available
        if (testSoundButton != null)
        {
            testSoundButton.onClick.AddListener(PlayTestSound);
            Debug.Log("Test sound button configured");
        }
        
        // Create debug folder if needed
        if (saveReceivedAudio)
        {
            System.IO.Directory.CreateDirectory(
                System.IO.Path.Combine(Application.persistentDataPath, debugFolder));
            Debug.Log($"Created debug folder at {Application.persistentDataPath}/{debugFolder}");
        }
        
        // Add event listeners to AudioPlayback
        if (audioPlayback != null)
        {
            audioPlayback.OnPlaybackStarted += () => Debug.Log("[DEBUG] Audio playback started");
            audioPlayback.OnPlaybackCompleted += () => Debug.Log("[DEBUG] Audio playback completed");
            audioPlayback.OnPlaybackError += (error) => Debug.LogError($"[DEBUG] Audio playback error: {error}");
            Debug.Log("Registered debug event handlers for AudioPlayback");
        }
        
        // Check for AudioListener
        if (FindObjectOfType<AudioListener>() == null)
        {
            Debug.LogError("No AudioListener found in the scene! Adding one to this GameObject");
            gameObject.AddComponent<AudioListener>();
        }
        else
        {
            Debug.Log("AudioListener found in scene");
        }
        
        // Log volume settings
        Debug.Log($"Audio settings: System volume = {AudioListener.volume}");
        if (audioSource != null)
        {
            Debug.Log($"AudioSource settings: Volume = {audioSource.volume}, Mute = {audioSource.mute}, Spatial Blend = {audioSource.spatialBlend}");
        }
        
        // Play test sound
        PlayTestSound();
    }
    
    /// <summary>
    /// Play test sound to verify audio system
    /// </summary>
    public void PlayTestSound()
    {
        Debug.Log("Playing test sound from AudioPlaybackDebug");
        if (audioPlayback != null)
        {
            audioPlayback.PlayTestSound();
        }
        else
        {
            Debug.LogError("Cannot play test sound - AudioPlayback reference is missing");
        }
    }
    
    /// <summary>
    /// Force audio playback with guaranteed settings
    /// </summary>
    public void ForcePlayAudio()
    {
        Debug.Log("Forcing audio playback with guaranteed settings");
        
        if (audioPlayback != null)
        {
            if (audioSource != null)
            {
                // Set guaranteed settings
                audioSource.spatialBlend = 0f; // Force 2D audio
                audioSource.volume = 1.0f;     // Set maximum volume
                audioSource.mute = false;      // Ensure not muted
                
                // Stop any playing audio
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
            
            // Play a test tone
            audioPlayback.PlayTestSound();
            
            Debug.Log("Forced audio playback initiated");
        }
        else
        {
            Debug.LogError("Cannot force audio playback - AudioPlayback reference is missing");
        }
    }
    
    private void Update()
    {
        // Press T key to play test sound
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T key pressed - playing test sound");
            PlayTestSound();
        }
        
        // Press F key to force audio
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F key pressed - forcing audio playback");
            ForcePlayAudio();
        }
    }
}