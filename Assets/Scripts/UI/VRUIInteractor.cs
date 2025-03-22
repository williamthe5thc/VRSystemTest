using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Handles VR interaction with UI elements, allowing for pointing and selection
/// with VR controllers in a way that's compatible with Unity's UI system.
/// </summary>
public class VRUIInteractor : MonoBehaviour
{
    [Header("Input Configuration")]
    [SerializeField] private InputActionReference pointerAction;
    [SerializeField] private InputActionReference triggerAction;
    
    [Header("Ray Settings")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float rayLength = 5.0f;
    [SerializeField] private LayerMask uiLayerMask = 1 << 5; // UI layer
    
    [Header("Visual Feedback")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color hoverColor = Color.cyan;
    [SerializeField] private float lineWidth = 0.005f;
    
    [Header("Haptic Feedback")]
    [SerializeField] private bool useHaptics = true;
    [SerializeField] private float hapticIntensity = 0.1f;
    [SerializeField] private float hapticDuration = 0.1f;
    
    // Private fields
    private XRController xrController;
    private bool isUIHovered = false;
    private GameObject currentHoveredObject = null;
    private bool triggerPressed = false;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    
    private void Awake()
    {
        // Get required components
        xrController = GetComponent<XRController>();
        
        // Configure line renderer if it exists
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startColor = defaultColor;
            lineRenderer.endColor = defaultColor;
            lineRenderer.enabled = false;
        }
        else
        {
            Debug.LogWarning("Line Renderer not assigned to VRUIInteractor!");
        }
        
        // Use controller transform as ray origin if not specified
        if (rayOrigin == null)
        {
            rayOrigin = transform;
        }
    }
    
    private void OnEnable()
    {
        // Enable input actions
        if (pointerAction != null)
            pointerAction.action.Enable();
        
        if (triggerAction != null)
        {
            triggerAction.action.Enable();
            triggerAction.action.started += OnTriggerPressed;
            triggerAction.action.canceled += OnTriggerReleased;
        }
    }
    
    private void OnDisable()
    {
        // Disable input actions
        if (pointerAction != null)
            pointerAction.action.Disable();
        
        if (triggerAction != null)
        {
            triggerAction.action.Disable();
            triggerAction.action.started -= OnTriggerPressed;
            triggerAction.action.canceled -= OnTriggerReleased;
        }
        
        // Disable line renderer
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
    
    private void Update()
    {
        // Cast ray from controller
        UpdateUIRaycast();
    }
    
    private void UpdateUIRaycast()
    {
        if (rayOrigin == null) return;
        
        RaycastHit hit;
        Vector3 rayStart = rayOrigin.position;
        Vector3 rayDirection = rayOrigin.forward;
        
        // Update line renderer positions
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, rayStart);
            lineRenderer.SetPosition(1, rayStart + (rayDirection * rayLength));
        }
        
        // Cast ray to find UI elements
        bool hitUI = Physics.Raycast(rayStart, rayDirection, out hit, rayLength, uiLayerMask);
        
        if (hitUI)
        {
            // Show line renderer
            if (lineRenderer != null && !lineRenderer.enabled)
            {
                lineRenderer.enabled = true;
            }
            
            // Update line end position
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(1, hit.point);
                lineRenderer.startColor = hoverColor;
                lineRenderer.endColor = hoverColor;
            }
            
            // Process UI interaction
            ProcessUIInteraction(hit);
            
            isUIHovered = true;
        }
        else
        {
            // Reset hover state
            if (isUIHovered)
            {
                OnUIHoverExit();
                isUIHovered = false;
            }
            
            // Show full-length line or hide based on preference
            if (lineRenderer != null)
            {
                if (!isUIHovered)
                {
                    lineRenderer.startColor = defaultColor;
                    lineRenderer.endColor = defaultColor;
                }
                
                // Option: disable line when not pointing at UI
                // lineRenderer.enabled = false;
            }
        }
    }
    
    private void ProcessUIInteraction(RaycastHit hit)
    {
        // Get the GameObject with UI elements
        GameObject hitObject = hit.collider.gameObject;
        
        // Handle hover state change
        if (currentHoveredObject != hitObject)
        {
            OnUIHoverExit();
            currentHoveredObject = hitObject;
            OnUIHoverEnter(hit);
        }
        
        // Handle click/selection
        if (triggerPressed)
        {
            OnUIClicked(hit);
            triggerPressed = false; // Consume the press
        }
    }
    
    private void OnUIHoverEnter(RaycastHit hit)
    {
        // Provide haptic feedback for hover
        if (useHaptics && xrController != null)
        {
            xrController.SendHapticImpulse(hapticIntensity * 0.5f, hapticDuration * 0.5f);
        }
        
        // Find UI components on the hit object
        IPointerEnterHandler[] enterHandlers = hit.collider.gameObject.GetComponents<IPointerEnterHandler>();
        foreach (var handler in enterHandlers)
        {
            // Create a pointer event data
            PointerEventData pointerEventData = CreatePointerEventData(hit);
            handler.OnPointerEnter(pointerEventData);
        }
    }
    
    private void OnUIHoverExit()
    {
        if (currentHoveredObject != null)
        {
            // Find UI components on the previously hovered object
            IPointerExitHandler[] exitHandlers = currentHoveredObject.GetComponents<IPointerExitHandler>();
            foreach (var handler in exitHandlers)
            {
                // Create a pointer event data
                PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
                handler.OnPointerExit(pointerEventData);
            }
            
            currentHoveredObject = null;
        }
    }
    
    private void OnUIClicked(RaycastHit hit)
    {
        // Provide haptic feedback for click
        if (useHaptics && xrController != null)
        {
            xrController.SendHapticImpulse(hapticIntensity, hapticDuration);
        }
        
        // Find UI components on the hit object
        IPointerClickHandler[] clickHandlers = hit.collider.gameObject.GetComponents<IPointerClickHandler>();
        
        if (clickHandlers.Length > 0)
        {
            // Create pointer event data
            PointerEventData pointerEventData = CreatePointerEventData(hit);
            
            // Send click events
            foreach (var handler in clickHandlers)
            {
                handler.OnPointerClick(pointerEventData);
            }
        }
        else
        {
            // If the object itself doesn't have handlers, try to find a UI button and click it
            UnityEngine.UI.Button button = hit.collider.gameObject.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.Invoke();
            }
        }
    }
    
    private PointerEventData CreatePointerEventData(RaycastHit hit)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        
        // Set camera
        pointerEventData.pressPosition = new Vector2(Screen.width / 2, Screen.height / 2); // Center of screen
        pointerEventData.position = pointerEventData.pressPosition;
        
        // Set raycast results
        raycastResults.Clear();
        raycastResults.Add(new RaycastResult
        {
            gameObject = hit.collider.gameObject,
            worldPosition = hit.point,
            worldNormal = hit.normal,
            screenPosition = pointerEventData.position,
            index = 0,
            distance = hit.distance,
            sortingLayer = 0,
            sortingOrder = 0
        });
        
        pointerEventData.hovered.Clear();
        pointerEventData.hovered.Add(hit.collider.gameObject);
        
        return pointerEventData;
    }
    
    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        triggerPressed = true;
    }
    
    private void OnTriggerReleased(InputAction.CallbackContext context)
    {
        triggerPressed = false;
    }
}