using System;
using System.Threading.Tasks;
using UnityEngine;
using VRInterview.Network;
using VRInterview.Audio;
using NativeWebSocket;

namespace VRInterview.Network
{
    /// <summary>
    /// Enhanced Session Manager for the VR Interview System.
    /// Handles session management with improved session ID synchronization and error recovery.
    /// </summary>
    public class EnhancedSessionManager : MonoBehaviour
    {
        [SerializeField] private WebSocketClient webSocketClient;
        [SerializeField] private AudioPlayback audioPlayback;
        [SerializeField] private float capabilitiesResendInterval = 15f;  // Seconds
        
        // Events
        public event Action<string> OnSessionChanged;
        public event Action<string> OnError;
        
        // Private variables
        private string _sessionId = "";
        private float _capabilitiesTimer = 0f;
        private bool _capabilitiesSent = false;
        private bool _sessionInitReceived = false;
        
        private void Awake()
        {
            // Find components if not set
            if (webSocketClient == null)
                webSocketClient = FindObjectOfType<WebSocketClient>();
                
            if (audioPlayback == null)
                audioPlayback = FindObjectOfType<AudioPlayback>();
                
            // Try to retrieve a saved session ID
            if (PlayerPrefs.HasKey("SessionId"))
            {
                _sessionId = PlayerPrefs.GetString("SessionId");
                Debug.Log($"Retrieved saved session ID: {_sessionId}");
            }
            else
            {
                // Generate a fallback session ID if needed
                _sessionId = Guid.NewGuid().ToString();
                Debug.Log($"Generated fallback session ID: {_sessionId}");
            }
        }
        
        private void Start()
        {
            // Register for WebSocket events
            if (webSocketClient != null)
            {
                webSocketClient.OnConnected += HandleConnected;
                webSocketClient.OnDisconnected += HandleDisconnected;
                webSocketClient.OnMessageReceived += HandleMessage;
                webSocketClient.OnError += HandleError;
            }
            else
            {
                Debug.LogError("WebSocketClient not found! Session management will not function properly.");
            }
        }
        
        private void Update()
        {
            // Periodically resend capabilities if needed
            if (_capabilitiesSent && webSocketClient != null && webSocketClient.IsConnected)
            {
                _capabilitiesTimer += Time.deltaTime;
                if (_capabilitiesTimer >= capabilitiesResendInterval)
                {
                    _capabilitiesTimer = 0f;
                    // Only resend if we haven't received session_init
                    if (!_sessionInitReceived)
                    {
                        Debug.Log("Resending capabilities as session_init not yet received");
                        SendClientCapabilities();
                    }
                }
            }
        }
        
        private void OnDestroy()
        {
            // Unregister from WebSocket events
            if (webSocketClient != null)
            {
                webSocketClient.OnConnected -= HandleConnected;
                webSocketClient.OnDisconnected -= HandleDisconnected;
                webSocketClient.OnMessageReceived -= HandleMessage;
                webSocketClient.OnError -= HandleError;
            }
        }
        
        private void HandleConnected()
        {
            Debug.Log("WebSocket connected, sending client capabilities");
            _sessionInitReceived = false;  // Reset on new connection
            SendClientCapabilities();
        }
        
        private void HandleDisconnected(NativeWebSocket.WebSocketCloseCode closeCode)
        {
        Debug.Log($"WebSocket disconnected: {closeCode}");
            _capabilitiesSent = false;
        }
        
        private void HandleError(string errorMessage)
        {
            Debug.LogError($"WebSocket error: {errorMessage}");
            OnError?.Invoke(errorMessage);
        }
        
        private void HandleMessage(string message)
        {
            try
            {
                // Try to parse as a general message first to get the type
                SimpleMessage simpleMsg = JsonUtility.FromJson<SimpleMessage>(message);
                if (simpleMsg == null || string.IsNullOrEmpty(simpleMsg.type))
                {
                    Debug.LogWarning($"Received message with invalid format: {message}");
                    return;
                }
                
                // Process based on message type
                switch (simpleMsg.type)
                {
                    case "session_init":
                        // Parse as session init message
                        EnhancedSessionInitMessage initMsg = JsonUtility.FromJson<EnhancedSessionInitMessage>(message);
                        HandleSessionInit(initMsg);
                        break;
                        
                    case "state_update":
                        // Process state updates
                        StateUpdateMessage stateMsg = JsonUtility.FromJson<StateUpdateMessage>(message);
                        HandleStateUpdate(stateMsg);
                        break;
                        
                    case "audio_response":
                        // Audio playback is handled elsewhere
                        break;
                        
                    case "audio_stream_url":
                        // Parse as enhanced audio stream URL message
                        EnhancedAudioStreamUrlMessage audioStreamMsg = JsonUtility.FromJson<EnhancedAudioStreamUrlMessage>(message);
                        
                        // Log diagnostics for troubleshooting
                        if (audioStreamMsg != null)
                        {
                            audioStreamMsg.LogDiagnostics();
                        }
                        break;
                        
                    case "system_message":
                    case "error":
                    case "heartbeat":
                        // These are handled by other components
                        break;
                        
                    default:
                        Debug.Log($"Received unhandled message type: {simpleMsg.type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing message: {ex.Message}\nMessage: {message}");
            }
        }
        
        private void HandleSessionInit(EnhancedSessionInitMessage initMsg)
        {
            if (initMsg != null && !string.IsNullOrEmpty(initMsg.session_id))
            {
                // Record that we received a session_init message
                _sessionInitReceived = true;
                
                // Store the server-provided session ID
                string oldSessionId = _sessionId;
                _sessionId = initMsg.session_id;
                
                Debug.Log($"Received session_init message. Updating session ID from {oldSessionId} to {_sessionId}");
                
                // Store in PlayerPrefs for persistence
                PlayerPrefs.SetString("SessionId", _sessionId);
                PlayerPrefs.Save();
                
                // Send capabilities message with the new session ID
                // This ensures the server has our capabilities
                SendClientCapabilities();
                
                // Notify listeners about the session ID change
                OnSessionChanged?.Invoke(_sessionId);
            }
            else
            {
                Debug.LogWarning("Received invalid session_init message");
            }
        }
        
        private void HandleStateUpdate(StateUpdateMessage stateMsg)
        {
            if (stateMsg != null)
            {
                // Log state changes
                Debug.Log($"State change: {stateMsg.previous} -> {stateMsg.current}");
                
                // Process special states or metadata
                if (stateMsg.metadata != null && stateMsg.metadata.ContainsKey("message"))
                {
                    Debug.Log($"State message: {stateMsg.metadata["message"]}");
                }
                
                // If we're in WAITING state, prepare for next interaction
                if (stateMsg.current == "WAITING")
                {
                    // Notify other components if needed
                }
            }
        }
        
        public async void SendClientCapabilities()
        {
            if (webSocketClient != null && webSocketClient.IsConnected)
            {
                try
                {
                    // Create enhanced capabilities object
                    EnhancedClientCapabilities capabilities = new EnhancedClientCapabilities();
                    
                    // Create message with current session ID
                    EnhancedClientCapabilitiesMessage message = new EnhancedClientCapabilitiesMessage(_sessionId, capabilities);
                    
                    // Get the JSON representation using our enhanced ToJson method
                    string json = message.ToJson();
                    
                    // Send the message
                    await webSocketClient.SendMessage(json);
                    
                    // Mark as sent and reset timer
                    _capabilitiesSent = true;
                    _capabilitiesTimer = 0f;
                    
                    Debug.Log($"Sent enhanced client capabilities with session ID: {_sessionId}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error sending client capabilities: {ex.Message}");
                    OnError?.Invoke($"Failed to send capabilities: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("Cannot send capabilities: WebSocket not connected");
            }
        }
        
        public async void NotifyPlaybackComplete()
        {
            if (webSocketClient != null && webSocketClient.IsConnected)
            {
                try
                {
                    // Create playback complete message
                    var message = new PlaybackCompleteMessage
                    {
                        type = "playback_complete",
                        session_id = _sessionId,
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
                    };
                    
                    // Convert to JSON and send
                    string json = JsonUtility.ToJson(message);
                    await webSocketClient.SendMessage(json);
                    
                    Debug.Log("Sent playback complete notification");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error sending playback complete: {ex.Message}");
                }
            }
        }
        
        public string GetSessionId()
        {
            return _sessionId;
        }
        
        public WebSocketClient GetWebSocketClient()
        {
            return webSocketClient;
        }
        
        // Helper classes for message parsing
        
        [Serializable]
        private class SimpleMessage
        {
            public string type;
        }
        
        [Serializable]
        private class StateUpdateMessage
        {
            public string type;
            public string session_id;
            public string previous;
            public string current;
            public double timestamp;
            public SerializableDictionary metadata;
        }
        
        [Serializable]
        private class SerializableDictionary
        {
            public string[] keys;
            public string[] values;
            
            public bool ContainsKey(string key)
            {
                if (keys == null) return false;
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i] == key) return true;
                }
                return false;
            }
            
            public string this[string key]
            {
                get
                {
                    if (keys == null || values == null) return null;
                    for (int i = 0; i < keys.Length; i++)
                    {
                        if (keys[i] == key) return values[i];
                    }
                    return null;
                }
            }
        }
        
        [Serializable]
        private class PlaybackCompleteMessage
        {
            public string type;
            public string session_id;
            public double timestamp;
        }
    }
}
