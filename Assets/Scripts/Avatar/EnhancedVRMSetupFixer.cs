using UnityEngine;
using VRM;
using System.Collections;
using System.Collections.Generic;

namespace VRInterview
{
    /// <summary>
    /// Enhanced VRM setup fixer with specific fixes for animator issues
    /// and improved lip sync support
    /// </summary>
    public class EnhancedVRMSetupFixer : MonoBehaviour
    {
        [Header("Components to Fix")]
        [SerializeField] private GameObject avatarRoot;
        [SerializeField] private VRMBlendShapeProxy blendShapeProxy;
        [SerializeField] private Animator animator;
        [SerializeField] private VRMLipSync vrmLipSync;
        [SerializeField] private VRMFacialExpressions facialExpressions;
        [SerializeField] private AudioSource audioSource;
        
        [Header("Fix Settings")]
        [SerializeField] private bool fixOnAwake = true;
        [SerializeField] private bool fixOnStart = true;
        [SerializeField] private bool fixOnEnable = true;
        [SerializeField] private float initialDelay = 0.5f;
        [SerializeField] private float secondFixDelay = 2.0f;
        [SerializeField] private bool debugMode = true;
        
        [Header("Animator Controller Settings")]
        [SerializeField] private RuntimeAnimatorController defaultController;
        [SerializeField] private bool createBasicControllerIfMissing = true;
        [SerializeField] private bool forceReplaceController = false;
        
        private bool _initialFixApplied = false;
        private Coroutine _fixCoroutine;
        
        private void Awake()
        {
            // Set avatar root to self if not specified
            if (avatarRoot == null)
            {
                avatarRoot = gameObject;
            }
            
            if (fixOnAwake)
            {
                _fixCoroutine = StartCoroutine(FixWithDelay(initialDelay, true));
            }
        }
        
        private void OnEnable()
        {
            if (fixOnEnable && !_initialFixApplied)
            {
                _fixCoroutine = StartCoroutine(FixWithDelay(initialDelay, true));
            }
        }
        
        private void Start()
        {
            if (fixOnStart && !_initialFixApplied)
            {
                _fixCoroutine = StartCoroutine(FixWithDelay(initialDelay, true));
            }
        }
        
        private IEnumerator FixWithDelay(float delay, bool isInitialFix = false)
        {
            yield return new WaitForSeconds(delay);
            
            // Apply main fixes
            ApplyFixes();
            
            // Mark initial fix as applied if this is the first fix
            if (isInitialFix)
            {
                _initialFixApplied = true;
                
                // Schedule a second fix to catch any late-initialized components
                _fixCoroutine = StartCoroutine(FixWithDelay(secondFixDelay, false));
            }
        }
        
        /// <summary>
        /// Apply all fixes to the VRM avatar
        /// </summary>
        public void ApplyFixes()
        {
            if (debugMode)
            {
                Debug.Log("EnhancedVRMSetupFixer: Starting fixes...");
            }
            
            // Find components if not assigned
            FindComponents();
            
            // Fix animator controller first
            FixAnimatorController();
            
            // Fix VRM specific components
            FixVRMComponents();
            
            // Fix audio connections last
            FixAudioConnections();
            
            if (debugMode)
            {
                Debug.Log("EnhancedVRMSetupFixer: Fixes completed");
            }
        }
        
        private void FindComponents()
        {
            // Find BlendShapeProxy
            if (blendShapeProxy == null)
            {
                blendShapeProxy = avatarRoot.GetComponent<VRMBlendShapeProxy>();
                if (blendShapeProxy == null)
                {
                    blendShapeProxy = avatarRoot.GetComponentInChildren<VRMBlendShapeProxy>(true);
                    
                    if (blendShapeProxy != null && debugMode)
                    {
                        Debug.Log($"EnhancedVRMSetupFixer: Found BlendShapeProxy on {blendShapeProxy.gameObject.name}");
                    }
                }
            }
            
            // Find Animator
            if (animator == null)
            {
                animator = avatarRoot.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = avatarRoot.GetComponentInChildren<Animator>(true);
                    
                    if (animator == null)
                    {
                        animator = avatarRoot.AddComponent<Animator>();
                        
                        if (debugMode)
                        {
                            Debug.Log("EnhancedVRMSetupFixer: Added Animator component to avatar");
                        }
                    }
                    else if (debugMode)
                    {
                        Debug.Log($"EnhancedVRMSetupFixer: Found Animator on {animator.gameObject.name}");
                    }
                }
            }
            
            // Find VRMLipSync
            if (vrmLipSync == null)
            {
                vrmLipSync = avatarRoot.GetComponent<VRMLipSync>();
                if (vrmLipSync == null)
                {
                    vrmLipSync = avatarRoot.GetComponentInChildren<VRMLipSync>(true);
                    
                    if (vrmLipSync == null)
                    {
                        // Check if we have the enhanced version
                        var enhanced = avatarRoot.GetComponentInChildren<EnhancedVRMLipSync>(true);
                        if (enhanced != null && debugMode)
                        {
                            Debug.Log($"EnhancedVRMSetupFixer: Found EnhancedVRMLipSync on {enhanced.gameObject.name}");
                        }
                    }
                    else if (debugMode)
                    {
                        Debug.Log($"EnhancedVRMSetupFixer: Found VRMLipSync on {vrmLipSync.gameObject.name}");
                    }
                }
            }
            
            // Find VRMFacialExpressions
            if (facialExpressions == null)
            {
                facialExpressions = avatarRoot.GetComponent<VRMFacialExpressions>();
                if (facialExpressions == null)
                {
                    facialExpressions = avatarRoot.GetComponentInChildren<VRMFacialExpressions>(true);
                    
                    if (facialExpressions != null && debugMode)
                    {
                        Debug.Log($"EnhancedVRMSetupFixer: Found VRMFacialExpressions on {facialExpressions.gameObject.name}");
                    }
                }
            }
            
            // Find AudioSource
            if (audioSource == null)
            {
                // First try to get from AudioPlayback
                var audioPlayback = FindObjectOfType<AudioPlayback>();
                if (audioPlayback != null)
                {
                    audioSource = audioPlayback.GetComponent<AudioSource>();
                    
                    if (audioSource == null)
                    {
                        // Try getting it via reflection
                        try
                        {
                            var field = audioPlayback.GetType().GetField("audioSource",
                                System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.Public |
                                System.Reflection.BindingFlags.NonPublic);
                                
                            if (field != null)
                            {
                                audioSource = field.GetValue(audioPlayback) as AudioSource;
                            }
                        }
                        catch (System.Exception)
                        {
                            // Ignore reflection errors
                        }
                    }
                    
                    // If still null, create one
                    if (audioSource == null)
                    {
                        audioSource = audioPlayback.gameObject.AddComponent<AudioSource>();
                        audioSource.playOnAwake = false;
                        audioSource.spatialBlend = 0f; // 2D sound
                        audioSource.volume = 0.8f;
                        
                        if (debugMode)
                        {
                            Debug.Log("EnhancedVRMSetupFixer: Created AudioSource on AudioPlayback");
                        }
                    }
                    else if (debugMode)
                    {
                        Debug.Log($"EnhancedVRMSetupFixer: Found AudioSource on {audioSource.gameObject.name}");
                    }
                }
            }
        }
        
        private void FixAnimatorController()
        {
            if (animator == null) return;
            
            bool needsNewController = animator.runtimeAnimatorController == null;
            bool isOverrideControllerWithoutBase = false;
            
            // Check if we have an AnimatorOverrideController without a base
            if (animator.runtimeAnimatorController is AnimatorOverrideController overrideController)
            {
                if (overrideController.runtimeAnimatorController == null)
                {
                    isOverrideControllerWithoutBase = true;
                    
                    if (debugMode)
                    {
                        Debug.LogWarning("EnhancedVRMSetupFixer: Found AnimatorOverrideController without base controller");
                    }
                }
            }
            
            // Only proceed if we need a new controller or are forcing replacement
            if ((needsNewController || isOverrideControllerWithoutBase || forceReplaceController) &&
                (defaultController != null || createBasicControllerIfMissing))
            {
                // Use provided default controller if available
                if (defaultController != null)
                {
                    if (debugMode)
                    {
                        Debug.Log($"EnhancedVRMSetupFixer: Setting animator controller to {defaultController.name}");
                    }
                    
                    animator.runtimeAnimatorController = defaultController;
                }
                else if (createBasicControllerIfMissing)
                {
                    // Try to find a controller in the project
                    RuntimeAnimatorController foundController = FindSuitableAnimatorController();
                    
                    if (foundController != null)
                    {
                        animator.runtimeAnimatorController = foundController;
                        
                        if (debugMode)
                        {
                            Debug.Log($"EnhancedVRMSetupFixer: Using found controller: {foundController.name}");
                        }
                    }
                    else
                    {
                        // Create a simple controller at runtime
                        CreateRuntimeAnimatorController();
                    }
                }
            }
        }
        
        private RuntimeAnimatorController FindSuitableAnimatorController()
        {
            // Try by specific names first
            string[] commonControllerNames = {
                "InterviewerAnimator",
                "AvatarAnimator",
                "VRMAnimator",
                "HumanoidAnimator"
            };
            
            foreach (string name in commonControllerNames)
            {
                RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(name);
                if (controller != null)
                {
                    return controller;
                }
            }
            
            // Try to find any controller in the project
            RuntimeAnimatorController[] controllers = Resources.FindObjectsOfTypeAll<RuntimeAnimatorController>();
            foreach (var controller in controllers)
            {
                // Skip override controllers without a base
                if (controller is AnimatorOverrideController aoc && aoc.runtimeAnimatorController == null)
                {
                    continue;
                }
                
                // Skip controllers with empty names
                if (string.IsNullOrEmpty(controller.name))
                {
                    continue;
                }
                
                // Found a usable controller
                return controller;
            }
            
            return null;
        }
        
        private void CreateRuntimeAnimatorController()
        {
            #if UNITY_EDITOR
            // Create a simple controller
            var controllerAsset = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets/Resources/GeneratedController.controller");
            
            // Add parameters
            controllerAsset.AddParameter("IsSpeaking", AnimatorControllerParameterType.Bool);
            controllerAsset.AddParameter("IsListening", AnimatorControllerParameterType.Bool);
            controllerAsset.AddParameter("IsThinking", AnimatorControllerParameterType.Bool);
            
            // Add a state
            var rootStateMachine = controllerAsset.layers[0].stateMachine;
            var idleState = rootStateMachine.AddState("Idle");
            rootStateMachine.defaultState = idleState;
            
            // Use it
            animator.runtimeAnimatorController = controllerAsset;
            
            if (debugMode)
            {
                Debug.Log("EnhancedVRMSetupFixer: Created runtime animator controller in Editor");
            }
            #else
            // Create a dynamic AnimatorController at runtime
            // This is much more limited than the editor version but provides a fallback
            Debug.LogWarning("EnhancedVRMSetupFixer: Creating runtime animator controllers only fully supported in Editor");
            
            // We can still set some basic parameters via the Animator interface
            animator.SetBool("IsSpeaking", false);
            animator.SetBool("IsListening", false);
            animator.SetBool("IsThinking", false);
            #endif
        }
        
        private void FixVRMComponents()
        {
            // Connect VRMLipSync to BlendShapeProxy
            if (vrmLipSync != null && blendShapeProxy != null)
            {
                vrmLipSync.BlendShapeProxy = blendShapeProxy;
                
                if (debugMode)
                {
                    Debug.Log("EnhancedVRMSetupFixer: Connected BlendShapeProxy to VRMLipSync");
                }
            }
            
            // Connect EnhancedVRMLipSync if available
            var enhancedLipSync = avatarRoot.GetComponentInChildren<EnhancedVRMLipSync>(true);
            if (enhancedLipSync != null && blendShapeProxy != null)
            {
                enhancedLipSync.BlendShapeProxy = blendShapeProxy;
                
                if (debugMode)
                {
                    Debug.Log("EnhancedVRMSetupFixer: Connected BlendShapeProxy to EnhancedVRMLipSync");
                }
            }
            
            // Connect VRMFacialExpressions to BlendShapeProxy
            if (facialExpressions != null && blendShapeProxy != null)
            {
                facialExpressions.BlendShapeProxy = blendShapeProxy;
                
                if (debugMode)
                {
                    Debug.Log("EnhancedVRMSetupFixer: Connected BlendShapeProxy to VRMFacialExpressions");
                }
            }
            
            // Fix VRMToAvatarBridge if present
            VRMToAvatarBridge bridge = avatarRoot.GetComponentInChildren<VRMToAvatarBridge>(true);
            if (bridge != null)
            {
                bridge.FixConnections();
                
                if (debugMode)
                {
                    Debug.Log("EnhancedVRMSetupFixer: Applied VRMToAvatarBridge.FixConnections()");
                }
            }
            
            // Connect VRMAvatarAdapter if present
            VRMAvatarAdapter adapter = avatarRoot.GetComponentInChildren<VRMAvatarAdapter>(true);
            if (adapter != null)
            {
                if (blendShapeProxy != null)
                {
                    adapter.BlendShapeProxy = blendShapeProxy;
                }
                
                if (vrmLipSync != null)
                {
                    adapter.VrmLipSync = vrmLipSync;
                }
                
                if (facialExpressions != null)
                {
                    adapter.VrmFacialExpressions = facialExpressions;
                }
                
                if (debugMode)
                {
                    Debug.Log("EnhancedVRMSetupFixer: Connected components to VRMAvatarAdapter");
                }
            }
            
            // Connect AvatarController with Animator
            AvatarController avatarController = avatarRoot.GetComponentInChildren<AvatarController>(true);
            if (avatarController != null && animator != null)
            {
                try
                {
                    var field = avatarController.GetType().GetField("animator",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic);
                        
                    if (field != null)
                    {
                        field.SetValue(avatarController, animator);
                        
                        if (debugMode)
                        {
                            Debug.Log("EnhancedVRMSetupFixer: Connected Animator to AvatarController");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"EnhancedVRMSetupFixer: Error connecting Animator to AvatarController: {ex.Message}");
                }
            }
        }
        
        private void FixAudioConnections()
        {
            if (audioSource == null) return;
            
            // Connect VRMLipSync to AudioSource
            if (vrmLipSync != null)
            {
                vrmLipSync.AudioSource = audioSource;
                
                if (debugMode)
                {
                    Debug.Log("EnhancedVRMSetupFixer: Connected AudioSource to VRMLipSync");
                }
            }
            
            // Connect EnhancedVRMLipSync to AudioSource
            var enhancedLipSync = avatarRoot.GetComponentInChildren<EnhancedVRMLipSync>(true);
            if (enhancedLipSync != null)
            {
                enhancedLipSync.AudioSource = audioSource;
                
                if (debugMode)
                {
                    Debug.Log("EnhancedVRMSetupFixer: Connected AudioSource to EnhancedVRMLipSync");
                }
            }
            
            // Connect regular LipSync components
            LipSync[] lipSyncs = avatarRoot.GetComponentsInChildren<LipSync>(true);
            foreach (var lipSync in lipSyncs)
            {
                if (lipSync != null)
                {
                    lipSync.SetAudioSource(audioSource);
                    
                    if (debugMode)
                    {
                        Debug.Log($"EnhancedVRMSetupFixer: Connected AudioSource to LipSync on {lipSync.gameObject.name}");
                    }
                }
            }
            
            // Connect AudioPlayback to VRMAvatarAdapter if present
            VRMAvatarAdapter adapter = avatarRoot.GetComponentInChildren<VRMAvatarAdapter>(true);
            if (adapter != null)
            {
                adapter.AudioSource = audioSource;
                
                if (debugMode)
                {
                    Debug.Log("EnhancedVRMSetupFixer: Connected AudioSource to VRMAvatarAdapter");
                }
            }
            
            // Connect AvatarController to AudioPlayback events
            AvatarController avatarController = avatarRoot.GetComponentInChildren<AvatarController>(true);
            AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
            
            if (avatarController != null && audioPlayback != null)
            {
                // Try to set the reference via reflection
                try
                {
                    var field = audioPlayback.GetType().GetField("avatarController",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic);
                        
                    if (field != null)
                    {
                        var currentValue = field.GetValue(audioPlayback);
                        if (currentValue != avatarController)
                        {
                            field.SetValue(audioPlayback, avatarController);
                            
                            if (debugMode)
                            {
                                Debug.Log("EnhancedVRMSetupFixer: Connected AvatarController to AudioPlayback");
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"EnhancedVRMSetupFixer: Error connecting AvatarController to AudioPlayback: {ex.Message}");
                }
                
                // Connect events
                try
                {
                    // Remove existing handlers to avoid duplicates
                    audioPlayback.OnPlaybackStarted -= avatarController.OnAudioPlaybackStarted;
                    audioPlayback.OnPlaybackCompleted -= avatarController.OnAudioPlaybackCompleted;
                    
                    // Add handlers
                    audioPlayback.OnPlaybackStarted += avatarController.OnAudioPlaybackStarted;
                    audioPlayback.OnPlaybackCompleted += avatarController.OnAudioPlaybackCompleted;
                    
                    if (debugMode)
                    {
                        Debug.Log("EnhancedVRMSetupFixer: Connected AudioPlayback events to AvatarController");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"EnhancedVRMSetupFixer: Error connecting events: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Creates an audio source for lip sync if needed
        /// </summary>
        /// <returns>The created or found audio source</returns>
        public AudioSource EnsureAudioSourceForLipSync()
        {
            if (audioSource != null)
            {
                return audioSource;
            }
            
            // First try the AudioPlayback component
            AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
            if (audioPlayback != null)
            {
                audioSource = audioPlayback.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = audioPlayback.gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f;
                    audioSource.volume = 0.8f;
                    
                    if (debugMode)
                    {
                        Debug.Log("EnhancedVRMSetupFixer: Created AudioSource on AudioPlayback");
                    }
                }
                return audioSource;
            }
            
            // If no AudioPlayback, create on avatar
            audioSource = avatarRoot.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = avatarRoot.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
                audioSource.volume = 0.8f;
                
                if (debugMode)
                {
                    Debug.Log("EnhancedVRMSetupFixer: Created AudioSource on avatar");
                }
            }
            
            return audioSource;
        }
    }
}
