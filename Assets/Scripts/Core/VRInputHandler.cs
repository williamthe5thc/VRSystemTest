using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Handles VR controller inputs for the interview system.
/// </summary>
public class VRInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset actionAsset;
    [SerializeField] private string menuActionMap = "XRI LeftHand Interaction";
    [SerializeField] private string menuActionName = "Menu";
    
    [Header("References")]
    [SerializeField] private MenuController menuController;
    [SerializeField] private UIManager uiManager;
    
    // Input actions
    private InputAction _menuAction;
    
    // State
    private bool _menuVisible = false;
    
    // Events
    public event Action OnMenuToggled;
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void OnEnable()
    {
        EnableActions();
    }
    
    private void OnDisable()
    {
        DisableActions();
    }
    
    /// <summary>
    /// Initializes required components.
    /// </summary>
    private void InitializeComponents()
    {
        // Find MenuController if not assigned
        if (menuController == null)
        {
            menuController = FindObjectOfType<MenuController>();
            
            if (menuController == null)
            {
                Debug.LogWarning("MenuController not found!");
            }
        }
        
        // Find UIManager if not assigned
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
            
            if (uiManager == null)
            {
                Debug.LogWarning("UIManager not found!");
            }
        }
        
        // Set up input actions
        SetupInputActions();
    }
    
    /// <summary>
    /// Sets up the input actions for VR controllers.
    /// </summary>
    private void SetupInputActions()
    {
        if (actionAsset == null)
        {
            // Try to find XRIDefaultInputActions in Resources
            actionAsset = Resources.Load<InputActionAsset>("XRIDefaultInputActions");
            
            if (actionAsset == null)
            {
                Debug.LogError("InputActionAsset not assigned and default not found! VR input will not work.");
                return;
            }
        }
        
        // Get menu action
        InputActionMap actionMap = actionAsset.FindActionMap(menuActionMap);
        if (actionMap != null)
        {
            _menuAction = actionMap.FindAction(menuActionName);
            
            if (_menuAction != null)
            {
                _menuAction.performed += OnMenuPressed;
            }
            else
            {
                Debug.LogError($"Menu action '{menuActionName}' not found in action map '{menuActionMap}'!");
            }
        }
        else
        {
            Debug.LogError($"Action map '{menuActionMap}' not found!");
        }
    }
    
    /// <summary>
    /// Enables input actions.
    /// </summary>
    private void EnableActions()
    {
        _menuAction?.Enable();
    }
    
    /// <summary>
    /// Disables input actions.
    /// </summary>
    private void DisableActions()
    {
        _menuAction?.Disable();
    }
    
    /// <summary>
    /// Handles menu button press.
    /// </summary>
    /// <param name="context">Callback context.</param>
    private void OnMenuPressed(InputAction.CallbackContext context)
    {
        ToggleMenu();
    }
    
    /// <summary>
    /// Toggles the menu visibility.
    /// </summary>
    public void ToggleMenu()
    {
        _menuVisible = !_menuVisible;
        
        if (menuController != null)
        {
            if (_menuVisible)
            {
                // Show appropriate menu based on interview state
                if (SessionManager.Instance != null && SessionManager.Instance.IsSessionActive)
                {
                    menuController.ShowInterviewMenu();
                }
                else
                {
                    menuController.ShowMainMenu();
                }
            }
            else
            {
                // Hide menus (would need to add this method to MenuController)
                // For now, we'll just return to the appropriate state
                if (SessionManager.Instance != null && SessionManager.Instance.IsSessionActive)
                {
                    menuController.ShowInterviewMenu();
                }
                else
                {
                    menuController.ShowMainMenu();
                }
            }
        }
        
        // Notify menu toggle
        OnMenuToggled?.Invoke();
        
        if (uiManager != null)
        {
            uiManager.ShowMessage(_menuVisible ? "Menu opened" : "Menu closed");
        }
    }
    
    /// <summary>
    /// Gets whether the menu is currently visible.
    /// </summary>
    /// <returns>Whether the menu is visible.</returns>
    public bool IsMenuVisible()
    {
        return _menuVisible;
    }
}