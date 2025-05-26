using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Provides menu items for directly forcing sound playback in the editor
/// This bypasses all normal audio systems to test if sound can be played at all
/// </summary>
public class ForceSoundPlayMenu
{
    [MenuItem("VR Interview/Debug/Play Test Sound", false, 100)]
    private static void PlayTestSound()
    {
        Debug.Log("ForceSoundPlayMenu: Playing test sound...");
        
        // Generate a test tone
        AudioClip testClip = GenerateTestClip();
        
        // Create a temporary GameObject to play the sound
        GameObject tempObject = new GameObject("_TempAudioPlayer");
        AudioSource source = tempObject.AddComponent<AudioSource>();
        
        // Configure for reliable playback
        source.clip = testClip;
        source.volume = 1.0f;
        source.spatialBlend = 0f; // 2D sound
        source.loop = false;
        source.playOnAwake = false;
        
        // Play the sound
        source.Play();
        
        Debug.Log($"ForceSoundPlayMenu: Playing {testClip.length}s test sound at volume 1.0");
        
        // Schedule destruction after the clip finishes
        // Fixed: The second parameter should be a boolean, not a float
        // Also, we should use Destroy instead of DestroyImmediate for delayed destruction
        Object.Destroy(tempObject, testClip.length + 1.0f);
    }
    
    [MenuItem("VR Interview/Debug/Fix Audio Chain", false, 101)]
    private static void FixAudioChain()
    {
        Debug.Log("ForceSoundPlayMenu: Fixing audio chain...");
        
        // Find or add AudioSystemReset
        AudioSystemReset reset = Object.FindObjectOfType<AudioSystemReset>();
        if (reset == null)
        {
            GameObject resetObject = new GameObject("AudioSystemReset");
            reset = resetObject.AddComponent<AudioSystemReset>();
            
            Debug.Log("Created AudioSystemReset GameObject");
        }
        
        // Force immediate reset
        reset.ResetAudioSystemsNow();
        
        // Check audio listener
        CheckAudioListener();
        
        // Check all audio sources
        List<AudioSource> allSources = new List<AudioSource>(Object.FindObjectsOfType<AudioSource>());
        Debug.Log($"Found {allSources.Count} AudioSource components in scene");
        
        // Check for at least one audio source
        if (allSources.Count == 0)
        {
            GameObject tempObject = new GameObject("DefaultAudioSource");
            tempObject.AddComponent<AudioSource>();
            Debug.Log("Added default AudioSource as none were found in scene");
        }
        
        // Check if AudioPlayback has an AudioSource
        AudioPlayback playback = Object.FindObjectOfType<AudioPlayback>();
        if (playback != null)
        {
            AudioSource source = playback.GetComponent<AudioSource>();
            if (source == null)
            {
                source = playback.gameObject.AddComponent<AudioSource>();
                Debug.Log("Added missing AudioSource to AudioPlayback");
            }
            
            // Configure AudioSource
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 1.0f;
            
            Debug.Log("Reconfigured AudioSource on AudioPlayback");
        }
        
        Debug.Log("Audio chain fixes completed");
    }
    
    [MenuItem("VR Interview/Debug/Check Audio System", false, 102)]
    private static void CheckAudioSystem()
    {
        Debug.Log("ForceSoundPlayMenu: Checking audio system...");
        
        // Check audio settings
        CheckAudioSettings();
        
        // Check audio listener
        CheckAudioListener();
        
        // Check audio playback components
        CheckAudioPlayback();
        
        Debug.Log("Audio system check complete. See console for details.");
    }
    
    [MenuItem("VR Interview/Debug/Restart Unity Audio", false, 103)]
    private static void RestartUnityAudio()
    {
        Debug.Log("ForceSoundPlayMenu: Attempting to restart Unity audio system...");
        
        // This will attempt to restart the Unity audio system
        // Note: This is a bit of a hack and not guaranteed to work
        AudioSettings.Reset(AudioSettings.GetConfiguration());
        
        Debug.Log("Requested audio system restart");
    }
    
    private static void CheckAudioSettings()
    {
        Debug.Log("Audio Settings Check:");
        Debug.Log($"- Real sample rate: {AudioSettings.outputSampleRate}Hz");
        
        AudioConfiguration config = AudioSettings.GetConfiguration();
        Debug.Log($"- Speaker mode: {config.speakerMode}");
        Debug.Log($"- Sample rate: {config.sampleRate}Hz");
        Debug.Log($"- DSP buffer size: {config.dspBufferSize}");
        Debug.Log($"- Spatial mode: {AudioSettings.GetSpatializerPluginName()}");
    }
    
    private static void CheckAudioListener()
    {
        AudioListener[] listeners = Object.FindObjectsOfType<AudioListener>();
        
        if (listeners.Length == 0)
        {
            Debug.LogError("No AudioListener found in scene!");
            
            // Try to add to main camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.gameObject.AddComponent<AudioListener>();
                Debug.Log("Added AudioListener to main camera");
            }
        }
        else if (listeners.Length > 1)
        {
            Debug.LogWarning($"Found {listeners.Length} AudioListeners - should only have 1!");
            
            string listenerInfo = "AudioListeners found on:";
            foreach (var listener in listeners)
            {
                listenerInfo += $"\n- {listener.gameObject.name} (Enabled: {listener.enabled})";
            }
            Debug.Log(listenerInfo);
        }
        else
        {
            Debug.Log($"AudioListener found on {listeners[0].gameObject.name} (Enabled: {listeners[0].enabled})");
        }
    }
    
    private static void CheckAudioPlayback()
    {
        AudioPlayback playback = Object.FindObjectOfType<AudioPlayback>();
        if (playback == null)
        {
            Debug.LogError("No AudioPlayback component found in scene!");
            return;
        }
        
        Debug.Log($"AudioPlayback found on {playback.gameObject.name}");
        
        // Check for AudioSource
        AudioSource source = playback.GetComponent<AudioSource>();
        if (source == null)
        {
            Debug.LogError("AudioPlayback is missing an AudioSource component!");
        }
        else
        {
            Debug.Log($"- AudioSource status: Volume={source.volume}, Mute={source.mute}, SpatialBlend={source.spatialBlend}");
            Debug.Log($"- Current AudioClip: {(source.clip != null ? source.clip.name : "None")}");
        }
        
        // Check for AudioPlaybackFix
        AudioPlaybackFix fix = playback.GetComponent<AudioPlaybackFix>();
        if (fix == null)
        {
            Debug.LogWarning("AudioPlayback is missing the AudioPlaybackFix component");
        }
        else
        {
            Debug.Log("AudioPlaybackFix component is present");
        }
        
        // Check for references to AvatarController
        AvatarController avatarController = Object.FindObjectOfType<AvatarController>();
        if (avatarController == null)
        {
            Debug.LogWarning("No AvatarController found in scene");
        }
        else
        {
            Debug.Log($"AvatarController found on {avatarController.gameObject.name}");
            
            // Check Animator
            Animator animator = avatarController.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("AvatarController is missing an Animator component!");
            }
            else
            {
                Debug.Log($"- Animator status: Controller={animator.runtimeAnimatorController != null}, Enabled={animator.enabled}");
            }
        }
    }
    
    private static AudioClip GenerateTestClip()
    {
        int sampleRate = 44100;
        float frequency = 440f; // A4 note
        float duration = 1.0f;
        
        AudioClip clip = AudioClip.Create("TestTone", (int)(sampleRate * duration), 1, sampleRate, false);
        
        float[] samples = new float[(int)(sampleRate * duration)];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / sampleRate;
            // Generate a sine wave tone that fades in and out
            float envelope = Mathf.Clamp01(Mathf.Min(t * 4, (duration - t) * 4));
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * envelope;
        }
        
        clip.SetData(samples, 0);
        return clip;
    }
}