using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using TMPro;

public class VRInteractionUI : MonoBehaviour
{
    [Header("VR UI Components")]
    [SerializeField] private GameObject vrMenuPanel;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button feedbackButton;
    
    [Header("Menu Activation")]
    [SerializeField] private XRController menuController; // Usually left controller
    [SerializeField] private InputHelpers.Button menuButton = InputHelpers.Button.PrimaryButton; // "X" button on left controller
    [SerializeField] private float menuActivationThreshold = 0.1f;
    [SerializeField] private Transform menuAttachPoint; // Point where menu follows the controller
    [SerializeField] private float menuDistance = 0.5f;
    [SerializeField] private float menuRotationOffset = 15f;
    
    [Header("Feedback Panel")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private Button submitFeedbackButton;
    [SerializeField] private Button cancelFeedbackButton;
    [SerializeField] private TMP_InputField feedbackText;
    
    [Header("Dependencies")]
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private UIManager uiManager;
    
    private bool isMenuVisible = false;
    private bool isFeedbackVisible = false;
    
    private void Start()
    {
        // Initialize UI
        if (vrMenuPanel != null)
        {
            vrMenuPanel.SetActive(false);
        }
        
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
        
        // Register button listeners
        RegisterButtonListeners();
    }
    
    private void Update()
    {
        // Check for menu button press
        CheckMenuButtonPress();
        
        // Position menu if visible
        if (isMenuVisible && vrMenuPanel != null && menuAttachPoint != null)
        {
            PositionMenu();
        }
    }
    
    private void RegisterButtonListeners()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseInterview);
        }
        
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeInterview);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartInterview);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitToMainMenu);
        }
        
        if (feedbackButton != null)
        {
            feedbackButton.onClick.AddListener(ShowFeedbackPanel);
        }
        
        if (submitFeedbackButton != null)
        {
            submitFeedbackButton.onClick.AddListener(SubmitFeedback);
        }
        
        if (cancelFeedbackButton != null)
        {
            cancelFeedbackButton.onClick.AddListener(HideFeedbackPanel);
        }
    }
    
    private void CheckMenuButtonPress()
    {
        if (menuController == null) return;
        
        // Check if menu button is pressed
        if (menuController.inputDevice.TryGetFeatureValue(
            new UnityEngine.XR.InputFeatureUsage<float>("PrimaryButton"), 
            out float value) && value >= menuActivationThreshold)
        {
            ToggleMenu();
        }
    }
    
    private void ToggleMenu()
    {
        isMenuVisible = !isMenuVisible;
        
        if (vrMenuPanel != null)
        {
            vrMenuPanel.SetActive(isMenuVisible);
        }
        
        // Hide feedback panel when menu is toggled off
        if (!isMenuVisible && feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
            isFeedbackVisible = false;
        }
    }
    
    private void PositionMenu()
    {
        // Position menu in front of the controller
        vrMenuPanel.transform.position = menuAttachPoint.position + 
                                        menuAttachPoint.forward * menuDistance;
        
        // Rotate menu to face the user
        vrMenuPanel.transform.rotation = menuAttachPoint.rotation * 
                                         Quaternion.Euler(0, menuRotationOffset, 0);
    }
    
    private void PauseInterview()
    {
        if (sessionManager != null)
        {
            // Pause the interview session
            sessionManager.PauseSession();
            
            // Log action
            if (uiManager != null)
            {
                uiManager.LogDebug("Interview paused by user");
            }
        }
    }
    
    private void ResumeInterview()
    {
        if (sessionManager != null)
        {
            // Resume the interview session
            sessionManager.ResumeSession();
            
            // Log action
            if (uiManager != null)
            {
                uiManager.LogDebug("Interview resumed by user");
            }
            
            // Hide menu after resuming
            isMenuVisible = false;
            if (vrMenuPanel != null)
            {
                vrMenuPanel.SetActive(false);
            }
        }
    }
    
    private void RestartInterview()
    {
        if (sessionManager != null)
        {
            // Reset the interview session
            _ = sessionManager.ResetSession();
            
            // Log action
            if (uiManager != null)
            {
                uiManager.LogDebug("Interview restarted by user");
            }
            
            // Hide menu after restarting
            isMenuVisible = false;
            if (vrMenuPanel != null)
            {
                vrMenuPanel.SetActive(false);
            }
        }
    }
    
    private void ExitToMainMenu()
    {
        if (sessionManager != null)
        {
            // End the session
            _ = sessionManager.EndSession();
        }
        
        // Load main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    private void ShowFeedbackPanel()
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
            isFeedbackVisible = true;
            
            // Clear previous feedback
            if (feedbackText != null)
            {
                feedbackText.text = "";
            }
        }
    }
    
    private void HideFeedbackPanel()
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
            isFeedbackVisible = false;
        }
    }
    
    private void SubmitFeedback()
    {
        if (feedbackText != null && !string.IsNullOrEmpty(feedbackText.text))
        {
            // TODO: Implement feedback submission to backend or analytics
            
            // Log feedback
            if (uiManager != null)
            {
                uiManager.LogDebug("User submitted feedback: " + feedbackText.text);
            }
            
            // Show confirmation
            if (uiManager != null)
            {
                uiManager.ShowError("Thank you for your feedback!");
            }
            
            // Hide feedback panel
            HideFeedbackPanel();
        }
        else
        {
            // Show error
            if (uiManager != null)
            {
                uiManager.ShowError("Please enter feedback before submitting");
            }
        }
    }
    
    // Methods for XR interaction events
    public void OnPointerEnter(GameObject hoverObject)
    {
        // Highlight the object or show tooltip
        // This would be connected to UI events in the Inspector
    }
    
    public void OnPointerExit(GameObject hoverObject)
    {
        // Remove highlight or hide tooltip
    }
}
