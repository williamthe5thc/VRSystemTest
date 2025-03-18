using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Extends the existing UIManager with additional functionality for transcript display and thinking updates
/// </summary>
[RequireComponent(typeof(UIManager))]
public class UIExtender : MonoBehaviour
{
    [Header("Transcript Panel")]
    [SerializeField] private GameObject transcriptPanel;
    [SerializeField] private TextMeshProUGUI userTranscriptText;
    [SerializeField] private TextMeshProUGUI assistantTranscriptText;
    [SerializeField] private Button dismissButton;

    [Header("Thinking Indicator")]
    [SerializeField] private GameObject thinkingIndicator;
    [SerializeField] private TextMeshProUGUI thinkingText;
    [SerializeField] private Image thinkingSpinner;

    [Header("Notification")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 3f;

    private UIManager uiManager;
    private Coroutine spinnerRotation;
    private Coroutine autoHideNotification;

    private void Awake()
    {
        uiManager = GetComponent<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIExtender requires a UIManager component");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        // Set up transcript panel
        if (transcriptPanel != null)
        {
            // Start with transcript panel visible
            transcriptPanel.SetActive(true);
            
            // Setup dismiss button
            if (dismissButton != null)
            {
                dismissButton.onClick.AddListener(HideTranscript);
            }
            
            // Initialize text
            if (userTranscriptText != null)
            {
                userTranscriptText.text = "Waiting for you to speak...";
            }
            
            if (assistantTranscriptText != null)
            {
                assistantTranscriptText.text = "Interviewer is ready...";
            }
        }
        
        // Initialize thinking indicator (hidden by default)
        if (thinkingIndicator != null)
        {
            thinkingIndicator.SetActive(false);
        }
        
        // Initialize notification panel (hidden by default)
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Clean up event handlers
        if (dismissButton != null)
        {
            dismissButton.onClick.RemoveAllListeners();
        }
        
        // Stop coroutines
        if (spinnerRotation != null)
        {
            StopCoroutine(spinnerRotation);
        }
        
        if (autoHideNotification != null)
        {
            StopCoroutine(autoHideNotification);
        }
    }

    #region Public UI Methods - Call these from other components

    /// <summary>
    /// Updates the user's transcript text
    /// </summary>
    public void ShowUserTranscript(string text)
    {
        if (transcriptPanel != null && !transcriptPanel.activeSelf)
        {
            transcriptPanel.SetActive(true);
        }
        
        if (userTranscriptText != null)
        {
            userTranscriptText.text = $"You: {text}";
        }
        
        // Also use existing UIManager method if available
        if (uiManager != null)
        {
            // Forward to any compatible existing methods
            TryInvokeMethod(uiManager, "ShowUserInput", text);
        }
    }

    /// <summary>
    /// Updates the assistant's transcript text
    /// </summary>
    public void ShowAssistantTranscript(string text)
    {
        if (transcriptPanel != null && !transcriptPanel.activeSelf)
        {
            transcriptPanel.SetActive(true);
        }
        
        if (assistantTranscriptText != null)
        {
            assistantTranscriptText.text = $"Interviewer: {text}";
        }
        
        // Also use existing UIManager method if available
        if (uiManager != null)
        {
            // Forward to any compatible existing methods
            TryInvokeMethod(uiManager, "ShowResponse", text);
        }
    }

    /// <summary>
    /// Shows a thinking message with spinning indicator
    /// </summary>
    public void ShowThinking(string message = "Thinking...")
    {
        // Show thinking indicator
        if (thinkingIndicator != null)
        {
            thinkingIndicator.SetActive(true);
            
            // Update text
            if (thinkingText != null)
            {
                thinkingText.text = message;
            }
            
            // Start spinner animation if not already running
            if (thinkingSpinner != null && spinnerRotation == null)
            {
                spinnerRotation = StartCoroutine(AnimateSpinner());
            }
        }
        
        // Update transcript to show thinking
        if (assistantTranscriptText != null)
        {
            assistantTranscriptText.text = $"Interviewer is thinking: {message}";
        }
        
        // Also use existing UIManager methods if available
        if (uiManager != null)
        {
            // Forward to any compatible existing methods
            TryInvokeMethod(uiManager, "UpdateStatus", $"Processing: {message}");
        }
    }

    /// <summary>
    /// Hides the thinking indicator
    /// </summary>
    public void HideThinking()
    {
        if (thinkingIndicator != null)
        {
            thinkingIndicator.SetActive(false);
        }
        
        // Stop spinner animation
        if (spinnerRotation != null)
        {
            StopCoroutine(spinnerRotation);
            spinnerRotation = null;
        }
    }

    /// <summary>
    /// Shows a notification that auto-hides after a few seconds
    /// </summary>
    public void ShowNotification(string message)
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
            
            if (notificationText != null)
            {
                notificationText.text = message;
            }
            
            // Cancel previous auto-hide if running
            if (autoHideNotification != null)
            {
                StopCoroutine(autoHideNotification);
            }
            
            // Start new auto-hide
            autoHideNotification = StartCoroutine(HideNotificationAfterDelay());
        }
        
        // Also use existing UIManager methods if available
        if (uiManager != null)
        {
            // Forward to any compatible existing methods
            TryInvokeMethod(uiManager, "ShowMessage", message);
        }
    }

    #endregion

    #region Private Helper Methods

    private void HideTranscript()
    {
        if (transcriptPanel != null)
        {
            transcriptPanel.SetActive(false);
        }
    }

    private IEnumerator AnimateSpinner()
    {
        while (thinkingSpinner != null && thinkingIndicator != null && thinkingIndicator.activeSelf)
        {
            thinkingSpinner.transform.Rotate(0, 0, -5f);
            yield return null;
        }
        
        spinnerRotation = null;
    }

    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
        
        autoHideNotification = null;
    }

    /// <summary>
    /// Try to invoke a method on an object using reflection
    /// </summary>
    private void TryInvokeMethod(object target, string methodName, params object[] parameters)
    {
        try
        {
            var method = target.GetType().GetMethod(methodName);
            if (method != null)
            {
                method.Invoke(target, parameters);
            }
        }
        catch (Exception) 
        {
            // Silently ignore reflection errors
        }
    }

    #endregion
}
