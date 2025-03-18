using System;
using UnityEngine;

/// <summary>
/// Main application controller that manages the overall app state and initialization.
/// </summary>
public class AppManager : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private SettingsManager settingsManager;
    
    // Singleton instance
    private static AppManager _instance;
    public static AppManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("AppManager instance not found!");
            }
            return _instance;
        }
    }
    
    // App state
    private bool _isInitialized = false;
    public bool IsInitialized => _isInitialized;
    
    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize the application
        Initialize();
    }
    
    private void Initialize()
    {
        Debug.Log("Initializing VR Interview System...");
        
        // Load settings first
        if (settingsManager != null)
        {
            settingsManager.LoadSettings();
        }
        else
        {
            Debug.LogWarning("SettingsManager not assigned!");
        }
        
        // Request necessary permissions
        RequestPermissions();
        
        // Mark as initialized
        _isInitialized = true;
        Debug.Log("VR Interview System initialized successfully.");
    }
    
    private void RequestPermissions()
    {
        Debug.Log("Requesting necessary permissions...");
        
        // Request microphone permission for Oculus Quest
        #if PLATFORM_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
        }
        #endif
    }
    
    /// <summary>
    /// Starts a new interview session.
    /// </summary>
    public async void StartInterview()
    {
        if (sessionManager != null)
        {
            await sessionManager.StartSession();
        }
        else
        {
            Debug.LogError("Cannot start interview: SessionManager not assigned!");
        }
    }
    
    /// <summary>
    /// Ends the current interview session.
    /// </summary>
    public async void EndInterview()
    {
        if (sessionManager != null)
        {
            await sessionManager.EndSession();
        }
        else
        {
            Debug.LogError("Cannot end interview: SessionManager not assigned!");
        }
    }
    
    /// <summary>
    /// Resets the current interview session.
    /// </summary>
    public async void ResetInterview()
    {
        if (sessionManager != null)
        {
            await sessionManager.ResetSession();
        }
        else
        {
            Debug.LogError("Cannot reset interview: SessionManager not assigned!");
        }
    }
    
    /// <summary>
    /// Exits the application.
    /// </summary>
    public async void ExitApplication()
    {
        Debug.Log("Exiting application...");
        
        // Clean up
        if (sessionManager != null)
        {
            await sessionManager.EndSession();
        }
        
        // Quit application
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}