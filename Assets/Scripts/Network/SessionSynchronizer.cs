using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(WebSocketClient))]
public class SessionSynchronizer : MonoBehaviour
{
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private bool showDebugLogs = true;

    private WebSocketClient webSocketClient;
    private string serverSessionId = null;
    private string clientSessionId = null;
    private bool sessionSynchronized = false;
    
    private const string SESSION_INIT_TYPE = "session_init";
    private const string CAPABILITIES_ACK_TYPE = "capabilities_ack";

    private void Awake()
    {
        // Get the WebSocketClient component
        webSocketClient = GetComponent<WebSocketClient>();
        if (webSocketClient == null)
        {
            Debug.LogError("SessionSynchronizer requires a WebSocketClient component");
            enabled = false;
            return;
        }
        
        // Generate a client session ID if not set
        if (string.IsNullOrEmpty(clientSessionId))
        {
            clientSessionId = Guid.NewGuid().ToString();
            LogDebug($"Generated client session ID: {clientSessionId}");
        }
    }

    private void OnEnable()
    {
        // Subscribe to WebSocketClient events
        if (webSocketClient != null)
        {
            webSocketClient.MessageReceived += OnWebSocketMessageReceived;
            
            // If there's a Connected event in your WebSocketClient, use it (may require reflection)
            try
            {
                var connectedEvent = webSocketClient.GetType().GetEvent("Connected");
                if (connectedEvent != null)
                {
                    // Create a delegate to handle connection events
                    System.Delegate connectionHandler = Delegate.CreateDelegate(
                        connectedEvent.EventHandlerType, 
                        this,
                        GetType().GetMethod("OnConnected", 
                            System.Reflection.BindingFlags.Instance | 
                            System.Reflection.BindingFlags.NonPublic)
                    );
                    
                    connectedEvent.AddEventHandler(webSocketClient, connectionHandler);
                    LogDebug("Successfully subscribed to WebSocketClient.Connected event");
                }
                else
                {
                    LogDebug("WebSocketClient does not have a Connected event, will try alternative approach");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to subscribe to WebSocketClient.Connected event: {ex.Message}");
            }
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from WebSocketClient events
        if (webSocketClient != null)
        {
            webSocketClient.MessageReceived -= OnWebSocketMessageReceived;
            
            // Attempt to unsubscribe from Connected event if it exists
            try
            {
                var connectedEvent = webSocketClient.GetType().GetEvent("Connected");
                if (connectedEvent != null)
                {
                    // Create a delegate to handle connection events
                    System.Delegate connectionHandler = Delegate.CreateDelegate(
                        connectedEvent.EventHandlerType, 
                        this,
                        GetType().GetMethod("OnConnected", 
                            System.Reflection.BindingFlags.Instance | 
                            System.Reflection.BindingFlags.NonPublic)
                    );
                    
                    connectedEvent.RemoveEventHandler(webSocketClient, connectionHandler);
                }
            }
            catch (Exception)
            {
                // Ignore errors during cleanup
            }
        }
    }
    
    // Will be called via reflection if WebSocketClient has a Connected event
    private void OnConnected()
    {
        LogDebug("Connection established, sending client capabilities");
        SendClientCapabilities();
    }

    // Alternative way to detect connection events
    private void Update()
    {
        // Check if WebSocketClient is connected via public property
        if (webSocketClient != null && 
            webSocketClient.IsConnected && 
            !sessionSynchronized && 
            Time.frameCount % 30 == 0)  // Only check every 30 frames to avoid spamming
        {
            SendClientCapabilities();
        }
    }

    public string GetCurrentSessionId()
    {
        // Return server session ID if synchronized, otherwise client ID
        return sessionSynchronized ? serverSessionId : clientSessionId;
    }

    private void OnWebSocketMessageReceived(string message)
    {
        try
        {
            JObject data = JObject.Parse(message);
            string messageType = data["type"]?.ToString();

            if (messageType == SESSION_INIT_TYPE)
            {
                HandleSessionInit(data);
            }
            else if (messageType == CAPABILITIES_ACK_TYPE)
            {
                HandleCapabilitiesAck(data);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error parsing protocol message: {ex.Message}");
        }
    }

    private void HandleSessionInit(JObject data)
    {
        string newServerSessionId = data["session_id"]?.ToString();
        if (!string.IsNullOrEmpty(newServerSessionId))
        {
            LogDebug($"Received server-generated session ID: {newServerSessionId}");
            
            // Store the server session ID
            serverSessionId = newServerSessionId;
            
            // Update session manager
            if (sessionManager != null)
            {
                // Try to call UpdateSessionId using reflection if it exists
                try
                {
                    var updateMethod = sessionManager.GetType().GetMethod("UpdateSessionId");
                    if (updateMethod != null)
                    {
                        updateMethod.Invoke(sessionManager, new object[] { serverSessionId });
                        LogDebug("Updated SessionManager with new session ID");
                    }
                    else
                    {
                        // Try to set a sessionId field or property directly
                        var sessionIdProp = sessionManager.GetType().GetProperty("SessionId");
                        if (sessionIdProp != null && sessionIdProp.CanWrite)
                        {
                            sessionIdProp.SetValue(sessionManager, serverSessionId);
                            LogDebug("Set SessionManager.SessionId property");
                        }
                        else
                        {
                            var sessionIdField = sessionManager.GetType().GetField("sessionId", 
                                System.Reflection.BindingFlags.Instance | 
                                System.Reflection.BindingFlags.Public | 
                                System.Reflection.BindingFlags.NonPublic);
                                
                            if (sessionIdField != null)
                            {
                                sessionIdField.SetValue(sessionManager, serverSessionId);
                                LogDebug("Set sessionId field directly");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to update SessionManager session ID: {ex.Message}");
                }
            }
            
            // Mark as synchronized
            sessionSynchronized = true;
            
            // Send capabilities with the new session ID
            SendClientCapabilities();
        }
    }

    private void HandleCapabilitiesAck(JObject data)
    {
        LogDebug("Server acknowledged client capabilities");
        
        // Check if the server provided its session ID
        string serverProvidedId = data["server_session_id"]?.ToString();
        if (!string.IsNullOrEmpty(serverProvidedId) && serverProvidedId != serverSessionId)
        {
            LogDebug($"Updating to server-provided session ID: {serverProvidedId}");
            serverSessionId = serverProvidedId;
            
            // Update session manager (see HandleSessionInit for details)
            if (sessionManager != null)
            {
                try
                {
                    var updateMethod = sessionManager.GetType().GetMethod("UpdateSessionId");
                    if (updateMethod != null)
                    {
                        updateMethod.Invoke(sessionManager, new object[] { serverSessionId });
                    }
                }
                catch (Exception) { /* Ignore errors */ }
            }
            
            // Mark as synchronized
            sessionSynchronized = true;
        }
    }

    public void SendClientCapabilities()
    {
        if (webSocketClient == null || !webSocketClient.IsConnected)
            return;

        try
        {
            // Create simplified capabilities object compatible with your server
            var message = new Dictionary<string, object>
            {
                { "type", "client_capabilities" },
                { "session_id", sessionSynchronized ? serverSessionId : clientSessionId },
                { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "capabilities", new Dictionary<string, object>
                    {
                        { "supports_streaming", true },
                        { "audio_formats", new[] { "wav", "mp3" } },
                        { "browser", new Dictionary<string, string>
                            {
                                { "name", "unity" },
                                { "version", Application.unityVersion }
                            }
                        }
                    }
                }
            };

            // Serialize and send
            string json = JsonConvert.SerializeObject(message);
            webSocketClient.SendMessage(json);
            LogDebug($"Sent client capabilities to server with session ID: {message["session_id"]}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending client capabilities: {ex.Message}");
        }
    }

    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[SessionSync] {message}");
        }
    }
}
