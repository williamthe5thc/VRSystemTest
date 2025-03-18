using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ClientProtocolHandler : MonoBehaviour
{
    [SerializeField] private WebSocketClient webSocketClient;
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private bool showDebugLogs = true;

    private string serverSessionId = null;
    private string clientSessionId = null;
    private bool sessionSynchronized = false;

    private void Start()
    {
        // Generate a client session ID if not set
        if (string.IsNullOrEmpty(clientSessionId))
        {
            clientSessionId = Guid.NewGuid().ToString();
            LogDebug($"Generated client session ID: {clientSessionId}");
        }

        // Register for events
        if (webSocketClient != null)
        {
            webSocketClient.OnConnectionEstablished += HandleConnectionEstablished;
            webSocketClient.OnMessageReceived += HandleMessage;
        }

        // Register for session manager events
        if (sessionManager != null)
        {
            sessionManager.OnSessionStart += HandleSessionStart;
        }
    }

    private void OnDestroy()
    {
        // Unregister from events
        if (webSocketClient != null)
        {
            webSocketClient.OnConnectionEstablished -= HandleConnectionEstablished;
            webSocketClient.OnMessageReceived -= HandleMessage;
        }

        if (sessionManager != null)
        {
            sessionManager.OnSessionStart -= HandleSessionStart;
        }
    }

    public string GetCurrentSessionId()
    {
        // Return server session ID if synchronized, otherwise client ID
        return sessionSynchronized ? serverSessionId : clientSessionId;
    }

    private void HandleConnectionEstablished()
    {
        LogDebug("Connection established, sending client capabilities");
        SendClientCapabilities();
    }

    private void HandleSessionStart(string sessionId)
    {
        LogDebug($"Session started with ID: {sessionId}");
        
        // No need to do anything special here, as we're already handling
        // session ID synchronization in other methods
    }

    private void HandleMessage(string message)
    {
        try
        {
            JObject data = JObject.Parse(message);
            string messageType = data["type"]?.ToString();

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
                sessionManager.UpdateSessionId(serverSessionId);
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
            
            // Update session manager
            if (sessionManager != null)
            {
                sessionManager.UpdateSessionId(serverSessionId);
            }
            
            // Mark as synchronized
            sessionSynchronized = true;
        }
    }

    public void SendClientCapabilities()
    {
        if (webSocketClient == null)
            return;

        // Create client capabilities object
        ClientCapabilities capabilities = new ClientCapabilities
        {
            SupportsStreaming = true,
            AudioFormats = new List<string> { "wav", "mp3" },
            Browser = new Dictionary<string, string>
            {
                { "name", "unity" },
                { "version", Application.unityVersion }
            }
        };

        // Create capabilities message
        ClientCapabilitiesMessage message = new ClientCapabilitiesMessage(
            // Use server session ID if available, otherwise client ID
            sessionSynchronized ? serverSessionId : clientSessionId,
            capabilities
        );

        // Serialize and send
        string json = JsonConvert.SerializeObject(message);
        webSocketClient.SendMessage(json);
        LogDebug($"Sent client capabilities to server with session ID: {message.SessionId}");
    }

    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log(message);
        }
    }
}

/// <summary>
/// Client capabilities information
/// </summary>
[Serializable]
public class ClientCapabilities
{
    [JsonProperty("supports_streaming")]
    public bool SupportsStreaming { get; set; }

    [JsonProperty("audio_formats")]
    public List<string> AudioFormats { get; set; }

    [JsonProperty("browser")]
    public Dictionary<string, string> Browser { get; set; }
}

/// <summary>
/// Message containing client capabilities
/// </summary>
[Serializable]
public class ClientCapabilitiesMessage
{
    [JsonProperty("type")]
    public string Type { get; } = "client_capabilities";

    [JsonProperty("session_id")]
    public string SessionId { get; }

    [JsonProperty("timestamp")]
    public double Timestamp { get; }

    [JsonProperty("capabilities")]
    public ClientCapabilities Capabilities { get; }

    public ClientCapabilitiesMessage(string sessionId, ClientCapabilities capabilities)
    {
        SessionId = sessionId;
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Capabilities = capabilities;
    }
}
