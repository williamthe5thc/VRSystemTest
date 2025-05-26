using System;
using UnityEngine;
using VRM;

/// <summary>
/// Adapter that bridges the existing avatar system with VRM avatar components
/// </summary>
public class VRMAvatarAdapter : MonoBehaviour
{
    [SerializeField] private AvatarController avatarController;
    
    // Property accessor for external access
    public AvatarController AvatarController {
        get { return avatarController; }
        set { avatarController = value; }
    }
    [SerializeField] private GameObject vrmAvatarRoot;
    
    // Property accessor for external access
    public GameObject VrmAvatarRoot {
        get { return vrmAvatarRoot; }
        set { vrmAvatarRoot = value; }
    }
    
    // VRM components
    [SerializeField] private VRMLipSync vrmLipSync;
    
    // Property accessor for external access
    public VRMLipSync VrmLipSync {
        get { return vrmLipSync; }
        set { vrmLipSync = value; }
    }
    [SerializeField] private VRMFacialExpressions vrmFacialExpressions;
    
    // Property accessor for external access
    public VRMFacialExpressions VrmFacialExpressions {
        get { return vrmFacialExpressions; }
        set { vrmFacialExpressions = value; }
    }
    [SerializeField] private VRMBlendShapeProxy blendShapeProxy;
    
    // Property accessor for external access
    public VRMBlendShapeProxy BlendShapeProxy {
        get { return blendShapeProxy; }
        set { blendShapeProxy = value; }
    }
    
    // Audio components
    [SerializeField] private AudioSource audioSource;
    
    // Property accessor for external access
    public AudioSource AudioSource {
        get { return audioSource; }
        set { audioSource = value; }
    }
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private void Awake()
    {
        if (debugMode)
        {
            Debug.Log("VRMAvatarAdapter initializing...");
        }
    }
    
    private void Start()
    {
        // Find components
        FindComponents();
        
        // Connect to the main avatar controller
        if (avatarController != null)
        {
            avatarController.OnStateChanged += HandleAvatarStateChanged;
            
            // Add callbacks for audio playback
            AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
            if (audioPlayback != null)
            {
                audioPlayback.OnPlaybackStarted += OnAudioPlaybackStarted;
                audioPlayback.OnPlaybackCompleted += OnAudioPlaybackCompleted;
                audioPlayback.OnPlaybackProgress += UpdateLipSync;
                
                if (debugMode)
                {
                    Debug.Log("Connected to AudioPlayback events");
                }
            }
            else
            {
                Debug.LogWarning("AudioPlayback not found. Lip sync will not be controlled by audio playback.");
            }
        }
        else
        {
            Debug.LogError("AvatarController not found for VRMAvatarAdapter!");
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (avatarController != null)
        {
            avatarController.OnStateChanged -= HandleAvatarStateChanged;
        }
        
        AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
        if (audioPlayback != null)
        {
            audioPlayback.OnPlaybackStarted -= OnAudioPlaybackStarted;
            audioPlayback.OnPlaybackCompleted -= OnAudioPlaybackCompleted;
            audioPlayback.OnPlaybackProgress -= UpdateLipSync;
        }
    }
    
    private void FindComponents()
    {
        // Find VRM avatar if not assigned
        if (vrmAvatarRoot == null)
        {
            vrmAvatarRoot = gameObject;
        }
        
        if (avatarController == null)
        {
            avatarController = GetComponent<AvatarController>();
            
            if (avatarController == null)
            {
                avatarController = GetComponentInParent<AvatarController>();
                
                if (avatarController == null)
                {
                    Debug.LogError("AvatarController not found! The adapter will not function correctly.");
                }
            }
        }
        
        // Find VRM components
        if (vrmLipSync == null)
        {
            vrmLipSync = GetComponent<VRMLipSync>();
            
            if (vrmLipSync == null && vrmAvatarRoot != null)
            {
                vrmLipSync = vrmAvatarRoot.GetComponent<VRMLipSync>();
                
                if (vrmLipSync == null)
                {
                    Debug.LogWarning("VRMLipSync component not found!");
                }
            }
        }
        
        if (vrmFacialExpressions == null)
        {
            vrmFacialExpressions = GetComponent<VRMFacialExpressions>();
            
            if (vrmFacialExpressions == null && vrmAvatarRoot != null)
            {
                vrmFacialExpressions = vrmAvatarRoot.GetComponent<VRMFacialExpressions>();
                
                if (vrmFacialExpressions == null)
                {
                    Debug.LogWarning("VRMFacialExpressions component not found!");
                }
            }
        }
        
        if (blendShapeProxy == null)
        {
            blendShapeProxy = GetComponent<VRMBlendShapeProxy>();
            
            if (blendShapeProxy == null && vrmAvatarRoot != null)
            {
                blendShapeProxy = vrmAvatarRoot.GetComponent<VRMBlendShapeProxy>();
                
                if (blendShapeProxy == null)
                {
                    Debug.LogWarning("VRMBlendShapeProxy component not found!");
                }
            }
        }
        
        // Find audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
                if (audioPlayback != null)
                {
                    audioSource = audioPlayback.GetComponent<AudioSource>();
                }
                
                if (audioSource == null)
                {
                    Debug.LogWarning("AudioSource not found!");
                }
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"VRMAvatarAdapter found components: " +
                $"AvatarController: {(avatarController != null ? "Yes" : "No")}, " +
                $"VRMLipSync: {(vrmLipSync != null ? "Yes" : "No")}, " +
                $"VRMFacialExpressions: {(vrmFacialExpressions != null ? "Yes" : "No")}, " +
                $"BlendShapeProxy: {(blendShapeProxy != null ? "Yes" : "No")}, " +
                $"AudioSource: {(audioSource != null ? "Yes" : "No")}");
        }
    }
    
    /// <summary>
    /// Handles state changes from the avatar controller
    /// </summary>
    private void HandleAvatarStateChanged(string state)
    {
        if (debugMode)
        {
            Debug.Log($"VRMAvatarAdapter: State changed to {state}");
        }
        
        // Apply appropriate facial expression based on state
        if (vrmFacialExpressions != null)
        {
            switch (state)
            {
                case "IDLE":
                    vrmFacialExpressions.SetExpression(FacialExpression.Neutral);
                    break;
                case "LISTENING":
                    vrmFacialExpressions.SetExpression(FacialExpression.Interested);
                    break;
                case "PROCESSING":
                    vrmFacialExpressions.SetExpression(FacialExpression.Thoughtful);
                    break;
                case "RESPONDING":
                    vrmFacialExpressions.SetExpression(FacialExpression.Talking);
                    
                    // Start lip sync if we're in responding state
                    if (vrmLipSync != null)
                    {
                        vrmLipSync.StartLipSync();
                        
                        if (debugMode)
                        {
                            Debug.Log("Started lip sync in response to RESPONDING state");
                        }
                    }
                    break;
                case "WAITING":
                    vrmFacialExpressions.SetExpression(FacialExpression.Attentive);
                    break;
                case "ERROR":
                    vrmFacialExpressions.SetExpression(FacialExpression.Confused);
                    break;
            }
        }
        
        // Deactivate lip sync for non-speaking states
        if (vrmLipSync != null && state != "RESPONDING")
        {
            vrmLipSync.StopLipSync();
            
            if (debugMode && state != "IDLE")
            {
                Debug.Log($"Stopped lip sync in response to {state} state");
            }
        }
    }
    
    /// <summary>
    /// Called when audio playback starts
    /// </summary>
    public void OnAudioPlaybackStarted()
    {
        if (vrmLipSync != null)
        {
            vrmLipSync.StartLipSync();
            
            if (debugMode)
            {
                Debug.Log("Started lip sync in response to audio playback start");
            }
        }
        
        if (vrmFacialExpressions != null)
        {
            vrmFacialExpressions.SetExpression(FacialExpression.Talking);
        }
    }
    
    /// <summary>
    /// Called when audio playback completes
    /// </summary>
    public void OnAudioPlaybackCompleted()
    {
        if (vrmLipSync != null)
        {
            vrmLipSync.StopLipSync();
            
            if (debugMode)
            {
                Debug.Log("Stopped lip sync in response to audio playback completion");
            }
        }
        
        if (vrmFacialExpressions != null && avatarController != null)
        {
            // Set expression based on current avatar state
            string currentState = avatarController.GetCurrentState();
            if (currentState == "RESPONDING")
            {
                // If we're still in RESPONDING state, transition to WAITING
                vrmFacialExpressions.SetExpression(FacialExpression.Attentive);
            }
        }
    }
    
    /// <summary>
    /// Updates lip sync during audio playback
    /// </summary>
    public void UpdateLipSync(float normalizedTime)
    {
        if (vrmLipSync != null)
        {
            vrmLipSync.UpdateLipSyncValue(normalizedTime);
        }
    }
    
    /// <summary>
    /// Sets a specific facial expression
    /// </summary>
    public void SetFacialExpression(FacialExpression expression)
    {
        if (vrmFacialExpressions != null)
        {
            vrmFacialExpressions.SetExpression(expression);
        }
    }
}