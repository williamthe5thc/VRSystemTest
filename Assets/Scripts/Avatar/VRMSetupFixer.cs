using UnityEngine;
using VRM;

/// <summary>
/// Helper component to fix VRM avatar setup issues at runtime
/// This addresses common issues with animator controllers, missing components, and more
/// </summary>
public class VRMSetupFixer : MonoBehaviour
{
    [Header("Fix Settings")]
    public bool fixOnStart = true;
    public bool fixOnAwake = true;
    public bool debugMode = true;
    public float initialFixDelay = 0.2f;
    public float secondFixDelay = 1.0f; // Second attempt after a longer delay
    
    private void Awake()
    {
        if (fixOnAwake)
        {
            // Add a small delay to ensure all components are initialized
            Invoke("FixVRMSetup", initialFixDelay);
            // Make a second attempt after more time (helps with race conditions)
            Invoke("FixVRMSetup", secondFixDelay);
        }
    }
    
    private void Start()
    {
        if (fixOnStart && !fixOnAwake) // Avoid duplicate fixing if both are enabled
        {
            // Add a small delay to ensure all components are initialized
            Invoke("FixVRMSetup", initialFixDelay);
        }
    }
    
    /// <summary>
    /// Call this method to fix common VRM setup issues
    /// </summary>
    public void FixVRMSetup()
    {
        if (debugMode)
        {
            Debug.Log("VRMSetupFixer: Starting to fix VRM avatar setup issues...");
        }
        
        GameObject avatarRoot = gameObject;
        
        // 1. Find BlendShapeProxy
        VRMBlendShapeProxy blendShapeProxy = avatarRoot.GetComponent<VRMBlendShapeProxy>();
        if (blendShapeProxy == null)
        {
            blendShapeProxy = avatarRoot.GetComponentInChildren<VRMBlendShapeProxy>();
            
            if (blendShapeProxy == null)
            {
                Debug.LogError("VRMBlendShapeProxy not found! VRM components will not function correctly.");
                return;
            }
            else if (debugMode)
            {
                Debug.Log("Found VRMBlendShapeProxy in children");
            }
        }
        
        // 2. Fix VRMLipSync
        VRMLipSync lipSync = avatarRoot.GetComponent<VRMLipSync>();
        if (lipSync != null)
        {
            // Set BlendShapeProxy
            lipSync.BlendShapeProxy = blendShapeProxy;
            
            // Find AudioSource
            AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
            if (audioPlayback != null)
            {
                AudioSource audioSourceComp = audioPlayback.GetComponent<AudioSource>();
                if (audioSourceComp != null)
                {
                    lipSync.AudioSource = audioSourceComp;
                    
                    if (debugMode)
                    {
                        Debug.Log("Connected AudioSource to VRMLipSync");
                    }
                }
                else
                {
                    // Create AudioSource if missing
                    audioSourceComp = audioPlayback.gameObject.AddComponent<AudioSource>();
                    audioSourceComp.playOnAwake = false;
                    audioSourceComp.spatialBlend = 0f; // 2D sound for reliability
                    audioSourceComp.volume = 1.0f;
                    audioSourceComp.priority = 0; // Highest priority
                    
                    lipSync.AudioSource = audioSourceComp;
                    Debug.Log("Created and connected new AudioSource to VRMLipSync");
                }
            }
            
            if (debugMode)
            {
                Debug.Log("Fixed VRMLipSync connections");
            }
        }
        
        // 3. Fix VRMFacialExpressions
        VRMFacialExpressions facialExpressions = avatarRoot.GetComponent<VRMFacialExpressions>();
        if (facialExpressions != null)
        {
            facialExpressions.BlendShapeProxy = blendShapeProxy;
            
            if (debugMode)
            {
                Debug.Log("Fixed VRMFacialExpressions connections");
            }
        }
        
        // 4. Fix Animator Connection to Avatar
        Animator animator = avatarRoot.GetComponent<Animator>();
        if (animator == null)
        {
            animator = avatarRoot.AddComponent<Animator>();
            Debug.Log("Added missing Animator component to avatar");
        }
        
        if (animator.runtimeAnimatorController == null)
        {
            // Try to find a suitable animator controller
            RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("InterviewerAnimator");
            if (controller == null)
            {
                // Look for any animator controller in the project
                RuntimeAnimatorController[] controllers = Resources.FindObjectsOfTypeAll<RuntimeAnimatorController>();
                if (controllers.Length > 0)
                {                    
                    // Use the first one that isn't an AnimatorOverrideController
                    foreach (var ctrl in controllers)
                    {
                        if (!(ctrl is AnimatorOverrideController))
                        {
                            controller = ctrl;
                            break;
                        }
                    }
                }
            }
            
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
                Debug.Log($"Connected RuntimeAnimatorController to Animator: {controller.name}");
            }
        }
        
        // 5. Fix VRMAvatarAdapter
        VRMAvatarAdapter adapter = avatarRoot.GetComponent<VRMAvatarAdapter>();
        if (adapter != null)
        {
            adapter.VrmAvatarRoot = avatarRoot;
            adapter.BlendShapeProxy = blendShapeProxy;
            
            if (lipSync != null)
            {
                adapter.VrmLipSync = lipSync;
            }
            
            if (facialExpressions != null)
            {
                adapter.VrmFacialExpressions = facialExpressions;
            }
            
            // Connect AvatarController
            AvatarController avatarController = avatarRoot.GetComponent<AvatarController>();
            if (avatarController != null)
            {
                adapter.AvatarController = avatarController;
                
                // Connect animator to AvatarController
                try 
                {
                    // Use reflection to set private field if necessary
                    var animatorField = avatarController.GetType().GetField("animator", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic);
                        
                    if (animatorField != null)
                    {
                        animatorField.SetValue(avatarController, animator);
                        
                        if (debugMode)
                        {
                            Debug.Log("Connected Animator to AvatarController via reflection");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Failed to connect Animator to AvatarController: " + ex.Message);
                }
            }
            
            // Find AudioSource
            AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
            if (audioPlayback != null)
            {
                AudioSource audioSourceComp = audioPlayback.GetComponent<AudioSource>();
                if (audioSourceComp != null)
                {
                    adapter.AudioSource = audioSourceComp;
                }
            }
            
            if (debugMode)
            {
                Debug.Log("Fixed VRMAvatarAdapter connections");
            }
        }
        
        // 6. Fix AudioPlayback connections with AvatarController
        AudioPlayback playback = FindObjectOfType<AudioPlayback>();
        AvatarController avatarCtrl = avatarRoot.GetComponent<AvatarController>();
        
        if (playback != null && avatarCtrl != null)
        {
            // Try to set AvatarController reference
            try
            {
                var controllerField = playback.GetType().GetField("avatarController", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic);
                    
                if (controllerField != null)
                {
                    controllerField.SetValue(playback, avatarCtrl);
                    
                    if (debugMode)
                    {
                        Debug.Log("Connected AvatarController to AudioPlayback via reflection");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to connect AvatarController to AudioPlayback: " + ex.Message);
            }
            
            // Connect event handlers
            try
            {
                // Remove existing handlers to avoid duplicates
                playback.OnPlaybackStarted -= avatarCtrl.OnAudioPlaybackStarted;
                playback.OnPlaybackCompleted -= avatarCtrl.OnAudioPlaybackCompleted;
                playback.OnPlaybackProgress -= avatarCtrl.UpdateLipSync;
                
                // Add handlers
                playback.OnPlaybackStarted += avatarCtrl.OnAudioPlaybackStarted;
                playback.OnPlaybackCompleted += avatarCtrl.OnAudioPlaybackCompleted;
                playback.OnPlaybackProgress += avatarCtrl.UpdateLipSync;
                
                if (debugMode)
                {
                    Debug.Log("Connected AudioPlayback events to AvatarController");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to connect AudioPlayback events: " + ex.Message);
            }
        }
        
        // 7. Fix VRMToAvatarBridge if present
        VRMToAvatarBridge bridge = avatarRoot.GetComponent<VRMToAvatarBridge>();
        if (bridge != null)
        {
            bridge.FixConnections();
            
            if (debugMode)
            {
                Debug.Log("Fixed VRMToAvatarBridge connections");
            }
        }
        
        // 8. Fix Audio Components for LipSync
        FixAudioComponentsForLipSync();
        
        if (debugMode)
        {
            Debug.Log("VRMSetupFixer: Setup fixes completed!");
        }
    }
    
    private void FixAudioComponentsForLipSync()
    {
        // Find all LipSync components
        LipSync[] lipSyncs = FindObjectsOfType<LipSync>(true);
        if (lipSyncs.Length == 0) return;
        
        // Get main AudioSource
        AudioSource mainAudioSource = null;
        AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
        if (audioPlayback != null)
        {
            mainAudioSource = audioPlayback.GetComponent<AudioSource>();
            if (mainAudioSource == null)
            {
                mainAudioSource = audioPlayback.gameObject.AddComponent<AudioSource>();
                mainAudioSource.playOnAwake = false;
                mainAudioSource.spatialBlend = 0f; // 2D sound for reliability
                mainAudioSource.volume = 1.0f;
                Debug.Log("Created new AudioSource for AudioPlayback");
            }
        }
        
        if (mainAudioSource == null) return;
        
        // Connect LipSync components to AudioSource
        foreach (var lipSync in lipSyncs)
        {
            if (lipSync != null)
            {
                try
                {
                    lipSync.SetAudioSource(mainAudioSource);
                    Debug.Log($"Connected AudioSource to LipSync: {lipSync.name}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Could not connect AudioSource to LipSync: {ex.Message}");
                }
            }
        }
    }
}