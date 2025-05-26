using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Helper class for creating UI elements with proper components and RectTransforms
/// </summary>
public static class UIFixHelper
{
    /// <summary>
    /// Creates a UI text element with proper RectTransform
    /// </summary>
    public static GameObject CreateUIText(string name, Transform parent)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = name;
        text.color = Color.white;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        
        return textObject;
    }
    
    /// <summary>
    /// Creates a UI input field with proper RectTransform
    /// </summary>
    public static GameObject CreateUIInputField(string name, Transform parent)
    {
        GameObject inputObject = new GameObject(name, typeof(RectTransform));
        inputObject.transform.SetParent(parent, false);
        
        Image background = inputObject.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f);
        
        TMP_InputField inputField = inputObject.AddComponent<TMP_InputField>();
        inputField.text = "ws://localhost:8765";
        
        // Create text area
        GameObject textArea = new GameObject("Text Area", typeof(RectTransform));
        textArea.transform.SetParent(inputObject.transform, false);
        
        RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
        textAreaRect.anchorMin = new Vector2(0, 0);
        textAreaRect.anchorMax = new Vector2(1, 1);
        textAreaRect.offsetMin = new Vector2(10, 0);
        textAreaRect.offsetMax = new Vector2(-10, 0);
        
        // Create text component
        GameObject textComponent = new GameObject("Text", typeof(RectTransform));
        textComponent.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI text = textComponent.AddComponent<TextMeshProUGUI>();
        text.color = Color.white;
        text.fontSize = 18;
        
        RectTransform textRect = textComponent.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.sizeDelta = Vector2.zero;
        
        // Create placeholder
        GameObject placeholder = new GameObject("Placeholder", typeof(RectTransform));
        placeholder.transform.SetParent(inputObject.transform, false);
        TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "Enter server URL...";
        placeholderText.color = new Color(1, 1, 1, 0.5f);
        placeholderText.fontSize = 18;
        
        RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
        placeholderRect.anchorMin = new Vector2(0, 0);
        placeholderRect.anchorMax = new Vector2(1, 1);
        placeholderRect.offsetMin = new Vector2(10, 0);
        placeholderRect.offsetMax = new Vector2(-10, 0);
        
        // Setup input field
        inputField.textComponent = text;
        inputField.placeholder = placeholderText;
        
        return inputObject;
    }
    
    /// <summary>
    /// Creates a button with proper RectTransform
    /// </summary>
    public static GameObject CreateUIButton(string name, string text, Color color, Transform parent)
    {
        GameObject button = new GameObject(name, typeof(RectTransform));
        button.transform.SetParent(parent, false);
        
        Image buttonImage = button.AddComponent<Image>();
        buttonImage.color = color;
        
        Button buttonComponent = button.AddComponent<Button>();
        buttonComponent.targetGraphic = buttonImage;
        
        // Create button text
        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(button.transform, false);
        
        TextMeshProUGUI buttonText = textObject.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.color = Color.white;
        buttonText.fontSize = 24;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        return button;
    }
    
    /// <summary>
    /// Creates a UI dropdown with proper RectTransform
    /// </summary>
    public static GameObject CreateUIDropdown(string name, Transform parent)
    {
        GameObject dropdownObject = new GameObject(name, typeof(RectTransform));
        dropdownObject.transform.SetParent(parent, false);
        
        Image background = dropdownObject.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f);
        
        TMP_Dropdown dropdown = dropdownObject.AddComponent<TMP_Dropdown>();
        
        // Create text
        GameObject textObject = new GameObject("Label", typeof(RectTransform));
        textObject.transform.SetParent(dropdownObject.transform, false);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.Left;
        
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-25, 0);
        
        // Create arrow
        GameObject arrow = new GameObject("Arrow", typeof(RectTransform));
        arrow.transform.SetParent(dropdownObject.transform, false);
        Image arrowImage = arrow.AddComponent<Image>();
        arrowImage.color = Color.white;
        
        RectTransform arrowRect = arrow.GetComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.pivot = new Vector2(1, 0.5f);
        arrowRect.sizeDelta = new Vector2(20, 20);
        arrowRect.anchoredPosition = new Vector2(-10, 0);
        
        // Set up the dropdown component
        dropdown.captionText = text;
        
        return dropdownObject;
    }
    
    /// <summary>
    /// Creates a UI panel with proper RectTransform
    /// </summary>
    public static GameObject CreateUIPanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        
        Image image = panel.AddComponent<Image>();
        image.color = color;
        
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        return panel;
    }
}
