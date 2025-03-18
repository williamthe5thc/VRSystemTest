using System;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json.Linq;
using VRInterview.Network;

/// <summary>
/// Extends the MessageHandler with additional functionality for handling text responses
/// </summary>
public static class MessageHandlerExtensions
{
    /// <summary>
    /// Patches the MessageHandler to support text_response messages.
    /// </summary>
    /// <param name="handler">The MessageHandler to patch</param>
    public static void PatchForTextResponses(this MessageHandler handler)
    {
        Debug.Log("Patching MessageHandler to support text_response messages");
        
        // Replace HandleMessage with enhanced version by using a custom event handler
        WebSocketClient webSocketClient = handler.GetComponent<WebSocketClient>();
        if (webSocketClient != null)
        {
            try {
                // We can't access the protected HandleMessage method directly, 
                // so we'll add our own handler to the WebSocketClient
                webSocketClient.OnMessageReceived += (message) => {
                    try {
                        // Parse the JSON message
                        JObject messageObj = JObject.Parse(message);
                        string messageType = messageObj["type"]?.ToString();
                        
                        if (messageType == "text_response")
                        {
                            // Handle text response with our own method
                            HandleTextResponse(handler, messageObj);
                        }
                        // Otherwise, let the normal message processing happen
                        // The original handler is still attached to OnMessageReceived
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error in message handler extension: {e.Message}");
                    }
                };
                
                Debug.Log("MessageHandler patched successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to patch MessageHandler: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("WebSocketClient not found, cannot patch MessageHandler");
        }
    }
    
    /// <summary>
    /// Handles text response messages from the server.
    /// </summary>
    /// <param name="handler">The MessageHandler instance</param>
    /// <param name="messageObj">The JSON message object</param>
    private static void HandleTextResponse(MessageHandler handler, JObject messageObj)
    {
        try
        {
            string textResponse = messageObj["text"]?.ToString();
            if (string.IsNullOrEmpty(textResponse))
            {
                Debug.LogWarning("Received text_response with no text");
                return;
            }
            
            Debug.Log($"Received text_response: {textResponse.Length} characters");
            
            // Find UI manager if available
            UIManager uiManager = handler.GetComponent<UIManager>();
            if (uiManager == null)
            {
                uiManager = handler.GetComponentInChildren<UIManager>();
                if (uiManager == null)
                {
                    uiManager = GameObject.FindObjectOfType<UIManager>();
                }
            }
            
            // Show text in UI
            if (uiManager != null)
            {
                uiManager.ShowFallbackText(textResponse);
                Debug.Log("Displayed text response in UI");
            }
            else
            {
                Debug.LogWarning("UIManager not found, cannot display text response");
            }
            
            // Set avatar to speaking state if available
            AvatarController avatarController = handler.GetComponent<AvatarController>();
            if (avatarController == null)
            {
                avatarController = handler.GetComponentInChildren<AvatarController>();
                if (avatarController == null)
                {
                    avatarController = GameObject.FindObjectOfType<AvatarController>();
                }
            }
            
            if (avatarController != null)
            {
                avatarController.SetSpeakingState();
                Debug.Log("Set avatar to speaking state");
                
                // Start coroutine to reset state
                // Calculate approximate reading time based on text length
                int wordCount = textResponse.Split(new char[] { ' ', '\n', '\t' }, 
                    StringSplitOptions.RemoveEmptyEntries).Length;
                float readingTime = Mathf.Max(3.0f, wordCount / 4.0f); // At least 3 seconds
                
                handler.StartCoroutine(DelayedAction(() => {
                    avatarController.SetAttentiveState();
                    Debug.Log("Reset avatar to attentive state");
                }, readingTime));
            }
            
            // Find audio playback if available
            AudioPlayback audioPlayback = handler.GetComponent<AudioPlayback>();
            if (audioPlayback == null)
            {
                audioPlayback = handler.GetComponentInChildren<AudioPlayback>();
                if (audioPlayback == null)
                {
                    audioPlayback = GameObject.FindObjectOfType<AudioPlayback>();
                }
            }
            
            // Try to call OnPlaybackComplete method to move conversation forward
            if (audioPlayback != null)
            {
                // Calculate delay based on text length
                int textWordCount = textResponse.Split(new char[] { ' ', '\n', '\t' }, 
                    StringSplitOptions.RemoveEmptyEntries).Length;
                float playbackDelay = Mathf.Max(3.0f, textWordCount / 3.0f); // At least 3 seconds
                
                // Use reflection to find and call the OnPlaybackComplete method if it exists
                handler.StartCoroutine(DelayedAction(() => {
                    // Try different method names that might be used for playback completion
                    try {
                        var methods = audioPlayback.GetType().GetMethods(
                            System.Reflection.BindingFlags.Public | 
                            System.Reflection.BindingFlags.NonPublic | 
                            System.Reflection.BindingFlags.Instance
                        );
                        
                        foreach (var method in methods)
                        {
                            if (method.Name.Contains("Playback") && method.Name.Contains("Complete"))
                            {
                                method.Invoke(audioPlayback, null);
                                Debug.Log($"Called playback completion method: {method.Name}");
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Error calling playback completion method: {e.Message}");
                    }
                }, playbackDelay));
            }
            
            // Emit event if any listeners
            if (handler is ITextResponseHandler textHandler)
            {
                textHandler.OnTextResponseReceived(textResponse);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error handling text_response: {e.Message}");
        }
    }
    
    /// <summary>
    /// Helper for delayed actions.
    /// </summary>
    private static IEnumerator DelayedAction(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
}

/// <summary>
/// Interface for objects that can handle text responses
/// </summary>
public interface ITextResponseHandler
{
    void OnTextResponseReceived(string text);
}