using UnityEngine;
using UnityEngine.XR;
#if UNITY_XR_MANAGEMENT
using UnityEngine.XR.Interaction.Toolkit;
#endif

/// <summary>
/// Sets up and configures the VR camera rig for the interview system.
/// </summary>
public class VRRigSetup : MonoBehaviour
{
    [Header("VR Components")]
    [SerializeField] private Transform cameraOffset;
    [SerializeField] private Camera xrCamera;
    
    // Use SerializeField instead of specific type to avoid compile errors
    [SerializeField] private MonoBehaviour xrRigOrOrigin;
    
    [Header("Player Settings")]
    [SerializeField] private float playerHeight = 1.7f;
    [SerializeField] private bool snapTurnEnabled = true;
    [SerializeField] private float snapTurnAmount = 30f;
    [SerializeField] private bool smoothTurnEnabled = false;
    [SerializeField] private float smoothTurnSpeed = 60f;
    
    [Header("Movement Settings")]
    [SerializeField] private bool teleportEnabled = true;
    [SerializeField] private bool continuousMovementEnabled = false;
    [SerializeField] private float movementSpeed = 1.0f;
    
    // Internal references
    private MonoBehaviour _moveProvider;
    private MonoBehaviour _snapTurnProvider;
    private MonoBehaviour _continuousTurnProvider;
    private MonoBehaviour _teleportProvider;
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void Start()
    {
        ConfigureRig();
        
        // Apply settings from preferences if available
        LoadSettingsFromPrefs();
    }
    
    /// <summary>
    /// Initializes required VR components if not assigned.
    /// </summary>
    private void InitializeComponents()
    {
        // Find camera and camera offset through generic means
        if (xrCamera == null)
        {
            xrCamera = Camera.main;
        }
        
        if (cameraOffset == null && xrCamera != null)
        {
            // Try to find a parent with "Camera Offset" in the name
            Transform current = xrCamera.transform.parent;
            while (current != null)
            {
                if (current.name.Contains("Camera Offset"))
                {
                    cameraOffset = current;
                    break;
                }
                current = current.parent;
            }
        }
        
        // Find movement and turn providers generically
        _moveProvider = FindFirstComponentInScene<MonoBehaviour>("MoveProvider");
        _snapTurnProvider = FindFirstComponentInScene<MonoBehaviour>("SnapTurnProvider");
        _continuousTurnProvider = FindFirstComponentInScene<MonoBehaviour>("ContinuousTurnProvider");
        _teleportProvider = FindFirstComponentInScene<MonoBehaviour>("TeleportationProvider");
    }
    
    private T FindFirstComponentInScene<T>() where T : Component
    {
        T[] components = FindObjectsOfType<T>();
        return components.Length > 0 ? components[0] : null;
    }
    
    private MonoBehaviour FindFirstComponentInScene<T>(string nameContains) where T : MonoBehaviour
    {
        MonoBehaviour[] components = FindObjectsOfType<MonoBehaviour>();
        foreach (var component in components)
        {
            if (component.GetType().Name.Contains(nameContains))
                return component;
        }
        return null;
    }
    
    /// <summary>
    /// Configures the VR rig based on settings.
    /// </summary>
    private void ConfigureRig()
    {
        // Configure height offset
        if (cameraOffset != null)
        {
            // Apply height offset
            Vector3 position = cameraOffset.localPosition;
            position.y = playerHeight;
            cameraOffset.localPosition = position;
        }
        
        // Configure providers through reflection to avoid type errors
        ConfigureProvider(_moveProvider, "enabled", continuousMovementEnabled);
        ConfigureProvider(_moveProvider, "moveSpeed", movementSpeed);
        
        ConfigureProvider(_snapTurnProvider, "enabled", snapTurnEnabled);
        ConfigureProvider(_snapTurnProvider, "turnAmount", snapTurnAmount);
        
        ConfigureProvider(_continuousTurnProvider, "enabled", smoothTurnEnabled && !snapTurnEnabled);
        ConfigureProvider(_continuousTurnProvider, "turnSpeed", smoothTurnSpeed);
        
        ConfigureProvider(_teleportProvider, "enabled", teleportEnabled);
    }
    
    private void ConfigureProvider(MonoBehaviour provider, string propertyName, object value)
    {
        if (provider == null) return;
        
        try
        {
            var property = provider.GetType().GetProperty(propertyName);
            if (property != null)
            {
                property.SetValue(provider, value);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to set {propertyName} on {provider.GetType().Name}: {e.Message}");
        }
    }
    
    /// <summary>
    /// Loads movement settings from player preferences.
    /// </summary>
    private void LoadSettingsFromPrefs()
    {
        if (SettingsManager.Instance != null)
        {
            // Load movement preferences
            continuousMovementEnabled = SettingsManager.Instance.GetSetting<bool>("ContinuousMovement");
            teleportEnabled = SettingsManager.Instance.GetSetting<bool>("TeleportEnabled");
            movementSpeed = SettingsManager.Instance.GetSetting<float>("MovementSpeed");
            
            // Load turning preferences
            snapTurnEnabled = SettingsManager.Instance.GetSetting<bool>("SnapTurnEnabled");
            smoothTurnEnabled = SettingsManager.Instance.GetSetting<bool>("SmoothTurnEnabled");
            snapTurnAmount = SettingsManager.Instance.GetSetting<float>("SnapTurnAmount");
            smoothTurnSpeed = SettingsManager.Instance.GetSetting<float>("SmoothTurnSpeed");
            
            // Apply loaded settings
            ConfigureRig();
        }
    }
    
    /// <summary>
    /// Sets the player height.
    /// </summary>
    /// <param name="height">Player height in meters.</param>
    public void SetPlayerHeight(float height)
    {
        playerHeight = Mathf.Clamp(height, 0.5f, 2.5f);
        
        if (cameraOffset != null)
        {
            Vector3 position = cameraOffset.localPosition;
            position.y = playerHeight;
            cameraOffset.localPosition = position;
        }
        
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetSetting("PlayerHeight", playerHeight);
        }
    }
    
    /// <summary>
    /// Sets the movement mode.
    /// </summary>
    /// <param name="useContinuousMovement">Whether to use continuous movement.</param>
    public void SetMovementMode(bool useContinuousMovement)
    {
        continuousMovementEnabled = useContinuousMovement;
        ConfigureProvider(_moveProvider, "enabled", continuousMovementEnabled);
        
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetSetting("ContinuousMovement", continuousMovementEnabled);
        }
    }
    
    /// <summary>
    /// Sets the turning mode.
    /// </summary>
    /// <param name="useSnapTurn">Whether to use snap turning.</param>
    public void SetTurningMode(bool useSnapTurn)
    {
        snapTurnEnabled = useSnapTurn;
        smoothTurnEnabled = !useSnapTurn;
        
        ConfigureProvider(_snapTurnProvider, "enabled", snapTurnEnabled);
        ConfigureProvider(_continuousTurnProvider, "enabled", smoothTurnEnabled);
        
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetSetting("SnapTurnEnabled", snapTurnEnabled);
            SettingsManager.Instance.SetSetting("SmoothTurnEnabled", smoothTurnEnabled);
        }
    }
    
    /// <summary>
    /// Sets whether teleportation is enabled.
    /// </summary>
    /// <param name="enabled">Whether teleportation is enabled.</param>
    public void SetTeleportEnabled(bool enabled)
    {
        teleportEnabled = enabled;
        ConfigureProvider(_teleportProvider, "enabled", teleportEnabled);
        
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetSetting("TeleportEnabled", teleportEnabled);
        }
    }
}