using UnityEngine;
using TMPro;
using System.Collections;
using System.Text;

/// <summary>
/// Runtime performance monitoring for Oculus Quest.
/// Displays FPS, memory usage, and other performance metrics.
/// </summary>
public class VRPerformanceMonitor : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private bool showPerformanceStats = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.P;
    [SerializeField] private TextMeshProUGUI performanceText;
    [SerializeField] private float updateInterval = 0.5f;
    
    [Header("Warning Thresholds")]
    [SerializeField] private float lowFpsThreshold = 60f;
    [SerializeField] private float criticalFpsThreshold = 45f;
    [SerializeField] private float highCpuTimeThreshold = 8f;
    [SerializeField] private float criticalCpuTimeThreshold = 12f;
    
    [Header("Display Options")]
    [SerializeField] private bool showFps = true;
    [SerializeField] private bool showCpuTime = true;
    [SerializeField] private bool showGpuTime = true;
    [SerializeField] private bool showMemory = true;
    [SerializeField] private bool showWarningsOnly = false;
    
    // Performance data
    private float fps;
    private float frameTime;
    private float cpuFrameTime;
    private float gpuFrameTime;
    private float memoryUsage;
    
    // Colors
    private Color normalColor = Color.green;
    private Color warningColor = Color.yellow;
    private Color criticalColor = Color.red;
    
    // Private variables
    private float timeSinceLastUpdate = 0f;
    private int frameCount = 0;
    private float accumulatedTime = 0f;
    private StringBuilder stringBuilder = new StringBuilder();
    
    private void Start()
    {
        if (performanceText == null)
        {
            Debug.LogWarning("Performance Text reference is missing!");
        }
        
        // Hide the text initially if specified
        if (performanceText != null && !showPerformanceStats)
        {
            performanceText.gameObject.SetActive(false);
        }
        
        // Start measuring GPU time - this is estimated since direct measurement isn't available
        StartCoroutine(EstimateGpuTime());
    }
    
    private void Update()
    {
        // Toggle display on/off with key press
        if (Input.GetKeyDown(toggleKey))
        {
            showPerformanceStats = !showPerformanceStats;
            
            if (performanceText != null)
            {
                performanceText.gameObject.SetActive(showPerformanceStats);
            }
        }
        
        // Check if we need to update the stats
        frameCount++;
        accumulatedTime += Time.unscaledDeltaTime;
        timeSinceLastUpdate += Time.unscaledDeltaTime;
        
        if (timeSinceLastUpdate >= updateInterval)
        {
            // Calculate FPS and frame time
            fps = frameCount / accumulatedTime;
            frameTime = 1000.0f / fps;
            cpuFrameTime = Time.unscaledDeltaTime * 1000.0f;
            
            // Get memory usage (in MB)
            memoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            
            // Update the display
            if (showPerformanceStats && performanceText != null)
            {
                UpdatePerformanceDisplay();
            }
            
            // Reset counters
            frameCount = 0;
            accumulatedTime = 0f;
            timeSinceLastUpdate = 0f;
        }
    }
    
    private void UpdatePerformanceDisplay()
    {
        stringBuilder.Clear();
        
        if (showFps)
        {
            Color fpsColor = fps > lowFpsThreshold ? normalColor : 
                            (fps > criticalFpsThreshold ? warningColor : criticalColor);
            
            if (!showWarningsOnly || fpsColor != normalColor)
            {
                AppendColoredText("FPS: ", fpsColor);
                AppendColoredText($"{fps:F1}\n", fpsColor);
            }
        }
        
        if (showCpuTime)
        {
            Color cpuColor = cpuFrameTime < highCpuTimeThreshold ? normalColor : 
                           (cpuFrameTime < criticalCpuTimeThreshold ? warningColor : criticalColor);
            
            if (!showWarningsOnly || cpuColor != normalColor)
            {
                AppendColoredText("CPU: ", cpuColor);
                AppendColoredText($"{cpuFrameTime:F1} ms\n", cpuColor);
            }
        }
        
        if (showGpuTime)
        {
            Color gpuColor = gpuFrameTime < highCpuTimeThreshold ? normalColor : 
                           (gpuFrameTime < criticalCpuTimeThreshold ? warningColor : criticalColor);
            
            if (!showWarningsOnly || gpuColor != normalColor)
            {
                AppendColoredText("GPU: ", gpuColor);
                AppendColoredText($"{gpuFrameTime:F1} ms\n", gpuColor);
            }
        }
        
        if (showMemory)
        {
            AppendColoredText("Memory: ", normalColor);
            AppendColoredText($"{memoryUsage:F1} MB\n", normalColor);
        }
        
        performanceText.text = stringBuilder.ToString();
    }
    
    private void AppendColoredText(string text, Color color)
    {
        stringBuilder.Append($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>");
    }
    
    /// <summary>
    /// Estimate GPU time based on total frame time minus CPU time
    /// This is not an accurate measurement but provides a rough estimate
    /// </summary>
    private IEnumerator EstimateGpuTime()
    {
        while (true)
        {
            // Estimate GPU time as the difference between total frame time and CPU time
            // This isn't perfect but gives a rough idea on mobile
            gpuFrameTime = Mathf.Max(0, frameTime - cpuFrameTime);
            
            yield return new WaitForSeconds(updateInterval);
        }
    }
    
    /// <summary>
    /// Toggles the display of performance stats
    /// </summary>
    public void TogglePerformanceDisplay()
    {
        showPerformanceStats = !showPerformanceStats;
        
        if (performanceText != null)
        {
            performanceText.gameObject.SetActive(showPerformanceStats);
        }
    }
    
    /// <summary>
    /// Sets which performance stats to display
    /// </summary>
    public void ConfigureDisplayOptions(bool fps, bool cpu, bool gpu, bool memory)
    {
        showFps = fps;
        showCpuTime = cpu;
        showGpuTime = gpu;
        showMemory = memory;
    }
    
    /// <summary>
    /// Sets whether to show only warnings
    /// </summary>
    public void SetShowWarningsOnly(bool warningsOnly)
    {
        showWarningsOnly = warningsOnly;
    }
}