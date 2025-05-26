using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages user settings and preferences for the VR Interview System.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    // Default settings
    [Header("Server Settings")]
    [SerializeField] private string defaultServerUrl = "ws://localhost:8765";
    
    [Header("Audio Settings")]
    [SerializeField] private float defaultVolume = 0.8f;
    [SerializeField] private string defaultMicrophone = "";
    [SerializeField] private int defaultSampleRate = 16000;
    
    [Header("Environment Settings")]
    [SerializeField] private string defaultEnvironment = "CorporateOffice";
    
    [Header("Avatar Settings")]
    [SerializeField] private string defaultAvatar = "DefaultInterviewer";
    
    // Current settings
    private Dictionary<string, object> _settings = new Dictionary<string, object>();
    
    // Events
    public event Action OnSettingsChanged;
    
    // Singleton pattern
    private static SettingsManager _instance;
    public static SettingsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("SettingsManager instance not found!");
            }
            return _instance;
        }
    }
    
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
    }
    
    /// <summary>
    /// Loads settings from PlayerPrefs or initializes defaults if not found.
    /// </summary>
    public void LoadSettings()
    {
        Debug.Log("Loading settings...");
        
        // Server settings
        SetSetting("ServerUrl", PlayerPrefs.GetString("ServerUrl", defaultServerUrl));
        
        // Audio settings
        SetSetting("Volume", PlayerPrefs.GetFloat("Volume", defaultVolume));
        SetSetting("Microphone", PlayerPrefs.GetString("Microphone", defaultMicrophone));
        SetSetting("SampleRate", PlayerPrefs.GetInt("SampleRate", defaultSampleRate));
        
        // Environment settings
        SetSetting("Environment", PlayerPrefs.GetString("Environment", defaultEnvironment));
        
        // Avatar settings
        SetSetting("Avatar", PlayerPrefs.GetString("Avatar", defaultAvatar));
        
        Debug.Log("Settings loaded successfully.");
    }
    
    /// <summary>
    /// Saves current settings to PlayerPrefs.
    /// </summary>
    public void SaveSettings()
    {
        Debug.Log("Saving settings...");
        
        // Server settings
        PlayerPrefs.SetString("ServerUrl", GetSetting<string>("ServerUrl"));
        
        // Audio settings
        PlayerPrefs.SetFloat("Volume", GetSetting<float>("Volume"));
        PlayerPrefs.SetString("Microphone", GetSetting<string>("Microphone"));
        PlayerPrefs.SetInt("SampleRate", GetSetting<int>("SampleRate"));
        
        // Environment settings
        PlayerPrefs.SetString("Environment", GetSetting<string>("Environment"));
        
        // Avatar settings
        PlayerPrefs.SetString("Avatar", GetSetting<string>("Avatar"));
        
        // Save to disk
        PlayerPrefs.Save();
        
        Debug.Log("Settings saved successfully.");
    }
    
    /// <summary>
    /// Gets a setting value with the specified key.
    /// </summary>
    /// <typeparam name="T">Type of the setting value.</typeparam>
    /// <param name="key">Setting key.</param>
    /// <returns>Setting value or default if not found.</returns>
    public T GetSetting<T>(string key)
    {
        if (_settings.TryGetValue(key, out object value))
        {
            if (value is T typedValue)
            {
                return typedValue;
            }
        }
        
        // Return default value for the type
        return default;
    }
    
    /// <summary>
    /// Sets a setting value with the specified key.
    /// </summary>
    /// <typeparam name="T">Type of the setting value.</typeparam>
    /// <param name="key">Setting key.</param>
    /// <param name="value">Setting value.</param>
    public void SetSetting<T>(string key, T value)
    {
        _settings[key] = value;
        
        // Notify listeners
        OnSettingsChanged?.Invoke();
    }
    
    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    public void ResetToDefaults()
    {
        Debug.Log("Resetting settings to defaults...");
        
        // Clear all settings
        _settings.Clear();
        
        // Set default values
        SetSetting("ServerUrl", defaultServerUrl);
        SetSetting("Volume", defaultVolume);
        SetSetting("Microphone", defaultMicrophone);
        SetSetting("SampleRate", defaultSampleRate);
        SetSetting("Environment", defaultEnvironment);
        SetSetting("Avatar", defaultAvatar);
        
        // Save to disk
        SaveSettings();
        
        Debug.Log("Settings reset to defaults.");
    }
}