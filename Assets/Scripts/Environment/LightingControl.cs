using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages lighting conditions in the interview environment.
/// </summary>
public class LightingControl : MonoBehaviour
{
    [System.Serializable]
    public class LightingPreset
    {
        public string presetName = "Default";
        public Color ambientColor = Color.white;
        public float ambientIntensity = 1.0f;
        public Color directionalLightColor = Color.white;
        public float directionalLightIntensity = 1.0f;
        public Vector3 directionalLightRotation = new Vector3(50f, -30f, 0f);
        public Color accentLightColor = Color.white;
        public float accentLightIntensity = 0.5f;
    }
    
    [Header("Lighting References")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private List<Light> accentLights = new List<Light>();
    
    [Header("Lighting Presets")]
    [SerializeField] private List<LightingPreset> lightingPresets = new List<LightingPreset>();
    [SerializeField] private string defaultPreset = "Neutral";
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private string _currentPreset;
    private Coroutine _transitionCoroutine;
    
    // Events
    public event Action<string> OnLightingPresetChanged;
    
    private void Start()
    {
        InitializeComponents();
        
        // Apply default preset on start
        ApplyPreset(defaultPreset);
    }
    
    /// <summary>
    /// Initializes required components if not assigned.
    /// </summary>
    private void InitializeComponents()
    {
        // Find directional light if not assigned
        if (directionalLight == null)
        {
            // Try to find the main directional light
            Light[] lights = FindObjectsOfType<Light>();
            
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
            
            if (directionalLight == null)
            {
                Debug.LogWarning("Directional light not found! Creating default.");
                
                // Create a default directional light
                GameObject lightObj = new GameObject("DirectionalLight");
                directionalLight = lightObj.AddComponent<Light>();
                directionalLight.type = LightType.Directional;
                directionalLight.intensity = 1.0f;
                directionalLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }
        
        // Find accent lights if empty
        if (accentLights.Count == 0)
        {
            Light[] lights = FindObjectsOfType<Light>();
            
            foreach (Light light in lights)
            {
                if (light.type != LightType.Directional && light != directionalLight)
                {
                    accentLights.Add(light);
                }
            }
        }
        
        // Add default presets if empty
        if (lightingPresets.Count == 0)
        {
            AddDefaultPresets();
        }
    }
    
    /// <summary>
    /// Adds default lighting presets if none are defined.
    /// </summary>
    private void AddDefaultPresets()
    {
        // Neutral (Default) preset
        LightingPreset neutral = new LightingPreset
        {
            presetName = "Neutral",
            ambientColor = new Color(0.5f, 0.5f, 0.5f),
            ambientIntensity = 1.0f,
            directionalLightColor = new Color(1.0f, 0.98f, 0.9f),
            directionalLightIntensity = 1.0f,
            directionalLightRotation = new Vector3(50f, -30f, 0f),
            accentLightColor = new Color(1.0f, 1.0f, 1.0f),
            accentLightIntensity = 0.5f
        };
        
        // Warm preset
        LightingPreset warm = new LightingPreset
        {
            presetName = "Warm",
            ambientColor = new Color(0.5f, 0.4f, 0.3f),
            ambientIntensity = 0.8f,
            directionalLightColor = new Color(1.0f, 0.9f, 0.8f),
            directionalLightIntensity = 1.2f,
            directionalLightRotation = new Vector3(45f, -20f, 0f),
            accentLightColor = new Color(1.0f, 0.8f, 0.6f),
            accentLightIntensity = 0.7f
        };
        
        // Cool preset
        LightingPreset cool = new LightingPreset
        {
            presetName = "Cool",
            ambientColor = new Color(0.3f, 0.4f, 0.5f),
            ambientIntensity = 0.8f,
            directionalLightColor = new Color(0.8f, 0.9f, 1.0f),
            directionalLightIntensity = 1.1f,
            directionalLightRotation = new Vector3(60f, -40f, 0f),
            accentLightColor = new Color(0.6f, 0.8f, 1.0f),
            accentLightIntensity = 0.6f
        };
        
        // Dark preset
        LightingPreset dark = new LightingPreset
        {
            presetName = "Dark",
            ambientColor = new Color(0.2f, 0.2f, 0.3f),
            ambientIntensity = 0.5f,
            directionalLightColor = new Color(0.7f, 0.7f, 0.8f),
            directionalLightIntensity = 0.8f,
            directionalLightRotation = new Vector3(70f, -10f, 0f),
            accentLightColor = new Color(0.8f, 0.7f, 1.0f),
            accentLightIntensity = 0.8f
        };
        
        // Bright preset
        LightingPreset bright = new LightingPreset
        {
            presetName = "Bright",
            ambientColor = new Color(0.6f, 0.6f, 0.6f),
            ambientIntensity = 1.2f,
            directionalLightColor = new Color(1.0f, 1.0f, 1.0f),
            directionalLightIntensity = 1.5f,
            directionalLightRotation = new Vector3(40f, -30f, 0f),
            accentLightColor = new Color(1.0f, 1.0f, 0.9f),
            accentLightIntensity = 0.4f
        };
        
        // Add presets to list
        lightingPresets.Add(neutral);
        lightingPresets.Add(warm);
        lightingPresets.Add(cool);
        lightingPresets.Add(dark);
        lightingPresets.Add(bright);
        
        if (debugMode)
        {
            Debug.Log("Added default lighting presets");
        }
    }
    
    /// <summary>
    /// Applies a lighting preset by name.
    /// </summary>
    /// <param name="presetName">Name of the preset to apply.</param>
    public void ApplyPreset(string presetName)
    {
        // Find the preset
        LightingPreset preset = lightingPresets.Find(p => p.presetName == presetName);
        
        if (preset == null)
        {
            Debug.LogWarning($"Lighting preset '{presetName}' not found! Using default.");
            
            // Try to use default preset
            preset = lightingPresets.Find(p => p.presetName == defaultPreset);
            
            // If still null, use first available preset
            if (preset == null && lightingPresets.Count > 0)
            {
                preset = lightingPresets[0];
            }
            
            if (preset == null)
            {
                Debug.LogError("No lighting presets available!");
                return;
            }
        }
        
        // Stop any ongoing transition
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
        }
        
        // Start transition to new preset
        _transitionCoroutine = StartCoroutine(TransitionToPreset(preset));
        
        // Update current preset
        _currentPreset = preset.presetName;
        
        if (debugMode)
        {
            Debug.Log($"Applied lighting preset: {preset.presetName}");
        }
    }
    
    /// <summary>
    /// Transitions smoothly to a new lighting preset.
    /// </summary>
    /// <param name="targetPreset">The target preset.</param>
    private IEnumerator TransitionToPreset(LightingPreset targetPreset)
    {
        // Store initial values
        Color initialAmbientColor = RenderSettings.ambientLight;
        float initialAmbientIntensity = RenderSettings.ambientIntensity;
        
        Color initialDirectionalColor = Color.white;
        float initialDirectionalIntensity = 1.0f;
        Quaternion initialDirectionalRotation = Quaternion.identity;
        
        if (directionalLight != null)
        {
            initialDirectionalColor = directionalLight.color;
            initialDirectionalIntensity = directionalLight.intensity;
            initialDirectionalRotation = directionalLight.transform.rotation;
        }
        
        // Store initial accent light values
        List<Color> initialAccentColors = new List<Color>();
        List<float> initialAccentIntensities = new List<float>();
        
        foreach (Light light in accentLights)
        {
            if (light != null)
            {
                initialAccentColors.Add(light.color);
                initialAccentIntensities.Add(light.intensity);
            }
        }
        
        // Perform transition
        float timer = 0f;
        
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = transitionCurve.Evaluate(Mathf.Clamp01(timer / transitionDuration));
            
            // Update ambient lighting
            RenderSettings.ambientLight = Color.Lerp(initialAmbientColor, targetPreset.ambientColor, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(initialAmbientIntensity, targetPreset.ambientIntensity, t);
            
            // Update directional light
            if (directionalLight != null)
            {
                directionalLight.color = Color.Lerp(initialDirectionalColor, targetPreset.directionalLightColor, t);
                directionalLight.intensity = Mathf.Lerp(initialDirectionalIntensity, targetPreset.directionalLightIntensity, t);
                directionalLight.transform.rotation = Quaternion.Lerp(
                    initialDirectionalRotation, 
                    Quaternion.Euler(targetPreset.directionalLightRotation),
                    t
                );
            }
            
            // Update accent lights
            for (int i = 0; i < accentLights.Count; i++)
            {
                if (accentLights[i] != null && i < initialAccentColors.Count)
                {
                    accentLights[i].color = Color.Lerp(initialAccentColors[i], targetPreset.accentLightColor, t);
                    accentLights[i].intensity = Mathf.Lerp(initialAccentIntensities[i], targetPreset.accentLightIntensity, t);
                }
            }
            
            yield return null;
        }
        
        // Ensure final values are set exactly
        RenderSettings.ambientLight = targetPreset.ambientColor;
        RenderSettings.ambientIntensity = targetPreset.ambientIntensity;
        
        if (directionalLight != null)
        {
            directionalLight.color = targetPreset.directionalLightColor;
            directionalLight.intensity = targetPreset.directionalLightIntensity;
            directionalLight.transform.rotation = Quaternion.Euler(targetPreset.directionalLightRotation);
        }
        
        foreach (Light light in accentLights)
        {
            if (light != null)
            {
                light.color = targetPreset.accentLightColor;
                light.intensity = targetPreset.accentLightIntensity;
            }
        }
        
        // Notify listeners
        OnLightingPresetChanged?.Invoke(targetPreset.presetName);
        
        _transitionCoroutine = null;
    }
    
    /// <summary>
    /// Gets the current lighting preset name.
    /// </summary>
    /// <returns>The current preset name.</returns>
    public string GetCurrentPreset()
    {
        return _currentPreset;
    }
    
    /// <summary>
    /// Gets all available lighting preset names.
    /// </summary>
    /// <returns>Array of preset names.</returns>
    public string[] GetAvailablePresets()
    {
        string[] presetNames = new string[lightingPresets.Count];
        
        for (int i = 0; i < lightingPresets.Count; i++)
        {
            presetNames[i] = lightingPresets[i].presetName;
        }
        
        return presetNames;
    }
    
    /// <summary>
    /// Adds a new lighting preset at runtime.
    /// </summary>
    /// <param name="preset">The preset to add.</param>
    public void AddPreset(LightingPreset preset)
    {
        // Check if preset with same name already exists
        int existingIndex = lightingPresets.FindIndex(p => p.presetName == preset.presetName);
        
        if (existingIndex >= 0)
        {
            // Replace existing preset
            lightingPresets[existingIndex] = preset;
            
            if (debugMode)
            {
                Debug.Log($"Updated existing lighting preset: {preset.presetName}");
            }
        }
        else
        {
            // Add new preset
            lightingPresets.Add(preset);
            
            if (debugMode)
            {
                Debug.Log($"Added new lighting preset: {preset.presetName}");
            }
        }
    }
}