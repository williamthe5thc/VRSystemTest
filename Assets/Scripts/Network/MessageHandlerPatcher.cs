using UnityEngine;

/// <summary>
/// This component patches the MessageHandler to add support for text_response messages.
/// Add this component to the same GameObject that has the MessageHandler.
/// </summary>
public class MessageHandlerPatcher : MonoBehaviour
{
    [Tooltip("The message handler to patch")]
    [SerializeField] private MessageHandler messageHandler;
    
    void Start()
    {
        // Find message handler if not assigned
        if (messageHandler == null)
        {
            messageHandler = GetComponent<MessageHandler>();
            if (messageHandler == null)
            {
                messageHandler = FindObjectOfType<MessageHandler>();
                if (messageHandler == null)
                {
                    Debug.LogError("MessageHandlerPatcher cannot find a MessageHandler component");
                    return;
                }
            }
        }
        
        // Apply patch
        Debug.Log("Applying text_response handler patch");
        messageHandler.PatchForTextResponses();
        
        // Show success message
        Debug.Log("MessageHandlerPatcher has successfully patched the MessageHandler");
    }
}