using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

namespace VRInterview.Editor
{
    /// <summary>
    /// Utility for creating and fixing animator controllers for the VR Interview System
    /// </summary>
    public static class AnimatorControllerCreator
    {
        private const string RESOURCES_PATH = "Assets/Resources";
        private const string ANIMATIONS_PATH = "Assets/Animations";
        private const string CONTROLLER_NAME = "InterviewerAnimator";
        
        [MenuItem("VR Interview/Fix Animator Controllers", false, 30)]
        public static void FixAnimatorControllers()
        {
            // Ensure folders exist
            EnsureFolders();
            
            // Check if controller already exists in either location
            AnimatorController controller = GetExistingController();
            
            if (controller == null)
            {
                // Create new controller if none exists
                controller = CreateNewController();
            }
            
            // Ensure it exists in Resources folder
            EnsureControllerInResources(controller);
            
            // Apply controller to all avatar animators in scene
            ApplyControllerToAvatars(controller);
            
            // Save assets
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Animator Controllers fixed successfully");
        }
        
        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(RESOURCES_PATH))
            {
                string parentFolder = Path.GetDirectoryName(RESOURCES_PATH).Replace('\\', '/');
                string folderName = Path.GetFileName(RESOURCES_PATH);
                AssetDatabase.CreateFolder(parentFolder, folderName);
                Debug.Log($"Created Resources folder at {RESOURCES_PATH}");
            }
            
            if (!AssetDatabase.IsValidFolder(ANIMATIONS_PATH))
            {
                string parentFolder = Path.GetDirectoryName(ANIMATIONS_PATH).Replace('\\', '/');
                string folderName = Path.GetFileName(ANIMATIONS_PATH);
                AssetDatabase.CreateFolder(parentFolder, folderName);
                Debug.Log($"Created Animations folder at {ANIMATIONS_PATH}");
            }
        }
        
        private static AnimatorController GetExistingController()
        {
            // Check Resources folder first
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{RESOURCES_PATH}/{CONTROLLER_NAME}.controller");
            if (controller != null)
            {
                return controller;
            }
            
            // Check Animations folder next
            controller = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{ANIMATIONS_PATH}/{CONTROLLER_NAME}.controller");
            if (controller != null)
            {
                return controller;
            }
            
            // Search entire project as last resort
            string[] guids = AssetDatabase.FindAssets("t:AnimatorController " + CONTROLLER_NAME);
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            }
            
            return null;
        }
        
        private static AnimatorController CreateNewController()
        {
            string path = $"{ANIMATIONS_PATH}/{CONTROLLER_NAME}.controller";
            
            // Create controller
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            
            // Add parameters
            controller.AddParameter("Talking", AnimatorControllerParameterType.Bool);
            controller.AddParameter("GesturePointLeft", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("GesturePointRight", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("GestureShakeHead", AnimatorControllerParameterType.Trigger);
            
            // Add states
            var rootStateMachine = controller.layers[0].stateMachine;
            var idleState = rootStateMachine.AddState("Idle");
            var talkingState = rootStateMachine.AddState("Talking");
            
            // Add transitions
            var idleToTalking = idleState.AddTransition(talkingState);
            idleToTalking.AddCondition(AnimatorConditionMode.If, 0, "Talking");
            
            var talkingToIdle = talkingState.AddTransition(idleState);
            talkingToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Talking");
            
            Debug.Log($"Created new controller at {path}");
            return controller;
        }
        
        private static void EnsureControllerInResources(AnimatorController controller)
        {
            string resourcesPath = $"{RESOURCES_PATH}/{CONTROLLER_NAME}.controller";
            
            // If controller isn't in Resources folder, copy it there
            if (AssetDatabase.GetAssetPath(controller) != resourcesPath)
            {
                // Use AssetDatabase CopyAsset
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(controller), resourcesPath);
                Debug.Log($"Copied controller to Resources folder: {resourcesPath}");
            }
        }
        
        private static void ApplyControllerToAvatars(AnimatorController controller)
        {
            // Find all avatar controllers in scene
            AvatarController[] avatarControllers = Object.FindObjectsOfType<AvatarController>();
            
            if (avatarControllers.Length == 0)
            {
                Debug.LogWarning("No AvatarController components found in scene");
                return;
            }
            
            foreach (var avatarController in avatarControllers)
            {
                // Get or add Animator component
                Animator animator = avatarController.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = avatarController.gameObject.AddComponent<Animator>();
                    Debug.Log($"Added Animator to {avatarController.name}");
                }
                
                // Set controller
                if (animator.runtimeAnimatorController == null || 
                    animator.runtimeAnimatorController != controller)
                {
                    animator.runtimeAnimatorController = controller;
                    Debug.Log($"Applied controller to {avatarController.name}");
                }
            }
        }
    }
}
