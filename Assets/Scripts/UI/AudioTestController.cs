using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class AudioTestController : MonoBehaviour
{
    [SerializeField] private Button startSpeakingButton;
    [SerializeField] private Button stopSpeakingButton;
    [SerializeField] private Button testAudioButton; // Add this button to test audio playback
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private MicrophoneCapture microphoneCapture;
    [SerializeField] private AudioPlayback audioPlayback;
    [SerializeField] private AudioProcessor audioProcessor;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private TMPro.TextMeshProUGUI debugText;

    private void Start()
    {
        if (startSpeakingButton != null)
            startSpeakingButton.onClick.AddListener(StartSpeaking);
        
        if (stopSpeakingButton != null)
            stopSpeakingButton.onClick.AddListener(StopSpeaking);
            
        if (testAudioButton != null)
            testAudioButton.onClick.AddListener(TestAudioPlayback);
            
        // Initially disable stop button
        if (stopSpeakingButton != null)
            stopSpeakingButton.interactable = false;
    }

    public void StartSpeaking()
    {
        LogDebug("Start Speaking button pressed");
        
        // Start the session if not already started
        if (sessionManager != null && !sessionManager.IsSessionActive)
        {
            sessionManager.StartSession();
            StartCoroutine(WaitForSessionStart());
        }
        else
        {
            // If session already active, start speaking directly
            StartMicrophoneCapture();
        }
        
        // Update button states
        if (startSpeakingButton != null)
            startSpeakingButton.interactable = false;
        
        if (stopSpeakingButton != null)
            stopSpeakingButton.interactable = true;
    }

    private IEnumerator WaitForSessionStart()
    {
        // Wait a bit for session to initialize
        yield return new WaitForSeconds(1.0f);
        StartMicrophoneCapture();
    }

    private void StartMicrophoneCapture()
    {
        LogDebug("Starting microphone capture");
        
        if (microphoneCapture != null)
        {
            microphoneCapture.StartRecording();
        }
    }

    public void StopSpeaking()
    {
        LogDebug("Stop Speaking button pressed");
        
        // Clear the transcript first to show the "thinking" message
        if (uiManager != null)
        {
            uiManager.ClearTranscript();
            LogDebug("Transcript cleared with thinking message");
        }
        
        // Then stop recording, which will initiate the sending of audio
        if (microphoneCapture != null)
        {
            microphoneCapture.StopRecording();
        }
        
        // Update button states
        if (startSpeakingButton != null)
            startSpeakingButton.interactable = true;
        
        if (stopSpeakingButton != null)
            stopSpeakingButton.interactable = false;
    }

    private void LogDebug(string message)
    {
        Debug.Log(message);
        
        if (debugText != null)
        {
            // Add timestamp
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            string entry = $"[{timestamp}] {message}\n";
            
            // Add to debug text
            debugText.text += entry;
            
            // Trim if too long
            if (debugText.text.Length > 2000)
            {
                debugText.text = debugText.text.Substring(debugText.text.Length - 2000);
            }
        }
    }
    
    /// <summary>
    /// Test function to verify audio playback is working
    /// </summary>
    public void TestAudioPlayback()
    {
        LogDebug("Testing audio playback...");
        
        // Generate a simple test tone
        int sampleRate = 16000;
        int sampleCount = sampleRate * 2; // 2 seconds of audio
        
        // Create sine wave samples (440Hz tone)
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            samples[i] = Mathf.Sin(2 * Mathf.PI * 440 * t) * 0.5f;
        }
        
        LogDebug($"Created test tone with {sampleCount} samples");
        
        // Create audio clip
        AudioClip testClip = AudioClip.Create("TestTone", sampleCount, 1, sampleRate, false);
        testClip.SetData(samples, 0);
        
        // Convert to WAV for testing the pipeline
        byte[] wavData = null;
        
        if (audioProcessor != null)
        {
            wavData = audioProcessor.ProcessAudioForServer(testClip);
            LogDebug($"Created test WAV data: {wavData.Length} bytes");
        }
        else
        {
            LogDebug("AudioProcessor not available for test");
            return;
        }
        
        // Test audio playback
        if (audioPlayback != null)
        {
            audioPlayback.PlayAudioResponse(wavData);
            LogDebug("Playing test audio");
        }
        else
        {
            LogDebug("AudioPlayback not available for test");
        }
    }
}