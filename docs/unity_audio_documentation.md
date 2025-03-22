# VR Interview System - Audio Documentation

## Architecture Overview

The audio subsystem in the VR Interview System handles the capture, processing, transmission, and playback of audio. It serves as the primary communication medium between the user and the virtual interviewer. The audio pipeline is designed to work efficiently within the constraints of VR environments while ensuring high-quality voice communication.

## Key Classes/Components

### MicrophoneCapture

`MicrophoneCapture` handles recording audio from the user's microphone, detecting voice activity, and preparing the audio data for transmission.

```csharp
public class MicrophoneCapture : MonoBehaviour
{
    // Core functionality
    public void StartRecording()
    public void StopRecording()
    private void ProcessRecordedAudio()
    private float[] GetMicrophoneData(int fromPosition, int toPosition)
    
    // Voice detection
    private float CalculateAudioLevel(float[] samples)
    
    // Events
    public event Action OnRecordingStarted;
    public event Action OnRecordingStopped;
    public event Action<float> OnAudioLevelChanged;
    
    // Properties
    public bool IsRecording => _isRecording;
    public float CurrentAudioLevel => _currentAudioLevel;
    public string[] AvailableMicrophones => Microphone.devices;
    public string SelectedMicrophone => _selectedMicrophone;
}
```

### AudioProcessor

`AudioProcessor` handles the preparation and formatting of audio data for the server, including sample rate conversion, compression, and format conversion.

```csharp
public class AudioProcessor : MonoBehaviour
{
    // Core functionality
    public byte[] ProcessAudioForServer(AudioClip clip)
    private byte[] ConvertToWavFormat(float[] samples, int sampleRate)
    private byte[] ApplyAudioCompression(byte[] audioData)
    
    // Audio quality monitoring
    public float GetSignalQuality(float[] samples)
    
    // Properties
    public int TargetSampleRate => targetSampleRate;
    public string TargetFormat => targetFormat;
}
```

### AudioPlayback

`AudioPlayback` manages the playback of audio responses from the server, including buffer management, synchronization with avatar animations, and audio quality settings.

```csharp
public class AudioPlayback : MonoBehaviour
{
    // Core functionality
    public void PlayAudioResponse(byte[] audioData)
    public void StopPlayback()
    private AudioClip CreateAudioClipFromWAV(byte[] wavData)
    
    // Playback control
    public void SetVolume(float volume)
    public void PausePlayback()
    public void ResumePlayback()
    
    // Events
    public event Action OnPlaybackStarted;
    public event Action OnPlaybackCompleted;
    public event Action<float> OnPlaybackProgress;
    
    // Properties
    public bool IsPlaying => _isPlaying;
    public float PlaybackPosition => _playbackPosition;
}
```

### AudioStreamer

`AudioStreamer` handles streaming audio to and from the server for real-time communication.

```csharp
public class AudioStreamer : MonoBehaviour
{
    // Stream management
    public void StartStreaming()
    public void StopStreaming()
    private void ProcessIncomingAudioPacket(byte[] audioData)
    
    // Buffer management
    private void ManageAudioBuffer()
    
    // Events
    public event Action<byte[]> OnAudioPacketReceived;
}
```

## Audio Pipeline

### Microphone Capture Implementation

The system captures audio from the microphone using Unity's `Microphone` class:

```csharp
// Start recording
_microphoneClip = Microphone.Start(_selectedMicrophone, true, recordingBufferLengthSec, sampleRate);

// Update in frames
int currentPosition = Microphone.GetPosition(_selectedMicrophone);
if (currentPosition != _lastSamplePosition)
{
    float[] samples = GetMicrophoneData(_lastSamplePosition, currentPosition);
    // Process samples...
    _lastSamplePosition = currentPosition;
}
```

Key features of the microphone capture system:

1. **Circular Buffer Management**: Handles Unity's circular microphone buffer correctly, including wrapping
2. **Device Selection**: Supports multiple microphone devices and remembers user preferences
3. **Voice Activity Detection**: Uses amplitude thresholds to detect when the user is speaking
4. **Auto-Stop**: Automatically stops recording after a silence threshold is reached
5. **Error Recovery**: Includes fallback mechanisms for microphone initialization failures

### Audio Playback System

The system plays back audio responses from the server using Unity's `AudioSource` component:

```csharp
public void PlayAudioResponse(byte[] audioData)
{
    try
    {
        // Create audio clip from WAV data
        AudioClip clip = CreateAudioClipFromWAV(audioData);
        
        if (clip != null)
        {
            // Configure audio source
            _audioSource.clip = clip;
            _audioSource.Play();
            
            _isPlaying = true;
            _playbackPosition = 0f;
            
            // Notify listeners
            OnPlaybackStarted?.Invoke();
            
            // Begin monitoring progress
            StartCoroutine(MonitorPlaybackProgress());
            
            Debug.Log($"Started playback of {audioData.Length} bytes of audio data");
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error playing audio response: {ex.Message}");
    }
}
```

Key features of the audio playback system:

1. **Format Conversion**: Converts server WAV data into Unity AudioClip format
2. **Playback Monitoring**: Tracks playback progress and exposes events
3. **Lip Sync Integration**: Coordinates with the avatar's lip sync system
4. **Error Handling**: Gracefully handles malformed audio data
5. **Notification System**: Notifies the server when playback completes

### Audio Streaming from Server

The system supports both chunked and streaming audio playback modes:

```csharp
private void HandleAudioResponse(JObject messageObj)
{
    try {
        // Extract base64 audio data
        string base64Audio = messageObj["data"]?.ToString();
        if (string.IsNullOrEmpty(base64Audio))
        {
            Debug.LogError("Received audio response with no data");
            return;
        }
        
        // Convert to binary
        byte[] audioData = Convert.FromBase64String(base64Audio);
        
        // Play the audio
        if (audioPlayback != null)
        {
            audioPlayback.PlayAudioResponse(audioData);
        }
        
        // Broadcast the audio response
        OnAudioResponse?.Invoke(audioData);
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error handling audio response: {ex.Message}");
    }
}
```

Streaming mode is handled through packet buffering and reassembly:

```csharp
// Streaming mode is implemented in AudioStreamer
private void ProcessIncomingAudioPacket(byte[] audioData)
{
    // Add packet to buffer
    _streamBuffer.AddRange(audioData);
    
    // Process buffer if it's large enough
    if (_streamBuffer.Count >= minBufferSize)
    {
        byte[] bufferSegment = _streamBuffer.GetRange(0, minBufferSize).ToArray();
        _streamBuffer.RemoveRange(0, minBufferSize);
        
        // Play buffer segment
        PlayBufferSegment(bufferSegment);
    }
}
```

### Voice Activity Detection

The system implements voice activity detection to determine when the user is speaking:

```csharp
private float CalculateAudioLevel(float[] samples)
{
    if (samples.Length == 0)
        return 0f;
    
    float sum = 0f;
    
    // Calculate RMS (Root Mean Square) of the audio samples
    for (int i = 0; i < samples.Length; i++)
    {
        sum += samples[i] * samples[i];
    }
    
    return Mathf.Sqrt(sum / samples.Length);
}

// In Update method
float audioLevel = CalculateAudioLevel(samples);
if (audioLevel < voiceDetectionThreshold)
{
    _silenceTimer += Time.deltaTime;
    
    // If silence lasts too long, stop recording
    if (_silenceTimer > silenceTimeoutSec)
    {
        StopRecording();
    }
}
else
{
    _silenceTimer = 0f;
}
```

### Audio Format Handling

The system supports various audio formats with a focus on WAV for server communication:

```csharp
private byte[] ConvertToWavFormat(float[] samples, int sampleRate)
{
    // WAV format constants
    const int HEADER_SIZE = 44;
    const int FORMAT_CHUNK_SIZE = 16;
    const int BITS_PER_SAMPLE = 16;
    
    // Calculate sizes
    int blockAlign = BITS_PER_SAMPLE / 8;
    int subchunkSize = samples.Length * blockAlign;
    int chunkSize = HEADER_SIZE + subchunkSize - 8;
    
    // Create buffer
    byte[] buffer = new byte[HEADER_SIZE + subchunkSize];
    
    // Write WAV header
    // ...header writing code...
    
    // Convert samples to 16-bit PCM
    for (int i = 0; i < samples.Length; i++)
    {
        short sampleValue = (short)(samples[i] * short.MaxValue);
        buffer[HEADER_SIZE + i * 2] = (byte)(sampleValue & 0xFF);
        buffer[HEADER_SIZE + i * 2 + 1] = (byte)((sampleValue >> 8) & 0xFF);
    }
    
    return buffer;
}
```

## Audio Communication with Server

### Audio Data Transmission

Audio is sent to the server in WAV format using WebSocket messages:

```csharp
public async Task SendAudioData(byte[] audioData, string sessionId)
{
    if (!IsConnected)
    {
        Debug.LogWarning("Cannot send audio: WebSocket not connected");
        await Connect();
        
        // If still not connected, abort
        if (!IsConnected)
            return;
    }
    
    try
    {
        // Create audio data message
        var audioMessage = new AudioDataMessage
        {
            type = "audio_data",
            session_id = sessionId,
            data = Convert.ToBase64String(audioData),
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
        };
        
        // Convert to JSON and send
        string jsonMessage = JsonUtility.ToJson(audioMessage);
        await _websocket.SendText(jsonMessage);
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error sending audio data: {ex.Message}");
        OnError?.Invoke(ex.Message);
    }
}
```

### Audio Response Handling

Audio responses from the server are parsed and played:

```csharp
private void HandleAudioResponse(JObject messageObj)
{
    try {
        // Extract base64 audio data
        string base64Audio = messageObj["data"]?.ToString();
        
        // Check for text response included with audio
        string textResponse = messageObj["text"]?.ToString();
        if (!string.IsNullOrEmpty(textResponse))
        {
            _currentLLMResponse = textResponse;
            
            // Show LLM response in UI
            if (uiManager != null)
            {
                uiManager.ShowLLMResponse(textResponse);
            }
        }
        
        // Convert to binary and play
        byte[] audioData = Convert.FromBase64String(base64Audio);
        if (audioPlayback != null)
        {
            audioPlayback.PlayAudioResponse(audioData);
        }
        
        // Broadcast the audio response
        OnAudioResponse?.Invoke(audioData);
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error handling audio response: {ex.Message}");
    }
}
```

### Playback Completion Notification

The client notifies the server when audio playback completes:

```csharp
public async void NotifyPlaybackComplete()
{
    if (!_isSessionActive || webSocketClient == null || !webSocketClient.IsConnected)
    {
        return;
    }
    
    try
    {
        var message = new PlaybackCompleteMessage
        {
            type = "playback_complete",
            session_id = _sessionId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
        };
        
        string jsonMessage = JsonConvert.SerializeObject(message);
        await webSocketClient.SendMessage(jsonMessage);
        
        Debug.Log("Sent playback complete notification");
        
        // Trigger response complete event for feedback system
        OnResponseComplete?.Invoke();
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error sending playback complete message: {ex.Message}");
    }
}
```

## Unity-Specific Implementation

### Microphone Access in Unity/VR

The system handles microphone access specifics for VR platforms:

```csharp
private void RequestPermissions()
{
    Debug.Log("Requesting necessary permissions...");
    
    // Request microphone permission for Oculus Quest
    #if PLATFORM_ANDROID
    if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
    {
        UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
    }
    #endif
}
```

Unity's built-in Microphone class is used with special handling for VR devices:

```csharp
// Check if microphone is available
if (Microphone.devices.Length == 0)
{
    Debug.LogError("No microphone found!");
    return;
}

// Get default microphone from settings or auto-select
if (SettingsManager.Instance != null)
{
    string savedMicrophone = SettingsManager.Instance.GetSetting<string>("Microphone");
    
    if (!string.IsNullOrEmpty(savedMicrophone) && Array.IndexOf(Microphone.devices, savedMicrophone) != -1)
    {
        _selectedMicrophone = savedMicrophone;
    }
    else if (autoSelectMicrophone)
    {
        // Use first microphone by default
        _selectedMicrophone = Microphone.devices[0];
    }
}
```

### Audio Buffering and Transmission

The system implements efficient audio buffering to maintain VR performance:

```csharp
private void AddSamplesToRecording(float[] samples)
{
    if (samples.Length > 0)
    {
        _currentRecordingSamples.AddRange(samples);
        
        if (debugMode && _currentRecordingSamples.Count % 8000 == 0)
        {
            Debug.Log($"Recording buffer now contains {_currentRecordingSamples.Count} samples");
        }
    }
}
```

### Handling Streaming Audio from Server

The system can handle both complete audio files and streaming audio chunks:

```csharp
// AudioStreamer handles streaming mode
public void StartStreaming()
{
    if (_isStreaming)
        return;
        
    _isStreaming = true;
    _streamBuffer.Clear();
    
    // Register for audio packet messages
    messageHandler.RegisterMessageHandler("audio_packet", HandleAudioPacket);
    
    Debug.Log("Started audio streaming mode");
}

private void HandleAudioPacket(string jsonMessage)
{
    try
    {
        JObject messageObj = JObject.Parse(jsonMessage);
        string base64Audio = messageObj["data"]?.ToString();
        
        if (!string.IsNullOrEmpty(base64Audio))
        {
            byte[] audioData = Convert.FromBase64String(base64Audio);
            ProcessIncomingAudioPacket(audioData);
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error handling audio packet: {ex.Message}");
    }
}
```

### Audio Format Conversion

The system handles format conversion between Unity's internal formats and WAV:

```csharp
private AudioClip CreateAudioClipFromWAV(byte[] wavData)
{
    // Validate WAV header
    if (wavData.Length < 44) // Minimum WAV header size
    {
        Debug.LogError("Invalid WAV data: too small for header");
        return null;
    }
    
    // Parse WAV header
    int channels = wavData[22]; // Mono = 1, Stereo = 2
    int sampleRate = BitConverter.ToInt32(wavData, 24);
    int bitsPerSample = wavData[34];
    
    // Find data chunk
    int dataChunkStart = 12; // Start of "fmt " chunk
    while (dataChunkStart < wavData.Length - 8)
    {
        // Look for "data" chunk ID
        if (wavData[dataChunkStart] == 'd' && 
            wavData[dataChunkStart + 1] == 'a' && 
            wavData[dataChunkStart + 2] == 't' && 
            wavData[dataChunkStart + 3] == 'a')
        {
            break;
        }
        
        // Move to next chunk
        int chunkSize = BitConverter.ToInt32(wavData, dataChunkStart + 4);
        dataChunkStart += 8 + chunkSize;
    }
    
    if (dataChunkStart >= wavData.Length - 8)
    {
        Debug.LogError("Invalid WAV data: no data chunk found");
        return null;
    }
    
    // Get data chunk size
    int dataSize = BitConverter.ToInt32(wavData, dataChunkStart + 4);
    int dataStart = dataChunkStart + 8;
    
    // Check if we have enough data
    if (dataStart + dataSize > wavData.Length)
    {
        Debug.LogError("Invalid WAV data: data chunk exceeds file size");
        return null;
    }
    
    // Create AudioClip
    AudioClip audioClip = AudioClip.Create(
        "ServerResponse", 
        dataSize / (bitsPerSample / 8) / channels, 
        channels, 
        sampleRate, 
        false
    );
    
    // Convert WAV data to float samples
    float[] audioData = new float[dataSize / (bitsPerSample / 8) / channels];
    
    // Convert based on bits per sample
    if (bitsPerSample == 16)
    {
        for (int i = 0; i < audioData.Length; i++)
        {
            int sampleIndex = dataStart + i * 2 * channels;
            short sample = (short)(wavData[sampleIndex] | (wavData[sampleIndex + 1] << 8));
            audioData[i] = sample / 32768f; // Convert to -1.0 to 1.0 range
        }
    }
    else if (bitsPerSample == 8)
    {
        for (int i = 0; i < audioData.Length; i++)
        {
            int sampleIndex = dataStart + i * channels;
            audioData[i] = (wavData[sampleIndex] - 128) / 128f; // Convert to -1.0 to 1.0 range
        }
    }
    else
    {
        Debug.LogError($"Unsupported bits per sample: {bitsPerSample}");
        return null;
    }
    
    // Set audio data
    audioClip.SetData(audioData, 0);
    
    return audioClip;
}
```

## VR-Specific Considerations

### Audio Performance in VR

The audio system is optimized for VR performance:
- Sample rate and buffer size optimized for Oculus Quest hardware
- Non-blocking audio processing to maintain frame rate
- Efficient memory usage to avoid garbage collection spikes

### Spatial Audio

The system supports spatial audio in the VR environment:

```csharp
// Configure spatial audio settings
if (_audioSource != null)
{
    // Set spatial blend to fully 3D
    _audioSource.spatialBlend = 1.0f;
    
    // Configure spatial settings
    _audioSource.dopplerLevel = 0f; // Disable doppler effect
    _audioSource.rolloffMode = AudioRolloffMode.Linear; // Linear falloff
    _audioSource.minDistance = 0.5f; // Close distance for clear speech
    _audioSource.maxDistance = 10f; // Maximum distance
    
    // Set spread for natural speech feeling
    _audioSource.spread = 15f;
    
    // Optional occlusion settings
    if (useLowPassFilter)
    {
        AudioLowPassFilter lowPassFilter = GetComponent<AudioLowPassFilter>();
        if (lowPassFilter == null)
        {
            lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        }
        
        lowPassFilter.cutoffFrequency = 5000f; // Set initial cutoff
        lowPassFilter.enabled = false; // Disabled by default
    }
}
```

### Occlusion and Reverberation

The system includes optional occlusion and reverberation effects for enhanced immersion:

```csharp
// Occlusion raycast check
private void CheckAudioOcclusion()
{
    if (Camera.main == null || !useLowPassFilter)
        return;
        
    Vector3 directionToListener = Camera.main.transform.position - transform.position;
    float distance = directionToListener.magnitude;
    
    RaycastHit hit;
    if (Physics.Raycast(transform.position, directionToListener, out hit, distance, occlusionLayers))
    {
        // Something is between audio source and listener
        AudioLowPassFilter lowPassFilter = GetComponent<AudioLowPassFilter>();
        if (lowPassFilter != null)
        {
            lowPassFilter.enabled = true;
            
            // Adjust cutoff based on material
            float cutoff = 5000f;
            
            // Simple material detection based on tag
            if (hit.collider.CompareTag("Glass"))
            {
                cutoff = 3000f;
            }
            else if (hit.collider.CompareTag("Wall"))
            {
                cutoff = 1000f;
            }
            else if (hit.collider.CompareTag("Door"))
            {
                cutoff = 2000f;
            }
            
            lowPassFilter.cutoffFrequency = cutoff;
        }
    }
    else
    {
        // Clear line of sight
        AudioLowPassFilter lowPassFilter = GetComponent<AudioLowPassFilter>();
        if (lowPassFilter != null)
        {
            lowPassFilter.enabled = false;
        }
    }
}
```

### Battery and Performance Considerations

The audio system includes optimizations for mobile VR battery life:

```csharp
// Audio processing settings that adapt based on power state
private void UpdatePowerSettings()
{
    // Check if device is in low power mode
    bool lowPowerMode = false;
    
    #if PLATFORM_ANDROID
    // Check Android battery state
    try
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
        {
            using (AndroidJavaObject batteryManager = context.Call<AndroidJavaObject>("getSystemService", "batterymanager"))
            {
                int batteryLevel = batteryManager.Call<int>("getIntProperty", 4); // 4 = BATTERY_PROPERTY_CAPACITY
                int batteryStatus = batteryManager.Call<int>("getIntProperty", 1); // 1 = BATTERY_PROPERTY_STATUS
                
                // If battery below 20% and not charging
                if (batteryLevel < 20 && batteryStatus != 2) // 2 = BATTERY_STATUS_CHARGING
                {
                    lowPowerMode = true;
                }
            }
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error checking battery state: {ex.Message}");
    }
    #endif
    
    // Adjust audio quality based on power mode
    if (lowPowerMode)
    {
        // Reduce quality to save battery
        sampleRate = 16000;
        recordingBufferLengthSec = 30;
        voiceDetectionThreshold = 0.03f; // Higher threshold to activate less often
    }
    else
    {
        // Normal quality
        sampleRate = 16000;
        recordingBufferLengthSec = 60;
        voiceDetectionThreshold = 0.02f;
    }
}
```

## Common Issues

### Microphone Access Issues

1. **Missing Microphone Permissions**:
   - **Symptoms**: No microphone devices available, recording fails
   - **Solution**: Add microphone permissions to manifest, request at runtime

2. **Microphone Setup Failure**:
   - **Symptoms**: "Failed to create microphone AudioClip" error
   - **Solution**: Ensure correct device name, try alternative microphones

### Audio Quality Issues

1. **Audio Clipping**:
   - **Symptoms**: Distortion in recorded audio
   - **Solution**: Implement dynamic gain control, normalize input

2. **Low Volume or Noise**:
   - **Symptoms**: Difficulty hearing voice, excessive background noise
   - **Solution**: Implement noise reduction, adjust voice detection threshold

### Playback Problems

1. **Playback Timing Issues**:
   - **Symptoms**: Lip sync out of sync with audio
   - **Solution**: Implement more precise timing correlation

2. **Audio Format Errors**:
   - **Symptoms**: "Failed to create AudioClip" errors
   - **Solution**: Validate WAV data, add support for additional formats

## Usage Examples

### Starting and Stopping Recording

```csharp
// Microphone initialization
MicrophoneCapture microphoneCapture = GetComponent<MicrophoneCapture>();

// Start recording
microphoneCapture.StartRecording();

// Stop recording (manually or automatically after silence)
microphoneCapture.StopRecording();

// Listen for recording events
microphoneCapture.OnRecordingStarted += HandleRecordingStarted;
microphoneCapture.OnRecordingStopped += HandleRecordingStopped;
microphoneCapture.OnAudioLevelChanged += HandleAudioLevelChanged;
```

### Processing and Sending Audio

```csharp
// In MicrophoneCapture
private void ProcessRecordedAudio()
{
    if (_currentRecordingSamples.Count == 0)
    {
        Debug.LogWarning("No audio samples collected during recording");
        return;
    }
    
    try
    {
        Debug.Log($"Processing {_currentRecordingSamples.Count} audio samples");
        
        // Convert samples to audio clip
        AudioClip recordedClip = AudioClip.Create(
            "RecordedAudio",
            _currentRecordingSamples.Count,
            1, // Mono
            sampleRate,
            false
        );
        
        recordedClip.SetData(_currentRecordingSamples.ToArray(), 0);
        
        // Process audio for server
        if (audioProcessor != null)
        {
            byte[] processedAudio = audioProcessor.ProcessAudioForServer(recordedClip);
            
            Debug.Log($"Audio processed successfully: {processedAudio.Length} bytes");
            
            // Send to server
            SendAudioToServer(processedAudio);
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error processing recorded audio: {ex.Message}");
    }
}
```

### Playing Back Server Responses

```csharp
// In MessageHandler
private void HandleAudioResponse(JObject messageObj)
{
    try {
        // Extract base64 audio data
        string base64Audio = messageObj["data"]?.ToString();
        if (string.IsNullOrEmpty(base64Audio))
        {
            Debug.LogError("Received audio response with no data");
            return;
        }
        
        // Convert to binary
        byte[] audioData = Convert.FromBase64String(base64Audio);
        Debug.Log($"Decoded audio data: {audioData.Length} bytes");
        
        // Play the audio
        if (audioPlayback != null)
        {
            audioPlayback.PlayAudioResponse(audioData);
            Debug.Log("Started audio playback");
        }
        
        // Broadcast the audio response
        OnAudioResponse?.Invoke(audioData);
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error handling audio response: {ex.Message}");
    }
}
```

## Conclusion

The audio subsystem is a critical component of the VR Interview System, providing seamless voice communication between the user and the virtual interviewer. It handles the complexities of audio capture, processing, transmission, and playback while optimizing for VR performance and quality. The system's modular design allows for easy customization and extension to support additional audio features in the future.
