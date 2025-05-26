using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] public string nextScene = "Scenes/MainMenu";
    [SerializeField] public float delay = 0.5f;
    
    private void Start()
    {
        // Make sure this GameObject persists between scenes
        DontDestroyOnLoad(transform.root.gameObject);
        
        Debug.Log("Bootstrap initializing core components...");
        ConnectComponents();
        
        // Allow time for system initialization
        Invoke("LoadNextScene", delay);
    }
    
    private void ConnectComponents()
    {
        var webSocketClient = GetComponentInChildren<WebSocketClient>();
        var messageHandler = GetComponentInChildren<MessageHandler>();
        var sessionManager = GetComponentInChildren<SessionManager>();
        
        // Check for key components
        if (!webSocketClient || !messageHandler || !sessionManager)
        {
            Debug.LogError("Missing one or more core components!");
            return;
        }
        
        // Connect the required components through references
        // Make sure these match your actual property names
        
        // Example method to assign references (use SerializeField references instead if possible)
        if (messageHandler && webSocketClient)
        {
            // This is just an example - use the actual method or property defined in your class
            Debug.Log("Connecting MessageHandler to WebSocketClient");
            // messageHandler.SetWebSocketClient(webSocketClient);
        }
    }
    
    private void LoadNextScene()
    {
        Debug.Log("Bootstrap complete, loading: " + nextScene);
        SceneManager.LoadScene(nextScene);
    }
}