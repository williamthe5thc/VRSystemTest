using System;
using System.Collections;
using UnityEngine;
using VRInterview.Network;

/// <summary>
/// Handles playback of audio responses from the server.
/// </summary>
public class AudioPlayback : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioProcessor audioProcessor;
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private AvatarController avatarController;
    
    [Header("Playback Settings")]
    [SerializeField] private float defaultVolume = 0.8f;
    [SerializeField] private bool spatialAudio = true;
    [SerializeField] private float spatialBlend = 1.0f;
    [SerializeField] private float streamingVolume = 1.0f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Events
    public event Action OnPlaybackStarted;
    public event Action OnPlaybackCompleted;
    public event Action<float> OnPlaybackProgress;
    public event Action<string> OnPlaybackError;
    
    // Properties
    public bool IsPlaying => audioSource != null && audioSource.isPlaying;
    public float Volume
    {
        get => audioSource != null ? audioSource.volume : 0f;
        set
        {
            if (audioSource != null)
            {
                audioSource.volume = Mathf.Clamp01(value);
                
                // Save to settings
                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.SetSetting("Volume", audioSource.volume);
                    SettingsManager.Instance.SaveSettings();
                }
            }
        }
    }
    
    private void Start()
    {
        InitializeAudioSource();
    }
    
    /// <summary>
    /// Initializes the audio source component.
    /// </summary>
    private void InitializeAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure audio source
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        // Load volume from settings
        if (SettingsManager.Instance != null)
        {
            float savedVolume = SettingsManager.Instance.GetSetting<float>("Volume");
            if (savedVolume > 0)
            {
                audioSource.volume = savedVolume;
            }
            else
            {
                audioSource.volume = defaultVolume;
            }
            
            // Load streaming volume
            if (SettingsManager.Instance != null)
            {
                streamingVolume = SettingsManager.Instance.GetSetting<float>("StreamingVolume");
                if (streamingVolume <= 0)
                {
                    streamingVolume = 1.0f; // Default value
                }
            }
        }
        else
        {
            audioSource.volume = defaultVolume;
        }
        
        // Configure spatial audio if enabled
        if (spatialAudio)
        {
            audioSource.spatialBlend = spatialBlend;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
        }
        else
        {
            audioSource.spatialBlend = 0f;
        }
    }
    
    /// <summary>
    /// Plays an audio response from raw audio data.
    /// </summary>
    /// <param name="audioData">The raw audio data to play.</param>
    public void PlayAudioResponse(byte[] audioData)
    {
        if (audioData == null || audioData.Length == 0)
        {
            Debug.LogError("Cannot play empty audio data");
            OnPlaybackError?.Invoke("Cannot play empty audio data");
            return;
        }
        
        if (audioProcessor == null)
        {
            Debug.LogError("AudioProcessor not assigned to AudioPlayback");
            OnPlaybackError?.Invoke("Audio processor not available");
            return;
        }
        
        StartCoroutine(PlayAudioResponseCoroutine(audioData));
    }
    
    /// <summary>
    /// Plays a streamed audio clip.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    public void PlayStreamedAudio(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("Cannot play null audio clip");
            OnPlaybackError?.Invoke("Invalid audio clip");
            return;
        }
        
        StartCoroutine(PlayStreamedAudioCoroutine(clip));
    }
    
    /// <summary>
    /// Coroutine for playing audio response and tracking progress.
    /// </summary>
    /// <param name="audioData">The raw audio data to play.</param>
    private IEnumerator PlayAudioResponseCoroutine(byte[] audioData)
    {
        // Convert audio data to playable format
        AudioClip clip = null;
        
        try
        {
            clip = audioProcessor.ConvertToAudioClip(audioData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting audio data: {ex.Message}");
            OnPlaybackError?.Invoke($"Error converting audio: {ex.Message}");
            yield break;
        }
        
        if (clip == null)
        {
            Debug.LogError("Failed to convert audio data to clip");
            OnPlaybackError?.Invoke("Failed to convert audio data");
            yield break;
        }
        
        if (debugMode)
        {
            Debug.Log($"Playing audio clip: {clip.length:F2}s, {clip.frequency}Hz, {clip.channels} channels");
        }
        
        // Play the audio
        audioSource.clip = clip;
        audioSource.Play();
        
        // Notify playback started
        OnPlaybackStarted?.Invoke();
        
        // Notify avatar if available
        if (avatarController != null)
        {
            avatarController.OnAudioPlaybackStarted();
        }
        
        // Wait for audio to finish playing
        float duration = clip.length;
        float elapsed = 0f;
        
        while (elapsed < duration && audioSource.isPlaying)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Update progress
            OnPlaybackProgress?.Invoke(progress);
            
            // Update avatar lip sync if available
            if (avatarController != null)
            {
                avatarController.UpdateLipSync(elapsed / duration);
            }
            
            yield return null;
        }
        
        // Audio playback complete
        OnPlaybackCompleted?.Invoke();
        
        // Notify avatar
        if (avatarController != null)
        {
            avatarController.OnAudioPlaybackCompleted();
        }
        
        // Notify server that playback is complete
        if (sessionManager != null)
        {
            sessionManager.NotifyPlaybackComplete();
        }
        
        if (debugMode)
        {
            Debug.Log("Audio playback completed");
        }
    }
    
    /// <summary>
    /// Coroutine for playing streamed audio.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    private IEnumerator PlayStreamedAudioCoroutine(AudioClip clip)
    {
        // Stop any playing audio
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        if (debugMode)
        {
            Debug.Log($"Playing streamed audio clip: {clip.name}, {clip.length:F2}s, {clip.frequency}Hz, {clip.channels} channels, state: {clip.loadState}");
        }
        
        // Verify clip is valid
        if (clip == null || clip.length <= 0)
        {
            Debug.LogError("Invalid audio clip: null or zero length");
            OnPlaybackError?.Invoke("Invalid audio clip");
            yield break;
        }
        
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null. Initializing...");
            InitializeAudioSource();
            
            if (audioSource == null)
            {
                Debug.LogError("Failed to initialize AudioSource");
                OnPlaybackError?.Invoke("Audio playback system unavailable");
                yield break;
            }
        }
        
        // Store original volume and set streaming volume
        float originalVolume = audioSource.volume;
        
        bool playbackStarted = false;
        try
        {
            audioSource.volume = streamingVolume;
            
            // Set the clip before trying to play
            audioSource.clip = clip;
            
            if (audioSource.clip == null)
            {
                Debug.LogError("Failed to assign audio clip to AudioSource");
                OnPlaybackError?.Invoke("Failed to assign audio clip");
                yield break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error preparing audio clip: {ex.Message}\nStack Trace: {ex.StackTrace}");
            OnPlaybackError?.Invoke(ex.Message);
            yield break;
        }
            
        // Play the audio with safety check (outside of try/catch to allow yielding)
        if (clip.loadState == AudioDataLoadState.Loaded)
        {
            try
            {
                audioSource.Play();
                playbackStarted = true;
                Debug.Log($"Audio playback started for clip: {clip.name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error playing audio: {ex.Message}\nStack Trace: {ex.StackTrace}");
                OnPlaybackError?.Invoke(ex.Message);
                yield break;
            }
        }
        else
        {
            Debug.LogWarning($"Clip not fully loaded. State: {clip.loadState}. Waiting...");
            
            // Wait for clip to load (up to 3 seconds)
            float startTime = Time.time;
            while (clip.loadState != AudioDataLoadState.Loaded && Time.time - startTime < 3.0f)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            if (clip.loadState == AudioDataLoadState.Loaded)
            {
                try
                {
                    audioSource.Play();
                    playbackStarted = true;
                    Debug.Log("Audio playback started after waiting for clip to load");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error playing audio after wait: {ex.Message}\nStack Trace: {ex.StackTrace}");
                    OnPlaybackError?.Invoke(ex.Message);
                    yield break;
                }
            }
            else
            {
                Debug.LogError($"Clip failed to load after waiting. State: {clip.loadState}");
                OnPlaybackError?.Invoke("Audio failed to load");
                yield break;
            }
        }
            
        // Only notify if playback actually started
        if (playbackStarted)
        {
            try
            {
                // Notify playback started
                OnPlaybackStarted?.Invoke();
                
                // Notify avatar if available
                if (avatarController != null)
                {
                    avatarController.OnAudioPlaybackStarted();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in playback notifications: {ex.Message}");
                // Continue anyway since audio is playing
            }
        }
        
        // Wait for audio to finish playing in a separate try block
        float duration = clip.length;
        float elapsed = 0f;
        
        while (elapsed < duration && audioSource.isPlaying)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Update progress
            OnPlaybackProgress?.Invoke(progress);
            
            // Update avatar lip sync if available
            if (avatarController != null)
            {
                avatarController.UpdateLipSync(elapsed / duration);
            }
            
            yield return null;
        }
        
        // Restore original volume
        audioSource.volume = originalVolume;
        
        try
        {
            // Audio playback complete
            OnPlaybackCompleted?.Invoke();
            
            // Notify avatar
            if (avatarController != null)
            {
                avatarController.OnAudioPlaybackCompleted();
            }
            
            // Notify server that playback is complete
            if (sessionManager != null)
            {
                sessionManager.NotifyPlaybackComplete();
            }
            
            if (debugMode)
            {
                Debug.Log("Streamed audio playback completed");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error completing streamed audio playback: {ex.Message}");
            OnPlaybackError?.Invoke(ex.Message);
        }
    }
    
    /// <summary>
    /// Stops the current audio playback.
    /// </summary>
    public void StopPlayback()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            
            // Notify completion
            OnPlaybackCompleted?.Invoke();
            
            // Notify avatar
            if (avatarController != null)
            {
                avatarController.OnAudioPlaybackCompleted();
            }
            
            // Notify server
            if (sessionManager != null)
            {
                sessionManager.NotifyPlaybackComplete();
            }
            
            if (debugMode)
            {
                Debug.Log("Audio playback stopped manually");
            }
        }
    }
    
    /// <summary>
    /// Pauses the current audio playback.
    /// </summary>
    public void PausePlayback()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            
            if (debugMode)
            {
                Debug.Log("Audio playback paused");
            }
        }
    }
    
    /// <summary>
    /// Resumes the paused audio playback.
    /// </summary>
    public void ResumePlayback()
    {
        if (audioSource != null && !audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.UnPause();
            
            if (debugMode)
            {
                Debug.Log("Audio playback resumed");
            }
        }
    }
    
    /// <summary>
    /// Sets the volume for streamed audio.
    /// </summary>
    /// <param name="volume">Volume level between 0 and 1.</param>
    public void SetStreamingVolume(float volume)
    {
        streamingVolume = Mathf.Clamp01(volume);
        
        // Save to settings
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetSetting("StreamingVolume", streamingVolume);
            SettingsManager.Instance.SaveSettings();
        }
        
        if (debugMode)
        {
            Debug.Log($"Streaming volume set to: {streamingVolume}");
        }
    }
    
    /// <summary>
    /// Gets the current streaming volume.
    /// </summary>
    /// <returns>The streaming volume level.</returns>
    public float GetStreamingVolume()
    {
        // Load from settings if available
        if (SettingsManager.Instance != null)
        {
            float savedVolume = SettingsManager.Instance.GetSetting<float>("StreamingVolume");
            if (savedVolume > 0)
            {
                streamingVolume = savedVolume;
            }
        }
        
        return streamingVolume;
    }
}