# VR Interview System - UI Documentation

## Architecture Overview

The UI subsystem in the VR Interview System manages all user interfaces and visual feedback within the virtual reality environment. It is designed to provide intuitive interaction, clear communication feedback, and a comfortable VR experience. The UI components are optimized for VR visibility and interaction while maintaining performance on standalone VR devices.

## Key Classes/Components

### UIManager

`UIManager` is the central hub for all UI operations, managing panels, text displays, notifications, and UI state transitions.

```csharp
public class UIManager : MonoBehaviour
{
    // Panel visibility control
    public void SetConnectionPanelVisible(bool visible)
    public void SetProcessingPanelVisible(bool visible)
    public void SetNotificationPanelVisible(bool visible)
    public void SetDebugPanelVisible(bool visible)
    
    // Transcript management
    public void ShowTranscript(string userText, string llmText)
    public void ShowUserTranscript(string text)
    public void ShowLLMResponse(string text)
    public void ClearTranscript()
    public void HideTranscript()
    
    // Status updates
    public void UpdateStatus(string status)
    public void UpdateProgress(float progress)
    public void ShowProcessingMessage(string message)
    public void ShowThinkingMessage(string message)
    
    // Notifications
    public void ShowNotification(string message)
    public void ShowError(string errorMessage)
    public void ShowMessage(string message)
    public void UpdateConnectionStatus(bool connected)
    public void UpdateStateDisplay(string state)
}
```

### FeedbackSystem

`FeedbackSystem` handles user feedback mechanisms, including visual indicators, haptic feedback, and interactive feedback collection.

```csharp
public class FeedbackSystem : MonoBehaviour
{
    // Feedback collection
    public void RecordPositiveFeedback()
    public void RecordNegativeFeedback()
    public void RecordNeutralFeedback()
    
    // Haptic feedback
    public void ProvideFeedback(FeedbackType type)
    
    // Visual feedback
    public void ShowVisualFeedback(string message, FeedbackType type)
    
    // Feedback history
    public List<FeedbackEntry> GetFeedbackHistory()
}
```

### VRInteractionUI

`VRInteractionUI` handles VR-specific UI interactions, including gaze detection, controller pointer events, and hand tracking.

```csharp
public class VRInteractionUI : MonoBehaviour
{
    // Interaction modes
    public void SetInteractionMode(InteractionMode mode)
    public void EnableGazeInteraction(bool enable)
    public void EnableControllerInteraction(bool enable)
    public void EnableHandInteraction(bool enable)
    
    // Interaction events
    public event Action<GameObject> OnElementFocused;
    public event Action<GameObject> OnElementSelected;
    public event Action<GameObject, Vector3> OnElementInteracted;
    
    // Feedback functions
    public void ProvideHapticFeedback(float strength, float duration)
    public void ProvideVisualFeedback(GameObject target)
}
```

### DebugDisplay

`DebugDisplay` provides real-time debugging information for developers within the VR environment.

```csharp
public class DebugDisplay : MonoBehaviour
{
    // Log management
    public void LogMessage(string message, LogType type)
    public void ClearLogs()
    
    // Visibility control
    public void SetVisible(bool visible)
    public void ToggleVisibility()
    
    // Debug data display
    public void ShowSystemStatus(string statusInfo)
    public void ShowNetworkStatus(string networkInfo)
    public void ShowAudioStatus(string audioInfo)
}
```

### LoadingScreen

`LoadingScreen` manages transitions between system states with visual feedback.

```csharp
public class LoadingScreen : MonoBehaviour
{
    // Control methods
    public void Show(string message = "Loading...")
    public void Hide()
    public void UpdateProgress(float progress, string message = null)
    
    // Animation control
    public void SetAnimationSpeed(float speed)
    public void UseCustomAnimation(string animationName)
}
```

## UI Component Architecture

The UI system follows a modular architecture with each component serving a specific purpose:

```
                  ┌─────────────────┐
                  │                 │
                  │    UIManager    │◄───────┐
                  │                 │        │
                  └────────┬────────┘        │
                           │                 │
           ┌───────────────┼───────────────┐ │
           │               │               │ │
           ▼               ▼               ▼ │
┌─────────────────┐ ┌─────────────────┐ ┌────┴──────────┐
│                 │ │                 │ │               │
│  TranscriptPanel│ │  StatusDisplay  │ │NotificationPanel
│                 │ │                 │ │               │
└────────┬────────┘ └────────┬────────┘ └────┬──────────┘
         │                   │                │
         │                   │                │
┌────────▼────────┐ ┌────────▼────────┐ ┌────▼──────────┐
│                 │ │                 │ │               │
│ VRInteractionUI │ │  FeedbackSystem │ │  DebugDisplay │
│                 │ │                 │ │               │
└─────────────────┘ └─────────────────┘ └───────────────┘
```

## Transcript Display System

The transcript display shows conversation history between the user and the virtual interviewer:

```csharp
public void ShowTranscript(string userText, string llmText)
{
    if (transcriptPanel != null)
        transcriptPanel.SetActive(true);
    
    if (userTranscriptText != null && !string.IsNullOrEmpty(userText))
        userTranscriptText.text = $"You: {userText}";
    
    if (llmResponseText != null && !string.IsNullOrEmpty(llmText))
        llmResponseText.text = $"Interviewer: {llmText}";
}
```

Key features:
1. **Separate User and AI Text**: Clear visual distinction between user speech and AI responses
2. **Automatic Scrolling**: Keeps the most recent exchange visible
3. **Text Formatting**: Supports rich text highlighting for important information
4. **Persistence Control**: Option to keep transcript visible or auto-hide after time
5. **VR-Optimized Typography**: Font sizes and styles optimized for VR legibility

## Progress Visualization

The system visualizes various processing states:

```csharp
public void UpdateProgress(float progress)
{
    if (progressBar != null)
    {
        progressBar.value = Mathf.Clamp01(progress);
    }
}

public void ShowProcessingMessage(string message)
{
    if (processingPanel != null && !processingPanel.activeSelf)
        SetProcessingPanelVisible(true);
    
    if (processingText != null)
        processingText.text = message;
}
```

Progress visualization includes:
1. **Linear Progress Bar**: Shows completion percentage for determinate operations
2. **Spinner Animation**: Indicates indeterminate processing states
3. **Status Text**: Provides contextual information about the current operation
4. **State Transitions**: Visual effects for transitions between system states

## VR Interaction Patterns

The UI system implements several VR-specific interaction patterns:

### Gaze-Based Interaction

```csharp
private void UpdateGazeInteraction()
{
    if (!enableGazeInteraction)
        return;
        
    // Cast ray from center of view
    Ray gazeRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
    RaycastHit hit;
    
    if (Physics.Raycast(gazeRay, out hit, gazeMaxDistance, interactionLayerMask))
    {
        // Check if hit object is interactable
        IVRInteractable interactable = hit.collider.GetComponent<IVRInteractable>();
        if (interactable != null)
        {
            // Focus handling
            if (_currentGazedObject != hit.collider.gameObject)
            {
                if (_currentGazedObject != null)
                {
                    // Unfocus previous object
                    IVRInteractable prevInteractable = _currentGazedObject.GetComponent<IVRInteractable>();
                    if (prevInteractable != null)
                        prevInteractable.OnGazeExit();
                }
                
                // Focus new object
                _currentGazedObject = hit.collider.gameObject;
                interactable.OnGazeEnter();
                OnElementFocused?.Invoke(_currentGazedObject);
            }
            
            // Update gaze timer for selection
            _currentGazeTime += Time.deltaTime;
            
            // Select if gaze duration exceeds threshold
            if (_currentGazeTime >= gazeSelectionTime)
            {
                interactable.OnSelect();
                OnElementSelected?.Invoke(_currentGazedObject);
                _currentGazeTime = 0f;
            }
            
            // Update visual feedback
            if (gazeIndicator != null)
            {
                gazeIndicator.transform.position = hit.point;
                gazeIndicator.transform.rotation = Quaternion.LookRotation(hit.normal);
                float fillAmount = _currentGazeTime / gazeSelectionTime;
                UpdateGazeIndicator(fillAmount);
            }
        }
    }
    else
    {
        // Nothing being gazed at
        if (_currentGazedObject != null)
        {
            // Unfocus current object
            IVRInteractable interactable = _currentGazedObject.GetComponent<IVRInteractable>();
            if (interactable != null)
                interactable.OnGazeExit();
                
            _currentGazedObject = null;
        }
        
        _currentGazeTime = 0f;
        
        // Reset gaze indicator
        if (gazeIndicator != null)
        {
            UpdateGazeIndicator(0f);
        }
    }
}
```

### Controller Pointer Interaction

```csharp
private void UpdateControllerInteraction()
{
    if (!enableControllerInteraction)
        return;
        
    // Get controller transform
    Transform controllerTransform = GetActiveController();
    if (controllerTransform == null)
        return;
        
    // Cast ray from controller
    Ray controllerRay = new Ray(controllerTransform.position, controllerTransform.forward);
    RaycastHit hit;
    
    if (Physics.Raycast(controllerRay, out hit, pointerMaxDistance, interactionLayerMask))
    {
        // Check if hit object is interactable
        IVRInteractable interactable = hit.collider.GetComponent<IVRInteractable>();
        if (interactable != null)
        {
            // Update pointer visualization
            if (controllerPointer != null)
            {
                controllerPointer.SetActive(true);
                UpdatePointerLine(controllerTransform.position, hit.point);
                UpdatePointerReticle(hit.point, hit.normal);
            }
            
            // Focus handling
            if (_currentPointedObject != hit.collider.gameObject)
            {
                if (_currentPointedObject != null)
                {
                    // Unfocus previous object
                    IVRInteractable prevInteractable = _currentPointedObject.GetComponent<IVRInteractable>();
                    if (prevInteractable != null)
                        prevInteractable.OnPointerExit();
                }
                
                // Focus new object
                _currentPointedObject = hit.collider.gameObject;
                interactable.OnPointerEnter();
                OnElementFocused?.Invoke(_currentPointedObject);
            }
            
            // Check for trigger press
            if (GetTriggerPressed())
            {
                if (!_triggerWasPressed)
                {
                    _triggerWasPressed = true;
                    interactable.OnSelect();
                    OnElementSelected?.Invoke(_currentPointedObject);
                    ProvideHapticFeedback(0.3f, 0.1f);
                }
            }
            else
            {
                _triggerWasPressed = false;
            }
        }
    }
    else
    {
        // Nothing being pointed at
        if (_currentPointedObject != null)
        {
            // Unfocus current object
            IVRInteractable interactable = _currentPointedObject.GetComponent<IVRInteractable>();
            if (interactable != null)
                interactable.OnPointerExit();
                
            _currentPointedObject = null;
        }
        
        // Update pointer visualization for default state
        if (controllerPointer != null)
        {
            UpdatePointerLine(controllerTransform.position, controllerTransform.position + controllerTransform.forward * pointerMaxDistance);
            controllerPointer.SetActive(GetTriggerPressed());
        }
        
        _triggerWasPressed = false;
    }
}
```

## Error and Status Notifications

The system provides clear error and status feedback to the user:

```csharp
public void ShowError(string errorMessage)
{
    ShowNotification($"Error: {errorMessage}");
}

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

private IEnumerator AutoHideNotification()
{
    yield return new WaitForSeconds(notificationDuration);
    SetNotificationPanelVisible(false);
    autoHideNotification = null;
}
```

Key features:
1. **Timed Auto-Hide**: Notifications automatically disappear after a configurable duration
2. **Visual Hierarchy**: Critical errors are more prominent than status updates
3. **Spatial Positioning**: Notifications appear in comfortable viewing areas
4. **Haptic Feedback**: Optional vibration for important notifications
5. **Error Recovery Suggestions**: Common errors include suggested fixes

## Unity-Specific Implementation

### Unity UI Components

The system leverages Unity's UI system with VR-specific optimizations:

```csharp
[SerializeField] private Canvas mainCanvas;
[SerializeField] private GraphicRaycaster raycaster;
[SerializeField] private EventSystem eventSystem;

private void ConfigureCanvasForVR()
{
    if (mainCanvas != null)
    {
        // Set render mode to world space for VR
        mainCanvas.renderMode = RenderMode.WorldSpace;
        
        // Position the canvas at comfortable viewing distance
        mainCanvas.transform.position = Camera.main.transform.position + Camera.main.transform.forward * canvasDistance;
        
        // Make canvas face the user
        mainCanvas.transform.rotation = Quaternion.LookRotation(
            mainCanvas.transform.position - Camera.main.transform.position
        );
        
        // Set appropriate scale for VR readability
        mainCanvas.transform.localScale = new Vector3(canvasScale, canvasScale, canvasScale);
    }
}
```

### TextMeshPro Integration

The system uses TextMeshPro for high-quality text rendering in VR:

```csharp
[SerializeField] private TMP_Text userTranscriptText;
[SerializeField] private TMP_Text llmResponseText;

private void ConfigureTextForVR()
{
    // Configure text for optimal VR readability
    if (userTranscriptText != null)
    {
        userTranscriptText.fontSize = vrFontSize;
        userTranscriptText.enableWordWrapping = true;
        userTranscriptText.overflowMode = TextOverflowModes.Ellipsis;
        userTranscriptText.extraPadding = true; // Better for VR legibility
    }
    
    if (llmResponseText != null)
    {
        llmResponseText.fontSize = vrFontSize;
        llmResponseText.enableWordWrapping = true;
        llmResponseText.overflowMode = TextOverflowModes.Ellipsis;
        llmResponseText.extraPadding = true;
    }
}
```

## VR-Specific Considerations

### Comfortable Viewing Distances

The UI is positioned at comfortable viewing distances to reduce eye strain:

```csharp
private void PositionUIElements()
{
    // Default comfortable viewing distance in VR (1.5 meters)
    float comfortableDistance = 1.5f;
    
    // Position main panels at comfortable distance
    if (transcriptPanel != null)
    {
        PositionPanelAtDistance(transcriptPanel, comfortableDistance, 0f, -0.1f);
    }
    
    if (processingPanel != null)
    {
        PositionPanelAtDistance(processingPanel, comfortableDistance, 0f, 0.1f);
    }
    
    if (notificationPanel != null)
    {
        // Position notifications slightly closer for emphasis
        PositionPanelAtDistance(notificationPanel, comfortableDistance * 0.9f, 0f, 0.2f);
    }
}
```

### Head-Locked vs. World-Locked UI

The system supports both head-locked and world-locked UI elements:

```csharp
private void UpdateUIPositioning()
{
    if (headLockedElements.Count > 0)
    {
        // Update head-locked elements to follow the user's gaze
        foreach (Transform element in headLockedElements)
        {
            if (element != null)
            {
                Vector3 targetPosition = Camera.main.transform.position + 
                                        Camera.main.transform.forward * element.GetComponent<UIElement>().ViewingDistance;
                                        
                // Smoothly move element
                element.position = Vector3.Lerp(element.position, targetPosition, Time.deltaTime * uiFollowSpeed);
                
                // Make element face the user
                element.rotation = Quaternion.Lerp(
                    element.rotation,
                    Quaternion.LookRotation(element.position - Camera.main.transform.position),
                    Time.deltaTime * uiRotationSpeed
                );
            }
        }
    }
}
```

## Common Issues

### Known Issues and Solutions

1. **Text Readability Issues**:
   - **Symptoms**: Difficulty reading text, eye strain
   - **Solution**: Increase font size, adjust contrast, position at optimal distance

2. **UI Element Placement**:
   - **Symptoms**: UI elements too close/far or in uncomfortable viewing angles
   - **Solution**: Adjust default distances and implement comfort zone checks

3. **Interaction Accuracy**:
   - **Symptoms**: Difficulty selecting UI elements, accidental selections
   - **Solution**: Increase interaction colliders, add visual feedback, implement hover states

4. **Canvas Performance**:
   - **Symptoms**: Frame rate drops when viewing UI
   - **Solution**: Reduce overdraw, optimize canvas rebuild, limit text updates

## Usage Examples

### Setting Up New UI Panel

```csharp
// Creating a new notification panel
private void SetupNotificationPanel()
{
    // Create panel gameobject
    GameObject panel = new GameObject("NotificationPanel");
    panel.transform.SetParent(transform);
    
    // Add required components
    RectTransform rectTransform = panel.AddComponent<RectTransform>();
    CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();
    Image background = panel.AddComponent<Image>();
    
    // Configure transform
    rectTransform.sizeDelta = new Vector2(400, 100);
    rectTransform.anchorMin = new Vector2(0.5f, 0.8f);
    rectTransform.anchorMax = new Vector2(0.5f, 0.8f);
    rectTransform.pivot = new Vector2(0.5f, 0.5f);
    rectTransform.anchoredPosition = Vector2.zero;
    
    // Configure appearance
    background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    
    // Add text component
    GameObject textObj = new GameObject("NotificationText");
    textObj.transform.SetParent(panel.transform);
    
    RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
    textRectTransform.sizeDelta = new Vector2(380, 80);
    textRectTransform.anchorMin = Vector2.zero;
    textRectTransform.anchorMax = Vector2.one;
    textRectTransform.pivot = new Vector2(0.5f, 0.5f);
    textRectTransform.anchoredPosition = Vector2.zero;
    
    TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
    text.fontSize = 24;
    text.alignment = TextAlignmentOptions.Center;
    text.enableWordWrapping = true;
    
    // Hide initially
    panel.SetActive(false);
    
    // Store reference
    notificationPanel = panel;
    notificationText = text;
}
```

### Showing User Transcript

```csharp
// Display user's transcribed speech
public void ShowUserTranscript(string text)
{
    if (transcriptPanel != null && !transcriptPanel.activeSelf)
        transcriptPanel.SetActive(true);
    
    if (userTranscriptText != null && !string.IsNullOrEmpty(text))
    {
        // Format text for display
        string formattedText = $"You: {text}";
        
        // Check if text has changed
        if (userTranscriptText.text != formattedText)
        {
            userTranscriptText.text = formattedText;
            
            // Animate text appearance
            if (animateTextChanges)
            {
                StartCoroutine(AnimateTextChange(userTranscriptText));
            }
        }
    }
}
```

## VR-Specific Optimizations

### Performance Considerations

1. **Canvas Batching**: UI elements are grouped to minimize draw calls
2. **Dynamic Resolution**: UI canvas resolution scales based on device performance
3. **Gaze-Based LOD**: Elements not in view are simplified or disabled
4. **Text Mesh Caching**: Frequently used text is pre-generated and cached
5. **Render Priority**: UI elements use a separate rendering queue to avoid depth issues

### Accessibility Features

The UI includes accessibility considerations:

1. **High Contrast Mode**: Enhanced contrast for better visibility
2. **Text Size Options**: Adjustable text scale for visual impairments
3. **Color Blind Modes**: Alternative color schemes for common color vision deficiencies
4. **Interaction Assistance**: Extended timers for selections
5. **Audio Cues**: Optional sound feedback for UI interactions

## Conclusion

The UI subsystem of the VR Interview System provides an intuitive, comfortable, and responsive interface for users to interact with the virtual interviewer. Its modular design, VR-specific optimizations, and accessibility features ensure that users can focus on the interview experience without interface distractions or discomfort.
