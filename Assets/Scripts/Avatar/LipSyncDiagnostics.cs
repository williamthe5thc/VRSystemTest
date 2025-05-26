using System;
using System.Linq;
using UnityEngine;
using VRM;

/// <summary>
/// Diagnostic tool to troubleshoot lip sync issues in the VR Interview System
/// </summary>
public class LipSyncDiagnostics : MonoBehaviour
{
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private bool monitorAudioData = true;
    [SerializeField] private float testLipSyncFrequency = 2.0f;
    [SerializeField] private float testLipSyncIntensity = 1.0f;
    
    private GameObject _avatarRoot;
    private AudioPlayback _audioPlayback;
    private AudioSource _audioSource;
    private AvatarController _avatarController;
    private LipSync _lipSync;
    private VRMLipSync _vrmLipSync;
    private VRMBlendShapeProxy _blendShapeProxy;
    private VRMToAvatarBridge _avatarBridge;
    private SkinnedMeshRenderer _faceRenderer;
    
    private float _lastPlaybackValue = 0f;
    private bool _isAudioPlaying = false;
    private bool _isLipSyncActive = false;
    private bool _testModeActive = false;

    private void Start()
    {
        if (runOnStart)
        {
            Invoke("RunDiagnostics", 1.0f);
        }
        
        if (monitorAudioData)
        {
            InvokeRepeating("MonitorAudio", 0.5f, 0.2f);
        }
    }
    
    /// <summary>
    /// Run diagnostics on the lip sync system
    /// </summary>
    public void RunDiagnostics()
    {
        Debug.Log("=== LIP SYNC DIAGNOSTICS ===");
        
        FindComponents();
        CheckAvatarComponents();
        CheckAudioComponents();
        CheckLipSyncComponents();
        CheckVRMComponents();
        CheckConnections();
        
        Debug.Log("=== DIAGNOSTICS COMPLETE ===");
    }
    
    /// <summary>
    /// Find all relevant components for diagnosis
    /// </summary>
    private void FindComponents()
    {
        Debug.Log("--- Finding Components ---");
        
        // Find avatar root
        if (_avatarRoot == null)
        {
            _avatarRoot = gameObject;
            Debug.Log($"Using current GameObject as avatar root: {_avatarRoot.name}");
            
            // Find alternative if this doesn't have VRM components
            if (_avatarRoot.GetComponent<VRMBlendShapeProxy>() == null && 
                _avatarRoot.GetComponentInChildren<VRMBlendShapeProxy>() == null)
            {
                VRMBlendShapeProxy blendShapeProxy = FindObjectOfType<VRMBlendShapeProxy>();
                if (blendShapeProxy != null)
                {
                    _avatarRoot = blendShapeProxy.gameObject;
                    Debug.Log($"Found better avatar root via BlendShapeProxy: {_avatarRoot.name}");
                }
            }
        }
        
        // Find components on avatar
        if (_avatarRoot != null)
        {
            _avatarController = _avatarRoot.GetComponent<AvatarController>();
            if (_avatarController != null)
            {
                Debug.Log("✓ Found AvatarController");
            }
            
            _lipSync = _avatarRoot.GetComponent<LipSync>();
            if (_lipSync != null)
            {
                Debug.Log("✓ Found LipSync");
            }
            
            _vrmLipSync = _avatarRoot.GetComponent<VRMLipSync>();
            if (_vrmLipSync != null)
            {
                Debug.Log("✓ Found VRMLipSync");
            }
            
            _blendShapeProxy = _avatarRoot.GetComponent<VRMBlendShapeProxy>();
            if (_blendShapeProxy != null)
            {
                Debug.Log("✓ Found VRMBlendShapeProxy");
            }
            
            _avatarBridge = _avatarRoot.GetComponent<VRMToAvatarBridge>();
            if (_avatarBridge != null)
            {
                Debug.Log("✓ Found VRMToAvatarBridge");
            }
            
            _faceRenderer = _avatarRoot.GetComponentInChildren<SkinnedMeshRenderer>();
            if (_faceRenderer != null)
            {
                Debug.Log($"✓ Found SkinnedMeshRenderer: {_faceRenderer.name}");
            }
        }
        
        // Find audio playback
        _audioPlayback = FindObjectOfType<AudioPlayback>();
        if (_audioPlayback != null)
        {
            Debug.Log($"✓ Found AudioPlayback: {_audioPlayback.name}");
            _audioSource = _audioPlayback.GetComponent<AudioSource>();
            if (_audioSource != null)
            {
                Debug.Log("✓ Found AudioSource on AudioPlayback");
            }
        }
    }
    
    /// <summary>
    /// Check avatar base components
    /// </summary>
    private void CheckAvatarComponents()
    {
        Debug.Log("--- Avatar Components ---");
        
        if (_avatarRoot == null)
        {
            Debug.LogError("❌ No avatar root found!");
            return;
        }
        
        // Check avatar controller
        if (_avatarController == null)
        {
            Debug.LogWarning("⚠ AvatarController missing!");
        }
        else
        {
            // Check animator connection
            Animator animator = _avatarRoot.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("⚠ Animator component missing!");
            }
            else
            {
                // Check if animator has controller
                if (animator.runtimeAnimatorController == null)
                {
                    Debug.LogWarning("⚠ Animator has no controller!");
                }
                else
                {
                    Debug.Log($"✓ Animator controller: {animator.runtimeAnimatorController.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// Check audio components
    /// </summary>
    private void CheckAudioComponents()
    {
        Debug.Log("--- Audio Components ---");
        
        if (_audioPlayback == null)
        {
            Debug.LogError("❌ AudioPlayback component not found!");
            return;
        }
        
        if (_audioSource == null)
        {
            Debug.LogWarning("⚠ AudioSource component missing from AudioPlayback!");
        }
        else
        {
            // Check audio source settings
            Debug.Log($"✓ AudioSource settings: volume={_audioSource.volume}, " +
                $"spatialBlend={_audioSource.spatialBlend}, " +
                $"isPlaying={_audioSource.isPlaying}");
                
            // Check clip
            if (_audioSource.clip != null)
            {
                Debug.Log($"✓ Current clip: {_audioSource.clip.name}, " +
                    $"length={_audioSource.clip.length:F2}s");
            }
            else
            {
                Debug.Log("ℹ No audio clip currently assigned");
            }
            
            // Check mute/pause
            if (_audioSource.mute)
            {
                Debug.LogWarning("⚠ AudioSource is muted!");
            }
            
            if (_audioSource.isPlaying && _audioSource.time > 0 && _audioSource.volume <= 0.01f)
            {
                Debug.LogWarning("⚠ AudioSource volume is too low (≤0.01)!");
            }
        }
    }
    
    /// <summary>
    /// Check standard lip sync components
    /// </summary>
    private void CheckLipSyncComponents()
    {
        Debug.Log("--- LipSync Components ---");
        
        if (_lipSync == null)
        {
            Debug.LogWarning("⚠ LipSync component not found!");
            return;
        }
        
        // Check faceRenderer connection
        try
        {
            var faceRendererField = _lipSync.GetType().GetField("faceRenderer", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
            
            if (faceRendererField != null)
            {
                SkinnedMeshRenderer connectedRenderer = faceRendererField.GetValue(_lipSync) as SkinnedMeshRenderer;
                if (connectedRenderer == null)
                {
                    Debug.LogWarning("⚠ LipSync's faceRenderer field is null!");
                }
                else
                {
                    Debug.Log($"✓ LipSync has valid faceRenderer: {connectedRenderer.name}");
                    
                    // Check if renderer has a valid mesh
                    if (connectedRenderer.sharedMesh == null)
                    {
                        Debug.LogWarning("⚠ SkinnedMeshRenderer has no shared mesh!");
                    }
                    else
                    {
                        // Check if mesh has blend shapes
                        int blendShapeCount = connectedRenderer.sharedMesh.blendShapeCount;
                        Debug.Log($"✓ Mesh has {blendShapeCount} blend shapes");
                        
                        if (blendShapeCount == 0)
                        {
                            Debug.LogWarning("⚠ Mesh has no blend shapes for lip sync!");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error checking faceRenderer field: {ex.Message}");
        }
        
        // Check audioSource connection
        try
        {
            var audioSourceField = _lipSync.GetType().GetField("audioSource", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic);
            
            if (audioSourceField != null)
            {
                AudioSource connectedSource = audioSourceField.GetValue(_lipSync) as AudioSource;
                if (connectedSource == null)
                {
                    Debug.LogWarning("⚠ LipSync's audioSource field is null!");
                }
                else
                {
                    Debug.Log($"✓ LipSync has valid audioSource: {connectedSource.name}");
                    
                    // Check if it's the same as the main audio source
                    if (_audioSource != null && connectedSource != _audioSource)
                    {
                        Debug.LogWarning("⚠ LipSync is using a different AudioSource than AudioPlayback!");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Error checking audioSource field: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Check VRM-specific components
    /// </summary>
    private void CheckVRMComponents()
    {
        Debug.Log("--- VRM Components ---");
        
        if (_vrmLipSync == null && _blendShapeProxy == null)
        {
            Debug.LogWarning("⚠ No VRM lip sync components found!");
            return;
        }
        
        // Check BlendShapeProxy
        if (_blendShapeProxy != null)
        {
            Debug.Log("✓ Found VRMBlendShapeProxy");
            
            try
            {
                BlendShapeAvatar blendShapeAvatar = _blendShapeProxy.BlendShapeAvatar;
                if (blendShapeAvatar != null)
                {
                    BlendShapeClip[] clips = blendShapeAvatar.Clips.ToArray();
                    Debug.Log($"✓ VRM has {clips.Length} blend shape clips");
                    
                    // Check if important presets exist
                    bool hasA = false;
                    bool hasO = false;
                    
                    foreach (var clip in clips)
                    {
                        if (clip == null) continue;
                        
                        if (clip.Preset == BlendShapePreset.A)
                        {
                            hasA = true;
                        }
                        else if (clip.Preset == BlendShapePreset.O)
                        {
                            hasO = true;
                        }
                    }
                    
                    if (!hasA)
                    {
                        Debug.LogWarning("⚠ VRM is missing 'A' blend shape preset which is essential for lip sync!");
                    }
                    
                    if (!hasO)
                    {
                        Debug.LogWarning("⚠ VRM is missing 'O' blend shape preset which is useful for lip sync!");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠ VRMBlendShapeProxy has no BlendShapeAvatar!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error checking BlendShapeProxy: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("⚠ VRMBlendShapeProxy not found!");
        }
        
        // Check VRMLipSync
        if (_vrmLipSync != null)
        {
            Debug.Log("✓ Found VRMLipSync");
            
            // Check BlendShapeProxy connection
            if (_vrmLipSync.BlendShapeProxy == null)
            {
                Debug.LogWarning("⚠ VRMLipSync has no BlendShapeProxy connection!");
            }
            else
            {
                Debug.Log("✓ VRMLipSync has valid BlendShapeProxy connection");
            }
            
            // Check AudioSource connection
            if (_vrmLipSync.AudioSource == null)
            {
                Debug.LogWarning("⚠ VRMLipSync has no AudioSource connection!");
            }
            else
            {
                Debug.Log("✓ VRMLipSync has valid AudioSource connection");
                
                // Check if it's the same as the main audio source
                if (_audioSource != null && _vrmLipSync.AudioSource != _audioSource)
                {
                    Debug.LogWarning("⚠ VRMLipSync is using a different AudioSource than AudioPlayback!");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠ VRMLipSync not found!");
        }
    }
    
    /// <summary>
    /// Check connections between components
    /// </summary>
    private void CheckConnections()
    {
        Debug.Log("--- Component Connections ---");
        
        // Check AvatarController to LipSync connection
        if (_avatarController != null && _lipSync != null)
        {
            try
            {
                var lipSyncField = _avatarController.GetType().GetField("lipSync", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic);
                
                if (lipSyncField != null)
                {
                    LipSync connectedLipSync = lipSyncField.GetValue(_avatarController) as LipSync;
                    if (connectedLipSync == null)
                    {
                        Debug.LogWarning("⚠ AvatarController has no LipSync reference!");
                    }
                    else
                    {
                        Debug.Log("✓ AvatarController has valid LipSync reference");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error checking AvatarController to LipSync connection: {ex.Message}");
            }
        }
        
        // Check AudioPlayback to AvatarController connection
        if (_audioPlayback != null && _avatarController != null)
        {
            try
            {
                var avatarControllerField = _audioPlayback.GetType().GetField("avatarController", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic);
                
                if (avatarControllerField != null)
                {
                    AvatarController connectedController = avatarControllerField.GetValue(_audioPlayback) as AvatarController;
                    if (connectedController == null)
                    {
                        Debug.LogWarning("⚠ AudioPlayback has no AvatarController reference!");
                    }
                    else
                    {
                        Debug.Log("✓ AudioPlayback has valid AvatarController reference");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error checking AudioPlayback connection: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Monitor audio and lip sync activity
    /// </summary>
    private void MonitorAudio()
    {
        // Check audio status
        if (_audioSource != null)
        {
            bool wasPlaying = _isAudioPlaying;
            _isAudioPlaying = _audioSource.isPlaying;
            
            // Detect audio playback state changes
            if (_isAudioPlaying != wasPlaying)
            {
                if (_isAudioPlaying)
                {
                    Debug.Log("[LipSyncDiagnostics] Audio playback started");
                }
                else
                {
                    Debug.Log("[LipSyncDiagnostics] Audio playback stopped");
                }
            }
            
            // Monitor audio data if enabled
            if (monitorAudioData && _isAudioPlaying)
            {
                float[] samples = new float[256];
                _audioSource.GetSpectrumData(samples, 0, FFTWindow.Rectangular);
                
                float sum = 0f;
                for (int i = 0; i < 64; i++) // Lower frequencies for speech
                {
                    sum += samples[i];
                }
                
                // Only log if significant change
                if (Mathf.Abs(sum - _lastPlaybackValue) > 0.1f)
                {
                    _lastPlaybackValue = sum;
                    Debug.Log($"[LipSyncDiagnostics] Audio level: {sum:F4}");
                    
                    // Detect lip sync activity
                    _isLipSyncActive = sum > 0.01f;
                }
            }
        }
        
        // Test mode
        if (_testModeActive)
        {
            TestLipSync();
        }
    }
    
    /// <summary>
    /// Test lip sync by directly setting blend shape values
    /// </summary>
    private void TestLipSync()
    {
        // Calculate a sine wave value for testing
        float value = (Mathf.Sin(Time.time * testLipSyncFrequency) + 1f) * 0.5f * testLipSyncIntensity;
        
        // Apply to standard LipSync
        if (_lipSync != null && _faceRenderer != null)
        {
            try
            {
                // Get mouth open index
                var mouthOpenIndexField = _lipSync.GetType().GetField("mouthOpenIndex", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic);
                
                if (mouthOpenIndexField != null)
                {
                    int mouthOpenIndex = (int)mouthOpenIndexField.GetValue(_lipSync);
                    if (mouthOpenIndex >= 0 && mouthOpenIndex < _faceRenderer.sharedMesh.blendShapeCount)
                    {
                        _faceRenderer.SetBlendShapeWeight(mouthOpenIndex, value * 100f);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LipSyncDiagnostics] Error in test mode: {ex.Message}");
            }
        }
        
        // Apply to VRM LipSync
        if (_vrmLipSync != null && _blendShapeProxy != null)
        {
            _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), value);
            _blendShapeProxy.Apply();
        }
    }
    
    /// <summary>
    /// Start test mode
    /// </summary>
    public void StartTestMode()
    {
        _testModeActive = true;
        Debug.Log("[LipSyncDiagnostics] Test mode started");
    }
    
    /// <summary>
    /// Stop test mode
    /// </summary>
    public void StopTestMode()
    {
        _testModeActive = false;
        Debug.Log("[LipSyncDiagnostics] Test mode stopped");
    }
    
    /// <summary>
    /// Fix basic connection issues
    /// </summary>
    public void QuickFix()
    {
        Debug.Log("[LipSyncDiagnostics] Performing quick fixes...");
        
        // Ensure we have found all components
        FindComponents();
        
        // Fix VRMLipSync connections
        if (_vrmLipSync != null)
        {
            if (_blendShapeProxy != null)
            {
                _vrmLipSync.BlendShapeProxy = _blendShapeProxy;
                Debug.Log("✓ Connected BlendShapeProxy to VRMLipSync");
            }
            
            if (_audioSource != null)
            {
                _vrmLipSync.AudioSource = _audioSource;
                Debug.Log("✓ Connected AudioSource to VRMLipSync");
            }
        }
        
        // Fix LipSync connections
        if (_lipSync != null)
        {
            // Try to set audioSource
            if (_audioSource != null)
            {
                try
                {
                    var audioSourceField = _lipSync.GetType().GetField("audioSource", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic);
                    
                    if (audioSourceField != null)
                    {
                        audioSourceField.SetValue(_lipSync, _audioSource);
                        Debug.Log("✓ Connected AudioSource to LipSync");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ Error connecting AudioSource: {ex.Message}");
                }
            }
            
            // Try to set face renderer
            if (_faceRenderer != null)
            {
                try
                {
                    var faceRendererField = _lipSync.GetType().GetField("faceRenderer", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic);
                    
                    if (faceRendererField != null)
                    {
                        faceRendererField.SetValue(_lipSync, _faceRenderer);
                        Debug.Log("✓ Connected SkinnedMeshRenderer to LipSync");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ Error connecting SkinnedMeshRenderer: {ex.Message}");
                }
            }
        }
        
        // Fix AvatarController connections
        if (_avatarController != null)
        {
            // Connect LipSync
            if (_lipSync != null)
            {
                try
                {
                    var lipSyncField = _avatarController.GetType().GetField("lipSync", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic);
                    
                    if (lipSyncField != null)
                    {
                        lipSyncField.SetValue(_avatarController, _lipSync);
                        Debug.Log("✓ Connected LipSync to AvatarController");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"❌ Error connecting LipSync: {ex.Message}");
                }
            }
        }
        
        // Connect AudioPlayback to AvatarController
        if (_audioPlayback != null && _avatarController != null)
        {
            try
            {
                var avatarControllerField = _audioPlayback.GetType().GetField("avatarController", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic);
                
                if (avatarControllerField != null)
                {
                    avatarControllerField.SetValue(_audioPlayback, _avatarController);
                    Debug.Log("✓ Connected AvatarController to AudioPlayback");
                }
                
                // Connect events
                _audioPlayback.OnPlaybackStarted -= _avatarController.OnAudioPlaybackStarted;
                _audioPlayback.OnPlaybackCompleted -= _avatarController.OnAudioPlaybackCompleted;
                _audioPlayback.OnPlaybackProgress -= _avatarController.UpdateLipSync;
                
                _audioPlayback.OnPlaybackStarted += _avatarController.OnAudioPlaybackStarted;
                _audioPlayback.OnPlaybackCompleted += _avatarController.OnAudioPlaybackCompleted;
                _audioPlayback.OnPlaybackProgress += _avatarController.UpdateLipSync;
                
                Debug.Log("✓ Connected AudioPlayback events to AvatarController");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error connecting AvatarController: {ex.Message}");
            }
        }
        
        Debug.Log("[LipSyncDiagnostics] Quick fix complete. Run diagnostics to verify.");
    }
}