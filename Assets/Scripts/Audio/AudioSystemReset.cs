using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

/// <summary>
/// Performs a complete reset of all audio systems to recover from common issues
/// </summary>
public class AudioSystemReset : MonoBehaviour
{
    [Header("Components to Reset")]
    [SerializeField] private bool resetAudioPlayback = true;
    [SerializeField] private bool resetAudioSources = true;
    [SerializeField] private bool resetAudioListener = true;
    
    [Header("Debug")]
    [SerializeField] private bool logDebugInfo = true;
    
    private void Start()
    {
        // Perform reset after a short delay to let everything initialize
        StartCoroutine(DelayedReset(1.0f));
    }
    
    /// <summary>
    /// Resets all audio systems immediately
    /// </summary>
    public void ResetAudioSystemsNow()
    {
        LogDebug("Starting audio system reset");
        
        if (resetAudioListener)
        {
            ResetAudioListener();
        }
        
        if (resetAudioSources)
        {
            ResetAllAudioSources();
        }
        
        if (resetAudioPlayback)
        {
            ResetAudioPlayback();
        }
        
        LogDebug("Audio system reset complete");
    }
    
    private IEnumerator DelayedReset(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetAudioSystemsNow();
    }
    
    private void ResetAudioListener()
    {
        // Find main AudioListener
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        
        if (listeners.Length == 0)
        {
            LogDebug("No AudioListener found in scene! Adding one to main camera");
            
            // Try to add to main camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.GetComponent<AudioListener>() == null)
            {
                mainCamera.gameObject.AddComponent<AudioListener>();
                LogDebug("Added AudioListener to main camera");
            }
            else
            {
                LogDebug("Could not find main camera to add AudioListener");
            }
        }
        else if (listeners.Length > 1)
        {
            LogDebug($"Found {listeners.Length} AudioListeners - should only have 1!");
            
            // Disable all but the first
            for (int i = 1; i < listeners.Length; i++)
            {
                listeners[i].enabled = false;
                LogDebug($"Disabled extra AudioListener on {listeners[i].gameObject.name}");
            }
        }
        else
        {
            LogDebug($"AudioListener found on {listeners[0].gameObject.name}");
        }
    }
    
    private void ResetAllAudioSources()
    {
        AudioSource[] sources = FindObjectsOfType<AudioSource>();
        LogDebug($"Found {sources.Length} AudioSource components");
        
        foreach (var source in sources)
        {
            // Reset common issues
            if (source.mute)
            {
                source.mute = false;
                LogDebug($"Unmuted AudioSource on {source.gameObject.name}");
            }
            
            if (source.volume < 0.5f)
            {
                source.volume = 1.0f;
                LogDebug($"Set volume to 1.0 on {source.gameObject.name}");
            }
            
            if (source.spatialBlend > 0f)
            {
                source.spatialBlend = 0f;
                LogDebug($"Set spatialBlend to 0 on {source.gameObject.name}");
            }
        }
        
        // Special handling for AudioPlayback
        AudioPlayback[] playbacks = FindObjectsOfType<AudioPlayback>();
        foreach (var playback in playbacks)
        {
            AudioSource source = playback.GetComponent<AudioSource>();
            if (source == null)
            {
                source = playback.gameObject.AddComponent<AudioSource>();
                LogDebug($"Added missing AudioSource to {playback.gameObject.name}");
            }
            
            // Ensure it's properly configured
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D sound
            source.volume = 1.0f;
            source.priority = 0; // Highest priority
            
            LogDebug($"Reconfigured AudioSource on {playback.gameObject.name}");
        }
    }
    
    private void ResetAudioPlayback()
    {
        AudioPlayback playback = FindObjectOfType<AudioPlayback>();
        if (playback == null)
        {
            LogDebug("No AudioPlayback component found");
            return;
        }
        
        LogDebug($"Found AudioPlayback on {playback.gameObject.name}");
        
        // Try to fix via AudioPlaybackFix
        AudioPlaybackFix fix = playback.GetComponent<AudioPlaybackFix>();
        if (fix != null)
        {
            fix.FixNow();
            LogDebug("Applied AudioPlaybackFix");
        }
        else
        {
            LogDebug("No AudioPlaybackFix component found");
        }
        
        // Force the Volume property 
        try
        {
            // Use reflection to set volume property
            var volumeProperty = typeof(AudioPlayback).GetProperty("Volume");
            if (volumeProperty != null)
            {
                volumeProperty.SetValue(playback, 1.0f);
                LogDebug("Set AudioPlayback.Volume property to 1.0");
            }
        }
        catch (System.Exception ex)
        {
            LogDebug($"Error setting Volume property: {ex.Message}");
        }
    }
    
    private void LogDebug(string message)
    {
        if (logDebugInfo)
        {
            Debug.Log($"AudioSystemReset: {message}");
        }
    }
}