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
    private GameObject levelIndicatorContainer;
    private Image[] levelIndicators;
    private TextMeshProUGUI levelText;
    
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
    /// Create level indicator UI (10 small bars showing current level)
    /// </summary>
    public void CreateLevelIndicator(int maxLevel = 10)
    {
        // Create container for level indicators at the bottom-right of the button
        levelIndicatorContainer = new GameObject("LevelIndicator");
        levelIndicatorContainer.transform.SetParent(transform, false);
        
        RectTransform containerRect = levelIndicatorContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(1, 0); // Bottom-right anchor
        containerRect.anchorMax = new Vector2(1, 0); // Bottom-right anchor
        containerRect.pivot = new Vector2(1, 0); // Bottom-right pivot
        containerRect.anchoredPosition = new Vector2(-5, 5); // 5px from right and bottom edges
        containerRect.sizeDelta = new Vector2(120, 20); // Increased height for text + bars
        
        // Create level text label ("Lvl 3/10")
        GameObject textObj = new GameObject("LevelText");
        textObj.transform.SetParent(levelIndicatorContainer.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 1);
        textRect.anchoredPosition = new Vector2(0, -2);
        textRect.sizeDelta = new Vector2(0, 12);
        
        levelText = textObj.AddComponent<TextMeshProUGUI>();
        levelText.text = "Lvl 0/10";
        levelText.fontSize = 10;
        levelText.fontStyle = FontStyles.Bold;
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.color = new Color(1f, 0.9f, 0.3f, 1f); // Golden yellow
        
        // Add glow to text using Outline
        Outline textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = new Color(1f, 0.6f, 0f, 0.8f); // Orange glow
        textOutline.effectDistance = new Vector2(1, -1);
        
        // Add shadow for depth
        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        textShadow.effectDistance = new Vector2(2, -2);
        
        // Create bars container
        GameObject barsContainer = new GameObject("Bars");
        barsContainer.transform.SetParent(levelIndicatorContainer.transform, false);
        
        RectTransform barsRect = barsContainer.AddComponent<RectTransform>();
        barsRect.anchorMin = new Vector2(0, 0);
        barsRect.anchorMax = new Vector2(1, 0);
        barsRect.pivot = new Vector2(0.5f, 0);
        barsRect.anchoredPosition = Vector2.zero;
        barsRect.sizeDelta = new Vector2(0, 8);
        
        // Add horizontal layout group for auto-spacing
        HorizontalLayoutGroup layout = barsContainer.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 2;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        
        // Create individual level bars
        levelIndicators = new Image[maxLevel];
        float barWidth = (120f - (maxLevel - 1) * 2) / maxLevel;
        
        for (int i = 0; i < maxLevel; i++)
        {
            GameObject barObj = new GameObject($"Level_{i + 1}");
            barObj.transform.SetParent(barsContainer.transform, false);
            
            RectTransform barRect = barObj.AddComponent<RectTransform>();
            barRect.sizeDelta = new Vector2(barWidth, 8);
            
            Image barImage = barObj.AddComponent<Image>();
            barImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Empty/inactive color
            
            // Add outline for glow effect on bars
            Outline barOutline = barObj.AddComponent<Outline>();
            barOutline.effectColor = new Color(0f, 0f, 0f, 0.3f);
            barOutline.effectDistance = new Vector2(1, -1);
            
            levelIndicators[i] = barImage;
        }
        
        levelIndicatorContainer.SetActive(false); // Hidden by default
    }
    
    /// <summary>
    /// Update level indicator to show current level out of max
    /// </summary>
    public void UpdateLevelIndicator(int currentLevel, int maxLevel, bool hasLevelSystem)
    {
        if (levelIndicatorContainer == null) return;
        
        // Show/hide based on whether this upgrade has a level system
        levelIndicatorContainer.SetActive(hasLevelSystem);
        
        if (!hasLevelSystem) return;
        
        // Update level text
        if (levelText != null)
        {
            levelText.text = $"Lvl {currentLevel}/{maxLevel}";
            
            // Change color based on progress
            if (currentLevel >= maxLevel)
            {
                levelText.color = new Color(1f, 0.3f, 0.3f, 1f); // Red when maxed
                levelText.GetComponent<Outline>().effectColor = new Color(0.5f, 0f, 0f, 0.8f);
            }
            else if (currentLevel >= maxLevel * 0.7f)
            {
                levelText.color = new Color(1f, 0.6f, 0.2f, 1f); // Orange when near max
                levelText.GetComponent<Outline>().effectColor = new Color(0.8f, 0.3f, 0f, 0.8f);
            }
            else
            {
                levelText.color = new Color(1f, 0.9f, 0.3f, 1f); // Golden yellow
                levelText.GetComponent<Outline>().effectColor = new Color(1f, 0.6f, 0f, 0.8f);
            }
        }
        
        // Update each bar's color based on current level
        for (int i = 0; i < levelIndicators.Length && i < maxLevel; i++)
        {
            if (levelIndicators[i] != null)
            {
                // Filled bars (up to current level) are bright with glow
                if (i < currentLevel)
                {
                    levelIndicators[i].color = new Color(0.2f, 1f, 0.4f, 1f); // Bright green for filled
                    
                    // Add glow to filled bars
                    Outline barOutline = levelIndicators[i].GetComponent<Outline>();
                    if (barOutline != null)
                    {
                        barOutline.effectColor = new Color(0f, 1f, 0.2f, 0.6f); // Green glow
                        barOutline.effectDistance = new Vector2(1.5f, -1.5f);
                    }
                }
                else
                {
                    levelIndicators[i].color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Dark for empty
                    
                    // Dim outline for empty bars
                    Outline barOutline = levelIndicators[i].GetComponent<Outline>();
                    if (barOutline != null)
                    {
                        barOutline.effectColor = new Color(0f, 0f, 0f, 0.3f);
                        barOutline.effectDistance = new Vector2(1, -1);
                    }
                }
                levelIndicators[i].gameObject.SetActive(true);
            }
        }
        
        // Hide excess indicators if maxLevel < 10
        for (int i = maxLevel; i < levelIndicators.Length; i++)
        {
            if (levelIndicators[i] != null)
            {
                levelIndicators[i].gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Show glow effect on hover
    /// </summary>
    private void ShowGlow()
    {
        if (glowOutline != null)
        {
            glowOutline.enabled = true;
            glowOutline.effectColor = new Color(0.3f, 0.6f, 1f, 0.8f);
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
