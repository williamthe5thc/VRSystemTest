using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRM;
using System.IO;

/// <summary>
/// Editor utility to help set up VRM avatar components
/// </summary>
[CustomEditor(typeof(VRMAvatarAdapter))]
public class VRMAvatarSetupHelper : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        VRMAvatarAdapter adapter = (VRMAvatarAdapter)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("VRM Avatar Setup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Setup VRM Avatar Components"))
        {
            SetupVRMAvatar(adapter.gameObject);
        }
        
        if (GUILayout.Button("Create Animator Controller"))
        {
            CreateAnimatorController();
        }
        
        if (GUILayout.Button("Fix Animator Issues"))
        {
            FixAnimatorIssues(adapter.gameObject);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("1. Click 'Create Animator Controller' first\n2. Then click 'Setup VRM Avatar Components'\n3. Finally click 'Fix Animator Issues'\n\nAfter this, add a VRMSetupFixer to handle runtime fixes.", MessageType.Info);
    }
    
    [MenuItem("Tools/VR Interview/Create Animator Controller")]
    public static void CreateAnimatorControllerMenuItem()
    {
        CreateAnimatorController();
    }
    
    [MenuItem("Tools/VR Interview/Fix Avatar Components")]
    public static void FixAvatarComponentsMenuItem()
    {
        // Find test Avatar in the scene
        GameObject avatar = GameObject.Find("test Avatar");
        if (avatar != null)
        {
            SetupVRMAvatar(avatar);
        }
        else
        {
            Debug.LogError("Could not find 'test Avatar' in the scene!");
            EditorUtility.DisplayDialog("Avatar Not Found", "Could not find 'test Avatar' in the scene. Please make sure it exists.", "OK");
        }
    }
    
    [MenuItem("Tools/VR Interview/Fix Animator Setup")]
    public static void FixAnimatorSetupMenuItem()
    {
        // Find test Avatar in the scene
        GameObject avatar = GameObject.Find("test Avatar");
        if (avatar != null)
        {
            FixAnimatorIssues(avatar);
        }
        else
        {
            Debug.LogError("Could not find 'test Avatar' in the scene!");
            EditorUtility.DisplayDialog("Avatar Not Found", "Could not find 'test Avatar' in the scene. Please make sure it exists.", "OK");
        }
    }
    
    [MenuItem("Tools/VR Interview/Complete Setup")]
    public static void CompleteSetupMenuItem()
    {
        // Find test Avatar in the scene
        GameObject avatar = GameObject.Find("test Avatar");
        if (avatar != null)
        {
            CreateAnimatorController();
            SetupVRMAvatar(avatar);
            FixAnimatorIssues(avatar);
            EditorUtility.DisplayDialog("Setup Complete", "VRM avatar setup has been completed successfully!", "OK");
        }
        else
        {
            Debug.LogError("Could not find 'test Avatar' in the scene!");
            EditorUtility.DisplayDialog("Avatar Not Found", "Could not find 'test Avatar' in the scene. Please make sure it exists.", "OK");
        }
    }
    
    /// <summary>
    /// Creates a basic animator controller for the VRM avatar
    /// </summary>
    public static void CreateAnimatorController()
    {
        // Create the Resources directory if it doesn't exist
        if (!Directory.Exists("Assets/Resources"))
        {
            Directory.CreateDirectory("Assets/Resources");
            AssetDatabase.Refresh();
        }
        
        // Check if controller already exists
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Resources/InterviewerAnimator.controller") != null)
        {
            if (!EditorUtility.DisplayDialog("Confirm Overwrite", 
                "InterviewerAnimator.controller already exists. Overwrite it?", 
                "Overwrite", "Cancel"))
            {
                return;
            }
        }
        
        // Create a new animator controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/Resources/InterviewerAnimator.controller");
        
        // Add parameters for each animation state
        controller.AddParameter("Idle", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Listening", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Thinking", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Speaking", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Attentive", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Confused", AnimatorControllerParameterType.Trigger);
        
        // Get the root state machine
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
        
        // Create states
        AnimatorState idleState = rootStateMachine.AddState("Idle");
        AnimatorState listeningState = rootStateMachine.AddState("Listening");
        AnimatorState thinkingState = rootStateMachine.AddState("Thinking");
        AnimatorState speakingState = rootStateMachine.AddState("Speaking");
        AnimatorState attentiveState = rootStateMachine.AddState("Attentive");
        AnimatorState confusedState = rootStateMachine.AddState("Confused");
        
        // Set Idle as the default state
        rootStateMachine.defaultState = idleState;
        
        // Save the controller
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("InterviewerAnimator.controller created successfully in the Resources folder");
        
        // Show the animator controller
        Selection.activeObject = controller;
        EditorGUIUtility.PingObject(controller);
    }
    
    /// <summary>
    /// Handles the setup of VRM avatar components
    /// </summary>
    public static void SetupVRMAvatar(GameObject avatarRoot)
    {
        if (avatarRoot == null)
        {
            Debug.LogError("Avatar root is null!");
            return;
        }
        
        // Find VRM components
        VRMBlendShapeProxy blendShapeProxy = avatarRoot.GetComponent<VRMBlendShapeProxy>();
        if (blendShapeProxy == null)
        {
            blendShapeProxy = avatarRoot.GetComponentInChildren<VRMBlendShapeProxy>();
            if (blendShapeProxy == null)
            {
                Debug.LogError("No VRMBlendShapeProxy found on the avatar!");
                return;
            }
        }
        
        // Find or add components
        bool addedComponents = false;
        
        // Add VRMLipSync if missing
        VRMLipSync lipSync = avatarRoot.GetComponent<VRMLipSync>();
        if (lipSync == null)
        {
            Undo.RecordObject(avatarRoot, "Add VRMLipSync");
            lipSync = avatarRoot.AddComponent<VRMLipSync>();
            addedComponents = true;
            Debug.Log("Added VRMLipSync component");
        }
        
        // Add VRMFacialExpressions if missing
        VRMFacialExpressions facialExpressions = avatarRoot.GetComponent<VRMFacialExpressions>();
        if (facialExpressions == null)
        {
            Undo.RecordObject(avatarRoot, "Add VRMFacialExpressions");
            facialExpressions = avatarRoot.AddComponent<VRMFacialExpressions>();
            addedComponents = true;
            Debug.Log("Added VRMFacialExpressions component");
        }
        
        // Add VRMAvatarAdapter if missing
        VRMAvatarAdapter adapter = avatarRoot.GetComponent<VRMAvatarAdapter>();
        if (adapter == null)
        {
            Undo.RecordObject(avatarRoot, "Add VRMAvatarAdapter");
            adapter = avatarRoot.AddComponent<VRMAvatarAdapter>();
            addedComponents = true;
            Debug.Log("Added VRMAvatarAdapter component");
        }
        
        // Find scene dependencies
        AudioPlayback audioPlayback = Object.FindObjectOfType<AudioPlayback>();
        AudioSource audioSource = null;
        if (audioPlayback != null)
        {
            audioSource = audioPlayback.GetComponent<AudioSource>();
        }
        
        AvatarController avatarController = avatarRoot.GetComponent<AvatarController>();
        
        // Add AvatarController if missing
        if (avatarController == null)
        {
            Undo.RecordObject(avatarRoot, "Add AvatarController");
            avatarController = avatarRoot.AddComponent<AvatarController>();
            addedComponents = true;
            Debug.Log("Added AvatarController component");
        }
        
        // Configure VRMLipSync
        if (lipSync != null)
        {
            Undo.RecordObject(lipSync, "Configure VRMLipSync");
            SerializedObject serializedLipSync = new SerializedObject(lipSync);
            
            SerializedProperty blendShapeProxyProp = serializedLipSync.FindProperty("blendShapeProxy");
            if (blendShapeProxyProp != null && blendShapeProxy != null)
            {
                blendShapeProxyProp.objectReferenceValue = blendShapeProxy;
            }
            
            if (audioSource != null)
            {
                SerializedProperty audioSourceProp = serializedLipSync.FindProperty("audioSource");
                if (audioSourceProp != null)
                {
                    audioSourceProp.objectReferenceValue = audioSource;
                }
            }
            
            serializedLipSync.ApplyModifiedProperties();
            EditorUtility.SetDirty(lipSync);
            Debug.Log("Configured VRMLipSync component");
        }
        
        // Configure VRMFacialExpressions
        if (facialExpressions != null)
        {
            Undo.RecordObject(facialExpressions, "Configure VRMFacialExpressions");
            SerializedObject serializedFacialExpressions = new SerializedObject(facialExpressions);
            
            SerializedProperty blendShapeProxyProp = serializedFacialExpressions.FindProperty("blendShapeProxy");
            if (blendShapeProxyProp != null && blendShapeProxy != null)
            {
                blendShapeProxyProp.objectReferenceValue = blendShapeProxy;
            }
            
            serializedFacialExpressions.ApplyModifiedProperties();
            EditorUtility.SetDirty(facialExpressions);
            Debug.Log("Configured VRMFacialExpressions component");
        }
        
        // Configure VRMAvatarAdapter
        if (adapter != null)
        {
            Undo.RecordObject(adapter, "Configure VRMAvatarAdapter");
            SerializedObject serializedAdapter = new SerializedObject(adapter);
            
            SerializedProperty avatarRootProp = serializedAdapter.FindProperty("vrmAvatarRoot");
            if (avatarRootProp != null)
            {
                avatarRootProp.objectReferenceValue = avatarRoot;
            }
            
            SerializedProperty blendShapeProxyProp = serializedAdapter.FindProperty("blendShapeProxy");
            if (blendShapeProxyProp != null && blendShapeProxy != null)
            {
                blendShapeProxyProp.objectReferenceValue = blendShapeProxy;
            }
            
            SerializedProperty lipSyncProp = serializedAdapter.FindProperty("vrmLipSync");
            if (lipSyncProp != null && lipSync != null)
            {
                lipSyncProp.objectReferenceValue = lipSync;
            }
            
            SerializedProperty facialExpressionsProp = serializedAdapter.FindProperty("vrmFacialExpressions");
            if (facialExpressionsProp != null && facialExpressions != null)
            {
                facialExpressionsProp.objectReferenceValue = facialExpressions;
            }
            
            SerializedProperty avatarControllerProp = serializedAdapter.FindProperty("avatarController");
            if (avatarControllerProp != null && avatarController != null)
            {
                avatarControllerProp.objectReferenceValue = avatarController;
            }
            
            if (audioSource != null)
            {
                SerializedProperty audioSourceProp = serializedAdapter.FindProperty("audioSource");
                if (audioSourceProp != null)
                {
                    audioSourceProp.objectReferenceValue = audioSource;
                }
            }
            
            serializedAdapter.ApplyModifiedProperties();
            EditorUtility.SetDirty(adapter);
            Debug.Log("Configured VRMAvatarAdapter component");
        }
        
        // Add VRMSetupFixer if missing
        if (avatarRoot.GetComponent<VRMSetupFixer>() == null)
        {
            Undo.RecordObject(avatarRoot, "Add VRMSetupFixer");
            avatarRoot.AddComponent<VRMSetupFixer>();
            addedComponents = true;
            Debug.Log("Added VRMSetupFixer component");
        }
        
        // Add VRMToAvatarBridge if missing 
        if (avatarRoot.GetComponent<VRMToAvatarBridge>() == null)
        {
            Undo.RecordObject(avatarRoot, "Add VRMToAvatarBridge");
            avatarRoot.AddComponent<VRMToAvatarBridge>();
            addedComponents = true;
            Debug.Log("Added VRMToAvatarBridge component");
        }
        
        // Save changes
        if (addedComponents)
        {
            EditorUtility.SetDirty(avatarRoot);
            Debug.Log("VRM avatar setup complete");
        }
        else
        {
            Debug.Log("VRM avatar already setup");
        }
    }
    
    /// <summary>
    /// Fixes animator issues on the avatar
    /// </summary>
    public static void FixAnimatorIssues(GameObject avatarRoot)
    {
        Animator animator = avatarRoot.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("No Animator component found! Adding one...");
            animator = avatarRoot.AddComponent<Animator>();
        }
        
        // Look for animator controller in Resources
        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("InterviewerAnimator");
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
            Debug.Log("Assigned InterviewerAnimator controller to Animator");
        }
        else
        {
            Debug.LogError("Could not find InterviewerAnimator.controller in Resources folder! Please create it first.");
        }
        
        // Fix AvatarController connections
        AvatarController avatarController = avatarRoot.GetComponent<AvatarController>();
        if (avatarController != null)
        {
            SerializedObject serializedController = new SerializedObject(avatarController);
            
            SerializedProperty animatorProp = serializedController.FindProperty("animator");
            if (animatorProp != null)
            {
                animatorProp.objectReferenceValue = animator;
            }
            
            serializedController.ApplyModifiedProperties();
            EditorUtility.SetDirty(avatarController);
            Debug.Log("Connected Animator to AvatarController");
        }
        
        EditorUtility.SetDirty(avatarRoot);
    }
}