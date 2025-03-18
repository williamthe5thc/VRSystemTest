using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles audio format conversion and processing for the interview system.
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
    
    private void Start()
    {
        // Create save directory if needed
        if (saveAudioFiles)
        {
            CreateSaveDirectory();
        }
        
        Debug.Log($"AudioProcessor initialized with sample rate: {sampleRate}, format: {audioFormat}");
        Debug.Log($"Save audio files: {saveAudioFiles}, directory: {Path.Combine(Application.persistentDataPath, saveDirectory)}");
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
        
        try
        {
            Debug.Log($"Processing audio clip: {clip.length} samples, {clip.channels} channels, {clip.frequency}Hz");
            
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
            
            // Convert AudioClip to byte array in the specified format
            byte[] audioData;
            
            // For simplicity and reliability, always use WAV format
            audioData = ConvertToWAV(clip);
            
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
    /// Converts an AudioClip to WAV format.
    /// </summary>
    /// <param name="clip">The audio clip to convert.</param>
    /// <returns>WAV audio data as byte array.</returns>
    private byte[] ConvertToWAV(AudioClip clip)
    {
        // Get audio data
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        
        Debug.Log($"Converting AudioClip to WAV: {samples.Length} samples, {clip.channels} channels, {clip.frequency}Hz");
        
        // Create WAV file bytes
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                // WAV header
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + samples.Length * 2);
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1); // PCM format
                writer.Write((short)clip.channels); // Channels
                writer.Write(clip.frequency); // Sample rate
                writer.Write(clip.frequency * clip.channels * 2); // Bytes per second
                writer.Write((short)(clip.channels * 2)); // Block align
                writer.Write((short)16); // Bits per sample
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(samples.Length * 2);
                
                // Convert and write sample data
                foreach (float sample in samples)
                {
                    writer.Write((short)(sample * 32767));
                }
            }
            
            byte[] result = ms.ToArray();
            Debug.Log($"WAV conversion complete: {result.Length} bytes");
            return result;
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
        
        try
        {
            Debug.Log($"Converting {audioData.Length} bytes of audio data to AudioClip");
            
            // Save debug copy if enabled
            if (saveAudioFiles)
            {
                SaveAudioFile(audioData, $"response_{_responseCount++}.wav");
            }
            
            // WAV data can be directly converted to AudioClip
            return ConvertWAVToAudioClip(audioData);
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