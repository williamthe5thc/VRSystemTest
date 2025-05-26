using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides manual restart functionality for audio playback
/// Add this component to a UI button that should restart audio
/// </summary>
public class AudioPlaybackRestarter : MonoBehaviour
{
    [SerializeField] private AudioPlayback audioPlayback;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Button restartButton;
    
    [Header("Auto-Find Settings")]
    [SerializeField] private bool findComponentsAutomatically = true;
    
    private void Start()
    {
        if (findComponentsAutomatically)
        {
            FindComponents();
        }
        
        // Set up button event
        if (restartButton == null)
        {
            restartButton = GetComponent<Button>();
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartAudio);
        }
    }
    
    private void FindComponents()
    {
        if (audioPlayback == null)
        {
            audioPlayback = FindObjectOfType<AudioPlayback>();
        }
        
        if (audioSource == null && audioPlayback != null)
        {
            audioSource = audioPlayback.GetComponent<AudioSource>();
        }
    }
    
    /// <summary>
    /// Call this method to manually restart audio playback
    /// </summary>
    public void RestartAudio()
    {
        Debug.Log("Attempting to restart audio playback");
        
        FindComponents();
        
        // First try using AudioPlaybackFix if available
        AudioPlaybackFix fix = null;
        if (audioPlayback != null)
        {
            fix = audioPlayback.GetComponent<AudioPlaybackFix>();
        }
        
        if (fix != null)
        {
            Debug.Log("Using AudioPlaybackFix to restart audio");
            fix.FixNow();
            return;
        }
        
        // If no fix component, try direct audio control
        if (audioSource != null && audioSource.clip != null)
        {
            Debug.Log("Directly restarting AudioSource playback");
            audioSource.Stop();
            audioSource.volume = 1.0f;
            audioSource.spatialBlend = 0.0f; // Force 2D audio for reliability
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Cannot restart audio: AudioSource or clip is null");
        }
    }
}