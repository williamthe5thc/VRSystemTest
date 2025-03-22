# VR Interview System - Remaining Class References

## SettingsManager

`SettingsManager` handles user preferences and application configuration.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `SettingsManager` | Static singleton instance |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `LoadSettings()` | `void` | Loads settings from persistent storage |
| `SaveSettings()` | `void` | Saves current settings to persistent storage |
| `SetSetting<T>(string key, T value)` | `void` | Sets a setting value by key |
| `GetSetting<T>(string key, T defaultValue = default)` | `T` | Gets a setting value by key |
| `ResetToDefaults()` | `void` | Resets all settings to defaults |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnSettingsChanged` | `Action` | Fired when any setting is changed |
| `OnSettingChanged<T>` | `Action<string, T, T>` | Fired when a specific setting changes |

### Usage Example

```csharp
// Access settings singleton
SettingsManager settings = SettingsManager.Instance;

// Load settings from storage
settings.LoadSettings();

// Get server URL setting with fallback
string serverUrl = settings.GetSetting<string>("ServerUrl", "ws://localhost:8765");

// Update a setting
settings.SetSetting("AudioVolume", 0.8f);

// Listen for settings changes
settings.OnSettingsChanged += HandleSettingsChanged;
settings.OnSettingChanged<float>("AudioVolume", HandleVolumeChanged);

// Save settings to storage
settings.SaveSettings();

// Event handler for volume changes
private void HandleVolumeChanged(string key, float oldValue, float newValue)
{
    Debug.Log($"Volume changed from {oldValue} to {newValue}");
    audioPlayback.SetVolume(newValue);
}
```

---

## UIManager

`UIManager` is the central controller for all user interface elements.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ConnectionStatus` | `TextMeshProUGUI` | UI text displaying connection status |
| `ReconnectButton` | `Button` | Button for manual reconnection |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `SetConnectionPanelVisible(bool visible)` | `void` | Shows/hides the connection panel |
| `UpdateConnectionStatus(string status)` | `void` | Updates the connection status text |
| `UpdateConnectionStatus(bool connected)` | `void` | Updates connection status based on connected state |
| `SetReconnectButtonInteractable(bool interactable)` | `void` | Enables/disables reconnect button |
| `UpdateStatus(string status)` | `void` | Updates the status display |
| `UpdateProgress(float progress)` | `void` | Updates the progress bar value |
| `ShowUserTranscript(string text)` | `void` | Shows user transcript in UI |
| `ShowLLMResponse(string text)` | `void` | Shows AI response in UI |
| `ClearTranscript()` | `void` | Clears the transcript display |
| `ShowError(string errorMessage)` | `void` | Shows an error message |
| `ShowMessage(string message)` | `void` | Shows a notification message |
| `ShowNotification(string message)` | `void` | Shows a temporary notification |

### Events

No public events.

### Usage Example

```csharp
// Update connection status
uiManager.UpdateConnectionStatus("Connected to server");
uiManager.UpdateConnectionStatus(true); // Alternative boolean version

// Show progress for operation
uiManager.UpdateStatus("Processing your request...");
uiManager.UpdateProgress(0.75f);

// Show conversation transcript
uiManager.ShowUserTranscript("Tell me about virtual reality");
uiManager.ShowLLMResponse("Virtual reality is an immersive technology that...");

// Show notifications
uiManager.ShowNotification("Settings saved successfully");
uiManager.ShowError("Failed to connect to server");
```

---

## VRInputHandler

`VRInputHandler` manages VR controller inputs and interactions.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsHandTrackingEnabled` | `bool` | Whether hand tracking is enabled |
| `LeftControllerActive` | `bool` | Whether left controller is active |
| `RightControllerActive` | `bool` | Whether right controller is active |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `EnableHandTracking(bool enable)` | `void` | Enables/disables hand tracking |
| `GetTriggerValue(ControllerHand hand)` | `float` | Gets trigger press value |
| `GetGripValue(ControllerHand hand)` | `float` | Gets grip press value |
| `GetThumbstickPosition(ControllerHand hand)` | `Vector2` | Gets thumbstick position |
| `IsButtonPressed(ControllerHand hand, VRButton button)` | `bool` | Checks if a button is pressed |
| `RegisterInteractable(IVRInteractable interactable)` | `void` | Registers an interactable object |
| `UnregisterInteractable(IVRInteractable interactable)` | `void` | Unregisters an interactable object |
| `TriggerHapticFeedback(ControllerHand hand, float amplitude, float duration)` | `void` | Triggers haptic feedback |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnControllerStateChanged` | `Action<ControllerHand, bool>` | Fired when controller state changes |
| `OnButtonPressed` | `Action<ControllerHand, VRButton>` | Fired when button is pressed |
| `OnButtonReleased` | `Action<ControllerHand, VRButton>` | Fired when button is released |

### Usage Example

```csharp
// Check for controller input
if (vrInputHandler.IsButtonPressed(ControllerHand.Right, VRButton.PrimaryButton))
{
    // Handle primary button press
    StartInterview();
}

// Get input values
float triggerValue = vrInputHandler.GetTriggerValue(ControllerHand.Right);
Vector2 thumbstickPos = vrInputHandler.GetThumbstickPosition(ControllerHand.Left);

// Register for button events
vrInputHandler.OnButtonPressed += HandleButtonPressed;
vrInputHandler.OnButtonReleased += HandleButtonReleased;

// Provide haptic feedback
vrInputHandler.TriggerHapticFeedback(ControllerHand.Right, 0.7f, 0.1f);

// Event handler for button press
private void HandleButtonPressed(ControllerHand hand, VRButton button)
{
    if (hand == ControllerHand.Right && button == VRButton.Trigger)
    {
        // Start recording when right trigger pressed
        microphoneCapture.StartRecording();
    }
}
```

---

## WebSocketClient

`WebSocketClient` manages the WebSocket connection to the interview server.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsConnected` | `bool` | Whether connection is established |
| `LastPingTime` | `float` | Time of last ping in ms |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Connect()` | `Task` | Establishes connection to server |
| `Close(bool intentional = true)` | `Task` | Closes the connection |
| `SendMessage(string message, bool allowQueue = true)` | `Task<bool>` | Sends a text message |
| `SendAudioData(byte[] audioData, string sessionId)` | `Task` | Sends audio data |
| `SendAudioDataNonAsync(byte[] audioData, string sessionId, Action onComplete = null, Action<string> onError = null)` | `void` | Sends audio without async |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnMessageReceived` | `Action<string>` | Fired when message received |
| `OnConnected` | `Action` | Fired when connection established |
| `OnDisconnected` | `Action<WebSocketCloseCode>` | Fired when connection closed |
| `OnError` | `Action<string>` | Fired when error occurs |
| `OnConnectionEstablished` | `Action` | Fired when connection fully established |
| `MessageReceived` | `Action<string>` | Alternative message received event |

### Usage Example

```csharp
// Subscribe to WebSocket events
webSocketClient.OnMessageReceived += HandleMessage;
webSocketClient.OnConnected += HandleConnected;
webSocketClient.OnDisconnected += HandleDisconnected;
webSocketClient.OnError += HandleError;

// Connect to server
await webSocketClient.Connect();

// Send a message
bool sent = await webSocketClient.SendMessage(jsonMessage);
if (!sent)
{
    Debug.LogError("Failed to send message");
}

// Send audio data
await webSocketClient.SendAudioData(audioBytes, sessionId);

// Close connection
await webSocketClient.Close();

// Handle received messages
private void HandleMessage(string message)
{
    Debug.Log($"Received message: {message.Substring(0, Math.Min(100, message.Length))}...");
    messageHandler.ProcessMessage(message);
}
```
