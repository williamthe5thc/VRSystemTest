using UnityEngine;
using UnityEditor;
using VRM;
using System.Collections.Generic;

/// <summary>
/// Editor utility for setting up VRM avatars in the Unity editor
/// </summary>
[CustomEditor(typeof(VRMAvatarAdapter))]
public class VRMAvatarSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        VRMAvatarAdapter adapter = (VRMAvatarAdapter)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("VRM Avatar Setup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Setup VRM Avatar Components"))
        {
            SetupVRMAvatar(adapter.gameObject);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This will add the necessary VRM components to your avatar and configure them.", MessageType.Info);
    }
    
    /// <summary>
    /// Sets up a VRM avatar with all required components
    /// </summary>
    private void SetupVRMAvatar(GameObject avatarRoot)
    {
        if (avatarRoot == null)
        {
            Debug.LogError("Avatar root is null!");
            return;
        }
        
        // Find VRM components
        VRMBlendShapeProxy blendShapeProxy = avatarRoot.GetComponent<VRMBlendShapeProxy>();
        if (blendShapeProxy == null)
        {
            blendShapeProxy = avatarRoot.GetComponentInChildren<VRMBlendShapeProxy>();
            if (blendShapeProxy == null)
            {
                Debug.LogError("No VRMBlendShapeProxy found on the avatar!");
                return;
            }
        }
        
        // Find or add components
        bool addedComponents = false;
        
        // Add VRMLipSync if missing
        VRMLipSync lipSync = avatarRoot.GetComponent<VRMLipSync>();
        if (lipSync == null)
        {
            Undo.RecordObject(avatarRoot, "Add VRMLipSync");
            lipSync = avatarRoot.AddComponent<VRMLipSync>();
            addedComponents = true;
            Debug.Log("Added VRMLipSync component");
        }
        
        // Add VRMFacialExpressions if missing
        VRMFacialExpressions facialExpressions = avatarRoot.GetComponent<VRMFacialExpressions>();
        if (facialExpressions == null)
        {
            Undo.RecordObject(avatarRoot, "Add VRMFacialExpressions");
            facialExpressions = avatarRoot.AddComponent<VRMFacialExpressions>();
            addedComponents = true;
            Debug.Log("Added VRMFacialExpressions component");
        }
        
        // Add VRMAvatarAdapter if missing
        VRMAvatarAdapter adapter = avatarRoot.GetComponent<VRMAvatarAdapter>();
        if (adapter == null)
        {
            Undo.RecordObject(avatarRoot, "Add VRMAvatarAdapter");
            adapter = avatarRoot.AddComponent<VRMAvatarAdapter>();
            addedComponents = true;
            Debug.Log("Added VRMAvatarAdapter component");
        }
        
        // Find scene dependencies
        AudioPlayback audioPlayback = Object.FindObjectOfType<AudioPlayback>();
        AudioSource audioSource = null;
        if (audioPlayback != null)
        {
            audioSource = audioPlayback.GetComponent<AudioSource>();
        }
        
        AvatarController avatarController = avatarRoot.GetComponent<AvatarController>();
        
        // Add AvatarController if missing
        if (avatarController == null)
        {
            Undo.RecordObject(avatarRoot, "Add AvatarController");
            avatarController = avatarRoot.AddComponent<AvatarController>();
            addedComponents = true;
            Debug.Log("Added AvatarController component");
        }
        
        // Configure VRMLipSync
        if (lipSync != null)
        {
            Undo.RecordObject(lipSync, "Configure VRMLipSync");
            lipSync.BlendShapeProxy = blendShapeProxy;
            if (audioSource != null)
            {
                lipSync.AudioSource = audioSource;
            }
            EditorUtility.SetDirty(lipSync);
            Debug.Log("Configured VRMLipSync component");
        }
        
        // Configure VRMFacialExpressions
        if (facialExpressions != null)
        {
            Undo.RecordObject(facialExpressions, "Configure VRMFacialExpressions");
            facialExpressions.BlendShapeProxy = blendShapeProxy;
            EditorUtility.SetDirty(facialExpressions);
            Debug.Log("Configured VRMFacialExpressions component");
        }
        
        // Configure VRMAvatarAdapter
        if (adapter != null)
        {
            Undo.RecordObject(adapter, "Configure VRMAvatarAdapter");
            
            // Set references using the new property accessors
            adapter.VrmAvatarRoot = avatarRoot;
            adapter.BlendShapeProxy = blendShapeProxy;
            adapter.VrmLipSync = lipSync;
            adapter.VrmFacialExpressions = facialExpressions;
            
            if (avatarController != null)
            {
                adapter.AvatarController = avatarController;
            }
            
            if (audioSource != null)
            {
                adapter.AudioSource = audioSource;
            }
            
            EditorUtility.SetDirty(adapter);
            Debug.Log("Configured VRMAvatarAdapter component");
        }
        
        // Configure Animator if needed
        Animator animator = avatarRoot.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController == null)
        {
            // Try to find any animator controller in the project
            string[] guids = AssetDatabase.FindAssets("t:AnimatorController");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
                if (controller != null)
                {
                    Undo.RecordObject(animator, "Set AnimatorController");
                    animator.runtimeAnimatorController = controller;
                    EditorUtility.SetDirty(animator);
                    Debug.Log($"Assigned animator controller: {controller.name}");
                }
            }
        }
        
        // Add VRMRuntimeConnector if not already present
        if (avatarRoot.GetComponent<VRMRuntimeConnector>() == null)
        {
            Undo.RecordObject(avatarRoot, "Add VRMRuntimeConnector");
            VRMRuntimeConnector connector = avatarRoot.AddComponent<VRMRuntimeConnector>();
            EditorUtility.SetDirty(connector);
            addedComponents = true;
            Debug.Log("Added VRMRuntimeConnector component for runtime connection fixing");
        }
        
        // Notify if changes were made
        if (addedComponents)
        {
            EditorUtility.SetDirty(avatarRoot);
            Debug.Log("VRM avatar setup complete");
        }
        else
        {
            Debug.Log("VRM avatar already setup");
        }
    }
}
