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
        webSocketClient.transform.parent = networkManager.transform;
        messageHandler.transform.parent = networkManager.transform;
        
        // Set up AudioManager children
        GameObject microphoneCapture = new GameObject("MicrophoneCapture");
        GameObject audioPlayback = new GameObject("AudioPlayback");
        microphoneCapture.transform.parent = audioManager.transform;
        audioPlayback.transform.parent = audioManager.transform;
        
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
        
        // Set up Environment children
        GameObject environmentManager = new GameObject("EnvironmentManager");
        GameObject office = new GameObject("Office");
        GameObject furniture = new GameObject("Furniture");
        GameObject props = new GameObject("Props");
        GameObject lightingControl = new GameObject("LightingControl");
        
        environmentManager.transform.parent = environment.transform;
        office.transform.parent = environment.transform;
        furniture.transform.parent = environment.transform;
        props.transform.parent = environment.transform;
        lightingControl.transform.parent = environment.transform;
        
        // Set up Avatar children
        GameObject avatarController = new GameObject("AvatarController");
        GameObject model = new GameObject("Model");
        GameObject lipSync = new GameObject("LipSync");
        GameObject facialExpressions = new GameObject("FacialExpressions");
        GameObject gestureSystem = new GameObject("GestureSystem");
        
        avatarController.transform.parent = avatar.transform;
        model.transform.parent = avatar.transform;
        lipSync.transform.parent = avatar.transform;
        facialExpressions.transform.parent = avatar.transform;
        gestureSystem.transform.parent = avatar.transform;
        
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
        
        mainCanvas.transform.parent = ui.transform;
        vrMenuCanvas.transform.parent = ui.transform;
        connectionStatusCanvas.transform.parent = ui.transform;
        
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
        floor.transform.parent = parent.transform;
        floor.transform.localPosition = new Vector3(0, -size.y/2, 0);
        floor.transform.localScale = new Vector3(size.x, 0.1f, size.z);
        
        Renderer floorRenderer = floor.GetComponent<Renderer>();
        if (floorRenderer != null)
        {
            floorRenderer.material = new Material(Shader.Find("Standard"));
            floorRenderer.material.color = floorColor;
        }
        
        // Create ceiling
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.parent = parent.transform;
        ceiling.transform.localPosition = new Vector3(0, size.y/2, 0);
        ceiling.transform.localScale = new Vector3(size.x, 0.1f, size.z);
        
        Renderer ceilingRenderer = ceiling.GetComponent<Renderer>();
        if (ceilingRenderer != null)
        {
            ceilingRenderer.material = new Material(Shader.Find("Standard"));
            ceilingRenderer.material.color = Color.white;
        }
        
        // Create walls
        // Back wall
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "BackWall";
        backWall.transform.parent = parent.transform;
        backWall.transform.localPosition = new Vector3(0, 0, -size.z/2);
        backWall.transform.localScale = new Vector3(size.x, size.y, 0.1f);
        
        // Front wall
        GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWall.name = "FrontWall";
        frontWall.transform.parent = parent.transform;
        frontWall.transform.localPosition = new Vector3(0, 0, size.z/2);
        frontWall.transform.localScale = new Vector3(size.x, size.y, 0.1f);
        
        // Left wall
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "LeftWall";
        leftWall.transform.parent = parent.transform;
        leftWall.transform.localPosition = new Vector3(-size.x/2, 0, 0);
        leftWall.transform.localScale = new Vector3(0.1f, size.y, size.z);
        
        // Right wall
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "RightWall";
        rightWall.transform.parent = parent.transform;
        rightWall.transform.localPosition = new Vector3(size.x/2, 0, 0);
        rightWall.transform.localScale = new Vector3(0.1f, size.y, size.z);
        
        // Set wall materials
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
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = wallColor;
            }
        }
        
        // Add a window to the back wall
        GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
        window.name = "Window";
        window.transform.parent = backWall.transform;
        window.transform.localPosition = new Vector3(0, 0.3f, -0.1f);
        window.transform.localScale = new Vector3(0.4f, 0.4f, 0.3f);
        
        Renderer windowRenderer = window.GetComponent<Renderer>();
        if (windowRenderer != null)
        {
            windowRenderer.material = new Material(Shader.Find("Standard"));
            windowRenderer.material.color = new Color(0.7f, 0.9f, 1.0f, 0.5f);
            windowRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            windowRenderer.material.renderQueue = 3000;
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
            desk.transform.localPosition = new Vector3(0, -1.4f, -2);
            
            GameObject chair1 = Instantiate(chairPrefab, parent.transform);
            chair1.name = "ExecutiveChair";
            chair1.transform.localPosition = new Vector3(0, -1.4f, -3);
            
            GameObject chair2 = Instantiate(chairPrefab, parent.transform);
            chair2.name = "GuestChair";
            chair2.transform.localPosition = new Vector3(0, -1.4f, -1);
            chair2.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            // Create simple primitive furniture if prefabs don't exist
            CreatePrimitiveDesk(parent, new Vector3(0, -1.4f, -2), new Vector3(1.5f, 0.05f, 0.8f), corporateAccentColor);
            CreatePrimitiveChair(parent, new Vector3(0, -1.4f, -3), corporateAccentColor);
            CreatePrimitiveChair(parent, new Vector3(0, -1.4f, -1), corporateAccentColor, true);
        }
        
        // Add bookshelf
        GameObject bookshelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bookshelf.name = "Bookshelf";
        bookshelf.transform.parent = parent.transform;
        bookshelf.transform.localPosition = new Vector3(-3, -0.5f, -2);
        bookshelf.transform.localScale = new Vector3(0.5f, 2, 1.5f);
        
        Renderer bookshelfRenderer = bookshelf.GetComponent<Renderer>();
        if (bookshelfRenderer != null)
        {
            bookshelfRenderer.material = new Material(Shader.Find("Standard"));
            bookshelfRenderer.material.color = new Color(0.3f, 0.2f, 0.1f);
        }
        
        // Add plant
        GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        plant.name = "Plant";
        plant.transform.parent = parent.transform;
        plant.transform.localPosition = new Vector3(3, -1, -3);
        plant.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        
        Renderer plantRenderer = plant.GetComponent<Renderer>();
        if (plantRenderer != null)
        {
            plantRenderer.material = new Material(Shader.Find("Standard"));
            plantRenderer.material.color = new Color(0.1f, 0.5f, 0.1f);
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
            desk.transform.localPosition = new Vector3(0, -1.4f, -2);
            
            GameObject chair1 = Instantiate(chairPrefab, parent.transform);
            chair1.name = "ModernChair1";
            chair1.transform.localPosition = new Vector3(-0.5f, -1.4f, -3);
            
            GameObject chair2 = Instantiate(chairPrefab, parent.transform);
            chair2.name = "ModernChair2";
            chair2.transform.localPosition = new Vector3(0.5f, -1.4f, -1);
            chair2.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            // Create simple primitive furniture if prefabs don't exist
            CreatePrimitiveDesk(parent, new Vector3(0, -1.4f, -2), new Vector3(1.2f, 0.03f, 0.6f), startupAccentColor);
            CreatePrimitiveChair(parent, new Vector3(-0.5f, -1.4f, -3), startupAccentColor);
            CreatePrimitiveChair(parent, new Vector3(0.5f, -1.4f, -1), startupAccentColor, true);
        }
        
        // Add whiteboard
        GameObject whiteboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        whiteboard.name = "Whiteboard";
        whiteboard.transform.parent = parent.transform;
        whiteboard.transform.localPosition = new Vector3(-2, 0, -1);
        whiteboard.transform.localScale = new Vector3(0.05f, 1.2f, 1.8f);
        whiteboard.transform.localRotation = Quaternion.Euler(0, 90, 0);
        
        Renderer whiteboardRenderer = whiteboard.GetComponent<Renderer>();
        if (whiteboardRenderer != null)
        {
            whiteboardRenderer.material = new Material(Shader.Find("Standard"));
            whiteboardRenderer.material.color = Color.white;
        }
        
        // Add beanbag
        GameObject beanbag = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        beanbag.name = "Beanbag";
        beanbag.transform.parent = parent.transform;
        beanbag.transform.localPosition = new Vector3(2, -1.25f, -1);
        beanbag.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);
        
        Renderer beanbagRenderer = beanbag.GetComponent<Renderer>();
        if (beanbagRenderer != null)
        {
            beanbagRenderer.material = new Material(Shader.Find("Standard"));
            beanbagRenderer.material.color = startupAccentColor;
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
            couch.transform.localPosition = new Vector3(0, -1.25f, -2);
            
            GameObject table = Instantiate(tablePrefab, parent.transform);
            table.name = "CoffeeTable";
            table.transform.localPosition = new Vector3(0, -1.35f, -1);
        }
        else
        {
            // Create simple primitive furniture if prefabs don't exist
            // Couch
            GameObject couch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            couch.name = "Couch";
            couch.transform.parent = parent.transform;
            couch.transform.localPosition = new Vector3(0, -1.25f, -2);
            couch.transform.localScale = new Vector3(2, 0.5f, 0.7f);
            
            Renderer couchRenderer = couch.GetComponent<Renderer>();
            if (couchRenderer != null)
            {
                couchRenderer.material = new Material(Shader.Find("Standard"));
                couchRenderer.material.color = casualAccentColor;
            }
            
            // Couch Back
            GameObject couchBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            couchBack.name = "CouchBack";
            couchBack.transform.parent = couch.transform;
            couchBack.transform.localPosition = new Vector3(0, 0.5f, -0.4f);
            couchBack.transform.localScale = new Vector3(1, 0.8f, 0.1f);
            
            Renderer couchBackRenderer = couchBack.GetComponent<Renderer>();
            if (couchBackRenderer != null)
            {
                couchBackRenderer.material = new Material(Shader.Find("Standard"));
                couchBackRenderer.material.color = casualAccentColor;
            }
            
            // Coffee Table
            GameObject coffeeTable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coffeeTable.name = "CoffeeTable";
            coffeeTable.transform.parent = parent.transform;
            coffeeTable.transform.localPosition = new Vector3(0, -1.35f, -1);
            coffeeTable.transform.localScale = new Vector3(1, 0.1f, 0.5f);
            
            Renderer tableRenderer = coffeeTable.GetComponent<Renderer>();
            if (tableRenderer != null)
            {
                tableRenderer.material = new Material(Shader.Find("Standard"));
                tableRenderer.material.color = new Color(0.4f, 0.3f, 0.2f);
            }
            
            // Coffee Table Legs
            for (int i = 0; i < 4; i++)
            {
                float xPos = (i % 2 == 0) ? -0.4f : 0.4f;
                float zPos = (i < 2) ? -0.2f : 0.2f;
                
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = $"TableLeg_{i}";
                leg.transform.parent = coffeeTable.transform;
                leg.transform.localPosition = new Vector3(xPos, -1, zPos);
                leg.transform.localScale = new Vector3(0.05f, 1, 0.05f);
                
                Renderer legRenderer = leg.GetComponent<Renderer>();
                if (legRenderer != null)
                {
                    legRenderer.material = new Material(Shader.Find("Standard"));
                    legRenderer.material.color = new Color(0.3f, 0.2f, 0.1f);
                }
            }
        }
        
        // Add a rug
        GameObject rug = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rug.name = "Rug";
        rug.transform.parent = parent.transform;
        rug.transform.localPosition = new Vector3(0, -1.45f, -1.5f);
        rug.transform.localScale = new Vector3(3, 0.02f, 2);
        
        Renderer rugRenderer = rug.GetComponent<Renderer>();
        if (rugRenderer != null)
        {
            rugRenderer.material = new Material(Shader.Find("Standard"));
            rugRenderer.material.color = new Color(0.8f, 0.7f, 0.6f);
        }
        
        // Add a lamp
        GameObject lampStand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lampStand.name = "LampStand";
        lampStand.transform.parent = parent.transform;
        lampStand.transform.localPosition = new Vector3(1.5f, -1, -0.5f);
        lampStand.transform.localScale = new Vector3(0.05f, 1, 0.05f);
        
        Renderer lampStandRenderer = lampStand.GetComponent<Renderer>();
        if (lampStandRenderer != null)
        {
            lampStandRenderer.material = new Material(Shader.Find("Standard"));
            lampStandRenderer.material.color = new Color(0.3f, 0.3f, 0.3f);
        }
        
        GameObject lampShade = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lampShade.name = "LampShade";
        lampShade.transform.parent = lampStand.transform;
        lampShade.transform.localPosition = new Vector3(0, 1.2f, 0);
        lampShade.transform.localScale = new Vector3(5, 0.3f, 5);
        
        Renderer lampShadeRenderer = lampShade.GetComponent<Renderer>();
        if (lampShadeRenderer != null)
        {
            lampShadeRenderer.material = new Material(Shader.Find("Standard"));
            lampShadeRenderer.material.color = new Color(0.9f, 0.9f, 0.8f);
        }
        
        // Add a light to the lamp
        GameObject lampLight = new GameObject("LampLight");
        lampLight.transform.parent = lampShade.transform;
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
        desk.transform.parent = parent.transform;
        desk.transform.localPosition = position;
        desk.transform.localScale = scale;
        
        Renderer deskRenderer = desk.GetComponent<Renderer>();
        if (deskRenderer != null)
        {
            deskRenderer.material = new Material(Shader.Find("Standard"));
            deskRenderer.material.color = color;
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
            leg.transform.parent = desk.transform;
            leg.transform.localPosition = new Vector3(xPos, -legHeight, zPos);
            leg.transform.localScale = new Vector3(0.05f, legHeight, 0.05f);
            
            Renderer legRenderer = leg.GetComponent<Renderer>();
            if (legRenderer != null)
            {
                legRenderer.material = new Material(Shader.Find("Standard"));
                legRenderer.material.color = color * 0.8f;
            }
        }
    }

    private void CreatePrimitiveChair(GameObject parent, Vector3 position, Color color, bool facingAway = false)
    {
        // Create chair seat
        GameObject chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chair.name = "Chair";
        chair.transform.parent = parent.transform;
        chair.transform.localPosition = position;
        chair.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        
        if (facingAway)
        {
            chair.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        
        Renderer chairRenderer = chair.GetComponent<Renderer>();
        if (chairRenderer != null)
        {
            chairRenderer.material = new Material(Shader.Find("Standard"));
            chairRenderer.material.color = color;
        }
        
        // Create chair back
        GameObject chairBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chairBack.name = "ChairBack";
        chairBack.transform.parent = chair.transform;
        chairBack.transform.localPosition = new Vector3(0, 0.5f, -0.25f);
        chairBack.transform.localScale = new Vector3(0.5f, 1, 0.1f);
        
        Renderer chairBackRenderer = chairBack.GetComponent<Renderer>();
        if (chairBackRenderer != null)
        {
            chairBackRenderer.material = new Material(Shader.Find("Standard"));
            chairBackRenderer.material.color = color;
        }
        
        // Create chair legs
        float xOffset = 0.2f;
        float zOffset = 0.2f;
        float legHeight = 0.7f;
        
        for (int i = 0; i < 4; i++)
        {
            float xPos = (i % 2 == 0) ? -xOffset : xOffset;
            float zPos = (i < 2) ? -zOffset : zOffset;
            
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = $"ChairLeg_{i}";
            leg.transform.parent = chair.transform;
            leg.transform.localPosition = new Vector3(xPos, -legHeight/2, zPos);
            leg.transform.localScale = new Vector3(0.05f, legHeight, 0.05f);
            
            Renderer legRenderer = leg.GetComponent<Renderer>();
            if (legRenderer != null)
            {
                legRenderer.material = new Material(Shader.Find("Standard"));
                legRenderer.material.color = color * 0.8f;
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
        phone.transform.parent = parent.transform;
        phone.transform.localPosition = new Vector3(0.8f, -1.35f, -2);
        phone.transform.localScale = new Vector3(0.1f, 0.02f, 0.2f);
        
        Renderer phoneRenderer = phone.GetComponent<Renderer>();
        if (phoneRenderer != null)
        {
            phoneRenderer.material = new Material(Shader.Find("Standard"));
            phoneRenderer.material.color = Color.black;
        }
        
        // Add script if available
        TryAddComponentByTypeName(phone, "InteractableItem");
        
        // Add a pen holder with pens
        GameObject penHolder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        penHolder.name = "PenHolder";
        penHolder.transform.parent = parent.transform;
        penHolder.transform.localPosition = new Vector3(-0.8f, -1.3f, -2);
        penHolder.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
        
        Renderer penHolderRenderer = penHolder.GetComponent<Renderer>();
        if (penHolderRenderer != null)
        {
            penHolderRenderer.material = new Material(Shader.Find("Standard"));
            penHolderRenderer.material.color = new Color(0.1f, 0.1f, 0.1f);
        }
        
        // Add a calendar
        GameObject calendar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        calendar.name = "Calendar";
        calendar.transform.parent = parent.transform;
        calendar.transform.localPosition = new Vector3(0, -1.37f, -2.3f);
        calendar.transform.localScale = new Vector3(0.3f, 0.02f, 0.25f);
        calendar.transform.localRotation = Quaternion.Euler(0, 0, 10);
        
        Renderer calendarRenderer = calendar.GetComponent<Renderer>();
        if (calendarRenderer != null)
        {
            calendarRenderer.material = new Material(Shader.Find("Standard"));
            calendarRenderer.material.color = Color.white;
        }
        
        TryAddComponentByTypeName(calendar, "InteractableItem");
    }

    private void AddStartupInteractiveElements(GameObject parent)
    {
        // Add a laptop
        GameObject laptop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        laptop.name = "Laptop";
        laptop.transform.parent = parent.transform;
        laptop.transform.localPosition = new Vector3(0, -1.37f, -2);
        laptop.transform.localScale = new Vector3(0.4f, 0.02f, 0.3f);
        
        Renderer laptopRenderer = laptop.GetComponent<Renderer>();
        if (laptopRenderer != null)
        {
            laptopRenderer.material = new Material(Shader.Find("Standard"));
            laptopRenderer.material.color = new Color(0.2f, 0.2f, 0.2f);
        }
        
        GameObject laptopScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        laptopScreen.name = "LaptopScreen";
        laptopScreen.transform.parent = laptop.transform;
        laptopScreen.transform.localPosition = new Vector3(0, 2, -0.15f);
        laptopScreen.transform.localScale = new Vector3(1, 8, 0.1f);
        laptopScreen.transform.localRotation = Quaternion.Euler(-15, 0, 0);
        
        Renderer laptopScreenRenderer = laptopScreen.GetComponent<Renderer>();
        if (laptopScreenRenderer != null)
        {
            laptopScreenRenderer.material = new Material(Shader.Find("Standard"));
            laptopScreenRenderer.material.color = new Color(0.1f, 0.1f, 0.1f);
        }
        
        TryAddComponentByTypeName(laptop, "InteractableItem");
        
        // Add a coffee mug
        GameObject mug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mug.name = "CoffeeMug";
        mug.transform.parent = parent.transform;
        mug.transform.localPosition = new Vector3(0.5f, -1.3f, -2);
        mug.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);
        
        Renderer mugRenderer = mug.GetComponent<Renderer>();
        if (mugRenderer != null)
        {
            mugRenderer.material = new Material(Shader.Find("Standard"));
            mugRenderer.material.color = startupAccentColor;
        }
        
        TryAddComponentByTypeName(mug, "InteractableItem");
        
        // Add a stress ball
        GameObject stressBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stressBall.name = "StressBall";
        stressBall.transform.parent = parent.transform;
        stressBall.transform.localPosition = new Vector3(-0.5f, -1.35f, -2);
        stressBall.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
        
        Renderer stressBallRenderer = stressBall.GetComponent<Renderer>();
        if (stressBallRenderer != null)
        {
            stressBallRenderer.material = new Material(Shader.Find("Standard"));
            stressBallRenderer.material.color = new Color(0.9f, 0.2f, 0.2f);
        }
        
        TryAddComponentByTypeName(stressBall, "InteractableItem");
    }

    private void AddCasualInteractiveElements(GameObject parent)
    {
        // Add a book
        GameObject book = GameObject.CreatePrimitive(PrimitiveType.Cube);
        book.name = "Book";
        book.transform.parent = parent.transform;
        book.transform.localPosition = new Vector3(0.3f, -1.28f, -1);
        book.transform.localScale = new Vector3(0.2f, 0.03f, 0.15f);
        book.transform.localRotation = Quaternion.Euler(0, 15, 0);
        
        Renderer bookRenderer = book.GetComponent<Renderer>();
        if (bookRenderer != null)
        {
            bookRenderer.material = new Material(Shader.Find("Standard"));
            bookRenderer.material.color = new Color(0.2f, 0.3f, 0.8f);
        }
        
        TryAddComponentByTypeName(book, "InteractableItem");
        
        // Add a remote control
        GameObject remote = GameObject.CreatePrimitive(PrimitiveType.Cube);
        remote.name = "RemoteControl";
        remote.transform.parent = parent.transform;
        remote.transform.localPosition = new Vector3(-0.2f, -1.28f, -1);
        remote.transform.localScale = new Vector3(0.05f, 0.02f, 0.15f);
        remote.transform.localRotation = Quaternion.Euler(0, -30, 0);
        
        Renderer remoteRenderer = remote.GetComponent<Renderer>();
        if (remoteRenderer != null)
        {
            remoteRenderer.material = new Material(Shader.Find("Standard"));
            remoteRenderer.material.color = Color.black;
        }
        
        TryAddComponentByTypeName(remote, "InteractableItem");
        
        // Add a coffee cup
        GameObject cup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cup.name = "CoffeeCup";
        cup.transform.parent = parent.transform;
        cup.transform.localPosition = new Vector3(0, -1.28f, -0.8f);
        cup.transform.localScale = new Vector3(0.04f, 0.07f, 0.04f);
        
        Renderer cupRenderer = cup.GetComponent<Renderer>();
        if (cupRenderer != null)
        {
            cupRenderer.material = new Material(Shader.Find("Standard"));
            cupRenderer.material.color = Color.white;
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
                    certificate.transform.parent = parent.transform;
                    certificate.transform.localPosition = new Vector3(-2.5f + i * 0.8f, 0, -3.95f);
                    certificate.transform.localScale = new Vector3(0.5f, 0.7f, 0.02f);
                    
                    Renderer certificateRenderer = certificate.GetComponent<Renderer>();
                    if (certificateRenderer != null)
                    {
                        certificateRenderer.material = new Material(Shader.Find("Standard"));
                        certificateRenderer.material.color = Color.white;
                    }
                }
                
                // Add a desk lamp
                GameObject deskLamp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                deskLamp.name = "DeskLamp";
                deskLamp.transform.parent = parent.transform;
                deskLamp.transform.localPosition = new Vector3(-0.5f, -1.2f, -2.3f);
                deskLamp.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
                
                GameObject lampHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lampHead.name = "LampHead";
                lampHead.transform.parent = deskLamp.transform;
                lampHead.transform.localPosition = new Vector3(0, 1, 0.5f);
                lampHead.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                
                Renderer lampHeadRenderer = lampHead.GetComponent<Renderer>();
                if (lampHeadRenderer != null)
                {
                    lampHeadRenderer.material = new Material(Shader.Find("Standard"));
                    lampHeadRenderer.material.color = new Color(0.8f, 0.8f, 0.7f);
                }
                
                // Add a light to the lamp
                GameObject lampLight = new GameObject("LampLight");
                lampLight.transform.parent = lampHead.transform;
                
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
                whiteboard.transform.parent = parent.transform;
                whiteboard.transform.localPosition = new Vector3(-3.95f, 0, 0);
                whiteboard.transform.localScale = new Vector3(0.05f, 1.5f, 2);
                whiteboard.transform.localRotation = Quaternion.Euler(0, 90, 0);
                
                Renderer whiteboardRenderer = whiteboard.GetComponent<Renderer>();
                if (whiteboardRenderer != null)
                {
                    whiteboardRenderer.material = new Material(Shader.Find("Standard"));
                    whiteboardRenderer.material.color = Color.white;
                }
                
                // Add sticky notes
                for (int i = 0; i < 5; i++)
                {
                    float xPos = -3.92f;
                    float yPos = -0.5f + i * 0.25f;
                    float zPos = Random.Range(-0.8f, 0.8f);
                    
                    GameObject stickyNote = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    stickyNote.name = $"StickyNote_{i}";
                    stickyNote.transform.parent = parent.transform;
                    stickyNote.transform.localPosition = new Vector3(xPos, yPos, zPos);
                    stickyNote.transform.localScale = new Vector3(0.02f, 0.1f, 0.1f);
                    stickyNote.transform.localRotation = Quaternion.Euler(Random.Range(-5f, 5f), 90, Random.Range(-5f, 5f));
                    
                    Renderer stickyNoteRenderer = stickyNote.GetComponent<Renderer>();
                    if (stickyNoteRenderer != null)
                    {
                        stickyNoteRenderer.material = new Material(Shader.Find("Standard"));
                        
                        // Random colors for sticky notes
                        Color[] stickyColors = new Color[] {
                            new Color(1.0f, 0.8f, 0.2f), // Yellow
                            new Color(0.2f, 0.8f, 1.0f), // Blue
                            new Color(1.0f, 0.5f, 0.5f), // Pink
                            new Color(0.5f, 1.0f, 0.5f), // Green
                            new Color(1.0f, 0.6f, 0.2f)  // Orange
                        };
                        
                        stickyNoteRenderer.material.color = stickyColors[i % stickyColors.Length];
                    }
                    
                    TryAddComponentByTypeName(stickyNote, "InteractableItem");
                }
                break;
                
            case EnvironmentType.Casual:
                // Add a picture frame
                GameObject pictureFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pictureFrame.name = "PictureFrame";
                pictureFrame.transform.parent = parent.transform;
                pictureFrame.transform.localPosition = new Vector3(0, 0, -2.45f);
                pictureFrame.transform.localScale = new Vector3(0.8f, 0.6f, 0.05f);
                
                Renderer frameRenderer = pictureFrame.GetComponent<Renderer>();
                if (frameRenderer != null)
                {
                    frameRenderer.material = new Material(Shader.Find("Standard"));
                    frameRenderer.material.color = new Color(0.5f, 0.3f, 0.2f);
                }
                
                GameObject picture = GameObject.CreatePrimitive(PrimitiveType.Cube);
                picture.name = "Picture";
                picture.transform.parent = pictureFrame.transform;
                picture.transform.localPosition = new Vector3(0, 0, -0.1f);
                picture.transform.localScale = new Vector3(0.9f, 0.9f, 0.2f);
                
                Renderer pictureRenderer = picture.GetComponent<Renderer>();
                if (pictureRenderer != null)
                {
                    pictureRenderer.material = new Material(Shader.Find("Standard"));
                    pictureRenderer.material.color = new Color(0.3f, 0.6f, 1.0f);
                }
                
                // Add a rug
                GameObject rug = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rug.name = "Rug";
                rug.transform.parent = parent.transform;
                rug.transform.localPosition = new Vector3(0, -1.45f, -1.5f);
                rug.transform.localScale = new Vector3(3, 0.02f, 2);
                
                Renderer rugRenderer = rug.GetComponent<Renderer>();
                if (rugRenderer != null)
                {
                    rugRenderer.material = new Material(Shader.Find("Standard"));
                    rugRenderer.material.color = new Color(0.6f, 0.4f, 0.3f);
                }
                
                // Add a plant
                GameObject plantPot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                plantPot.name = "PlantPot";
                plantPot.transform.parent = parent.transform;
                plantPot.transform.localPosition = new Vector3(2, -1, 2);
                plantPot.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                
                Renderer potRenderer = plantPot.GetComponent<Renderer>();
                if (potRenderer != null)
                {
                    potRenderer.material = new Material(Shader.Find("Standard"));
                    potRenderer.material.color = new Color(0.6f, 0.4f, 0.3f);
                }
                
                GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                plant.name = "Plant";
                plant.transform.parent = plantPot.transform;
                plant.transform.localPosition = new Vector3(0, 1.5f, 0);
                plant.transform.localScale = new Vector3(2, 1.5f, 2);
                
                Renderer plantRenderer = plant.GetComponent<Renderer>();
                if (plantRenderer != null)
                {
                    plantRenderer.material = new Material(Shader.Find("Standard"));
                    plantRenderer.material.color = new Color(0.2f, 0.5f, 0.2f);
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
        mainLight.transform.parent = lightingControl.transform;
        
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
                lampLight.transform.parent = lightingControl.transform;
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
                ceilingLight.transform.parent = parent.transform;
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
                avatar.transform.position = new Vector3(0, -0.5f, -3);
                avatar.transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
                
            case EnvironmentType.Startup:
                // More casual position
                avatar.transform.position = new Vector3(0, -0.5f, -2.5f);
                avatar.transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
                
            case EnvironmentType.Casual:
                // Sitting on the couch
                avatar.transform.position = new Vector3(0.5f, -0.8f, -2);
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
            head.transform.parent = model.transform;
            head.transform.localPosition = new Vector3(0, 1.7f, 0);
            head.transform.localScale = new Vector3(0.2f, 0.25f, 0.2f);
            
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.parent = model.transform;
            body.transform.localPosition = new Vector3(0, 1.0f, 0);
            body.transform.localScale = new Vector3(0.4f, 1.0f, 0.4f);
            
            Renderer headRenderer = head.GetComponent<Renderer>();
            if (headRenderer != null)
            {
                headRenderer.material = new Material(Shader.Find("Standard"));
                headRenderer.material.color = new Color(0.9f, 0.8f, 0.7f);
            }
            
            Renderer bodyRenderer = body.GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                bodyRenderer.material = new Material(Shader.Find("Standard"));
                
                // Different suit colors based on environment
                switch (environmentType)
                {
                    case EnvironmentType.Corporate:
                        bodyRenderer.material.color = new Color(0.2f, 0.2f, 0.3f); // Dark suit
                        break;
                    case EnvironmentType.Startup:
                        bodyRenderer.material.color = new Color(0.4f, 0.4f, 0.5f); // Business casual
                        break;
                    case EnvironmentType.Casual:
                        bodyRenderer.material.color = new Color(0.3f, 0.5f, 0.8f); // Casual blue
                        break;
                }
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
            debugPanel.transform.parent = ui.transform;
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
        background.transform.parent = debugPanel.transform;
        
        UnityEngine.UI.Image backgroundImage = background.AddComponent<UnityEngine.UI.Image>();
        backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.offsetMin = new Vector2(0, 0);
        bgRect.offsetMax = new Vector2(0, 0);
        
        // Add title text
        GameObject titleObject = new GameObject("Title");
        titleObject.transform.parent = debugPanel.transform;
        
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
        infoObject.transform.parent = debugPanel.transform;
        
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