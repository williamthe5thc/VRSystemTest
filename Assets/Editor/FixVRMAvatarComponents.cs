using UnityEngine;
using UnityEditor;
using VRM;

/// <summary>
/// Editor utility to fix VRM component connections
/// </summary>
public class FixVRMAvatarComponents : EditorWindow
{
    [MenuItem("VR Interview/Fix VRM Components")]
    public static void FixComponents()
    {
        // Find the test Avatar in the scene
        GameObject avatar = GameObject.Find("test Avatar");
        if (avatar == null)
        {
            Debug.LogError("Could not find 'test Avatar' in the scene!");
            return;
        }
        
        // Fix VRMAvatarAdapter
        FixVRMAvatarAdapter(avatar);
        
        // Fix VRMLipSync
        FixVRMLipSync(avatar);
        
        // Fix VRMFacialExpressions
        FixVRMFacialExpressions(avatar);
        
        // Fix AvatarController
        FixAvatarController(avatar);
        
        // Add Runtime Fixer if missing
        if (avatar.GetComponent<VRMSetupFixer>() == null)
        {
            avatar.AddComponent<VRMSetupFixer>();
            Debug.Log("Added VRMSetupFixer component to fix issues at runtime");
        }
        
        // Save changes
        EditorUtility.SetDirty(avatar);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(avatar.scene);
        
        Debug.Log("VRM Component fixes applied! Remember to save the scene.");
    }
    
    private static void FixVRMAvatarAdapter(GameObject avatar)
    {
        VRMAvatarAdapter adapter = avatar.GetComponent<VRMAvatarAdapter>();
        if (adapter == null)
        {
            Debug.LogWarning("VRMAvatarAdapter component not found!");
            return;
        }
        
        // Get the required components
        VRMBlendShapeProxy blendShapeProxy = avatar.GetComponent<VRMBlendShapeProxy>();
        if (blendShapeProxy == null)
        {
            blendShapeProxy = avatar.GetComponentInChildren<VRMBlendShapeProxy>();
        }
        
        VRMLipSync lipSync = avatar.GetComponent<VRMLipSync>();
        VRMFacialExpressions facialExpressions = avatar.GetComponent<VRMFacialExpressions>();
        AvatarController avatarController = avatar.GetComponent<AvatarController>();
        
        AudioPlayback audioPlayback = Object.FindObjectOfType<AudioPlayback>();
        AudioSource audioSource = audioPlayback != null ? audioPlayback.GetComponent<AudioSource>() : null;
        
        // Set all properties using SerializedObject to ensure proper serialization
        SerializedObject serializedAdapter = new SerializedObject(adapter);
        
        SerializedProperty avatarRootProp = serializedAdapter.FindProperty("vrmAvatarRoot");
        avatarRootProp.objectReferenceValue = avatar;
        
        if (blendShapeProxy != null)
        {
            SerializedProperty blendShapeProxyProp = serializedAdapter.FindProperty("blendShapeProxy");
            blendShapeProxyProp.objectReferenceValue = blendShapeProxy;
        }
        
        if (lipSync != null)
        {
            SerializedProperty lipSyncProp = serializedAdapter.FindProperty("vrmLipSync");
            lipSyncProp.objectReferenceValue = lipSync;
        }
        
        if (facialExpressions != null)
        {
            SerializedProperty facialExpressionsProp = serializedAdapter.FindProperty("vrmFacialExpressions");
            facialExpressionsProp.objectReferenceValue = facialExpressions;
        }
        
        if (avatarController != null)
        {
            SerializedProperty avatarControllerProp = serializedAdapter.FindProperty("avatarController");
            avatarControllerProp.objectReferenceValue = avatarController;
        }
        
        if (audioSource != null)
        {
            SerializedProperty audioSourceProp = serializedAdapter.FindProperty("audioSource");
            audioSourceProp.objectReferenceValue = audioSource;
        }
        
        // Apply the changes
        serializedAdapter.ApplyModifiedProperties();
        
        Debug.Log("Fixed VRMAvatarAdapter component connections");
    }
    
    private static void FixVRMLipSync(GameObject avatar)
    {
        VRMLipSync lipSync = avatar.GetComponent<VRMLipSync>();
        if (lipSync == null)
        {
            Debug.LogWarning("VRMLipSync component not found!");
            return;
        }
        
        // Get the required components
        VRMBlendShapeProxy blendShapeProxy = avatar.GetComponent<VRMBlendShapeProxy>();
        if (blendShapeProxy == null)
        {
            blendShapeProxy = avatar.GetComponentInChildren<VRMBlendShapeProxy>();
        }
        
        AudioPlayback audioPlayback = Object.FindObjectOfType<AudioPlayback>();
        AudioSource audioSource = audioPlayback != null ? audioPlayback.GetComponent<AudioSource>() : null;
        
        // Use SerializedObject to set properties
        SerializedObject serializedLipSync = new SerializedObject(lipSync);
        
        if (blendShapeProxy != null)
        {
            SerializedProperty blendShapeProxyProp = serializedLipSync.FindProperty("blendShapeProxy");
            if (blendShapeProxyProp != null)
            {
                blendShapeProxyProp.objectReferenceValue = blendShapeProxy;
            }
        }
        
        if (audioSource != null)
        {
            SerializedProperty audioSourceProp = serializedLipSync.FindProperty("audioSource");
            if (audioSourceProp != null)
            {
                audioSourceProp.objectReferenceValue = audioSource;
            }
        }
        
        // Apply the changes
        serializedLipSync.ApplyModifiedProperties();
        
        Debug.Log("Fixed VRMLipSync component connections");
    }
    
    private static void FixVRMFacialExpressions(GameObject avatar)
    {
        VRMFacialExpressions facialExpressions = avatar.GetComponent<VRMFacialExpressions>();
        if (facialExpressions == null)
        {
            Debug.LogWarning("VRMFacialExpressions component not found!");
            return;
        }
        
        // Get the required components
        VRMBlendShapeProxy blendShapeProxy = avatar.GetComponent<VRMBlendShapeProxy>();
        if (blendShapeProxy == null)
        {
            blendShapeProxy = avatar.GetComponentInChildren<VRMBlendShapeProxy>();
        }
        
        // Use SerializedObject to set properties
        SerializedObject serializedFacialExpressions = new SerializedObject(facialExpressions);
        
        if (blendShapeProxy != null)
        {
            SerializedProperty blendShapeProxyProp = serializedFacialExpressions.FindProperty("blendShapeProxy");
            if (blendShapeProxyProp != null)
            {
                blendShapeProxyProp.objectReferenceValue = blendShapeProxy;
            }
        }
        
        // Apply the changes
        serializedFacialExpressions.ApplyModifiedProperties();
        
        Debug.Log("Fixed VRMFacialExpressions component connections");
    }
    
    private static void FixAvatarController(GameObject avatar)
    {
        AvatarController avatarController = avatar.GetComponent<AvatarController>();
        if (avatarController == null)
        {
            Debug.LogWarning("AvatarController component not found!");
            return;
        }
        
        // Get required components
        Animator animator = avatar.GetComponent<Animator>();
        
        // Get or create LipSync and FacialExpressions components that AvatarController uses
        LipSync lipSync = avatar.GetComponent<LipSync>();
        if (lipSync == null)
        {
            lipSync = avatar.AddComponent<LipSync>();
            Debug.Log("Added LipSync component for AvatarController");
        }
        
        FacialExpressions facialExpressions = avatar.GetComponent<FacialExpressions>();
        if (facialExpressions == null)
        {
            facialExpressions = avatar.AddComponent<FacialExpressions>();
            Debug.Log("Added FacialExpressions component for AvatarController");
        }
        
        // Use SerializedObject to set properties
        SerializedObject serializedController = new SerializedObject(avatarController);
        
        if (animator != null)
        {
            SerializedProperty animatorProp = serializedController.FindProperty("animator");
            if (animatorProp != null)
            {
                animatorProp.objectReferenceValue = animator;
            }
        }
        
        if (lipSync != null)
        {
            SerializedProperty lipSyncProp = serializedController.FindProperty("lipSync");
            if (lipSyncProp != null)
            {
                lipSyncProp.objectReferenceValue = lipSync;
            }
        }
        
        if (facialExpressions != null)
        {
            SerializedProperty facialExpressionsProp = serializedController.FindProperty("facialExpressions");
            if (facialExpressionsProp != null)
            {
                facialExpressionsProp.objectReferenceValue = facialExpressions;
            }
        }
        
        // Apply the changes
        serializedController.ApplyModifiedProperties();
        
        // Set up AudioPlayback connections
        AudioPlayback audioPlayback = Object.FindObjectOfType<AudioPlayback>();
        if (audioPlayback != null)
        {
            SerializedObject serializedAudioPlayback = new SerializedObject(audioPlayback);
            
            SerializedProperty avatarControllerProp = serializedAudioPlayback.FindProperty("avatarController");
            if (avatarControllerProp != null)
            {
                avatarControllerProp.objectReferenceValue = avatarController;
                serializedAudioPlayback.ApplyModifiedProperties();
                Debug.Log("Connected AudioPlayback to AvatarController");
            }
        }
        
        Debug.Log("Fixed AvatarController component connections");
    }
    
    [MenuItem("VR Interview/Create Missing VRM Components")]
    public static void CreateMissingComponents()
    {
        // Find the test Avatar in the scene
        GameObject avatar = GameObject.Find("test Avatar");
        if (avatar == null)
        {
            Debug.LogError("Could not find 'test Avatar' in the scene!");
            return;
        }
        
        // Check and add VRMLipSync
        if (avatar.GetComponent<VRMLipSync>() == null)
        {
            avatar.AddComponent<VRMLipSync>();
            Debug.Log("Added VRMLipSync component");
        }
        
        // Check and add VRMFacialExpressions
        if (avatar.GetComponent<VRMFacialExpressions>() == null)
        {
            avatar.AddComponent<VRMFacialExpressions>();
            Debug.Log("Added VRMFacialExpressions component");
        }
        
        // Check and add VRMAvatarAdapter
        if (avatar.GetComponent<VRMAvatarAdapter>() == null)
        {
            avatar.AddComponent<VRMAvatarAdapter>();
            Debug.Log("Added VRMAvatarAdapter component");
        }
        
        // Check and add VRMSetupFixer
        if (avatar.GetComponent<VRMSetupFixer>() == null)
        {
            avatar.AddComponent<VRMSetupFixer>();
            Debug.Log("Added VRMSetupFixer component");
        }
        
        // Check and add AvatarController
        if (avatar.GetComponent<AvatarController>() == null)
        {
            avatar.AddComponent<AvatarController>();
            Debug.Log("Added AvatarController component");
        }
        
        // Save changes
        EditorUtility.SetDirty(avatar);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(avatar.scene);
        
        Debug.Log("Missing VRM components created! Remember to save the scene.");
        
        // Run the fix connections method
        FixComponents();
    }
}