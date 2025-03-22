using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VRInterview.Network
{
    /// <summary>
    /// Handles the enhanced state system with more granular processing states.
    /// </summary>
    public class EnhancedStateHandler : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI stateText;
        [SerializeField] private TextMeshProUGUI processingStageText;
        [SerializeField] private Image progressBar;
        [SerializeField] private float progressBarSmoothing = 5f;
        
        // Current state info
        private string _currentState = "IDLE";
        private string _currentStage = "";
        private float _targetProgress = 0f;
        private float _currentProgress = 0f;
        
        // UI text mappings
        private Dictionary<string, string> _stateDisplayText = new Dictionary<string, string>
        {
            { "IDLE", "Ready to start" },
            { "LISTENING", "Listening..." },
            { "PROCESSING", "Processing..." },
            { "PROCESSING_STT", "Transcribing your speech..." },
            { "PROCESSING_LLM", "Generating response..." },
            { "PROCESSING_TTS", "Creating audio..." },
            { "RESPONDING", "Interviewer is speaking" },
            { "WAITING", "Waiting for you to speak" },
            { "ERROR", "Error occurred" }
        };
        
        private void Start()
        {
            // Register for messages
            var messageHandler = FindObjectOfType<MessageHandler>();
            if (messageHandler != null)
            {
                messageHandler.RegisterMessageHandler("state_update", HandleStateUpdate);
                messageHandler.RegisterMessageHandler("heartbeat", HandleHeartbeat);
            }
            
            // Initialize UI
            UpdateUI();
        }
        
        private void Update()
        {
            // Smooth progress bar
            if (Math.Abs(_currentProgress - _targetProgress) > 0.01f)
            {
                _currentProgress = Mathf.Lerp(_currentProgress, _targetProgress, Time.deltaTime * progressBarSmoothing);
                UpdateProgressBar();
            }
        }
        
        private void HandleStateUpdate(string jsonMessage)
        {
            try
            {
                // Parse state update message
                StateUpdateMessage stateMsg = JsonUtility.FromJson<StateUpdateMessage>(jsonMessage);
                if (stateMsg == null) return;
                
                // Update current state
                _currentState = stateMsg.current;
                
                // Extract processing stage if available
                if (stateMsg.metadata != null)
                {
                    // Try to get stage info from metadata
                    string stage = GetValueFromMetadata(stateMsg.metadata, "stage");
                    if (!string.IsNullOrEmpty(stage))
                    {
                        _currentStage = stage;
                    }
                    
                    // Try to get progress info
                    string progressStr = GetValueFromMetadata(stateMsg.metadata, "progress");
                    if (!string.IsNullOrEmpty(progressStr) && float.TryParse(progressStr, out float progress))
                    {
                        _targetProgress = progress / 100f; // Assuming progress is 0-100
                    }
                }
                
                // Determine if this is a more detailed processing state
                if (_currentState == "PROCESSING" && !string.IsNullOrEmpty(_currentStage))
                {
                    // Handle granular processing states
                    if (_currentStage == "stt")
                    {
                        _currentState = "PROCESSING_STT";
                    }
                    else if (_currentStage == "llm")
                    {
                        _currentState = "PROCESSING_LLM";
                    }
                    else if (_currentStage == "tts")
                    {
                        _currentState = "PROCESSING_TTS";
                    }
                }
                
                // Update UI
                UpdateUI();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling state update: {ex.Message}");
            }
        }
        
        private void HandleHeartbeat(string jsonMessage)
        {
            try
            {
                // Parse heartbeat message
                HeartbeatMessage heartbeatMsg = JsonUtility.FromJson<HeartbeatMessage>(jsonMessage);
                if (heartbeatMsg == null) return;
                
                // Update progress if available
                if (heartbeatMsg.progress > 0)
                {
                    _targetProgress = heartbeatMsg.progress / 100f; // Assuming progress is 0-100
                }
                
                // Update processing stage text if available
                if (!string.IsNullOrEmpty(heartbeatMsg.message))
                {
                    processingStageText.text = heartbeatMsg.message;
                }
                
                // Update progress bar
                UpdateProgressBar();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling heartbeat: {ex.Message}");
            }
        }
        
        private void UpdateUI()
        {
            // Update state text
            if (stateText != null)
            {
                if (_stateDisplayText.TryGetValue(_currentState, out string displayText))
                {
                    stateText.text = displayText;
                }
                else
                {
                    stateText.text = _currentState; // Fallback to raw state
                }
            }
            
            // Update processing stage text
            if (processingStageText != null)
            {
                if (_currentState.StartsWith("PROCESSING"))
                {
                    processingStageText.gameObject.SetActive(true);
                    
                    // Set default text based on processing state if not already set
                    if (string.IsNullOrEmpty(processingStageText.text))
                    {
                        if (_currentState == "PROCESSING_STT")
                            processingStageText.text = "Transcribing audio...";
                        else if (_currentState == "PROCESSING_LLM")
                            processingStageText.text = "The interviewer is thinking...";
                        else if (_currentState == "PROCESSING_TTS")
                            processingStageText.text = "Creating interview response...";
                        else
                            processingStageText.text = "Processing...";
                    }
                }
                else
                {
                    processingStageText.gameObject.SetActive(false);
                    processingStageText.text = ""; // Clear text
                }
            }
            
            // Update progress bar
            UpdateProgressBar();
        }
        
        private void UpdateProgressBar()
        {
            if (progressBar != null)
            {
                // Only show progress bar for processing states
                if (_currentState.StartsWith("PROCESSING") || _currentState == "RESPONDING")
                {
                    progressBar.gameObject.SetActive(true);
                    progressBar.fillAmount = _currentProgress;
                }
                else
                {
                    progressBar.gameObject.SetActive(false);
                }
            }
        }
        
        private string GetValueFromMetadata(SerializableDictionary metadata, string key)
        {
            if (metadata != null && metadata.pairs != null)
            {
                foreach (var pair in metadata.pairs)
                {
                    if (pair.key == key)
                        return pair.value;
                }
            }
            return string.Empty;
        }
        
        // Message Types
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
        private class HeartbeatMessage
        {
            public string type;
            public string session_id;
            public double timestamp;
            public int progress;
            public string message;
        }
        
        [Serializable]
        private class SerializableDictionary
        {
            public KeyValuePair[] pairs;
        }
        
        [Serializable]
        private class KeyValuePair
        {
            public string key;
            public string value;
        }
    }
}