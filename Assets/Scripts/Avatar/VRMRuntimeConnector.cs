using UnityEngine;
using VRM;
using UnityEditor.Animations;

/// <summary>
/// Runtime connector that ensures all VRM components are properly connected at startup
/// Add this to your avatar GameObject to automatically configure VRM components
/// </summary>
public class VRMRuntimeConnector : MonoBehaviour
{
    [SerializeField] private bool connectOnStart = true;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool createMissingComponents = true;
    
    private void Start()
    {
        if (connectOnStart)
        {
            // Add a small delay to ensure all components are initialized
            Invoke("ConnectVRMComponents", 0.1f);
        }
    }
    
    /// <summary>
    /// Call this method to manually connect all VRM components
    /// </summary>
    public void ConnectVRMComponents()
    {
        if (debugMode)
        {
            Debug.Log("VRMRuntimeConnector: Connecting VRM components...");
        }
        
        // Get all key components
        GameObject avatarRoot = gameObject;
        
        // Find BlendShapeProxy
        VRMBlendShapeProxy blendShapeProxy = avatarRoot.GetComponent<VRMBlendShapeProxy>();
        if (blendShapeProxy == null)
        {
            blendShapeProxy = avatarRoot.GetComponentInChildren<VRMBlendShapeProxy>();
            
            if (blendShapeProxy == null)
            {
                Debug.LogError("VRMBlendShapeProxy not found! VRM components will not function correctly.");
                return;
            }
        }
        
        // Get components (we won't create them here to avoid duplicates)
        VRMLipSync lipSync = avatarRoot.GetComponent<VRMLipSync>();
        VRMFacialExpressions facialExpressions = avatarRoot.GetComponent<VRMFacialExpressions>();
        VRMAvatarAdapter avatarAdapter = avatarRoot.GetComponent<VRMAvatarAdapter>();
        AvatarController avatarController = avatarRoot.GetComponent<AvatarController>();
        Animator animator = GetComponent<Animator>();
        
        // Add necessary components if missing
        if (createMissingComponents)
        {
            // Add AudioSource if needed
            AudioSource localAudioSource = avatarRoot.GetComponent<AudioSource>();
            if (localAudioSource == null)
            {
                localAudioSource = avatarRoot.AddComponent<AudioSource>();
                localAudioSource.playOnAwake = false;
                localAudioSource.loop = false;
                localAudioSource.spatialBlend = 1.0f; // For 3D sound
                localAudioSource.volume = 0.8f;
                
                if (debugMode)
                {
                    Debug.Log("Added AudioSource component to avatar");
                }
            }
            
            // Add or fix animator
            if (animator == null)
            {
                animator = avatarRoot.AddComponent<Animator>();
                if (debugMode)
                {
                    Debug.Log("Added Animator component to avatar");
                }
            }
            
            // Ensure skinned mesh renderer is assigned properly
            SkinnedMeshRenderer[] renderers = avatarRoot.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (renderers.Length > 0)
            {
                if (debugMode)
                {
                    Debug.Log($"Found {renderers.Length} SkinnedMeshRenderer components");
                }
            }
            else
            {
                Debug.LogWarning("No SkinnedMeshRenderer found in the avatar hierarchy!");
            }
        }
        
        if (avatarController == null)
        {
            avatarController = FindObjectOfType<AvatarController>();
        }
        
        // Find AudioSource
        AudioSource audioSource = null;
        AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
        if (audioPlayback != null)
        {
            audioSource = audioPlayback.GetComponent<AudioSource>();
        }
        
        // Connect VRMLipSync
        if (lipSync != null)
        {
            lipSync.BlendShapeProxy = blendShapeProxy;
            if (audioSource != null)
            {
                lipSync.AudioSource = audioSource;
            }
            
            if (debugMode)
            {
                Debug.Log("Connected BlendShapeProxy to VRMLipSync");
            }
        }
        
        // Connect VRMFacialExpressions
        if (facialExpressions != null)
        {
            facialExpressions.BlendShapeProxy = blendShapeProxy;
            
            if (debugMode)
            {
                Debug.Log("Connected BlendShapeProxy to VRMFacialExpressions");
            }
        }
        
        // Connect Adapter
        if (avatarAdapter != null)
        {
            avatarAdapter.VrmAvatarRoot = avatarRoot;
            avatarAdapter.BlendShapeProxy = blendShapeProxy;
            avatarAdapter.VrmLipSync = lipSync;
            avatarAdapter.VrmFacialExpressions = facialExpressions;
            
            if (avatarController != null)
            {
                avatarAdapter.AvatarController = avatarController;
            }
            
            if (audioSource != null)
            {
                avatarAdapter.AudioSource = audioSource;
            }
            
            if (debugMode)
            {
                Debug.Log("Connected all components to VRMAvatarAdapter");
            }
        }
        
        // Connect component events
        if (audioPlayback != null && avatarController != null)
        {
            // Add callbacks for audio playback
            audioPlayback.OnPlaybackStarted += avatarController.OnAudioPlaybackStarted;
            audioPlayback.OnPlaybackCompleted += avatarController.OnAudioPlaybackCompleted;
            audioPlayback.OnPlaybackProgress += avatarController.UpdateLipSync;
            
            if (debugMode)
            {
                Debug.Log("Connected AudioPlayback events to AvatarController");
            }
        }
        
        // Fix Animator setup if needed
        if (animator != null)
        {
            if (animator.runtimeAnimatorController == null)
            {
                // First try to load from Resources
                RuntimeAnimatorController animCtrl = Resources.Load<RuntimeAnimatorController>("InterviewerAnimator");
                
                if (animCtrl == null)
                {
                    // Try to find any animator controller in the project as a fallback
                    RuntimeAnimatorController[] controllers = Resources.FindObjectsOfTypeAll<RuntimeAnimatorController>();
                    foreach (var foundController in controllers)
                    {
                        if (!(foundController is AnimatorOverrideController))
                        {
                            animCtrl = foundController;
                            break;
                        }
                    }
                }
                
                if (animCtrl != null)
                {
                    animator.runtimeAnimatorController = animCtrl;
                    if (debugMode)
                    {
                        Debug.Log($"Assigned animator controller: {animCtrl.name}");
                    }
                }
                else
                {
                    Debug.LogWarning("Could not find a valid animator controller to assign!");
                }
            }
            
            // Ensure avatar is correctly configured
            if (animator.avatar == null)
            {
                // Try to find an avatar
                Avatar[] avatars = Resources.FindObjectsOfTypeAll<Avatar>();
                if (avatars.Length > 0)
                {
                    animator.avatar = avatars[0];
                    if (debugMode)
                    {
                        Debug.Log($"Assigned avatar: {avatars[0].name}");
                    }
                }
            }
        }
        
        if (debugMode)
        {
            Debug.Log("VRM component connections complete!");
        }
    }
}