using UnityEngine;
using System.Collections;

/// <summary>
/// Controls dynamic lighting effects in the interview environment.
/// Provides smooth transitions between lighting states and ambient light changes.
/// </summary>
public class DynamicLightingController : MonoBehaviour
{
    [Header("Light References")]
    [SerializeField] private Light mainDirectionalLight;
    [SerializeField] private Light[] accentLights;
    [SerializeField] private Light[] ambientLights;
    
    [Header("Lighting Presets")]
    [SerializeField] private LightingPreset defaultLighting;
    [SerializeField] private LightingPreset focusedLighting;
    [SerializeField] private LightingPreset dramaticLighting;
    [SerializeField] private LightingPreset eveningLighting;
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionSpeed = 1.0f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Dynamic Effects")]
    [SerializeField] private bool enableDynamicEffects = true;
    [SerializeField] private float subtleIntensityVariation = 0.05f;
    [SerializeField] private float intensityChangeSpeed = 0.5f;
    
    // Current state tracking
    private LightingPreset currentPreset;
    private LightingPreset targetPreset;
    private float transitionProgress = 1.0f;
    private Coroutine activeTransition;
    private Coroutine dynamicEffectCoroutine;
    
    [System.Serializable]
    public class LightingPreset
    {
        public string presetName = "New Preset";
        
        [Header("Directional Light")]
        public Color directionalColor = Color.white;
        public float directionalIntensity = 1.0f;
        public Vector3 directionalRotation = new Vector3(50, -30, 0);
        
        [Header("Accent Lights")]
        public Color accentColor = Color.white;
        public float accentIntensity = 1.0f;
        
        [Header("Ambient Lights")]
        public Color ambientColor = Color.white;
        public float ambientIntensity = 0.5f;
        
        [Header("Environment Lighting")]
        public Color ambientSkyColor = Color.blue;
        public Color ambientEquatorColor = Color.gray;
        public Color ambientGroundColor = Color.gray;
        public float ambientIntensityMultiplier = 1.0f;
        public float reflectionIntensity = 1.0f;
    }
    
    private void Awake()
    {
        // If no directional light is set, try to find one
        if (mainDirectionalLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    mainDirectionalLight = light;
                    break;
                }
            }
            
            if (mainDirectionalLight == null)
            {
                Debug.LogWarning("No directional light found in the scene!");
            }
        }
        
        // Set up default preset if not defined
        if (defaultLighting == null)
        {
            defaultLighting = new LightingPreset();
            defaultLighting.presetName = "Default";
        }
        
        // Initialize with default lighting
        currentPreset = defaultLighting;
        targetPreset = defaultLighting;
        
        // Apply default lighting
        ApplyLightingPreset(defaultLighting, 0f);
    }
    
    private void Start()
    {
        // Start dynamic effects if enabled
        if (enableDynamicEffects)
        {
            StartDynamicEffects();
        }
    }
    
    private void OnDestroy()
    {
        // Clean up coroutines
        if (activeTransition != null)
        {
            StopCoroutine(activeTransition);
        }
        
        if (dynamicEffectCoroutine != null)
        {
            StopCoroutine(dynamicEffectCoroutine);
        }
    }
    
    /// <summary>
    /// Transition to the default lighting preset.
    /// </summary>
    /// <param name="transitionTime">Duration of the transition in seconds.</param>
    public void TransitionToDefault(float transitionTime = -1)
    {
        if (defaultLighting != null)
        {
            TransitionToPreset(defaultLighting, transitionTime);
        }
    }
    
    /// <summary>
    /// Transition to the focused lighting preset.
    /// </summary>
    /// <param name="transitionTime">Duration of the transition in seconds.</param>
    public void TransitionToFocused(float transitionTime = -1)
    {
        if (focusedLighting != null)
        {
            TransitionToPreset(focusedLighting, transitionTime);
        }
    }
    
    /// <summary>
    /// Transition to the dramatic lighting preset.
    /// </summary>
    /// <param name="transitionTime">Duration of the transition in seconds.</param>
    public void TransitionToDramatic(float transitionTime = -1)
    {
        if (dramaticLighting != null)
        {
            TransitionToPreset(dramaticLighting, transitionTime);
        }
    }
    
    /// <summary>
    /// Transition to the evening lighting preset.
    /// </summary>
    /// <param name="transitionTime">Duration of the transition in seconds.</param>
    public void TransitionToEvening(float transitionTime = -1)
    {
        if (eveningLighting != null)
        {
            TransitionToPreset(eveningLighting, transitionTime);
        }
    }
    
    /// <summary>
    /// Transition to a specific lighting preset.
    /// </summary>
    /// <param name="preset">The lighting preset to transition to.</param>
    /// <param name="transitionTime">Duration of the transition in seconds. If negative, uses the default transition speed.</param>
    public void TransitionToPreset(LightingPreset preset, float transitionTime = -1)
    {
        if (preset == null) return;
        
        // If already transitioning, stop the current transition
        if (activeTransition != null)
        {
            StopCoroutine(activeTransition);
        }
        
        // Calculate transition time
        float duration = (transitionTime < 0) ? (1.0f / transitionSpeed) : transitionTime;
        
        // Start transition coroutine
        targetPreset = preset;
        transitionProgress = 0;
        activeTransition = StartCoroutine(LightingTransition(currentPreset, targetPreset, duration));
    }
    
    /// <summary>
    /// Coroutine to handle smooth transitions between lighting presets.
    /// </summary>
    private IEnumerator LightingTransition(LightingPreset fromPreset, LightingPreset toPreset, float duration)
    {
        float startTime = Time.time;
        float elapsedTime = 0;
        
        // Perform the transition
        while (elapsedTime < duration)
        {
            elapsedTime = Time.time - startTime;
            transitionProgress = Mathf.Clamp01(elapsedTime / duration);
            
            // Apply easing curve
            float curvedProgress = transitionCurve.Evaluate(transitionProgress);
            
            // Apply interpolated lighting
            ApplyLightingPreset(fromPreset, toPreset, curvedProgress);
            
            yield return null;
        }
        
        // Ensure we end at exactly the target values
        ApplyLightingPreset(toPreset, 0f);
        currentPreset = toPreset;
        transitionProgress = 1.0f;
        activeTransition = null;
    }
    
    /// <summary>
    /// Apply a single lighting preset directly.
    /// </summary>
    private void ApplyLightingPreset(LightingPreset preset, float blendFactor)
    {
        // Apply directional light settings
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.color = preset.directionalColor;
            mainDirectionalLight.intensity = preset.directionalIntensity;
            mainDirectionalLight.transform.rotation = Quaternion.Euler(preset.directionalRotation);
        }
        
        // Apply accent light settings
        if (accentLights != null)
        {
            foreach (Light light in accentLights)
            {
                if (light != null)
                {
                    light.color = preset.accentColor;
                    light.intensity = preset.accentIntensity;
                }
            }
        }
        
        // Apply ambient light settings
        if (ambientLights != null)
        {
            foreach (Light light in ambientLights)
            {
                if (light != null)
                {
                    light.color = preset.ambientColor;
                    light.intensity = preset.ambientIntensity;
                }
            }
        }
        
        // Apply environment lighting
        RenderSettings.ambientSkyColor = preset.ambientSkyColor;
        RenderSettings.ambientEquatorColor = preset.ambientEquatorColor;
        RenderSettings.ambientGroundColor = preset.ambientGroundColor;
        RenderSettings.ambientIntensity = preset.ambientIntensityMultiplier;
        RenderSettings.reflectionIntensity = preset.reflectionIntensity;
    }
    
    /// <summary>
    /// Apply a blended lighting preset between two states.
    /// </summary>
    private void ApplyLightingPreset(LightingPreset fromPreset, LightingPreset toPreset, float blendFactor)
    {
        // Blend directional light settings
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.color = Color.Lerp(fromPreset.directionalColor, toPreset.directionalColor, blendFactor);
            mainDirectionalLight.intensity = Mathf.Lerp(fromPreset.directionalIntensity, toPreset.directionalIntensity, blendFactor);
            
            // Blend rotation correctly using quaternions
            Quaternion fromRotation = Quaternion.Euler(fromPreset.directionalRotation);
            Quaternion toRotation = Quaternion.Euler(toPreset.directionalRotation);
            mainDirectionalLight.transform.rotation = Quaternion.Slerp(fromRotation, toRotation, blendFactor);
        }
        
        // Blend accent light settings
        if (accentLights != null)
        {
            foreach (Light light in accentLights)
            {
                if (light != null)
                {
                    light.color = Color.Lerp(fromPreset.accentColor, toPreset.accentColor, blendFactor);
                    light.intensity = Mathf.Lerp(fromPreset.accentIntensity, toPreset.accentIntensity, blendFactor);
                }
            }
        }
        
        // Blend ambient light settings
        if (ambientLights != null)
        {
            foreach (Light light in ambientLights)
            {
                if (light != null)
                {
                    light.color = Color.Lerp(fromPreset.ambientColor, toPreset.ambientColor, blendFactor);
                    light.intensity = Mathf.Lerp(fromPreset.ambientIntensity, toPreset.ambientIntensity, blendFactor);
                }
            }
        }
        
        // Blend environment lighting
        RenderSettings.ambientSkyColor = Color.Lerp(fromPreset.ambientSkyColor, toPreset.ambientSkyColor, blendFactor);
        RenderSettings.ambientEquatorColor = Color.Lerp(fromPreset.ambientEquatorColor, toPreset.ambientEquatorColor, blendFactor);
        RenderSettings.ambientGroundColor = Color.Lerp(fromPreset.ambientGroundColor, toPreset.ambientGroundColor, blendFactor);
        RenderSettings.ambientIntensity = Mathf.Lerp(fromPreset.ambientIntensityMultiplier, toPreset.ambientIntensityMultiplier, blendFactor);
        RenderSettings.reflectionIntensity = Mathf.Lerp(fromPreset.reflectionIntensity, toPreset.reflectionIntensity, blendFactor);
    }
    
    /// <summary>
    /// Start the subtle dynamic lighting effects.
    /// </summary>
    public void StartDynamicEffects()
    {
        if (dynamicEffectCoroutine != null)
        {
            StopCoroutine(dynamicEffectCoroutine);
        }
        
        dynamicEffectCoroutine = StartCoroutine(DynamicLightingEffects());
    }
    
    /// <summary>
    /// Stop the dynamic lighting effects.
    /// </summary>
    public void StopDynamicEffects()
    {
        if (dynamicEffectCoroutine != null)
        {
            StopCoroutine(dynamicEffectCoroutine);
            dynamicEffectCoroutine = null;
        }
    }
    
    /// <summary>
    /// Coroutine to handle subtle dynamic lighting variations.
    /// </summary>
    private IEnumerator DynamicLightingEffects()
    {
        // Base intensities for each light type
        float directionalBaseIntensity = 0;
        float[] accentBaseIntensities = new float[accentLights != null ? accentLights.Length : 0];
        float[] ambientBaseIntensities = new float[ambientLights != null ? ambientLights.Length : 0];
        
        // Store the base intensities
        if (mainDirectionalLight != null)
        {
            directionalBaseIntensity = mainDirectionalLight.intensity;
        }
        
        if (accentLights != null)
        {
            for (int i = 0; i < accentLights.Length; i++)
            {
                if (accentLights[i] != null)
                {
                    accentBaseIntensities[i] = accentLights[i].intensity;
                }
            }
        }
        
        if (ambientLights != null)
        {
            for (int i = 0; i < ambientLights.Length; i++)
            {
                if (ambientLights[i] != null)
                {
                    ambientBaseIntensities[i] = ambientLights[i].intensity;
                }
            }
        }
        
        // Continuously update with subtle variations
        while (true)
        {
            // Get a subtle variation using perlin noise
            float timeValue = Time.time * intensityChangeSpeed;
            float noiseValue = (Mathf.PerlinNoise(timeValue, 0) * 2.0f - 1.0f) * subtleIntensityVariation;
            
            // Only apply dynamic effects when no transition is active
            if (activeTransition == null)
            {
                // Apply to directional light
                if (mainDirectionalLight != null)
                {
                    mainDirectionalLight.intensity = directionalBaseIntensity + noiseValue;
                }
                
                // Apply to accent lights (use different noise pattern for each)
                if (accentLights != null)
                {
                    for (int i = 0; i < accentLights.Length; i++)
                    {
                        if (accentLights[i] != null)
                        {
                            float uniqueNoise = (Mathf.PerlinNoise(timeValue, i * 0.1f) * 2.0f - 1.0f) * subtleIntensityVariation;
                            accentLights[i].intensity = accentBaseIntensities[i] + uniqueNoise;
                        }
                    }
                }
                
                // Apply to ambient lights (using yet another pattern)
                if (ambientLights != null)
                {
                    for (int i = 0; i < ambientLights.Length; i++)
                    {
                        if (ambientLights[i] != null)
                        {
                            float uniqueNoise = (Mathf.PerlinNoise(timeValue, i * 0.2f + 0.5f) * 2.0f - 1.0f) * subtleIntensityVariation;
                            ambientLights[i].intensity = ambientBaseIntensities[i] + uniqueNoise;
                        }
                    }
                }
            }
            
            // Update base intensities if we're at a new stable state
            if (activeTransition == null && transitionProgress >= 1.0f)
            {
                if (mainDirectionalLight != null)
                {
                    directionalBaseIntensity = currentPreset.directionalIntensity;
                }
                
                if (accentLights != null)
                {
                    for (int i = 0; i < accentLights.Length; i++)
                    {
                        if (accentLights[i] != null)
                        {
                            accentBaseIntensities[i] = currentPreset.accentIntensity;
                        }
                    }
                }
                
                if (ambientLights != null)
                {
                    for (int i = 0; i < ambientLights.Length; i++)
                    {
                        if (ambientLights[i] != null)
                        {
                            ambientBaseIntensities[i] = currentPreset.ambientIntensity;
                        }
                    }
                }
            }
            
            yield return null;
        }
    }
}