using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class BootstrapSceneCreator : EditorWindow
{
    [MenuItem("Tools/VR Interview/Create Bootstrap Scene")]
    public static void CreateBootstrapScene()
    {
        // Create a new scene
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Create root object
        GameObject bootstrapRoot = new GameObject("Bootstrap");
        
        // Create SystemCore with BootstrapLoader
        GameObject systemCore = new GameObject("SystemCore");
        systemCore.transform.parent = bootstrapRoot.transform;
        BootstrapLoader bootstrapLoader = systemCore.AddComponent<BootstrapLoader>();
        
        // Set properties using SerializeField
        SerializedObject so = new SerializedObject(bootstrapLoader);
        so.FindProperty("nextScene").stringValue = "Scenes/MainMenu";
        so.FindProperty("delay").floatValue = 0.5f;
        so.ApplyModifiedProperties();
        
        // Create AppManager
        GameObject appManagerObj = new GameObject("AppManager");
        appManagerObj.transform.parent = systemCore.transform;
        appManagerObj.AddComponent<AppManager>();
        
        // Create SettingsManager
        GameObject settingsManagerObj = new GameObject("SettingsManager");
        settingsManagerObj.transform.parent = systemCore.transform;
        settingsManagerObj.AddComponent<SettingsManager>();
        
        // Create SessionManager
        GameObject sessionManagerObj = new GameObject("SessionManager");
        sessionManagerObj.transform.parent = systemCore.transform;
        sessionManagerObj.AddComponent<SessionManager>();
        
        // Create WebSocketClient
        GameObject webSocketClientObj = new GameObject("WebSocketClient");
        webSocketClientObj.transform.parent = systemCore.transform;
        webSocketClientObj.AddComponent<WebSocketClient>();
        
        // Create MessageHandler
        GameObject messageHandlerObj = new GameObject("MessageHandler");
        messageHandlerObj.transform.parent = systemCore.transform;
        messageHandlerObj.AddComponent<MessageHandler>();
        
        // Create AudioManager with children
        GameObject audioManagerObj = new GameObject("AudioManager");
        audioManagerObj.transform.parent = systemCore.transform;
        
        GameObject audioProcessorObj = new GameObject("AudioProcessor");
        audioProcessorObj.transform.parent = audioManagerObj.transform;
        audioProcessorObj.AddComponent<AudioProcessor>();
        
        GameObject audioPlaybackObj = new GameObject("AudioPlayback");
        audioPlaybackObj.transform.parent = audioManagerObj.transform;
        audioPlaybackObj.AddComponent<AudioPlayback>();
        
        // Save the scene
        string scenePath = "Assets/Scenes/Bootstrap.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        
        // Add scene to build settings if not already present
        AddSceneToBuildSettings(scenePath);
        
        Debug.Log("Bootstrap scene created at: " + scenePath);
    }
    
    private static void AddSceneToBuildSettings(string scenePath)
    {
        // Get all scenes currently in build settings
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        
        // Check if our scene is already in build settings
        string sceneAssetPath = scenePath.Replace("Assets/", "");
        bool sceneExists = scenes.Any(s => s.path == scenePath);
        
        if (!sceneExists)
        {
            // Add the new scene to the beginning of the list (index 0)
            EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[scenes.Count + 1];
            newScenes[0] = new EditorBuildSettingsScene(scenePath, true);
            for (int i = 0; i < scenes.Count; i++)
            {
                newScenes[i + 1] = scenes[i];
            }
            
            // Apply the new build settings
            EditorBuildSettings.scenes = newScenes;
        }
    }
}