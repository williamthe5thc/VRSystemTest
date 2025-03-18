using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class ProgressHandler : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private bool showDebugMessages = true;

    private MessageHandler messageHandler;

    private void Start()
    {
        // Get the message handler reference
        messageHandler = FindObjectOfType<MessageHandler>();
        if (messageHandler == null)
        {
            Debug.LogError("ProgressHandler could not find MessageHandler in the scene");
            return;
        }

        // Register for message handling
        messageHandler.RegisterMessageHandler("progress_update", OnProgressUpdate);
        messageHandler.RegisterMessageHandler("thinking_update", OnThinkingUpdate);
        messageHandler.RegisterMessageHandler("transcript_update", OnTranscriptUpdate);

        LogDebug("ProgressHandler initialized");
    }

    /// <summary>
    /// Handles progress update messages during LLM processing
    /// </summary>
    private void OnProgressUpdate(string jsonMessage)
    {
        try
        {
            JObject data = JObject.Parse(jsonMessage);
            string message = data["message"]?.ToString() ?? "Processing...";
            LogDebug($"Progress update: {message}");
            
            // Update the UI with the progress message
            if (uiManager != null)
            {
                uiManager.ShowProcessingMessage(message);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error handling progress update: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles thinking update messages during LLM processing
    /// </summary>
    private void OnThinkingUpdate(string jsonMessage)
    {
        try
        {
            JObject data = JObject.Parse(jsonMessage);
            string message = data["message"]?.ToString() ?? "Thinking...";
            LogDebug($"Thinking update: {message}");
            
            // Update the UI with the thinking message
            if (uiManager != null)
            {
                uiManager.ShowThinkingMessage(message);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error handling thinking update: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles transcript update messages from the server
    /// </summary>
    private void OnTranscriptUpdate(string jsonMessage)
    {
        try
        {
            JObject data = JObject.Parse(jsonMessage);
            string transcript = data["transcript"]?.ToString() ?? "";
            string source = data["source"]?.ToString() ?? "system";
            bool delayed = data["delayed"] != null && (bool)data["delayed"];
            
            LogDebug($"Transcript update from {source}: {transcript.Substring(0, Math.Min(30, transcript.Length))}...");
            
            // Handle based on source
            if (source == "user")
            {
                if (uiManager != null)
                {
                    uiManager.ShowUserTranscript(transcript);
                }
                
                if (sessionManager != null)
                {
                    try
                    {
                        var method = sessionManager.GetType().GetMethod("AddUserMessage");
                        if (method != null)
                        {
                            method.Invoke(sessionManager, new object[] { transcript });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error calling AddUserMessage: {ex.Message}");
                    }
                }
            }
            else if (source == "llm")
            {
                if (uiManager != null)
                {
                    uiManager.ShowLLMResponse(transcript);
                }
                
                if (sessionManager != null)
                {
                    try
                    {
                        var method = sessionManager.GetType().GetMethod("AddAssistantMessage");
                        if (method != null)
                        {
                            method.Invoke(sessionManager, new object[] { transcript });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error calling AddAssistantMessage: {ex.Message}");
                    }
                }
                
                // If this is a delayed response, show a notification
                if (delayed && uiManager != null)
                {
                    uiManager.ShowNotification("Delayed response received");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error handling transcript update: {ex.Message}");
        }
    }

    private void LogDebug(string message)
    {
        if (showDebugMessages)
        {
            Debug.Log(message);
        }
    }
}