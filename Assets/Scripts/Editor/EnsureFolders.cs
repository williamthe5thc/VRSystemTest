using UnityEditor;

namespace VRInterview.Editor
{
    /// <summary>
    /// Ensures that required folders exist in the project
    /// </summary>
    [InitializeOnLoad]
    public class EnsureFolders
    {
        static EnsureFolders()
        {
            // Ensure folders exist
            CreateFolderIfNotExists("Assets", "Animations");
            CreateFolderIfNotExists("Assets", "Resources");
        }
        
        private static void CreateFolderIfNotExists(string parent, string folder)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{folder}"))
            {
                AssetDatabase.CreateFolder(parent, folder);
                UnityEngine.Debug.Log($"Created folder: {parent}/{folder}");
            }
        }
    }
}
