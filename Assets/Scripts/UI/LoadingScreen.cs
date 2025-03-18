using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the loading screen UI for scene transitions.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI progressText;
    
    [Header("Animation Settings")]
    [SerializeField] private float minLoadTime = 1.0f;
    [SerializeField] private float maxLoadTime = 3.0f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private string[] loadingMessages;
    
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        // Get or add canvas group for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Start with fully transparent
        canvasGroup.alpha = 0f;
    }
    
    private void Start()
    {
        // Start the loading animation
        StartCoroutine(AnimateLoading());
    }
    
    /// <summary>
    /// Coroutine to animate the loading screen.
    /// </summary>
    private IEnumerator AnimateLoading()
    {
        // Fade in
        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));
        
        // Display random loading message
        if (loadingText != null && loadingMessages != null && loadingMessages.Length > 0)
        {
            int messageIndex = Random.Range(0, loadingMessages.Length);
            loadingText.text = loadingMessages[messageIndex];
        }
        
        // Animate progress bar
        float loadTime = Random.Range(minLoadTime, maxLoadTime);
        float elapsed = 0f;
        
        while (elapsed < loadTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / loadTime;
            
            // Update progress bar
            if (progressBar != null)
            {
                progressBar.fillAmount = progress;
            }
            
            // Update progress text
            if (progressText != null)
            {
                progressText.text = $"{Mathf.FloorToInt(progress * 100)}%";
            }
            
            yield return null;
        }
        
        // Ensure progress is complete
        if (progressBar != null)
        {
            progressBar.fillAmount = 1f;
        }
        
        if (progressText != null)
        {
            progressText.text = "100%";
        }
        
        // Wait a moment at 100%
        yield return new WaitForSeconds(0.5f);
        
        // The loading screen will be destroyed by the SceneInitializer
        // No need to fade out here
    }
    
    /// <summary>
    /// Fades the canvas group between alpha values.
    /// </summary>
    /// <param name="startAlpha">Starting alpha value.</param>
    /// <param name="endAlpha">Ending alpha value.</param>
    /// <param name="duration">Duration of the fade.</param>
    private IEnumerator FadeCanvas(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }
    
    /// <summary>
    /// Sets a custom loading message.
    /// </summary>
    /// <param name="message">The loading message to display.</param>
    public void SetLoadingMessage(string message)
    {
        if (loadingText != null)
        {
            loadingText.text = message;
        }
    }
}
