using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class MessageHandlerAdapter : MonoBehaviour
{
    [SerializeField] private MessageHandler messageHandler;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private bool showDebugLogs = true;

    // Message type handlers
    private Dictionary<string, Action<JObject>> messageHandlers = new Dictionary<string, Action<JObject>>();

    private void Start()
    {
        if (messageHandler == null)
        {
            messageHandler = FindObjectOfType<MessageHandler>();
            if (messageHandler == null)
            {
                Debug.LogError("MessageHandlerAdapter couldn't find MessageHandler in the scene");
                return;
            }
        }

        // Register our handler for all messages
        // This uses the existing MessageHandler event system
        messageHandler.MessageReceived += OnMessageReceived;

        // Register our specific message type handlers
        RegisterHandler("progress_update", HandleProgressUpdate);
        RegisterHandler("thinking_update", HandleThinkingUpdate);
        RegisterHandler("transcript_update", HandleTranscriptUpdate);
        RegisterHandler("system_message", HandleSystemMessage);

        LogDebug("MessageHandlerAdapter initialized");
    }

    private void OnDestroy()
    {
        // Unregister from MessageHandler events
        if (messageHandler != null)
        {
            messageHandler.MessageReceived -= OnMessageReceived;
        }
    }

    private void OnMessageReceived(MessageHandler handler, string json)
    {
        try
        {
            JObject data = JObject.Parse(json);
            string messageType = data["type"]?.ToString();

            if (!string.IsNullOrEmpty(messageType) && messageHandlers.ContainsKey(messageType))
            {
                // Call the specific handler for this message type
                messageHandlers[messageType](data);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing message: {ex.Message}");
        }
    }

    // Register a handler for a specific message type
    private void RegisterHandler(string messageType, Action<JObject> handler)
    {
        messageHandlers[messageType] = handler;
    }

    // Individual message type handlers
    private void HandleProgressUpdate(JObject data)
    {
        string message = data["message"]?.ToString() ?? "Processing...";
        LogDebug($"Progress update: {message}");
        
        // Update UI using methods available in the existing UIManager
        if (uiManager != null)
        {
            // Call existing method if it exists, otherwise use notification method
            if (HasMethod(uiManager, "UpdateStatus"))
            {
                uiManager.SendMessage("UpdateStatus", message);
            }
            else if (HasMethod(uiManager, "UpdateProgress"))
            {
                uiManager.SendMessage("UpdateProgress", message);
            }
            else if (HasMethod(uiManager, "ShowNotification"))
            {
                uiManager.SendMessage("ShowNotification", message);
            }
        }
    }

    private void HandleThinkingUpdate(JObject data)
    {
        string message = data["message"]?.ToString() ?? "Thinking...";
        LogDebug($"Thinking update: {message}");
        
        // Update UI using methods available in the existing UIManager
        if (uiManager != null)
        {
            if (HasMethod(uiManager, "UpdateStatus"))
            {
                uiManager.SendMessage("UpdateStatus", $"Interviewer is thinking: {message}");
            }
            else if (HasMethod(uiManager, "ShowFallbackText"))
            {
                uiManager.SendMessage("ShowFallbackText", $"Interviewer is thinking: {message}");
            }
            else if (HasMethod(uiManager, "ShowNotification"))
            {
                uiManager.SendMessage("ShowNotification", message);
            }
        }
    }

    private void HandleTranscriptUpdate(JObject data)
    {
        string transcript = data["transcript"]?.ToString() ?? "";
        string source = data["source"]?.ToString() ?? "system";
        bool delayed = data["delayed"] != null && (bool)data["delayed"];
        
        LogDebug($"Transcript update from {source}: {(transcript.Length > 30 ? transcript.Substring(0, 30) + "..." : transcript)}");
        
        // Handle based on source
        if (source == "user")
        {
            if (uiManager != null)
            {
                if (HasMethod(uiManager, "ShowUserTranscript"))
                {
                    uiManager.SendMessage("ShowUserTranscript", transcript);
                }
                else if (HasMethod(uiManager, "ShowFallbackText"))
                {
                    uiManager.SendMessage("ShowFallbackText", $"You: {transcript}");
                }
            }
            
            if (sessionManager != null && HasMethod(sessionManager, "AddUserMessage"))
            {
                sessionManager.SendMessage("AddUserMessage", transcript);
            }
        }
        else if (source == "llm")
        {
            if (uiManager != null)
            {
                if (HasMethod(uiManager, "ShowLLMResponse"))
                {
                    uiManager.SendMessage("ShowLLMResponse", transcript);
                }
                else if (HasMethod(uiManager, "ShowFallbackText"))
                {
                    uiManager.SendMessage("ShowFallbackText", $"Interviewer: {transcript}");
                }
                
                // If this is a delayed response, show a notification
                if (delayed && HasMethod(uiManager, "UpdateStatus"))
                {
                    uiManager.SendMessage("UpdateStatus", "Delayed response received");
                }
            }
            
            if (sessionManager != null && HasMethod(sessionManager, "AddAssistantMessage"))
            {
                sessionManager.SendMessage("AddAssistantMessage", transcript);
            }
        }
    }

    private void HandleSystemMessage(JObject data)
    {
        string message = data["message"]?.ToString() ?? "";
        LogDebug($"System message: {message}");
        
        if (uiManager != null)
        {
            if (HasMethod(uiManager, "UpdateStatus"))
            {
                uiManager.SendMessage("UpdateStatus", message);
            }
            else if (HasMethod(uiManager, "ShowNotification"))
            {
                uiManager.SendMessage("ShowNotification", message);
            }
        }
    }

    // Helper method to check if a GameObject has a specific method
    private bool HasMethod(UnityEngine.Object obj, string methodName)
    {
        var type = obj.GetType();
        return type.GetMethod(methodName) != null;
    }

    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log(message);
        }
    }
}
