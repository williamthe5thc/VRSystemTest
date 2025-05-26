using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Connection UI")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private TextMeshProUGUI connectionStatus;
    [SerializeField] private Button reconnectButton;

    [Header("Transcript UI")]
    [SerializeField] private GameObject transcriptPanel;
    [SerializeField] private TextMeshProUGUI userTranscriptText;
    [SerializeField] private TextMeshProUGUI llmResponseText;
    [SerializeField] private Button dismissTranscriptButton;

    [Header("Processing UI")]
    [SerializeField] private GameObject processingPanel;
    [SerializeField] private TextMeshProUGUI processingText;
    [SerializeField] private Image processingSpinner;

    [Header("Notification UI")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 3f;
    
    [Header("Debug UI")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private TextMeshProUGUI debugLogText;
    [SerializeField] private TextMeshProUGUI fallbackText;
    [SerializeField] private Slider progressBar;

    // Properties to get elements
    public TextMeshProUGUI ConnectionStatus => connectionStatus;
    public Button ReconnectButton => reconnectButton;

    private Coroutine spinnerRotation;
    private Coroutine autoHideNotification;

    private void Start()
    {
        // Initialize panels
        SetConnectionPanelVisible(false);
        SetProcessingPanelVisible(false);
        SetNotificationPanelVisible(false);
        
        // Initialize transcript panel
        if (transcriptPanel != null)
        {
            transcriptPanel.SetActive(true);
            
            // Clear initial texts
            if (userTranscriptText != null)
                userTranscriptText.text = "Waiting for you to speak...";
            
            if (llmResponseText != null)
                llmResponseText.text = "Interviewer is ready to respond...";
            
            // Add dismiss button listener
            if (dismissTranscriptButton != null)
                dismissTranscriptButton.onClick.AddListener(HideTranscript);
        }
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        if (dismissTranscriptButton != null)
            dismissTranscriptButton.onClick.RemoveAllListeners();
    }

    #region Connection UI
    
    /// <summary>
    /// Shows or hides the connection panel
    /// </summary>
    public void SetConnectionPanelVisible(bool visible)
    {
        if (connectionPanel != null)
            connectionPanel.SetActive(visible);
    }

    /// <summary>
    /// Updates the connection status text
    /// </summary>
    public void UpdateConnectionStatus(string status)
    {
        if (connectionStatus != null)
            connectionStatus.text = status;
    }

    /// <summary>
    /// Sets the reconnect button interactable state
    /// </summary>
    public void SetReconnectButtonInteractable(bool interactable)
    {
        if (reconnectButton != null)
            reconnectButton.interactable = interactable;
    }
    
    #endregion
    
    #region Debug and Status UI
    
    /// <summary>
    /// Updates the status text display
    /// </summary>
    public void UpdateStatus(string status)
    {
        Debug.Log($"Status update: {status}");
        ShowProcessingMessage(status);
    }
    
    /// <summary>
    /// Updates the progress bar value
    /// </summary>
    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = Mathf.Clamp01(progress);
        }
    }
    
    /// <summary>
    /// Shows fallback text when audio fails
    /// </summary>
    public void ShowFallbackText(string text)
    {
        if (fallbackText != null)
        {
            fallbackText.gameObject.SetActive(!string.IsNullOrEmpty(text));
            fallbackText.text = text;
        }
        
        // Also show as a notification
        ShowNotification(text);
    }
    
    /// <summary>
    /// Logs a debug message to the debug panel
    /// </summary>
    public void LogDebug(string message)
    {
        Debug.Log(message);
        
        if (debugLogText != null)
        {
            // Add timestamp
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string entry = $"[{timestamp}] {message}\n";
            
            // Keep only last 10 lines
            string[] lines = debugLogText.text.Split('\n');
            if (lines.Length > 10)
            {
                string newText = "";
                for (int i = 1; i < lines.Length; i++)
                {
                    newText += lines[i] + "\n";
                }
                debugLogText.text = newText;
            }
            
            // Add new message
            debugLogText.text += entry;
        }
    }
    
    /// <summary>
    /// Shows or hides the debug panel
    /// </summary>
    public void SetDebugPanelVisible(bool visible)
    {
        if (debugPanel != null)
            debugPanel.SetActive(visible);
    }
    
    #endregion

    #region Transcript UI
    
    /// <summary>
    /// Shows the full transcript with both user and LLM text
    /// </summary>
    public void ShowTranscript(string userText, string llmText)
    {
        if (transcriptPanel != null)
            transcriptPanel.SetActive(true);
        
        if (userTranscriptText != null && !string.IsNullOrEmpty(userText))
            userTranscriptText.text = $"You: {userText}";
        
        if (llmResponseText != null && !string.IsNullOrEmpty(llmText))
            llmResponseText.text = $"Interviewer: {llmText}";
    }

    /// <summary>
    /// Updates just the user transcript portion
    /// </summary>
    public void ShowUserTranscript(string text)
    {
        if (transcriptPanel != null && !transcriptPanel.activeSelf)
            transcriptPanel.SetActive(true);
        
        if (userTranscriptText != null && !string.IsNullOrEmpty(text))
            userTranscriptText.text = $"You: {text}";
    }

    /// <summary>
    /// Updates just the LLM response portion
    /// </summary>
    public void ShowLLMResponse(string text)
    {
        if (transcriptPanel != null && !transcriptPanel.activeSelf)
            transcriptPanel.SetActive(true);
        
        if (llmResponseText != null)
        {
            if (string.IsNullOrEmpty(text))
                llmResponseText.text = "Interviewer is thinking...";
            else
                llmResponseText.text = $"Interviewer: {text}";
        }
    }
    
    /// <summary>
    /// Shows the "Interviewer is thinking..." message in the LLM response area
    /// </summary>
    public void ShowLLMThinking()
    {
        if (transcriptPanel != null && !transcriptPanel.activeSelf)
            transcriptPanel.SetActive(true);
        
        if (llmResponseText != null)
            llmResponseText.text = "Interviewer is thinking...";
    }

    /// <summary>
    /// Clears the transcript and sets to thinking state
    /// </summary>
    public void ClearTranscript()
    {
        if (llmResponseText != null)
            llmResponseText.text = "Interviewer is thinking...";
            
        // Don't clear the user text to maintain context
    }

    /// <summary>
    /// Hides the transcript panel
    /// </summary>
    public void HideTranscript()
    {
        if (transcriptPanel != null)
            transcriptPanel.SetActive(false);
    }
    
    #endregion

    #region Processing UI
    
    /// <summary>
    /// Shows or hides the processing panel
    /// </summary>
    public void SetProcessingPanelVisible(bool visible)
    {
        if (processingPanel != null)
        {
            processingPanel.SetActive(visible);
            
            // Start or stop spinner animation
            if (visible && spinnerRotation == null && processingSpinner != null)
            {
                spinnerRotation = StartCoroutine(AnimateSpinner());
            }
            else if (!visible && spinnerRotation != null)
            {
                StopCoroutine(spinnerRotation);
                spinnerRotation = null;
            }
        }
    }

    /// <summary>
    /// Updates the processing text
    /// </summary>
    public void ShowProcessingMessage(string message)
    {
        if (processingPanel != null && !processingPanel.activeSelf)
            SetProcessingPanelVisible(true);
        
        if (processingText != null)
            processingText.text = message;
    }
    
    /// <summary>
    /// Shows thinking message in both processing panel and transcript
    /// </summary>
    public void ShowThinkingMessage(string message)
    {
        ShowProcessingMessage(message);
        
        // Also update transcript
        if (llmResponseText != null)
            llmResponseText.text = $"Interviewer is thinking: {message}";
    }

    /// <summary>
    /// Animate spinner rotation
    /// </summary>
    private IEnumerator AnimateSpinner()
    {
        if (processingSpinner == null)
            yield break;
            
        while (true)
        {
            processingSpinner.transform.Rotate(0, 0, -5);
            yield return null;
        }
    }
    
    #endregion

    #region Notification UI
    
    /// <summary>
    /// Shows a notification message that auto-hides after duration
    /// </summary>
    public void ShowNotification(string message)
    {
        if (notificationPanel != null)
            notificationPanel.SetActive(true);
        
        if (notificationText != null)
            notificationText.text = message;
            
        // Cancel existing auto-hide coroutine if running
        if (autoHideNotification != null)
        {
            StopCoroutine(autoHideNotification);
            autoHideNotification = null;
        }
        
        // Start new auto-hide coroutine
        autoHideNotification = StartCoroutine(AutoHideNotification());
    }

    /// <summary>
    /// Shows an error message
    /// </summary>
    public void ShowError(string errorMessage)
    {
        ShowNotification($"Error: {errorMessage}");
    }
    
    /// <summary>
    /// Shows a message to the user
    /// </summary>
    public void ShowMessage(string message)
    {
        ShowNotification(message);
    }
    
    /// <summary>
    /// Updates the connection status display
    /// </summary>
    public void UpdateConnectionStatus(bool connected)
    {
        if (connectionStatus != null)
        {
            connectionStatus.text = connected ? "Connected" : "Disconnected";
        }
    }
    
    /// <summary>
    /// Updates the state display
    /// </summary>
    public void UpdateStateDisplay(string state)
    {
        // This could update a UI element showing the current state
        // For now, just log it
        Debug.Log($"State updated to: {state}");
    }
    
    /// <summary>
    /// Shows or hides the notification panel
    /// </summary>
    public void SetNotificationPanelVisible(bool visible)
    {
        if (notificationPanel != null)
            notificationPanel.SetActive(visible);
    }

    /// <summary>
    /// Auto-hide notification after duration
    /// </summary>
    private IEnumerator AutoHideNotification()
    {
        yield return new WaitForSeconds(notificationDuration);
        SetNotificationPanelVisible(false);
        autoHideNotification = null;
    }
    
    #endregion
}