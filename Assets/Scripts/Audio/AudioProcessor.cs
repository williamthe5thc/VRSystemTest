using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using VRInterview; // Added namespace reference for PlatformManager

/// <summary>
/// Handles audio format conversion and processing for the interview system.
/// Enhanced with cross-platform compatibility for Mac and AMD GPU systems.
/// </summary>
public class AudioProcessor : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private int sampleRate = 16000;
    [SerializeField] private string audioFormat = "wav"; // Options: wav, mp3, webm
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true; // Enable debug by default
    [SerializeField] private bool saveAudioFiles = true; // Enable saving audio files for debugging
    [SerializeField] private string saveDirectory = "AudioDebug";
    
    private int _captureCount = 0;
    private int _responseCount = 0;
    private int _platformOptimalSampleRate;
    private bool _isInitialized = false;
    
    private void Start()
    {
        Initialize();
    }
    
    /// <summary>
    /// Initializes the audio processor with platform-specific settings.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized) return;
        
        // Get platform-specific settings
        if (PlatformManager.Instance != null)
        {
            _platformOptimalSampleRate = PlatformManager.Instance.GetOptimalSampleRate();
            
            // Use platform-recommended sample rate if one wasn't explicitly set
            if (sampleRate <= 0)
            {
                sampleRate = _platformOptimalSampleRate;
            }
            
            Debug.Log($"Platform optimal sample rate: {_platformOptimalSampleRate} Hz, Using: {sampleRate} Hz");
        }
        else
        {
            Debug.LogWarning("PlatformManager not available. Using default sample rate.");
        }
        
        // Create save directory if needed
        if (saveAudioFiles)
        {
            CreateSaveDirectory();
        }
        
        Debug.Log($"AudioProcessor initialized with sample rate: {sampleRate}, format: {audioFormat}");
        Debug.Log($"Save audio files: {saveAudioFiles}, directory: {Path.Combine(Application.persistentDataPath, saveDirectory)}");
        
        _isInitialized = true;
    }
    
    /// <summary>
    /// Processes an AudioClip for sending to the server.
    /// </summary>
    /// <param name="clip">The audio clip to process.</param>
    /// <returns>Processed audio data as byte array.</returns>
    public byte[] ProcessAudioForServer(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("Cannot process null AudioClip");
            return new byte[0];
        }
        
        if (!_isInitialized)
        {
            Initialize();
        }
        
        try
        {
            Debug.Log($"Processing audio clip: {clip.length} seconds, {clip.samples} samples, {clip.channels} channels, {clip.frequency}Hz");
            
            // Check if the clip actually contains audio data
            float[] sampleData = new float[clip.samples * clip.channels];
            clip.GetData(sampleData, 0);
            
            float maxSample = 0f;
            for (int i = 0; i < sampleData.Length; i++)
            {
                if (Mathf.Abs(sampleData[i]) > maxSample)
                    maxSample = Mathf.Abs(sampleData[i]);
            }
            
            Debug.Log($"Audio clip max amplitude: {maxSample}");
            if (maxSample < 0.01f)
            {
                Debug.LogWarning("Audio clip contains very low amplitude. May result in poor transcription.");
            }
            
            // Check if resampling is needed
            AudioClip processedClip = clip;
            if (clip.frequency != sampleRate)
            {
                Debug.Log($"Resampling from {clip.frequency}Hz to {sampleRate}Hz");
                processedClip = ResampleAudioClip(clip, sampleRate);
            }
            
            // Convert AudioClip to byte array in the specified format
            byte[] audioData;
            
            // For simplicity and reliability, always use WAV format
            audioData = AudioClipToWav(processedClip);
            
            // Save debug copy if enabled
            if (saveAudioFiles)
            {
                SaveAudioFile(audioData, $"capture_{_captureCount++}.wav");
            }
            
            Debug.Log($"Processed audio: {audioData.Length} bytes, format: WAV");
            
            return audioData;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing audio: {ex.Message}");
            return new byte[0];
        }
    }
    
    /// <summary>
    /// Resamples an AudioClip to a different sample rate
    /// Critical for Mac compatibility since macOS often uses 44.1kHz or 48kHz
    /// </summary>
    public AudioClip ResampleAudioClip(AudioClip clip, int targetSampleRate)
    {
        if (clip == null) return null;
        
        // Check if resampling is needed
        if (clip.frequency == targetSampleRate)
        {
            return clip;
        }
        
        Debug.Log($"Resampling audio from {clip.frequency}Hz to {targetSampleRate}Hz");
        
        // Get original clip data
        float[] originalData = new float[clip.samples * clip.channels];
        clip.GetData(originalData, 0);
        
        // Calculate resampling ratio
        float ratio = (float)targetSampleRate / clip.frequency;
        
        // Calculate new sample count
        int newSampleCount = Mathf.CeilToInt(clip.samples * ratio);
        
        // Create resampled data array
        float[] resampledData = new float[newSampleCount];
        
        // Simple linear interpolation resampling
        // Note: For production, consider using a more sophisticated algorithm
        for (int i = 0; i < newSampleCount; i++)
        {
            // Calculate the position in the original array
            float position = i / ratio;
            int index1 = Mathf.FloorToInt(position);
            int index2 = Mathf.Min(index1 + 1, clip.samples - 1);
            
            // Get interpolation factor
            float factor = position - index1;
            
            // Linear interpolation
            resampledData[i] = Mathf.Lerp(originalData[index1], originalData[index2], factor);
        }
        
        // Create new AudioClip
        AudioClip resampledClip = AudioClip.Create(
            $"{clip.name}_resampled",
            newSampleCount,
            1,  // Always mono for our use case
            targetSampleRate,
            false
        );
        
        // Set data
        resampledClip.SetData(resampledData, 0);
        
        return resampledClip;
    }
    
    /// <summary>
    /// Converts AudioClip to WAV byte array with platform-specific optimizations
    /// </summary>
    public byte[] AudioClipToWav(AudioClip clip)
    {
        // Get platform information
        bool isMac = false;
        if (PlatformManager.Instance != null)
        {
            isMac = PlatformManager.Instance.CurrentPlatform == PlatformManager.Platform.MacOS;
        }
        
        // Get samples
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);
        
        // Convert to 16-bit PCM
        Int16[] intData = new Int16[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * 32767);
        }
        
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // Write WAV header
                // "RIFF" chunk descriptor
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + intData.Length * 2); // File size
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                
                // "fmt " sub-chunk
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16); // Sub-chunk size
                writer.Write((short)1); // Audio format (1 = PCM)
                writer.Write((short)1); // Channels (1 = mono)
                writer.Write(clip.frequency); // Sample rate
                writer.Write(clip.frequency * 2); // Byte rate
                writer.Write((short)2); // Block align
                writer.Write((short)16); // Bits per sample
                
                // "data" sub-chunk
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(intData.Length * 2); // Sub-chunk size
                
                // Write audio data
                foreach (short sample in intData)
                {
                    writer.Write(sample);
                }
            }
            
            return stream.ToArray();
        }
    }
    
    /// <summary>
    /// Converts raw audio data back to an AudioClip for playback.
    /// </summary>
    /// <param name="audioData">The raw audio data.</param>
    /// <returns>An AudioClip for playback.</returns>
    public AudioClip ConvertToAudioClip(byte[] audioData)
    {
        if (audioData == null || audioData.Length == 0)
        {
            Debug.LogError("Cannot convert empty audio data");
            return null;
        }
        
        if (!_isInitialized)
        {
            Initialize();
        }
        
        try
        {
            Debug.Log($"Converting {audioData.Length} bytes of audio data to AudioClip");
            
            // Save debug copy if enabled
            if (saveAudioFiles)
            {
                SaveAudioFile(audioData, $"response_{_responseCount++}.wav");
            }
            
            // WAV data can be directly converted to AudioClip
            AudioClip clip = ConvertWAVToAudioClip(audioData);
            
            // Check if resampling is needed for playback on this platform
            if (clip != null && clip.frequency != AudioSettings.outputSampleRate)
            {
                Debug.Log($"Resampling audio for playback from {clip.frequency}Hz to {AudioSettings.outputSampleRate}Hz");
                clip = ResampleAudioClip(clip, AudioSettings.outputSampleRate);
            }
            
            return clip;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting to AudioClip: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Coroutine for loading audio clip from a file path.
    /// </summary>
    /// <param name="path">Path to the audio file.</param>
    /// <param name="callback">Callback for when the AudioClip is loaded.</param>
    private System.Collections.IEnumerator LoadAudioClipCoroutine(string path, Action<AudioClip> callback)
    {
        string url = "file://" + path;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                callback(clip);
            }
            else
            {
                Debug.LogError($"Failed to load audio: {www.error}");
                callback(null);
            }
        }
    }
    
    /// <summary>
    /// Converts WAV data directly to an AudioClip.
    /// Enhanced with platform-specific handling for Mac compatibility.
    /// </summary>
    /// <param name="wavData">The WAV audio data.</param>
    /// <returns>An AudioClip created from WAV data.</returns>
    private AudioClip ConvertWAVToAudioClip(byte[] wavData)
    {
        try {
            // Add more robust error handling and logging
            if (wavData.Length < 44) {
                Debug.LogError($"WAV data too short: {wavData.Length} bytes (needs at least 44 bytes for header)");
                return null;
            }
            
            // Parse WAV header
            int channels = BitConverter.ToInt16(wavData, 22);
            int sampleRate = BitConverter.ToInt32(wavData, 24);
            int bitsPerSample = BitConverter.ToInt16(wavData, 34);
            
            Debug.Log($"Parsing WAV data: {channels} channels, {sampleRate}Hz, {bitsPerSample} bits per sample, total size: {wavData.Length} bytes");
            
            // Find data chunk
            int dataIndex = 0;
            for (int i = 0; i < wavData.Length - 4; i++)
            {
                if (wavData[i] == 'd' && wavData[i + 1] == 'a' && wavData[i + 2] == 't' && wavData[i + 3] == 'a')
                {
                    dataIndex = i + 8; // Start of data after "data" and chunk size
                    Debug.Log($"Found 'data' chunk at index {i}, data starts at {dataIndex}");
                    break;
                }
            }
            
            if (dataIndex == 0 || dataIndex >= wavData.Length)
            {
                Debug.LogError("Failed to parse WAV: data chunk not found or invalid position");
                return null;
            }
            
            // Convert samples
            int bytesPerSample = bitsPerSample / 8;
            int sampleCount = (wavData.Length - dataIndex) / bytesPerSample;
            float[] samples = new float[sampleCount];
            
            Debug.Log($"Sample count: {sampleCount}, bytes per sample: {bytesPerSample}");
            
            for (int i = 0; i < sampleCount && (dataIndex + i * bytesPerSample) < wavData.Length; i++)
            {
                if (bitsPerSample == 16)
                {
                    short sample = BitConverter.ToInt16(wavData, dataIndex + i * bytesPerSample);
                    samples[i] = sample / 32768f;
                }
                else if (bitsPerSample == 8)
                {
                    samples[i] = (wavData[dataIndex + i] - 128) / 128f;
                }
            }
            
            // Create AudioClip
            AudioClip clip = AudioClip.Create("LoadedClip", samples.Length / channels, channels, sampleRate, false);
            clip.SetData(samples, 0);
            
            Debug.Log($"Successfully created AudioClip from WAV data: {samples.Length} samples");
            
            return clip;
        }
        catch (Exception ex) {
            Debug.LogError($"Error parsing WAV data: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }
    
    /// <summary>
    /// Creates directory for saving debug audio files.
    /// </summary>
    private void CreateSaveDirectory()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, saveDirectory);
        
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
            Debug.Log($"Created audio debug directory: {fullPath}");
        }
    }
    
    /// <summary>
    /// Saves audio file for debugging.
    /// </summary>
    /// <param name="audioData">The audio data to save.</param>
    /// <param name="filename">The filename to use.</param>
    private void SaveAudioFile(byte[] audioData, string filename)
    {
        try
        {
            string fullPath = Path.Combine(Application.persistentDataPath, saveDirectory, filename);
            File.WriteAllBytes(fullPath, audioData);
            
            Debug.Log($"Saved audio debug file: {fullPath} ({audioData.Length} bytes)");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving audio file: {ex.Message}");
        }
    }
}