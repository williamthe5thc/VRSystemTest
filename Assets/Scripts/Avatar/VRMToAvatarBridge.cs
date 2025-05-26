using UnityEngine;
using VRM;
using System.Collections;

/// <summary>
/// Bridges the VRM animation systems with the existing avatar systems
/// This handles the connection between regular LipSync and FacialExpressions with VRM components
/// </summary>
public class VRMToAvatarBridge : MonoBehaviour
{
    [Header("VRM Components")]
    [SerializeField] private VRMBlendShapeProxy blendShapeProxy;
    [SerializeField] private VRMLipSync vrmLipSync;
    [SerializeField] private VRMFacialExpressions vrmFacialExpressions;
    
    [Header("Regular Components")]
    [SerializeField] private LipSync regularLipSync;
    [SerializeField] private FacialExpressions regularFacialExpressions;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private FacialExpression _lastExpression = FacialExpression.Neutral;
    private float _lastLipSyncValue = 0f;
    
    private void Start()
    {
        FindComponents();
        FixConnections();
    }
    
    private void FindComponents()
    {
        // Find VRM components if not assigned
        if (blendShapeProxy == null)
        {
            blendShapeProxy = GetComponent<VRMBlendShapeProxy>();
            
            if (blendShapeProxy == null)
            {
                blendShapeProxy = GetComponentInChildren<VRMBlendShapeProxy>();
                
                if (blendShapeProxy == null)
                {
                    Debug.LogError("BlendShapeProxy not found. VRM avatar animation will not work.");
                    return;
                }
            }
        }
        
        // Find VRM Lip Sync
        if (vrmLipSync == null)
        {
            vrmLipSync = GetComponent<VRMLipSync>();
        }
        
        // Find VRM Facial Expressions
        if (vrmFacialExpressions == null)
        {
            vrmFacialExpressions = GetComponent<VRMFacialExpressions>();
        }
        
        // Find regular components
        if (regularLipSync == null)
        {
            regularLipSync = GetComponent<LipSync>();
        }
        
        if (regularFacialExpressions == null)
        {
            regularFacialExpressions = GetComponent<FacialExpressions>();
        }
    }
    
    /// <summary>
    /// Helper method to fix all component connections
    /// </summary>
    public void FixConnections()
    {
        FindComponents();
        
        // Connect VRM components to BlendShapeProxy
        if (vrmLipSync != null && blendShapeProxy != null)
        {
            vrmLipSync.BlendShapeProxy = blendShapeProxy;
            
            // Connect AudioSource
            AudioPlayback audioPlayback = FindObjectOfType<AudioPlayback>();
            if (audioPlayback != null)
            {
                AudioSource audioSource = audioPlayback.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    vrmLipSync.AudioSource = audioSource;
                }
            }
            
            if (debugMode)
            {
                Debug.Log("Connected BlendShapeProxy to VRMLipSync");
            }
        }
        
        if (vrmFacialExpressions != null && blendShapeProxy != null)
        {
            vrmFacialExpressions.BlendShapeProxy = blendShapeProxy;
            
            if (debugMode)
            {
                Debug.Log("Connected BlendShapeProxy to VRMFacialExpressions");
            }
        }
        
        // Connect regular components to VRM components
        ConnectRegularToVRM();
    }
    
    private void ConnectRegularToVRM()
    {
        // Connect LipSync if available
        if (regularLipSync != null)
        {
            try
            {
                // Get the OnLipSyncUpdate event field using reflection
                var eventField = regularLipSync.GetType().GetField("OnLipSyncUpdate", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic);
                    
                // Check if the event exists
                if (eventField != null)
                {
                    // Remove the handler first to avoid duplicates
                    try
                    {
                        System.Delegate d = (System.Delegate)eventField.GetValue(regularLipSync);
                        if (d != null)
                        {
                            System.Delegate[] delegates = d.GetInvocationList();
                            foreach (System.Delegate del in delegates)
                            {
                                if (del.Method.Name == "HandleLipSyncUpdate")
                                {
                                    var removeMethod = regularLipSync.GetType().GetMethod("remove_OnLipSyncUpdate", 
                                        System.Reflection.BindingFlags.Instance | 
                                        System.Reflection.BindingFlags.Public | 
                                        System.Reflection.BindingFlags.NonPublic);
                                    if (removeMethod != null)
                                    {
                                        removeMethod.Invoke(regularLipSync, new object[] { del });
                                    }
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning("Error removing existing handlers: " + ex.Message);
                    }
                    
                    // Manually forward LipSync updates to VRM system
                    StartCoroutine(MonitorLipSyncValue());
                    
                    if (debugMode)
                    {
                        Debug.Log("Connected LipSync to VRM system via coroutine");
                    }
                }
                else
                {
                    Debug.LogWarning("OnLipSyncUpdate event not found in LipSync");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error connecting LipSync: " + ex.Message);
            }
        }
        
        // Connect FacialExpressions if available
        if (regularFacialExpressions != null)
        {
            try
            {
                // Get the OnExpressionChanged event field using reflection
                var eventField = regularFacialExpressions.GetType().GetField("OnExpressionChanged", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.NonPublic);
                    
                // Check if the event exists
                if (eventField != null)
                {
                    // Remove the handler first to avoid duplicates
                    try
                    {
                        System.Delegate d = (System.Delegate)eventField.GetValue(regularFacialExpressions);
                        if (d != null)
                        {
                            System.Delegate[] delegates = d.GetInvocationList();
                            foreach (System.Delegate del in delegates)
                            {
                                if (del.Method.Name == "HandleExpressionChanged")
                                {
                                    var removeMethod = regularFacialExpressions.GetType().GetMethod("remove_OnExpressionChanged", 
                                        System.Reflection.BindingFlags.Instance | 
                                        System.Reflection.BindingFlags.Public | 
                                        System.Reflection.BindingFlags.NonPublic);
                                    if (removeMethod != null)
                                    {
                                        removeMethod.Invoke(regularFacialExpressions, new object[] { del });
                                    }
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning("Error removing existing handlers: " + ex.Message);
                    }
                    
                    // Manually forward expression changes
                    StartCoroutine(MonitorFacialExpression());
                    
                    if (debugMode)
                    {
                        Debug.Log("Connected FacialExpressions to VRM system via coroutine");
                    }
                }
                else
                {
                    Debug.LogWarning("OnExpressionChanged event not found in FacialExpressions");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error connecting FacialExpressions: " + ex.Message);
            }
        }
    }
    
    /// <summary>
    /// Coroutine to monitor LipSync value and apply to VRM
    /// </summary>
    private IEnumerator MonitorLipSyncValue()
    {
        // Get LipSyncValue field from LipSync
        var lipSyncValueField = regularLipSync.GetType().GetField("_lipSyncValue", 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic);
            
        if (lipSyncValueField == null)
        {
            Debug.LogError("Could not find _lipSyncValue field in LipSync");
            yield break;
        }
        
        while (true)
        {
            try
            {
                // Get the current value
                float value = (float)lipSyncValueField.GetValue(regularLipSync);
                
                // Only update if changed
                if (Mathf.Abs(value - _lastLipSyncValue) > 0.01f)
                {
                    _lastLipSyncValue = value;
                    
                    // Forward to VRM
                    if (vrmLipSync != null)
                    {
                        // Update VRM lip sync
                        vrmLipSync.UpdateLipSyncValue(value);
                        
                        // Start/stop lip sync
                        if (value > 0.01f)
                        {
                            vrmLipSync.StartLipSync();
                        }
                        else if (value < 0.01f)
                        {
                            vrmLipSync.StopLipSync();
                        }
                    }
                    else if (blendShapeProxy != null)
                    {
                        // Direct blend shape control
                        float scaledValue = value;
                        blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), scaledValue);
                        blendShapeProxy.Apply();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error in MonitorLipSyncValue: " + ex.Message);
            }
            
            // Check every 1/30 second (30fps)
            yield return new WaitForSeconds(0.033f);
        }
    }
    
    /// <summary>
    /// Coroutine to monitor facial expression and apply to VRM
    /// </summary>
    private IEnumerator MonitorFacialExpression()
    {
        // Get currentExpression field from FacialExpressions
        var expressionField = regularFacialExpressions.GetType().GetField("_currentExpression", 
            System.Reflection.BindingFlags.Instance | 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic);
            
        if (expressionField == null)
        {
            Debug.LogError("Could not find _currentExpression field in FacialExpressions");
            yield break;
        }
        
        while (true)
        {
            try
            {
                // Get the current expression
                FacialExpression expression = (FacialExpression)expressionField.GetValue(regularFacialExpressions);
                
                // Only update if changed
                if (expression != _lastExpression)
                {
                    _lastExpression = expression;
                    
                    // Forward to VRM
                    if (vrmFacialExpressions != null)
                    {
                        // Set VRM facial expression
                        vrmFacialExpressions.SetExpression(expression);
                    }
                    else if (blendShapeProxy != null)
                    {
                        // Direct blend shape control
                        // Reset all expressions first
                        blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), 0f);
                        blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry), 0f);
                        blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), 0f);
                        blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun), 0f);
                        
                        // Set appropriate expression
                        switch (expression)
                        {
                            case FacialExpression.Happy:
                                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), 1.0f);
                                break;
                                
                            case FacialExpression.Sad:
                                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), 1.0f);
                                break;
                                
                            case FacialExpression.Angry:
                                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Angry), 1.0f);
                                break;
                                
                            case FacialExpression.Surprised:
                                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Fun), 1.0f);
                                break;
                                
                            case FacialExpression.Confused:
                                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), 0.7f);
                                break;
                                
                            case FacialExpression.Thoughtful:
                                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), 0.5f);
                                break;
                                
                            case FacialExpression.Interested:
                                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), 0.4f);
                                break;
                                
                            case FacialExpression.Attentive:
                                // Slight expression
                                blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), 0.2f);
                                break;
                                
                            case FacialExpression.Talking:
                                // Handled by lip sync
                                break;
                        }
                        
                        blendShapeProxy.Apply();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error in MonitorFacialExpression: " + ex.Message);
            }
            
            // Check every 1/10 second (10fps is enough for expressions)
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    /// <summary>
    /// Direct method to set a facial expression
    /// </summary>
    public void SetFacialExpression(FacialExpression expression)
    {
        if (vrmFacialExpressions != null)
        {
            vrmFacialExpressions.SetExpression(expression);
        }
    }
    
    /// <summary>
    /// Direct method to update lip sync value
    /// </summary>
    public void UpdateLipSyncValue(float value)
    {
        if (vrmLipSync != null)
        {
            vrmLipSync.UpdateLipSyncValue(value);
            
            if (value > 0.01f)
            {
                vrmLipSync.StartLipSync();
            }
            else
            {
                vrmLipSync.StopLipSync();
            }
        }
    }
}