using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class ProgressHandlerAdapter : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private MessageHandler messageHandler;
    [SerializeField] private bool showDebugMessages = true;

    private void Start()
    {
        if (messageHandler == null)
        {
            messageHandler = FindObjectOfType<MessageHandler>();
            if (messageHandler == null)
            {
                Debug.LogError("ProgressHandlerAdapter could not find MessageHandler in the scene");
                return;
            }
        }

        // Add event handler to message handler
        if (messageHandler != null)
        {
            messageHandler.MessageReceived += OnMessageReceived;
            LogDebug("ProgressHandlerAdapter initialized and listening for messages");
        }
    }

    private void OnDestroy()
    {
        // Clean up event handler
        if (messageHandler != null)
        {
            messageHandler.MessageReceived -= OnMessageReceived;
        }
    }

    private void OnMessageReceived(MessageHandler handler, string messageJson)
    {
        try
        {
            // Parse the message
            JObject data = JObject.Parse(messageJson);
            string messageType = data["type"]?.ToString();

            // Handle different message types
            switch (messageType)
            {
                case "progress_update":
                    HandleProgressUpdate(data);
                    break;
                case "thinking_update":
                    HandleThinkingUpdate(data);
                    break;
                case "transcript_update":
                    HandleTranscriptUpdate(data);
                    break;
                case "system_message":
                    HandleSystemMessage(data);
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing message: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles progress update messages during LLM processing
    /// </summary>
    private void HandleProgressUpdate(JObject data)
    {
        string message = data["message"]?.ToString() ?? "Processing...";
        LogDebug($"Progress update: {message}");
        
        // Update the UI
        if (uiManager != null)
        {
            // Use existing methods that are compatible with your UIManager
            uiManager.UpdateStatus($"Processing: {message}");
            
            // If you have a progress bar
            uiManager.UpdateProgress(0.5f); // Use an indeterminate progress
        }
    }

    /// <summary>
    /// Handles thinking update messages during LLM processing
    /// </summary>
    private void HandleThinkingUpdate(JObject data)
    {
        string message = data["message"]?.ToString() ?? "Thinking...";
        LogDebug($"Thinking update: {message}");
        
        // Update the UI
        if (uiManager != null)
        {
            uiManager.UpdateStatus($"Thinking: {message}");
            
            // Display as fallback text to show it prominently
            string formattedMessage = $"<color=#FFD700>Interviewer is thinking:</color> {message}";
            uiManager.ShowFallbackText(formattedMessage);
        }
    }

    /// <summary>
    /// Handles transcript update messages from the server
    /// </summary>
    private void HandleTranscriptUpdate(JObject data)
    {
        string transcript = data["transcript"]?.ToString() ?? "";
        string source = data["source"]?.ToString() ?? "system";
        bool delayed = data["delayed"] != null && (bool)data["delayed"];
        
        LogDebug($"Transcript update from {source}: {transcript.Substring(0, Math.Min(30, transcript.Length))}...");
        
        // Handle based on source
        if (source == "user")
        {
            if (uiManager != null)
            {
                // Show transcript in your existing UI format
                uiManager.ShowFallbackText($"<color=#88AAFF>You:</color> {transcript}");
            }
            
            if (sessionManager != null && sessionManager.HasMethod("AddUserMessage"))
            {
                // Call AddUserMessage method if it exists using reflection
                System.Reflection.MethodInfo method = sessionManager.GetType().GetMethod("AddUserMessage");
                if (method != null)
                {
                    method.Invoke(sessionManager, new object[] { transcript });
                }
            }
        }
        else if (source == "llm")
        {
            if (uiManager != null)
            {
                // Show transcript in your existing UI format
                string delayedTag = delayed ? " <color=#FF8888>(Delayed)</color>" : "";
                uiManager.ShowFallbackText($"<color=#AAFFAA>Interviewer{delayedTag}:</color> {transcript}");
                
                // Update status
                uiManager.UpdateStatus("Received response");
            }
            
            if (sessionManager != null && sessionManager.HasMethod("AddAssistantMessage"))
            {
                // Call AddAssistantMessage method if it exists using reflection
                System.Reflection.MethodInfo method = sessionManager.GetType().GetMethod("AddAssistantMessage");
                if (method != null)
                {
                    method.Invoke(sessionManager, new object[] { transcript });
                }
            }
        }
    }

    /// <summary>
    /// Handles system messages from the server
    /// </summary>
    private void HandleSystemMessage(JObject data)
    {
        string message = data["message"]?.ToString() ?? "System message";
        LogDebug($"System message: {message}");
        
        if (uiManager != null)
        {
            uiManager.UpdateStatus(message);
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

// Extension method to check if a method exists on an object
public static class MonoBehaviourExtensions
{
    public static bool HasMethod(this MonoBehaviour behaviour, string methodName)
    {
        return behaviour.GetType().GetMethod(methodName) != null;
    }
}
