using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
using TMPro;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor tool for creating the main menu scene for the VR Interview System.
/// Adds menu items under "Tools > VR Interview System > Create Main Menu".
/// </summary>
public class MainMenuCreatorTool : EditorWindow
{
    private string menuName = "MainMenu";
    private bool createBackground = true;
    private bool createSettingsPanel = true;
    private bool createInfoPanel = true;
    private bool saveSceneAfterCreation = true;
    
    private Color backgroundColor = new Color(0.1f, 0.1f, 0.2f);
    private Color panelColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
    private Color buttonColor = new Color(0.3f, 0.5f, 0.8f);
    private Color titleColor = Color.white;
    private Vector3 menuPosition = new Vector3(0, 1.7f, 2.0f);
    
    // Paths
    private readonly string scenePath = "Assets/Scenes/";
    
    [MenuItem("Tools/VR Interview System/Create Main Menu", false, 10)]
    public static void ShowWindow()
    {
        GetWindow<MainMenuCreatorTool>("Main Menu Creator");
    }
    
    [MenuItem("Tools/VR Interview System/Quick Create/Main Menu", false, 10)]
    public static void QuickCreateMainMenu()
    {
        CreateMainMenuScene("MainMenu", true, true, true, true);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("VR Interview System - Main Menu Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        menuName = EditorGUILayout.TextField("Menu Scene Name:", menuName);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Menu Options", EditorStyles.boldLabel);
        
        createBackground = EditorGUILayout.Toggle("Create Background Environment", createBackground);
        createSettingsPanel = EditorGUILayout.Toggle("Create Settings Panel", createSettingsPanel);
        createInfoPanel = EditorGUILayout.Toggle("Create Info Panel", createInfoPanel);
        saveSceneAfterCreation = EditorGUILayout.Toggle("Save Scene After Creation", saveSceneAfterCreation);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);
        
        backgroundColor = EditorGUILayout.ColorField("Background Color:", backgroundColor);
        panelColor = EditorGUILayout.ColorField("Panel Color:", panelColor);
        buttonColor = EditorGUILayout.ColorField("Button Color:", buttonColor);
        titleColor = EditorGUILayout.ColorField("Title Color:", titleColor);
        menuPosition = EditorGUILayout.Vector3Field("Menu Position:", menuPosition);
        
        EditorGUILayout.Space();
        
        // Validate and show warnings
        if (string.IsNullOrEmpty(menuName))
        {
            EditorGUILayout.HelpBox("Menu name cannot be empty.", MessageType.Warning);
        }
        else if (menuName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            EditorGUILayout.HelpBox("Menu name contains invalid characters.", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        
        using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(menuName) || 
                                            menuName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
        {
            if (GUILayout.Button("Create Main Menu"))
            {
                CreateMainMenuScene(menuName, createBackground, createSettingsPanel, createInfoPanel, saveSceneAfterCreation);
            }
        }
    }
    
    private static void CreateMainMenuScene(string menuName, bool createBackground, bool createSettingsPanel, 
                                           bool createInfoPanel, bool saveScene)
    {
        // Create a new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Get instance to access non-static fields and methods
        MainMenuCreatorTool instance = CreateInstance<MainMenuCreatorTool>();
        
        // Set up the basic scene structure
        instance.SetupSceneHierarchy();
        
        // Create the main menu UI
        instance.CreateMainMenuUI(createSettingsPanel, createInfoPanel);
        
        // Create background if requested
        if (createBackground)
        {
            instance.CreateBackground();
        }
        
        // Save the scene
        if (saveScene)
        {
            // Create the directory if it doesn't exist
            if (!Directory.Exists(instance.scenePath))
            {
                Directory.CreateDirectory(instance.scenePath);
            }
            
            string scenePath = instance.scenePath + menuName + ".unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log("Main menu scene created and saved at: " + scenePath);
        }
        
        // Clean up
        DestroyImmediate(instance);
    }
    
    private void SetupSceneHierarchy()
    {
        // Create root objects for the scene
        GameObject sceneInitializer = new GameObject("SceneInitializer");
        GameObject appManager = new GameObject("AppManager");
        GameObject xrRig = new GameObject("XRRig");
        GameObject ui = new GameObject("UI");
        GameObject eventSystem = new GameObject("EventSystem");
        
        // Set up XRRig children
        GameObject cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.parent = xrRig.transform;
        
        GameObject mainCamera = new GameObject("Main Camera");
        mainCamera.AddComponent<Camera>();
        mainCamera.tag = "MainCamera";
        mainCamera.transform.parent = cameraOffset.transform;
        
        GameObject leftController = new GameObject("LeftHand Controller");
        GameObject rightController = new GameObject("RightHand Controller");
        leftController.transform.parent = xrRig.transform;
        rightController.transform.parent = xrRig.transform;
        
        // Add necessary components
        // SceneInitializer components
        if (TryAddComponentByTypeName(sceneInitializer, "SceneInitializer"))
        {
            Debug.Log("Added SceneInitializer component");
        }
        
        // AppManager components
        if (TryAddComponentByTypeName(appManager, "AppManager"))
        {
            Debug.Log("Added AppManager component");
        }
        if (TryAddComponentByTypeName(appManager, "SettingsManager"))
        {
            Debug.Log("Added SettingsManager component");
        }
        
        // XR Rig components
        if (TryAddComponentByTypeName(xrRig, "VRRigSetup"))
        {
            Debug.Log("Added VRRigSetup component");
        }
        
        // Setup XR components
        SetupXRComponents(xrRig, leftController, rightController, mainCamera);
        
        // Setup Event System
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
        
        // Try to add XR UI Input Module if available
        TryAddComponentByTypeName(eventSystem, "XRUIInputModule");
    }
    
    private void SetupXRComponents(GameObject xrRig, GameObject leftController, GameObject rightController, GameObject mainCamera)
    {
        // Try to add XR components
        // Try different namespaces since XR components might be in different places depending on versions
        
        // XR Origin / XR Rig
        bool addedXROrigin = TryAddComponentByTypeName(xrRig, "Unity.XR.CoreUtils.XROrigin") || 
                             TryAddComponentByTypeName(xrRig, "UnityEngine.XR.Interaction.Toolkit.XROrigin") ||
                             TryAddComponentByTypeName(xrRig, "UnityEngine.XR.Interaction.Toolkit.XRRig");
        
        if (addedXROrigin)
        {
            Debug.Log("Added XR Origin/Rig component");
        }
        else
        {
            Debug.LogWarning("Could not add XR Origin/Rig component. Make sure XR packages are installed.");
        }
        
        // Camera components
        TryAddComponentByTypeName(mainCamera, "UnityEngine.XR.Interaction.Toolkit.TrackedPoseDriver");
        
        // Controller components
        bool addedLeftController = TryAddComponentByTypeName(leftController, "UnityEngine.XR.Interaction.Toolkit.XRController") ||
                                  TryAddComponentByTypeName(leftController, "UnityEngine.XR.Interaction.Toolkit.ActionBasedController");
        
        bool addedRightController = TryAddComponentByTypeName(rightController, "UnityEngine.XR.Interaction.Toolkit.XRController") ||
                                   TryAddComponentByTypeName(rightController, "UnityEngine.XR.Interaction.Toolkit.ActionBasedController");
        
        if (addedLeftController && addedRightController)
        {
            Debug.Log("Added XR Controller components");
        }
        else
        {
            Debug.LogWarning("Could not add XR Controller components. Make sure XR packages are installed.");
        }
        
        // Interactors for UI interaction
        bool addedLeftRay = TryAddComponentByTypeName(leftController, "UnityEngine.XR.Interaction.Toolkit.XRRayInteractor") || 
                           TryAddComponentByTypeName(leftController, "UnityEngine.XR.Interaction.Toolkit.XRInteractionGroup");
        
        bool addedRightRay = TryAddComponentByTypeName(rightController, "UnityEngine.XR.Interaction.Toolkit.XRRayInteractor") || 
                            TryAddComponentByTypeName(rightController, "UnityEngine.XR.Interaction.Toolkit.XRInteractionGroup");
        
        if (addedLeftRay && addedRightRay)
        {
            Debug.Log("Added XR Ray Interactor components");
        }
        else
        {
            Debug.LogWarning("Could not add XR Ray Interactor components. Make sure XR packages are installed.");
            
            // Add custom VRUIInteractor as a fallback if needed
            TryAddComponentByTypeName(rightController, "VRUIInteractor");
        }
    }
    
    private void CreateMainMenuUI(bool createSettingsPanel, bool createInfoPanel)
    {
        // Find UI parent
        GameObject ui = GameObject.Find("UI");
        if (ui == null)
        {
            Debug.LogError("UI object not found in scene hierarchy!");
            return;
        }
        
        // Create main canvas
        GameObject mainMenuCanvas = new GameObject("MainMenuCanvas");
        mainMenuCanvas.transform.parent = ui.transform;
        
        // Add canvas components
        Canvas canvas = mainMenuCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler canvasScaler = mainMenuCanvas.AddComponent<CanvasScaler>();
        canvasScaler.dynamicPixelsPerUnit = 100;
        canvasScaler.referencePixelsPerUnit = 100;
        
        mainMenuCanvas.AddComponent<GraphicRaycaster>();
        
        // Position the canvas
        mainMenuCanvas.transform.position = menuPosition;
        mainMenuCanvas.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        mainMenuCanvas.transform.LookAt(new Vector3(0, menuPosition.y, 0));
        
        // Create panels
        GameObject mainMenuPanel = CreatePanel(mainMenuCanvas, "MainMenuPanel");
        GameObject settingsPanel = null;
        GameObject infoPanel = null;
        
        if (createSettingsPanel)
        {
            settingsPanel = CreatePanel(mainMenuCanvas, "SettingsPanel");
            settingsPanel.SetActive(false);
        }
        
        if (createInfoPanel)
        {
            infoPanel = CreatePanel(mainMenuCanvas, "InfoPanel");
            infoPanel.SetActive(false);
        }
        
        // Create main menu content
        PopulateMainMenuPanel(mainMenuPanel);
        
        if (createSettingsPanel)
        {
            PopulateSettingsPanel(settingsPanel);
        }
        
        if (createInfoPanel)
        {
            PopulateInfoPanel(infoPanel);
        }
        
        // Add MenuController script
        MenuController menuController = mainMenuCanvas.AddComponent<MenuController>();
        
        // Configure the MenuController references
        if (menuController != null)
        {
            // Find the prefab assets
            SerializedObject serializedController = new SerializedObject(menuController);
            
            // Set panel references
            SerializedProperty mainMenuPanelProperty = serializedController.FindProperty("mainMenuPanel");
            if (mainMenuPanelProperty != null)
                mainMenuPanelProperty.objectReferenceValue = mainMenuPanel;
            
            if (createSettingsPanel)
            {
                SerializedProperty settingsPanelProperty = serializedController.FindProperty("settingsPanel");
                if (settingsPanelProperty != null)
                    settingsPanelProperty.objectReferenceValue = settingsPanel;
            }
            
            if (createInfoPanel)
            {
                SerializedProperty infoPanelProperty = serializedController.FindProperty("infoPanel");
                if (infoPanelProperty != null)
                    infoPanelProperty.objectReferenceValue = infoPanel;
            }
            
            // Find buttons
            SerializedProperty startButtonProperty = serializedController.FindProperty("startButton");
            if (startButtonProperty != null)
                startButtonProperty.objectReferenceValue = GameObject.Find("UI/MainMenuCanvas/MainMenuPanel/StartButton")?.GetComponent<Button>();
            
            SerializedProperty settingsButtonProperty = serializedController.FindProperty("settingsButton");
            if (settingsButtonProperty != null)
                settingsButtonProperty.objectReferenceValue = GameObject.Find("UI/MainMenuCanvas/MainMenuPanel/SettingsButton")?.GetComponent<Button>();
            
            SerializedProperty infoButtonProperty = serializedController.FindProperty("infoButton");
            if (infoButtonProperty != null)
                infoButtonProperty.objectReferenceValue = GameObject.Find("UI/MainMenuCanvas/MainMenuPanel/InfoButton")?.GetComponent<Button>();
            
            SerializedProperty exitButtonProperty = serializedController.FindProperty("exitButton");
            if (exitButtonProperty != null)
                exitButtonProperty.objectReferenceValue = GameObject.Find("UI/MainMenuCanvas/MainMenuPanel/ExitButton")?.GetComponent<Button>();
            
            // Find dropdowns
            SerializedProperty environmentDropdownProperty = serializedController.FindProperty("environmentDropdown");
            if (environmentDropdownProperty != null)
                environmentDropdownProperty.objectReferenceValue = GameObject.Find("UI/MainMenuCanvas/MainMenuPanel/EnvironmentDropdown")?.GetComponent<TMP_Dropdown>();
            
            SerializedProperty avatarDropdownProperty = serializedController.FindProperty("avatarDropdown");
            if (avatarDropdownProperty != null)
                avatarDropdownProperty.objectReferenceValue = GameObject.Find("UI/MainMenuCanvas/MainMenuPanel/AvatarDropdown")?.GetComponent<TMP_Dropdown>();
            
            // Find settings controls
            if (createSettingsPanel)
            {
                SerializedProperty volumeSliderProperty = serializedController.FindProperty("volumeSlider");
                if (volumeSliderProperty != null)
                    volumeSliderProperty.objectReferenceValue = GameObject.Find("UI/MainMenuCanvas/SettingsPanel/VolumeSlider")?.GetComponent<Slider>();
                
                SerializedProperty micToggleProperty = serializedController.FindProperty("micToggle");
                if (micToggleProperty != null)
                    micToggleProperty.objectReferenceValue = GameObject.Find("UI/MainMenuCanvas/SettingsPanel/MicToggle")?.GetComponent<Toggle>();
                
                SerializedProperty settingsBackButtonProperty = serializedController.FindProperty("settingsBackButton");
                if (settingsBackButtonProperty != null)
                    settingsBackButtonProperty.objectReferenceValue = GameObject.Find("UI/MainMenuCanvas/SettingsPanel/BackButton")?.GetComponent<Button>();
                
                SerializedProperty serverUrlInputProperty = serializedController.FindProperty("serverUrlInput");
                if (serverUrlInputProperty != null)
                    serverUrlInputProperty.objectReferenceValue = GameObject.Find("UI/MainMenuCanvas/SettingsPanel/ServerUrlInput")?.GetComponent<TMP_InputField>();
            }
            
            // Find info back button
            if (createInfoPanel)
            {
                SerializedProperty infoBackButtonProperty = serializedController.FindProperty("infoBackButton");
                if (infoBackButtonProperty != null)
                    infoBackButtonProperty.objectReferenceValue = GameObject.Find("UI/MainMenuCanvas/InfoPanel/BackButton")?.GetComponent<Button>();
            }
            
            // Apply the changes
            serializedController.ApplyModifiedProperties();
        }
        
        // Create connection status canvas
        GameObject connectionStatusCanvas = new GameObject("ConnectionStatusCanvas");
        connectionStatusCanvas.transform.parent = ui.transform;
        
        Canvas connectionCanvas = connectionStatusCanvas.AddComponent<Canvas>();
        connectionCanvas.renderMode = RenderMode.WorldSpace;
        connectionStatusCanvas.AddComponent<CanvasScaler>();
        connectionStatusCanvas.AddComponent<GraphicRaycaster>();
        
        // Position the canvas
        connectionStatusCanvas.transform.position = new Vector3(0, 0.2f, 1.5f);
        connectionStatusCanvas.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        
        GameObject connectionStatusPanel = CreatePanel(connectionStatusCanvas, "ConnectionStatusPanel");
        
        // Add status text
        GameObject statusText = new GameObject("StatusText");
        statusText.transform.parent = connectionStatusPanel.transform;
        
        TextMeshProUGUI statusTMP = statusText.AddComponent<TextMeshProUGUI>();
        statusTMP.text = "Server Status: Not Connected";
        statusTMP.color = Color.white;
        statusTMP.fontSize = 24;
        statusTMP.alignment = TextAlignmentOptions.Center;
        
        RectTransform statusRect = statusText.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0);
        statusRect.anchorMax = new Vector2(1, 1);
        statusRect.offsetMin = new Vector2(10, 10);
        statusRect.offsetMax = new Vector2(-10, -10);
    }
    
    private GameObject CreatePanel(GameObject parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.parent = parent.transform;
        
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        Image image = panel.AddComponent<Image>();
        image.color = panelColor;
        
        return panel;
    }
    
    private void PopulateMainMenuPanel(GameObject panel)
    {
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        
        // Title
        GameObject titleObject = new GameObject("Title");
        titleObject.transform.parent = panel.transform;
        
        TextMeshProUGUI titleText = titleObject.AddComponent<TextMeshProUGUI>();
        titleText.text = "VR INTERVIEW PRACTICE SYSTEM";
        titleText.color = titleColor;
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 50);
        titleRect.anchoredPosition = new Vector2(0, -40);
        
        // Environment selection
        GameObject environmentLabel = CreateLabel(panel, "EnvironmentLabel", "Select Environment:", 24, new Vector2(0, -100));
        
        GameObject environmentDropdown = new GameObject("EnvironmentDropdown");
        environmentDropdown.transform.parent = panel.transform;
        
        TMP_Dropdown envDropdown = environmentDropdown.AddComponent<TMP_Dropdown>();
        envDropdown.options.Clear();
        envDropdown.options.Add(new TMP_Dropdown.OptionData("Corporate Office"));
        envDropdown.options.Add(new TMP_Dropdown.OptionData("Startup Office"));
        envDropdown.options.Add(new TMP_Dropdown.OptionData("Casual Office"));
        
        RectTransform envDropRect = environmentDropdown.GetComponent<RectTransform>();
        envDropRect.anchorMin = new Vector2(0.5f, 1);
        envDropRect.anchorMax = new Vector2(0.5f, 1);
        envDropRect.pivot = new Vector2(0.5f, 1);
        envDropRect.sizeDelta = new Vector2(300, 40);
        envDropRect.anchoredPosition = new Vector2(0, -140);
        
        // Avatar selection
        GameObject avatarLabel = CreateLabel(panel, "AvatarLabel", "Select Interviewer:", 24, new Vector2(0, -190));
        
        GameObject avatarDropdown = new GameObject("AvatarDropdown");
        avatarDropdown.transform.parent = panel.transform;
        
        TMP_Dropdown avDropdown = avatarDropdown.AddComponent<TMP_Dropdown>();
        avDropdown.options.Clear();
        avDropdown.options.Add(new TMP_Dropdown.OptionData("Professional Male"));
        avDropdown.options.Add(new TMP_Dropdown.OptionData("Professional Female"));
        avDropdown.options.Add(new TMP_Dropdown.OptionData("Casual Male"));
        avDropdown.options.Add(new TMP_Dropdown.OptionData("Casual Female"));
        
        RectTransform avDropRect = avatarDropdown.GetComponent<RectTransform>();
        avDropRect.anchorMin = new Vector2(0.5f, 1);
        avDropRect.anchorMax = new Vector2(0.5f, 1);
        avDropRect.pivot = new Vector2(0.5f, 1);
        avDropRect.sizeDelta = new Vector2(300, 40);
        avDropRect.anchoredPosition = new Vector2(0, -230);
        
        // Start button
        GameObject startButton = CreateButton(panel, "StartButton", "START INTERVIEW", buttonColor, new Vector2(0, -300));
        
        // Settings button
        GameObject settingsButton = CreateButton(panel, "SettingsButton", "SETTINGS", buttonColor, new Vector2(0, -370));
        
        // Info button
        GameObject infoButton = CreateButton(panel, "InfoButton", "INFORMATION", buttonColor, new Vector2(0, -440));
        
        // Exit button
        GameObject exitButton = CreateButton(panel, "ExitButton", "EXIT", new Color(0.8f, 0.2f, 0.2f), new Vector2(0, -510));
    }
    
    private void PopulateSettingsPanel(GameObject panel)
    {
        // Title
        GameObject titleObject = new GameObject("Title");
        titleObject.transform.parent = panel.transform;
        
        TextMeshProUGUI titleText = titleObject.AddComponent<TextMeshProUGUI>();
        titleText.text = "SETTINGS";
        titleText.color = titleColor;
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 50);
        titleRect.anchoredPosition = new Vector2(0, -40);
        
        // Volume slider
        GameObject volumeLabel = CreateLabel(panel, "VolumeLabel", "Volume:", 24, new Vector2(0, -100));
        
        GameObject volumeSlider = new GameObject("VolumeSlider");
        volumeSlider.transform.parent = panel.transform;
        
        Slider slider = volumeSlider.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0.8f;
        
        RectTransform sliderRect = volumeSlider.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 1);
        sliderRect.anchorMax = new Vector2(0.5f, 1);
        sliderRect.pivot = new Vector2(0.5f, 1);
        sliderRect.sizeDelta = new Vector2(300, 30);
        sliderRect.anchoredPosition = new Vector2(0, -140);
        
        // Add slider parts
        GameObject background = new GameObject("Background");
        background.transform.parent = volumeSlider.transform;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(1, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(0, 10);
        
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.parent = volumeSlider.transform;
        
        RectTransform fillRect = fillArea.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0.5f);
        fillRect.anchorMax = new Vector2(1, 0.5f);
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.sizeDelta = new Vector2(-20, 10);
        fillRect.offsetMin = new Vector2(10, 0);
        fillRect.offsetMax = new Vector2(-10, 0);
        
        GameObject fill = new GameObject("Fill");
        fill.transform.parent = fillArea.transform;
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = buttonColor;
        
        RectTransform fillImageRect = fill.GetComponent<RectTransform>();
        fillImageRect.anchorMin = new Vector2(0, 0);
        fillImageRect.anchorMax = new Vector2(0.8f, 1); // 0.8 represents the default value
        fillImageRect.sizeDelta = Vector2.zero;
        
        slider.fillRect = fillImageRect;
        
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.parent = volumeSlider.transform;
        
        RectTransform handleRect = handleArea.AddComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0, 0.5f);
        handleRect.anchorMax = new Vector2(1, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(-20, 0);
        handleRect.offsetMin = new Vector2(10, -10);
        handleRect.offsetMax = new Vector2(-10, 10);
        
        GameObject handle = new GameObject("Handle");
        handle.transform.parent = handleArea.transform;
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(1, 1, 1);
        
        RectTransform handleImageRect = handle.GetComponent<RectTransform>();
        handleImageRect.anchorMin = new Vector2(0.8f, 0); // 0.8 represents the default value
        handleImageRect.anchorMax = new Vector2(0.8f, 1);
        handleImageRect.pivot = new Vector2(0.5f, 0.5f);
        handleImageRect.sizeDelta = new Vector2(20, 0);
        
        slider.handleRect = handleImageRect;
        
        // Microphone toggle
        GameObject micLabel = CreateLabel(panel, "MicLabel", "Enable Microphone:", 24, new Vector2(0, -190));
        
        GameObject micToggle = new GameObject("MicToggle");
        micToggle.transform.parent = panel.transform;
        
        Toggle toggle = micToggle.AddComponent<Toggle>();
        toggle.isOn = true;
        
        RectTransform toggleRect = micToggle.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0.5f, 1);
        toggleRect.anchorMax = new Vector2(0.5f, 1);
        toggleRect.pivot = new Vector2(0.5f, 1);
        toggleRect.sizeDelta = new Vector2(40, 40);
        toggleRect.anchoredPosition = new Vector2(0, -230);
        
        // Toggle parts
        GameObject toggleBg = new GameObject("Background");
        toggleBg.transform.parent = micToggle.transform;
        
        Image toggleBgImage = toggleBg.AddComponent<Image>();
        toggleBgImage.color = new Color(0.2f, 0.2f, 0.2f);
        
        RectTransform toggleBgRect = toggleBg.GetComponent<RectTransform>();
        toggleBgRect.anchorMin = Vector2.zero;
        toggleBgRect.anchorMax = Vector2.one;
        toggleBgRect.sizeDelta = Vector2.zero;
        
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.parent = toggleBg.transform;
        
        Image checkmarkImage = checkmark.AddComponent<Image>();
        checkmarkImage.color = buttonColor;
        
        RectTransform checkmarkRect = checkmark.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0.1f, 0.1f);
        checkmarkRect.anchorMax = new Vector2(0.9f, 0.9f);
        checkmarkRect.sizeDelta = Vector2.zero;
        
        toggle.graphic = checkmarkImage;
        toggle.targetGraphic = toggleBgImage;
        
        // Server URL input
        GameObject urlLabel = CreateLabel(panel, "ServerUrlLabel", "Server URL:", 24, new Vector2(0, -280));
        
        GameObject serverUrlInput = new GameObject("ServerUrlInput");
        serverUrlInput.transform.parent = panel.transform;
        
        TMP_InputField inputField = serverUrlInput.AddComponent<TMP_InputField>();
        inputField.text = "ws://localhost:8765";
        
        RectTransform inputRect = serverUrlInput.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 1);
        inputRect.anchorMax = new Vector2(0.5f, 1);
        inputRect.pivot = new Vector2(0.5f, 1);
        inputRect.sizeDelta = new Vector2(300, 40);
        inputRect.anchoredPosition = new Vector2(0, -320);
        
        // Input field parts
        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.parent = serverUrlInput.transform;
        
        TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "Enter server URL...";
        placeholderText.color = new Color(1, 1, 1, 0.5f);
        placeholderText.fontSize = 18;
        
        RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
        placeholderRect.anchorMin = new Vector2(0, 0);
        placeholderRect.anchorMax = new Vector2(1, 1);
        placeholderRect.offsetMin = new Vector2(10, 0);
        placeholderRect.offsetMax = new Vector2(-10, 0);
        
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.parent = serverUrlInput.transform;
        
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = new Vector2(0, 0);
        textAreaRect.anchorMax = new Vector2(1, 1);
        textAreaRect.offsetMin = new Vector2(10, 0);
        textAreaRect.offsetMax = new Vector2(-10, 0);
        
        GameObject textComponent = new GameObject("Text");
        textComponent.transform.parent = textArea.transform;
        
        TextMeshProUGUI inputText = textComponent.AddComponent<TextMeshProUGUI>();
        inputText.text = "ws://localhost:8765";
        inputText.color = Color.white;
        inputText.fontSize = 18;
        
        RectTransform inputTextRect = textComponent.GetComponent<RectTransform>();
        inputTextRect.anchorMin = new Vector2(0, 0);
        inputTextRect.anchorMax = new Vector2(1, 1);
        inputTextRect.sizeDelta = Vector2.zero;
        
        inputField.textComponent = inputText;
        inputField.placeholder = placeholderText;
        
        // Add background image
        Image inputBg = serverUrlInput.AddComponent<Image>();
        inputBg.color = new Color(0.2f, 0.2f, 0.2f);
        
        // Back button
        GameObject backButton = CreateButton(panel, "BackButton", "BACK", buttonColor, new Vector2(0, -400));
    }
    
    private void PopulateInfoPanel(GameObject panel)
    {
        // Title
        GameObject titleObject = new GameObject("Title");
        titleObject.transform.parent = panel.transform;
        
        TextMeshProUGUI titleText = titleObject.AddComponent<TextMeshProUGUI>();
        titleText.text = "INFORMATION";
        titleText.color = titleColor;
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 50);
        titleRect.anchoredPosition = new Vector2(0, -40);
        
        // Info text
        GameObject infoTextObject = new GameObject("InfoText");
        infoTextObject.transform.parent = panel.transform;
        
        TextMeshProUGUI infoText = infoTextObject.AddComponent<TextMeshProUGUI>();
        infoText.text = "VR Interview Practice System\n\n" +
            "This application helps you practice for job interviews in a realistic virtual environment.\n\n" +
            "- Choose different office environments\n" +
            "- Practice with various interviewer personalities\n" +
            "- Receive real-time feedback\n" +
            "- Improve your interview skills in a safe, private setting\n\n" +
            "For best results, use a good microphone and speak clearly.\n\n" +
            "Version 1.0.0";
        
        infoText.color = Color.white;
        infoText.fontSize = 24;
        infoText.alignment = TextAlignmentOptions.Left;
        
        RectTransform infoTextRect = infoTextObject.GetComponent<RectTransform>();
        infoTextRect.anchorMin = new Vector2(0, 0);
        infoTextRect.anchorMax = new Vector2(1, 1);
        infoTextRect.pivot = new Vector2(0.5f, 1);
        infoTextRect.offsetMin = new Vector2(50, 100);
        infoTextRect.offsetMax = new Vector2(-50, -100);
        
        // Back button
        GameObject backButton = CreateButton(panel, "BackButton", "BACK", buttonColor, new Vector2(0, -450));
    }
    
    private GameObject CreateLabel(GameObject parent, string name, string text, int fontSize, Vector2 position)
    {
        GameObject label = new GameObject(name);
        label.transform.parent = parent.transform;
        
        TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
        labelText.text = text;
        labelText.color = Color.white;
        labelText.fontSize = fontSize;
        labelText.alignment = TextAlignmentOptions.Center;
        
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 1);
        labelRect.anchorMax = new Vector2(0.5f, 1);
        labelRect.pivot = new Vector2(0.5f, 1);
        labelRect.sizeDelta = new Vector2(300, 30);
        labelRect.anchoredPosition = position;
        
        return label;
    }
    
    private GameObject CreateButton(GameObject parent, string name, string text, Color color, Vector2 position)
    {
        GameObject button = new GameObject(name);
        button.transform.parent = parent.transform;
        
        Image buttonImage = button.AddComponent<Image>();
        buttonImage.color = color;
        
        Button buttonComponent = button.AddComponent<Button>();
        buttonComponent.targetGraphic = buttonImage;
        
        // Create a color block with slight darkening on pressed
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = color;
        colors.highlightedColor = new Color(color.r * 1.1f, color.g * 1.1f, color.b * 1.1f, color.a);
        colors.pressedColor = new Color(color.r * 0.9f, color.g * 0.9f, color.b * 0.9f, color.a);
        colors.selectedColor = color;
        buttonComponent.colors = colors;
        
        // Create button text
        GameObject textObject = new GameObject("Text");
        textObject.transform.parent = button.transform;
        
        TextMeshProUGUI buttonText = textObject.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.color = Color.white;
        buttonText.fontSize = 24;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        // Set button position and size
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 1);
        buttonRect.anchorMax = new Vector2(0.5f, 1);
        buttonRect.pivot = new Vector2(0.5f, 1);
        buttonRect.sizeDelta = new Vector2(250, 50);
        buttonRect.anchoredPosition = position;
        
        return button;
    }
    
    private void CreateBackground()
    {
        // Create a simple environment
        GameObject environment = new GameObject("Environment");
        
        // Create a skybox
        GameObject skybox = new GameObject("Skybox");
        skybox.transform.parent = environment.transform;
        
        // Create a floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.parent = environment.transform;
        floor.transform.position = new Vector3(0, 0, 0);
        floor.transform.localScale = new Vector3(10, 1, 10);
        
        Renderer floorRenderer = floor.GetComponent<Renderer>();
        if (floorRenderer != null)
        {
            floorRenderer.material = new Material(Shader.Find("Standard"));
            floorRenderer.material.color = new Color(0.3f, 0.3f, 0.3f);
        }
        
        // Create a directional light
        GameObject directionalLight = new GameObject("Directional Light");
        directionalLight.transform.parent = environment.transform;
        
        Light light = directionalLight.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.0f;
        light.color = Color.white;
        
        directionalLight.transform.rotation = Quaternion.Euler(50, -30, 0);
        
        // Create simple decorative elements
        // Create a desk
        GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desk.name = "MenuDesk";
        desk.transform.parent = environment.transform;
        desk.transform.position = new Vector3(0, 0.4f, 1.5f);
        desk.transform.localScale = new Vector3(1.5f, 0.05f, 0.7f);
        
        Renderer deskRenderer = desk.GetComponent<Renderer>();
        if (deskRenderer != null)
        {
            deskRenderer.material = new Material(Shader.Find("Standard"));
            deskRenderer.material.color = new Color(0.4f, 0.3f, 0.2f);
        }
        
        // Create desk legs
        for (int i = 0; i < 4; i++)
        {
            float xPos = ((i % 2) * 2 - 1) * 0.7f;
            float zPos = (i < 2 ? -1 : 1) * 0.3f;
            
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = $"DeskLeg_{i}";
            leg.transform.parent = desk.transform;
            leg.transform.localPosition = new Vector3(xPos, -8, zPos);
            leg.transform.localScale = new Vector3(0.1f, 8, 0.1f);
            
            Renderer legRenderer = leg.GetComponent<Renderer>();
            if (legRenderer != null)
            {
                legRenderer.material = new Material(Shader.Find("Standard"));
                legRenderer.material.color = new Color(0.3f, 0.2f, 0.1f);
            }
        }
        
        // Create a chair
        GameObject chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chair.name = "Chair";
        chair.transform.parent = environment.transform;
        chair.transform.position = new Vector3(0, 0.25f, 0.5f);
        chair.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
        
        Renderer chairRenderer = chair.GetComponent<Renderer>();
        if (chairRenderer != null)
        {
            chairRenderer.material = new Material(Shader.Find("Standard"));
            chairRenderer.material.color = new Color(0.2f, 0.2f, 0.2f);
        }
        
        // Add a back to the chair
        GameObject chairBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chairBack.name = "ChairBack";
        chairBack.transform.parent = chair.transform;
        chairBack.transform.localPosition = new Vector3(0, 8, -0.5f);
        chairBack.transform.localScale = new Vector3(1, 16, 0.2f);
        
        Renderer chairBackRenderer = chairBack.GetComponent<Renderer>();
        if (chairBackRenderer != null)
        {
            chairBackRenderer.material = new Material(Shader.Find("Standard"));
            chairBackRenderer.material.color = new Color(0.2f, 0.2f, 0.2f);
        }
        
        // Add walls with different colors
        Color[] wallColors = new Color[] {
            new Color(0.8f, 0.8f, 0.9f), // Light blue-gray
            new Color(0.9f, 0.85f, 0.8f), // Light peach
            new Color(0.85f, 0.9f, 0.85f) // Light green-gray
        };
        
        for (int i = 0; i < 3; i++)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"Wall_{i}";
            wall.transform.parent = environment.transform;
            
            float angle = i * 120f;
            float radians = angle * Mathf.Deg2Rad;
            float xPos = Mathf.Sin(radians) * 15;
            float zPos = Mathf.Cos(radians) * 15;
            
            wall.transform.position = new Vector3(xPos, 5, zPos);
            wall.transform.localScale = new Vector3(30, 10, 0.1f);
            wall.transform.LookAt(new Vector3(0, 5, 0));
            
            Renderer wallRenderer = wall.GetComponent<Renderer>();
            if (wallRenderer != null)
            {
                wallRenderer.material = new Material(Shader.Find("Standard"));
                wallRenderer.material.color = wallColors[i];
            }
        }
        
        // Add ambient lights
        for (int i = 0; i < 3; i++)
        {
            GameObject ambientLight = new GameObject($"AmbientLight_{i}");
            ambientLight.transform.parent = environment.transform;
            
            float angle = i * 120f;
            float radians = angle * Mathf.Deg2Rad;
            float xPos = Mathf.Sin(radians) * 8;
            float zPos = Mathf.Cos(radians) * 8;
            
            ambientLight.transform.position = new Vector3(xPos, 5, zPos);
            
            Light pointLight = ambientLight.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.intensity = 0.5f;
            pointLight.range = 15;
            pointLight.color = new Color(0.9f, 0.9f, 0.8f);
        }
        
        // Set the camera to default position
        GameObject mainCamera = GameObject.FindWithTag("MainCamera");
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(0, 1.6f, 0);
            mainCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        
        // Set the ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.1f, 0.1f, 0.2f);
        RenderSettings.ambientEquatorColor = new Color(0.1f, 0.1f, 0.15f);
        RenderSettings.ambientGroundColor = new Color(0.1f, 0.05f, 0.05f);
    }
    
    private bool TryAddComponentByTypeName(GameObject gameObject, string typeName)
    {
        // Try to find the type by name in all assemblies
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type == null)
            {
                // Try with the default namespace
                type = assembly.GetType("VRInterviewSystem." + typeName);
            }
            
            if (type != null && type.IsSubclassOf(typeof(Component)))
            {
                gameObject.AddComponent(type);
                return true;
            }
        }
        
        Debug.LogWarning($"Component type '{typeName}' not found. Make sure the script exists and is compiled.");
        return false;
    }
}