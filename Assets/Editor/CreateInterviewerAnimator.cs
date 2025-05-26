using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

/// <summary>
/// Editor utility to create a basic animator controller for the interviewer avatar
/// </summary>
public class CreateInterviewerAnimator : EditorWindow
{
    [MenuItem("VR Interview/Create Interviewer Animator")]
    public static void CreateAnimatorController()
    {
        // Create the Resources directory if it doesn't exist
        if (!Directory.Exists("Assets/Resources"))
        {
            Directory.CreateDirectory("Assets/Resources");
            AssetDatabase.Refresh();
        }
        
        // Check if controller already exists
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Resources/InterviewerAnimator.controller") != null)
        {
            if (!EditorUtility.DisplayDialog("Confirm Overwrite", 
                "InterviewerAnimator.controller already exists. Overwrite it?", 
                "Overwrite", "Cancel"))
            {
                return;
            }
        }
        
        // Create a new animator controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/Resources/InterviewerAnimator.controller");
        
        // Add parameters for each animation state
        controller.AddParameter("Idle", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Listening", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Thinking", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Speaking", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Attentive", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Confused", AnimatorControllerParameterType.Trigger);
        
        // Get the root state machine
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
        
        // Create states
        AnimatorState idleState = rootStateMachine.AddState("Idle");
        AnimatorState listeningState = rootStateMachine.AddState("Listening");
        AnimatorState thinkingState = rootStateMachine.AddState("Thinking");
        AnimatorState speakingState = rootStateMachine.AddState("Speaking");
        AnimatorState attentiveState = rootStateMachine.AddState("Attentive");
        AnimatorState confusedState = rootStateMachine.AddState("Confused");
        
        // Set Idle as the default state
        rootStateMachine.defaultState = idleState;
        
        // Create transitions between states
        // Idle → any state
        CreateTransition(idleState, listeningState, "Listening");
        CreateTransition(idleState, thinkingState, "Thinking");
        CreateTransition(idleState, speakingState, "Speaking");
        CreateTransition(idleState, attentiveState, "Attentive");
        CreateTransition(idleState, confusedState, "Confused");
        
        // Listening → any state
        CreateTransition(listeningState, idleState, "Idle");
        CreateTransition(listeningState, thinkingState, "Thinking");
        CreateTransition(listeningState, speakingState, "Speaking");
        CreateTransition(listeningState, attentiveState, "Attentive");
        CreateTransition(listeningState, confusedState, "Confused");
        
        // Thinking → any state
        CreateTransition(thinkingState, idleState, "Idle");
        CreateTransition(thinkingState, listeningState, "Listening");
        CreateTransition(thinkingState, speakingState, "Speaking");
        CreateTransition(thinkingState, attentiveState, "Attentive");
        CreateTransition(thinkingState, confusedState, "Confused");
        
        // Speaking → any state
        CreateTransition(speakingState, idleState, "Idle");
        CreateTransition(speakingState, listeningState, "Listening");
        CreateTransition(speakingState, thinkingState, "Thinking");
        CreateTransition(speakingState, attentiveState, "Attentive");
        CreateTransition(speakingState, confusedState, "Confused");
        
        // Attentive → any state
        CreateTransition(attentiveState, idleState, "Idle");
        CreateTransition(attentiveState, listeningState, "Listening");
        CreateTransition(attentiveState, thinkingState, "Thinking");
        CreateTransition(attentiveState, speakingState, "Speaking");
        CreateTransition(attentiveState, confusedState, "Confused");
        
        // Confused → any state
        CreateTransition(confusedState, idleState, "Idle");
        CreateTransition(confusedState, listeningState, "Listening");
        CreateTransition(confusedState, thinkingState, "Thinking");
        CreateTransition(confusedState, speakingState, "Speaking");
        CreateTransition(confusedState, attentiveState, "Attentive");
        
        // Save the controller
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("InterviewerAnimator.controller created successfully in the Resources folder");
        
        // Show the animator controller
        Selection.activeObject = controller;
        EditorGUIUtility.PingObject(controller);
    }
    
    /// <summary>
    /// Creates a transition between two states with the given trigger parameter
    /// </summary>
    private static AnimatorStateTransition CreateTransition(AnimatorState from, AnimatorState to, string triggerName)
    {
        // Create the transition
        AnimatorStateTransition transition = from.AddTransition(to);
        
        // Set up transition properties
        transition.hasExitTime = false;
        transition.duration = 0.25f;
        transition.canTransitionToSelf = false;
        transition.AddCondition(AnimatorConditionMode.If, 0, triggerName);
        
        return transition;
    }
    
    [MenuItem("VR Interview/Setup Avatar Animation")]
    public static void SetupAvatarAnimation()
    {
        // Find the test Avatar in the scene
        GameObject avatar = GameObject.Find("test Avatar");
        if (avatar == null)
        {
            Debug.LogError("Could not find 'test Avatar' in the scene!");
            return;
        }
        
        // Ensure animator controller exists
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Resources/InterviewerAnimator.controller");
        if (controller == null)
        {
            CreateAnimatorController();
            controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Resources/InterviewerAnimator.controller");
            
            if (controller == null)
            {
                Debug.LogError("Failed to create InterviewerAnimator.controller!");
                return;
            }
        }
        
        // Find or add Animator component to avatar
        Animator animator = avatar.GetComponent<Animator>();
        if (animator == null)
        {
            animator = avatar.AddComponent<Animator>();
            Debug.Log("Added Animator component to test Avatar");
        }
        
        // Assign the controller
        animator.runtimeAnimatorController = controller;
        Debug.Log("Assigned InterviewerAnimator.controller to test Avatar");
        
        // Set up default motion clips if available
        // This could be expanded with actual animation clips in a real project
        
        // Save scene
        EditorUtility.SetDirty(avatar);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(avatar.scene);
        
        Debug.Log("Setup complete! Remember to save the scene.");
    }
}