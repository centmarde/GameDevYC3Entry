using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Factory class for creating UI elements programmatically
/// </summary>
public static class UpgradeUIFactory
{
    /// <summary>
    /// Create the main canvas
    /// </summary>
    public static Canvas CreateCanvas(Transform parent)
    {
        GameObject canvasObj = new GameObject("UpgradeCanvas");
        canvasObj.transform.SetParent(parent, false);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767; // Maximum sorting order to ensure front-most rendering
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        return canvas;
    }
    
    /// <summary>
    /// Create the upgrade panel
    /// </summary>
    public static GameObject CreateUpgradePanel(Transform parent, Color panelColor)
    {
        GameObject upgradePanel = new GameObject("UpgradePanel");
        upgradePanel.transform.SetParent(parent, false);
        
        RectTransform panelRect = upgradePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(600, 400);
        
        Image panelImage = upgradePanel.AddComponent<Image>();
        panelImage.color = panelColor;
        
        return upgradePanel;
    }
    
    /// <summary>
    /// Create title text
    /// </summary>
    public static TextMeshProUGUI CreateTitle(Transform parent, Color textColor)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(560, 60);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "CHOOSE UPGRADE";
        titleText.fontSize = 42;
        titleText.color = textColor;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        
        return titleText;
    }
    
    /// <summary>
    /// Create a single upgrade button
    /// </summary>
    public static UpgradeButton CreateUpgradeButton(Transform parent, string name, Vector2 position, 
        Color buttonColor, Color buttonHighlight, Color textColor)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(500, 70);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = buttonColor;
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonHighlight;
        colors.pressedColor = new Color(0.5f, 0.7f, 1f, 1f);
        colors.selectedColor = buttonHighlight;
        button.colors = colors;
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "UPGRADE";
        text.fontSize = 32;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        
        UpgradeButton upgradeButton = buttonObj.AddComponent<UpgradeButton>();
        
        return upgradeButton;
    }
}
