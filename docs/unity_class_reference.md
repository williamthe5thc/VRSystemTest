
## Overview

This document provides an incomplete comprehensive reference of the key classes in the VR Interview System's Unity client. Classes are organized alphabetically, with descriptions of their purpose, public methods and properties, event definitions, and usage examples.

## Table of Contents

1. [AppManager](#appmanager)
2. [AudioPlayback](#audioplayback)
3. [AudioProcessor](#audioprocessor)
4. [AudioStreamer](#audiostreamer)
5. [AvatarController](#avatarcontroller)
6. [ClientCapabilities](#clientcapabilities)
7. [ConnectionManager](#connectionmanager)
8. [DebugDisplay](#debugdisplay)
9. [FacialExpressions](#facialexpressions)
10. [FeedbackSystem](#feedbacksystem)
11. [GestureSystem](#gesturesystem)
12. [LipSync](#lipsync)
13. [LoadingScreen](#loadingscreen)
14. [MessageHandler](#messagehandler)
15. [MicrophoneCapture](#microphonecapture)
16. [SceneInitializer](#sceneinitializer)
17. [SessionManager](#sessionmanager)
18. [SettingsManager](#settingsmanager)
19. [UIManager](#uimanager)
20. [VRInputHandler](#vrinputhandler)
21. [WebSocketClient](#websocketclient)

---

## AppManager

`AppManager` is the main application controller responsible for initializing and managing the overall system.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `AppManager` | Static singleton instance |
| `IsInitialized` | `bool` | Whether the application is initialized |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `StartInterview()` | `void` | Starts a new interview session |
| `EndInterview()` | `void` | Ends the current interview session |
| `ResetInterview()` | `void` | Resets the current interview session |
| `ExitApplication()` | `void` | Exits the application |

### MonoBehaviour Lifecycle

- `Awake()`: Sets up singleton and initializes the application
- `Start()`: Not used in this class
- `Update()`: Not used in this class

### Usage Example

```csharp
// Access the AppManager from any script
AppManager.Instance.StartInterview();

// Check if initialized before performing operations
if (AppManager.Instance.IsInitialized)
{
    // Perform operation that requires initialization
}

// End the interview session
AppManager.Instance.EndInterview();
```

---

## AudioPlayback

`AudioPlayback` handles playing audio responses received from the server.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsPlaying` | `bool` | Whether audio is currently playing |
| `PlaybackPosition` | `float` | Current normalized playback position (0-1) |
| `Volume` | `float` | Current audio volume |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `PlayAudioResponse(byte[] audioData)` | `void` | Plays audio data received from server |
| `StopPlayback()` | `void` | Stops the current audio playback |
| `PausePlayback()` | `void` | Pauses the current audio playback |
| `ResumePlayback()` | `void` | Resumes a paused audio playback |
| `SetVolume(float volume)` | `void` | Sets the playback volume |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnPlaybackStarted` | `Action` | Fired when audio playback begins |
| `OnPlaybackCompleted` | `Action` | Fired when audio playback finishes |
| `OnPlaybackProgress` | `Action<float>` | Fired during playback with normalized progress |

### Usage Example

```csharp
// Subscribe to playback events
audioPlayback.OnPlaybackStarted += HandlePlaybackStarted;
audioPlayback.OnPlaybackCompleted += HandlePlaybackCompleted;
audioPlayback.OnPlaybackProgress += UpdateProgressBar;

// Play audio data received from server
byte[] audioData = /* audio data from server */;
audioPlayback.PlayAudioResponse(audioData);

// Update UI based on playback state
if (audioPlayback.IsPlaying)
{
    ShowPlayingIndicator();
}

// Set the volume
audioPlayback.SetVolume(0.8f);
```

---

## AudioProcessor

`AudioProcessor` handles audio data processing, conversion, and formatting for transmission.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `TargetSampleRate` | `int` | Sample rate for processed audio |
| `TargetFormat` | `string` | Target audio format for transmission |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `ProcessAudioForServer(AudioClip clip)` | `byte[]` | Converts AudioClip to byte array for sending |
| `GetSignalQuality(float[] samples)` | `float` | Calculates signal quality of audio samples |
| `ConvertToWavFormat(float[] samples, int sampleRate)` | `byte[]` | Converts raw samples to WAV format |
| `SetCompressionQuality(AudioCompressionQuality quality)` | `void` | Sets the compression quality level |

### Usage Example

```csharp
// Process an audio clip for transmission
AudioClip recordedClip = // recorded audio clip
byte[] processedAudio = audioProcessor.ProcessAudioForServer(recordedClip);

// Check the signal quality from raw samples
float[] audioSamples = new float[1024]; // audio samples
microphoneClip.GetData(audioSamples, 0);
float signalQuality = audioProcessor.GetSignalQuality(audioSamples);

// Log quality information
Debug.Log($\"Signal quality: {signalQuality}, Target format: {audioProcessor.TargetFormat}\");
```

---

## AudioStreamer

`AudioStreamer` manages streaming audio to and from the server for real-time communication.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsStreaming` | `bool` | Whether audio streaming is active |
| `BufferSize` | `int` | Size of the audio buffer in samples |
| `BufferCount` | `int` | Number of buffered audio packets |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `StartStreaming()` | `void` | Starts audio streaming mode |
| `StopStreaming()` | `void` | Stops audio streaming mode |
| `ProcessIncomingAudioPacket(byte[] audioData)` | `void` | Processes a received audio packet |
| `SetBufferSize(int bufferSize)` | `void` | Sets the audio buffer size |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnAudioPacketReceived` | `Action<byte[]>` | Fired when a new audio packet is received |
| `OnStreamingStarted` | `Action` | Fired when streaming begins |
| `OnStreamingStopped` | `Action` | Fired when streaming ends |

### Usage Example

```csharp
// Start streaming mode
audioStreamer.StartStreaming();

// Register for packet events
audioStreamer.OnAudioPacketReceived += HandleAudioPacket;

// Set larger buffer for unstable connections
if (hasUnstableConnection)
{
    audioStreamer.SetBufferSize(4096);
}

// Process an incoming packet
void HandleAudioPacket(byte[] packetData)
{
    // Process or play the audio packet
    audioPlayback.PlayAudioBuffer(packetData);
}
```

---

## AvatarController

`AvatarController` manages the virtual interviewer avatar's animations, expressions, and states.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CurrentState` | `string` | Current avatar state (e.g., \"IDLE\", \"RESPONDING\") |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `SetIdleState()` | `void` | Sets the avatar to idle state |
| `SetListeningState()` | `void` | Sets the avatar to listening state |
| `SetThinkingState()` | `void` | Sets the avatar to thinking state |
| `SetSpeakingState()` | `void` | Sets the avatar to speaking state |
| `SetAttentiveState()` | `void` | Sets the avatar to attentive state |
| `SetConfusedState()` | `void` | Sets the avatar to confused state |
| `OnAudioPlaybackStarted()` | `void` | Called when audio playback begins |
| `OnAudioPlaybackCompleted()` | `void` | Called when audio playback ends |
| `UpdateLipSync(float normalizedTime)` | `void` | Updates lip sync during audio playback |
| `SetAnimationSpeed(float speed)` | `void` | Sets the animation speed multiplier |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnStateChanged` | `Action<string>` | Fired when avatar state changes |

### Usage Example

```csharp
// Subscribe to state change events
avatarController.OnStateChanged += HandleAvatarStateChanged;

// Set avatar state based on interview state
switch (currentInterviewState)
{
    case \"LISTENING\":
        avatarController.SetListeningState();
        break;
    case \"PROCESSING\":
        avatarController.SetThinkingState();
        break;
    case \"RESPONDING\":
        avatarController.SetSpeakingState();
        break;
}

// Connect with audio system
audioPlayback.OnPlaybackStarted += avatarController.OnAudioPlaybackStarted;
audioPlayback.OnPlaybackCompleted += avatarController.OnAudioPlaybackCompleted;
audioPlayback.OnPlaybackProgress += avatarController.UpdateLipSync;
```

---

## ClientCapabilities

`ClientCapabilities` defines the client's supported features for server communication.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `supportsAudio` | `bool` | Whether client supports audio |
| `supportsLipSync` | `bool` | Whether client supports lip sync |
| `supportsFacialExpressions` | `bool` | Whether client supports facial expressions |
| `supportsGestures` | `bool` | Whether client supports gestures |
| `supportedAudioFormats` | `string[]` | Array of supported audio formats |
| `sampleRate` | `int` | Preferred audio sample rate |
| `channels` | `int` | Preferred audio channel count |

### Usage Example

```csharp
// Create capabilities object
ClientCapabilities capabilities = new ClientCapabilities();
capabilities.supportsAudio = true;
capabilities.supportsLipSync = true;
capabilities.supportsFacialExpressions = true;
capabilities.supportsGestures = true;
capabilities.supportedAudioFormats = new string[] { \"wav\", \"mp3\" };
capabilities.sampleRate = 16000;
capabilities.channels = 1;

// Create message with capabilities
ClientCapabilitiesMessage message = new ClientCapabilitiesMessage(sessionId, capabilities);

// Convert to JSON and send
string jsonMessage = JsonUtility.ToJson(message);
await webSocketClient.SendMessage(jsonMessage);
```

---

## ConnectionManager

`ConnectionManager` oversees the WebSocket connection state and reconnection logic.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsConnected` | `bool` | Whether connection is established |
| `ReconnectAttempts` | `int` | Number of reconnection attempts |
| `ServerUrl` | `string` | WebSocket server URL |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Connect()` | `Task<bool>` | Establishes connection to server |
| `Disconnect()` | `Task` | Disconnects from server |
| `ForceReconnect()` | `Task<bool>` | Forces a reconnection attempt |
| `SetReconnectParameters(int maxAttempts, float delay)` | `void` | Sets reconnection parameters |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnConnected` | `Action` | Fired when connection established |
| `OnDisconnected` | `Action<DisconnectReason>` | Fired when connection lost |
| `OnReconnecting` | `Action<int, int>` | Fired during reconnection attempt |
| `OnReconnectFailed` | `Action` | Fired when all reconnection attempts fail |

### Usage Example

```csharp
// Subscribe to connection events
connectionManager.OnConnected += HandleConnected;
connectionManager.OnDisconnected += HandleDisconnected;
connectionManager.OnReconnecting += HandleReconnecting;

// Set reconnection parameters for poor connections
if (isUnstableNetwork)
{
    connectionManager.SetReconnectParameters(10, 3.0f);
}

// Attempt connection
bool connected = await connectionManager.Connect();
if (!connected)
{
    DisplayConnectionError();
}

// Handle reconnection attempts
void HandleReconnecting(int attempt, int maxAttempts)
{
    uiManager.ShowMessage($\"Reconnecting: Attempt {attempt} of {maxAttempts}\");
}
```

---

## DebugDisplay

`DebugDisplay` provides in-game debugging information display.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsVisible` | `bool` | Whether debug display is visible |
| `LogLevel` | `LogLevel` | Current log verbosity level |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `LogMessage(string message, LogType type)` | `void` | Logs a message to debug display |
| `ClearLogs()` | `void` | Clears all logged messages |
| `SetVisible(bool visible)` | `void` | Shows or hides debug display |
| `ToggleVisibility()` | `void` | Toggles debug display visibility |
| `ShowSystemStatus(string statusInfo)` | `void` | Displays system status info |
| `ShowNetworkStatus(string networkInfo)` | `void` | Displays network status info |
| `ShowAudioStatus(string audioInfo)` | `void` | Displays audio status info |

### Usage Example

```csharp
// Toggle debug display with keyboard shortcut
if (Input.GetKeyDown(KeyCode.F1))
{
    debugDisplay.ToggleVisibility();
}

// Log various message types
debugDisplay.LogMessage(\"Information message\", LogType.Log);
debugDisplay.LogMessage(\"Warning: Connection unstable\", LogType.Warning);
debugDisplay.LogMessage(\"Error: Failed to send message\", LogType.Error);

// Display current system status
string status = $\"Session ID: {sessionManager.SessionId}, State: {sessionManager.CurrentState}\";
debugDisplay.ShowSystemStatus(status);

// Display network information
string network = $\"Connected: {webSocketClient.IsConnected}, Ping: {webSocketClient.LastPingTime}ms\";
debugDisplay.ShowNetworkStatus(network);
```

---

## FacialExpressions

`FacialExpressions` controls the avatar's facial expressions and blending.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CurrentExpression` | `FacialExpression` | Current facial expression |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `SetExpression(FacialExpression expression)` | `void` | Sets a facial expression |
| `Blink()` | `void` | Triggers a blink animation |
| `ClearExpressions()` | `void` | Resets to neutral expression |
| `BlendToExpression(FacialExpression expression, float duration)` | `void` | Smoothly transitions to expression |
| `AddExpressionLayer(FacialExpression expression, float weight)` | `void` | Adds weighted expression layer |
| `EnableRandomExpressions(bool enable)` | `void` | Enables/disables random expressions |
| `TriggerEmotionalResponse(EmotionType emotion, float intensity)` | `void` | Triggers emotional response |

### Usage Example

```csharp
// Set a basic expression
facialExpressions.SetExpression(FacialExpression.Interested);

// Blend smoothly to another expression over 1.5 seconds
facialExpressions.BlendToExpression(FacialExpression.Thoughtful, 1.5f);

// Trigger an emotional response
facialExpressions.TriggerEmotionalResponse(EmotionType.Surprise, 0.7f);

// Trigger periodic blinking
InvokeRepeating(\"TriggerBlink\", 1.0f, Random.Range(3.0f, 5.0f));

private void TriggerBlink()
{
    facialExpressions.Blink();
}
```

---

## FeedbackSystem

`FeedbackSystem` handles user feedback collection and visualization.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `FeedbackEnabled` | `bool` | Whether feedback collection is enabled |
| `FeedbackCount` | `int` | Number of feedback entries collected |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `RecordPositiveFeedback()` | `void` | Records positive feedback |
| `RecordNegativeFeedback()` | `void` | Records negative feedback |
| `RecordNeutralFeedback()` | `void` | Records neutral feedback |
| `ProvideFeedback(FeedbackType type)` | `void` | Provides haptic/visual feedback |
| `ShowVisualFeedback(string message, FeedbackType type)` | `void` | Shows feedback message |
| `GetFeedbackHistory()` | `List<FeedbackEntry>` | Gets collected feedback history |
| `ClearFeedbackHistory()` | `void` | Clears feedback history |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnFeedbackRecorded` | `Action<FeedbackEntry>` | Fired when feedback is recorded |

### Usage Example

```csharp
// Connect feedback system to UI buttons
positiveButton.onClick.AddListener(feedbackSystem.RecordPositiveFeedback);
negativeButton.onClick.AddListener(feedbackSystem.RecordNegativeFeedback);
neutralButton.onClick.AddListener(feedbackSystem.RecordNeutralFeedback);

// Listen for feedback events
feedbackSystem.OnFeedbackRecorded += HandleFeedbackRecorded;

// Show feedback with custom message
feedbackSystem.ShowVisualFeedback(\"Thank you for your feedback!\", FeedbackType.Positive);

// Get feedback history for analytics
List<FeedbackEntry> history = feedbackSystem.GetFeedbackHistory();
AnalyzeFeedbackTrends(history);

// Process feedback in handle method
private void HandleFeedbackRecorded(FeedbackEntry entry)
{
    sessionManager.RecordUserFeedback(entry.feedbackType.ToString());
    uiManager.ShowMessage(\"Feedback recorded, thank you!\");
}
```

---

## GestureSystem

`GestureSystem` manages the avatar's hand and body gestures.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CurrentGesture` | `GestureType` | Current active gesture |
| `GestureFrequency` | `float` | Frequency of random gestures |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `PerformGesture(GestureType gestureType)` | `void` | Performs a specific gesture |
| `PerformRandomGesture()` | `void` | Performs a random appropriate gesture |
| `CancelGesture()` | `void` | Cancels the current gesture |
| `SetGestureFrequency(float frequency)` | `void` | Sets random gesture frequency |
| `EnableGestureCategory(string category, bool enabled)` | `void` | Enables/disables gesture category |
| `SyncGestureWithSpeech(string speechText)` | `void` | Synchronizes gestures with speech |
| `SetEmphasisPoints(List<float> normalizedTimes)` | `void` | Sets gesture emphasis timing points |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnGestureStarted` | `Action<GestureType>` | Fired when gesture begins |
| `OnGestureCompleted` | `Action<GestureType>` | Fired when gesture completes |

### Usage Example

```csharp
// Perform specific gesture
gestureSystem.PerformGesture(GestureType.PointForward);

// Connect with audio playback to sync gestures
audioPlayback.OnPlaybackStarted += () => {
    if (!string.IsNullOrEmpty(messageHandler.GetCurrentLLMResponse()))
    {
        gestureSystem.SyncGestureWithSpeech(messageHandler.GetCurrentLLMResponse());
    }
};

// Set emphasis points for speech
List<float> emphasisPoints = new List<float> { 0.2f, 0.5f, 0.8f };
gestureSystem.SetEmphasisPoints(emphasisPoints);

// Enable calmer gestures for formal interview
gestureSystem.EnableGestureCategory(\"Emphatic\", false);
gestureSystem.EnableGestureCategory(\"Subtle\", true);
```

---

## LipSync

`LipSync` manages the avatar's mouth movements to match audio.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsActive` | `bool` | Whether lip sync is active |
| `CurrentAmplitude` | `float` | Current audio amplitude |
| `Intensity` | `float` | Lip movement intensity |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `StartLipSync()` | `void` | Activates lip sync |
| `StopLipSync()` | `void` | Deactivates lip sync |
| `UpdateLipSyncValue(float audioAmplitude)` | `void` | Updates based on audio amplitude |
| `SetViseme(VisemeType viseme, float blend)` | `void` | Sets specific viseme blend |
| `ProcessAudioBuffer(float[] audioBuffer)` | `void` | Processes audio buffer for visemes |
| `SetVisemePreset(string presetName)` | `void` | Sets a viseme configuration preset |
| `SetSmoothingAmount(float amount)` | `void` | Sets smoothing between lip positions |
| `SetIntensity(float intensity)` | `void` | Sets lip movement intensity |

### Usage Example

```csharp
// Start lip sync during audio playback
audioPlayback.OnPlaybackStarted += lipSync.StartLipSync;
audioPlayback.OnPlaybackCompleted += lipSync.StopLipSync;

// Connect audio analysis to lip sync
void UpdateMouthMovement()
{
    if (audioSource.isPlaying)
    {
        // Get audio samples
        float[] samples = new float[256];
        audioSource.GetOutputData(samples, 0);
        
        // Calculate amplitude
        float sum = 0;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += Mathf.Abs(samples[i]);
        }
        float amplitude = sum / samples.Length;
        
        // Update lip sync
        lipSync.UpdateLipSyncValue(amplitude);
    }
}

// Set lip sync parameters
lipSync.SetSmoothingAmount(0.1f);
lipSync.SetIntensity(1.2f);
```

---

## LoadingScreen

`LoadingScreen` manages transition states and loading indicators.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsVisible` | `bool` | Whether loading screen is visible |
| `Progress` | `float` | Current progress value (0-1) |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Show(string message = \"Loading...\")` | `void` | Shows loading screen with message |
| `Hide()` | `void` | Hides loading screen |
| `UpdateProgress(float progress, string message = null)` | `void` | Updates progress and optionally message |
| `SetAnimationSpeed(float speed)` | `void` | Sets loading animation speed |
| `UseCustomAnimation(string animationName)` | `void` | Uses custom loading animation |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnShown` | `Action` | Fired when loading screen appears |
| `OnHidden` | `Action` | Fired when loading screen disappears |
| `OnProgressChanged` | `Action<float>` | Fired when progress value changes |

### Usage Example

```csharp
// Show loading screen at start of operation
loadingScreen.Show(\"Connecting to server...\");

// Update progress during operation
IEnumerator PerformOperation()
{
    for (int i = 0; i < 10; i++)
    {
        // Perform step
        yield return new WaitForSeconds(0.5f);
        
        // Update progress
        float progress = (i + 1) / 10f;
        loadingScreen.UpdateProgress(progress, $\"Processing step {i+1}/10...\");
    }
    
    // Hide when complete
    loadingScreen.Hide();
}

// Change animation for specific operations
if (isLongOperation)
{
    loadingScreen.SetAnimationSpeed(0.5f);
    loadingScreen.UseCustomAnimation(\"SlowPulse\");
}
else
{
    loadingScreen.SetAnimationSpeed(1.0f);
    loadingScreen.UseCustomAnimation(\"FastSpin\");
}
```

---

## MessageHandler

`MessageHandler` processes WebSocket messages and routes them to appropriate handlers.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CurrentUserTranscript` | `string` | Current transcript of user speech |
| `CurrentLLMResponse` | `string` | Current LLM response text |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `ProcessMessage(string jsonMessage)` | `void` | Processes received JSON message |
| `RegisterMessageHandler(string messageType, Action<string> handler)` | `void` | Registers custom message handler |
| `UnregisterMessageHandler(string messageType)` | `void` | Removes custom message handler |
| `GetCurrentUserTranscript()` | `string` | Gets the current user transcript |
| `GetCurrentLLMResponse()` | `string` | Gets the current LLM response |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnStateUpdate` | `Action<string, string, Dictionary<string, object>>` | Fired on state update |
| `OnAudioResponse` | `Action<byte[]>` | Fired when audio response received |
| `OnError` | `Action<string>` | Fired when error received |
| `MessageReceived` | `Action<MessageHandler, string>` | Fired when any message received |

### Usage Example

```csharp
// Register for standard message events
messageHandler.OnStateUpdate += HandleStateUpdate;
messageHandler.OnAudioResponse += HandleAudioResponse;
messageHandler.OnError += HandleError;

// Register custom handler for specific message type
messageHandler.RegisterMessageHandler(\"system_message\", HandleSystemMessage);

// Custom message handler
private void HandleSystemMessage(string jsonMessage)
{
    JObject messageObj = JObject.Parse(jsonMessage);
    string systemMessage = messageObj[\"message\"]?.ToString();
    
    uiManager.ShowNotification(systemMessage);
}

// Access transcript
string userTranscript = messageHandler.GetCurrentUserTranscript();
uiManager.ShowUserTranscript(userTranscript);
```

---

## MicrophoneCapture

`MicrophoneCapture` handles recording from the microphone and voice activity detection.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsRecording` | `bool` | Whether recording is active |
| `CurrentAudioLevel` | `float` | Current audio input level |
| `AvailableMicrophones` | `string[]` | Array of available microphone devices |
| `SelectedMicrophone` | `string` | Currently selected microphone |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `StartRecording()` | `void` | Begins recording from microphone |
| `StopRecording()` | `void` | Stops recording and processes audio |
| `SetMicrophone(string deviceName)` | `void` | Selects specific microphone device |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnRecordingStarted` | `Action` | Fired when recording begins |
| `OnRecordingStopped` | `Action` | Fired when recording ends |
| `OnAudioLevelChanged` | `Action<float>` | Fired when audio level changes |

### Usage Example

```csharp
// Display available microphones
string[] mics = microphoneCapture.AvailableMicrophones;
foreach (string mic in mics)
{
    microphoneDropdown.AddOption(mic);
}

// Set selected microphone
microphoneDropdown.onValueChanged.AddListener((index) => {
    microphoneCapture.SetMicrophone(mics[index]);
});

// Start recording
startButton.onClick.AddListener(microphoneCapture.StartRecording);
stopButton.onClick.AddListener(microphoneCapture.StopRecording);

// Listen for audio level changes to update UI
microphoneCapture.OnAudioLevelChanged += UpdateVolumeIndicator;

void UpdateVolumeIndicator(float level)
{
    volumeBar.value = level;
    volumeBar.color = level > 0.6f ? Color.red : (level > 0.2f ? Color.yellow : Color.green);
}
```

---

## SceneInitializer

`SceneInitializer` handles Unity scene initialization and setup.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsInitialized` | `bool` | Whether initialization is complete |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `InitializeScene()` | `void` | Initializes scene objects and references |
| `ResetScene()` | `void` | Resets scene to initial state |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnSceneInitialized` | `Action` | Fired when initialization completes |

### Usage Example

```csharp
// Wait for scene initialization before starting
sceneInitializer.OnSceneInitialized += StartInterviewProcess;

// Call initialization manually if needed
if (!sceneInitializer.IsInitialized)
{
    sceneInitializer.InitializeScene();
}

// Reset scene for new interview
resetButton.onClick.AddListener(() => {
    sceneInitializer.ResetScene();
    appManager.ResetInterview();
});

// Handle initialization completion
private void StartInterviewProcess()
{
    Debug.Log(\"Scene initialized, ready to begin interview\");
    uiManager.ShowMessage(\"Welcome to the interview system\");
}
```

---

## SessionManager

`SessionManager` manages the interview session state and server communication.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `SessionId` | `string` | Current session identifier |
| `CurrentState` | `string` | Current session state |
| `IsSessionActive` | `bool` | Whether session is active |
| `SessionMetadata` | `Dictionary<string, object>` | Session metadata dictionary |

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `StartSession()` | `Task` | Starts a new interview session |
| `EndSession()` | `Task` | Ends the current interview session |
| `Re`
{
  `path`: `D:\\VRSystemTest\\docs\\unity_class_reference_remaining.md`,
  `content`: `# VR Interview System - Remaining Class References

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
string serverUrl = settings.GetSetting<string>(\"ServerUrl\", \"ws://localhost:8765\");

// Update a setting
settings.SetSetting(\"AudioVolume\", 0.8f);

// Listen for settings changes
settings.OnSettingsChanged += HandleSettingsChanged;
settings.OnSettingChanged<float>(\"AudioVolume\", HandleVolumeChanged);

// Save settings to storage
settings.SaveSettings();

// Event handler for volume changes
private void HandleVolumeChanged(string key, float oldValue, float newValue)
{
    Debug.Log($\"Volume changed from {oldValue} to {newValue}\");
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
uiManager.UpdateConnectionStatus(\"Connected to server\");
uiManager.UpdateConnectionStatus(true); // Alternative boolean version

// Show progress for operation
uiManager.UpdateStatus(\"Processing your request...\");
uiManager.UpdateProgress(0.75f);

// Show conversation transcript
uiManager.ShowUserTranscript(\"Tell me about virtual reality\");
uiManager.ShowLLMResponse(\"Virtual reality is an immersive technology that...\");

// Show notifications
uiManager.ShowNotification(\"Settings saved successfully\");
uiManager.ShowError(\"Failed to connect to server\");
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
    Debug.LogError(\"Failed to send message\");
}

// Send audio data
await webSocketClient.SendAudioData(audioBytes, sessionId);

// Close connection
await webSocketClient.Close();

// Handle received messages
private void HandleMessage(string message)
{
    Debug.Log($\"Received message: {message.Substring(0, Math.Min(100, message.Length))}...\");
    messageHandler.ProcessMessage(message);
}
```
`
}