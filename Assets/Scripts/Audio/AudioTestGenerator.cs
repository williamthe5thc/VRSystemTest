using UnityEngine;

/// <summary>
/// Generates test audio tones to verify audio system functionality
/// </summary>
public class AudioTestGenerator : MonoBehaviour
{
    [Header("References")]
    public AudioSource targetAudioSource;
    
    [Header("Test Tone Settings")]
    [SerializeField] private float frequency = 440f; // A4 note
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private float volume = 1.0f;
    
    private AudioClip _testTone;
    
    private void Start()
    {
        // Find AudioSource if not assigned
        if (targetAudioSource == null)
        {
            targetAudioSource = GetComponent<AudioSource>();
            
            if (targetAudioSource == null)
            {
                targetAudioSource = FindObjectOfType<AudioPlayback>()?.GetComponent<AudioSource>();
                
                if (targetAudioSource == null)
                {
                    Debug.LogWarning("AudioTestGenerator: No AudioSource found");
                }
            }
        }
        
        // Generate test tone
        GenerateTestTone();
    }
    
    /// <summary>
    /// Generates a test tone AudioClip
    /// </summary>
    public void GenerateTestTone()
    {
        int sampleRate = AudioSettings.outputSampleRate;
        
        // Create clip
        _testTone = AudioClip.Create("TestTone", (int)(sampleRate * duration), 1, sampleRate, false);
        
        // Generate sine wave samples
        float[] samples = new float[(int)(sampleRate * duration)];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / sampleRate;
            // Generate a sine wave with fade in/out envelope
            float envelope = Mathf.Clamp01(Mathf.Min(t * 4, (duration - t) * 4));
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope * volume;
        }
        
        // Set data
        _testTone.SetData(samples, 0);
        
        Debug.Log($"AudioTestGenerator: Generated test tone at {frequency}Hz for {duration}s");
    }
    
    /// <summary>
    /// Plays the test tone through the target AudioSource
    /// </summary>
    public void PlayTestTone()
    {
        if (targetAudioSource == null)
        {
            Debug.LogError("AudioTestGenerator: Cannot play test tone - No AudioSource assigned");
            return;
        }
        
        if (_testTone == null)
        {
            GenerateTestTone();
        }
        
        // Save original audio source settings
        AudioClip originalClip = targetAudioSource.clip;
        float originalVolume = targetAudioSource.volume;
        bool wasPlaying = targetAudioSource.isPlaying;
        
        // Stop any current playback
        if (targetAudioSource.isPlaying)
        {
            targetAudioSource.Stop();
        }
        
        // Configure for test tone
        targetAudioSource.clip = _testTone;
        targetAudioSource.volume = volume;
        targetAudioSource.loop = false;
        targetAudioSource.spatialBlend = 0f; // Force 2D audio
        
        // Play the tone
        targetAudioSource.Play();
        
        Debug.Log("AudioTestGenerator: Playing test tone - you should hear a brief tone");
        
        // Restore original settings after playback
        Invoke("RestoreAudioSource", duration + 0.1f);
    }
    
    /// <summary>
    /// Restores the original AudioSource settings after test tone playback
    /// </summary>
    private void RestoreAudioSource()
    {
        if (targetAudioSource != null)
        {
            targetAudioSource.Stop();
            
            // Restore original properties if needed
        }
    }
}