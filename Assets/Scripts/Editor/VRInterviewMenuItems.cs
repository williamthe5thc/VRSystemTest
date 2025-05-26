using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRM;
using System.Linq;

/// <summary>
/// Provides Unity Editor menu items for fixing common issues in the VR Interview System
/// </summary>
public class VRInterviewMenuItems
{
    [MenuItem("VR Interview/Complete Setup")]
    private static void PerformCompleteSetup()
    {
        Debug.Log("Starting complete VR Interview system setup");
        
        // Add SystemInitializer if missing
        EnsureSystemInitializerExists();
        
        // Fix Avatar Components
        FixAvatarComponents();
        
        // Fix Audio Components
        FixAudioComponents();
        
        // Fix Animator Setup
        FixAnimatorSetup();
        
        Debug.Log("VR Interview system setup complete");
    }
    
    [MenuItem("VR Interview/Create Animator Controller")]
    private static void CreateAnimatorController()
    {
        string path = "Assets/Animations/InterviewerAnimator.controller";
        
        // Check if controller already exists
        if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path) != null)
        {
            Debug.Log("Animator controller already exists at: " + path);
            return;
        }
        
        // Create folder if needed
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
        {
            AssetDatabase.CreateFolder("Assets", "Animations");
        }
        
        // Create a new animator controller
        UnityEditor.Animations.AnimatorController controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(path);
        
        // Add parameters
        controller.AddParameter("Talking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Gesture", AnimatorControllerParameterType.Trigger);
        
        // Add states
        var rootStateMachine = controller.layers[0].stateMachine;
        var idleState = rootStateMachine.AddState("Idle");
        var talkingState = rootStateMachine.AddState("Talking");
        
        // Add transitions
        var idleToTalking = idleState.AddTransition(talkingState);
        idleToTalking.AddCondition(AnimatorConditionMode.If, 0, "Talking");
        
        var talkingToIdle = talkingState.AddTransition(idleState);
        talkingToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Talking");
        
        AssetDatabase.SaveAssets();
        Debug.Log("Created new animator controller at: " + path);
    }
    
    [MenuItem("VR Interview/Create Bootstrap Scene")]
    private static void CreateBootstrapScene()
    {
        // TODO: Implement scene creation logic
        Debug.Log("Bootstrap scene creation not yet implemented");
    }
    
    [MenuItem("VR Interview/Fix Animator Setup")]
    private static void FixAnimatorSetup()
    {
        Debug.Log("Starting animator setup fix");
        
        // Find all avatar controllers
        AvatarController[] avatarControllers = Object.FindObjectsOfType<AvatarController>();
        if (avatarControllers.Length == 0)
        {
            Debug.LogWarning("No AvatarController found in the scene");
            return;
        }
        
        foreach (var avatarController in avatarControllers)
        {
            // Find or add Animator
            Animator animator = avatarController.GetComponent<Animator>();
            if (animator == null)
            {
                animator = avatarController.gameObject.AddComponent<Animator>();
                Debug.Log("Added Animator to " + avatarController.name);
            }
            
            // Ensure animator has a controller
            if (animator.runtimeAnimatorController == null)
            {
                // Try to find an existing controller
                RuntimeAnimatorController controller = AssetDatabase.FindAssets("t:AnimatorController")
                    .Select(guid => AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AssetDatabase.GUIDToAssetPath(guid)))
                    .FirstOrDefault();
                
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                    Debug.Log("Assigned " + controller.name + " to " + avatarController.name);
                }
                else
                {
                    Debug.LogWarning("No animator controller found in project. Please create one first.");
                }
            }
        }
        
        Debug.Log("Animator setup fix completed");
    }
    
    [MenuItem("VR Interview/Fix Avatar Components")]
    private static void FixAvatarComponents()
    {
        Debug.Log("Starting avatar component fix");
        
        // Find all avatars
        GameObject[] avatars = GameObject.FindGameObjectsWithTag("Avatar");
        if (avatars.Length == 0)
        {
            // Try to find by name if tag is not set
            avatars = Object.FindObjectsOfType<GameObject>()
                .Where(go => go.name.ToLower().Contains("avatar"))
                .ToArray();
                
            if (avatars.Length == 0)
            {
                Debug.LogWarning("No avatar found in scene");
                return;
            }
        }
        
        foreach (var avatar in avatars)
        {
            // Ensure VRMSetupFixer exists
            VRMSetupFixer fixer = avatar.GetComponent<VRMSetupFixer>();
            if (fixer == null)
            {
                fixer = avatar.AddComponent<VRMSetupFixer>();
                Debug.Log("Added VRMSetupFixer to " + avatar.name);
            }
            
            // Run fixer
            fixer.FixVRMSetup();
            
            // Ensure VRMRuntimeConnector exists
            VRMRuntimeConnector connector = avatar.GetComponent<VRMRuntimeConnector>();
            if (connector == null)
            {
                connector = avatar.AddComponent<VRMRuntimeConnector>();
                Debug.Log("Added VRMRuntimeConnector to " + avatar.name);
            }
            
            // Run connector
            connector.ConnectVRMComponents();
        }
        
        Debug.Log("Avatar component fix completed");
    }
    
    [MenuItem("VR Interview/Fix Audio Components")]
    private static void FixAudioComponents()
    {
        Debug.Log("Starting audio component fix");
        
        // Find AudioPlayback
        AudioPlayback audioPlayback = Object.FindObjectOfType<AudioPlayback>();
        if (audioPlayback == null)
        {
            Debug.LogWarning("No AudioPlayback found in scene");
            return;
        }
        
        // Ensure AudioSource exists
        AudioSource audioSource = audioPlayback.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = audioPlayback.gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D audio for reliable playback
            audioSource.volume = 1.0f;
            audioSource.priority = 0; // Highest priority
            Debug.Log("Added AudioSource to AudioPlayback");
        }
        
        // Ensure AudioPlaybackFix exists
        AudioPlaybackFix fix = audioPlayback.GetComponent<AudioPlaybackFix>();
        if (fix == null)
        {
            fix = audioPlayback.gameObject.AddComponent<AudioPlaybackFix>();
            fix.audioPlayback = audioPlayback;
            fix.audioSource = audioSource;
            Debug.Log("Added AudioPlaybackFix to AudioPlayback");
        }
        
        // Run fix
        fix.FixNow();
        
        Debug.Log("Audio component fix completed");
    }
    
    private static void EnsureSystemInitializerExists()
    {
        // Find existing initializer
        VRSystemInitializer initializer = Object.FindObjectOfType<VRSystemInitializer>();
        
        if (initializer == null)
        {
            // Create new GameObject for initializer
            GameObject initializerObject = new GameObject("SystemInitializer");
            initializer = initializerObject.AddComponent<VRSystemInitializer>();
            
            // Set up references
            initializer.audioPlayback = Object.FindObjectOfType<AudioPlayback>();
            initializer.audioPlaybackFix = Object.FindObjectOfType<AudioPlaybackFix>();
            initializer.avatarController = Object.FindObjectOfType<AvatarController>();
            
            Debug.Log("Created SystemInitializer GameObject with VRSystemInitializer component");
        }
        else
        {
            Debug.Log("SystemInitializer already exists");
        }
    }
}