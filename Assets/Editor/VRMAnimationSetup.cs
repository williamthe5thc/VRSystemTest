using System;
using System.Collections;
using UnityEngine;
using VRM;
using VRInterview.Audio;

/// <summary>
/// Handles setup and initialization of VRM avatars for the interview system
/// </summary>
public class VRMAvatarSetup : MonoBehaviour
{
    [SerializeField] private GameObject vrmAvatarRoot;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioPlayback audioPlayback;
    [SerializeField] private AvatarController avatarController;
    
    [Header("VRM Components")]
    [SerializeField] private VRMBlendShapeProxy blendShapeProxy;
    [SerializeField] private Animator animator;
    
    [Header("System Creation")]
    [SerializeField] private bool createMissingComponents = true;
    [SerializeField] private bool replaceExistingComponents = false;
    
    private VRMLipSync lipSync;
    private VRMFacialExpressions facialExpressions;

    private void Start()
    {
        // Setup routine
        SetupVRMAvatar();
    }
    
    /// <summary>
    /// Sets up the VRM avatar with all required components
    /// </summary>
    public void SetupVRMAvatar()
    {
        // Step 1: Find the VRM model if not assigned
        if (vrmAvatarRoot == null)
        {
            var meta = FindObjectOfType<VRMMeta>();
            if (meta != null)
            {
                vrmAvatarRoot = meta.gameObject;
                Debug.Log($"Found VRM avatar: {vrmAvatarRoot.name}");
            }
            else
            {
                Debug.LogError("No VRM avatar found in the scene!");
                return;
            }
        }
        
        // Step 2: Find required VRM components
        if (blendShapeProxy == null)
        {
            blendShapeProxy = vrmAvatarRoot.GetComponent<VRMBlendShapeProxy>();
            
            if (blendShapeProxy == null)
            {
                blendShapeProxy = vrmAvatarRoot.GetComponentInChildren<VRMBlendShapeProxy>();
                
                if (blendShapeProxy == null)
                {
                    Debug.LogError("VRMBlendShapeProxy not found on VRM avatar!");
                    return;
                }
            }
        }
        
        if (animator == null)
        {
            animator = vrmAvatarRoot.GetComponent<Animator>();
            
            if (animator == null)
            {
                animator = vrmAvatarRoot.GetComponentInChildren<Animator>();
                
                if (animator == null)
                {
                    Debug.LogError("Animator not found on VRM avatar!");
                    return;
                }
            }
        }
        
        // Step 3: Find audio components
        if (audioPlayback == null)
        {
            audioPlayback = FindObjectOfType<AudioPlayback>();
            
            if (audioPlayback == null)
            {
                Debug.LogWarning("AudioPlayback not found in the scene!");
            }
        }
        
        if (audioSource == null && audioPlayback != null)
        {
            audioSource = audioPlayback.GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                Debug.LogWarning("AudioSource not found on AudioPlayback!");
            }
        }
        
        // Step 4: Set up VRM-specific components
        SetupVRMLipSync();
        SetupVRMFacialExpressions();
        SetupAvatarController();
        
        Debug.Log("VRM avatar setup complete");
    }
    
    /// <summary>
    /// Sets up the VRM lip sync component
    /// </summary>
    private void SetupVRMLipSync()
    {
        // Check if we have an existing component
        lipSync = vrmAvatarRoot.GetComponent<VRMLipSync>();
        
        // Create if missing and allowed
        if (lipSync == null && createMissingComponents)
        {
            lipSync = vrmAvatarRoot.AddComponent<VRMLipSync>();
            Debug.Log("Added VRMLipSync component to avatar");
        }
        else if (lipSync != null && replaceExistingComponents)
        {
            DestroyImmediate(lipSync);
            lipSync = vrmAvatarRoot.AddComponent<VRMLipSync>();
            Debug.Log("Replaced existing LipSync with VRMLipSync");
        }
        
        // Configure the component
        if (lipSync != null)
        {
            // Set references via reflection to avoid requiring serialized field
            var audioSourceField = lipSync.GetType().GetField("audioSource", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                
            var blendShapeProxyField = lipSync.GetType().GetField("blendShapeProxy", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
            if (audioSourceField != null && audioSource != null)
            {
                audioSourceField.SetValue(lipSync, audioSource);
            }
            
            if (blendShapeProxyField != null && blendShapeProxy != null)
            {
                blendShapeProxyField.SetValue(lipSync, blendShapeProxy);
            }
        }
    }
    
    /// <summary>
    /// Sets up the VRM facial expressions component
    /// </summary>
    private void SetupVRMFacialExpressions()
    {
        // Check if we have an existing component
        facialExpressions = vrmAvatarRoot.GetComponent<VRMFacialExpressions>();
        
        // Create if missing and allowed
        if (facialExpressions == null && createMissingComponents)
        {
            facialExpressions = vrmAvatarRoot.AddComponent<VRMFacialExpressions>();
            Debug.Log("Added VRMFacialExpressions component to avatar");
        }
        else if (facialExpressions != null && replaceExistingComponents)
        {
            DestroyImmediate(facialExpressions);
            facialExpressions = vrmAvatarRoot.AddComponent<VRMFacialExpressions>();
            Debug.Log("Replaced existing FacialExpressions with VRMFacialExpressions");
        }
        
        // Configure the component
        if (facialExpressions != null)
        {
            // Set references via reflection
            var blendShapeProxyField = facialExpressions.GetType().GetField("blendShapeProxy", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                
            if (blendShapeProxyField != null && blendShapeProxy != null)
            {
                blendShapeProxyField.SetValue(facialExpressions, blendShapeProxy);
            }
        }
    }
    
    /// <summary>
    /// Sets up the avatar controller to work with VRM components
    /// </summary>
    private void SetupAvatarController()
    {
        // Find or create AvatarController
        if (avatarController == null)
        {
            avatarController = vrmAvatarRoot.GetComponent<AvatarController>();
            
            if (avatarController == null && createMissingComponents)
            {
                avatarController = vrmAvatarRoot.AddComponent<AvatarController>();
                Debug.Log("Added AvatarController to VRM avatar");
            }
        }
        
        // Configure the avatar controller
        if (avatarController != null)
        {
            // Set references via reflection
            var animatorField = avatarController.GetType().GetField("animator", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                
            var lipSyncField = avatarController.GetType().GetField("lipSync", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                
            var facialExpressionsField = avatarController.GetType().GetField("facialExpressions", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
            if (animatorField != null && animator != null)
            {
                animatorField.SetValue(avatarController, animator);
            }
            
            if (lipSyncField != null && lipSync != null)
            {
                lipSyncField.SetValue(avatarController, lipSync);
            }
            
            if (facialExpressionsField != null && facialExpressions != null)
            {
                facialExpressionsField.SetValue(avatarController, facialExpressions);
            }
        }
    }
}