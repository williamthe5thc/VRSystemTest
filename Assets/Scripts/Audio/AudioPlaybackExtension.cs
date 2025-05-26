using UnityEngine;
using System.Collections;

/// <summary>
/// Extension methods for AudioPlayback to ensure proper audio routing
/// </summary>
public static class AudioPlaybackExtension
{
    /// <summary>
    /// Ensures the AudioSource is properly configured for playback
    /// </summary>
    public static void EnsureAudioSourceSetup(this AudioPlayback audioPlayback)
    {
        if (audioPlayback == null) return;
        
        // Get or add AudioSource
        AudioSource audioSource = audioPlayback.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = audioPlayback.gameObject.AddComponent<AudioSource>();
            Debug.Log("Added AudioSource component to AudioPlayback");
        }
        
        // Configure AudioSource for optimal playback
        audioSource.spatialBlend = 0f; // Set to 2D audio (non-spatial)
        audioSource.volume = 1.0f;     // Set volume to maximum
        audioSource.playOnAwake = false; // Don't play automatically
        audioSource.loop = false;      // Don't loop
        
        // Set priority to high
        audioSource.priority = 0;
        
        Debug.Log("AudioSource configured for optimal playback");
    }
    
    /// <summary>
    /// Forces playback on the AudioSource
    /// </summary>
    public static void ForcePlay(this AudioPlayback audioPlayback)
    {
        if (audioPlayback == null) return;
        
        AudioSource audioSource = audioPlayback.GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Stop();
            audioSource.spatialBlend = 0f; // Ensure 2D audio
            audioSource.volume = 1.0f;     // Ensure full volume
            audioSource.Play();
            Debug.Log($"Forced playback of audio clip: {audioSource.clip.name}, Length: {audioSource.clip.length}s");
        }
        else
        {
            Debug.LogWarning("Cannot force play: AudioSource or clip is null");
        }
    }
}
