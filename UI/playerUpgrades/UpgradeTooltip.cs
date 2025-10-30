using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the tooltip display for upgrade buttons
/// </summary>
public class UpgradeTooltip : MonoBehaviour
{
    private GameObject tooltipPanel;
    private TextMeshProUGUI tooltipText;
    private CanvasGroup tooltipCanvasGroup;
    private RectTransform tooltipRect;
    private bool isVisible = false;
    private float offsetX = 20f; // Horizontal offset from button edge
    
    [SerializeField] private float fadeSpeed = 8f;
    
    public bool IsVisible => isVisible;
    
    /// <summary>
    /// Create tooltip UI elements
    /// </summary>
    public void Create(Transform parent, Color backgroundColor, Color textColor)
    {
        tooltipPanel = new GameObject("TooltipPanel");
        tooltipPanel.transform.SetParent(parent, false);
        
        tooltipRect = tooltipPanel.AddComponent<RectTransform>();
        tooltipRect.anchorMin = new Vector2(0.5f, 0f);
        tooltipRect.anchorMax = new Vector2(0.5f, 0f);
        tooltipRect.pivot = new Vector2(0.5f, 0f); // Bottom center pivot
        tooltipRect.anchoredPosition = new Vector2(0, 20); // Fixed position at bottom center
        tooltipRect.sizeDelta = new Vector2(800, 120);
        
        tooltipCanvasGroup = tooltipPanel.AddComponent<CanvasGroup>();
        tooltipCanvasGroup.alpha = 0f;
        tooltipCanvasGroup.blocksRaycasts = false;
        
        Image tooltipBg = tooltipPanel.AddComponent<Image>();
        // Glass effect with transparency
        tooltipBg.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.75f);
        
        Outline outline = tooltipPanel.AddComponent<Outline>();
        outline.effectColor = new Color(0.3f, 0.6f, 0.9f, 0.5f);
        outline.effectDistance = new Vector2(2, -2);
        
        GameObject textObj = new GameObject("TooltipText");
        textObj.transform.SetParent(tooltipPanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-40, -20); // Padding for text
        textRect.anchoredPosition = Vector2.zero;
        
        tooltipText = textObj.AddComponent<TextMeshProUGUI>();
        tooltipText.fontSize = 16;
        tooltipText.color = textColor;
        tooltipText.alignment = TextAlignmentOptions.Center;
        tooltipText.verticalAlignment = VerticalAlignmentOptions.Middle;
        tooltipText.enableWordWrapping = true;
        tooltipText.text = "";
        tooltipText.overflowMode = TextOverflowModes.Ellipsis;
        
        tooltipPanel.SetActive(false);
    }
    
    /// <summary>
    /// Show tooltip with text content at fixed bottom center position
    /// </summary>
    public void Show(string text, RectTransform buttonRect)
    {
        if (tooltipPanel == null || tooltipText == null) 
        {
            Debug.LogWarning("UpgradeTooltip: Tooltip panel or text is null!");
            return;
        }
        
        tooltipText.text = text;
        
        // Keep fixed position at bottom center (no dynamic positioning)
        
        tooltipPanel.SetActive(true);
        tooltipPanel.transform.SetAsLastSibling(); // Ensure it renders on top
        isVisible = true;
        
        // Reset alpha to start fade
        if (tooltipCanvasGroup != null)
        {
            tooltipCanvasGroup.alpha = 0f;
        }
    }
    
    /// <summary>
    /// Hide tooltip
    /// </summary>
    public void Hide()
    {
        if (tooltipPanel == null) return;
        
        isVisible = false;
        tooltipPanel.SetActive(false);
    }
    
    /// <summary>
    /// Update fade animation
    /// </summary>
    public void UpdateFade()
    {
        if (tooltipCanvasGroup == null) return;
        
        float targetAlpha = isVisible ? 1f : 0f;
        tooltipCanvasGroup.alpha = Mathf.Lerp(tooltipCanvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
    }
}
