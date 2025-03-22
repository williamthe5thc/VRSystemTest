using System;
using System.Text;
using UnityEngine;
using Newtonsoft.Json.Linq;
using VRInterview.Network;

namespace VRInterview.Network
{
    public static class MessageValidator
    {
        /// <summary>
        /// Validates and ensures an outgoing message has required fields
        /// </summary>
        /// <param name="jsonMessage">The JSON message to validate</param>
        /// <returns>Validated and possibly fixed message</returns>
        public static string ValidateOutgoingMessage(string jsonMessage)
        {
            try
            {
                // Parse the message to check for required fields
                JObject messageObj = JObject.Parse(jsonMessage);
                
                // Check for type field
                if (!messageObj.ContainsKey("type"))
                {
                    Debug.LogWarning("Message missing 'type' field - adding generic type");
                    messageObj["type"] = "generic_message";
                }
                
                // Check for session_id if applicable
                if (messageObj["type"].ToString() != "session_init" && 
                    !messageObj.ContainsKey("session_id"))
                {
                    // Try to get session from SessionManager first
                    var sessionManager = GameObject.FindObjectOfType<SessionManager>();
                    if (sessionManager != null)
                    {
                        messageObj["session_id"] = sessionManager.GetSessionId();
                        Debug.Log($"Added session ID from SessionManager: {sessionManager.GetSessionId()}");
                    }
                    else
                    {
                        // Try EnhancedSessionManager as fallback
                        var enhancedSessionManager = GameObject.FindObjectOfType<EnhancedSessionManager>();
                        if (enhancedSessionManager != null)
                        {
                            messageObj["session_id"] = enhancedSessionManager.GetSessionId();
                            Debug.Log($"Added session ID from EnhancedSessionManager: {enhancedSessionManager.GetSessionId()}");
                        }
                        else
                        {
                            Debug.LogWarning("Message missing 'session_id' field and can't find SessionManager");
                        }
                    }
                }
                
                // Add timestamp if missing
                if (!messageObj.ContainsKey("timestamp"))
                {
                    messageObj["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
                }
                
                // Return the validated message
                return messageObj.ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error validating message: {ex.Message}");
                return jsonMessage; // Return original if validation fails
            }
        }
        
        /// <summary>
        /// Validates an incoming message has the required fields
        /// </summary>
        /// <param name="jsonMessage">The JSON message to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateIncomingMessage(string jsonMessage)
        {
            try
            {
                // Parse the message
                JObject messageObj = JObject.Parse(jsonMessage);
                
                // Check for type field
                if (!messageObj.ContainsKey("type"))
                {
                    Debug.LogWarning("Received message missing 'type' field");
                    return false;
                }
                
                // Further validations could be added here
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}