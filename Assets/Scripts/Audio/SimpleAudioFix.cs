using UnityEngine;

/// <summary>
/// Very simple fix for audio playback issues - just attach to AudioPlayback GameObject
/// </summary>
public class SimpleAudioFix : MonoBehaviour
{
    private void Start()
    {
        // Get the AudioPlayback component
        AudioPlayback audioPlayback = GetComponent<AudioPlayback>();
        if (audioPlayback == null)
        {
            Debug.LogError("SimpleAudioFix: No AudioPlayback component found!");
            return;
        }
        
        // Get or create AudioSource
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("SimpleAudioFix: Added AudioSource component");
        }
        
        // Configure AudioSource for optimal playback
        audioSource.spatialBlend = 0f; // Set to 2D (non-spatial)
        audioSource.volume = 1.0f;     // Full volume
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.priority = 0;      // High priority
        
        Debug.Log("SimpleAudioFix: AudioSource configured for optimal playback");
        
        // Hook up events
        audioPlayback.OnPlaybackStarted += () => {
            Debug.Log("SimpleAudioFix: Audio playback started, ensuring audio source is ready");
            if (audioSource.spatialBlend > 0f)
            {
                audioSource.spatialBlend = 0f;
                Debug.Log("SimpleAudioFix: Fixed spatial blend");
            }
            
            if (audioSource.volume < 0.8f)
            {
                audioSource.volume = 1.0f;
                Debug.Log("SimpleAudioFix: Fixed volume");
            }
        };
    }
    
    public void ForcePlayAudio()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Stop();
            audioSource.spatialBlend = 0f; // Ensure 2D sound
            audioSource.volume = 1.0f;     // Ensure full volume
            audioSource.Play();
            Debug.Log("SimpleAudioFix: Forced audio playback");
        }
        else
        {
            Debug.LogError("SimpleAudioFix: Cannot force play audio - AudioSource or clip is missing");
        }
    }
}