using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Represents a single upgrade button with its associated UI elements and hover behavior
/// </summary>
public class UpgradeButton : MonoBehaviour
{
    private Button button;
    private TextMeshProUGUI buttonText;
    private Image buttonImage;
    private int buttonIndex;
    private RectTransform rectTransform;
    private Outline glowOutline;
    
    public Button Button => button;
    public int ButtonIndex => buttonIndex;
    public RectTransform RectTransform => rectTransform;
    
    /// <summary>
    /// Initialize the upgrade button
    /// </summary>
    public void Initialize(int index)
    {
        buttonIndex = index;
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        buttonImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        
        // Add glow outline (hidden by default)
        glowOutline = gameObject.GetComponent<Outline>();
        if (glowOutline == null)
        {
            glowOutline = gameObject.AddComponent<Outline>();
        }
        glowOutline.effectColor = new Color(0.3f, 0.6f, 1f, 0f); // Transparent initially
        glowOutline.effectDistance = new Vector2(4, -4);
        glowOutline.enabled = false;
    }
    
    /// <summary>
    /// Update button text content
    /// </summary>
    public void SetText(string text)
    {
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }
    
    /// <summary>
    /// Update button background sprite
    /// </summary>
    public void SetSprite(Sprite sprite, Color normalColor, Color highlightColor, Color pressedColor)
    {
        if (buttonImage == null) return;
        
        if (sprite != null)
        {
            buttonImage.sprite = sprite;
            buttonImage.type = Image.Type.Sliced;
            
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 1f);
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            button.colors = colors;
        }
        else
        {
            buttonImage.sprite = null;
            ColorBlock colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightColor;
            colors.pressedColor = pressedColor;
            button.colors = colors;
        }
    }
    
    /// <summary>
    /// Add click listener
    /// </summary>
    public void AddClickListener(UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }
    
    /// <summary>
    /// Add hover detection for tooltips
    /// </summary>
    public void AddHoverDetection(System.Action<int> onEnter, System.Action onExit)
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<EventTrigger>();
        }
        
        // On Pointer Enter
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => 
        { 
            ShowGlow();
            onEnter?.Invoke(buttonIndex); 
        });
        trigger.triggers.Add(enterEntry);
        
        // On Pointer Exit
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => 
        { 
            HideGlow();
            onExit?.Invoke(); 
        });
        trigger.triggers.Add(exitEntry);
    }
    
    /// <summary>
    /// Show glow effect on hover
    /// </summary>
    private void ShowGlow()
    {
        if (glowOutline != null)
        {
            glowOutline.enabled = true;
            glowOutline.effectColor = new Color(0.3f, 0.6f, 1f, 1f); // Full opacity
        }
    }
    
    /// <summary>
    /// Hide glow effect
    /// </summary>
    private void HideGlow()
    {
        if (glowOutline != null)
        {
            glowOutline.effectColor = new Color(0.3f, 0.6f, 1f, 0f); // Transparent
            glowOutline.enabled = false;
        }
    }
}
