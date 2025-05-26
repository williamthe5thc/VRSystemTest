using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VRM;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace VRInterview
{
    /// <summary>
    /// Debug visualizer for lip sync showing audio levels and blend shape values
    /// </summary>
    public class LipSyncDebugVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas debugCanvas;
        [SerializeField] private RectTransform debugPanel;
        [SerializeField] private Image audioLevelBar;
        [SerializeField] private Image mouthOpenBar;
        [SerializeField] private TextMeshProUGUI debugText;
        [SerializeField] private AudioSource targetAudioSource;
        [SerializeField] private VRMBlendShapeProxy blendShapeProxy;
        
        [Header("Settings")]
        [SerializeField] private bool enableOnStart = true;
        [SerializeField] private bool showNumericValues = true;
        [SerializeField] private float updateInterval = 0.05f;
        [SerializeField] private int sampleWindow = 1024;
        [SerializeField] private float audioSensitivity = 2f;
        [SerializeField] private KeyCode toggleKey = KeyCode.F2;
        
        // Private variables
        private float[] _samples;
        private float _lastUpdateTime = 0f;
        private float _audioLevel = 0f;
        private float _smoothAudioLevel = 0f;
        private float _smoothMouthOpen = 0f;
        private Dictionary<string, float> _debugValues = new Dictionary<string, float>();
        private bool _initialized = false;
        
        void Awake()
        {
            _samples = new float[sampleWindow];
            
            // If we don't have a canvas, create it
            if (debugCanvas == null)
            {
                CreateDebugUI();
            }
        }
        
        void Start()
        {
            StartCoroutine(DelayedStart());
        }
        
        private IEnumerator DelayedStart()
        {
            // Wait a moment to let other components initialize
            yield return new WaitForSeconds(1f);
            
            // Find components if not assigned
            FindComponents();
            
            // Initialize UI
            InitializeUI();
            
            _initialized = true;
            Debug.Log("LipSyncDebugVisualizer: Initialized");
        }
        
        void Update()
        {
            // Toggle visibility with key press
            if (Input.GetKeyDown(toggleKey) && debugCanvas != null)
            {
                debugCanvas.enabled = !debugCanvas.enabled;
                
                if (debugCanvas.enabled)
                {
                    Debug.Log("LipSyncDebugVisualizer: Enabled");
                }
            }
            
            if (!_initialized || !debugCanvas.enabled)
                return;
                
            // Update at specified interval
            if (Time.time - _lastUpdateTime < updateInterval)
                return;
                
            _lastUpdateTime = Time.time;
            
            // Update audio monitoring
            UpdateAudioLevel();
            
            // Update mouth shape monitoring
            UpdateMouthValue();
            
            // Update UI
            UpdateDebugDisplay();
        }
        
        private void FindComponents()
        {
            // Find audio source if not assigned
            if (targetAudioSource == null)
            {
                // Try to find from AudioPlayback first
                AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
                if (audioPlayback != null)
                {
                    targetAudioSource = audioPlayback.GetComponent<AudioSource>();
                    
                    // If not found on AudioPlayback, try getting it via reflection
                    if (targetAudioSource == null)
                    {
                        var field = audioPlayback.GetType().GetField("audioSource",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            
                        if (field != null)
                        {
                            targetAudioSource = field.GetValue(audioPlayback) as AudioSource;
                        }
                    }
                }
                
                // Fallback to any AudioSource
                if (targetAudioSource == null)
                {
                    targetAudioSource = FindObjectOfType<AudioSource>();
                }
                
                if (targetAudioSource != null)
                {
                    Debug.Log("LipSyncDebugVisualizer: Found AudioSource");
                }
            }
            
            // Find blend shape proxy if not assigned
            if (blendShapeProxy == null)
            {
                blendShapeProxy = FindObjectOfType<VRMBlendShapeProxy>();
                if (blendShapeProxy != null)
                {
                    Debug.Log("LipSyncDebugVisualizer: Found VRMBlendShapeProxy");
                }
            }
        }
        
        private void CreateDebugUI()
        {
            // Create canvas GameObject
            GameObject canvasObj = new GameObject("LipSyncDebugCanvas");
            debugCanvas = canvasObj.AddComponent<Canvas>();
            debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create panel
            GameObject panelObj = new GameObject("DebugPanel");
            panelObj.transform.SetParent(debugCanvas.transform, false);
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            debugPanel = panelImage.rectTransform;
            debugPanel.anchorMin = new Vector2(0, 0);
            debugPanel.anchorMax = new Vector2(0.3f, 0.3f);
            debugPanel.pivot = new Vector2(0, 0);
            debugPanel.offsetMin = new Vector2(10, 10);
            debugPanel.offsetMax = new Vector2(10, 10);
            
            // Create audio level bar
            GameObject audioBarObj = new GameObject("AudioBar");
            audioBarObj.transform.SetParent(debugPanel, false);
            audioLevelBar = audioBarObj.AddComponent<Image>();
            audioLevelBar.color = Color.green;
            RectTransform audioRect = audioLevelBar.rectTransform;
            audioRect.anchorMin = new Vector2(0, 0.9f);
            audioRect.anchorMax = new Vector2(0, 0.95f);
            audioRect.pivot = new Vector2(0, 0.5f);
            audioRect.offsetMin = new Vector2(10, 0);
            audioRect.offsetMax = new Vector2(10, 0);
            
            // Create mouth open bar
            GameObject mouthBarObj = new GameObject("MouthBar");
            mouthBarObj.transform.SetParent(debugPanel, false);
            mouthOpenBar = mouthBarObj.AddComponent<Image>();
            mouthOpenBar.color = Color.cyan;
            RectTransform mouthRect = mouthOpenBar.rectTransform;
            mouthRect.anchorMin = new Vector2(0, 0.8f);
            mouthRect.anchorMax = new Vector2(0, 0.85f);
            mouthRect.pivot = new Vector2(0, 0.5f);
            mouthRect.offsetMin = new Vector2(10, 0);
            mouthRect.offsetMax = new Vector2(10, 0);
            
            // Create debug text
            GameObject textObj = new GameObject("DebugText");
            textObj.transform.SetParent(debugPanel, false);
            debugText = textObj.AddComponent<TextMeshProUGUI>();
            debugText.fontSize = 14;
            debugText.color = Color.white;
            RectTransform textRect = debugText.rectTransform;
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 0.8f);
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -30);
            
            Debug.Log("LipSyncDebugVisualizer: Created debug UI elements");
        }
        
        private void InitializeUI()
        {
            if (debugCanvas != null)
            {
                debugCanvas.enabled = enableOnStart;
            }
            
            if (debugText != null)
            {
                debugText.text = "Initializing lip sync debug...";
            }
            
            // Initialize bars with zero width
            if (audioLevelBar != null)
            {
                audioLevelBar.rectTransform.sizeDelta = new Vector2(0, audioLevelBar.rectTransform.sizeDelta.y);
            }
            
            if (mouthOpenBar != null)
            {
                mouthOpenBar.rectTransform.sizeDelta = new Vector2(0, mouthOpenBar.rectTransform.sizeDelta.y);
            }
        }
        
        private void UpdateAudioLevel()
        {
            if (targetAudioSource == null) return;
            
            _debugValues["Audio Playing"] = targetAudioSource.isPlaying ? 1 : 0;
            
            if (targetAudioSource.isPlaying)
            {
                // Get spectrum data
                targetAudioSource.GetSpectrumData(_samples, 0, FFTWindow.BlackmanHarris);
                
                // Calculate level
                float sum = 0;
                for (int i = 0; i < _samples.Length; i++)
                {
                    sum += _samples[i];
                }
                
                _audioLevel = Mathf.Clamp01(sum / _samples.Length * 100f * audioSensitivity);
                _smoothAudioLevel = Mathf.Lerp(_smoothAudioLevel, _audioLevel, Time.deltaTime * 10f);
                
                _debugValues["Audio Level"] = _audioLevel;
                _debugValues["Smooth Audio"] = _smoothAudioLevel;
                
                // Update audio level bar
                if (audioLevelBar != null)
                {
                    float width = debugPanel.rect.width * _smoothAudioLevel;
                    audioLevelBar.rectTransform.sizeDelta = new Vector2(width, audioLevelBar.rectTransform.sizeDelta.y);
                }
            }
            else
            {
                _audioLevel = 0;
                _smoothAudioLevel = Mathf.Lerp(_smoothAudioLevel, 0, Time.deltaTime * 5f);
                
                if (audioLevelBar != null)
                {
                    audioLevelBar.rectTransform.sizeDelta = new Vector2(0, audioLevelBar.rectTransform.sizeDelta.y);
                }
            }
        }
        
        private void UpdateMouthValue()
        {
            float mouthOpenValue = 0;
            
            if (blendShapeProxy != null)
            {
                try
                {
                    // Get the "A" blend shape value (divided by 100 as VRM uses 0-100 range)
                    mouthOpenValue = blendShapeProxy.GetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A)) / 100f;
                    _debugValues["Mouth Open"] = mouthOpenValue;
                }
                catch (System.Exception)
                {
                    // Ignore exceptions when getting blend shape values
                }
            }
            
            // Also try to get from VRMLipSync if available
            if (mouthOpenValue <= 0)
            {
                // Try to find VRMLipSync
                var vrmLipSync = FindObjectOfType<VRMLipSync>();
                if (vrmLipSync != null)
                {
                    // Try to get the value via reflection
                    try
                    {
                        var field = vrmLipSync.GetType().GetField("_currentLipSyncValue",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                            
                        if (field != null)
                        {
                            float value = (float)field.GetValue(vrmLipSync);
                            mouthOpenValue = value;
                            _debugValues["Lip Sync Value"] = value;
                        }
                    }
                    catch (System.Exception)
                    {
                        // Ignore reflection errors
                    }
                }
                
                // Try EnhancedVRMLipSync if available
                var enhancedLipSync = FindObjectOfType<EnhancedVRMLipSync>();
                if (enhancedLipSync != null)
                {
                    try
                    {
                        var field = enhancedLipSync.GetType().GetField("_currentLipSyncValue",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                            
                        if (field != null)
                        {
                            float value = (float)field.GetValue(enhancedLipSync);
                            mouthOpenValue = value;
                            _debugValues["Enhanced Lip Value"] = value;
                        }
                    }
                    catch (System.Exception)
                    {
                        // Ignore reflection errors
                    }
                }
            }
            
            // Smooth the value
            _smoothMouthOpen = Mathf.Lerp(_smoothMouthOpen, mouthOpenValue, Time.deltaTime * 10f);
            _debugValues["Smooth Mouth"] = _smoothMouthOpen;
            
            // Update mouth open bar
            if (mouthOpenBar != null)
            {
                float width = debugPanel.rect.width * _smoothMouthOpen;
                mouthOpenBar.rectTransform.sizeDelta = new Vector2(width, mouthOpenBar.rectTransform.sizeDelta.y);
            }
        }
        
        private void UpdateDebugDisplay()
        {
            if (debugText == null || !showNumericValues) return;
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // Title
            sb.AppendLine("<b>Lip Sync Debug</b>");
            
            // Audio status
            sb.AppendLine($"Audio Playing: {(targetAudioSource != null && targetAudioSource.isPlaying ? "Yes" : "No")}");
            
            // Component status
            sb.AppendLine($"AudioSource: {(targetAudioSource != null ? "OK" : "Missing")}");
            sb.AppendLine($"BlendShapeProxy: {(blendShapeProxy != null ? "OK" : "Missing")}");
            sb.AppendLine($"VRMLipSync: {(FindObjectOfType<VRMLipSync>() != null ? "OK" : "Missing")}");
            sb.AppendLine($"Enhanced: {(FindObjectOfType<EnhancedVRMLipSync>() != null ? "OK" : "Missing")}");
            
            sb.AppendLine();
            
            // Debug values
            foreach (var kvp in _debugValues)
            {
                sb.AppendLine($"{kvp.Key}: {kvp.Value:F3}");
            }
            
            // Set text
            debugText.text = sb.ToString();
        }
        
        // Public method to manually refresh component references
        public void RefreshComponents()
        {
            FindComponents();
            Debug.Log("LipSyncDebugVisualizer: Refreshed component references");
        }
        
        // Public method to toggle visibility
        public void ToggleVisibility()
        {
            if (debugCanvas != null)
            {
                debugCanvas.enabled = !debugCanvas.enabled;
                Debug.Log($"LipSyncDebugVisualizer: {(debugCanvas.enabled ? "Enabled" : "Disabled")}");
            }
        }
    }
}
