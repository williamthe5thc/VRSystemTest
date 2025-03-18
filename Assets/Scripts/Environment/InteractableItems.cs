using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Manages interactive objects in the interview environment.
/// </summary>
public class InteractableItems : MonoBehaviour
{
    [System.Serializable]
    public class InteractableItem
    {
        public string name;
        public GameObject itemObject;
        public bool startActive = true;
        public InteractionType interactionType = InteractionType.Grab;
        public AudioClip interactionSound;
    }
    
    public enum InteractionType
    {
        Grab,
        Touch,
        Look,
        Custom
    }
    
    [Header("Interactive Items")]
    [SerializeField] private List<InteractableItem> interactableItems = new List<InteractableItem>();
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 2.0f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private AudioSource interactionAudioSource;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Active interactable objects
    private Dictionary<string, GameObject> _activeItems = new Dictionary<string, GameObject>();
    
    // Events
    public event Action<string> OnItemInteracted;
    
    private void Start()
    {
        InitializeComponents();
        InitializeItems();
    }
    
    /// <summary>
    /// Initializes required components if not assigned.
    /// </summary>
    private void InitializeComponents()
    {
        // Create audio source if not assigned
        if (interactionAudioSource == null)
        {
            interactionAudioSource = GetComponent<AudioSource>();
            
            if (interactionAudioSource == null)
            {
                GameObject audioObj = new GameObject("InteractionAudio");
                audioObj.transform.parent = transform;
                interactionAudioSource = audioObj.AddComponent<AudioSource>();
                interactionAudioSource.playOnAwake = false;
                interactionAudioSource.spatialBlend = 1.0f; // 3D sound
            }
        }
        
        // Set default interaction layer if not set
        if (interactionLayer == 0)
        {
            interactionLayer = LayerMask.GetMask("Default");
        }
    }
    
    /// <summary>
    /// Initializes all interactable items.
    /// </summary>
    public void InitializeItems()
    {
        _activeItems.Clear();
        
        foreach (InteractableItem item in interactableItems)
        {
            if (item.itemObject != null)
            {
                // Set initial state
                item.itemObject.SetActive(item.startActive);
                
                // Add to active items if enabled
                if (item.startActive)
                {
                    _activeItems[item.name] = item.itemObject;
                }
                
                // Set up interaction based on type
                SetupInteraction(item);
            }
            else
            {
                Debug.LogWarning($"Interactable item '{item.name}' has no GameObject assigned!");
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"Initialized {_activeItems.Count} active interactable items.");
        }
    }
    
    /// <summary>
    /// Sets up interaction components for an item.
    /// </summary>
    /// <param name="item">The interactable item.</param>
    private void SetupInteraction(InteractableItem item)
    {
        if (item.itemObject == null) return;
        
        switch (item.interactionType)
        {
            case InteractionType.Grab:
                // Make sure it has a collider
                if (item.itemObject.GetComponent<Collider>() == null)
                {
                    item.itemObject.AddComponent<BoxCollider>();
                }
                
                // Add rigidbody if missing
                if (item.itemObject.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody rb = item.itemObject.AddComponent<Rigidbody>();
                    rb.useGravity = true;
                    rb.isKinematic = false;
                }
                
                // Add XR Grab Interactable if available
                if (item.itemObject.GetComponent<XRGrabInteractable>() == null)
                {
                    // Check if XR Interaction Toolkit is available
                    try
                    {
                        XRGrabInteractable grabInteractable = item.itemObject.AddComponent<XRGrabInteractable>();
                        grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
                        
                        // Add interaction event
                        grabInteractable.selectEntered.AddListener((args) => {
                            HandleItemInteraction(item.name);
                        });
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning("XR Interaction Toolkit not available. Using basic interaction.");
                        
                        // Fall back to basic interaction
                        SetupBasicInteraction(item);
                    }
                }
                break;
                
            case InteractionType.Touch:
                // Make sure it has a collider
                if (item.itemObject.GetComponent<Collider>() == null)
                {
                    BoxCollider collider = item.itemObject.AddComponent<BoxCollider>();
                    collider.isTrigger = true;
                }
                
                // Add basic interaction script
                SetupBasicInteraction(item);
                break;
                
            case InteractionType.Look:
                // No collider needed for gaze interaction
                
                // Add basic interaction script
                SetupBasicInteraction(item);
                break;
                
            case InteractionType.Custom:
                // Custom interactions handled separately
                break;
        }
    }
    
    /// <summary>
    /// Sets up basic interaction for items without XR components.
    /// </summary>
    /// <param name="item">The interactable item.</param>
    private void SetupBasicInteraction(InteractableItem item)
    {
        // Add our own component to handle interactions
        BasicInteractable interactable = item.itemObject.GetComponent<BasicInteractable>();
        
        if (interactable == null)
        {
            interactable = item.itemObject.AddComponent<BasicInteractable>();
        }
        
        // Set up the interactable
        interactable.ItemName = item.name;
        interactable.InteractionType = item.interactionType;
        
        // Add listener
        interactable.OnInteracted += (itemName) => {
            HandleItemInteraction(itemName);
        };
    }
    
    /// <summary>
    /// Handles interaction with an item.
    /// </summary>
    /// <param name="itemName">Name of the interacted item.</param>
    private void HandleItemInteraction(string itemName)
    {
        if (debugMode)
        {
            Debug.Log($"Interacted with item: {itemName}");
        }
        
        // Find the item in our list
        InteractableItem item = interactableItems.Find(i => i.name == itemName);
        
        if (item != null)
        {
            // Play interaction sound if available
            if (item.interactionSound != null && interactionAudioSource != null)
            {
                interactionAudioSource.PlayOneShot(item.interactionSound);
            }
            
            // Trigger event
            OnItemInteracted?.Invoke(itemName);
        }
    }
    
    /// <summary>
    /// Enables an interactable item by name.
    /// </summary>
    /// <param name="itemName">Name of the item to enable.</param>
    public void EnableItem(string itemName)
    {
        InteractableItem item = interactableItems.Find(i => i.name == itemName);
        
        if (item != null && item.itemObject != null)
        {
            item.itemObject.SetActive(true);
            _activeItems[itemName] = item.itemObject;
            
            if (debugMode)
            {
                Debug.Log($"Enabled interactable item: {itemName}");
            }
        }
    }
    
    /// <summary>
    /// Disables an interactable item by name.
    /// </summary>
    /// <param name="itemName">Name of the item to disable.</param>
    public void DisableItem(string itemName)
    {
        InteractableItem item = interactableItems.Find(i => i.name == itemName);
        
        if (item != null && item.itemObject != null)
        {
            item.itemObject.SetActive(false);
            
            if (_activeItems.ContainsKey(itemName))
            {
                _activeItems.Remove(itemName);
            }
            
            if (debugMode)
            {
                Debug.Log($"Disabled interactable item: {itemName}");
            }
        }
    }
    
    /// <summary>
    /// Gets an interactable item by name.
    /// </summary>
    /// <param name="itemName">Name of the item to get.</param>
    /// <returns>The item GameObject or null if not found.</returns>
    public GameObject GetItem(string itemName)
    {
        if (_activeItems.TryGetValue(itemName, out GameObject itemObject))
        {
            return itemObject;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all active interactable items.
    /// </summary>
    /// <returns>Dictionary of active items.</returns>
    public Dictionary<string, GameObject> GetActiveItems()
    {
        return _activeItems;
    }
    
    /// <summary>
    /// Adds a new interactable item at runtime.
    /// </summary>
    /// <param name="name">Item name.</param>
    /// <param name="itemObject">Item GameObject.</param>
    /// <param name="interactionType">Type of interaction.</param>
    public void AddInteractableItem(string name, GameObject itemObject, InteractionType interactionType = InteractionType.Grab)
    {
        if (itemObject == null)
        {
            Debug.LogError("Cannot add null item object!");
            return;
        }
        
        // Create new item
        InteractableItem newItem = new InteractableItem
        {
            name = name,
            itemObject = itemObject,
            startActive = true,
            interactionType = interactionType
        };
        
        // Add to items list
        interactableItems.Add(newItem);
        
        // Set up interaction
        SetupInteraction(newItem);
        
        // Add to active items
        _activeItems[name] = itemObject;
        
        if (debugMode)
        {
            Debug.Log($"Added new interactable item: {name}");
        }
    }
}

/// <summary>
/// Basic interactable component for objects without XR components.
/// </summary>
public class BasicInteractable : MonoBehaviour
{
    public string ItemName { get; set; }
    public InteractableItems.InteractionType InteractionType { get; set; }
    
    // Event
    public event Action<string> OnInteracted;
    
    private void OnMouseDown()
    {
        if (InteractionType == InteractableItems.InteractionType.Touch ||
            InteractionType == InteractableItems.InteractionType.Look)
        {
            OnInteracted?.Invoke(ItemName);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (InteractionType == InteractableItems.InteractionType.Touch &&
            other.CompareTag("Player"))
        {
            OnInteracted?.Invoke(ItemName);
        }
    }
}