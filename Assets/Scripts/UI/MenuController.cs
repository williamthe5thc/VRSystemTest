using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject infoPanel;
    
    [Header("Main Menu Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button testSceneButton; // New button for direct test scene access
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button infoButton;
    [SerializeField] private Button exitButton;
    
    [Header("Environment Selection")]
    [SerializeField] private TMP_Dropdown environmentDropdown;
    [SerializeField] private List<string> environmentOptions = new List<string>()
    {
        "Corporate Office",
        "Startup Office",
        "Casual Office"
    };
    [SerializeField] private List<string> environmentScenes = new List<string>()
    {
        "Scenes/Environments/Corporate",
        "Scenes/Environments/Startup",
        "Scenes/Environments/Casual"
    };
    
    [Header("Avatar Selection")]
    [SerializeField] private TMP_Dropdown avatarDropdown;
    [SerializeField] private List<string> avatarOptions = new List<string>()
    {
        "Professional Male",
        "Professional Female",
        "Casual Male",
        "Casual Female"
    };
    
    [Header("Settings Panel")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle micToggle;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private TMP_InputField serverUrlInput;
    
    [Header("Info Panel")]
    [SerializeField] private Button infoBackButton;
    
    [Header("Dependencies")]
    [SerializeField] private SettingsManager settingsManager;
    [SerializeField] private SessionManager sessionManager;
    
    private string selectedEnvironment;
    private string selectedAvatar;
    
    private void Start()
    {
        // Initialize UI elements
        InitializeUI();
        
        // Register button listeners
        RegisterButtonListeners();
        
        // Load saved settings
        LoadSettings();
        
        // Show main menu initially
        ShowMainMenu();
    }
    
    private void InitializeUI()
    {
        // Set up environment dropdown
        if (environmentDropdown != null)
        {
            environmentDropdown.ClearOptions();
            environmentDropdown.AddOptions(environmentOptions);
            environmentDropdown.onValueChanged.AddListener(OnEnvironmentSelected);
            OnEnvironmentSelected(0); // Select first by default
        }
        
        // Set up avatar dropdown
        if (avatarDropdown != null)
        {
            avatarDropdown.ClearOptions();
            avatarDropdown.AddOptions(avatarOptions);
            avatarDropdown.onValueChanged.AddListener(OnAvatarSelected);
            OnAvatarSelected(0); // Select first by default
        }
        
        // Set up volume slider
        if (volumeSlider != null && settingsManager != null)
        {
            volumeSlider.onValueChanged.AddListener((float value) => {
                if (settingsManager != null) {
                    settingsManager.SetSetting("Volume", value);
                }
            });
        }
        
        // Set up microphone toggle
        if (micToggle != null && settingsManager != null)
        {
            micToggle.onValueChanged.AddListener((bool value) => {
                if (settingsManager != null) {
                    settingsManager.SetSetting("MicrophoneEnabled", value);
                }
            });
        }
        
        // Set up server URL input
        if (serverUrlInput != null && settingsManager != null)
        {
            serverUrlInput.onEndEdit.AddListener((string value) => {
                if (settingsManager != null) {
                    settingsManager.SetSetting("ServerUrl", value);
                }
            });
        }
    }
    
    private void RegisterButtonListeners()
    {
        // Main menu buttons
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClicked);
        }
        
        // Test scene button
        if (testSceneButton != null)
        {
            testSceneButton.onClick.AddListener(OnTestSceneClicked);
        }
        
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ShowSettings);
        }
        
        if (infoButton != null)
        {
            infoButton.onClick.AddListener(ShowInfo);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
        }
        
        // Settings back button
        if (settingsBackButton != null)
        {
            settingsBackButton.onClick.AddListener(ShowMainMenu);
        }
        
        // Info back button
        if (infoBackButton != null)
        {
            infoBackButton.onClick.AddListener(ShowMainMenu);
        }
    }
    
    private void LoadSettings()
    {
        if (settingsManager == null) return;
        
        // Load volume
        if (volumeSlider != null)
        {
            volumeSlider.value = settingsManager != null ? settingsManager.GetSetting<float>("Volume") : 0.8f;
        }
        
        // Load microphone state
        if (micToggle != null)
        {
            micToggle.isOn = settingsManager != null ? settingsManager.GetSetting<bool>("MicrophoneEnabled") : true;
        }
        
        // Load server URL
        if (serverUrlInput != null)
        {
            serverUrlInput.text = settingsManager != null ? settingsManager.GetSetting<string>("ServerUrl") : "ws://localhost:8765";
        }
        
        // Load last environment selection
        string lastEnvironment = settingsManager != null ? settingsManager.GetSetting<string>("LastEnvironment") : "";
        if (!string.IsNullOrEmpty(lastEnvironment) && environmentDropdown != null)
        {
            int index = environmentOptions.IndexOf(lastEnvironment);
            if (index >= 0)
            {
                environmentDropdown.value = index;
            }
        }
        
        // Load last avatar selection
        string lastAvatar = settingsManager != null ? settingsManager.GetSetting<string>("LastAvatar") : "";
        if (!string.IsNullOrEmpty(lastAvatar) && avatarDropdown != null)
        {
            int index = avatarOptions.IndexOf(lastAvatar);
            if (index >= 0)
            {
                avatarDropdown.value = index;
            }
        }
    }
    
    private void OnEnvironmentSelected(int index)
    {
        if (index >= 0 && index < environmentOptions.Count)
        {
            selectedEnvironment = environmentOptions[index];
            
            // Save selection
            if (settingsManager != null)
            {
                settingsManager.SetSetting("LastEnvironment", selectedEnvironment);
            }
        }
    }
    
    private void OnAvatarSelected(int index)
    {
        if (index >= 0 && index < avatarOptions.Count)
        {
            selectedAvatar = avatarOptions[index];
            
            // Save selection
            if (settingsManager != null)
            {
                settingsManager.SetSetting("LastAvatar", selectedAvatar);
            }
        }
    }
    
    private void OnStartClicked()
    {
        // Save current settings
        if (settingsManager != null)
        {
            settingsManager.SaveSettings();
        }
        
        // Load selected environment scene
        int environmentIndex = environmentOptions.IndexOf(selectedEnvironment);
        if (environmentIndex >= 0 && environmentIndex < environmentScenes.Count)
        {
            // Set avatar information for the next scene
            PlayerPrefs.SetString("SelectedAvatar", selectedAvatar);
            
            string sceneName = environmentScenes[environmentIndex];
            Debug.Log($"Attempting to load scene: {sceneName}");
            
            try {
                // Try to load the scene
                SceneManager.LoadScene(sceneName);
            }
            catch (System.Exception e) {
                // If that fails, fall back to the TestScene which we know exists
                Debug.LogError($"Failed to load scene {sceneName}: {e.Message}. Falling back to TestScene.");
                SceneManager.LoadScene("Scenes/TestScene");
            }
        }
        else
        {
            Debug.LogError($"Invalid environment index: {environmentIndex}");
            // Fall back to test scene
            SceneManager.LoadScene("Scenes/TestScene");
        }
    }
    
    private void OnExitClicked()
    {
        // Save settings before exiting
        if (settingsManager != null)
        {
            settingsManager.SaveSettings();
        }
        
        // Quit application (works in standalone build, not in editor)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void ShowMainMenu()
    {
        // Show main menu panel, hide others
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (infoPanel != null) infoPanel.SetActive(false);
    }
    
    public void ShowSettings()
    {
        // Show settings panel, hide others
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (infoPanel != null) infoPanel.SetActive(false);
    }
    
    public void ShowInfo()
    {
        // Show info panel, hide others
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (infoPanel != null) infoPanel.SetActive(true);
    }
    
    // Direct access to Test Scene
    private void OnTestSceneClicked()
    {
        // Save current settings
        if (settingsManager != null)
        {
            settingsManager.SaveSettings();
        }
        
        // Set avatar information for the next scene
        PlayerPrefs.SetString("SelectedAvatar", selectedAvatar);
        
        // Load the TestScene directly
        Debug.Log("Loading Test Scene directly");
        SceneManager.LoadScene("Scenes/TestScene");
    }
    
    // New methods for interview menu
    public void ShowInterviewMenu()
    {
        // Show the interview-specific menu
        Debug.Log("Showing interview menu");
        // Implementation depends on your UI structure
        // For now, just show the main menu
        ShowMainMenu();
    }
}