using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the interview environment and scene settings.
/// </summary>
public class EnvironmentManager : MonoBehaviour
{
    [Header("Environment Settings")]
    [SerializeField] private string environmentName = "Default";
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform avatarSpawnPoint;
    
    [Header("Lighting")]
    [SerializeField] private LightingControl lightingControl;
    [SerializeField] private string defaultLightingPreset = "Neutral";
    
    [Header("Interactables")]
    [SerializeField] private InteractableItems interactableItems;
    
    [Header("Audio")]
    [SerializeField] private AudioSource ambientAudioSource;
    [SerializeField] private AudioClip[] ambientSounds;
    [SerializeField] private float ambientVolume = 0.2f;
    
    // Events
    public event Action<string> OnEnvironmentLoaded;
    
    private void Start()
    {
        InitializeComponents();
        SetupEnvironment();
    }
    
    /// <summary>
    /// Initializes required components if not assigned.
    /// </summary>
    private void InitializeComponents()
    {
        // Find player spawn point if not assigned
        if (playerSpawnPoint == null)
        {
            GameObject spawnObj = GameObject.FindGameObjectWithTag("PlayerSpawn");
            if (spawnObj != null)
            {
                playerSpawnPoint = spawnObj.transform;
            }
            else
            {
                Debug.LogWarning("Player spawn point not found! Creating default.");
                
                // Create a default spawn point
                GameObject newSpawn = new GameObject("PlayerSpawn");
                newSpawn.transform.position = new Vector3(0, 1.6f, 0);
                newSpawn.tag = "PlayerSpawn";
                playerSpawnPoint = newSpawn.transform;
            }
        }
        
        // Find avatar spawn point if not assigned
        if (avatarSpawnPoint == null)
        {
            GameObject spawnObj = GameObject.FindGameObjectWithTag("AvatarSpawn");
            if (spawnObj != null)
            {
                avatarSpawnPoint = spawnObj.transform;
            }
            else
            {
                Debug.LogWarning("Avatar spawn point not found! Creating default.");
                
                // Create a default spawn point
                GameObject newSpawn = new GameObject("AvatarSpawn");
                newSpawn.transform.position = new Vector3(0, 1.6f, 2f);
                newSpawn.transform.rotation = Quaternion.Euler(0, 180f, 0); // Face player
                newSpawn.tag = "AvatarSpawn";
                avatarSpawnPoint = newSpawn.transform;
            }
        }
        
        // Find lighting control if not assigned
        if (lightingControl == null)
        {
            lightingControl = GetComponent<LightingControl>();
            
            if (lightingControl == null)
            {
                lightingControl = FindObjectOfType<LightingControl>();
                
                if (lightingControl == null)
                {
                    Debug.LogWarning("LightingControl not found! Creating default.");
                    
                    // Create a default lighting control
                    lightingControl = gameObject.AddComponent<LightingControl>();
                }
            }
        }
        
        // Find interactable items if not assigned
        if (interactableItems == null)
        {
            interactableItems = GetComponent<InteractableItems>();
            
            if (interactableItems == null)
            {
                interactableItems = FindObjectOfType<InteractableItems>();
                
                if (interactableItems == null)
                {
                    Debug.LogWarning("InteractableItems not found! Creating default.");
                    
                    // Create a default interactable items controller
                    interactableItems = gameObject.AddComponent<InteractableItems>();
                }
            }
        }
        
        // Create ambient audio source if not assigned
        if (ambientAudioSource == null)
        {
            // Look for existing ambient audio source
            ambientAudioSource = GameObject.FindGameObjectWithTag("AmbientAudio")?.GetComponent<AudioSource>();
            
            if (ambientAudioSource == null)
            {
                // Create new audio source
                GameObject audioObj = new GameObject("AmbientAudio");
                audioObj.tag = "AmbientAudio";
                ambientAudioSource = audioObj.AddComponent<AudioSource>();
                ambientAudioSource.spatialBlend = 0f; // 2D sound
                ambientAudioSource.loop = true;
                ambientAudioSource.playOnAwake = false;
                ambientAudioSource.volume = ambientVolume;
                
                // Make it a child of this object
                audioObj.transform.parent = transform;
            }
        }
    }
    
    /// <summary>
    /// Sets up the environment based on settings.
    /// </summary>
    private void SetupEnvironment()
    {
        // Apply lighting preset
        if (lightingControl != null)
        {
            lightingControl.ApplyPreset(defaultLightingPreset);
        }
        
        // Initialize interactable items
        if (interactableItems != null)
        {
            interactableItems.InitializeItems();
        }
        
        // Start ambient audio
        SetupAmbientAudio();
        
        // Position player and avatar
        PositionPlayerAndAvatar();
        
        // Notify environment loaded
        OnEnvironmentLoaded?.Invoke(environmentName);
        
        Debug.Log($"Environment '{environmentName}' loaded and configured.");
    }
    
    /// <summary>
    /// Sets up ambient audio for the environment.
    /// </summary>
    private void SetupAmbientAudio()
    {
        if (ambientAudioSource == null || ambientSounds == null || ambientSounds.Length == 0)
        {
            return;
        }
        
        // Select a random ambient sound
        AudioClip selectedClip = ambientSounds[UnityEngine.Random.Range(0, ambientSounds.Length)];
        
        if (selectedClip != null)
        {
            ambientAudioSource.clip = selectedClip;
            ambientAudioSource.volume = ambientVolume;
            ambientAudioSource.Play();
        }
    }
    
    /// <summary>
    /// Positions the player and avatar at their spawn points.
    /// </summary>
    private void PositionPlayerAndAvatar()
    {
        // Position player
        if (playerSpawnPoint != null)
        {
            // Find player based on tag or camera
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (player == null)
            {
                // Try to find main camera as fallback
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    player = mainCamera.gameObject;
                }
            }
            
            if (player != null)
            {
                // Position the player
                player.transform.position = playerSpawnPoint.position;
                player.transform.rotation = playerSpawnPoint.rotation;
            }
        }
        
        // Position avatar
        if (avatarSpawnPoint != null)
        {
            // Find avatar
            AvatarController avatar = FindObjectOfType<AvatarController>();
            
            if (avatar != null)
            {
                // Position the avatar
                avatar.transform.position = avatarSpawnPoint.position;
                avatar.transform.rotation = avatarSpawnPoint.rotation;
            }
        }
    }
    
    /// <summary>
    /// Changes the lighting preset.
    /// </summary>
    /// <param name="presetName">Name of the lighting preset.</param>
    public void ChangeLighting(string presetName)
    {
        if (lightingControl != null)
        {
            lightingControl.ApplyPreset(presetName);
        }
    }
    
    /// <summary>
    /// Sets the ambient audio volume.
    /// </summary>
    /// <param name="volume">Volume (0-1).</param>
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        
        if (ambientAudioSource != null)
        {
            ambientAudioSource.volume = ambientVolume;
        }
    }
    
    /// <summary>
    /// Gets the player spawn point.
    /// </summary>
    /// <returns>The player spawn transform.</returns>
    public Transform GetPlayerSpawnPoint()
    {
        return playerSpawnPoint;
    }
    
    /// <summary>
    /// Gets the avatar spawn point.
    /// </summary>
    /// <returns>The avatar spawn transform.</returns>
    public Transform GetAvatarSpawnPoint()
    {
        return avatarSpawnPoint;
    }
    
    /// <summary>
    /// Gets the environment name.
    /// </summary>
    /// <returns>The environment name.</returns>
    public string GetEnvironmentName()
    {
        return environmentName;
    }
}