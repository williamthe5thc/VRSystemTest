using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles scene initialization and loading for the interview system.
/// </summary>
public class SceneInitializer : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuScene = "Scenes/MainMenu";
    [SerializeField] private float initializationDelay = 0.5f;
    [SerializeField] private bool showLoadingScreen = true;
    
    [Header("References")]
    [SerializeField] private GameObject loadingScreenPrefab;
    [SerializeField] private GameObject persistentSystemsPrefab;
    
    // Events
    public event Action<string> OnSceneInitialized;
    
    private static SceneInitializer _instance;
    private GameObject _loadingScreenInstance;
    
    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // Fix: Make the GameObject a root object before using DontDestroyOnLoad
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        
        DontDestroyOnLoad(gameObject);
        
        // Initialize persistent systems
        InitializePersistentSystems();
        
        // Register for scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void Start()
    {
        // Check if we need to load the main menu
        if (SceneManager.GetActiveScene().name != mainMenuScene)
        {
            LoadMainMenu();
        }
        else
        {
            StartCoroutine(InitializeSceneCoroutine(mainMenuScene));
        }
    }
    
    private void OnDestroy()
    {
        // Unregister from scene loading events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Initializes persistent system prefabs.
    /// </summary>
    private void InitializePersistentSystems()
    {
        if (persistentSystemsPrefab != null)
        {
            Instantiate(persistentSystemsPrefab);
        }
    }
    
    /// <summary>
    /// Handles scene loaded events.
    /// </summary>
    /// <param name="scene">The loaded scene.</param>
    /// <param name="mode">The load scene mode.</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(InitializeSceneCoroutine(scene.name));
    }
    
    /// <summary>
    /// Coroutine for initializing a scene after loading.
    /// </summary>
    /// <param name="sceneName">The name of the scene.</param>
    private IEnumerator InitializeSceneCoroutine(string sceneName)
    {
        // Wait for a frame to ensure everything is loaded
        yield return null;
        
        // Additional initialization delay
        yield return new WaitForSeconds(initializationDelay);
        
        // Find and initialize systems
        InitializeSceneSystems(sceneName);
        
        // Hide loading screen if active
        HideLoadingScreen();
        
        // Notify scene initialized
        OnSceneInitialized?.Invoke(sceneName);
        
        Debug.Log($"Scene '{sceneName}' initialized");
    }
    
    /// <summary>
    /// Initializes systems for the specified scene.
    /// </summary>
    /// <param name="sceneName">The name of the scene.</param>
    private void InitializeSceneSystems(string sceneName)
    {
        // Find and initialize environment manager
        EnvironmentManager environmentManager = FindObjectOfType<EnvironmentManager>();
        if (environmentManager != null)
        {
            // Environment will self-initialize in its Start method
        }
        
        // Find and initialize session manager if not in main menu
        if (sceneName != mainMenuScene)
        {
            SessionManager sessionManager = FindObjectOfType<SessionManager>();
            if (sessionManager != null && !sessionManager.IsSessionActive)
            {
                // Don't auto-start the session, let the UI handle it
            }
        }
        
        // Initialize VR systems
        VRRigSetup vrRigSetup = FindObjectOfType<VRRigSetup>();
        if (vrRigSetup != null)
        {
            // VR rig will self-initialize in its Start method
        }
    }
    
    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void LoadMainMenu()
    {
        LoadScene(mainMenuScene);
    }
    
    /// <summary>
    /// Loads the specified scene.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Cannot load empty scene name!");
            return;
        }
        
        // Show loading screen
        if (showLoadingScreen)
        {
            ShowLoadingScreen();
        }
        
        // Load the scene
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Shows the loading screen.
    /// </summary>
    private void ShowLoadingScreen()
    {
        if (loadingScreenPrefab != null && _loadingScreenInstance == null)
        {
            _loadingScreenInstance = Instantiate(loadingScreenPrefab);
            DontDestroyOnLoad(_loadingScreenInstance);
        }
    }
    
    /// <summary>
    /// Hides the loading screen.
    /// </summary>
    private void HideLoadingScreen()
    {
        if (_loadingScreenInstance != null)
        {
            Destroy(_loadingScreenInstance);
            _loadingScreenInstance = null;
        }
    }
    
    /// <summary>
    /// Gets the singleton instance.
    /// </summary>
    public static SceneInitializer Instance => _instance;
}