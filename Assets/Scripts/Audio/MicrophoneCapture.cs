using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRInterview; // Added namespace reference for PlatformManager

/// <summary>
/// Handles microphone input capture and processing for the interview system.
/// Cross-platform compatible with Mac, Windows, and AMD GPU systems.
/// </summary>
public class MicrophoneCapture : MonoBehaviour
{
    [SerializeField] private WebSocketClient webSocketClient;
    [SerializeField] private AudioProcessor audioProcessor;
    [SerializeField] private SessionManager sessionManager;
    
    [Header("Audio Settings")]
    [SerializeField] private int sampleRate = 16000;
    [SerializeField] private int recordingBufferLengthSec = 60;
    [SerializeField] private float voiceDetectionThreshold = 0.02f;
    [SerializeField] private float silenceTimeoutSec = 1.5f;
    [SerializeField] private bool autoSelectMicrophone = true;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true; // Enable debug by default
    [SerializeField] private bool visualizeAudio = false;
    
    private AudioClip _microphoneClip;
    private string _selectedMicrophone;
    private bool _isRecording = false;
    private float _silenceTimer = 0f;
    private int _lastSamplePosition = 0;
    private List<float> _currentRecordingSamples = new List<float>();
    private float _currentAudioLevel = 0f;
    private bool _isInitialized = false;
    private int _platformOptimalSampleRate;
    
    // Events
    public event Action OnRecordingStarted;
    public event Action OnRecordingStopped;
    public event Action<float> OnAudioLevelChanged;
    
    // Properties
    public bool IsRecording => _isRecording;
    public float CurrentAudioLevel => _currentAudioLevel;
    public string[] AvailableMicrophones => Microphone.devices;
    public string SelectedMicrophone => _selectedMicrophone;
    
    private void Start()
    {
        Initialize();
        
        // Listen for state changes
        if (sessionManager != null)
        {
            sessionManager.OnStateChanged += HandleStateChange;
        }
        else
        {
            Debug.LogError("SessionManager not assigned to MicrophoneCapture!");
        }
    }
    
    /// <summary>
    /// Initializes the microphone system with platform-specific settings.
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
        
        // Check if microphone is available
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone found!");
            return;
        }
        
        // Get platform-specific microphone device
        if (PlatformManager.Instance != null && autoSelectMicrophone)
        {
            _selectedMicrophone = PlatformManager.Instance.GetDefaultMicrophoneDevice();
            Debug.Log($"Platform selected microphone: {_selectedMicrophone}");
        }
        else
        {
            // Get default microphone from settings or auto-select
            if (SettingsManager.Instance != null)
            {
                string savedMicrophone = SettingsManager.Instance.GetSetting<string>("Microphone");
                
                if (!string.IsNullOrEmpty(savedMicrophone) && Array.IndexOf(Microphone.devices, savedMicrophone) != -1)
                {
                    _selectedMicrophone = savedMicrophone;
                    Debug.Log($"Using saved microphone: {_selectedMicrophone}");
                }
                else if (autoSelectMicrophone)
                {
                    // Use first microphone by default
                    _selectedMicrophone = Microphone.devices[0];
                    Debug.Log($"Auto-selected microphone: {_selectedMicrophone}");
                    
                    // Save selection
                    if (SettingsManager.Instance != null)
                    {
                        SettingsManager.Instance.SetSetting("Microphone", _selectedMicrophone);
                        SettingsManager.Instance.SaveSettings();
                    }
                }
            }
            else if (autoSelectMicrophone)
            {
                // Use first microphone by default
                _selectedMicrophone = Microphone.devices[0];
                Debug.Log($"Auto-selected microphone: {_selectedMicrophone}");
            }
        }
        
        _isInitialized = true;
        
        // Log all available microphones for debugging
        if (debugMode)
        {
            Debug.Log("Available microphones:");
            foreach (string device in Microphone.devices)
            {
                Debug.Log($"- {device}");
            }
        }
    }
    
    /// <summary>
    /// Starts recording from the microphone with platform-specific settings.
    /// </summary>
    public void StartRecording()
    {
        if (_isRecording)
        {
            Debug.LogWarning("Already recording! Ignoring StartRecording call.");
            return;
        }
        
        if (!_isInitialized)
        {
            Initialize();
        }
        
        // Make sure we have a valid microphone selected
        if (string.IsNullOrEmpty(_selectedMicrophone))
        {
            Debug.LogWarning("No microphone selected, attempting to initialize");
            Initialize();
            
            // If still no microphone, try to use default
            if (string.IsNullOrEmpty(_selectedMicrophone))
            {
                if (Microphone.devices.Length > 0)
                {
                    _selectedMicrophone = Microphone.devices[0];
                    Debug.Log($"Selected default microphone: {_selectedMicrophone}");
                }
                else
                {
                    Debug.LogError("No microphone devices available!");
                    return;
                }
            }
        }
        
        try
        {
            // Stop any existing recording first
            if (Microphone.IsRecording(_selectedMicrophone))
            {
                Debug.Log("Stopping existing microphone recording");
                Microphone.End(_selectedMicrophone);
            }
            
            // Wait a frame to ensure microphone is properly released
            StartCoroutine(StartRecordingAfterDelay());
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error starting microphone recording: {ex.Message}");
            
            // Try again with default microphone as fallback
            if (Microphone.devices.Length > 0 && _selectedMicrophone != Microphone.devices[0])
            {
                Debug.Log("Attempting with default microphone as fallback");
                _selectedMicrophone = Microphone.devices[0];
                StartCoroutine(StartRecordingAfterDelay());
            }
        }
    }
    
    private IEnumerator StartRecordingAfterDelay()
    {
        yield return null; // Wait one frame
        
        bool success = false;
        Exception exception = null;
        
        // Start recording
        Debug.Log($"Starting microphone recording from: {_selectedMicrophone}");
        
        try
        {
            // Check if the device supports the selected sample rate
            // Some platforms (especially macOS) may not support lower sample rates
            int deviceSampleRate = GetCompatibleSampleRate();
            
            // Start recording
            _microphoneClip = Microphone.Start(_selectedMicrophone, true, recordingBufferLengthSec, deviceSampleRate);
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        
        // Check if microphone clip was created successfully
        if (_microphoneClip == null)
        {
            Debug.LogError("Failed to create microphone AudioClip!");
            yield break;
        }
        
        // Wait a few frames to ensure recording has started
        yield return new WaitForSeconds(0.1f);
        
        // Verify recording actually started
        if (!Microphone.IsRecording(_selectedMicrophone))
        {
            Debug.LogError("Microphone.IsRecording returned false after Start() call");
            yield break;
        }
        
        try
        {
            // Double-check the actual sample rate used
            int actualSampleRate = _microphoneClip.frequency;
            Debug.Log($"Microphone actual sample rate: {actualSampleRate} Hz (requested: {sampleRate} Hz)");
            
            _isRecording = true;
            _lastSamplePosition = 0;
            _currentRecordingSamples.Clear();
            _silenceTimer = 0f;
            
            // Notify listeners
            OnRecordingStarted?.Invoke();
            
            if (debugMode)
            {
                Debug.Log($"Recording started with sample rate: {actualSampleRate}, buffer length: {recordingBufferLengthSec}s");
            }
            
            success = true;
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        
        // Handle any exceptions that occurred
        if (!success && exception != null)
        {
            Debug.LogError($"Error in StartRecordingAfterDelay: {exception.Message}");
        }
    }
    
    /// <summary>
    /// Gets a sample rate that is compatible with the current microphone device.
    /// macOS often requires higher sample rates.
    /// </summary>
    private int GetCompatibleSampleRate()
    {
        // Check if we have platform information
        if (PlatformManager.Instance != null)
        {
            var platform = PlatformManager.Instance.CurrentPlatform;
            
            // Special handling for macOS
            if (platform == PlatformManager.Platform.MacOS)
            {
                int minFreq, maxFreq;
                Microphone.GetDeviceCaps(_selectedMicrophone, out minFreq, out maxFreq);
                
                Debug.Log($"macOS microphone caps: min={minFreq}Hz, max={maxFreq}Hz");
                
                // If no limitations (0,0) or only min limit (>0,0), use the platform's optimal rate
                if (maxFreq == 0)
                {
                    return _platformOptimalSampleRate;
                }
                
                // Check if our desired rate is within range
                if (sampleRate >= minFreq && sampleRate <= maxFreq)
                {
                    return sampleRate;
                }
                // Otherwise use the closest valid rate
                else if (sampleRate < minFreq)
                {
                    Debug.LogWarning($"Sample rate {sampleRate} too low for macOS device. Using minimum: {minFreq}");
                    return minFreq;
                }
                else
                {
                    Debug.LogWarning($"Sample rate {sampleRate} too high for macOS device. Using maximum: {maxFreq}");
                    return maxFreq;
                }
            }
        }
        
        // For other platforms, use the configured sample rate
        return sampleRate;
    }
    
    /// <summary>
    /// Stops recording and processes the recorded audio.
    /// </summary>
    public void StopRecording()
    {
        if (!_isRecording || string.IsNullOrEmpty(_selectedMicrophone))
            return;
        
        try
        {
            // Get the final position before stopping
            int finalPosition = Microphone.GetPosition(_selectedMicrophone);
            
            // If we haven't collected any samples yet, get the final batch
            if (_currentRecordingSamples.Count == 0)
            {
                float[] finalSamples = GetMicrophoneData(_lastSamplePosition, finalPosition);
                AddSamplesToRecording(finalSamples);
                
                Debug.Log($"Gathered {finalSamples.Length} samples at the end of recording");
            }
            
            // Stop recording
            Microphone.End(_selectedMicrophone);
            
            // Process the final audio
            ProcessRecordedAudio();
            
            _isRecording = false;
            _currentAudioLevel = 0f;
            
            // Notify listeners
            OnRecordingStopped?.Invoke();
            OnAudioLevelChanged?.Invoke(0f);
            
            if (debugMode)
            {
                Debug.Log($"Recording stopped. Captured {_currentRecordingSamples.Count} samples.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error stopping microphone recording: {ex.Message}");
        }
    }
    
    private void Update()
    {
        if (!_isRecording)
            return;
        
        try
        {
            // Get current microphone position
            int currentPosition = Microphone.GetPosition(_selectedMicrophone);
            
            // Check if new audio data is available
            if (currentPosition != _lastSamplePosition)
            {
                // Get new audio samples
                float[] samples = GetMicrophoneData(_lastSamplePosition, currentPosition);
                
                if (samples.Length > 0)
                {
                    // Calculate audio level for UI feedback
                    float audioLevel = CalculateAudioLevel(samples);
                    _currentAudioLevel = audioLevel;
                    OnAudioLevelChanged?.Invoke(audioLevel);
                    
                    // Add samples to current recording
                    AddSamplesToRecording(samples);
                    
                    if (debugMode)
                    {
                        Debug.Log($"Added {samples.Length} samples, audio level: {audioLevel:F3}");
                    }
                    
                    // Check for silence
                    if (audioLevel < voiceDetectionThreshold)
                    {
                        _silenceTimer += Time.deltaTime;
                        
                        // If silence lasts too long, stop recording
                        if (_silenceTimer > silenceTimeoutSec)
                        {
                            if (debugMode)
                            {
                                Debug.Log($"Silence detected for {silenceTimeoutSec}s. Stopping recording.");
                            }
                            StopRecording();
                            return;
                        }
                    }
                    else
                    {
                        _silenceTimer = 0f;
                    }
                    
                    // Visualize audio in debug mode
                    if (debugMode && visualizeAudio)
                    {
                        VisualizeAudioLevel(audioLevel);
                    }
                }
                
                // Update last position
                _lastSamplePosition = currentPosition;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in MicrophoneCapture Update: {ex.Message}");
            StopRecording();
        }
    }
    
    /// <summary>
    /// Gets microphone data between two positions in the recording buffer.
    /// </summary>
    /// <param name="fromPosition">Start position.</param>
    /// <param name="toPosition">End position.</param>
    /// <returns>Array of audio samples.</returns>
    private float[] GetMicrophoneData(int fromPosition, int toPosition)
    {
        if (_microphoneClip == null)
            return new float[0];
        
        int sampleCount;
        
        // Handle wrap-around in the circular buffer
        if (toPosition < fromPosition)
        {
            sampleCount = (_microphoneClip.samples - fromPosition) + toPosition;
        }
        else
        {
            sampleCount = toPosition - fromPosition;
        }
        
        if (sampleCount <= 0)
            return new float[0];
            
        float[] samples = new float[sampleCount];
        
        try
        {
            // Handle wrap-around when copying data
            if (toPosition < fromPosition)
            {
                // Copy from fromPosition to end of clip
                int firstPartSamples = _microphoneClip.samples - fromPosition;
                float[] firstPartData = new float[firstPartSamples];
                _microphoneClip.GetData(firstPartData, fromPosition);
                Array.Copy(firstPartData, 0, samples, 0, firstPartSamples);
                
                // Copy from start of clip to toPosition
                if (toPosition > 0)
                {
                    float[] secondPartData = new float[toPosition];
                    _microphoneClip.GetData(secondPartData, 0);
                    Array.Copy(secondPartData, 0, samples, firstPartSamples, toPosition);
                }
            }
            else
            {
                // Simple case, no wrap-around
                _microphoneClip.GetData(samples, fromPosition);
            }
            
            if (debugMode)
            {
                // Check if samples contain audio data
                float maxValue = 0f;
                for (int i = 0; i < samples.Length; i++)
                {
                    if (Mathf.Abs(samples[i]) > maxValue)
                        maxValue = Mathf.Abs(samples[i]);
                }
                
                if (maxValue < 0.01f)
                {
                    Debug.LogWarning($"Very low audio levels detected: max value = {maxValue}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting microphone data: {ex.Message}");
            return new float[0];
        }
        
        return samples;
    }
    
    /// <summary>
    /// Calculates audio level from samples.
    /// </summary>
    /// <param name="samples">Audio samples.</param>
    /// <returns>Audio level between 0 and 1.</returns>
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
    
    /// <summary>
    /// Adds samples to the current recording buffer.
    /// </summary>
    /// <param name="samples">Audio samples to add.</param>
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
    
    /// <summary>
    /// Processes the recorded audio and sends it to the server.
    /// </summary>
    private void ProcessRecordedAudio()
    {
        if (_currentRecordingSamples.Count == 0)
        {
            Debug.LogWarning("No audio samples collected during recording");
            
            // Try to get samples directly from the Microphone class
            if (_microphoneClip != null)
            {
                int position = Microphone.GetPosition(_selectedMicrophone);
                if (position > 0)
                {                    
                    // Create buffer and get data
                    float[] allSamples = new float[_microphoneClip.samples];
                    _microphoneClip.GetData(allSamples, 0);
                    
                    // Find sections with actual audio (non-zero samples)
                    List<float> validSamples = new List<float>();
                    for (int i = 0; i < allSamples.Length; i++)
                    {
                        if (Mathf.Abs(allSamples[i]) > 0.01f)
                        {
                            // Found some audio data, expand to include context
                            int start = Mathf.Max(0, i - 1000);  // Include 1000 samples before
                            int end = Mathf.Min(allSamples.Length, i + 16000); // And ~1 second after
                            
                            for (int j = start; j < end; j++)
                            {
                                validSamples.Add(allSamples[j]);
                            }
                            
                            Debug.Log($"Found audio section from sample {start} to {end}");
                            break;
                        }
                    }
                    
                    if (validSamples.Count > 0)
                    {
                        _currentRecordingSamples = validSamples;
                        Debug.Log($"Recovered {validSamples.Count} valid audio samples");
                    }
                }
            }
            
            // If still no samples, generate a test tone as fallback
            if (_currentRecordingSamples.Count == 0 && debugMode)
            {
                Debug.Log("Generating test audio since no samples were collected");
                GenerateTestTone();
            }
            
            if (_currentRecordingSamples.Count == 0)
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
                _microphoneClip?.frequency ?? sampleRate, // Use actual recorded frequency if available
                false
            );
            
            recordedClip.SetData(_currentRecordingSamples.ToArray(), 0);
            
            // Process audio for server - may need resampling on Mac
            if (audioProcessor != null)
            {
                // Check if we need to resample the audio
                if (recordedClip.frequency != sampleRate)
                {
                    Debug.Log($"Resampling audio from {recordedClip.frequency}Hz to {sampleRate}Hz");
                    recordedClip = audioProcessor.ResampleAudioClip(recordedClip, sampleRate);
                }
                
                byte[] processedAudio = audioProcessor.ProcessAudioForServer(recordedClip);
                
                Debug.Log($"Audio processed successfully: {processedAudio.Length} bytes");
                
                // Send to server without using async/await in this method
                SendAudioToServer(processedAudio);
            }
            else
            {
                Debug.LogError("AudioProcessor not assigned to MicrophoneCapture!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing recorded audio: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Sends processed audio data to the server using a coroutine.
    /// </summary>
    /// <param name="audioData">Processed audio data.</param>
    private void SendAudioToServer(byte[] audioData)
    {
        if (audioData == null || audioData.Length == 0)
        {
            Debug.LogError("Cannot send empty audio data to server");
            return;
        }
            
        if (webSocketClient == null || !webSocketClient.IsConnected)
        {
            Debug.LogError("Cannot send audio: WebSocketClient not available or not connected");
            return;
        }
        
        if (sessionManager == null)
        {
            Debug.LogError("Cannot send audio: SessionManager not available");
            return;
        }
        
        // Get session ID from session manager
        string sessionId = sessionManager.SessionId;
        
        Debug.Log($"Sending {audioData.Length} bytes of audio data to server with session ID: {sessionId}");
        
        // Use coroutine instead of async/await
        StartCoroutine(SendAudioToServerCoroutine(audioData, sessionId));
    }
    
    /// <summary>
    /// Coroutine to send audio data to the server.
    /// </summary>
    private IEnumerator SendAudioToServerCoroutine(byte[] audioData, string sessionId)
    {
        // Start sending the data
        var operation = webSocketClient.SendAudioData(audioData, sessionId);
        
        // Wait for operation to complete without using try/catch
        while (!operation.IsCompleted)
        {
            yield return null;
        }
        
        // Now that we're outside the loop, we can use try/catch
        try
        {
            // Check if the operation faulted
            if (operation.IsFaulted)
            {
                Debug.LogError($"Error sending audio to server: {operation.Exception?.Message ?? "Unknown error"}");
            }
            else
            {
                Debug.Log("Audio data sent successfully");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing audio send result: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Generates a simple test tone to verify the audio pipeline.
    /// </summary>
    private void GenerateTestTone()
    {
        Debug.Log("Generating test tone...");
        
        // Generate a simple sine wave at 440Hz (A4 note) for 1 second
        int sampleCount = sampleRate;
        _currentRecordingSamples.Clear();
        
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = Mathf.Sin(2 * Mathf.PI * 440 * t) * 0.5f; // 440Hz sine wave at half volume
            _currentRecordingSamples.Add(sample);
        }
        
        Debug.Log($"Generated test tone with {_currentRecordingSamples.Count} samples");
    }
    
    /// <summary>
    /// Handles state changes from the session manager.
    /// </summary>
    /// <param name="previousState">Previous state.</param>
    /// <param name="currentState">Current state.</param>
    private void HandleStateChange(string previousState, string currentState)
    {
        Debug.Log($"Handling state change: {previousState} -> {currentState}");
        
        // Start recording when in WAITING or IDLE state and user starts speaking
        if ((previousState == "WAITING" || previousState == "IDLE") && 
            currentState == "LISTENING")
        {
            // Add a safety check - only start recording if not already recording
            if (!_isRecording)
            {
                Debug.Log("Starting microphone recording due to LISTENING state");
                StartRecording();
            }
            else
            {
                Debug.LogWarning("Received LISTENING state but microphone already recording");
            }
        }
        
        // Stop recording when transitioning to PROCESSING
        if (currentState == "PROCESSING")
        {
            // Always stop recording when entering PROCESSING state, regardless of previous state
            if (_isRecording)
            {
                Debug.Log("Stopping microphone recording due to PROCESSING state");
                StopRecording();
            }
        }
    }
    
    /// <summary>
    /// Sets the active microphone device.
    /// </summary>
    /// <param name="deviceName">Name of the microphone device.</param>
    public void SetMicrophone(string deviceName)
    {
        if (Array.IndexOf(Microphone.devices, deviceName) != -1)
        {
            _selectedMicrophone = deviceName;
            
            // Save selection
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetSetting("Microphone", _selectedMicrophone);
                SettingsManager.Instance.SaveSettings();
            }
            
            Debug.Log($"Selected microphone: {_selectedMicrophone}");
            
            // Restart microphone if currently recording
            if (_isRecording)
            {
                StopRecording();
                StartRecording();
            }
        }
        else
        {
            Debug.LogError($"Microphone '{deviceName}' not found");
        }
    }
    
    /// <summary>
    /// Visualizes audio level in the console (debug only).
    /// </summary>
    /// <param name="level">Audio level between 0 and 1.</param>
    private void VisualizeAudioLevel(float level)
    {
        int barLength = Mathf.RoundToInt(level * 50);
        string bar = new string('|', barLength);
        Debug.Log($"Audio Level: [{bar}] {level:F3}");
    }
}