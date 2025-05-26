using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Controls the behavior of interactable items in the VR environment.
/// This script extends XRGrabInteractable to add custom functionality
/// specific to the interview environment.
/// </summary>
public class InteractableItem : XRGrabInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private InteractionType interactionType = InteractionType.Grab;
    [SerializeField] private bool returnToOriginalPosition = true;
    [SerializeField] private float returnDelay = 2.0f;
    [SerializeField] private bool playSound = true;
    [SerializeField] private AudioClip interactSound;
    [SerializeField] private AudioClip releaseSound;
    
    [Header("Highlight Settings")]
    [SerializeField] private bool useHighlight = true;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private float highlightIntensity = 0.3f;
    
    // Events
    public event Action<InteractableItem> OnItemInteracted;
    
    // Private fields
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private AudioSource audioSource;
    private Material[] originalMaterials;
    private Renderer objectRenderer;
    private bool isGrabbed = false;
    
    public enum InteractionType
    {
        Grab,
        Touch,
        Look
    }
    
    protected override void Awake()
    {
        base.Awake();
        
        // Store original transform
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        // Get renderer for highlight
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null && useHighlight)
        {
            // Store original materials
            originalMaterials = objectRenderer.materials;
        }
        
        // Set up audio
        if (playSound && (interactSound != null || releaseSound != null))
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f; // 3D sound
                audioSource.volume = 0.5f;
            }
        }
        
        // Configure based on interaction type
        switch (interactionType)
        {
            case InteractionType.Grab:
                // Default XRGrabInteractable behavior
                movementType = MovementType.VelocityTracking;
                trackPosition = true;
                trackRotation = true;
                throwOnDetach = true;
                break;
                
            case InteractionType.Touch:
                // Object doesn't move but can be touched
                movementType = MovementType.Kinematic;
                trackPosition = false;
                trackRotation = false;
                throwOnDetach = false;
                break;
                
            case InteractionType.Look:
                // Object just highlights when looked at
                movementType = MovementType.Kinematic;
                trackPosition = false;
                trackRotation = false;
                throwOnDetach = false;
                break;
        }
    }
    
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        
        isGrabbed = true;
        
        // Play interaction sound
        if (playSound && audioSource != null && interactSound != null)
        {
            audioSource.clip = interactSound;
            audioSource.Play();
        }
        
        // Apply highlight if not using touch or look
        if (interactionType == InteractionType.Grab && useHighlight && objectRenderer != null && highlightMaterial != null)
        {
            ApplyHighlight();
        }
        
        // Trigger the interaction event
        OnItemInteracted?.Invoke(this);
        
        // Debug log
        Debug.Log($"Item {name} was interacted with");
    }
    
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        
        isGrabbed = false;
        
        // Play release sound
        if (playSound && audioSource != null && releaseSound != null)
        {
            audioSource.clip = releaseSound;
            audioSource.Play();
        }
        
        // Remove highlight
        if (useHighlight && objectRenderer != null && originalMaterials != null)
        {
            RemoveHighlight();
        }
        
        // Return to original position if configured
        if (returnToOriginalPosition && interactionType == InteractionType.Grab)
        {
            Invoke("ReturnToOriginalPosition", returnDelay);
        }
    }
    
    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        
        // Apply highlight for look or touch interactions
        if ((interactionType == InteractionType.Look || interactionType == InteractionType.Touch) 
            && useHighlight && objectRenderer != null && highlightMaterial != null && !isGrabbed)
        {
            ApplyHighlight();
        }
    }
    
    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        
        // Remove highlight if not grabbed
        if ((interactionType == InteractionType.Look || interactionType == InteractionType.Touch) 
            && useHighlight && objectRenderer != null && originalMaterials != null && !isGrabbed)
        {
            RemoveHighlight();
        }
    }
    
    private void ApplyHighlight()
    {
        if (objectRenderer == null || highlightMaterial == null) return;
        
        Material[] highlightMaterials = new Material[objectRenderer.materials.Length];
        for (int i = 0; i < objectRenderer.materials.Length; i++)
        {
            highlightMaterials[i] = new Material(highlightMaterial);
            
            // Copy main texture if it exists
            if (objectRenderer.materials[i].HasProperty("_MainTex"))
            {
                highlightMaterials[i].SetTexture("_MainTex", 
                    objectRenderer.materials[i].GetTexture("_MainTex"));
            }
            
            // Set highlight intensity if property exists
            if (highlightMaterials[i].HasProperty("_HighlightIntensity"))
            {
                highlightMaterials[i].SetFloat("_HighlightIntensity", highlightIntensity);
            }
        }
        
        objectRenderer.materials = highlightMaterials;
    }
    
    private void RemoveHighlight()
    {
        if (objectRenderer == null || originalMaterials == null) return;
        
        objectRenderer.materials = originalMaterials;
    }
    
    private void ReturnToOriginalPosition()
    {
        if (!isGrabbed) // Only return if not currently grabbed
        {
            // Use physics if rigidbody exists
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                
                // Smoothly move back
                StartCoroutine(SmoothReturn(rb));
            }
            else
            {
                // Direct position reset
                transform.position = originalPosition;
                transform.rotation = originalRotation;
            }
        }
    }
    
    private System.Collections.IEnumerator SmoothReturn(Rigidbody rb)
    {
        float elapsedTime = 0;
        float returnTime = 1.0f;
        
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        
        while (elapsedTime < returnTime)
        {
            rb.MovePosition(Vector3.Lerp(startPos, originalPosition, elapsedTime / returnTime));
            rb.MoveRotation(Quaternion.Lerp(startRot, originalRotation, elapsedTime / returnTime));
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Final exact position
        rb.MovePosition(originalPosition);
        rb.MoveRotation(originalRotation);
    }
    
    // Reset to original transform (for editor use)
    public void ResetPosition()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}