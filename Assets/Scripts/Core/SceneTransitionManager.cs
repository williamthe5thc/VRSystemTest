using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages scene transitions with loading screens and progress tracking.
/// Handles scene loading, unloading, and provides transition effects.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Loading Screen")]
    [SerializeField] private Canvas loadingCanvas;
    [SerializeField] private Image loadingFillImage;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private CanvasGroup fadeGroup;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float minimumLoadingScreenTime = 1.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Loading Tips")]
    [SerializeField] private string[] loadingTips;
    
    // Tracking variables
    private bool isTransitioning = false;
    private string targetScene = "";
    private Action onSceneLoadedCallback = null;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Make sure loading screen is initially hidden
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            loadingCanvas.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Load a scene with transition effects
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    /// <param name="onLoaded">Optional callback after scene is loaded</param>
    /// <param name="showLoadingScreen">Whether to show loading screen (default: true)</param>
    public void LoadScene(string sceneName, Action onLoaded = null, bool showLoadingScreen = true)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Scene transition already in progress!");
            return;
        }
        
        isTransitioning = true;
        targetScene = sceneName;
        onSceneLoadedCallback = onLoaded;
        
        if (showLoadingScreen)
        {
            StartCoroutine(LoadSceneWithTransition(sceneName));
        }
        else
        {
            StartCoroutine(LoadSceneDirectly(sceneName));
        }
    }
    
    /// <summary>
    /// Reload the current scene
    /// </summary>
    /// <param name="onLoaded">Optional callback after scene is loaded</param>
    /// <param name="showLoadingScreen">Whether to show loading screen (default: true)</param>
    public void ReloadCurrentScene(Action onLoaded = null, bool showLoadingScreen = true)
    {
        LoadScene(SceneManager.GetActiveScene().name, onLoaded, showLoadingScreen);
    }
    
    /// <summary>
    /// Load a scene with full loading screen and progress bar
    /// </summary>
    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        // Activate loading canvas
        loadingCanvas.gameObject.SetActive(true);
        
        // Show a random tip
        if (tipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            tipText.text = loadingTips[UnityEngine.Random.Range(0, loadingTips.Length)];
        }
        
        // Fade in loading screen
        yield return StartCoroutine(FadeLoadingScreen(true, fadeInDuration));
        
        // Start loading the scene asynchronously
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;
        
        // Track when loading started
        float loadStartTime = Time.time;
        
        // Update progress bar while loading
        while (!asyncOperation.isDone)
        {
            // Update progress text and bar
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            
            if (loadingFillImage != null)
            {
                loadingFillImage.fillAmount = progress;
            }
            
            if (loadingText != null)
            {
                loadingText.text = $"Loading... {Mathf.Floor(progress * 100)}%";
            }
            
            // Check if loading is almost complete
            if (asyncOperation.progress >= 0.9f)
            {
                // Ensure minimum loading screen time
                float timeElapsed = Time.time - loadStartTime;
                if (timeElapsed >= minimumLoadingScreenTime)
                {
                    // Allow scene activation
                    asyncOperation.allowSceneActivation = true;
                }
                else
                {
                    // Wait until minimum time has elapsed
                    yield return new WaitForSeconds(minimumLoadingScreenTime - timeElapsed);
                    asyncOperation.allowSceneActivation = true;
                }
            }
            
            yield return null;
        }
        
        // Scene is loaded, wait a moment before fading out
        yield return new WaitForSeconds(0.5f);
        
        // Fade out loading screen
        yield return StartCoroutine(FadeLoadingScreen(false, fadeOutDuration));
        
        // Deactivate loading canvas
        loadingCanvas.gameObject.SetActive(false);
        
        // Reset transition state
        isTransitioning = false;
        
        // Invoke callback if provided
        onSceneLoadedCallback?.Invoke();
    }
    
    /// <summary>
    /// Load a scene directly without showing the loading screen
    /// </summary>
    private IEnumerator LoadSceneDirectly(string sceneName)
    {
        // Load the scene
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        
        // Wait until the scene is loaded
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        
        // Reset transition state
        isTransitioning = false;
        
        // Invoke callback if provided
        onSceneLoadedCallback?.Invoke();
    }
    
    /// <summary>
    /// Fade the loading screen in or out
    /// </summary>
    private IEnumerator FadeLoadingScreen(bool fadeIn, float duration)
    {
        if (fadeGroup == null) yield break;
        
        float startAlpha = fadeGroup.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;
        float time = 0f;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float normalizedTime = time / duration;
            float curveValue = fadeCurve.Evaluate(normalizedTime);
            
            fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
            
            yield return null;
        }
        
        // Ensure we reach the target alpha
        fadeGroup.alpha = targetAlpha;
    }
    
    /// <summary>
    /// Preload a scene in the background without activating it.
    /// Useful for reducing loading times when you know a scene will be needed soon.
    /// </summary>
    /// <param name="sceneName">Name of the scene to preload</param>
    /// <param name="onPreloaded">Optional callback when preloading is complete</param>
    /// <returns>AsyncOperation tracking the load progress</returns>
    public AsyncOperation PreloadScene(string sceneName, Action onPreloaded = null)
    {
        // Load the scene additively and don't activate it
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        operation.allowSceneActivation = false;
        
        // Start a coroutine to track progress
        StartCoroutine(TrackPreloadProgress(operation, onPreloaded));
        
        return operation;
    }
    
    private IEnumerator TrackPreloadProgress(AsyncOperation operation, Action onPreloaded)
    {
        // Wait until the scene is almost loaded (0.9 = 90%)
        while (operation.progress < 0.9f)
        {
            yield return null;
        }
        
        // Invoke callback
        onPreloaded?.Invoke();
    }
    
    /// <summary>
    /// Activate a previously preloaded scene
    /// </summary>
    /// <param name="operation">AsyncOperation from PreloadScene</param>
    /// <param name="unloadCurrent">Whether to unload the current scene</param>
    public void ActivatePreloadedScene(AsyncOperation operation, bool unloadCurrent = true)
    {
        if (operation != null)
        {
            // Start a coroutine to activate the scene
            StartCoroutine(ActivatePreloadedSceneRoutine(operation, unloadCurrent));
        }
    }
    
    private IEnumerator ActivatePreloadedSceneRoutine(AsyncOperation operation, bool unloadCurrent)
    {
        // Remember current scene
        Scene currentScene = SceneManager.GetActiveScene();
        
        // Allow the preloaded scene to activate
        operation.allowSceneActivation = true;
        
        // Wait for the operation to complete
        while (!operation.isDone)
        {
            yield return null;
        }
        
        // Wait a frame to ensure everything is loaded
        yield return null;
        
        // Find the newly loaded scene
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene != currentScene)
            {
                // Set it as the active scene
                SceneManager.SetActiveScene(scene);
                break;
            }
        }
        
        // Unload the previous scene if requested
        if (unloadCurrent)
        {
            SceneManager.UnloadSceneAsync(currentScene);
        }
    }
    
    /// <summary>
    /// Set custom loading tips
    /// </summary>
    /// <param name="tips">Array of tips to show during loading</param>
    public void SetLoadingTips(string[] tips)
    {
        loadingTips = tips;
    }
    
    /// <summary>
    /// Add a loading tip to the existing tips
    /// </summary>
    /// <param name="tip">Tip to add</param>
    public void AddLoadingTip(string tip)
    {
        if (loadingTips == null)
        {
            loadingTips = new string[] { tip };
            return;
        }
        
        string[] newTips = new string[loadingTips.Length + 1];
        loadingTips.CopyTo(newTips, 0);
        newTips[loadingTips.Length] = tip;
        loadingTips = newTips;
    }
}