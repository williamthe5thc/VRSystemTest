using UnityEngine;
using System.Collections;

/// <summary>
/// Coordinates the initialization sequence for the VR Interview System
/// Ensures proper order of component initialization and connection
/// </summary>
public class VRSystemInitializer : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] public AudioPlayback audioPlayback;
    [SerializeField] public AudioPlaybackFix audioPlaybackFix;
    [SerializeField] public AvatarController avatarController;
    
    [Header("Initialization Settings")]
    [SerializeField] private float initialDelay = 0.5f;
    [SerializeField] private float secondPassDelay = 1.5f;
    [SerializeField] private bool debugMode = true;
    
    [Header("Fix Settings")]
    [SerializeField] private bool fixAudioIssues = true;
    [SerializeField] private bool fixAvatarIssues = true;
    
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    
    private void Start()
    {
        // Start the initialization sequence with staged delays
        StartCoroutine(InitializeSystemSequence());
    }
    
    private IEnumerator InitializeSystemSequence()
    {
        if (debugMode)
        {
            Debug.Log("VRSystemInitializer: Starting system initialization sequence");
        }
        
        // Wait for initial delay
        yield return new WaitForSeconds(initialDelay);
        
        // Step 1: Find and validate all required components
        FindComponents();
        
        // Step 2: First pass - fix audio components
        if (fixAudioIssues)
        {
            FixAudioComponents();
        }
        
        // Step 3: First pass - fix avatar components
        if (fixAvatarIssues)
        {
            FixAvatarComponents();
        }
        
        // Step 4: Wait for a second before making another pass
        yield return new WaitForSeconds(secondPassDelay - initialDelay);
        
        // Step 5: Second pass - fix audio components again
        if (fixAudioIssues)
        {
            FixAudioComponents();
        }
        
        // Step 6: Second pass - fix avatar components again
        if (fixAvatarIssues)
        {
            FixAvatarComponents();
        }
        
        // Step 7: Verify all connections are properly established
        yield return new WaitForSeconds(0.5f);
        VerifyConnections();
        
        if (debugMode)
        {
            Debug.Log("VRSystemInitializer: System initialization complete");
        }
    }
    
    private void FindComponents()
    {
        // Find AudioPlayback if not assigned
        if (audioPlayback == null)
        {
            audioPlayback = FindObjectOfType<AudioPlayback>();
        }
        
        // Find AudioPlaybackFix if not assigned
        if (audioPlaybackFix == null && audioPlayback != null)
        {
            audioPlaybackFix = audioPlayback.GetComponent<AudioPlaybackFix>();
            if (audioPlaybackFix == null)
            {
                audioPlaybackFix = audioPlayback.gameObject.AddComponent<AudioPlaybackFix>();
                audioPlaybackFix.audioPlayback = audioPlayback;
                
                if (debugMode)
                {
                    Debug.Log("VRSystemInitializer: Added missing AudioPlaybackFix component");
                }
            }
        }
        
        // Find AvatarController if not assigned
        if (avatarController == null)
        {
            avatarController = FindObjectOfType<AvatarController>();
        }
        
        // Add VRMSetupFixer to avatar if needed
        if (avatarController != null)
        {
            VRMSetupFixer setupFixer = avatarController.GetComponent<VRMSetupFixer>();
            if (setupFixer == null)
            {
                setupFixer = avatarController.gameObject.AddComponent<VRMSetupFixer>();
                
                if (debugMode)
                {
                    Debug.Log("VRSystemInitializer: Added missing VRMSetupFixer component");
                }
            }
            
            // Add VRMRuntimeConnector to avatar if needed
            VRMRuntimeConnector runtimeConnector = avatarController.GetComponent<VRMRuntimeConnector>();
            if (runtimeConnector == null)
            {
                runtimeConnector = avatarController.gameObject.AddComponent<VRMRuntimeConnector>();
                
                if (debugMode)
                {
                    Debug.Log("VRSystemInitializer: Added missing VRMRuntimeConnector component");
                }
            }
        }
    }
    
    private void FixAudioComponents()
    {
        if (audioPlaybackFix != null)
        {
            audioPlaybackFix.FixNow();
            
            if (debugMode)
            {
                Debug.Log("VRSystemInitializer: Fixed audio components");
            }
        }
        else if (audioPlayback != null)
        {
            // Ensure AudioSource exists
            AudioSource audioSource = audioPlayback.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = audioPlayback.gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // Use 2D audio for reliable playback
                audioSource.volume = 1.0f;
                audioSource.priority = 0; // Highest priority
                
                if (debugMode)
                {
                    Debug.Log("VRSystemInitializer: Added missing AudioSource to AudioPlayback");
                }
            }
            
            // Connect to avatar controller
            if (avatarController != null)
            {
                // Try to connect via reflection
                try
                {
                    var avatarField = audioPlayback.GetType().GetField("avatarController", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic);
                    
                    if (avatarField != null)
                    {
                        avatarField.SetValue(audioPlayback, avatarController);
                        
                        if (debugMode)
                        {
                            Debug.Log("VRSystemInitializer: Connected AvatarController to AudioPlayback");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"VRSystemInitializer: Error connecting controller - {ex.Message}");
                }
            }
        }
    }
    
    private void FixAvatarComponents()
    {
        if (avatarController != null)
        {
            // Get VRMSetupFixer
            VRMSetupFixer setupFixer = avatarController.GetComponent<VRMSetupFixer>();
            if (setupFixer != null)
            {
                setupFixer.FixVRMSetup();
                
                if (debugMode)
                {
                    Debug.Log("VRSystemInitializer: Fixed avatar components");
                }
            }
            
            // Also try VRMRuntimeConnector
            VRMRuntimeConnector connector = avatarController.GetComponent<VRMRuntimeConnector>();
            if (connector != null)
            {
                connector.ConnectVRMComponents();
                
                if (debugMode)
                {
                    Debug.Log("VRSystemInitializer: Connected VRM components");
                }
            }
            
            // Ensure Animator has a valid controller
            Animator animator = avatarController.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController == null)
            {
                // Look for animator controller in resources
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
                    if (debugMode)
                    {
                        Debug.Log($"VRSystemInitializer: Assigned AnimatorController to avatar");
                    }
                }
                else if (debugMode)
                {
                    Debug.LogWarning("VRSystemInitializer: No AnimatorController found to assign to avatar");
                }
            }
            
            // Ensure AudioSource is connected to LipSync
            if (audioPlayback != null)
            {
                AudioSource audioSource = audioPlayback.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    // Find all LipSync components
                    LipSync[] lipSyncs = avatarController.GetComponentsInChildren<LipSync>(true);
                    foreach (var lipSync in lipSyncs)
                    {
                        if (lipSync != null)
                        {
                            lipSync.SetAudioSource(audioSource);
                            
                            if (debugMode)
                            {
                                Debug.Log($"VRSystemInitializer: Connected AudioSource to {lipSync.name}");
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void VerifyConnections()
    {
        if (audioPlayback != null)
        {
            AudioSource audioSource = audioPlayback.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("VRSystemInitializer: AudioSource STILL missing from AudioPlayback after fixes!");
            }
            else if (debugMode)
            {
                Debug.Log("VRSystemInitializer: Verified AudioSource exists on AudioPlayback");
            }
            
            // Connect events if avatar controller exists
            if (avatarController != null)
            {
                bool eventHandlersConnected = false;
                
                // Use reflection to check if event handlers are connected
                try
                {
                    var startedField = audioPlayback.GetType().GetField("OnPlaybackStarted", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic);
                        
                    if (startedField != null)
                    {
                        System.Delegate d = (System.Delegate)startedField.GetValue(audioPlayback);
                        eventHandlersConnected = (d != null);
                    }
                }
                catch { }
                
                // If not connected, connect them
                if (!eventHandlersConnected)
                {
                    // Remove existing handlers to avoid duplicates
                    audioPlayback.OnPlaybackStarted -= avatarController.OnAudioPlaybackStarted;
                    audioPlayback.OnPlaybackCompleted -= avatarController.OnAudioPlaybackCompleted;
                    
                    // Add event handlers
                    audioPlayback.OnPlaybackStarted += avatarController.OnAudioPlaybackStarted;
                    audioPlayback.OnPlaybackCompleted += avatarController.OnAudioPlaybackCompleted;
                    
                    if (debugMode)
                    {
                        Debug.Log("VRSystemInitializer: Connected AudioPlayback events to AvatarController");
                    }
                }
            }
        }
        
        // Final debug log to confirm everything is complete
        if (debugMode)
        {
            Debug.Log("VRSystemInitializer: All system connections verified");
        }
    }
}