using System.Collections;
using UnityEngine;
using VRM;

/// <summary>
/// Enhanced fixer specifically focused on resolving lip sync issues for VRM avatars
/// </summary>
public class EnhancedVRMLipSyncFixer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VRMBlendShapeProxy blendShapeProxy;
    [SerializeField] private VRMLipSync vrmLipSync;
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Options")]
    [SerializeField] private bool fixOnAwake = true;
    [SerializeField] private bool fixOnStart = true;
    [SerializeField] private bool runSequentialFixes = true;
    [SerializeField] private bool debugMode = true;
    
    // Timing for multiple fix attempts
    private float[] fixDelays = new float[] { 0.1f, 0.5f, 1.0f, 2.0f };
    
    private void Awake()
    {
        if (fixOnAwake)
        {
            FindAllComponents();
            ApplyFixes();
            
            if (runSequentialFixes)
            {
                StartCoroutine(SequentialFixes());
            }
        }
    }
    
    private void Start()
    {
        if (fixOnStart && !fixOnAwake)
        {
            FindAllComponents();
            ApplyFixes();
            
            if (runSequentialFixes)
            {
                StartCoroutine(SequentialFixes());
            }
        }
    }
    
    /// <summary>
    /// Find all required components in the scene
    /// </summary>
    private void FindAllComponents()
    {
        // Find components on this GameObject if not assigned
        if (blendShapeProxy == null)
            blendShapeProxy = GetComponent<VRMBlendShapeProxy>();
        
        if (vrmLipSync == null)
            vrmLipSync = GetComponent<VRMLipSync>();
        
        if (faceRenderer == null)
        {
            // Try to find the face renderer
            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (renderers.Length > 0)
            {
                // Look for one with "face" in the name
                foreach (var renderer in renderers)
                {
                    if (renderer.name.ToLower().Contains("face"))
                    {
                        faceRenderer = renderer;
                        break;
                    }
                }
                
                // If no face-specific renderer found, use the first one
                if (faceRenderer == null && renderers.Length > 0)
                {
                    faceRenderer = renderers[0];
                }
            }
        }
        
        // Find the main audio source from AudioPlayback
        if (audioSource == null)
        {
            AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
            if (audioPlayback != null)
            {
                // Try getting the AudioSource component
                audioSource = audioPlayback.GetComponent<AudioSource>();
                
                // If not found on AudioPlayback, create one
                if (audioSource == null)
                {
                    audioSource = audioPlayback.gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 0f; // 2D sound for lip sync
                    audioSource.priority = 0; // Highest priority
                    
                    if (debugMode)
                        Debug.Log("Created new AudioSource on AudioPlayback object");
                }
            }
        }
    }
    
    /// <summary>
    /// Apply all fixes in one go
    /// </summary>
    public void ApplyFixes()
    {
        if (debugMode)
            Debug.Log("EnhancedVRMLipSyncFixer: Applying fixes...");
        
        // Ensure we have all components
        FindAllComponents();
        
        // Fix VRMLipSync
        FixVRMLipSync();
        
        // Fix LipSync
        FixRegularLipSync();
        
        // Fix connections between components
        FixComponentConnections();
        
        if (debugMode)
            Debug.Log("EnhancedVRMLipSyncFixer: Fixes applied");
    }
    
    /// <summary>
    /// Fix VRMLipSync specific issues
    /// </summary>
    private void FixVRMLipSync()
    {
        if (vrmLipSync == null)
        {
            if (debugMode)
                Debug.LogWarning("VRMLipSync component not found, creating one");
            
            vrmLipSync = gameObject.AddComponent<VRMLipSync>();
        }
        
        // Connect blend shape proxy
        if (blendShapeProxy != null)
        {
            vrmLipSync.BlendShapeProxy = blendShapeProxy;
        }
        else if (debugMode)
        {
            Debug.LogError("BlendShapeProxy not found, VRMLipSync will not work correctly");
        }
        
        // Connect audio source
        if (audioSource != null)
        {
            vrmLipSync.AudioSource = audioSource;
            
            if (debugMode)
                Debug.Log("Connected AudioSource to VRMLipSync");
        }
        else if (debugMode)
        {
            Debug.LogError("AudioSource not found, VRMLipSync will not have audio to analyze");
        }
        
        // Force VRMLipSync to use amplitude-based lip sync
        var ampBasedField = vrmLipSync.GetType().GetField("useAmplitudeBasedLipSync", 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic);
            
        if (ampBasedField != null)
        {
            ampBasedField.SetValue(vrmLipSync, true);
            
            if (debugMode)
                Debug.Log("Set VRMLipSync to use amplitude-based mode");
        }
        
        // Increase sensitivity for better visual effect
        var sensitivityField = vrmLipSync.GetType().GetField("lipSyncSensitivity", 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic);
            
        if (sensitivityField != null)
        {
            sensitivityField.SetValue(vrmLipSync, 5.0f); // Higher sensitivity
            
            if (debugMode)
                Debug.Log("Increased VRMLipSync sensitivity");
        }
    }
    
    /// <summary>
    /// Fix regular LipSync issues
    /// </summary>
    private void FixRegularLipSync()
    {
        // Find any LipSync components in the avatar
        LipSync regularLipSync = GetComponent<LipSync>();
        
        if (regularLipSync != null)
        {
            // Connect face renderer
            if (faceRenderer != null)
            {
                regularLipSync.SetFaceRenderer(faceRenderer);
                
                if (debugMode)
                    Debug.Log("Connected SkinnedMeshRenderer to LipSync");
            }
            
            // Connect audio source
            if (audioSource != null)
            {
                regularLipSync.SetAudioSource(audioSource);
                
                if (debugMode)
                    Debug.Log("Connected AudioSource to LipSync");
            }
            
            // Increase sensitivity
            regularLipSync.SetSensitivity(5.0f);
        }
    }
    
    /// <summary>
    /// Fix connections between AudioPlayback and AvatarController
    /// </summary>
    private void FixComponentConnections()
    {
        // Get references
        AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
        AvatarController avatarController = GetComponent<AvatarController>();
        
        if (audioPlayback != null && avatarController != null)
        {
            // Connect AvatarController to AudioPlayback
            var controllerField = audioPlayback.GetType().GetField("avatarController", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (controllerField != null)
            {
                controllerField.SetValue(audioPlayback, avatarController);
                
                if (debugMode)
                    Debug.Log("Connected AvatarController to AudioPlayback");
            }
            
            // Subscribe to events
            try
            {
                // Remove existing handlers to avoid duplicates
                audioPlayback.OnPlaybackStarted -= avatarController.OnAudioPlaybackStarted;
                audioPlayback.OnPlaybackCompleted -= avatarController.OnAudioPlaybackCompleted;
                audioPlayback.OnPlaybackProgress -= avatarController.UpdateLipSync;
                
                // Add handlers
                audioPlayback.OnPlaybackStarted += avatarController.OnAudioPlaybackStarted;
                audioPlayback.OnPlaybackCompleted += avatarController.OnAudioPlaybackCompleted;
                audioPlayback.OnPlaybackProgress += avatarController.UpdateLipSync;
                
                if (debugMode)
                    Debug.Log("Connected AudioPlayback events to AvatarController");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error connecting events: {ex.Message}");
            }
        }
        
        // Fix connections to existing VRMToAvatarBridge
        VRMToAvatarBridge bridge = GetComponent<VRMToAvatarBridge>();
        if (bridge != null)
        {
            bridge.FixConnections();
            
            if (debugMode)
                Debug.Log("Called FixConnections on VRMToAvatarBridge");
        }
    }
    
    /// <summary>
    /// Make multiple fix attempts over time
    /// </summary>
    private IEnumerator SequentialFixes()
    {
        foreach (float delay in fixDelays)
        {
            yield return new WaitForSeconds(delay);
            
            if (debugMode)
                Debug.Log($"EnhancedVRMLipSyncFixer: Running fix attempt after {delay}s");
            
            FindAllComponents();
            ApplyFixes();
        }
    }
    
    /// <summary>
    /// Force-reconnect audio source on enable
    /// </summary>
    private void OnEnable()
    {
        // Short delay to let components initialize
        Invoke("ReconnectAudioSource", 0.2f);
    }
    
    /// <summary>
    /// Specifically reconnect audio source
    /// </summary>
    private void ReconnectAudioSource()
    {
        // Find the main audio source
        AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
        if (audioPlayback != null)
        {
            audioSource = audioPlayback.GetComponent<AudioSource>();
            
            if (audioSource != null && vrmLipSync != null)
            {
                vrmLipSync.AudioSource = audioSource;
                
                if (debugMode)
                    Debug.Log("Reconnected AudioSource to VRMLipSync on enable");
            }
        }
    }
}