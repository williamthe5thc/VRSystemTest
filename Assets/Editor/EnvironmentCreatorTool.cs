using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Editor tool for creating different interview environments.
/// Adds menu items under "Tools > VR Interview System > Create Environment".
/// </summary>
public class EnvironmentCreatorTool : EditorWindow
{
    private enum EnvironmentType
    {
        Corporate,
        Startup,
        Casual
    }

    private EnvironmentType selectedEnvironment = EnvironmentType.Corporate;
    private string environmentName = "";
    private bool addBasicFurniture = true;
    private bool addInteractiveElements = true;
    private bool createLightingSetup = true;
    private bool setupAvatarPosition = true;
    private bool addDebugElements = true;
    private bool saveSceneAfterCreation = true;

    // Environment-specific settings
    private Color corporateWallColor = new Color(0.85f, 0.85f, 0.85f);
    private Color corporateFloorColor = new Color(0.3f, 0.3f, 0.3f);
    private Color corporateAccentColor = new Color(0.1f, 0.3f, 0.5f);

    private Color startupWallColor = new Color(0.95f, 0.95f, 0.95f);
    private Color startupFloorColor = new Color(0.7f, 0.7f, 0.7f);
    private Color startupAccentColor = new Color(0.2f, 0.6f, 0.8f);

    private Color casualWallColor = new Color(0.9f, 0.85f, 0.7f);
    private Color casualFloorColor = new Color(0.4f, 0.3f, 0.2f);
    private Color casualAccentColor = new Color(0.6f, 0.4f, 0.2f);

    // Prefab paths - you'll need to replace these with your actual prefab paths or create them
    private readonly string corporateDeskPath = "Assets/Prefabs/Environments/Corporate/Desk.prefab";
    private readonly string corporateChairPath = "Assets/Prefabs/Environments/Corporate/Chair.prefab";
    private readonly string startupDeskPath = "Assets/Prefabs/Environments/Startup/Desk.prefab";
    private readonly string startupChairPath = "Assets/Prefabs/Environments/Startup/Chair.prefab";
    private readonly string casualCouchPath = "Assets/Prefabs/Environments/Casual/Couch.prefab";
    private readonly string casualTablePath = "Assets/Prefabs/Environments/Casual/Table.prefab";

    // File paths
    private readonly string environmentsScenePath = "Assets/Scenes/Environments/";

    [MenuItem("Tools/VR Interview System/Create Environment", false, 100)]
    public static void ShowWindow()
    {
        GetWindow<EnvironmentCreatorTool>("Environment Creator");
    }

    [MenuItem("Tools/VR Interview System/Quick Create/Corporate Office", false, 101)]
    public static void QuickCreateCorporate()
    {
        CreateEnvironmentScene(EnvironmentType.Corporate, "CorporateOffice", true, true, true, true, false, true);
    }

    [MenuItem("Tools/VR Interview System/Quick Create/Startup Office", false, 102)]
    public static void QuickCreateStartup()
    {
        CreateEnvironmentScene(EnvironmentType.Startup, "StartupOffice", true, true, true, true, false, true);
    }

    [MenuItem("Tools/VR Interview System/Quick Create/Casual Office", false, 103)]
    public static void QuickCreateCasual()
    {
        CreateEnvironmentScene(EnvironmentType.Casual, "CasualOffice", true, true, true, true, false, true);
    }

    private void OnGUI()
    {
        GUILayout.Label("VR Interview Environment Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        selectedEnvironment = (EnvironmentType)EditorGUILayout.EnumPopup("Environment Type:", selectedEnvironment);
        
        // Set default name based on selection if empty
        if (string.IsNullOrEmpty(environmentName))
        {
            environmentName = selectedEnvironment.ToString() + "Office";
        }
        
        environmentName = EditorGUILayout.TextField("Environment Name:", environmentName);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Environment Options", EditorStyles.boldLabel);

        addBasicFurniture = EditorGUILayout.Toggle("Add Basic Furniture", addBasicFurniture);
        addInteractiveElements = EditorGUILayout.Toggle("Add Interactive Elements", addInteractiveElements);
        createLightingSetup = EditorGUILayout.Toggle("Create Lighting Setup", createLightingSetup);
        setupAvatarPosition = EditorGUILayout.Toggle("Setup Avatar Position", setupAvatarPosition);
        addDebugElements = EditorGUILayout.Toggle("Add Debug Elements", addDebugElements);
        saveSceneAfterCreation = EditorGUILayout.Toggle("Save Scene After Creation", saveSceneAfterCreation);

        EditorGUILayout.Space();

        // Show environment-specific settings based on selection
        switch (selectedEnvironment)
        {
            case EnvironmentType.Corporate:
                ShowCorporateSettings();
                break;
            case EnvironmentType.Startup:
                ShowStartupSettings();
                break;
            case EnvironmentType.Casual:
                ShowCasualSettings();
                break;
        }

        EditorGUILayout.Space();

        // Validate and show warnings
        if (string.IsNullOrEmpty(environmentName))
        {
            EditorGUILayout.HelpBox("Environment name cannot be empty.", MessageType.Warning);
        }
        else if (environmentName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            EditorGUILayout.HelpBox("Environment name contains invalid characters.", MessageType.Warning);
        }

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(environmentName) || 
                                           environmentName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
        {
            if (GUILayout.Button("Create Environment"))
            {
                CreateEnvironmentScene(selectedEnvironment, environmentName, addBasicFurniture, 
                                      addInteractiveElements, createLightingSetup, setupAvatarPosition, 
                                      addDebugElements, saveSceneAfterCreation);
            }
        }
    }

    private void ShowCorporateSettings()
    {
        EditorGUILayout.LabelField("Corporate Office Settings", EditorStyles.boldLabel);
        corporateWallColor = EditorGUILayout.ColorField("Wall Color", corporateWallColor);
        corporateFloorColor = EditorGUILayout.ColorField("Floor Color", corporateFloorColor);
        corporateAccentColor = EditorGUILayout.ColorField("Accent Color", corporateAccentColor);
    }

    private void ShowStartupSettings()
    {
        EditorGUILayout.LabelField("Startup Office Settings", EditorStyles.boldLabel);
        startupWallColor = EditorGUILayout.ColorField("Wall Color", startupWallColor);
        startupFloorColor = EditorGUILayout.ColorField("Floor Color", startupFloorColor);
        startupAccentColor = EditorGUILayout.ColorField("Accent Color", startupAccentColor);
    }

    private void ShowCasualSettings()
    {
        EditorGUILayout.LabelField("Casual Office Settings", EditorStyles.boldLabel);
        casualWallColor = EditorGUILayout.ColorField("Wall Color", casualWallColor);
        casualFloorColor = EditorGUILayout.ColorField("Floor Color", casualFloorColor);
        casualAccentColor = EditorGUILayout.ColorField("Accent Color", casualAccentColor);
    }

    private static void CreateEnvironmentScene(EnvironmentType environmentType, string environmentName, 
                                             bool addFurniture, bool addInteractive, bool setupLighting, 
                                             bool setupAvatar, bool addDebug, bool saveScene)
    {
        // Create a new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Get instance to access non-static fields and methods
        EnvironmentCreatorTool instance = CreateInstance<EnvironmentCreatorTool>();
        
        // Set up the basic scene structure
        instance.SetupSceneHierarchy(environmentType);
        
        // Add the environment elements
        instance.CreateEnvironment(environmentType, addFurniture, addInteractive);
        
        // Set up lighting
        if (setupLighting)
        {
            instance.SetupLighting(environmentType);
        }
        
        // Set up avatar position
        if (setupAvatar)
        {
            instance.SetupAvatarPosition(environmentType);
        }
        
        // Add debug elements
        if (addDebug)
        {
            instance.AddDebugElements();
        }
        
        // Save the scene
        if (saveScene)
        {
            // Create the directory if it doesn't exist
            if (!Directory.Exists(instance.environmentsScenePath))
            {
                Directory.CreateDirectory(instance.environmentsScenePath);
            }
            
            string scenePath = instance.environmentsScenePath + environmentName + ".unity";
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log("Environment scene created and saved at: " + scenePath);
        }
        
        // Clean up
        DestroyImmediate(instance);
    }

    private void SetupSceneHierarchy(EnvironmentType environmentType)
    {
        // Create root objects for the scene
        GameObject sceneInitializer = new GameObject("SceneInitializer");
        GameObject sessionManager = new GameObject("SessionManager");
        GameObject networkManager = new GameObject("NetworkManager");
        GameObject audioManager = new GameObject("AudioManager");
        GameObject xrRig = new GameObject("XRRig");
        GameObject environment = new GameObject("Environment");
        GameObject avatar = new GameObject("Avatar");
        GameObject ui = new GameObject("UI");
        
        // Set up NetworkManager children
        GameObject webSocketClient = new GameObject("WebSocketClient");
        GameObject messageHandler = new GameObject("MessageHandler");
        webSocketClient.transform.SetParent(networkManager.transform, false);
        messageHandler.transform.SetParent(networkManager.transform, false);
        
        // Set up AudioManager children
        GameObject microphoneCapture = new GameObject("MicrophoneCapture");
        GameObject audioPlayback = new GameObject("AudioPlayback");
        microphoneCapture.transform.SetParent(audioManager.transform, false);
        audioPlayback.transform.SetParent(audioManager.transform, false);
        
        // Set up XRRig children
        GameObject cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.SetParent(xrRig.transform, false);
        
        GameObject mainCamera = new GameObject("Main Camera");
        mainCamera.AddComponent<Camera>();
        mainCamera.tag = "MainCamera";
        mainCamera.transform.SetParent(cameraOffset.transform, false);
        
        GameObject leftController = new GameObject("LeftHand Controller");
        GameObject rightController = new GameObject("RightHand Controller");
        leftController.transform.SetParent(xrRig.transform, false);
        rightController.transform.SetParent(xrRig.transform, false);
        
        // Set up Environment children
        GameObject environmentManager = new GameObject("EnvironmentManager");
        GameObject office = new GameObject("Office");
        GameObject furniture = new GameObject("Furniture");
        GameObject props = new GameObject("Props");
        GameObject lightingControl = new GameObject("LightingControl");
        
        environmentManager.transform.SetParent(environment.transform, false);
        office.transform.SetParent(environment.transform, false);
        furniture.transform.SetParent(environment.transform, false);
        props.transform.SetParent(environment.transform, false);
        lightingControl.transform.SetParent(environment.transform, false);
        
        // Set up Avatar children
        GameObject avatarController = new GameObject("AvatarController");
        GameObject model = new GameObject("Model");
        GameObject lipSync = new GameObject("LipSync");
        GameObject facialExpressions = new GameObject("FacialExpressions");
        GameObject gestureSystem = new GameObject("GestureSystem");
        
        avatarController.transform.SetParent(avatar.transform, false);
        model.transform.SetParent(avatar.transform, false);
        lipSync.transform.SetParent(avatar.transform, false);
        facialExpressions.transform.SetParent(avatar.transform, false);
        gestureSystem.transform.SetParent(avatar.transform, false);
        
        // Set up UI children
        GameObject mainCanvas = new GameObject("MainCanvas");
        mainCanvas.AddComponent<Canvas>();
        mainCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        mainCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        GameObject vrMenuCanvas = new GameObject("VRMenuCanvas");
        vrMenuCanvas.AddComponent<Canvas>();
        vrMenuCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        vrMenuCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        GameObject connectionStatusCanvas = new GameObject("ConnectionStatusCanvas");
        connectionStatusCanvas.AddComponent<Canvas>();
        connectionStatusCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        connectionStatusCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        mainCanvas.transform.SetParent(ui.transform, false);
        vrMenuCanvas.transform.SetParent(ui.transform, false);
        connectionStatusCanvas.transform.SetParent(ui.transform, false);
        
        // Add necessary components based on the SCENE_SETUP.md information
        // Note: You may need to modify these if your component requirements are different
        
        // SceneInitializer components
        if (TryAddComponentByTypeName(sceneInitializer, "SceneInitializer"))
        {
            Debug.Log("Added SceneInitializer component");
        }
        
        // SessionManager components
        if (TryAddComponentByTypeName(sessionManager, "SessionManager"))
        {
            Debug.Log("Added SessionManager component");
        }
        
        // NetworkManager components
        if (TryAddComponentByTypeName(networkManager, "ConnectionManager"))
        {
            Debug.Log("Added ConnectionManager component");
        }
        if (TryAddComponentByTypeName(webSocketClient, "WebSocketClient"))
        {
            Debug.Log("Added WebSocketClient component");
        }
        if (TryAddComponentByTypeName(messageHandler, "MessageHandler"))
        {
            Debug.Log("Added MessageHandler component");
        }
        
        // AudioManager components
        if (TryAddComponentByTypeName(microphoneCapture, "MicrophoneCapture"))
        {
            Debug.Log("Added MicrophoneCapture component");
        }
        if (TryAddComponentByTypeName(audioPlayback, "AudioPlayback"))
        {
            Debug.Log("Added AudioPlayback component");
        }
        
        // XR Rig components
        if (TryAddComponentByTypeName(xrRig, "VRRigSetup"))
        {
            Debug.Log("Added VRRigSetup component");
        }
        
        // Environment components
        if (TryAddComponentByTypeName(environmentManager, "EnvironmentManager"))
        {
            Debug.Log("Added EnvironmentManager component");
        }
        if (TryAddComponentByTypeName(lightingControl, "LightingControl"))
        {
            Debug.Log("Added LightingControl component");
            TryAddComponentByTypeName(lightingControl, "DynamicLightingController");
        }
        
        // Avatar components
        if (TryAddComponentByTypeName(avatarController, "AvatarController"))
        {
            Debug.Log("Added AvatarController component");
        }
        if (TryAddComponentByTypeName(lipSync, "LipSync"))
        {
            Debug.Log("Added LipSync component");
        }
        if (TryAddComponentByTypeName(facialExpressions, "FacialExpressions"))
        {
            Debug.Log("Added FacialExpressions component");
        }
        if (TryAddComponentByTypeName(gestureSystem, "GestureSystem"))
        {
            Debug.Log("Added GestureSystem component");
        }
        
        // UI components
        if (TryAddComponentByTypeName(mainCanvas, "UIManager"))
        {
            Debug.Log("Added UIManager component");
        }
        if (TryAddComponentByTypeName(vrMenuCanvas, "VRInteractionUI"))
        {
            Debug.Log("Added VRInteractionUI component");
        }
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

    private void CreateEnvironment(EnvironmentType environmentType, bool addFurniture, bool addInteractive)
    {
        // Find the Environment GameObject
        GameObject environment = GameObject.Find("Environment");
        if (environment == null)
        {
            Debug.LogError("Environment object not found in scene hierarchy!");
            return;
        }
        
        GameObject office = environment.transform.Find("Office")?.gameObject;
        if (office == null)
        {
            Debug.LogError("Office object not found under Environment!");
            return;
        }
        
        // Create room dimensions based on environment type
        Vector3 roomSize = GetRoomSize(environmentType);
        
        // Create walls, floor, ceiling
        CreateRoom(office, roomSize, GetWallColor(environmentType), GetFloorColor(environmentType));
        
        // Add furniture if requested
        if (addFurniture)
        {
            GameObject furniture = environment.transform.Find("Furniture")?.gameObject;
            if (furniture != null)
            {
                AddFurniture(furniture, environmentType);
            }
        }
        
        // Add interactive elements if requested
        if (addInteractive)
        {
            GameObject props = environment.transform.Find("Props")?.gameObject;
            if (props != null)
            {
                AddInteractiveElements(props, environmentType);
            }
        }
        
        // Add environment-specific elements
        AddEnvironmentSpecificElements(environment, environmentType);
    }

    private Vector3 GetRoomSize(EnvironmentType environmentType)
    {
        // Define room sizes for different environment types
        switch (environmentType)
        {
            case EnvironmentType.Corporate:
                return new Vector3(10, 3, 8); // Larger, formal office space
                
            case EnvironmentType.Startup:
                return new Vector3(8, 2.8f, 6); // Medium-sized, open space
                
            case EnvironmentType.Casual:
                return new Vector3(6, 2.5f, 5); // Smaller, cozy space
                
            default:
                return new Vector3(8, 3, 6);
        }
    }

    private Color GetWallColor(EnvironmentType environmentType)
    {
        // Return wall color based on environment type and user preferences
        switch (environmentType)
        {
            case EnvironmentType.Corporate:
                return corporateWallColor;
                
            case EnvironmentType.Startup:
                return startupWallColor;
                
            case EnvironmentType.Casual:
                return casualWallColor;
                
            default:
                return Color.white;
        }
    }

    private Color GetFloorColor(EnvironmentType environmentType)
    {
        // Return floor color based on environment type and user preferences
        switch (environmentType)
        {
            case EnvironmentType.Corporate:
                return corporateFloorColor;
                
            case EnvironmentType.Startup:
                return startupFloorColor;
                
            case EnvironmentType.Casual:
                return casualFloorColor;
                
            default:
                return Color.gray;
        }
    }

    private Color GetAccentColor(EnvironmentType environmentType)
    {
        // Return accent color based on environment type and user preferences
        switch (environmentType)
        {
            case EnvironmentType.Corporate:
                return corporateAccentColor;
                
            case EnvironmentType.Startup:
                return startupAccentColor;
                
            case EnvironmentType.Casual:
                return casualAccentColor;
                
            default:
                return Color.blue;
        }
    }

    private void CreateRoom(GameObject parent, Vector3 size, Color wallColor, Color floorColor)
    {
        // Create floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(parent.transform, false);
        floor.transform.localPosition = new Vector3(0, -size.y/2, 0);
        floor.transform.localScale = new Vector3(size.x, 0.1f, size.z);
        
        Renderer floorRenderer = floor.GetComponent<Renderer>();
        if (floorRenderer != null)
        {
            Material floorMaterial = new Material(Shader.Find("Standard"));
            floorMaterial.color = floorColor;
            floorRenderer.sharedMaterial = floorMaterial;
        }
        
        // Create ceiling
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(parent.transform, false);
        ceiling.transform.localPosition = new Vector3(0, size.y/2, 0);
        ceiling.transform.localScale = new Vector3(size.x, 0.1f, size.z);
        
        Renderer ceilingRenderer = ceiling.GetComponent<Renderer>();
        if (ceilingRenderer != null)
        {
            Material ceilingMaterial = new Material(Shader.Find("Standard"));
            ceilingMaterial.color = Color.white;
            ceilingRenderer.sharedMaterial = ceilingMaterial;
        }
        
        // Create walls
        // Back wall
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "BackWall";
        backWall.transform.SetParent(parent.transform, false);
        backWall.transform.localPosition = new Vector3(0, 0, -size.z/2);
        backWall.transform.localScale = new Vector3(size.x, size.y, 0.1f);
        
        // Front wall
        GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWall.name = "FrontWall";
        frontWall.transform.SetParent(parent.transform, false);
        frontWall.transform.localPosition = new Vector3(0, 0, size.z/2);
        frontWall.transform.localScale = new Vector3(size.x, size.y, 0.1f);
        
        // Left wall
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "LeftWall";
        leftWall.transform.SetParent(parent.transform, false);
        leftWall.transform.localPosition = new Vector3(-size.x/2, 0, 0);
        leftWall.transform.localScale = new Vector3(0.1f, size.y, size.z);
        
        // Right wall
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "RightWall";
        rightWall.transform.SetParent(parent.transform, false);
        rightWall.transform.localPosition = new Vector3(size.x/2, 0, 0);
        rightWall.transform.localScale = new Vector3(0.1f, size.y, size.z);
        
        // Set wall materials
        Material wallMaterial = new Material(Shader.Find("Standard"));
        wallMaterial.color = wallColor;
        
        Renderer[] wallRenderers = new Renderer[] {
            backWall.GetComponent<Renderer>(),
            frontWall.GetComponent<Renderer>(),
            leftWall.GetComponent<Renderer>(),
            rightWall.GetComponent<Renderer>()
        };
        
        foreach (Renderer renderer in wallRenderers)
        {
            if (renderer != null)
            {
                renderer.sharedMaterial = wallMaterial;
            }
        }
        
        // Add a window to the back wall
        GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
        window.name = "Window";
        window.transform.SetParent(backWall.transform, false);
        window.transform.localPosition = new Vector3(0, 0.3f, -0.1f);
        window.transform.localScale = new Vector3(0.4f, 0.4f, 0.3f);
        
        Renderer windowRenderer = window.GetComponent<Renderer>();
        if (windowRenderer != null)
        {
            Material windowMaterial = new Material(Shader.Find("Standard"));
            windowMaterial.color = new Color(0.7f, 0.9f, 1.0f, 0.5f);
            windowMaterial.EnableKeyword("_ALPHABLEND_ON");
            windowMaterial.renderQueue = 3000;
            windowRenderer.sharedMaterial = windowMaterial;
        }
    }

    private void AddFurniture(GameObject parent, EnvironmentType environmentType)
    {
        // Add different furniture based on environment type
        switch (environmentType)
        {
            case EnvironmentType.Corporate:
                AddCorporateFurniture(parent);
                break;
                
            case EnvironmentType.Startup:
                AddStartupFurniture(parent);
                break;
                
            case EnvironmentType.Casual:
                AddCasualFurniture(parent);
                break;
        }
    }

    private void AddCorporateFurniture(GameObject parent)
    {
        // Try to load prefabs first
        GameObject deskPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(corporateDeskPath);
        GameObject chairPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(corporateChairPath);
        
        // If prefabs exist, instantiate them
        if (deskPrefab != null && chairPrefab != null)
        {
            GameObject desk = Instantiate(deskPrefab, parent.transform);
            desk.name = "ExecutiveDesk";
            desk.transform.localPosition = new Vector3(0, 0.0f, -2);
            
            GameObject chair1 = Instantiate(chairPrefab, parent.transform);
            chair1.name = "ExecutiveChair";
            chair1.transform.localPosition = new Vector3(0, 0.0f, -3);
            
            GameObject chair2 = Instantiate(chairPrefab, parent.transform);
            chair2.name = "GuestChair";
            chair2.transform.localPosition = new Vector3(0, 0.0f, -1);
            chair2.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            // Create simple primitive furniture if prefabs don't exist
            CreatePrimitiveDesk(parent, new Vector3(0, 0.0f, -2), new Vector3(1.5f, 0.05f, 0.8f), corporateAccentColor);
            CreatePrimitiveChair(parent, new Vector3(0, 0.0f, -3), corporateAccentColor);
            CreatePrimitiveChair(parent, new Vector3(0, 0.0f, -1), corporateAccentColor, true);
        }
        
        // Add bookshelf
        GameObject bookshelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bookshelf.name = "Bookshelf";
        bookshelf.transform.SetParent(parent.transform, false);
        bookshelf.transform.localPosition = new Vector3(-3, 0.5f, -2);
        bookshelf.transform.localScale = new Vector3(0.5f, 2, 1.5f);
        
        Renderer bookshelfRenderer = bookshelf.GetComponent<Renderer>();
        if (bookshelfRenderer != null)
        {
            Material bookshelfMaterial = new Material(Shader.Find("Standard"));
            bookshelfMaterial.color = new Color(0.3f, 0.2f, 0.1f);
            bookshelfRenderer.sharedMaterial = bookshelfMaterial;
        }
        
        // Add plant
        GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        plant.name = "Plant";
        plant.transform.SetParent(parent.transform, false);
        plant.transform.localPosition = new Vector3(3, 0.5f, -3);
        plant.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        
        Renderer plantRenderer = plant.GetComponent<Renderer>();
        if (plantRenderer != null)
        {
            Material plantMaterial = new Material(Shader.Find("Standard"));
            plantMaterial.color = new Color(0.1f, 0.5f, 0.1f);
            plantRenderer.sharedMaterial = plantMaterial;
        }
    }

    private void AddStartupFurniture(GameObject parent)
    {
        // Try to load prefabs first
        GameObject deskPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(startupDeskPath);
        GameObject chairPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(startupChairPath);
        
        // If prefabs exist, instantiate them
        if (deskPrefab != null && chairPrefab != null)
        {
            GameObject desk = Instantiate(deskPrefab, parent.transform);
            desk.name = "ModernDesk";
            desk.transform.localPosition = new Vector3(0, 0.0f, -2);
            
            GameObject chair1 = Instantiate(chairPrefab, parent.transform);
            chair1.name = "ModernChair1";
            chair1.transform.localPosition = new Vector3(-0.5f, 0.0f, -3);
            
            GameObject chair2 = Instantiate(chairPrefab, parent.transform);
            chair2.name = "ModernChair2";
            chair2.transform.localPosition = new Vector3(0.5f, 0.0f, -1);
            chair2.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            // Create simple primitive furniture if prefabs don't exist
            CreatePrimitiveDesk(parent, new Vector3(0, 0.0f, -2), new Vector3(1.2f, 0.03f, 0.6f), startupAccentColor);
            CreatePrimitiveChair(parent, new Vector3(-0.5f, 0.0f, -3), startupAccentColor);
            CreatePrimitiveChair(parent, new Vector3(0.5f, 0.0f, -1), startupAccentColor, true);
        }
        
        // Add whiteboard
        GameObject whiteboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        whiteboard.name = "Whiteboard";
        whiteboard.transform.SetParent(parent.transform, false);
        whiteboard.transform.localPosition = new Vector3(-2, 0, -1);
        whiteboard.transform.localScale = new Vector3(0.05f, 1.2f, 1.8f);
        whiteboard.transform.localRotation = Quaternion.Euler(0, 90, 0);
        
        Renderer whiteboardRenderer = whiteboard.GetComponent<Renderer>();
        if (whiteboardRenderer != null)
        {
            Material whiteboardMaterial = new Material(Shader.Find("Standard"));
            whiteboardMaterial.color = Color.white;
            whiteboardRenderer.sharedMaterial = whiteboardMaterial;
        }
        
        // Add beanbag
        GameObject beanbag = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        beanbag.name = "Beanbag";
        beanbag.transform.SetParent(parent.transform, false);
        beanbag.transform.localPosition = new Vector3(2, 0.25f, -1);
        beanbag.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);
        
        Renderer beanbagRenderer = beanbag.GetComponent<Renderer>();
        if (beanbagRenderer != null)
        {
            Material beanbagMaterial = new Material(Shader.Find("Standard"));
            beanbagMaterial.color = startupAccentColor;
            beanbagRenderer.sharedMaterial = beanbagMaterial;
        }
    }

    private void AddCasualFurniture(GameObject parent)
    {
        // Try to load prefabs first
        GameObject couchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(casualCouchPath);
        GameObject tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(casualTablePath);
        
        // If prefabs exist, instantiate them
        if (couchPrefab != null && tablePrefab != null)
        {
            GameObject couch = Instantiate(couchPrefab, parent.transform);
            couch.name = "Couch";
            couch.transform.localPosition = new Vector3(0, 0.25f, -2);
            
            GameObject table = Instantiate(tablePrefab, parent.transform);
            table.name = "CoffeeTable";
            table.transform.localPosition = new Vector3(0, 0.15f, -1);
        }
        else
        {
            // Create simple primitive furniture if prefabs don't exist
            // Couch
            GameObject couch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            couch.name = "Couch";
            couch.transform.SetParent(parent.transform, false);
            couch.transform.localPosition = new Vector3(0, 0.25f, -2);
            couch.transform.localScale = new Vector3(2, 0.5f, 0.7f);
            
            Renderer couchRenderer = couch.GetComponent<Renderer>();
            if (couchRenderer != null)
            {
                Material couchMaterial = new Material(Shader.Find("Standard"));
                couchMaterial.color = casualAccentColor;
                couchRenderer.sharedMaterial = couchMaterial;
            }
            
            // Couch Back
            GameObject couchBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            couchBack.name = "CouchBack";
            couchBack.transform.SetParent(couch.transform, false);
            couchBack.transform.localPosition = new Vector3(0, 0.5f, -0.4f);
            couchBack.transform.localScale = new Vector3(1, 0.8f, 0.1f);
            
            Renderer couchBackRenderer = couchBack.GetComponent<Renderer>();
            if (couchBackRenderer != null)
            {
                Material couchBackMaterial = new Material(Shader.Find("Standard"));
                couchBackMaterial.color = casualAccentColor;
                couchBackRenderer.sharedMaterial = couchBackMaterial;
            }
            
            // Coffee Table
            GameObject coffeeTable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coffeeTable.name = "CoffeeTable";
            coffeeTable.transform.SetParent(parent.transform, false);
            coffeeTable.transform.localPosition = new Vector3(0, 0.15f, -1);
            coffeeTable.transform.localScale = new Vector3(1, 0.1f, 0.5f);
            
            Renderer tableRenderer = coffeeTable.GetComponent<Renderer>();
            if (tableRenderer != null)
            {
                Material tableMaterial = new Material(Shader.Find("Standard"));
                tableMaterial.color = new Color(0.4f, 0.3f, 0.2f);
                tableRenderer.sharedMaterial = tableMaterial;
            }
            
            // Coffee Table Legs
            for (int i = 0; i < 4; i++)
            {
                float xPos = (i % 2 == 0) ? -0.4f : 0.4f;
                float zPos = (i < 2) ? -0.2f : 0.2f;
                
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = $"TableLeg_{i}";
                leg.transform.SetParent(coffeeTable.transform, false);
                leg.transform.localPosition = new Vector3(xPos, -1, zPos);
                leg.transform.localScale = new Vector3(0.05f, 1, 0.05f);
                
                Renderer legRenderer = leg.GetComponent<Renderer>();
                if (legRenderer != null)
                {
                    Material legMaterial = new Material(Shader.Find("Standard"));
                    legMaterial.color = new Color(0.3f, 0.2f, 0.1f);
                    legRenderer.sharedMaterial = legMaterial;
                }
            }
        }
        
        // Add a rug
        GameObject rug = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rug.name = "Rug";
        rug.transform.SetParent(parent.transform, false);
        rug.transform.localPosition = new Vector3(0, 0.05f, -1.5f);
        rug.transform.localScale = new Vector3(3, 0.02f, 2);
        
        Renderer rugRenderer = rug.GetComponent<Renderer>();
        if (rugRenderer != null)
        {
            Material rugMaterial = new Material(Shader.Find("Standard"));
            rugMaterial.color = new Color(0.8f, 0.7f, 0.6f);
            rugRenderer.sharedMaterial = rugMaterial;
        }
        
        // Add a lamp
        GameObject lampStand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lampStand.name = "LampStand";
        lampStand.transform.SetParent(parent.transform, false);
        lampStand.transform.localPosition = new Vector3(1.5f, 0.5f, -0.5f);
        lampStand.transform.localScale = new Vector3(0.05f, 1, 0.05f);
        
        Renderer lampStandRenderer = lampStand.GetComponent<Renderer>();
        if (lampStandRenderer != null)
        {
            Material lampStandMaterial = new Material(Shader.Find("Standard"));
            lampStandMaterial.color = new Color(0.3f, 0.3f, 0.3f);
            lampStandRenderer.sharedMaterial = lampStandMaterial;
        }
        
        GameObject lampShade = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lampShade.name = "LampShade";
        lampShade.transform.SetParent(lampStand.transform, false);
        lampShade.transform.localPosition = new Vector3(0, 1.2f, 0);
        lampShade.transform.localScale = new Vector3(5, 0.3f, 5);
        
        Renderer lampShadeRenderer = lampShade.GetComponent<Renderer>();
        if (lampShadeRenderer != null)
        {
            Material lampShadeMaterial = new Material(Shader.Find("Standard"));
            lampShadeMaterial.color = new Color(0.9f, 0.9f, 0.8f);
            lampShadeRenderer.sharedMaterial = lampShadeMaterial;
        }
        
        // Add a light to the lamp
        GameObject lampLight = new GameObject("LampLight");
        lampLight.transform.SetParent(lampShade.transform, false);
        lampLight.transform.localPosition = new Vector3(0, 0, 0);
        
        Light light = lampLight.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1.0f, 0.9f, 0.8f);
        light.intensity = 0.7f;
        light.range = 5;
    }

    private void CreatePrimitiveDesk(GameObject parent, Vector3 position, Vector3 scale, Color color)
    {
        // Create desk top
        GameObject desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desk.name = "Desk";
        desk.transform.SetParent(parent.transform, false);
        desk.transform.localPosition = new Vector3(position.x, position.y + scale.y/2, position.z);
        desk.transform.localScale = scale;
        
        Renderer deskRenderer = desk.GetComponent<Renderer>();
        if (deskRenderer != null)
        {
            Material deskMaterial = new Material(Shader.Find("Standard"));
            deskMaterial.color = color;
            deskRenderer.sharedMaterial = deskMaterial;
        }
        
        // Create desk legs
        float xOffset = scale.x * 0.4f;
        float zOffset = scale.z * 0.4f;
        float legHeight = 0.7f;
        
        for (int i = 0; i < 4; i++)
        {
            float xPos = (i % 2 == 0) ? -xOffset : xOffset;
            float zPos = (i < 2) ? -zOffset : zOffset;
            
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = $"DeskLeg_{i}";
            leg.transform.SetParent(desk.transform, false);
            leg.transform.localPosition = new Vector3(xPos, -(legHeight/2 + scale.y/2), zPos);
            leg.transform.localScale = new Vector3(0.05f, legHeight, 0.05f);
            
            Renderer legRenderer = leg.GetComponent<Renderer>();
            if (legRenderer != null)
            {
                Material legMaterial = new Material(Shader.Find("Standard"));
                legMaterial.color = color * 0.8f;
                legRenderer.sharedMaterial = legMaterial;
            }
        }
    }

    private void CreatePrimitiveChair(GameObject parent, Vector3 position, Color color, bool facingAway = false)
    {
        float seatHeight = 0.4f;
        
        // Create chair seat
        GameObject chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chair.name = "Chair";
        chair.transform.SetParent(parent.transform, false);
        chair.transform.localPosition = new Vector3(position.x, position.y + seatHeight, position.z);
        chair.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        
        if (facingAway)
        {
            chair.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        
        Renderer chairRenderer = chair.GetComponent<Renderer>();
        if (chairRenderer != null)
        {
            Material chairMaterial = new Material(Shader.Find("Standard"));
            chairMaterial.color = color;
            chairRenderer.sharedMaterial = chairMaterial;
        }
        
        // Create chair back
        GameObject chairBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chairBack.name = "ChairBack";
        chairBack.transform.SetParent(chair.transform, false);
        chairBack.transform.localPosition = new Vector3(0, 0.5f, -0.25f);
        chairBack.transform.localScale = new Vector3(0.5f, 1, 0.1f);
        
        Renderer chairBackRenderer = chairBack.GetComponent<Renderer>();
        if (chairBackRenderer != null)
        {
            Material chairBackMaterial = new Material(Shader.Find("Standard"));
            chairBackMaterial.color = color;
            chairBackRenderer.sharedMaterial = chairBackMaterial;
        }
        
        // Create chair legs
        float xOffset = 0.2f;
        float zOffset = 0.2f;
        float legHeight = 0.4f;
        
        for (int i = 0; i < 4; i++)
        {
            float xPos = (i % 2 == 0) ? -xOffset : xOffset;
            float zPos = (i < 2) ? -zOffset : zOffset;
            
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = $"ChairLeg_{i}";
            leg.transform.SetParent(chair.transform, false);
            leg.transform.localPosition = new Vector3(xPos, -(legHeight/2 + 0.05f), zPos);
            leg.transform.localScale = new Vector3(0.05f, legHeight, 0.05f);
            
            Renderer legRenderer = leg.GetComponent<Renderer>();
            if (legRenderer != null)
            {
                Material legMaterial = new Material(Shader.Find("Standard"));
                legMaterial.color = color * 0.8f;
                legRenderer.sharedMaterial = legMaterial;
            }
        }
    }

    private void AddInteractiveElements(GameObject parent, EnvironmentType environmentType)
    {
        // Add environment-specific interactive elements
        switch (environmentType)
        {
            case EnvironmentType.Corporate:
                AddCorporateInteractiveElements(parent);
                break;
                
            case EnvironmentType.Startup:
                AddStartupInteractiveElements(parent);
                break;
                
            case EnvironmentType.Casual:
                AddCasualInteractiveElements(parent);
                break;
        }
    }

    private void AddCorporateInteractiveElements(GameObject parent)
    {
        // Add a phone
        GameObject phone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        phone.name = "Phone";
        phone.transform.SetParent(parent.transform, false);
        phone.transform.localPosition = new Vector3(0.8f, 0.15f, -2);
        phone.transform.localScale = new Vector3(0.1f, 0.02f, 0.2f);
        
        Renderer phoneRenderer = phone.GetComponent<Renderer>();
        if (phoneRenderer != null)
        {
            Material phoneMaterial = new Material(Shader.Find("Standard"));
            phoneMaterial.color = Color.black;
            phoneRenderer.sharedMaterial = phoneMaterial;
        }
        
        // Add script if available
        TryAddComponentByTypeName(phone, "InteractableItem");
        
        // Add a pen holder with pens
        GameObject penHolder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        penHolder.name = "PenHolder";
        penHolder.transform.SetParent(parent.transform, false);
        penHolder.transform.localPosition = new Vector3(-0.8f, 0.2f, -2);
        penHolder.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
        
        Renderer penHolderRenderer = penHolder.GetComponent<Renderer>();
        if (penHolderRenderer != null)
        {
            Material penHolderMaterial = new Material(Shader.Find("Standard"));
            penHolderMaterial.color = new Color(0.1f, 0.1f, 0.1f);
            penHolderRenderer.sharedMaterial = penHolderMaterial;
        }
        
        // Add a calendar
        GameObject calendar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        calendar.name = "Calendar";
        calendar.transform.SetParent(parent.transform, false);
        calendar.transform.localPosition = new Vector3(0, 0.13f, -2.3f);
        calendar.transform.localScale = new Vector3(0.3f, 0.02f, 0.25f);
        calendar.transform.localRotation = Quaternion.Euler(0, 0, 10);
        
        Renderer calendarRenderer = calendar.GetComponent<Renderer>();
        if (calendarRenderer != null)
        {
            Material calendarMaterial = new Material(Shader.Find("Standard"));
            calendarMaterial.color = Color.white;
            calendarRenderer.sharedMaterial = calendarMaterial;
        }
        
        TryAddComponentByTypeName(calendar, "InteractableItem");
    }

    private void AddStartupInteractiveElements(GameObject parent)
    {
        // Add a laptop
        GameObject laptop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        laptop.name = "Laptop";
        laptop.transform.SetParent(parent.transform, false);
        laptop.transform.localPosition = new Vector3(0, 0.13f, -2);
        laptop.transform.localScale = new Vector3(0.4f, 0.02f, 0.3f);
        
        Renderer laptopRenderer = laptop.GetComponent<Renderer>();
        if (laptopRenderer != null)
        {
            Material laptopMaterial = new Material(Shader.Find("Standard"));
            laptopMaterial.color = new Color(0.2f, 0.2f, 0.2f);
            laptopRenderer.sharedMaterial = laptopMaterial;
        }
        
        GameObject laptopScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        laptopScreen.name = "LaptopScreen";
        laptopScreen.transform.SetParent(laptop.transform, false);
        laptopScreen.transform.localPosition = new Vector3(0, 2, -0.15f);
        laptopScreen.transform.localScale = new Vector3(1, 8, 0.1f);
        laptopScreen.transform.localRotation = Quaternion.Euler(-15, 0, 0);
        
        Renderer laptopScreenRenderer = laptopScreen.GetComponent<Renderer>();
        if (laptopScreenRenderer != null)
        {
            Material laptopScreenMaterial = new Material(Shader.Find("Standard"));
            laptopScreenMaterial.color = new Color(0.1f, 0.1f, 0.1f);
            laptopScreenRenderer.sharedMaterial = laptopScreenMaterial;
        }
        
        TryAddComponentByTypeName(laptop, "InteractableItem");
        
        // Add a coffee mug
        GameObject mug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mug.name = "CoffeeMug";
        mug.transform.SetParent(parent.transform, false);
        mug.transform.localPosition = new Vector3(0.5f, 0.2f, -2);
        mug.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);
        
        Renderer mugRenderer = mug.GetComponent<Renderer>();
        if (mugRenderer != null)
        {
            Material mugMaterial = new Material(Shader.Find("Standard"));
            mugMaterial.color = startupAccentColor;
            mugRenderer.sharedMaterial = mugMaterial;
        }
        
        TryAddComponentByTypeName(mug, "InteractableItem");
        
        // Add a stress ball
        GameObject stressBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stressBall.name = "StressBall";
        stressBall.transform.SetParent(parent.transform, false);
        stressBall.transform.localPosition = new Vector3(-0.5f, 0.15f, -2);
        stressBall.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
        
        Renderer stressBallRenderer = stressBall.GetComponent<Renderer>();
        if (stressBallRenderer != null)
        {
            Material stressBallMaterial = new Material(Shader.Find("Standard"));
            stressBallMaterial.color = new Color(0.9f, 0.2f, 0.2f);
            stressBallRenderer.sharedMaterial = stressBallMaterial;
        }
        
        TryAddComponentByTypeName(stressBall, "InteractableItem");
    }

    private void AddCasualInteractiveElements(GameObject parent)
    {
        // Add a book
        GameObject book = GameObject.CreatePrimitive(PrimitiveType.Cube);
        book.name = "Book";
        book.transform.SetParent(parent.transform, false);
        book.transform.localPosition = new Vector3(0.3f, 0.22f, -1);
        book.transform.localScale = new Vector3(0.2f, 0.03f, 0.15f);
        book.transform.localRotation = Quaternion.Euler(0, 15, 0);
        
        Renderer bookRenderer = book.GetComponent<Renderer>();
        if (bookRenderer != null)
        {
            Material bookMaterial = new Material(Shader.Find("Standard"));
            bookMaterial.color = new Color(0.2f, 0.3f, 0.8f);
            bookRenderer.sharedMaterial = bookMaterial;
        }
        
        TryAddComponentByTypeName(book, "InteractableItem");
        
        // Add a remote control
        GameObject remote = GameObject.CreatePrimitive(PrimitiveType.Cube);
        remote.name = "RemoteControl";
        remote.transform.SetParent(parent.transform, false);
        remote.transform.localPosition = new Vector3(-0.2f, 0.22f, -1);
        remote.transform.localScale = new Vector3(0.05f, 0.02f, 0.15f);
        remote.transform.localRotation = Quaternion.Euler(0, -30, 0);
        
        Renderer remoteRenderer = remote.GetComponent<Renderer>();
        if (remoteRenderer != null)
        {
            Material remoteMaterial = new Material(Shader.Find("Standard"));
            remoteMaterial.color = Color.black;
            remoteRenderer.sharedMaterial = remoteMaterial;
        }
        
        TryAddComponentByTypeName(remote, "InteractableItem");
        
        // Add a coffee cup
        GameObject cup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cup.name = "CoffeeCup";
        cup.transform.SetParent(parent.transform, false);
        cup.transform.localPosition = new Vector3(0, 0.22f, -0.8f);
        cup.transform.localScale = new Vector3(0.04f, 0.07f, 0.04f);
        
        Renderer cupRenderer = cup.GetComponent<Renderer>();
        if (cupRenderer != null)
        {
            Material cupMaterial = new Material(Shader.Find("Standard"));
            cupMaterial.color = Color.white;
            cupRenderer.sharedMaterial = cupMaterial;
        }
        
        TryAddComponentByTypeName(cup, "InteractableItem");
    }

    private void AddEnvironmentSpecificElements(GameObject parent, EnvironmentType environmentType)
    {
        // Add specific elements to complete the environment ambiance
        switch (environmentType)
        {
            case EnvironmentType.Corporate:
                // Add diplomas or certificates on the wall
                for (int i = 0; i < 3; i++)
                {
                    GameObject certificate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    certificate.name = $"Certificate_{i}";
                    certificate.transform.SetParent(parent.transform, false);
                    certificate.transform.localPosition = new Vector3(-2.5f + i * 0.8f, 0, -3.95f);
                    certificate.transform.localScale = new Vector3(0.5f, 0.7f, 0.02f);
                    
                    Renderer certificateRenderer = certificate.GetComponent<Renderer>();
                    if (certificateRenderer != null)
                    {
                        Material certificateMaterial = new Material(Shader.Find("Standard"));
                        certificateMaterial.color = Color.white;
                        certificateRenderer.sharedMaterial = certificateMaterial;
                    }
                }
                
                // Add a desk lamp
                GameObject deskLamp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                deskLamp.name = "DeskLamp";
                deskLamp.transform.SetParent(parent.transform, false);
                deskLamp.transform.localPosition = new Vector3(-0.5f, 0.3f, -2.3f);
                deskLamp.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
                
                GameObject lampHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lampHead.name = "LampHead";
                lampHead.transform.SetParent(deskLamp.transform, false);
                lampHead.transform.localPosition = new Vector3(0, 1, 0.5f);
                lampHead.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                
                Renderer lampHeadRenderer = lampHead.GetComponent<Renderer>();
                if (lampHeadRenderer != null)
                {
                    Material lampHeadMaterial = new Material(Shader.Find("Standard"));
                    lampHeadMaterial.color = new Color(0.8f, 0.8f, 0.7f);
                    lampHeadRenderer.sharedMaterial = lampHeadMaterial;
                }
                
                // Add a light to the lamp
                GameObject lampLight = new GameObject("LampLight");
                lampLight.transform.SetParent(lampHead.transform, false);
                
                Light light = lampLight.AddComponent<Light>();
                light.type = LightType.Spot;
                light.color = new Color(1.0f, 0.9f, 0.8f);
                light.intensity = 1.0f;
                light.range = 3;
                light.spotAngle = 60;
                break;
                
            case EnvironmentType.Startup:
                // Add a whiteboard with sketches
                GameObject whiteboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                whiteboard.name = "Whiteboard";
                whiteboard.transform.SetParent(parent.transform, false);
                whiteboard.transform.localPosition = new Vector3(-3.95f, 0, 0);
                whiteboard.transform.localScale = new Vector3(0.05f, 1.5f, 2);
                whiteboard.transform.localRotation = Quaternion.Euler(0, 90, 0);
                
                Renderer whiteboardRenderer = whiteboard.GetComponent<Renderer>();
                if (whiteboardRenderer != null)
                {
                    Material whiteboardMaterial = new Material(Shader.Find("Standard"));
                    whiteboardMaterial.color = Color.white;
                    whiteboardRenderer.sharedMaterial = whiteboardMaterial;
                }
                
                // Add sticky notes
                for (int i = 0; i < 5; i++)
                {
                    float xPos = -3.92f;
                    float yPos = -0.5f + i * 0.25f;
                    float zPos = Random.Range(-0.8f, 0.8f);
                    
                    GameObject stickyNote = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    stickyNote.name = $"StickyNote_{i}";
                    stickyNote.transform.SetParent(parent.transform, false);
                    stickyNote.transform.localPosition = new Vector3(xPos, yPos, zPos);
                    stickyNote.transform.localScale = new Vector3(0.02f, 0.1f, 0.1f);
                    stickyNote.transform.localRotation = Quaternion.Euler(Random.Range(-5f, 5f), 90, Random.Range(-5f, 5f));
                    
                    Renderer stickyNoteRenderer = stickyNote.GetComponent<Renderer>();
                    if (stickyNoteRenderer != null)
                    {
                        Material stickyNoteMaterial = new Material(Shader.Find("Standard"));
                        
                        // Random colors for sticky notes
                        Color[] stickyColors = new Color[] {
                            new Color(1.0f, 0.8f, 0.2f), // Yellow
                            new Color(0.2f, 0.8f, 1.0f), // Blue
                            new Color(1.0f, 0.5f, 0.5f), // Pink
                            new Color(0.5f, 1.0f, 0.5f), // Green
                            new Color(1.0f, 0.6f, 0.2f)  // Orange
                        };
                        
                        stickyNoteMaterial.color = stickyColors[i % stickyColors.Length];
                        stickyNoteRenderer.sharedMaterial = stickyNoteMaterial;
                    }
                    
                    TryAddComponentByTypeName(stickyNote, "InteractableItem");
                }
                break;
                
            case EnvironmentType.Casual:
                // Add a picture frame
                GameObject pictureFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pictureFrame.name = "PictureFrame";
                pictureFrame.transform.SetParent(parent.transform, false);
                pictureFrame.transform.localPosition = new Vector3(0, 0, -2.45f);
                pictureFrame.transform.localScale = new Vector3(0.8f, 0.6f, 0.05f);
                
                Renderer frameRenderer = pictureFrame.GetComponent<Renderer>();
                if (frameRenderer != null)
                {
                    Material frameMaterial = new Material(Shader.Find("Standard"));
                    frameMaterial.color = new Color(0.5f, 0.3f, 0.2f);
                    frameRenderer.sharedMaterial = frameMaterial;
                }
                
                GameObject picture = GameObject.CreatePrimitive(PrimitiveType.Cube);
                picture.name = "Picture";
                picture.transform.SetParent(pictureFrame.transform, false);
                picture.transform.localPosition = new Vector3(0, 0, -0.1f);
                picture.transform.localScale = new Vector3(0.9f, 0.9f, 0.2f);
                
                Renderer pictureRenderer = picture.GetComponent<Renderer>();
                if (pictureRenderer != null)
                {
                    Material pictureMaterial = new Material(Shader.Find("Standard"));
                    pictureMaterial.color = new Color(0.3f, 0.6f, 1.0f);
                    pictureRenderer.sharedMaterial = pictureMaterial;
                }
                
                // Add a rug
                GameObject rug = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rug.name = "Rug";
                rug.transform.SetParent(parent.transform, false);
                rug.transform.localPosition = new Vector3(0, 0.05f, -1.5f);
                rug.transform.localScale = new Vector3(3, 0.02f, 2);
                
                Renderer rugRenderer = rug.GetComponent<Renderer>();
                if (rugRenderer != null)
                {
                    Material rugMaterial = new Material(Shader.Find("Standard"));
                    rugMaterial.color = new Color(0.6f, 0.4f, 0.3f);
                    rugRenderer.sharedMaterial = rugMaterial;
                }
                
                // Add a plant
                GameObject plantPot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                plantPot.name = "PlantPot";
                plantPot.transform.SetParent(parent.transform, false);
                plantPot.transform.localPosition = new Vector3(2, 0.5f, 2);
                plantPot.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                
                Renderer potRenderer = plantPot.GetComponent<Renderer>();
                if (potRenderer != null)
                {
                    Material potMaterial = new Material(Shader.Find("Standard"));
                    potMaterial.color = new Color(0.6f, 0.4f, 0.3f);
                    potRenderer.sharedMaterial = potMaterial;
                }
                
                GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                plant.name = "Plant";
                plant.transform.SetParent(plantPot.transform, false);
                plant.transform.localPosition = new Vector3(0, 1.5f, 0);
                plant.transform.localScale = new Vector3(2, 1.5f, 2);
                
                Renderer plantRenderer = plant.GetComponent<Renderer>();
                if (plantRenderer != null)
                {
                    Material plantMaterial = new Material(Shader.Find("Standard"));
                    plantMaterial.color = new Color(0.2f, 0.5f, 0.2f);
                    plantRenderer.sharedMaterial = plantMaterial;
                }
                break;
        }
    }

    private void SetupLighting(EnvironmentType environmentType)
    {
        // Find the lighting control object
        GameObject lightingControl = GameObject.Find("Environment/LightingControl");
        
        if (lightingControl == null)
        {
            Debug.LogError("LightingControl object not found in scene hierarchy!");
            return;
        }
        
        // Create a main directional light
        GameObject mainLight = new GameObject("MainDirectionalLight");
        mainLight.transform.SetParent(lightingControl.transform, false);
        
        Light directionalLight = mainLight.AddComponent<Light>();
        directionalLight.type = LightType.Directional;
        
        // Set light parameters based on environment type
        switch (environmentType)
        {
            case EnvironmentType.Corporate:
                // More formal, bright white lighting
                directionalLight.color = new Color(1.0f, 0.98f, 0.95f);
                directionalLight.intensity = 0.8f;
                mainLight.transform.rotation = Quaternion.Euler(50, -30, 0);
                
                // Add ceiling lights
                AddCeilingLights(lightingControl, 2, 2, new Color(1.0f, 1.0f, 1.0f), 0.6f);
                break;
                
            case EnvironmentType.Startup:
                // Cooler, modern lighting
                directionalLight.color = new Color(0.9f, 0.95f, 1.0f);
                directionalLight.intensity = 0.7f;
                mainLight.transform.rotation = Quaternion.Euler(45, -40, 0);
                
                // Add ceiling lights with startup color
                AddCeilingLights(lightingControl, 2, 1, new Color(0.9f, 0.95f, 1.0f), 0.7f);
                break;
                
            case EnvironmentType.Casual:
                // Warmer, more cozy lighting
                directionalLight.color = new Color(1.0f, 0.95f, 0.8f);
                directionalLight.intensity = 0.6f;
                mainLight.transform.rotation = Quaternion.Euler(40, -45, 0);
                
                // Add a warm lamp light
                GameObject lampLight = new GameObject("WarmLampLight");
                lampLight.transform.SetParent(lightingControl.transform, false);
                lampLight.transform.position = new Vector3(2, 1, 1);
                
                Light lamp = lampLight.AddComponent<Light>();
                lamp.type = LightType.Point;
                lamp.color = new Color(1.0f, 0.9f, 0.7f);
                lamp.intensity = 0.8f;
                lamp.range = 5;
                break;
        }
        
        // Set up environment lighting parameters
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        
        switch (environmentType)
        {
            case EnvironmentType.Corporate:
                RenderSettings.ambientSkyColor = new Color(0.8f, 0.8f, 0.9f);
                RenderSettings.ambientEquatorColor = new Color(0.7f, 0.7f, 0.8f);
                RenderSettings.ambientGroundColor = new Color(0.5f, 0.5f, 0.6f);
                RenderSettings.ambientIntensity = 1.0f;
                break;
                
            case EnvironmentType.Startup:
                RenderSettings.ambientSkyColor = new Color(0.7f, 0.8f, 1.0f);
                RenderSettings.ambientEquatorColor = new Color(0.6f, 0.7f, 0.9f);
                RenderSettings.ambientGroundColor = new Color(0.5f, 0.6f, 0.7f);
                RenderSettings.ambientIntensity = 1.1f;
                break;
                
            case EnvironmentType.Casual:
                RenderSettings.ambientSkyColor = new Color(0.9f, 0.8f, 0.7f);
                RenderSettings.ambientEquatorColor = new Color(0.8f, 0.7f, 0.6f);
                RenderSettings.ambientGroundColor = new Color(0.6f, 0.55f, 0.5f);
                RenderSettings.ambientIntensity = 0.9f;
                break;
        }
        
        // Try to set up lighting presets using DynamicLightingController if available
        DynamicLightingController lightingController = lightingControl.GetComponent<DynamicLightingController>();
        if (lightingController != null)
        {
            Debug.Log("Found DynamicLightingController, configuring presets");
            
            // Configuration would be done through the Inspector
            // but we can't easily do that programmatically here
        }
    }

    private void AddCeilingLights(GameObject parent, int rows, int columns, Color color, float intensity)
    {
        for (int x = 0; x < columns; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                // Calculate positions to spread lights evenly
                float xPos = (x - (columns - 1) / 2.0f) * 3.0f;
                float zPos = (z - (rows - 1) / 2.0f) * 3.0f;
                
                GameObject ceilingLight = new GameObject($"CeilingLight_{x}_{z}");
                ceilingLight.transform.SetParent(parent.transform, false);
                ceilingLight.transform.position = new Vector3(xPos, 1.4f, zPos);
                
                Light light = ceilingLight.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = color;
                light.intensity = intensity;
                light.range = 4;
            }
        }
    }

    private void SetupAvatarPosition(EnvironmentType environmentType)
    {
        // Find the Avatar object
        GameObject avatar = GameObject.Find("Avatar");
        if (avatar == null)
        {
            Debug.LogError("Avatar object not found in scene hierarchy!");
            return;
        }
        
        // Position the avatar based on environment type
        switch (environmentType)
        {
            case EnvironmentType.Corporate:
                // Behind the desk
                avatar.transform.position = new Vector3(0, 0.0f, -3);
                avatar.transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
                
            case EnvironmentType.Startup:
                // More casual position
                avatar.transform.position = new Vector3(0, 0.0f, -2.5f);
                avatar.transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
                
            case EnvironmentType.Casual:
                // Sitting on the couch
                avatar.transform.position = new Vector3(0.5f, -0.3f, -2);
                avatar.transform.rotation = Quaternion.Euler(0, 160, 0);
                break;
        }
        
        // Add a placeholder model for the avatar
        GameObject model = avatar.transform.Find("Model")?.gameObject;
        if (model != null)
        {
            // Create a simple avatar placeholder
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(model.transform, false);
            head.transform.localPosition = new Vector3(0, 1.7f, 0);
            head.transform.localScale = new Vector3(0.2f, 0.25f, 0.2f);
            
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(model.transform, false);
            body.transform.localPosition = new Vector3(0, 1.0f, 0);
            body.transform.localScale = new Vector3(0.4f, 1.0f, 0.4f);
            
            Renderer headRenderer = head.GetComponent<Renderer>();
            if (headRenderer != null)
            {
                Material headMaterial = new Material(Shader.Find("Standard"));
                headMaterial.color = new Color(0.9f, 0.8f, 0.7f);
                headRenderer.sharedMaterial = headMaterial;
            }
            
            Renderer bodyRenderer = body.GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                Material bodyMaterial = new Material(Shader.Find("Standard"));
                
                // Different suit colors based on environment
                switch (environmentType)
                {
                    case EnvironmentType.Corporate:
                        bodyMaterial.color = new Color(0.2f, 0.2f, 0.3f); // Dark suit
                        break;
                    case EnvironmentType.Startup:
                        bodyMaterial.color = new Color(0.4f, 0.4f, 0.5f); // Business casual
                        break;
                    case EnvironmentType.Casual:
                        bodyMaterial.color = new Color(0.3f, 0.5f, 0.8f); // Casual blue
                        break;
                }
                
                bodyRenderer.sharedMaterial = bodyMaterial;
            }
        }
    }

    private void AddDebugElements()
    {
        // Create a debug panel for development
        GameObject debugPanel = new GameObject("DebugPanel");
        
        // Find UI parent
        GameObject ui = GameObject.Find("UI");
        if (ui != null)
        {
            debugPanel.transform.SetParent(ui.transform, false);
        }
        
        // Add a canvas for the debug panel
        Canvas canvas = debugPanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        debugPanel.AddComponent<UnityEngine.UI.CanvasScaler>();
        debugPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Position the debug panel
        debugPanel.transform.position = new Vector3(2, 1, 0);
        debugPanel.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        debugPanel.transform.rotation = Quaternion.Euler(0, -90, 0);
        
        // Add background panel
        GameObject background = new GameObject("Background");
        background.transform.SetParent(debugPanel.transform, false);
        
        UnityEngine.UI.Image backgroundImage = background.AddComponent<UnityEngine.UI.Image>();
        backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.offsetMin = new Vector2(0, 0);
        bgRect.offsetMax = new Vector2(0, 0);
        
        // Add title text
        GameObject titleObject = new GameObject("Title");
        titleObject.transform.SetParent(debugPanel.transform, false);
        
        // Try to add TextMeshProUGUI if available, otherwise use regular Text
        bool usedTMP = TryAddTMPText(titleObject, "DEBUG PANEL", 20, TextAnchor.MiddleCenter);
        
        if (!usedTMP)
        {
            UnityEngine.UI.Text titleText = titleObject.AddComponent<UnityEngine.UI.Text>();
            titleText.text = "DEBUG PANEL";
            titleText.fontSize = 20;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            
            // Try to find font
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.sizeDelta = new Vector2(0, 30);
        titleRect.anchoredPosition = new Vector2(0, 0);
        
        // Add a placeholder for debug info
        GameObject infoObject = new GameObject("DebugInfo");
        infoObject.transform.SetParent(debugPanel.transform, false);
        
        usedTMP = TryAddTMPText(infoObject, "State: IDLE\nConnected: No\nSession ID: None", 14, TextAnchor.UpperLeft);
        
        if (!usedTMP)
        {
            UnityEngine.UI.Text infoText = infoObject.AddComponent<UnityEngine.UI.Text>();
            infoText.text = "State: IDLE\nConnected: No\nSession ID: None";
            infoText.fontSize = 14;
            infoText.alignment = TextAnchor.UpperLeft;
            infoText.color = Color.white;
            
            // Try to find font
            infoText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        
        RectTransform infoRect = infoObject.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0);
        infoRect.anchorMax = new Vector2(1, 1);
        infoRect.pivot = new Vector2(0.5f, 1);
        infoRect.sizeDelta = new Vector2(-20, -40);
        infoRect.anchoredPosition = new Vector2(0, -30);
        
        // Try to add debug display script
        TryAddComponentByTypeName(debugPanel, "DebugDisplay");
    }

    private bool TryAddTMPText(GameObject gameObject, string text, int fontSize, TextAnchor alignment)
    {
        // Try to find TextMeshProUGUI type
        System.Type tmpType = null;
        
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            tmpType = assembly.GetType("TMPro.TextMeshProUGUI");
            if (tmpType != null) break;
        }
        
        if (tmpType != null && tmpType.IsSubclassOf(typeof(Component)))
        {
            Component tmpText = gameObject.AddComponent(tmpType);
            
            // Set text property using reflection
            var textProperty = tmpType.GetProperty("text");
            if (textProperty != null)
            {
                textProperty.SetValue(tmpText, text);
            }
            
            // Set fontSize property using reflection
            var fontSizeProperty = tmpType.GetProperty("fontSize");
            if (fontSizeProperty != null)
            {
                fontSizeProperty.SetValue(tmpText, (float)fontSize);
            }
            
            // Set alignment property using reflection
            var alignmentProperty = tmpType.GetProperty("alignment");
            if (alignmentProperty != null)
            {
                // TextMeshPro uses a different enum for alignment
                // We can't easily convert TextAnchor to TextAlignmentOptions, so we'll use a default value
                alignmentProperty.SetValue(tmpText, 257); // Center alignment
            }
            
            // Set color property using reflection
            var colorProperty = tmpType.GetProperty("color");
            if (colorProperty != null)
            {
                colorProperty.SetValue(tmpText, Color.white);
            }
            
            return true;
        }
        
        return false;
    }
}