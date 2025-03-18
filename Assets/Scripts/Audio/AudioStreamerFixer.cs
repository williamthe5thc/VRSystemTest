using UnityEngine;
using System.Collections;

namespace VRInterview.Audio
{
    /// <summary>
    /// Utility component to find or create AudioStreamer component and ensure it's properly connected.
    /// This helps resolve issues with streaming audio from AllTalk.
    /// </summary>
    [DefaultExecutionOrder(-100)]  // Make sure this initializes early
    public class AudioStreamerFixer : MonoBehaviour
    {
        [SerializeField] private MessageHandler messageHandler;
        [SerializeField] private AudioPlayback audioPlayback;
        
        // Reference to created or found AudioStreamer
        private AudioStreamer audioStreamer;
        
        private void Awake()
        {
            Debug.Log("AudioStreamerFixer initializing...");
            
            // Find references if not set in inspector
            if (messageHandler == null)
            {
                messageHandler = FindObjectOfType<MessageHandler>();
                if (messageHandler == null)
                {
                    Debug.LogError("MessageHandler not found in scene! Cannot properly set up AudioStreamer.");
                }
            }
            
            if (audioPlayback == null)
            {
                audioPlayback = FindObjectOfType<AudioPlayback>();
                if (audioPlayback == null)
                {
                    Debug.LogError("AudioPlayback not found in scene! Cannot properly set up AudioStreamer.");
                }
            }
            
            // Find existing AudioStreamer or create a new one
            audioStreamer = FindObjectOfType<AudioStreamer>();
            if (audioStreamer == null)
            {
                Debug.Log("AudioStreamer not found in scene, creating one...");
                GameObject streamerObject = new GameObject("AudioStreamer");
                streamerObject.transform.SetParent(transform);
                audioStreamer = streamerObject.AddComponent<AudioStreamer>();
                Debug.Log("AudioStreamer created successfully");
            }
            else
            {
                Debug.Log("Found existing AudioStreamer");
            }
            
            // Give Unity a frame to initialize components
            StartCoroutine(ConnectComponents());
        }
        
        private IEnumerator ConnectComponents()
        {
            // Wait for a frame to ensure all components are initialized
            yield return null;
            
            // Connect AudioStreamer to MessageHandler
            if (messageHandler != null && audioStreamer != null)
            {
                // Use reflection to set the field
                var field = typeof(MessageHandler).GetField("audioStreamer", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Public);
                
                if (field != null)
                {
                    var currentValue = field.GetValue(messageHandler);
                    if (currentValue == null)
                    {
                        Debug.Log("Connecting AudioStreamer to MessageHandler");
                        field.SetValue(messageHandler, audioStreamer);
                    }
                    else
                    {
                        Debug.Log("MessageHandler already has an AudioStreamer reference");
                    }
                }
                else
                {
                    Debug.LogError("Could not find audioStreamer field in MessageHandler via reflection");
                }
            }
            
            // Connect SessionManager to AudioStreamer if needed
            if (audioStreamer != null)
            {
                var sessionManager = FindObjectOfType<SessionManager>();
                if (sessionManager != null)
                {
                    var field = typeof(AudioStreamer).GetField("sessionManager", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Public);
                    
                    if (field != null)
                    {
                        var currentValue = field.GetValue(audioStreamer);
                        if (currentValue == null)
                        {
                            Debug.Log("Connecting SessionManager to AudioStreamer");
                            field.SetValue(audioStreamer, sessionManager);
                        }
                    }
                }
            }
            
            Debug.Log("AudioStreamerFixer setup complete");
        }
    }
}
