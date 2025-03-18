using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SessionSyncAdapter : MonoBehaviour
{
    [SerializeField] private WebSocketClient webSocketClient;
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private bool showDebugLogs = true;
    
    private MessageHandlerAdapter messageHandlerAdapter;
    private string serverSessionId = null;
    private string clientSessionId = null;
    private bool sessionSynchronized = false;

    private void Start()
    {
        // Find components if not set
        if (webSocketClient == null)
            webSocketClient = FindObjectOfType<WebSocketClient>();
            
        if (sessionManager == null)
            sessionManager = FindObjectOfType<SessionManager>();
            
        // Get or add MessageHandlerAdapter
        messageHandlerAdapter = FindObjectOfType<MessageHandlerAdapter>();
        if (messageHandlerAdapter == null)
        {
            messageHandlerAdapter = gameObject.AddComponent<MessageHandlerAdapter>();
        }
        
        // Generate a client session ID if not set
        if (string.IsNullOrEmpty(clientSessionId))
        {
            clientSessionId = Guid.NewGuid().ToString();
            LogDebug($"Generated client session ID: {clientSessionId}");
        }

        // Add listeners to WebSocketClient events
        if (webSocketClient != null)
        {
            // Use existing events or hooks
            // If the WebSocketClient doesn't have these events, we'll need to find alternatives
            // For now, we'll assume there's a MessageReceived event
            if (HasEvent(webSocketClient, "MessageReceived"))
            {
                // This uses reflection to add our handler to an existing event
                // Only uncomment if you confirm the event exists
                // AddEventHandler(webSocketClient, "MessageReceived", HandleMessage);
            }
            
            // For now, rely on MessageHandlerAdapter for message handling
        }

        // Monitor session start/connect events
        if (sessionManager != null)
        {
            // Look for common session events
            // We'll need to adapt this to whatever events your SessionManager has
        }
        
        LogDebug("SessionSyncAdapter initialized");
    }

    // This is a utility method to get the current session ID
    public string GetCurrentSessionId()
    {
        // Return server session ID if synchronized, otherwise client ID
        return sessionSynchronized ? serverSessionId : clientSessionId;
    }

    // This would be connected to WebSocketClient's message event if available
    private void HandleMessage(string message)
    {
        try
        {
            JObject data = JObject.Parse(message);
            string messageType = data["type"]?.ToString();

            // Handle session-related messages
            if (messageType == "session_init")
            {
                HandleSessionInit(data);
            }
            else if (messageType == "capabilities_ack")
            {
                HandleCapabilitiesAck(data);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing message: {ex.Message}");
        }
    }

    // Process session initialization message
    private void HandleSessionInit(JObject data)
    {
        string newServerSessionId = data["session_id"]?.ToString();
        if (!string.IsNullOrEmpty(newServerSessionId))
        {
            LogDebug($"Received server-generated session ID: {newServerSessionId}");
            
            // Store the server session ID
            serverSessionId = newServerSessionId;
            
            // Try to update session manager with reflection
            if (sessionManager != null && HasMethod(sessionManager, "UpdateSessionId"))
            {
                sessionManager.SendMessage("UpdateSessionId", serverSessionId);
            }
            
            // Mark as synchronized
            sessionSynchronized = true;
            
            // Send capabilities with new ID if method exists
            if (HasMethod(this, "SendClientCapabilities"))
            {
                SendClientCapabilities();
            }
        }
    }

    // Process capabilities acknowledgment
    private void HandleCapabilitiesAck(JObject data)
    {
        LogDebug("Server acknowledged client capabilities");
        
        // Check if the server provided its session ID
        string serverProvidedId = data["server_session_id"]?.ToString();
        if (!string.IsNullOrEmpty(serverProvidedId) && serverProvidedId != serverSessionId)
        {
            LogDebug($"Updating to server-provided session ID: {serverProvidedId}");
            serverSessionId = serverProvidedId;
            
            // Update session manager if method exists
            if (sessionManager != null && HasMethod(sessionManager, "UpdateSessionId"))
            {
                sessionManager.SendMessage("UpdateSessionId", serverSessionId);
            }
            
            // Mark as synchronized
            sessionSynchronized = true;
        }
    }

    // This should be called when the connection is established
    public void SendClientCapabilities()
    {
        if (webSocketClient == null || !HasMethod(webSocketClient, "SendMessage"))
            return;

        try {
            // Create capabilities object compatible with server expectations
            var capabilities = new Dictionary<string, object>
            {
                ["type"] = "client_capabilities",
                ["session_id"] = sessionSynchronized ? serverSessionId : clientSessionId,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["capabilities"] = new Dictionary<string, object>
                {
                    ["supports_streaming"] = true,
                    ["audio_formats"] = new List<string> { "wav", "mp3" },
                    ["browser"] = new Dictionary<string, string>
                    {
                        ["name"] = "unity",
                        ["version"] = Application.unityVersion
                    }
                }
            };

            // Convert to JSON
            string json = JsonConvert.SerializeObject(capabilities);
            
            // Send using existing method
            webSocketClient.SendMessage(json);
            LogDebug($"Sent client capabilities to server with session ID: {(sessionSynchronized ? serverSessionId : clientSessionId)}");
        }
        catch (Exception ex) {
            Debug.LogError($"Error sending capabilities: {ex.Message}");
        }
    }

    // Helper method to check if a GameObject has a specific method
    private bool HasMethod(UnityEngine.Object obj, string methodName)
    {
        var type = obj.GetType();
        return type.GetMethod(methodName) != null;
    }

    // Helper method to check if an object has a specific event
    private bool HasEvent(object obj, string eventName)
    {
        var type = obj.GetType();
        var eventField = type.GetField(eventName, System.Reflection.BindingFlags.Public | 
                                                   System.Reflection.BindingFlags.NonPublic | 
                                                   System.Reflection.BindingFlags.Instance);
        return eventField != null;
    }

    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log(message);
        }
    }
}
