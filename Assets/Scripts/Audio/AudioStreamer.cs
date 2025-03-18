using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VRInterview.Network;
using System.Linq;

namespace VRInterview.Audio
{
    /// <summary>
    /// Handles streaming audio from URLs for the VR Interview System.
    /// This class manages the streaming of audio from AllTalk TTS server 
    /// and reports streaming status.
    /// </summary>
    public class AudioStreamer : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SessionManager sessionManager;
        [SerializeField] private AudioPlayback audioPlayback;
        [SerializeField] private UIManager uiManager;
        
        [Header("Streaming Settings")]
        [SerializeField] private float streamingTimeout = 30f; // Maximum time to wait for streaming
        [SerializeField] private int maxRetryAttempts = 2;
        [SerializeField] private bool preferStreaming = true;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = true;
        
        // Events
        public event Action<float> OnStreamingProgress;
        public event Action<AudioClip> OnStreamingComplete;
        public event Action<string> OnStreamingFailed;
        
        // References
        private WebSocketClient _webSocketClient;
        private string _sessionId;
        private Coroutine _activeStreamingCoroutine;
        private string _lastPlayedUrl;

        private void Start()
        {
            // Find references if not set
            if (sessionManager == null)
            {
                sessionManager = FindObjectOfType<SessionManager>();
            }
            
            if (audioPlayback == null)
            {
                audioPlayback = FindObjectOfType<AudioPlayback>();
            }
            
            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }
            
            // Get WebSocketClient reference from SessionManager
            if (sessionManager != null)
            {
                _webSocketClient = sessionManager.GetWebSocketClient();
                _sessionId = sessionManager.GetSessionId();
            }
            
            if (_webSocketClient == null)
            {
                _webSocketClient = FindObjectOfType<WebSocketClient>();
            }
            
            if (debugMode)
            {
                Debug.Log("AudioStreamer initialized");
            }
        }

        /// <summary>
        /// Stream audio from a URL
        /// </summary>
        /// <param name="url">URL to stream audio from</param>
        /// <param name="format">Format of the audio (wav, mp3, etc.)</param>
        /// <param name="fallbackText">Fallback text if streaming fails</param>
        public void StreamAudioFromUrl(string url, string format, string fallbackText)
        {
        if (debugMode)
        {
            Debug.Log($"StreamAudioFromUrl called with URL: {url}, format: {format}");
        if (fallbackText != null && fallbackText.Length > 0)
            {
                Debug.Log($"Fallback text provided: {fallbackText.Substring(0, Math.Min(30, fallbackText.Length))}...");
            }
        }
        
        // Since we're no longer using streaming, just show the fallback text
        if (!string.IsNullOrEmpty(fallbackText) && uiManager != null)
        {
            Debug.Log("Using fallback text directly since streaming is disabled");
            uiManager.ShowFallbackText(fallbackText);
            return;
        }
        
        // If no fallback text, show an error
        Debug.LogWarning("StreamAudioFromUrl called but streaming is disabled and no fallback text provided");
        if (uiManager != null)
        {
            uiManager.ShowError("Audio streaming is disabled. Please check system configuration.");
        }
    }

        /// <summary>
        /// Cancel active streaming
        /// </summary>
        public void CancelStreaming()
        {
            if (_activeStreamingCoroutine != null)
            {
                StopCoroutine(_activeStreamingCoroutine);
                _activeStreamingCoroutine = null;
                
                if (debugMode)
                {
                    Debug.Log("Streaming canceled");
                }
                
                // Report cancellation if we have a URL
                if (!string.IsNullOrEmpty(_lastPlayedUrl) && _webSocketClient != null)
                {
                    SendStreamingStatus("failed", _lastPlayedUrl, "Streaming canceled by client");
                }
            }
        }

        private IEnumerator StreamAudioCoroutine(string url, string format, string fallbackText)
        {
            if (debugMode)
            {
                Debug.Log($"Starting audio streaming from URL: {url}, format: {format}");
            }
            
            // URL normalization - make sure we have a full URL
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                // If URL is relative, try to convert to absolute based on known patterns
                if (url.StartsWith("/"))
                {
                    // Extract base URL by looking for known domain patterns
                    string baseUrl = "http://127.0.0.1:7851"; // Default fallback
                    if (url.Contains(":7851"))
                    {
                        baseUrl = "http://127.0.0.1:7851";
                    }
                    
                    // Combine base URL with relative path
                    url = baseUrl + url;
                    Debug.Log($"Converted relative URL to absolute: {url}");
                }
            }
            
            // Show streaming status in UI
            if (uiManager != null)
            {
                uiManager.UpdateStatus("Streaming audio response...");
            }

            if (debugMode)
            {
                Debug.Log($"StreamAudioCoroutine - URL Format Check: '{url}'");
                
                // Check if URL is properly formatted
                bool hasHttp = url.StartsWith("http://") || url.StartsWith("https://");
                Debug.Log($"URL has http/https prefix: {hasHttp}");
                
                // Check if AllTalk endpoint is in URL
                bool hasCorrectEndpoint = url.Contains("/api/tts-generate-streaming");
                Debug.Log($"URL contains tts-generate-streaming endpoint: {hasCorrectEndpoint}");
                
                // Check for required parameters
                bool hasText = url.Contains("text=");
                bool hasVoice = url.Contains("voice=");
                bool hasLanguage = url.Contains("language=");
                bool hasOutputFile = url.Contains("output_file=");
                Debug.Log($"URL parameters - text: {hasText}, voice: {hasVoice}, language: {hasLanguage}, output_file: {hasOutputFile}");
                
                // If URL doesn't look valid, but we have a fallback, use it
                if (!hasHttp || !hasCorrectEndpoint || !hasText || !hasVoice)
                {
                    Debug.LogWarning("URL doesn't appear to be a valid AllTalk streaming URL");
                }
            }
            
            // Determine the appropriate audio type based on format
            AudioType audioType = GetAudioTypeFromFormat(format);
            
            // Report streaming started
            SendStreamingStatus("started", url);

            // Attempt streaming with retries
            AudioClip streamedClip = null;
            string errorMessage = null;
            int attempts = 0;
            
            while (streamedClip == null && attempts < maxRetryAttempts)
            {
                attempts++;
                
                if (debugMode)
                {
                    Debug.Log($"Streaming attempt {attempts}/{maxRetryAttempts}");
                }
                
                // Create request with appropriate audio type
                // Log the attempt with more details
                if (debugMode)
                {
                    Debug.Log($"Starting streaming attempt {attempts} with URL: {url.Substring(0, Math.Min(100, url.Length))}...");
                }
                
                using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
                {
                    // Set request headers
                    request.SetRequestHeader("Accept", "audio/*");
                    
                    // Create a new download handler with specific options
                    DownloadHandlerAudioClip audioHandler = new DownloadHandlerAudioClip(url, audioType);
                    audioHandler.streamAudio = true;
                    audioHandler.compressed = false;
                    
                    // Set the download handler
                    request.downloadHandler = audioHandler;
                    // Set timeout
                    request.timeout = (int)streamingTimeout;
                    
                    // Start request
                    request.SendWebRequest();
                    
                    // Wait for completion with progress updates
                    while (!request.isDone)
                    {
                        // Update progress
                        float progress = request.downloadProgress;
                        OnStreamingProgress?.Invoke(progress);
                        
                        // Update UI if available
                        if (uiManager != null && progress > 0)
                        {
                            uiManager.UpdateProgress(progress);
                        }
                        
                        yield return null;
                    }
                    
                    // Check for errors
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        errorMessage = request.error;
                        Debug.LogWarning($"Streaming attempt {attempts} failed: {errorMessage}");
                        
                        // Wait before retry
                        if (attempts < maxRetryAttempts)
                        {
                            yield return new WaitForSeconds(1f);
                        }
                    }
                    else
                    {
                        // Get audio clip from response
                        streamedClip = DownloadHandlerAudioClip.GetContent(request);
                        
                        // Ensure the clip has a name to avoid FMOD errors
                        if (streamedClip != null)
                        {
                            streamedClip.name = "StreamedResponse_" + System.DateTime.Now.Ticks;
                            
                            // Verify clip is valid
                            if (streamedClip.loadState != AudioDataLoadState.Loaded)
                            {
                                Debug.LogWarning($"Streamed clip not fully loaded (state: {streamedClip.loadState}). Waiting...");
                                
                                // Wait for up to 5 seconds for the clip to load
                                float waitStart = Time.time;
                                while (streamedClip.loadState != AudioDataLoadState.Loaded && Time.time - waitStart < 5f)
                                {
                                    yield return new WaitForSeconds(0.1f);
                                }
                                
                                if (streamedClip.loadState != AudioDataLoadState.Loaded)
                                {
                                    Debug.LogError($"Streamed clip failed to load properly. State: {streamedClip.loadState}");
                                    streamedClip = null;
                                    errorMessage = "Audio clip failed to load properly";
                                    continue;
                                }
                            }
                        }
                        
                        if (streamedClip == null)
                        {
                            errorMessage = "Failed to extract audio content from response";
                            Debug.LogWarning(errorMessage);
                        }
                        else
                        {
                            if (debugMode)
                            {
                                Debug.Log($"Successfully streamed audio clip: {streamedClip.length}s, {streamedClip.frequency}Hz, {streamedClip.channels} channels");
                            }
                            // Success - break retry loop
                            break;
                        }
                    }
                }
            }

            // Clear active coroutine reference
            _activeStreamingCoroutine = null;
            
            // Handle result
            if (streamedClip != null)
            {
                // Report success
                SendStreamingStatus("completed", url);
                
                if (debugMode)
                {
                    Debug.Log("Audio streaming completed successfully");
                }
                
                // Play the streamed audio
                if (audioPlayback != null)
                {
                    // Create a PlayResponseDelegate for this streamed audio
                    Action playAction = () => {
                        StartCoroutine(PlayStreamedAudioCoroutine(streamedClip));
                    };
                    
                    // Don't play immediately if we're already playing something
                    if (audioPlayback.IsPlaying)
                    {
                        // Wait for current playback to finish
                        if (debugMode)
                        {
                            Debug.Log("Waiting for current playback to finish before playing streamed audio");
                        }
                        
                        // Define the event handler with correct signature
                        Action completionHandler = null;
                        completionHandler = () => {
                            audioPlayback.OnPlaybackCompleted -= completionHandler;
                            playAction();
                        };
                        
                        // Wait for current playback to finish, then play
                        audioPlayback.OnPlaybackCompleted += completionHandler;
                    }
                    else
                    {
                        // Play immediately
                        playAction();
                    }
                }
                
                // Notify listeners
                OnStreamingComplete?.Invoke(streamedClip);
            }
            else
            {
                Debug.LogWarning($"Audio streaming failed after {attempts} attempts: {errorMessage}");
                
                // Try direct audio download as a fallback
                Debug.Log("Attempting direct download as fallback...");
                yield return StartCoroutine(TryDirectAudioDownload(url));
                
                // Check if fallback succeeded (TryDirectAudioDownload sends playback notification)
                if (debugMode)
                {
                    Debug.Log("Fallback download attempt finished");
                }
                
                // If we reached here, the fallback either succeeded or failed
                // We'll report failure in case it didn't work
                SendStreamingStatus("failed", url, errorMessage);
                
                // Notify listeners anyway
                OnStreamingFailed?.Invoke(errorMessage);
                
                // Show fallback text if available
                if (!string.IsNullOrEmpty(fallbackText) && uiManager != null)
                {
                    uiManager.ShowFallbackText(fallbackText);
                }
            }
        }

        private IEnumerator PlayStreamedAudioCoroutine(AudioClip clip)
        {
        if (clip == null)
        {
        Debug.LogError("Cannot play null audio clip");
        yield break;
        }
        
        // Log detailed clip information to help with debugging
        if (debugMode)
        {
        Debug.Log($"Preparing to play audio clip: {clip.name}, Length: {clip.length}s, State: {clip.loadState}, Channels: {clip.channels}, Frequency: {clip.frequency}Hz");
        }
        
        // Just call the AudioPlayback directly and wait for completion
        audioPlayback.PlayStreamedAudio(clip);
        
        // Wait until playback is complete
        while (audioPlayback.IsPlaying)
        {
        yield return null;
        }
        
        if (debugMode)
        {
        Debug.Log("Streamed audio playback completed");
        }
        }
    
    /// <summary>
    /// Downloads an audio clip directly from the server's API instead of using streaming
    /// </summary>
    /// <param name="url">The streaming URL to extract file info from</param>
    /// <returns>A coroutine that downloads the audio clip</returns>
    private IEnumerator TryDirectAudioDownload(string url)
    {
        if (debugMode)
        {
            Debug.Log("Attempting direct audio download as fallback");
        }

        // Extract the output_file parameter from the URL
        string outputFileName = null;
        if (url.Contains("output_file="))
        {
            int startIndex = url.IndexOf("output_file=") + "output_file=".Length;
            int endIndex = url.IndexOf("&", startIndex);
            if (endIndex == -1) endIndex = url.Length;
            outputFileName = url.Substring(startIndex, endIndex - startIndex);

            // URL-decode the output file name
            outputFileName = UnityEngine.Networking.UnityWebRequest.UnEscapeURL(outputFileName);
            
            if (debugMode)
            {
                Debug.Log($"Extracted output file name: {outputFileName}");
            }
        }
        else
        {
            Debug.LogWarning("Could not find output_file parameter in URL for direct download");
            yield break;
        }

        // Build the direct URL to download the file
        string baseUrl = "http://127.0.0.1:7851";
        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            int domainEnd = url.IndexOf("/", 8); // Skip http(s)://
            if (domainEnd > 0)
            {
                baseUrl = url.Substring(0, domainEnd);
            }
        }

        // Create direct audio file URLs (try both with and without extension)
        List<string> filesToTry = new List<string>();
        
        // Try variations of file paths since AllTalk can be inconsistent
        filesToTry.Add($"{baseUrl}/outputs/{outputFileName}.wav");
        filesToTry.Add($"{baseUrl}/outputs/{outputFileName}");
        filesToTry.Add($"{baseUrl}/api/get-file?filename={outputFileName}.wav");
        filesToTry.Add($"{baseUrl}/api/get-file?filename={outputFileName}");
        
        if (debugMode)
        {
            Debug.Log($"Will try to download from the following URLs: {string.Join(", ", filesToTry)}");
        }
        
        AudioClip directClip = null;
        foreach (string fileUrl in filesToTry)
        {
            if (debugMode)
            {
                Debug.Log($"Trying to download audio from: {fileUrl}");
            }
            
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.WAV))
            {
                // Set headers and options
                request.SetRequestHeader("Accept", "audio/*");
                request.timeout = 30; // 30 second timeout
                
                // Start request
                yield return request.SendWebRequest();
                
                // Check for errors
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Failed to download from {fileUrl}: {request.error}");
                    continue; // Try next URL
                }
                
                // Get audio clip
                directClip = DownloadHandlerAudioClip.GetContent(request);
                
                if (directClip != null)
                {
                    directClip.name = "DirectDownload_" + outputFileName;
                    
                    if (debugMode)
                    {
                        Debug.Log($"Successfully downloaded audio directly: {directClip.length}s, {directClip.frequency}Hz");
                    }
                    
                    break; // Success - stop trying more URLs
                }
            }
        }
        
        if (directClip != null && directClip.length > 0)
        {
            // Play the directly downloaded audio
            Action playAction = () => {
                StartCoroutine(PlayStreamedAudioCoroutine(directClip));
            };
            
            if (audioPlayback.IsPlaying)
            {
                // Define the event handler first with the correct signature
                Action completionHandler = null;
                completionHandler = () => {
                    audioPlayback.OnPlaybackCompleted -= completionHandler;
                    playAction();
                };
                
                // Wait for current playback to finish
                audioPlayback.OnPlaybackCompleted += completionHandler;
            }
            else
            {
                // Play immediately
                playAction();
            }
            
            yield return true; // Success
        }
        else
        {
            Debug.LogWarning("All direct download attempts failed");
            yield return false; // Failure
        }
    }

        private AudioType GetAudioTypeFromFormat(string format)
        {
        if (string.IsNullOrEmpty(format))
        {
        Debug.LogWarning("Audio format was null or empty, defaulting to WAV");
        return AudioType.WAV;
        }
        
        switch (format.ToLower().Trim())
        {
        case "wav":
        return AudioType.WAV;
        case "mp3":
        return AudioType.MPEG;
        case "ogg":
                return AudioType.OGGVORBIS;
                case "aac":
                return AudioType.ACC;
            default:
                Debug.LogWarning($"Unknown audio format: {format}, defaulting to WAV");
                return AudioType.WAV;
        }
    }

        private void SendStreamingStatus(string status, string url, string error = null)
        {
            if (_webSocketClient == null) 
            {
                Debug.LogError("Cannot send streaming status: WebSocketClient is null");
                return;
            }
            
            // Ensure we have the latest session ID
            if (sessionManager != null)
            {
                _sessionId = sessionManager.GetSessionId();
            }
            
            try
            {
                StreamingStatusMessage message = new StreamingStatusMessage(
                    _sessionId,
                    status,
                    url,
                    error
                );
                
                string json = JsonUtility.ToJson(message);
                
                // Send the message asynchronously
                _webSocketClient.SendMessage(json);
                
                // IMPORTANT: Send a log that's clearly visible in both Unity and server logs
                string logMessage = $"[STREAMING_STATUS] {_sessionId} - {status} - {url} {(error != null ? "- Error: " + error : "")}";
                
                Debug.Log(logMessage);
                
                if (debugMode)
                {
                    Debug.Log($"Sent streaming status message: {json}");
                }
                
                // If this is a "started" status, also notify Session Manager's playback handler
                if (status == "completed" && sessionManager != null)
                {
                    // This will ensure the server knows playback is complete
                    sessionManager.NotifyPlaybackComplete();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error sending streaming status: {ex.Message}");
                
                // Try sending a simplified message if JSON serialization failed
                try
                {
                    string simpleJson = $"{{\"type\":\"streaming_status\",\"session_id\":\"{_sessionId}\",\"status\":\"{status}\",\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0}}}";
                    _webSocketClient.SendMessage(simpleJson);
                    Debug.Log($"Sent simplified streaming status: {status}");
                }
                catch (Exception fallbackEx)
                {
                    Debug.LogError($"Error sending simplified status: {fallbackEx.Message}");
                }
            }
        }

        /// <summary>
        /// Set preference for streaming vs direct audio
        /// </summary>
        public void SetStreamingPreference(bool prefer)
        {
            preferStreaming = prefer;
            
            // Save to settings
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetSetting("PreferStreaming", prefer);
                SettingsManager.Instance.SaveSettings();
            }
            
            if (debugMode)
            {
                Debug.Log($"Streaming preference set to: {prefer}");
            }
        }
        
        /// <summary>
        /// Get current streaming preference
        /// </summary>
        public bool GetStreamingPreference()
        {
            // Load from settings if available
            if (SettingsManager.Instance != null)
            {
                preferStreaming = SettingsManager.Instance.GetSetting<bool>("PreferStreaming");
            }
            
            return preferStreaming;
        }
    }
}
