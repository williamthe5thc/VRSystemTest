using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Editor tool to create and set up mobile-optimized materials for VR
/// </summary>
public class MobileOptimizedMaterialCreator : EditorWindow
{
    private enum MaterialType
    {
        Opaque,
        Transparent,
        Highlight,
        UI,
        WorldSpaceUI
    }
    
    private MaterialType materialType = MaterialType.Opaque;
    private string materialName = "New Material";
    private Color materialColor = Color.white;
    private Texture2D albedoTexture;
    private float smoothness = 0.5f;
    private float metallic = 0.0f;
    private bool useEmission = false;
    private Color emissionColor = Color.black;
    private float alpha = 1.0f;
    private Color highlightColor = new Color(0, 1, 1);
    private float highlightIntensity = 0.5f;
    private float pulseSpeed = 1.0f;
    private float edgeFalloff = 1.0f;
    private Color rimColor = Color.white;
    private float rimPower = 3.0f;
    private bool vertexColorTint = false;
    
    private string savePath = "Assets/Materials";
    
    [MenuItem("Tools/VR Interview System/Create Mobile-Optimized Material")]
    public static void ShowWindow()
    {
        GetWindow<MobileOptimizedMaterialCreator>("Material Creator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("VR Interview System - Mobile Material Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        materialType = (MaterialType)EditorGUILayout.EnumPopup("Material Type:", materialType);
        materialName = EditorGUILayout.TextField("Material Name:", materialName);
        materialColor = EditorGUILayout.ColorField("Main Color:", materialColor);
        albedoTexture = (Texture2D)EditorGUILayout.ObjectField("Albedo Texture:", albedoTexture, typeof(Texture2D), false);
        
        // Show material type specific properties
        switch (materialType)
        {
            case MaterialType.Opaque:
                smoothness = EditorGUILayout.Slider("Smoothness:", smoothness, 0f, 1f);
                metallic = EditorGUILayout.Slider("Metallic:", metallic, 0f, 1f);
                useEmission = EditorGUILayout.Toggle("Use Emission:", useEmission);
                if (useEmission)
                {
                    emissionColor = EditorGUILayout.ColorField("Emission Color:", emissionColor);
                }
                vertexColorTint = EditorGUILayout.Toggle("Vertex Color Tint:", vertexColorTint);
                break;
                
            case MaterialType.Transparent:
                smoothness = EditorGUILayout.Slider("Smoothness:", smoothness, 0f, 1f);
                alpha = EditorGUILayout.Slider("Alpha:", alpha, 0f, 1f);
                break;
                
            case MaterialType.Highlight:
                highlightColor = EditorGUILayout.ColorField("Highlight Color:", highlightColor);
                highlightIntensity = EditorGUILayout.Slider("Highlight Intensity:", highlightIntensity, 0f, 1f);
                pulseSpeed = EditorGUILayout.Slider("Pulse Speed:", pulseSpeed, 0f, 10f);
                break;
                
            case MaterialType.UI:
                edgeFalloff = EditorGUILayout.Slider("Edge Visibility:", edgeFalloff, 0f, 5f);
                break;
                
            case MaterialType.WorldSpaceUI:
                rimColor = EditorGUILayout.ColorField("Rim Color:", rimColor);
                rimPower = EditorGUILayout.Slider("Rim Power:", rimPower, 0.5f, 8f);
                break;
        }
        
        EditorGUILayout.Space();
        
        // Save path
        savePath = EditorGUILayout.TextField("Save Path:", savePath);
        
        // Create material button
        if (GUILayout.Button("Create Material"))
        {
            CreateMaterial();
        }
        
        // Quick create buttons for common materials
        EditorGUILayout.Space();
        GUILayout.Label("Quick Create Common Materials", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("White Wall"))
        {
            materialName = "Wall_White";
            materialColor = new Color(0.9f, 0.9f, 0.9f);
            materialType = MaterialType.Opaque;
            smoothness = 0.1f;
            metallic = 0.0f;
            useEmission = false;
            CreateMaterial();
        }
        if (GUILayout.Button("Wood Floor"))
        {
            materialName = "Floor_Wood";
            materialColor = new Color(0.6f, 0.4f, 0.2f);
            materialType = MaterialType.Opaque;
            smoothness = 0.3f;
            metallic = 0.0f;
            useEmission = false;
            CreateMaterial();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Blue Glass"))
        {
            materialName = "Glass_Blue";
            materialColor = new Color(0.7f, 0.85f, 1.0f, 0.5f);
            materialType = MaterialType.Transparent;
            smoothness = 0.8f;
            alpha = 0.5f;
            CreateMaterial();
        }
        if (GUILayout.Button("Metal"))
        {
            materialName = "Metal_Basic";
            materialColor = new Color(0.8f, 0.8f, 0.8f);
            materialType = MaterialType.Opaque;
            smoothness = 0.7f;
            metallic = 0.8f;
            useEmission = false;
            CreateMaterial();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Interactable Highlight"))
        {
            materialName = "Interactable_Highlight";
            materialColor = Color.white;
            materialType = MaterialType.Highlight;
            highlightColor = new Color(0.0f, 0.7f, 1.0f);
            highlightIntensity = 0.5f;
            pulseSpeed = 2.0f;
            CreateMaterial();
        }
        if (GUILayout.Button("UI Panel"))
        {
            materialName = "UI_Panel";
            materialColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
            materialType = MaterialType.WorldSpaceUI;
            rimColor = new Color(0.5f, 0.5f, 1.0f, 0.5f);
            rimPower = 2.0f;
            CreateMaterial();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        // Create material set for office environments
        if (GUILayout.Button("Create Complete Office Material Set"))
        {
            CreateOfficeMaterialSet();
        }
    }
    
    private void CreateMaterial()
    {
        // Make sure the save directory exists
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        
        // Create the material
        Material material = null;
        
        switch (materialType)
        {
            case MaterialType.Opaque:
                material = new Material(Shader.Find("VRInterviewSystem/MobileOptimized"));
                material.SetFloat("_Glossiness", smoothness);
                material.SetFloat("_Metallic", metallic);
                material.SetFloat("_UseEmission", useEmission ? 1.0f : 0.0f);
                material.SetColor("_EmissionColor", emissionColor);
                material.SetFloat("_VertexColorTint", vertexColorTint ? 1.0f : 0.0f);
                break;
                
            case MaterialType.Transparent:
                material = new Material(Shader.Find("VRInterviewSystem/MobileTransparent"));
                material.SetFloat("_Glossiness", smoothness);
                material.SetFloat("_Alpha", alpha);
                break;
                
            case MaterialType.Highlight:
                material = new Material(Shader.Find("VRInterviewSystem/Highlight"));
                material.SetColor("_HighlightColor", highlightColor);
                material.SetFloat("_HighlightIntensity", highlightIntensity);
                material.SetFloat("_PulseSpeed", pulseSpeed);
                break;
                
            case MaterialType.UI:
                material = new Material(Shader.Find("VRInterviewSystem/UI"));
                material.SetFloat("_EdgeFalloff", edgeFalloff);
                break;
                
            case MaterialType.WorldSpaceUI:
                material = new Material(Shader.Find("VRInterviewSystem/WorldSpaceUI"));
                material.SetColor("_RimColor", rimColor);
                material.SetFloat("_RimPower", rimPower);
                break;
        }
        
        if (material != null)
        {
            // Apply common properties
            material.SetColor("_Color", materialColor);
            if (albedoTexture != null)
            {
                material.SetTexture("_MainTex", albedoTexture);
            }
            
            // Save the material
            string filename = Path.Combine(savePath, materialName + ".mat");
            AssetDatabase.CreateAsset(material, filename);
            AssetDatabase.SaveAssets();
            
            // Show the material in the Project view
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = material;
            
            Debug.Log($"Material '{materialName}' created at '{filename}'");
        }
        else
        {
            Debug.LogError("Failed to create material. Shader not found.");
        }
    }
    
    private void CreateOfficeMaterialSet()
    {
        // Define material configurations for office set
        List<MaterialConfig> officeMaterials = new List<MaterialConfig>
        {
            // Walls
            new MaterialConfig
            {
                name = "Wall_Corporate",
                type = MaterialType.Opaque,
                color = new Color(0.85f, 0.85f, 0.85f),
                smoothness = 0.1f,
                metallic = 0.0f
            },
            new MaterialConfig
            {
                name = "Wall_Startup",
                type = MaterialType.Opaque,
                color = new Color(0.95f, 0.95f, 0.95f),
                smoothness = 0.1f,
                metallic = 0.0f
            },
            new MaterialConfig
            {
                name = "Wall_Casual",
                type = MaterialType.Opaque,
                color = new Color(0.9f, 0.85f, 0.7f),
                smoothness = 0.1f,
                metallic = 0.0f
            },
            
            // Floors
            new MaterialConfig
            {
                name = "Floor_Corporate",
                type = MaterialType.Opaque,
                color = new Color(0.3f, 0.3f, 0.3f),
                smoothness = 0.3f,
                metallic = 0.0f
            },
            new MaterialConfig
            {
                name = "Floor_Startup",
                type = MaterialType.Opaque,
                color = new Color(0.7f, 0.7f, 0.7f),
                smoothness = 0.2f,
                metallic = 0.0f
            },
            new MaterialConfig
            {
                name = "Floor_Casual",
                type = MaterialType.Opaque,
                color = new Color(0.4f, 0.3f, 0.2f),
                smoothness = 0.2f,
                metallic = 0.0f
            },
            
            // Furniture - Corporate
            new MaterialConfig
            {
                name = "Desk_Corporate",
                type = MaterialType.Opaque,
                color = new Color(0.3f, 0.2f, 0.1f),
                smoothness = 0.5f,
                metallic = 0.0f
            },
            new MaterialConfig
            {
                name = "Chair_Corporate",
                type = MaterialType.Opaque,
                color = new Color(0.1f, 0.1f, 0.1f),
                smoothness = 0.3f,
                metallic = 0.0f
            },
            
            // Furniture - Startup
            new MaterialConfig
            {
                name = "Desk_Startup",
                type = MaterialType.Opaque,
                color = new Color(0.8f, 0.8f, 0.8f),
                smoothness = 0.5f,
                metallic = 0.3f
            },
            new MaterialConfig
            {
                name = "Chair_Startup",
                type = MaterialType.Opaque,
                color = new Color(0.2f, 0.6f, 0.8f),
                smoothness = 0.3f,
                metallic = 0.0f
            },
            
            // Furniture - Casual
            new MaterialConfig
            {
                name = "Couch_Casual",
                type = MaterialType.Opaque,
                color = new Color(0.6f, 0.4f, 0.2f),
                smoothness = 0.2f,
                metallic = 0.0f
            },
            new MaterialConfig
            {
                name = "Table_Casual",
                type = MaterialType.Opaque,
                color = new Color(0.4f, 0.3f, 0.2f),
                smoothness = 0.4f,
                metallic = 0.0f
            },
            
            // Glass - All environments
            new MaterialConfig
            {
                name = "Glass_Window",
                type = MaterialType.Transparent,
                color = new Color(0.7f, 0.85f, 1.0f, 0.5f),
                smoothness = 0.8f,
                alpha = 0.5f
            },
            
            // Metals
            new MaterialConfig
            {
                name = "Metal_Chrome",
                type = MaterialType.Opaque,
                color = new Color(0.9f, 0.9f, 0.9f),
                smoothness = 0.9f,
                metallic = 1.0f
            },
            
            // UI Materials
            new MaterialConfig
            {
                name = "UI_Panel_Dark",
                type = MaterialType.WorldSpaceUI,
                color = new Color(0.2f, 0.2f, 0.3f, 0.8f),
                rimColor = new Color(0.5f, 0.5f, 1.0f, 0.5f),
                rimPower = 2.0f
            },
            new MaterialConfig
            {
                name = "UI_Button",
                type = MaterialType.WorldSpaceUI,
                color = new Color(0.3f, 0.5f, 0.8f, 1.0f),
                rimColor = new Color(0.7f, 0.7f, 1.0f, 0.7f),
                rimPower = 1.5f
            },
            
            // Interactive objects
            new MaterialConfig
            {
                name = "Interactable_Object",
                type = MaterialType.Highlight,
                color = Color.white,
                highlightColor = new Color(0.0f, 0.7f, 1.0f),
                highlightIntensity = 0.5f,
                pulseSpeed = 2.0f
            }
        };
        
        // Create a special folder for the office material set
        string officeMaterialsPath = Path.Combine(savePath, "OfficeMaterials");
        if (!Directory.Exists(officeMaterialsPath))
        {
            Directory.CreateDirectory(officeMaterialsPath);
        }
        
        // Create each material in the set
        foreach (var config in officeMaterials)
        {
            // Store current settings
            string originalName = materialName;
            MaterialType originalType = materialType;
            Color originalColor = materialColor;
            float originalSmoothness = smoothness;
            float originalMetallic = metallic;
            float originalAlpha = alpha;
            Color originalHighlightColor = highlightColor;
            float originalHighlightIntensity = highlightIntensity;
            float originalPulseSpeed = pulseSpeed;
            Color originalRimColor = rimColor;
            float originalRimPower = rimPower;
            
            // Apply configuration
            materialName = config.name;
            materialType = config.type;
            materialColor = config.color;
            smoothness = config.smoothness;
            metallic = config.metallic;
            alpha = config.alpha;
            highlightColor = config.highlightColor;
            highlightIntensity = config.highlightIntensity;
            pulseSpeed = config.pulseSpeed;
            rimColor = config.rimColor;
            rimPower = config.rimPower;
            
            // Save the original path
            string originalPath = savePath;
            savePath = officeMaterialsPath;
            
            // Create the material
            CreateMaterial();
            
            // Restore original settings
            materialName = originalName;
            materialType = originalType;
            materialColor = originalColor;
            smoothness = originalSmoothness;
            metallic = originalMetallic;
            alpha = originalAlpha;
            highlightColor = originalHighlightColor;
            highlightIntensity = originalHighlightIntensity;
            pulseSpeed = originalPulseSpeed;
            rimColor = originalRimColor;
            rimPower = originalRimPower;
            savePath = originalPath;
        }
        
        Debug.Log($"Created {officeMaterials.Count} materials in '{officeMaterialsPath}'");
    }
    
    private class MaterialConfig
    {
        public string name = "New Material";
        public MaterialType type = MaterialType.Opaque;
        public Color color = Color.white;
        public float smoothness = 0.5f;
        public float metallic = 0.0f;
        public float alpha = 1.0f;
        public Color highlightColor = new Color(0, 1, 1);
        public float highlightIntensity = 0.5f;
        public float pulseSpeed = 1.0f;
        public Color rimColor = Color.white;
        public float rimPower = 3.0f;
    }
}