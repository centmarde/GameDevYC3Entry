using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Minimalist UI for player stat upgrades after each wave
/// Auto-creates its own canvas and UI elements
/// </summary>
public class PlayerUpgradeUI : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color buttonHighlight = new Color(0.3f, 0.6f, 0.9f, 1f);
    [SerializeField] private Color textColor = Color.white;
    
    private Canvas canvas;
    private GameObject upgradePanel;
    private TextMeshProUGUI titleText;
    private Button damageButton;
    private Button healthButton;
    private Button speedButton;
    private PlayerUpgradeManager upgradeManager;
    
    private void Awake()
    {
        CreateUI();
    }
    
    /// <summary>
    /// Setup UI with reference to upgrade manager
    /// </summary>
    public void SetupWithManager(PlayerUpgradeManager manager)
    {
        upgradeManager = manager;
    }
    
    /// <summary>
    /// Create all UI elements programmatically
    /// </summary>
    private void CreateUI()
    {
        // Create canvas
        CreateCanvas();
        
        // Create main panel
        CreateUpgradePanel();
        
        // Create title
        CreateTitle();
        
        // Create upgrade buttons
        CreateUpgradeButtons();
        
        // Hide panel initially
        upgradePanel.SetActive(false);
    }
    
    /// <summary>
    /// Create the main canvas
    /// </summary>
    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("UpgradeCanvas");
        canvasObj.transform.SetParent(transform, false);
        
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Always on top
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
    }
    
    /// <summary>
    /// Create the upgrade panel
    /// </summary>
    private void CreateUpgradePanel()
    {
        upgradePanel = new GameObject("UpgradePanel");
        upgradePanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = upgradePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(600, 400);
        
        Image panelImage = upgradePanel.AddComponent<Image>();
        panelImage.color = panelColor;
    }
    
    /// <summary>
    /// Create title text
    /// </summary>
    private void CreateTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(upgradePanel.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(560, 60);
        
        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "CHOOSE UPGRADE";
        titleText.fontSize = 42;
        titleText.color = textColor;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
    }
    
    /// <summary>
    /// Create upgrade buttons
    /// </summary>
    private void CreateUpgradeButtons()
    {
        float buttonY = -120;
        float buttonSpacing = 90;
        
        // Damage button
        damageButton = CreateButton("DamageButton", new Vector2(0, buttonY), "⚔ DAMAGE", OnDamageButtonClicked);
        
        // Health button
        healthButton = CreateButton("HealthButton", new Vector2(0, buttonY - buttonSpacing), "❤ HEALTH", OnHealthButtonClicked);
        
        // Speed button
        speedButton = CreateButton("SpeedButton", new Vector2(0, buttonY - buttonSpacing * 2), "⚡ SPEED", OnSpeedButtonClicked);
    }
    
    /// <summary>
    /// Create a single button
    /// </summary>
    private Button CreateButton(string name, Vector2 position, string buttonText, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(upgradePanel.transform, false);
        
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
        
        // Set button colors
        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonHighlight;
        colors.pressedColor = new Color(0.5f, 0.7f, 1f, 1f);
        colors.selectedColor = buttonHighlight;
        button.colors = colors;
        
        button.onClick.AddListener(onClick);
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 32;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        
        return button;
    }
    
    /// <summary>
    /// Update button texts with current stats and upgrade amounts
    /// </summary>
    private void UpdateButtonTexts()
    {
        if (upgradeManager == null) return;
        
        // Update damage button
        if (damageButton != null)
        {
            var text = damageButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                float current = upgradeManager.GetCurrentDamage();
                float upgrade = upgradeManager.GetDamageUpgradeAmount();
                text.text = $"⚔ DAMAGE\n<size=24>{current:F1} → {current + upgrade:F1}</size>";
            }
        }
        
        // Update health button
        if (healthButton != null)
        {
            var text = healthButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                float current = upgradeManager.GetCurrentHealth();
                float upgrade = upgradeManager.GetHealthUpgradeAmount();
                text.text = $"❤ HEALTH\n<size=24>{current:F0} → {current + upgrade:F0}</size>";
            }
        }
        
        // Update speed button
        if (speedButton != null)
        {
            var text = speedButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                float current = upgradeManager.GetCurrentSpeed();
                float upgrade = upgradeManager.GetSpeedUpgradeAmount();
                text.text = $"⚡ SPEED\n<size=24>{current:F1} → {current + upgrade:F1}</size>";
            }
        }
    }
    
    /// <summary>
    /// Show the upgrade panel
    /// </summary>
    public void ShowUpgradePanel()
    {
        if (upgradePanel != null)
        {
            UpdateButtonTexts();
            upgradePanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide the upgrade panel
    /// </summary>
    public void HideUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }
    
    // Button click handlers
    private void OnDamageButtonClicked()
    {
        if (upgradeManager != null)
        {
            upgradeManager.ApplyUpgrade(PlayerUpgradeManager.UpgradeType.Damage);
        }
    }
    
    private void OnHealthButtonClicked()
    {
        if (upgradeManager != null)
        {
            upgradeManager.ApplyUpgrade(PlayerUpgradeManager.UpgradeType.Health);
        }
    }
    
    private void OnSpeedButtonClicked()
    {
        if (upgradeManager != null)
        {
            upgradeManager.ApplyUpgrade(PlayerUpgradeManager.UpgradeType.Speed);
        }
    }
}
