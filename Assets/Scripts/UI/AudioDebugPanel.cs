using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides a simple UI for debugging audio issues
/// </summary>
public class AudioDebugPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioPlayback audioPlayback;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioPlaybackFix audioFix;
    [SerializeField] private AudioPlaybackDebug audioDebug;
    
    [Header("UI Elements")]
    [SerializeField] private Button fixAudioButton;
    [SerializeField] private Button forcePlayButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle spatialAudioToggle;
    [SerializeField] private Text statusText;
    
    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    
    private float _lastUpdateTime = 0f;
    
    private void Start()
    {
        // Find references if not assigned
        if (audioPlayback == null)
        {
            audioPlayback = FindObjectOfType<AudioPlayback>();
        }
        
        if (audioSource == null && audioPlayback != null)
        {
            audioSource = audioPlayback.GetComponent<AudioSource>();
        }
        
        if (audioFix == null && audioPlayback != null)
        {
            audioFix = audioPlayback.GetComponent<AudioPlaybackFix>();
            
            if (audioFix == null)
            {
                audioFix = audioPlayback.gameObject.AddComponent<AudioPlaybackFix>();
            }
        }
        
        if (audioDebug == null && audioPlayback != null)
        {
            audioDebug = audioPlayback.GetComponent<AudioPlaybackDebug>();
            
            if (audioDebug == null)
            {
                audioDebug = audioPlayback.gameObject.AddComponent<AudioPlaybackDebug>();
            }
        }
        
        // Set up UI elements
        if (fixAudioButton != null)
        {
            fixAudioButton.onClick.AddListener(FixAudio);
        }
        
        if (forcePlayButton != null)
        {
            forcePlayButton.onClick.AddListener(ForcePlayAudio);
        }
        
        if (volumeSlider != null && audioSource != null)
        {
            volumeSlider.value = audioSource.volume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        if (spatialAudioToggle != null && audioSource != null)
        {
            spatialAudioToggle.isOn = audioSource.spatialBlend > 0.5f;
            spatialAudioToggle.onValueChanged.AddListener(OnSpatialAudioToggled);
        }
        
        // Initial status update
        UpdateStatus();
    }
    
    private void Update()
    {
        // Update status periodically
        if (Time.time - _lastUpdateTime > updateInterval)
        {
            UpdateStatus();
            _lastUpdateTime = Time.time;
        }
    }
    
    private void UpdateStatus()
    {
        if (statusText == null) return;
        
        string status = "Audio Status:\n";
        
        if (audioSource != null)
        {
            status += $"Playing: {audioSource.isPlaying}\n";
            status += $"Volume: {audioSource.volume:F2}\n";
            status += $"Spatial: {audioSource.spatialBlend:F2}\n";
            
            if (audioSource.clip != null)
            {
                status += $"Clip Length: {audioSource.clip.length:F2}s\n";
                status += $"Progress: {audioSource.time:F2}s\n";
                status += $"Channels: {audioSource.clip.channels}\n";
            }
            else
            {
                status += "No audio clip loaded\n";
            }
        }
        else
        {
            status += "No AudioSource found\n";
        }
        
        statusText.text = status;
    }
    
    public void FixAudio()
    {
        if (audioFix != null)
        {
            audioFix.FixNow();
            UpdateStatus();
        }
        else
        {
            Debug.LogWarning("AudioPlaybackFix not found");
        }
    }
    
    public void ForcePlayAudio()
    {
        if (audioDebug != null)
        {
            audioDebug.ForcePlayAudio();
            UpdateStatus();
        }
        else if (audioSource != null)
        {
            // Fallback if debug component not available
            audioSource.Stop();
            audioSource.volume = 1.0f;
            audioSource.spatialBlend = 0f;
            audioSource.Play();
            Debug.Log("Forced audio playback");
            UpdateStatus();
        }
    }
    
    private void OnVolumeChanged(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
            Debug.Log($"Changed audio volume to {volume:F2}");
            UpdateStatus();
        }
    }
    
    private void OnSpatialAudioToggled(bool isSpatial)
    {
        if (audioSource != null)
        {
            audioSource.spatialBlend = isSpatial ? 1f : 0f;
            Debug.Log($"Changed spatial audio to {isSpatial}");
            UpdateStatus();
        }
    }
}