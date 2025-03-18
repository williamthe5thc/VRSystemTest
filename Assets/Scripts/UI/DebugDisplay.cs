using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private int maxLines = 20;
    [SerializeField] private bool showTimestamp = true;
    [SerializeField] private bool persistentLog = true;
    
    private Queue<string> logQueue = new Queue<string>();
    
    private static DebugDisplay instance;
    
    public static DebugDisplay Instance => instance;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // Keep this object when loading new scenes if persistent
        if (persistentLog)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    
    private void OnEnable()
    {
        // Subscribe to Unity's debug log events
        Application.logMessageReceived += HandleUnityLog;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from Unity's debug log events
        Application.logMessageReceived -= HandleUnityLog;
    }
    
    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        // Filter out warnings and errors if desired
        if (type == LogType.Error || type == LogType.Exception)
        {
            Log($"<color=red>[ERROR]</color> {logString}");
        }
        else if (type == LogType.Warning)
        {
            Log($"<color=yellow>[WARNING]</color> {logString}");
        }
        else
        {
            // Only log regular Debug.Log messages if desired
            // Uncomment the line below to log normal debug messages
            // Log($"[LOG] {logString}");
        }
    }
    
    public void Log(string message)
    {
        if (debugText == null) return;
        
        // Add timestamp if enabled
        if (showTimestamp)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            message = $"[{timestamp}] {message}";
        }
        
        // Add to queue and keep queue size limited
        logQueue.Enqueue(message);
        if (logQueue.Count > maxLines)
        {
            logQueue.Dequeue();
        }
        
        // Update display
        UpdateDebugText();
    }
    
    private void UpdateDebugText()
    {
        if (debugText == null) return;
        
        // Build text from queue
        debugText.text = string.Join("\n", logQueue);
    }
    
    public void Clear()
    {
        logQueue.Clear();
        
        if (debugText != null)
        {
            debugText.text = "";
        }
    }
    
    // Static methods for easier access
    public static void LogMessage(string message)
    {
        if (instance != null)
        {
            instance.Log(message);
        }
    }
    
    public static void ClearLog()
    {
        if (instance != null)
        {
            instance.Clear();
        }
    }
}
