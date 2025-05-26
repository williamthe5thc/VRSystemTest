using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all audio components and ensures proper setup
/// Attach this to a GameObject in your scene
/// </summary>
public class AudioSystemManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioPlayback audioPlayback;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Test UI")]
    [SerializeField] private Button testAudioButton;
    [SerializeField] private Button fixAudioButton;
    [SerializeField] private Toggle spatialAudioToggle;
    [SerializeField] private Slider volumeSlider;
    
    [Header("Debug")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool createTestUIIfMissing = true;
    [SerializeField] private bool verbose = true;
    
    // Components
    private AudioTestGenerator testGenerator;
    
    private void Start()
    {
        // Find components if not assigned
        FindComponents();
        
        if (setupOnStart)
        {
            // Fix audio setup
            SetupAudioSystem();
            
            // Create test UI if enabled
            if (createTestUIIfMissing && testAudioButton == null)
            {
                CreateTestUI();
            }
        }
    }
    
    /// <summary>
    /// Finds all required components
    /// </summary>
    private void FindComponents()
    {
        // Find AudioPlayback
        if (audioPlayback == null)
        {
            audioPlayback = FindObjectOfType<AudioPlayback>();
            
            if (audioPlayback == null && verbose)
            {
                Debug.LogWarning("AudioPlayback not found");
            }
        }
        
        // Find or get AudioSource
        if (audioSource == null && audioPlayback != null)
        {
            audioSource = audioPlayback.GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = audioPlayback.gameObject.AddComponent<AudioSource>();
                if (verbose) Debug.Log("Added AudioSource to AudioPlayback");
            }
        }
        
        // Add AudioTestGenerator
        if (testGenerator == null)
        {
            testGenerator = gameObject.GetComponent<AudioTestGenerator>();
            
            if (testGenerator == null)
            {
                testGenerator = gameObject.AddComponent<AudioTestGenerator>();
                if (verbose) Debug.Log("Added AudioTestGenerator component");
            }
            
            if (audioSource != null)
            {
                testGenerator.targetAudioSource = audioSource;
            }
        }
    }
    
    /// <summary>
    /// Sets up the audio system components
    /// </summary>
    public void SetupAudioSystem()
    {
        if (audioPlayback == null || audioSource == null)
        {
            FindComponents();
            
            if (audioPlayback == null || audioSource == null)
            {
                Debug.LogError("Cannot setup audio system: AudioPlayback or AudioSource not found");
                return;
            }
        }
        
        // Configure AudioPlayback
        var audioPlaybackDebug = audioPlayback.GetComponent<AudioPlaybackDebug>();
        if (audioPlaybackDebug == null)
        {
            audioPlaybackDebug = audioPlayback.gameObject.AddComponent<AudioPlaybackDebug>();
            if (verbose) Debug.Log("Added AudioPlaybackDebug component");
        }
        
        // Set references for AudioPlaybackDebug
        audioPlaybackDebug.audioPlayback = audioPlayback;
        audioPlaybackDebug.audioSource = audioSource;
        
        var audioPlaybackFix = audioPlayback.GetComponent<AudioPlaybackFix>();
        if (audioPlaybackFix == null)
        {
            audioPlaybackFix = audioPlayback.gameObject.AddComponent<AudioPlaybackFix>();
            if (verbose) Debug.Log("Added AudioPlaybackFix component");
        }
        
        // Set references for AudioPlaybackFix
        audioPlaybackFix.audioPlayback = audioPlayback;
        audioPlaybackFix.audioSource = audioSource;
        
        // Configure AudioSource
        audioSource.spatialBlend = 0f; // 2D audio
        audioSource.volume = 1.0f;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.priority = 0; // High priority
        
        if (verbose) Debug.Log("Audio system setup complete");
        
        // Generate test audio
        testGenerator.GenerateTestTone();
    }
    
    /// <summary>
    /// Creates a simple UI for testing audio
    /// </summary>
    private void CreateTestUI()
    {
        // Check if Canvas exists
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Debug Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            if (verbose) Debug.Log("Created debug Canvas");
        }
        
        // Create panel
        GameObject panelObj = new GameObject("Audio Debug Panel");
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0.3f, 0.3f);
        panelRect.anchoredPosition = new Vector2(10, 10);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        // Create test audio button
        GameObject testButtonObj = new GameObject("Test Audio Button");
        testButtonObj.transform.SetParent(panelRect, false);
        RectTransform testButtonRect = testButtonObj.AddComponent<RectTransform>();
        testButtonRect.anchorMin = new Vector2(0.1f, 0.7f);
        testButtonRect.anchorMax = new Vector2(0.9f, 0.9f);
        testButtonRect.anchoredPosition = Vector2.zero;
        
        Image testButtonImage = testButtonObj.AddComponent<Image>();
        testButtonImage.color = new Color(0.2f, 0.5f, 0.9f);
        
        testAudioButton = testButtonObj.AddComponent<Button>();
        testAudioButton.onClick.AddListener(PlayTestAudio);
        
        GameObject testButtonTextObj = new GameObject("Text");
        testButtonTextObj.transform.SetParent(testButtonRect, false);
        RectTransform testButtonTextRect = testButtonTextObj.AddComponent<RectTransform>();
        testButtonTextRect.anchorMin = Vector2.zero;
        testButtonTextRect.anchorMax = Vector2.one;
        
        Text testButtonText = testButtonTextObj.AddComponent<Text>();
        testButtonText.text = "Play Test Audio";
        testButtonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        testButtonText.color = Color.white;
        testButtonText.alignment = TextAnchor.MiddleCenter;
        
        // Create fix audio button
        GameObject fixButtonObj = new GameObject("Fix Audio Button");
        fixButtonObj.transform.SetParent(panelRect, false);
        RectTransform fixButtonRect = fixButtonObj.AddComponent<RectTransform>();
        fixButtonRect.anchorMin = new Vector2(0.1f, 0.4f);
        fixButtonRect.anchorMax = new Vector2(0.9f, 0.6f);
        fixButtonRect.anchoredPosition = Vector2.zero;
        
        Image fixButtonImage = fixButtonObj.AddComponent<Image>();
        fixButtonImage.color = new Color(0.9f, 0.5f, 0.2f);
        
        fixAudioButton = fixButtonObj.AddComponent<Button>();
        fixAudioButton.onClick.AddListener(FixAudio);
        
        GameObject fixButtonTextObj = new GameObject("Text");
        fixButtonTextObj.transform.SetParent(fixButtonRect, false);
        RectTransform fixButtonTextRect = fixButtonTextObj.AddComponent<RectTransform>();
        fixButtonTextRect.anchorMin = Vector2.zero;
        fixButtonTextRect.anchorMax = Vector2.one;
        
        Text fixButtonText = fixButtonTextObj.AddComponent<Text>();
        fixButtonText.text = "Fix Audio System";
        fixButtonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        fixButtonText.color = Color.white;
        fixButtonText.alignment = TextAnchor.MiddleCenter;
        
        if (verbose) Debug.Log("Created audio debug UI");
    }
    
    /// <summary>
    /// Plays test audio through the AudioSource
    /// </summary>
    public void PlayTestAudio()
    {
        if (testGenerator != null)
        {
            testGenerator.PlayTestTone();
        }
        else
        {
            Debug.LogError("Cannot play test audio: AudioTestGenerator not found");
        }
    }
    
    /// <summary>
    /// Fixes audio system issues
    /// </summary>
    public void FixAudio()
    {
        if (audioPlayback == null || audioSource == null)
        {
            FindComponents();
        }
        
        if (audioPlayback != null && audioSource != null)
        {
            // Set up audio source manually
            audioSource.spatialBlend = 0f; // Set to 2D audio
            audioSource.volume = 1.0f;     // Set volume to maximum
            audioSource.playOnAwake = false; // Don't play automatically
            audioSource.loop = false;      // Don't loop
            audioSource.priority = 0;      // High priority
            
            var fix = audioPlayback.GetComponent<AudioPlaybackFix>();
            if (fix != null)
            {
                fix.fixPlaybackIssues = true;
                fix.forceNonSpatialAudio = true;
                fix.FixNow();
            }
            
            Debug.Log("Audio system fixed");
        }
        else
        {
            Debug.LogError("Cannot fix audio: AudioPlayback or AudioSource not found");
        }
    }
}