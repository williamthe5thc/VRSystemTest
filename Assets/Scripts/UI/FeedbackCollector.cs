using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Collects user feedback after interview sessions and saves it locally and/or sends it to the server.
/// Provides different feedback mechanisms (ratings, text input, multiple choice).
/// </summary>
public class FeedbackCollector : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private Button[] ratingButtons;
    [SerializeField] private TMP_InputField commentInput;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private GameObject multiChoiceContainer;
    [SerializeField] private GameObject ratingContainer;
    [SerializeField] private GameObject textInputContainer;
    [SerializeField] private GameObject thankYouPanel;
    
    [Header("Feedback Configuration")]
    [SerializeField] private FeedbackQuestion[] feedbackQuestions;
    [SerializeField] private float delayBeforeShowingFeedback = 1.0f;
    [SerializeField] private bool sendToServer = true;
    [SerializeField] private bool saveLocally = true;
    [SerializeField] private string localSavePath = "Feedback";
    
    [Header("Dependencies")]
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private WebSocketClient webSocketClient;
    
    // Tracking variables
    private int currentQuestionIndex = 0;
    private int currentRating = 0;
    private string currentTextInput = "";
    private List<string> currentMultiChoiceSelections = new List<string>();
    private Dictionary<string, object> feedbackResults = new Dictionary<string, object>();
    private bool isCollectingFeedback = false;
    
    // Events
    public event Action<Dictionary<string, object>> OnFeedbackCompleted;
    public event Action OnFeedbackSkipped;
    
    [Serializable]
    public class FeedbackQuestion
    {
        public string questionId;
        public string questionText;
        public FeedbackType type;
        public string[] options; // For multiple choice questions
        public bool required = false;
        
        public enum FeedbackType
        {
            Rating,
            Text,
            MultipleChoice
        }
    }
    
    private void Awake()
    {
        // Initialize UI if available
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
        
        if (thankYouPanel != null)
        {
            thankYouPanel.SetActive(false);
        }
        
        // Set up button listeners
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitClicked);
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipClicked);
        }
        
        // Set up rating buttons
        if (ratingButtons != null)
        {
            for (int i = 0; i < ratingButtons.Length; i++)
            {
                int rating = i + 1; // Ratings from 1 to 5 (or however many buttons)
                if (ratingButtons[i] != null)
                {
                    ratingButtons[i].onClick.AddListener(() => SelectRating(rating));
                }
            }
        }
        
        // Set up text input field
        if (commentInput != null)
        {
            commentInput.onValueChanged.AddListener(OnTextInputChanged);
        }
        
        // Find dependencies if not assigned
        if (sessionManager == null)
        {
            sessionManager = FindObjectOfType<SessionManager>();
        }
        
        if (webSocketClient == null)
        {
            webSocketClient = FindObjectOfType<WebSocketClient>();
        }
        
        // Create local save directory if needed
        if (saveLocally && !Directory.Exists(Path.Combine(Application.persistentDataPath, localSavePath)))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, localSavePath));
        }
    }
    
    private void Start()
    {
        // Register for session end events if session manager exists
        if (sessionManager != null)
        {
            sessionManager.OnSessionEnded += OnSessionEnded;
        }
    }
    
    private void OnDestroy()
    {
        // Unregister from events
        if (sessionManager != null)
        {
            sessionManager.OnSessionEnded -= OnSessionEnded;
        }
        
        // Remove button listeners
        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(OnSubmitClicked);
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(OnSkipClicked);
        }
        
        // Remove rating button listeners
        if (ratingButtons != null)
        {
            for (int i = 0; i < ratingButtons.Length; i++)
            {
                if (ratingButtons[i] != null)
                {
                    ratingButtons[i].onClick.RemoveAllListeners();
                }
            }
        }
        
        // Remove text input listener
        if (commentInput != null)
        {
            commentInput.onValueChanged.RemoveListener(OnTextInputChanged);
        }
    }
    
    /// <summary>
    /// Triggered when a session ends, show feedback after a delay
    /// </summary>
    private void OnSessionEnded()
    {
        StartCoroutine(ShowFeedbackAfterDelay());
    }
    
    private IEnumerator ShowFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeShowingFeedback);
        StartFeedbackCollection();
    }
    
    /// <summary>
    /// Manually start the feedback collection process
    /// </summary>
    public void StartFeedbackCollection()
    {
        if (isCollectingFeedback) return;
        
        isCollectingFeedback = true;
        currentQuestionIndex = 0;
        feedbackResults.Clear();
        
        // Reset input tracking variables
        currentRating = 0;
        currentTextInput = "";
        currentMultiChoiceSelections.Clear();
        
        // Show feedback panel
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
        }
        
        // Hide thank you panel
        if (thankYouPanel != null)
        {
            thankYouPanel.SetActive(false);
        }
        
        // Show first question
        ShowCurrentQuestion();
    }
    
    private void ShowCurrentQuestion()
    {
        if (currentQuestionIndex >= feedbackQuestions.Length)
        {
            CompleteFeedbackCollection();
            return;
        }
        
        FeedbackQuestion question = feedbackQuestions[currentQuestionIndex];
        
        // Update question text
        if (questionText != null)
        {
            questionText.text = question.questionText;
        }
        
        // Update header if desired
        if (headerText != null)
        {
            headerText.text = $"Question {currentQuestionIndex + 1} of {feedbackQuestions.Length}";
        }
        
        // Reset inputs
        currentRating = 0;
        currentTextInput = "";
        currentMultiChoiceSelections.Clear();
        
        // Update submit button state
        UpdateSubmitButtonState();
        
        // Show the appropriate input type
        ShowQuestionInputType(question.type);
        
        // Prepare input for current question type
        switch (question.type)
        {
            case FeedbackQuestion.FeedbackType.Rating:
                PrepareRatingQuestion();
                break;
            
            case FeedbackQuestion.FeedbackType.Text:
                PrepareTextQuestion();
                break;
            
            case FeedbackQuestion.FeedbackType.MultipleChoice:
                PrepareMultiChoiceQuestion(question.options);
                break;
        }
    }
    
    private void ShowQuestionInputType(FeedbackQuestion.FeedbackType type)
    {
        // Hide all containers first
        if (ratingContainer != null) ratingContainer.SetActive(false);
        if (textInputContainer != null) textInputContainer.SetActive(false);
        if (multiChoiceContainer != null) multiChoiceContainer.SetActive(false);
        
        // Show the appropriate container
        switch (type)
        {
            case FeedbackQuestion.FeedbackType.Rating:
                if (ratingContainer != null) ratingContainer.SetActive(true);
                break;
            
            case FeedbackQuestion.FeedbackType.Text:
                if (textInputContainer != null) textInputContainer.SetActive(true);
                break;
            
            case FeedbackQuestion.FeedbackType.MultipleChoice:
                if (multiChoiceContainer != null) multiChoiceContainer.SetActive(true);
                break;
        }
    }
    
    private void PrepareRatingQuestion()
    {
        // Reset all rating buttons to not selected
        if (ratingButtons != null)
        {
            foreach (Button button in ratingButtons)
            {
                if (button != null)
                {
                    ColorBlock colors = button.colors;
                    colors.normalColor = new Color(0.8f, 0.8f, 0.8f);
                    button.colors = colors;
                }
            }
        }
    }
    
    private void PrepareTextQuestion()
    {
        // Clear input field
        if (commentInput != null)
        {
            commentInput.text = "";
        }
    }
    
    private void PrepareMultiChoiceQuestion(string[] options)
    {
        // Clear existing options
        if (multiChoiceContainer != null)
        {
            foreach (Transform child in multiChoiceContainer.transform)
            {
                Destroy(child.gameObject);
            }
            
            // Create toggle for each option
            if (options != null)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    string option = options[i];
                    
                    // Create toggle game object
                    GameObject toggleObj = new GameObject("Toggle_" + i);
                    toggleObj.transform.SetParent(multiChoiceContainer.transform, false);
                    
                    // Add toggle component
                    Toggle toggle = toggleObj.AddComponent<Toggle>();
                    
                    // Add background image
                    GameObject background = new GameObject("Background");
                    background.transform.SetParent(toggleObj.transform, false);
                    Image bgImage = background.AddComponent<Image>();
                    bgImage.color = new Color(0.2f, 0.2f, 0.2f);
                    
                    // Add checkmark image
                    GameObject checkmark = new GameObject("Checkmark");
                    checkmark.transform.SetParent(background.transform, false);
                    Image checkImage = checkmark.AddComponent<Image>();
                    checkImage.color = new Color(0.0f, 0.6f, 1.0f);
                    
                    // Configure toggle
                    toggle.targetGraphic = bgImage;
                    toggle.graphic = checkImage;
                    toggle.isOn = false;
                    
                    // Add label
                    GameObject label = new GameObject("Label");
                    label.transform.SetParent(toggleObj.transform, false);
                    TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
                    labelText.text = option;
                    labelText.color = Color.white;
                    labelText.fontSize = 14;
                    
                    // Set up layouts
                    RectTransform toggleRect = toggleObj.GetComponent<RectTransform>();
                    toggleRect.anchorMin = new Vector2(0, 0);
                    toggleRect.anchorMax = new Vector2(1, 0);
                    toggleRect.pivot = new Vector2(0.5f, 0);
                    toggleRect.sizeDelta = new Vector2(0, 30);
                    toggleRect.anchoredPosition = new Vector2(0, i * 35);
                    
                    RectTransform bgRect = background.GetComponent<RectTransform>();
                    bgRect.anchorMin = new Vector2(0, 0.5f);
                    bgRect.anchorMax = new Vector2(0, 0.5f);
                    bgRect.pivot = new Vector2(0.5f, 0.5f);
                    bgRect.sizeDelta = new Vector2(20, 20);
                    bgRect.anchoredPosition = new Vector2(15, 0);
                    
                    RectTransform checkRect = checkmark.GetComponent<RectTransform>();
                    checkRect.anchorMin = new Vector2(0.1f, 0.1f);
                    checkRect.anchorMax = new Vector2(0.9f, 0.9f);
                    checkRect.sizeDelta = Vector2.zero;
                    
                    RectTransform labelRect = label.GetComponent<RectTransform>();
                    labelRect.anchorMin = new Vector2(0, 0);
                    labelRect.anchorMax = new Vector2(1, 1);
                    labelRect.pivot = new Vector2(0.5f, 0.5f);
                    labelRect.offsetMin = new Vector2(40, 0);
                    labelRect.offsetMax = new Vector2(-10, 0);
                    
                    // Add listener for selection
                    string optionValue = option; // Create a local copy for closure
                    toggle.onValueChanged.AddListener((isOn) => {
                        OnMultiChoiceChanged(optionValue, isOn);
                    });
                }
                
                // Update the container size
                RectTransform containerRect = multiChoiceContainer.GetComponent<RectTransform>();
                if (containerRect != null)
                {
                    containerRect.sizeDelta = new Vector2(containerRect.sizeDelta.x, options.Length * 35);
                }
            }
        }
    }
    
    /// <summary>
    /// Update the submit button based on whether the current question is required
    /// and has a valid input
    /// </summary>
    private void UpdateSubmitButtonState()
    {
        if (submitButton == null || currentQuestionIndex >= feedbackQuestions.Length) return;
        
        bool canSubmit = true;
        FeedbackQuestion question = feedbackQuestions[currentQuestionIndex];
        
        if (question.required)
        {
            switch (question.type)
            {
                case FeedbackQuestion.FeedbackType.Rating:
                    canSubmit = currentRating > 0;
                    break;
                
                case FeedbackQuestion.FeedbackType.Text:
                    canSubmit = !string.IsNullOrWhiteSpace(currentTextInput);
                    break;
                
                case FeedbackQuestion.FeedbackType.MultipleChoice:
                    canSubmit = currentMultiChoiceSelections.Count > 0;
                    break;
            }
        }
        
        submitButton.interactable = canSubmit;
    }
    
    /// <summary>
    /// Handle rating selection
    /// </summary>
    private void SelectRating(int rating)
    {
        currentRating = rating;
        
        // Update button colors
        if (ratingButtons != null)
        {
            for (int i = 0; i < ratingButtons.Length; i++)
            {
                if (ratingButtons[i] != null)
                {
                    ColorBlock colors = ratingButtons[i].colors;
                    
                    if (i + 1 <= rating)
                    {
                        // Selected rating
                        colors.normalColor = new Color(0.0f, 0.6f, 1.0f);
                    }
                    else
                    {
                        // Unselected rating
                        colors.normalColor = new Color(0.8f, 0.8f, 0.8f);
                    }
                    
                    ratingButtons[i].colors = colors;
                }
            }
        }
        
        UpdateSubmitButtonState();
    }
    
    /// <summary>
    /// Handle text input changes
    /// </summary>
    private void OnTextInputChanged(string text)
    {
        currentTextInput = text;
        UpdateSubmitButtonState();
    }
    
    /// <summary>
    /// Handle multiple choice selection changes
    /// </summary>
    private void OnMultiChoiceChanged(string option, bool isSelected)
    {
        if (isSelected)
        {
            if (!currentMultiChoiceSelections.Contains(option))
            {
                currentMultiChoiceSelections.Add(option);
            }
        }
        else
        {
            currentMultiChoiceSelections.Remove(option);
        }
        
        UpdateSubmitButtonState();
    }
    
    /// <summary>
    /// Handle submit button click
    /// </summary>
    private void OnSubmitClicked()
    {
        SaveCurrentAnswer();
        currentQuestionIndex++;
        ShowCurrentQuestion();
    }
    
    /// <summary>
    /// Handle skip button click
    /// </summary>
    private void OnSkipClicked()
    {
        isCollectingFeedback = false;
        
        // Hide feedback panel
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
        
        // Trigger skip event
        OnFeedbackSkipped?.Invoke();
    }
    
    /// <summary>
    /// Save the answer for the current question
    /// </summary>
    private void SaveCurrentAnswer()
    {
        if (currentQuestionIndex >= feedbackQuestions.Length) return;
        
        FeedbackQuestion question = feedbackQuestions[currentQuestionIndex];
        
        switch (question.type)
        {
            case FeedbackQuestion.FeedbackType.Rating:
                feedbackResults[question.questionId] = currentRating;
                break;
            
            case FeedbackQuestion.FeedbackType.Text:
                feedbackResults[question.questionId] = currentTextInput;
                break;
            
            case FeedbackQuestion.FeedbackType.MultipleChoice:
                feedbackResults[question.questionId] = new List<string>(currentMultiChoiceSelections);
                break;
        }
    }
    
    /// <summary>
    /// Complete the feedback collection process
    /// </summary>
    private void CompleteFeedbackCollection()
    {
        isCollectingFeedback = false;
        
        // Add metadata
        feedbackResults["timestamp"] = DateTime.UtcNow.ToString("o");
        feedbackResults["session_id"] = sessionManager != null ? sessionManager.GetSessionId() : "unknown";
        
        // Save feedback locally if enabled
        if (saveLocally)
        {
            SaveFeedbackLocally();
        }
        
        // Send to server if enabled
        if (sendToServer)
        {
            SendFeedbackToServer();
        }
        
        // Show thank you panel
        if (thankYouPanel != null)
        {
            feedbackPanel.SetActive(false);
            thankYouPanel.SetActive(true);
            
            // Hide thank you panel after a few seconds
            StartCoroutine(HideThankYouPanel());
        }
        else
        {
            // Just hide the feedback panel
            if (feedbackPanel != null)
            {
                feedbackPanel.SetActive(false);
            }
        }
        
        // Trigger completion event
        OnFeedbackCompleted?.Invoke(feedbackResults);
    }
    
    private IEnumerator HideThankYouPanel()
    {
        yield return new WaitForSeconds(3.0f);
        
        if (thankYouPanel != null)
        {
            thankYouPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Save feedback results to local storage
    /// </summary>
    private void SaveFeedbackLocally()
    {
        try
        {
            string filename = $"feedback_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
            string filePath = Path.Combine(Application.persistentDataPath, localSavePath, filename);
            
            string json = JsonConvert.SerializeObject(feedbackResults, Formatting.Indented);
            File.WriteAllText(filePath, json);
            
            Debug.Log($"Feedback saved locally to: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving feedback locally: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Send feedback results to the server
    /// </summary>
    private async void SendFeedbackToServer()
    {
        if (webSocketClient == null || !webSocketClient.IsConnected)
        {
            Debug.LogWarning("Cannot send feedback: WebSocket not connected");
            return;
        }
        
        try
        {
            // Create feedback message
            var feedbackMessage = new
            {
                type = "feedback",
                session_id = sessionManager != null ? sessionManager.GetSessionId() : "unknown",
                timestamp = DateTime.UtcNow.ToString("o"),
                data = feedbackResults
            };
            
            // Convert to JSON and send
            string jsonMessage = JsonConvert.SerializeObject(feedbackMessage);
            await webSocketClient.SendMessage(jsonMessage);
            
            Debug.Log("Feedback sent to server");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending feedback to server: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Configure feedback questions programmatically
    /// </summary>
    /// <param name="questions">Array of feedback questions</param>
    public void SetFeedbackQuestions(FeedbackQuestion[] questions)
    {
        feedbackQuestions = questions;
    }
}