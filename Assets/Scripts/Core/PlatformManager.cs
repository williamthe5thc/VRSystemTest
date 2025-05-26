using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRInterview
{
    /// <summary>
    /// Handles platform-specific functionality and settings
    /// </summary>
    public class PlatformManager : MonoBehaviour
    {
        private static PlatformManager _instance;
        public static PlatformManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlatformManager");
                    _instance = go.AddComponent<PlatformManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public enum Platform
        {
            Windows,
            MacOS,
            Linux,
            Android,
            IOS,
            Unknown
        }

        public Platform CurrentPlatform { get; private set; }
        public bool IsStandalone { get; private set; }
        public bool IsMobile { get; private set; }
        public bool IsVR { get; private set; }

        [SerializeField] private string defaultWindowsServerUrl = "ws://localhost:8765";
        [SerializeField] private string defaultMacServerUrl = "ws://localhost:8765";
        [SerializeField] private string defaultLinuxServerUrl = "ws://localhost:8765";
        [SerializeField] private string defaultMobileServerUrl = "ws://192.168.1.100:8765";

        // Audio settings
        [SerializeField] private int defaultWindowsSampleRate = 16000;
        [SerializeField] private int defaultMacSampleRate = 44100; 
        [SerializeField] private int defaultLinuxSampleRate = 16000;
        [SerializeField] private int defaultMobileSampleRate = 16000;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Detect platform
            DetectPlatform();
            
            // Initialize settings
            ApplyPlatformSpecificSettings();

            Debug.Log($"Platform initialized: {CurrentPlatform}");
        }

        private void DetectPlatform()
        {
            // Detect runtime platform
            RuntimePlatform platform = Application.platform;
            
            switch (platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    CurrentPlatform = Platform.Windows;
                    IsStandalone = true;
                    break;
                
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    CurrentPlatform = Platform.MacOS;
                    IsStandalone = true;
                    break;
                
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    CurrentPlatform = Platform.Linux;
                    IsStandalone = true;
                    break;
                
                case RuntimePlatform.Android:
                    CurrentPlatform = Platform.Android;
                    IsMobile = true;
                    break;
                
                case RuntimePlatform.IPhonePlayer:
                    CurrentPlatform = Platform.IOS;
                    IsMobile = true;
                    break;
                
                default:
                    CurrentPlatform = Platform.Unknown;
                    break;
            }

            // Check for VR
            IsVR = UnityEngine.XR.XRSettings.enabled;
        }

        private void ApplyPlatformSpecificSettings()
        {
            // Apply default settings based on platform
            switch (CurrentPlatform)
            {
                case Platform.Windows:
                    ApplyWindowsSettings();
                    break;
                
                case Platform.MacOS:
                    ApplyMacSettings();
                    break;
                
                case Platform.Linux:
                    ApplyLinuxSettings();
                    break;
                
                case Platform.Android:
                case Platform.IOS:
                    ApplyMobileSettings();
                    break;
                
                default:
                    ApplyDefaultSettings();
                    break;
            }
        }

        private void ApplyWindowsSettings()
        {
            // Apply Windows-specific settings
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetSetting("ServerUrl", PlayerPrefs.GetString("ServerUrl", defaultWindowsServerUrl));
                SettingsManager.Instance.SetSetting("SampleRate", PlayerPrefs.GetInt("SampleRate", defaultWindowsSampleRate));
            }
        }

        private void ApplyMacSettings()
        {
            // Apply Mac-specific settings
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetSetting("ServerUrl", PlayerPrefs.GetString("ServerUrl", defaultMacServerUrl));
                SettingsManager.Instance.SetSetting("SampleRate", PlayerPrefs.GetInt("SampleRate", defaultMacSampleRate));
            }
            
            // Mac-specific audio settings
            AudioSettings.outputSampleRate = defaultMacSampleRate;
        }

        private void ApplyLinuxSettings()
        {
            // Apply Linux-specific settings
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetSetting("ServerUrl", PlayerPrefs.GetString("ServerUrl", defaultLinuxServerUrl));
                SettingsManager.Instance.SetSetting("SampleRate", PlayerPrefs.GetInt("SampleRate", defaultLinuxSampleRate));
            }
        }

        private void ApplyMobileSettings()
        {
            // Apply mobile-specific settings
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetSetting("ServerUrl", PlayerPrefs.GetString("ServerUrl", defaultMobileServerUrl));
                SettingsManager.Instance.SetSetting("SampleRate", PlayerPrefs.GetInt("SampleRate", defaultMobileSampleRate));
            }
        }

        private void ApplyDefaultSettings()
        {
            // Apply default settings
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetSetting("ServerUrl", defaultWindowsServerUrl);
                SettingsManager.Instance.SetSetting("SampleRate", defaultWindowsSampleRate);
            }
        }

        /// <summary>
        /// Gets the appropriate microphone device for the current platform
        /// </summary>
        public string GetDefaultMicrophoneDevice()
        {
            string[] devices = Microphone.devices;
            
            if (devices.Length == 0)
            {
                Debug.LogError("No microphone devices available");
                return null;
            }

            switch (CurrentPlatform)
            {
                case Platform.MacOS:
                    // On Mac, prefer device with "Built-in" in the name if available
                    foreach (string device in devices)
                    {
                        if (device.Contains("Built-in") || device.Contains("MacBook"))
                        {
                            return device;
                        }
                    }
                    break;
                
                case Platform.Android:
                case Platform.IOS:
                    // On mobile, prefer device with "Microphone" in the name if available
                    foreach (string device in devices)
                    {
                        if (device.Contains("Microphone"))
                        {
                            return device;
                        }
                    }
                    break;
            }

            // Default to first device
            return devices[0];
        }

        /// <summary>
        /// Gets the optimal sample rate for the current platform
        /// </summary>
        public int GetOptimalSampleRate()
        {
            switch (CurrentPlatform)
            {
                case Platform.Windows:
                    return defaultWindowsSampleRate;
                
                case Platform.MacOS:
                    return defaultMacSampleRate;
                
                case Platform.Linux:
                    return defaultLinuxSampleRate;
                
                case Platform.Android:
                case Platform.IOS:
                    return defaultMobileSampleRate;
                
                default:
                    return 16000;
            }
        }

        /// <summary>
        /// Gets the WebSocket URL appropriate for the current platform
        /// </summary>
        public string GetDefaultServerUrl()
        {
            switch (CurrentPlatform)
            {
                case Platform.Windows:
                    return defaultWindowsServerUrl;
                
                case Platform.MacOS:
                    return defaultMacServerUrl;
                
                case Platform.Linux:
                    return defaultLinuxServerUrl;
                
                case Platform.Android:
                case Platform.IOS:
                    return defaultMobileServerUrl;
                
                default:
                    return defaultWindowsServerUrl;
            }
        }
    }
}
