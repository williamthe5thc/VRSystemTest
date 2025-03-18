using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FeedbackSystem : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private GameObject visualFeedbackPanel;
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private Image stateIcon;
    [SerializeField] private Image audioLevelIndicator;
    [SerializeField] private float feedbackDisplayTime = 2.0f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("State Icons")]
    [SerializeField] private Sprite idleIcon;
    [SerializeField] private Sprite listeningIcon;
    [SerializeField] private Sprite processingIcon;
    [SerializeField] private Sprite respondingIcon;
    [SerializeField] private Sprite waitingIcon;
    [SerializeField] private Sprite errorIcon;
    
    [Header("Quick Feedback UI")]
    [SerializeField] private GameObject quickFeedbackPanel;
    [SerializeField] private Button goodResponseButton;
    [SerializeField] private Button badResponseButton;
    [SerializeField] private Button neutralResponseButton;
    [SerializeField] private float quickFeedbackDisplayTime = 10.0f;
    
    [Header("Dependencies")]
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private MicrophoneCapture microphoneCapture;
    
    private Coroutine fadeOutCoroutine;
    private Coroutine quickFeedbackCoroutine;
    private CanvasGroup visualFeedbackCanvasGroup;
    private CanvasGroup quickFeedbackCanvasGroup;
    
    private void Start()
    {
        // Get canvas groups
        if (visualFeedbackPanel != null)
        {
            visualFeedbackCanvasGroup = visualFeedbackPanel.GetComponent<CanvasGroup>();
            if (visualFeedbackCanvasGroup == null)
            {
                visualFeedbackCanvasGroup = visualFeedbackPanel.AddComponent<CanvasGroup>();
            }
        }
        
        if (quickFeedbackPanel != null)
        {
            quickFeedbackCanvasGroup = quickFeedbackPanel.GetComponent<CanvasGroup>();
            if (quickFeedbackCanvasGroup == null)
            {
                quickFeedbackCanvasGroup = quickFeedbackPanel.AddComponent<CanvasGroup>();
            }
            
            // Hide quick feedback initially
            quickFeedbackPanel.SetActive(false);
        }
        
        // Register button listeners
        RegisterButtonListeners();
        
        // Register for state changes
        if (sessionManager != null)
        {
            sessionManager.OnStateChanged += HandleStateChange;
        }
        
        // Register for audio level changes
        if (microphoneCapture != null)
        {
            microphoneCapture.OnAudioLevelChanged += UpdateAudioLevelIndicator;
        }
    }
    
    private void RegisterButtonListeners()
    {
        if (goodResponseButton != null)
        {
            goodResponseButton.onClick.AddListener(() => RecordFeedback("good"));
        }
        
        if (badResponseButton != null)
        {
            badResponseButton.onClick.AddListener(() => RecordFeedback("bad"));
        }
        
        if (neutralResponseButton != null)
        {
            neutralResponseButton.onClick.AddListener(() => RecordFeedback("neutral"));
        }
    }
    
    private void HandleStateChange(string previousState, string currentState)
    {
        // Update state text and icon
        UpdateStateVisuals(currentState);
        
        // Show visual feedback
        ShowVisualFeedback();
        
        // If state changed to RESPONDING (when the interviewer is speaking), 
        // prepare to show quick feedback panel after the response
        if (currentState == "RESPONDING")
        {
            // Wait for the response to finish before showing feedback options
            // This will be triggered after the audio playback is complete
            if (sessionManager != null)
            {
                sessionManager.OnResponseComplete += ShowQuickFeedback;
            }
        }
    }
    
    private void UpdateStateVisuals(string state)
    {
        if (stateText != null)
        {
            stateText.text = state;
        }
        
        if (stateIcon != null)
        {
            // Set icon based on state
            switch (state)
            {
                case "IDLE":
                    stateIcon.sprite = idleIcon;
                    break;
                case "LISTENING":
                    stateIcon.sprite = listeningIcon;
                    break;
                case "PROCESSING":
                    stateIcon.sprite = processingIcon;
                    break;
                case "RESPONDING":
                    stateIcon.sprite = respondingIcon;
                    break;
                case "WAITING":
                    stateIcon.sprite = waitingIcon;
                    break;
                case "ERROR":
                    stateIcon.sprite = errorIcon;
                    break;
            }
        }
    }
    
    private void ShowVisualFeedback()
    {
        if (visualFeedbackPanel != null)
        {
            // Cancel any existing fade out
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
            }
            
            // Show panel
            visualFeedbackPanel.SetActive(true);
            
            // Reset opacity
            if (visualFeedbackCanvasGroup != null)
            {
                visualFeedbackCanvasGroup.alpha = 1f;
            }
            
            // Start fade out coroutine
            fadeOutCoroutine = StartCoroutine(FadeOutVisualFeedback());
        }
    }
    
    private IEnumerator FadeOutVisualFeedback()
    {
        // Wait for display time
        yield return new WaitForSeconds(feedbackDisplayTime);
        
        // Fade out
        if (visualFeedbackCanvasGroup != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                visualFeedbackCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Ensure it's fully transparent
            visualFeedbackCanvasGroup.alpha = 0f;
        }
        
        // Hide panel
        visualFeedbackPanel.SetActive(false);
    }
    
    private void UpdateAudioLevelIndicator(float level)
    {
        if (audioLevelIndicator != null)
        {
            // Scale the fill amount based on audio level (0-1)
            audioLevelIndicator.fillAmount = Mathf.Clamp01(level);
            
            // Optionally change color based on level
            audioLevelIndicator.color = Color.Lerp(Color.green, Color.red, level);
        }
    }
    
    private void ShowQuickFeedback()
    {
        // Unsubscribe to prevent multiple calls
        if (sessionManager != null)
        {
            sessionManager.OnResponseComplete -= ShowQuickFeedback;
        }
        
        if (quickFeedbackPanel != null)
        {
            // Cancel any existing coroutine
            if (quickFeedbackCoroutine != null)
            {
                StopCoroutine(quickFeedbackCoroutine);
            }
            
            // Show panel
            quickFeedbackPanel.SetActive(true);
            
            // Reset opacity
            if (quickFeedbackCanvasGroup != null)
            {
                quickFeedbackCanvasGroup.alpha = 1f;
            }
            
            // Start auto-hide coroutine
            quickFeedbackCoroutine = StartCoroutine(AutoHideQuickFeedback());
        }
    }
    
    private IEnumerator AutoHideQuickFeedback()
    {
        // Wait for display time
        yield return new WaitForSeconds(quickFeedbackDisplayTime);
        
        // Fade out
        if (quickFeedbackCanvasGroup != null)
        {
            float elapsedTime = 0f;
            float fadeDuration = 0.5f;
            while (elapsedTime < fadeDuration)
            {
                quickFeedbackCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Ensure it's fully transparent
            quickFeedbackCanvasGroup.alpha = 0f;
        }
        
        // Hide panel
        quickFeedbackPanel.SetActive(false);
    }
    
    private void RecordFeedback(string feedbackType)
    {
        // Send feedback to analytics or server
        Debug.Log($"User feedback: {feedbackType}");
        
        // Send feedback to session manager if needed
        if (sessionManager != null)
        {
            sessionManager.RecordUserFeedback(feedbackType);
        }
        
        // Hide feedback panel
        if (quickFeedbackPanel != null)
        {
            quickFeedbackPanel.SetActive(false);
            
            // Stop auto-hide coroutine
            if (quickFeedbackCoroutine != null)
            {
                StopCoroutine(quickFeedbackCoroutine);
            }
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (sessionManager != null)
        {
            sessionManager.OnStateChanged -= HandleStateChange;
            sessionManager.OnResponseComplete -= ShowQuickFeedback;
        }
        
        if (microphoneCapture != null)
        {
            microphoneCapture.OnAudioLevelChanged -= UpdateAudioLevelIndicator;
        }
    }
}
