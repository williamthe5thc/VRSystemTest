using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Direct audio testing utility that bypasses the normal audio chain
/// Use this to verify that basic audio functionality works
/// </summary>
public class DirectAudioTest : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip testClip; // Assign in inspector or will generate tone
    [SerializeField] private bool generateToneIfNoClip = true;
    [SerializeField] private float volume = 1.0f;
    
    [Header("UI Controls")]
    [SerializeField] private Button playButton;
    [SerializeField] private Text statusText;
    
    // Private audio source that doesn't depend on other scripts
    private AudioSource _dedicatedAudioSource;
    
    private void Start()
    {
        // Create our own dedicated audio source
        _dedicatedAudioSource = gameObject.AddComponent<AudioSource>();
        _dedicatedAudioSource.playOnAwake = false;
        _dedicatedAudioSource.spatialBlend = 0f; // Pure 2D audio
        _dedicatedAudioSource.volume = volume;
        
        // Generate a test tone if needed
        if (testClip == null && generateToneIfNoClip)
        {
            testClip = GenerateAudioClip();
            Debug.Log("Generated test tone audio clip");
        }
        
        // Set up UI if available
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayTestSound);
        }
        
        // Update status
        UpdateStatus("Ready to test audio");
    }
    
    /// <summary>
    /// Plays the test sound
    /// </summary>
    public void PlayTestSound()
    {
        if (_dedicatedAudioSource == null)
        {
            Debug.LogError("Audio source not initialized");
            UpdateStatus("ERROR: Audio source not initialized");
            return;
        }
        
        if (testClip == null)
        {
            Debug.LogError("No test clip available");
            UpdateStatus("ERROR: No test clip available");
            return;
        }
        
        // Stop any playing audio
        if (_dedicatedAudioSource.isPlaying)
        {
            _dedicatedAudioSource.Stop();
        }
        
        // Force settings again to be sure
        _dedicatedAudioSource.spatialBlend = 0f;
        _dedicatedAudioSource.volume = volume;
        _dedicatedAudioSource.clip = testClip;
        
        // Play the sound
        _dedicatedAudioSource.Play();
        
        Debug.Log($"Playing test sound. Volume: {volume}, Duration: {testClip.length}s");
        UpdateStatus("Playing test sound...");
        
        // After the sound completes
        StartCoroutine(ResetStatusAfterPlay());
    }
    
    private IEnumerator ResetStatusAfterPlay()
    {
        // Wait for the clip to finish
        if (testClip != null)
        {
            yield return new WaitForSeconds(testClip.length + 0.1f);
        }
        else
        {
            yield return new WaitForSeconds(2.0f);
        }
        
        // Check if it actually played
        if (!_dedicatedAudioSource.isPlaying)
        {
            Debug.Log("Test sound completed");
            UpdateStatus("Test completed. Did you hear any sound?");
        }
        else
        {
            Debug.LogWarning("Audio still playing after expected duration");
            UpdateStatus("Audio still playing? Check console");
        }
    }
    
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"DirectAudioTest: {message}");
    }
    
    /// <summary>
    /// Generates a simple audio clip for testing
    /// </summary>
    private AudioClip GenerateAudioClip()
    {
        int sampleRate = 44100;
        float frequency = 440f; // A4 note
        float duration = 1.0f;
        
        AudioClip clip = AudioClip.Create("TestTone", (int)(sampleRate * duration), 1, sampleRate, false);
        
        float[] samples = new float[(int)(sampleRate * duration)];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / sampleRate;
            // Generate a sine wave tone that fades in and out
            float envelope = Mathf.Clamp01(Mathf.Min(t * 4, (duration - t) * 4));
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope;
        }
        
        clip.SetData(samples, 0);
        return clip;
    }
}