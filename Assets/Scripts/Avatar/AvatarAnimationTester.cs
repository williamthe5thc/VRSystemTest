using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Utility script for testing avatar animations, lip sync, and facial expressions in the editor.
/// Attach this to a GameObject in the scene with UI buttons to trigger different animation states.
/// </summary>
public class AvatarAnimationTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AvatarController avatarController;
    [SerializeField] private AudioSource testAudioSource;
    
    [Header("Test Audio")]
    [SerializeField] private AudioClip[] testAudioClips;
    
    [Header("UI Elements")]
    [SerializeField] private Button idleButton;
    [SerializeField] private Button listeningButton;
    [SerializeField] private Button thinkingButton;
    [SerializeField] private Button speakingButton;
    [SerializeField] private Button attentiveButton;
    [SerializeField] private Button confusedButton;
    [SerializeField] private Button playAudioButton;
    [SerializeField] private TMP_Dropdown audioClipDropdown;
    
    private int selectedAudioClip = 0;
    
    private void Start()
    {
        // Validate required components
        if (avatarController == null)
        {
            Debug.LogError("Avatar Controller reference is missing!");
            return;
        }
        
        if (testAudioSource == null)
        {
            Debug.LogWarning("Test Audio Source reference is missing. Creating one.");
            testAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Set up UI button listeners
        SetupButtons();
        
        // Set up audio dropdown
        SetupAudioDropdown();
    }
    
    private void SetupButtons()
    {
        if (idleButton != null)
            idleButton.onClick.AddListener(OnIdleButtonClicked);
        
        if (listeningButton != null)
            listeningButton.onClick.AddListener(OnListeningButtonClicked);
        
        if (thinkingButton != null)
            thinkingButton.onClick.AddListener(OnThinkingButtonClicked);
        
        if (speakingButton != null)
            speakingButton.onClick.AddListener(OnSpeakingButtonClicked);
        
        if (attentiveButton != null)
            attentiveButton.onClick.AddListener(OnAttentiveButtonClicked);
        
        if (confusedButton != null)
            confusedButton.onClick.AddListener(OnConfusedButtonClicked);
        
        if (playAudioButton != null)
            playAudioButton.onClick.AddListener(OnPlayAudioButtonClicked);
    }
    
    private void SetupAudioDropdown()
    {
        if (audioClipDropdown != null && testAudioClips != null && testAudioClips.Length > 0)
        {
            audioClipDropdown.ClearOptions();
            
            var options = new System.Collections.Generic.List<string>();
            for (int i = 0; i < testAudioClips.Length; i++)
            {
                if (testAudioClips[i] != null)
                    options.Add(testAudioClips[i].name);
                else
                    options.Add("Empty Clip " + i);
            }
            
            audioClipDropdown.AddOptions(options);
            audioClipDropdown.onValueChanged.AddListener(OnAudioClipSelected);
        }
    }
    
    private void OnAudioClipSelected(int index)
    {
        if (index >= 0 && index < testAudioClips.Length)
        {
            selectedAudioClip = index;
        }
    }
    
    // Button event handlers
    public void OnIdleButtonClicked()
    {
        if (avatarController != null)
        {
            avatarController.SetIdleState();
            Debug.Log("Set avatar to Idle state");
        }
    }
    
    public void OnListeningButtonClicked()
    {
        if (avatarController != null)
        {
            avatarController.SetListeningState();
            Debug.Log("Set avatar to Listening state");
        }
    }
    
    public void OnThinkingButtonClicked()
    {
        if (avatarController != null)
        {
            avatarController.SetThinkingState();
            Debug.Log("Set avatar to Thinking state");
        }
    }
    
    public void OnSpeakingButtonClicked()
    {
        if (avatarController != null)
        {
            avatarController.SetSpeakingState();
            Debug.Log("Set avatar to Speaking state");
        }
    }
    
    public void OnAttentiveButtonClicked()
    {
        if (avatarController != null)
        {
            avatarController.SetAttentiveState();
            Debug.Log("Set avatar to Attentive state");
        }
    }
    
    public void OnConfusedButtonClicked()
    {
        if (avatarController != null)
        {
            avatarController.SetConfusedState();
            Debug.Log("Set avatar to Confused state");
        }
    }
    
    public void OnPlayAudioButtonClicked()
    {
        if (testAudioSource != null && testAudioClips != null && selectedAudioClip < testAudioClips.Length)
        {
            if (testAudioClips[selectedAudioClip] != null)
            {
                // Stop any playing audio
                testAudioSource.Stop();
                
                // Set the clip and play
                testAudioSource.clip = testAudioClips[selectedAudioClip];
                testAudioSource.Play();
                
                // Set the avatar to speaking state
                if (avatarController != null)
                {
                    avatarController.SetSpeakingState();
                    avatarController.OnAudioPlaybackStarted();
                    
                    // Register for audio completion
                    Invoke("OnTestAudioComplete", testAudioSource.clip.length);
                }
                
                Debug.Log($"Playing audio clip: {testAudioClips[selectedAudioClip].name}");
            }
            else
            {
                Debug.LogWarning("Selected audio clip is null");
            }
        }
    }
    
    private void OnTestAudioComplete()
    {
        if (avatarController != null)
        {
            avatarController.OnAudioPlaybackCompleted();
            Debug.Log("Audio playback completed");
        }
    }
    
    private void OnDestroy()
    {
        // Clean up any event subscriptions
        if (audioClipDropdown != null)
            audioClipDropdown.onValueChanged.RemoveAllListeners();
    }
}