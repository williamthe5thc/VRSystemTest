using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VRInterview.Audio;
using VRInterview.Network;

/// <summary>
/// Handles parsing and routing of WebSocket messages between the server and client components.
/// </summary>
public class MessageHandler : MonoBehaviour
{
    [SerializeField] private WebSocketClient webSocketClient;
    [SerializeField] private AudioPlayback audioPlayback;
    [SerializeField] private AvatarController avatarController;
    [SerializeField] private UIManager uiManager;
    
    // Events for different message types
    public event Action<string, string, Dictionary<string, object>> OnStateUpdate;
    public event Action<byte[]> OnAudioResponse;
    public event Action<string> OnError;
    public event Action<MessageHandler, string> MessageReceived;
    
    // Dictionary to store message handlers
    private Dictionary<string, Action<string>> messageHandlers = new Dictionary<string, Action<string>>();
    
    // Store the current transcript and response for reference
    private string _currentUserTranscript;
    private string _currentLLMResponse;
    
    private void Start()
    {
        if (webSocketClient != null)
        {
            webSocketClient.OnMessageReceived += ProcessMessage;
            webSocketClient.OnError += HandleConnectionError;
        }
        else
        {
            Debug.LogError("WebSocketClient not assigned to MessageHandler!");
        }
    }
    
    public void RegisterMessageHandler(string messageType, Action<string> handler)
    {
        if (messageHandlers.ContainsKey(messageType))
        {
            messageHandlers[messageType] += handler;
        }
        else
        {
            messageHandlers[messageType] = handler;
        }
    }
    
    public void UnregisterMessageHandler(string messageType)
    {
        if (messageHandlers.ContainsKey(messageType))
        {
            messageHandlers.Remove(messageType);
        }
    }
    
    /// <summary>
    /// Handles incoming WebSocket messages.
    /// </summary>
    /// <param name="jsonMessage">The JSON message from the server.</param>
    public void ProcessMessage(string jsonMessage)
    {
        // Raise the MessageReceived event
        MessageReceived?.Invoke(this, jsonMessage);
        
        // Update activity timestamp in SessionManager
        var sessionManager = FindObjectOfType<SessionManager>();
        if (sessionManager != null)
        {
            sessionManager.UpdateActivityTimestamp();
        }
        
        // Handle message
        try
        {
            // Parse the JSON message
            JObject messageObj = JObject.Parse(jsonMessage);
            string messageType = messageObj["type"]?.ToString();
            
            // Check if we have a registered handler for this message type
            if (!string.IsNullOrEmpty(messageType) && messageHandlers.ContainsKey(messageType))
            {
                messageHandlers[messageType]?.Invoke(jsonMessage);
                return;
            }
            
            // Route based on message type
            switch (messageType)
            {
                case "session_init":
                    HandleSessionInit(messageObj);
                    break;
                    
                case "state_update":
                    HandleStateUpdate(messageObj);
                    break;
                
                case "audio_response":
                    HandleAudioResponse(messageObj);
                    break;
                
                case "pong":
                    // Heartbeat response, update connection status
                    Debug.Log("Received pong from server");
                    break;
                
                case "heartbeat":
                    // Just acknowledge heartbeat messages
                    Debug.Log("Received heartbeat from server");
                    break;
                
                case "error":
                    HandleError(messageObj);
                    break;
                    
                case "playback_ack":
                    // This is just an acknowledgment from the server, no action needed
                    Debug.Log("Received playback acknowledgment from server");
                    break;
                    
                case "text_response":
                    HandleTextResponse(messageObj);
                    break;
                    
                case "system_message":
                    HandleSystemMessage(messageObj);
                    break;
                    
                case "progress_update":
                    HandleProgressUpdate(messageObj);
                    break;
                
                default:
                    Debug.LogWarning($"Unknown message type: {messageType}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error handling message: {e.Message}");
        }
    }
    
    /// <summary>
    /// Handles state update messages from the server.
    /// </summary>
    /// <param name="messageObj">The JSON message object.</param>
    private void HandleStateUpdate(JObject messageObj)
    {
        string previousState = messageObj["previous"]?.ToString();
        string currentState = messageObj["current"]?.ToString();
        
        // Extract metadata, if any
        Dictionary<string, object> metadata = new Dictionary<string, object>();
        JObject metadataObj = messageObj["metadata"] as JObject;
        if (metadataObj != null)
        {
            foreach (var property in metadataObj.Properties())
            {
                metadata[property.Name] = property.Value.ToObject<object>();
                
                // Check for transcript in metadata
                if (property.Name == "transcript" && property.Value != null)
                {
                    string transcript = property.Value.ToString();
                    _currentUserTranscript = transcript;
                    
                    // Show user transcript in UI
                    if (uiManager != null && !string.IsNullOrEmpty(transcript))
                    {
                        uiManager.ShowUserTranscript(transcript);
                    }
                    
                    Debug.Log($"Found transcript in metadata: {transcript}");
                }
                
                // Check for response in metadata
                if (property.Name == "response" && property.Value != null)
                {
                    string response = property.Value.ToString();
                    _currentLLMResponse = response;
                    
                    // Show LLM response in UI
                    if (uiManager != null && !string.IsNullOrEmpty(response))
                    {
                        uiManager.ShowLLMResponse(response);
                    }
                    
                    Debug.Log($"Found response in metadata: {response}");
                }
            }
        }
        
        Debug.Log($"State update: {previousState} → {currentState}");
        
        // Update UI and other components
        if (uiManager != null)
        {
            uiManager.UpdateStateDisplay(currentState);
        }
        
        // Set avatar state based on conversation state
        if (avatarController != null)
        {
            switch (currentState)
            {
                case "IDLE":
                    avatarController.SetIdleState();
                    break;
                
                case "LISTENING":
                    avatarController.SetListeningState();
                    break;
                
                case "PROCESSING":
                    avatarController.SetThinkingState();
                    break;
                
                case "RESPONDING":
                    avatarController.SetSpeakingState();
                    break;
                
                case "WAITING":
                    avatarController.SetAttentiveState();
                    break;
                
                case "ERROR":
                    avatarController.SetConfusedState();
                    break;
            }
        }
        
        // Broadcast the state change
        OnStateUpdate?.Invoke(previousState, currentState, metadata);
    }
    
    /// <summary>
    /// Handles audio response messages from the server.
    /// </summary>
    /// <param name="messageObj">The JSON message object.</param>
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
            
            // Check for text response in the audio response message
            string textResponse = messageObj["text"]?.ToString();
            if (!string.IsNullOrEmpty(textResponse))
            {
                _currentLLMResponse = textResponse;
                
                // Show LLM response in UI
                if (uiManager != null)
                {
                    uiManager.ShowLLMResponse(textResponse);
                }
                
                Debug.Log($"Found text in audio response: {textResponse.Substring(0, Math.Min(30, textResponse.Length))}...");
            }
            
            Debug.Log($"Received audio response: {base64Audio.Length} characters of base64 data");
            
            // Convert to binary
            byte[] audioData = Convert.FromBase64String(base64Audio);
            Debug.Log($"Decoded audio data: {audioData.Length} bytes");
            
            // Play the audio
            if (audioPlayback != null)
            {
                audioPlayback.PlayAudioResponse(audioData);
                Debug.Log("Started audio playback");
            }
            else
            {
                Debug.LogError("AudioPlayback reference is null in MessageHandler");
            }
            
            // Broadcast the audio response
            OnAudioResponse?.Invoke(audioData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error handling audio response: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Handles error messages from the server.
    /// </summary>
    /// <param name="messageObj">The JSON message object.</param>
    private void HandleError(JObject messageObj)
    {
        string errorMessage = messageObj["message"]?.ToString();
        Debug.LogError($"Server error: {errorMessage}");
        
        // Show error to user
        if (uiManager != null)
        {
            uiManager.ShowError(errorMessage);
        }
        
        // Broadcast the error
        OnError?.Invoke(errorMessage);
    }
    
    /// <summary>
    /// Handles session initialization messages from the server.
    /// </summary>
    /// <param name="messageObj">The JSON message object.</param>
    private void HandleSessionInit(JObject messageObj)
    {
        string serverSessionId = messageObj["session_id"]?.ToString();
        if (!string.IsNullOrEmpty(serverSessionId))
        {
            Debug.Log($"Received server-generated session ID: {serverSessionId}");
            
            // Update SessionManager with server's session ID
            var sessionManager = FindObjectOfType<SessionManager>();
            if (sessionManager != null)
            {
                sessionManager.UpdateSessionId(serverSessionId);
                
                // Send client capabilities with the new session ID
                sessionManager.SendClientCapabilities();
            }
            else
            {
                Debug.LogError("SessionManager not found. Cannot update session ID.");
            }
        }
        else
        {
            Debug.LogError("Received empty session ID from server.");
        }
    }
    
    /// <summary>
    /// Handles connection errors.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    private void HandleConnectionError(string errorMessage)
    {
        Debug.LogError($"WebSocket connection error: {errorMessage}");
        
        // Show error to user
        if (uiManager != null)
        {
            uiManager.ShowError($"Connection error: {errorMessage}");
        }
        
        // Broadcast the error
        OnError?.Invoke($"Connection error: {errorMessage}");
    }
    
    /// <summary>
    /// Handles text response messages from the server.
    /// </summary>
    /// <param name="messageObj">The JSON message object.</param>
    private void HandleTextResponse(JObject messageObj)
    {
        string text = messageObj["text"]?.ToString();
        string responseType = messageObj["response_type"]?.ToString() ?? "llm_response";
        
        Debug.Log($"Received text response: {(text != null ? text.Substring(0, Math.Min(30, text.Length)) : "null")}...");
        
        // Determine if this is a user transcript or an LLM response
        if (responseType == "transcript")
        {
            _currentUserTranscript = text;
            
            // Show user transcript in UI
            if (uiManager != null && !string.IsNullOrEmpty(text))
            {
                uiManager.ShowUserTranscript(text);
            }
        }
        else // Assume it's an LLM response
        {
            _currentLLMResponse = text;
            
            // Show LLM response in UI
            if (uiManager != null && !string.IsNullOrEmpty(text))
            {
                uiManager.ShowLLMResponse(text);
            }
            
            // If avatar controller is available, set speaking state
            if (avatarController != null)
            {
                avatarController.SetSpeakingState();
                
                // Schedule return to attentive state after a delay
                StartCoroutine(ReturnToAttentiveState(5.0f));
            }
        }
    }
    
    /// <summary>
    /// Handles system message notifications from the server.
    /// </summary>
    /// <param name="messageObj">The JSON message object.</param>
    private void HandleSystemMessage(JObject messageObj)
    {
        string message = messageObj["message"]?.ToString();
        
        Debug.Log($"Received system message: {message}");
        
        // Show message in UI
        if (uiManager != null && !string.IsNullOrEmpty(message))
        {
            uiManager.ShowMessage(message);
        }
    }
    
    /// <summary>
    /// Gets the current user transcript.
    /// </summary>
    /// <returns>The current user transcript.</returns>
    public string GetCurrentUserTranscript()
    {
        return _currentUserTranscript;
    }
    
    /// <summary>
    /// Gets the current LLM response.
    /// </summary>
    /// <returns>The current LLM response.</returns>
    public string GetCurrentLLMResponse()
    {
        return _currentLLMResponse;
    }
    
    private IEnumerator ReturnToAttentiveState(float delay = 5)
    {
        yield return new WaitForSeconds(delay);
        
        if (avatarController != null)
        {
            avatarController.SetAttentiveState();
        }
        
        yield return null;
    }
    
    /// <summary>
    /// Handles progress update messages from the server.
    /// </summary>
    /// <param name="messageObj">The JSON message object.</param>
    private void HandleProgressUpdate(JObject messageObj)
    {
        string message = messageObj["message"]?.ToString() ?? "Processing...";
        float progress = 0f;
        
        // Try to parse progress value if present
        if (messageObj["progress"] != null)
        {
            try {
                progress = messageObj["progress"].ToObject<float>();
            } catch {
                // If parsing fails, use indeterminate progress
                progress = -1f;
            }
        }
        
        string state = messageObj["state"]?.ToString() ?? "PROCESSING";
        
        Debug.Log($"Progress update: {message} ({(progress >= 0 ? $"{progress:P0}" : "unknown")})");
        
        // Update UI if available
        if (uiManager != null)
        {
            uiManager.UpdateStatus(message);
            
            // Only update progress bar if we have a valid progress value
            if (progress >= 0)
            {
                uiManager.UpdateProgress(progress);
            }
            
            // Make sure avatar is in thinking state during processing
            if (state.StartsWith("PROCESSING") && avatarController != null)
            {
                avatarController.SetThinkingState();
            }
        }
    }
}