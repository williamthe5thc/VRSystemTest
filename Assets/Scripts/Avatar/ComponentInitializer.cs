using System.Collections;
using UnityEngine;
using VRM;

namespace VRInterview
{
    /// <summary>
    /// Ensures correct initialization order for audio and lip sync components.
    /// </summary>
    public class ComponentInitializer : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private AudioPlayback audioPlayback;
        [SerializeField] private AudioPlaybackFix audioPlaybackFix;
        [SerializeField] private VRMSetupFixer vrmSetupFixer;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private VRMBlendShapeProxy blendShapeProxy;
        [SerializeField] private VRMLipSync vrmLipSync;
        
        [Header("Fix Settings")]
        [SerializeField] private float initialDelay = 0.5f;
        [SerializeField] private float secondAttemptDelay = 2.0f;
        [SerializeField] private bool forceNonSpatialAudio = true;
        [SerializeField] private float minimumVolume = 0.8f;
        [SerializeField] private bool fixOnStart = true;
        
        private bool _fixesApplied = false;
        
        private void Start()
        {
            if (fixOnStart)
            {
                StartCoroutine(InitializeInOrder());
            }
        }
        
        private IEnumerator InitializeInOrder()
        {
            // First delay to let everything load
            yield return new WaitForSeconds(initialDelay);
            
            // Find components if not assigned
            FindAllComponents();
            
            // Configure audio source
            ConfigureAudioSource();
            
            // Apply VRM fixes first
            ApplyVRMFixes();
            
            // Then apply audio fixes
            ApplyAudioFixes();
            
            // Connect components
            ConnectComponents();
            
            // Mark first round complete
            _fixesApplied = true;
            
            Debug.Log("ComponentInitializer: First round of fixes applied");
            
            // Second attempt after delay
            yield return new WaitForSeconds(secondAttemptDelay);
            
            // Find components again in case any were added after first attempt
            FindAllComponents();
            
            // Reapply fixes
            ApplyVRMFixes();
            ApplyAudioFixes();
            ConnectComponents();
            
            Debug.Log("ComponentInitializer: Second round of fixes applied");
        }
        
        private void FindAllComponents()
        {
            // Find AudioPlayback if not assigned
            if (audioPlayback == null)
            {
                audioPlayback = FindObjectOfType<AudioPlayback>();
                if (audioPlayback != null)
                {
                    Debug.Log("ComponentInitializer: Found AudioPlayback component");
                }
            }
            
            // Find AudioPlaybackFix if not assigned
            if (audioPlaybackFix == null)
            {
                audioPlaybackFix = FindObjectOfType<AudioPlaybackFix>();
                if (audioPlaybackFix != null)
                {
                    Debug.Log("ComponentInitializer: Found AudioPlaybackFix component");
                }
            }
            
            // Find VRMSetupFixer if not assigned
            if (vrmSetupFixer == null)
            {
                vrmSetupFixer = FindObjectOfType<VRMSetupFixer>();
                if (vrmSetupFixer != null)
                {
                    Debug.Log("ComponentInitializer: Found VRMSetupFixer component");
                }
            }
            
            // Find BlendShapeProxy if not assigned
            if (blendShapeProxy == null)
            {
                blendShapeProxy = FindObjectOfType<VRMBlendShapeProxy>();
                if (blendShapeProxy != null)
                {
                    Debug.Log("ComponentInitializer: Found VRMBlendShapeProxy component");
                }
            }
            
            // Find VRMLipSync if not assigned
            if (vrmLipSync == null)
            {
                vrmLipSync = FindObjectOfType<VRMLipSync>();
                if (vrmLipSync != null)
                {
                    Debug.Log("ComponentInitializer: Found VRMLipSync component");
                }
            }
            
            // Ensure AudioSource is properly found or created
            if (audioSource == null && audioPlayback != null)
            {
                audioSource = audioPlayback.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = audioPlayback.gameObject.AddComponent<AudioSource>();
                    Debug.Log("ComponentInitializer: Created AudioSource component");
                }
                else
                {
                    Debug.Log("ComponentInitializer: Found AudioSource component");
                }
            }
        }
        
        private void ConfigureAudioSource()
        {
            if (audioSource == null) return;
            
            // Configure for reliable audio
            audioSource.playOnAwake = false;
            if (forceNonSpatialAudio)
            {
                audioSource.spatialBlend = 0f;
            }
            if (audioSource.volume < minimumVolume)
            {
                audioSource.volume = minimumVolume;
            }
            audioSource.priority = 0;
            
            // Ensure not muted
            if (audioSource.mute)
            {
                audioSource.mute = false;
            }
            
            Debug.Log($"ComponentInitializer: Configured AudioSource (volume={audioSource.volume}, spatialBlend={audioSource.spatialBlend})");
        }
        
        private void ApplyVRMFixes()
        {
            if (vrmSetupFixer != null)
            {
                vrmSetupFixer.FixVRMSetup();
                Debug.Log("ComponentInitializer: Applied VRMSetupFixer");
            }
            else
            {
                Debug.LogWarning("ComponentInitializer: VRMSetupFixer not found, skipping VRM fixes");
            }
        }
        
        private void ApplyAudioFixes()
        {
            if (audioPlaybackFix != null)
            {
                // Make sure required settings are enabled
                try
                {
                    var fixPlaybackIssues = audioPlaybackFix.GetType().GetField("fixPlaybackIssues");
                    var forceNonSpatialAudio = audioPlaybackFix.GetType().GetField("forceNonSpatialAudio");
                    
                    if (fixPlaybackIssues != null)
                    {
                        fixPlaybackIssues.SetValue(audioPlaybackFix, true);
                    }
                    
                    if (forceNonSpatialAudio != null)
                    {
                        forceNonSpatialAudio.SetValue(audioPlaybackFix, true);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"ComponentInitializer: Failed to set AudioPlaybackFix properties: {ex.Message}");
                }
                
                // Apply fixes
                audioPlaybackFix.FixNow();
                Debug.Log("ComponentInitializer: Applied AudioPlaybackFix");
            }
            else
            {
                Debug.LogWarning("ComponentInitializer: AudioPlaybackFix not found, skipping audio fixes");
            }
        }
        
        private void ConnectComponents()
        {
            // Connect VRMLipSync to AudioSource
            if (vrmLipSync != null && audioSource != null)
            {
                vrmLipSync.AudioSource = audioSource;
                Debug.Log("ComponentInitializer: Connected AudioSource to VRMLipSync");
            }
            
            // Connect VRMLipSync to BlendShapeProxy
            if (vrmLipSync != null && blendShapeProxy != null)
            {
                vrmLipSync.BlendShapeProxy = blendShapeProxy;
                Debug.Log("ComponentInitializer: Connected BlendShapeProxy to VRMLipSync");
            }
            
            // Connect VRMToAvatarBridge if available
            VRMToAvatarBridge bridge = FindObjectOfType<VRMToAvatarBridge>();
            if (bridge != null)
            {
                bridge.FixConnections();
                Debug.Log("ComponentInitializer: Fixed VRMToAvatarBridge connections");
            }
            
            // Find AvatarController and connect to AudioPlayback
            AvatarController avatarController = FindObjectOfType<AvatarController>();
            if (avatarController != null && audioPlayback != null)
            {
                try
                {
                    // Use reflection to set avatarController field in AudioPlayback
                    var field = audioPlayback.GetType().GetField("avatarController",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic);
                        
                    if (field != null)
                    {
                        field.SetValue(audioPlayback, avatarController);
                        Debug.Log("ComponentInitializer: Connected AvatarController to AudioPlayback");
                    }
                    
                    // Ensure event handlers are connected
                    audioPlayback.OnPlaybackStarted -= avatarController.OnAudioPlaybackStarted;
                    audioPlayback.OnPlaybackCompleted -= avatarController.OnAudioPlaybackCompleted;
                    
                    audioPlayback.OnPlaybackStarted += avatarController.OnAudioPlaybackStarted;
                    audioPlayback.OnPlaybackCompleted += avatarController.OnAudioPlaybackCompleted;
                    
                    Debug.Log("ComponentInitializer: Connected AudioPlayback events to AvatarController");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"ComponentInitializer: Failed to connect AvatarController: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Call this method manually to apply fixes at any time.
        /// </summary>
        public void ApplyFixesNow()
        {
            StopAllCoroutines();
            FindAllComponents();
            ConfigureAudioSource();
            ApplyVRMFixes();
            ApplyAudioFixes();
            ConnectComponents();
            
            Debug.Log("ComponentInitializer: Manual fixes applied");
        }
    }
}
