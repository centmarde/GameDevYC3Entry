using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Minimalist UI for player stat upgrades after each wave
/// Supports both auto-creation and manual setup of UI elements
/// </summary>
public class PlayerUpgradeUI : MonoBehaviour
{
    [Header("Setup Mode")]
    [SerializeField] private bool autoCreateUI = true;
    [Tooltip("If true, UI elements will be created programmatically. If false, assign manual references below.")]
    
    [Header("Manual Setup (Only if Auto-Create is OFF)")]
    [SerializeField] private Canvas manualCanvas;
    [SerializeField] private GameObject manualUpgradePanel;
    [SerializeField] private TextMeshProUGUI manualTitleText;
    [SerializeField] private Button manualButton1;
    [SerializeField] private Button manualButton2;
    [SerializeField] private Button manualButton3;
    
    [Header("Appearance (For Auto-Create Only)")]
    [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private Color buttonHighlight = new Color(0.3f, 0.6f, 0.9f, 1f);
    [SerializeField] private Color textColor = Color.white;
    
    private Canvas canvas;
    private GameObject upgradePanel;
    private TextMeshProUGUI titleText;
    private Button[] upgradeButtons = new Button[3];
    private PlayerUpgradeManager upgradeManager;
    
    private void Awake()
    {
        if (autoCreateUI)
        {
            CreateUI();
        }
        else
        {
            SetupManualReferences();
        }
    }
    
    /// <summary>
    /// Setup UI with reference to upgrade manager
    /// </summary>
    public void SetupWithManager(PlayerUpgradeManager manager)
    {
        upgradeManager = manager;
    }
    
    /// <summary>
    /// Setup references from manually assigned UI elements
    /// </summary>
    private void SetupManualReferences()
    {
        // Use manual references
        canvas = manualCanvas;
        upgradePanel = manualUpgradePanel;
        titleText = manualTitleText;
        
        upgradeButtons[0] = manualButton1;
        upgradeButtons[1] = manualButton2;
        upgradeButtons[2] = manualButton3;
        
        // Validate references
        
        // Setup button listeners
        for (int i = 0; i < 3; i++)
        {
            if (upgradeButtons[i] != null)
            {
                int index = i; // Capture for lambda
                upgradeButtons[i].onClick.RemoveAllListeners(); // Clear any existing listeners
                upgradeButtons[i].onClick.AddListener(() => OnUpgradeButtonClicked(index));
            }
        }
        
        // Hide panel initially
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Create all UI elements programmatically (auto-create mode)
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
        
        // Create 3 upgrade buttons
        for (int i = 0; i < 3; i++)
        {
            int index = i; // Capture for lambda
            upgradeButtons[i] = CreateButton($"UpgradeButton{i}", new Vector2(0, buttonY - buttonSpacing * i), "UPGRADE", () => OnUpgradeButtonClicked(index));
        }
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
        
        var upgradeOptions = upgradeManager.GetCurrentUpgradeOptions();
        
        for (int i = 0; i < 3; i++)
        {
            if (upgradeButtons[i] != null)
            {
                var text = upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string buttonText = GetUpgradeButtonText(upgradeOptions[i]);
                    
                    // Safety check: If button text is error/unknown, hide the button
                    if (buttonText == "ERROR" || buttonText == "UNKNOWN")
                    {
                        upgradeButtons[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        upgradeButtons[i].gameObject.SetActive(true);
                        text.text = buttonText;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Get the display text for an upgrade type
    /// </summary>
    private string GetUpgradeButtonText(PlayerUpgradeManager.UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case PlayerUpgradeManager.UpgradeType.Damage:
                float currentDamage = upgradeManager.GetCurrentDamage();
                float damageUpgrade = upgradeManager.GetDamageUpgradeAmount();
                return $"⚔ DAMAGE\n<size=24>{currentDamage:F1} → {currentDamage + damageUpgrade:F1}</size>";
                
            case PlayerUpgradeManager.UpgradeType.MaxHealth:
                float currentMaxHealth = upgradeManager.GetCurrentHealth();
                float maxHealthUpgrade = upgradeManager.GetMaxHealthUpgradeAmount();
                return $"❤ MAX HEALTH\n<size=24>{currentMaxHealth:F0} → {currentMaxHealth + maxHealthUpgrade:F0}</size>";
                
            case PlayerUpgradeManager.UpgradeType.Heal:
                return $"✚ HEAL\n<size=24>Restore to Full Health</size>";
                
            case PlayerUpgradeManager.UpgradeType.CriticalChance:
                float currentCritChance = upgradeManager.GetCurrentCriticalChance();
                float critChanceUpgrade = upgradeManager.GetCriticalChanceUpgradeAmount();
                return $"💥 CRIT CHANCE\n<size=24>{currentCritChance:F1}% → {Mathf.Min(currentCritChance + critChanceUpgrade, 100f):F1}%</size>";
                
            case PlayerUpgradeManager.UpgradeType.CriticalDamage:
                float currentCritDamage = upgradeManager.GetCurrentCriticalDamage();
                float critDamageUpgrade = upgradeManager.GetCriticalDamageUpgradeAmount();
                return $"⚡ CRIT DAMAGE\n<size=24>{currentCritDamage:F2}x → {currentCritDamage + critDamageUpgrade:F2}x</size>";
                
            case PlayerUpgradeManager.UpgradeType.Evasion:
                float currentEvasion = upgradeManager.GetCurrentEvasion();
                float evasionUpgrade = upgradeManager.GetEvasionChanceUpgradeAmount();
                return $"🌀 EVASION\n<size=24>{currentEvasion:F1}% → {Mathf.Min(currentEvasion + evasionUpgrade, 100f):F1}%</size>";
                
            case PlayerUpgradeManager.UpgradeType.UnlockCirclingProjectiles:
                return $"🌀 UNLOCK SKILL\n<size=24>Circling Projectiles</size>";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileCount:
                // Safety check: Only show if skill is obtained
                if (upgradeManager.GetCurrentProjectileCount() > 0)
                {
                    int currentCount = upgradeManager.GetCurrentProjectileCount();
                    return $"🌀 ADD PROJECTILE\n<size=24>{currentCount} → {currentCount + 1}</size>";
                }
                return "ERROR";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileDamage:
                // Safety check: Only show if skill is obtained
                if (upgradeManager.GetCurrentProjectileDamage() > 0)
                {
                    float currentSkillDmg = upgradeManager.GetCurrentProjectileDamage();
                    float skillDmgUpgrade = upgradeManager.GetSkillDamageUpgradeAmount();
                    return $"⚔ PROJECTILE DMG\n<size=24>{currentSkillDmg:F1} → {currentSkillDmg + skillDmgUpgrade:F1}</size>";
                }
                return "ERROR";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileRadius:
                // Safety check: Only show if skill is obtained
                if (upgradeManager.GetCurrentProjectileRadius() > 0)
                {
                    float currentRadius = upgradeManager.GetCurrentProjectileRadius();
                    float radiusUpgrade = upgradeManager.GetSkillRadiusUpgradeAmount();
                    return $"📏 ORBIT RADIUS\n<size=24>{currentRadius:F1}m → {currentRadius + radiusUpgrade:F1}m</size>";
                }
                return "ERROR";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileSpeed:
                // Safety check: Only show if skill is obtained
                if (upgradeManager.GetCurrentProjectileSpeed() > 0)
                {
                    float currentSpeed = upgradeManager.GetCurrentProjectileSpeed();
                    float speedUpgrade = upgradeManager.GetSkillSpeedUpgradeAmount();
                    return $"⚡ ORBIT SPEED\n<size=24>{currentSpeed:F0}° → {currentSpeed + speedUpgrade:F0}°/s</size>";
                }
                return "ERROR";
                
            // Player2 specific upgrades
            case PlayerUpgradeManager.UpgradeType.UpgradeBlinkDistance:
                float currentBlinkDist = upgradeManager.GetCurrentBlinkDistance();
                float blinkDistUpgrade = upgradeManager.GetBlinkDistanceUpgradeAmount();
                return $"🎯 BLINK RANGE\n<size=24>{currentBlinkDist:F1}m → {currentBlinkDist + blinkDistUpgrade:F1}m</size>";
                
            case PlayerUpgradeManager.UpgradeType.ReduceBlinkCooldown:
                float currentBlinkCD = upgradeManager.GetCurrentBlinkCooldown();
                float blinkCDReduction = upgradeManager.GetBlinkCooldownReduction();
                float newBlinkCD = Mathf.Max(currentBlinkCD - blinkCDReduction, 0.5f);
                return $"⏱ BLINK COOLDOWN\n<size=24>{currentBlinkCD:F1}s → {newBlinkCD:F1}s</size>";
                
            case PlayerUpgradeManager.UpgradeType.ReduceDashCooldown:
                float currentDashCD = upgradeManager.GetCurrentDashCooldown();
                float dashCDReduction = upgradeManager.GetDashCooldownReduction();
                float newDashCD = Mathf.Max(currentDashCD - dashCDReduction, 0.3f);
                return $"⏱ DASH COOLDOWN\n<size=24>{currentDashCD:F1}s → {newDashCD:F1}s</size>";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeBlinkDashSpeed:
                float currentSpeed2 = upgradeManager.GetCurrentBlinkDashSpeed();
                float speedUpgrade2 = upgradeManager.GetBlinkDashSpeedUpgrade();
                return $"💨 DASH SPEED\n<size=24>{currentSpeed2:F0} → {currentSpeed2 + speedUpgrade2:F0}</size>";
                
            default:
                return "UNKNOWN";
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
    private void OnUpgradeButtonClicked(int buttonIndex)
    {
        if (upgradeManager != null)
        {
            var upgradeOptions = upgradeManager.GetCurrentUpgradeOptions();
            if (buttonIndex >= 0 && buttonIndex < upgradeOptions.Length)
            {
                upgradeManager.ApplyUpgrade(upgradeOptions[buttonIndex]);
            }
        }
    }
}
