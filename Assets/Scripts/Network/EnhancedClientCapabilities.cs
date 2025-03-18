using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRInterview.Network
{
    /// <summary>
    /// Enhanced client capabilities detection and reporting for the VR Interview System.
    /// This class detects and reports device capabilities such as audio streaming support
    /// and available audio formats to the server with improved JSON handling.
    /// </summary>
    [Serializable]
    public class EnhancedClientCapabilities
    {
        [Serializable]
        public class BrowserInfo
        {
            public string name = "unity";
            public string version;
        }

        public bool supports_streaming = true;
        public List<string> audio_formats = new List<string>() { "wav", "mp3" };
        public BrowserInfo browser = new BrowserInfo();

        public EnhancedClientCapabilities()
        {
            // Get Unity version for reporting
            browser.version = Application.unityVersion;
            
            // Detect if the client supports streaming
            DetectStreamingSupport();
            
            // Detect supported audio formats
            DetectSupportedFormats();
        }

        private void DetectStreamingSupport()
        {
            // For Oculus Quest, streaming is generally supported
            // We could add more sophisticated detection based on OS/device
            supports_streaming = true;
            
#if UNITY_ANDROID && !UNITY_EDITOR
            // Check if we're on older Oculus Quest hardware
            if (SystemInfo.deviceModel.Contains("Quest 1"))
            {
                // Quest 1 might have more limitations, but still generally supports streaming
                supports_streaming = true;
            }
#endif

            // Check for specific environment variables or settings
            if (PlayerPrefs.HasKey("DisableStreaming") && PlayerPrefs.GetInt("DisableStreaming") == 1)
            {
                Debug.Log("Streaming disabled by PlayerPrefs setting");
                supports_streaming = false;
            }

            Debug.Log($"StreamingSupport detection result: {supports_streaming}");
        }

        private void DetectSupportedFormats()
        {
            // Clear the list and add known supported formats
            audio_formats.Clear();
            
            // WAV format is always supported
            audio_formats.Add("wav");
            
            // MP3 format is supported
            audio_formats.Add("mp3");
            
            // Add additional formats as needed
#if UNITY_ANDROID && !UNITY_EDITOR
            // Oculus Quest supports these formats
            audio_formats.Add("ogg");
            audio_formats.Add("aac");
#endif

            Debug.Log($"Detected audio formats: {string.Join(", ", audio_formats)}");
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }

    /// <summary>
    /// Enhanced message class for sending client capabilities to the server with improved reliability
    /// </summary>
    [Serializable]
    public class EnhancedClientCapabilitiesMessage
    {
        [Serializable]
        public class CapabilitiesRoot
        {
            public EnhancedClientCapabilities capabilities;
        }

        public string type = "client_capabilities";
        public string session_id;
        public double timestamp;
        public EnhancedClientCapabilities capabilities;
        public string debug_info;

        public EnhancedClientCapabilitiesMessage(string sessionId, EnhancedClientCapabilities capabilities)
        {
            this.type = "client_capabilities";
            this.session_id = sessionId;
            this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
            this.capabilities = capabilities;
            
            // Add debug info to help troubleshoot capabilities issues
            this.debug_info = $"Unity {Application.unityVersion}, {SystemInfo.deviceModel}, OS {SystemInfo.operatingSystem}";
            
            // Log debug information
            Debug.Log($"Created client capabilities message with session_id: {sessionId}");
            Debug.Log($"Capabilities: streaming={capabilities.supports_streaming}, formats={string.Join(",", capabilities.audio_formats)}");
            Debug.Log($"Message JSON: {JsonUtility.ToJson(this)}");
        }
        
        public string ToJson()
        {
            try {
                // First try direct serialization
                string json = JsonUtility.ToJson(this);
                
                // Verify the JSON is valid by checking for supports_streaming field
                if (json.Contains("supports_streaming"))
                {
                    Debug.Log($"Direct serialization successful: {json}");
                    return json;
                }
                
                // If the direct approach didn't include capabilities, try nesting
                CapabilitiesRoot root = new CapabilitiesRoot();
                root.capabilities = this.capabilities;
                string rootJson = JsonUtility.ToJson(root);
                
                // Extract the capabilities part and manually construct the full message
                int startIndex = rootJson.IndexOf("{", rootJson.IndexOf("capabilities")) + 1;
                int endIndex = rootJson.LastIndexOf("}") - 1;
                string capsJson = rootJson.Substring(startIndex, endIndex - startIndex);
                
                // Construct correct JSON manually as fallback
                string manualJson = $"{{\"type\":\"{type}\",\"session_id\":\"{session_id}\",\"timestamp\":{timestamp},\"capabilities\":{{{capsJson}}},\"debug_info\":\"{debug_info}\"}}";
                Debug.Log($"Manual JSON construction: {manualJson}");
                
                return manualJson;
            }
            catch (Exception ex) {
                Debug.LogError($"Error serializing capabilities: {ex.Message}");
                // Last resort fallback - simple JSON with core fields
                return $"{{\"type\":\"{type}\",\"session_id\":\"{session_id}\",\"timestamp\":{timestamp},\"capabilities\":{{\"supports_streaming\":true,\"audio_formats\":[\"wav\",\"mp3\"],\"browser\":{{\"name\":\"unity\",\"version\":\"{Application.unityVersion}\"}}}}}}";
            }
        }
    }

    /// <summary>
    /// Enhanced message class for streaming status updates with improved diagnostics
    /// </summary>
    [Serializable]
    public class EnhancedStreamingStatusMessage
    {
        public string type = "streaming_status";
        public string session_id;
        public double timestamp;
        public string status; // "started", "failed", or "completed"
        public string url;
        public string error;
        public string debug_info;

        public EnhancedStreamingStatusMessage(string sessionId, string status, string url, string error = null)
        {
            this.session_id = sessionId;
            this.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
            this.status = status;
            this.url = url;
            this.error = error;
            
            // Add debug info for better diagnostics
            this.debug_info = $"Unity {Application.unityVersion}, {SystemInfo.deviceModel}, Time: {DateTime.Now.ToString("HH:mm:ss.fff")}";
            
            // Log creation for debugging
            Debug.Log($"Created streaming status message: Session={sessionId}, Status={status}, URL={url?.Substring(0, Math.Min(50, url?.Length ?? 0))}...");
        }
        
        public string ToJson()
        {
            try {
                return JsonUtility.ToJson(this);
            }
            catch (Exception ex) {
                Debug.LogError($"Error serializing streaming status: {ex.Message}");
                // Fallback - construct manually
                return $"{{\"type\":\"{type}\",\"session_id\":\"{session_id}\",\"timestamp\":{timestamp},\"status\":\"{status}\",\"url\":\"{url}\",\"error\":\"{error}\"}}";
            }
        }
    }

    /// <summary>
    /// Enhanced message class for receiving audio stream URL from server with additional validation
    /// </summary>
    [Serializable]
    public class EnhancedAudioStreamUrlMessage
    {
        public string type = "audio_stream_url";
        public string session_id;
        public double timestamp;
        public string url;
        public string format;
        public string text;
        
        // Additional field to help validate messages
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(session_id);
        }
        
        // Helper method to log diagnostic information
        public void LogDiagnostics()
        {
            Debug.Log($"Audio Stream URL Message Diagnostics:");
            Debug.Log($"  - Session ID: {session_id}");
            Debug.Log($"  - URL: {url?.Substring(0, Math.Min(50, url?.Length ?? 0))}...");
            Debug.Log($"  - Format: {format}");
            Debug.Log($"  - Has text fallback: {!string.IsNullOrEmpty(text)}");
            
            // Try to validate the URL
            if (!string.IsNullOrEmpty(url)) {
                bool hasHttp = url.StartsWith("http://") || url.StartsWith("https://");
                bool hasTtsEndpoint = url.Contains("/api/tts-generate-streaming");
                bool hasTextParam = url.Contains("text=");
                bool hasVoiceParam = url.Contains("voice=");
                bool hasOutputFileParam = url.Contains("output_file=");
                
                Debug.Log($"  - URL validation: HTTP prefix={hasHttp}, TTS endpoint={hasTtsEndpoint}, " +
                          $"text param={hasTextParam}, voice param={hasVoiceParam}, output_file param={hasOutputFileParam}");
            }
        }
    }
    
    /// <summary>
    /// Enhanced session initialization message to ensure client-server session ID synchronization
    /// </summary>
    [Serializable]
    public class EnhancedSessionInitMessage
    {
        public string type = "session_init";  
        public string session_id;
        public double timestamp;
        
        public void LogReceived()
        {
            Debug.Log($"Received session_init message with session ID: {session_id}");
        }
    }
}
