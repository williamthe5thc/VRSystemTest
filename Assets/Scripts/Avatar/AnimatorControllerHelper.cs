using UnityEngine;

/// <summary>
/// Helper utility for creating runtime animator controllers
/// </summary>
public static class AnimatorControllerHelper
{
    /// <summary>
    /// Creates a simple animator controller at runtime with basic state transitions
    /// </summary>
    public static RuntimeAnimatorController CreateSimpleController()
    {
        // Create an animator override controller with basic states
        AnimatorOverrideController controller = new AnimatorOverrideController();

        // Try to find a base controller from resources first
        RuntimeAnimatorController baseController = Resources.Load<RuntimeAnimatorController>("InterviewerAnimator");
        
        // If not found in resources, look for any available controller
        if (baseController == null)
        {
            Object[] controllers = Resources.FindObjectsOfTypeAll(typeof(RuntimeAnimatorController));
            if (controllers.Length > 0)
            {
                baseController = controllers[0] as RuntimeAnimatorController;
            }
        }
        
        // If we found a base controller, use it for the override
        if (baseController != null)
        {
            controller.runtimeAnimatorController = baseController;
            Debug.Log($"Using base controller: {baseController.name}");
            return controller;
        }
        
        // If no base controller found, we need to create one in the editor
        // This won't work at runtime, but at least we'll log the issue
        Debug.LogError("No base controller found for animation. Please create an 'InterviewerAnimator' controller in the Resources folder.");
        return null;
    }
    
    /// <summary>
    /// Checks and fixes animator setup
    /// </summary>
    public static void FixAnimatorSetup(Animator animator)
    {
        if (animator == null) return;
        
        // Check if animator has a controller
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("Animator has no controller. Attempting to assign a default controller.");
            
            // Try to create and assign a controller
            RuntimeAnimatorController controller = CreateSimpleController();
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
                Debug.Log("Applied simple animator controller");
            }
        }
    }
}