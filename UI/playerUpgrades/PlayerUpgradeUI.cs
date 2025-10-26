using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Minimalist UI for player stat upgrades after each wave
/// Supports both auto-creation and manual setup of UI elements
/// Includes hover tooltip system for detailed upgrade information
/// Refactored into modular components for better maintainability
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
    
    [Header("Sprite Management")]
    [SerializeField] private UpgradeSpriteManager spriteManager = new UpgradeSpriteManager();
    
    [Header("Tooltip Settings")]
    [SerializeField] private bool enableTooltips = true;
    [SerializeField] private Color tooltipBackgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.98f);
    [SerializeField] private Color tooltipTextColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip upgradeUISound;
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 0.7f;
    
    [Header("Confirmation Dialog")]
    [SerializeField] private Sprite confirmationDialogBackground;
    [Tooltip("Optional background image for the confirmation dialog. If not set, uses solid color.")]
    [SerializeField] private Sprite confirmationConfirmButtonSprite;
    [Tooltip("Optional sprite for the Confirm button. If not set, uses solid color.")]
    [SerializeField] private Sprite confirmationCancelButtonSprite;
    [Tooltip("Optional sprite for the Cancel button. If not set, uses solid color.")]
    
    private Canvas canvas;
    private AudioSource audioSource;
    private GameObject upgradePanel;
    private TextMeshProUGUI titleText;
    private UpgradeButton[] upgradeButtons = new UpgradeButton[3];
    private PlayerUpgradeManager upgradeManager;
    private UpgradeTooltip tooltip;
    private UpgradeConfirmationDialog confirmationDialog;
    private PlayerUpgradeData.UpgradeType pendingUpgradeType;
    
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
        
        // Setup audio source
        SetupAudioSource();
    }
    
    /// <summary>
    /// Setup UI with reference to upgrade manager
    /// </summary>
    public void SetupWithManager(PlayerUpgradeManager manager)
    {
        upgradeManager = manager;
    }
    
    /// <summary>
    /// Setup audio source for sound effects
    /// </summary>
    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D UI sound
        }
    }
    
    /// <summary>
    /// Play the upgrade UI sound effect
    /// </summary>
    private void PlayUpgradeUISound()
    {
        if (upgradeUISound != null && audioSource != null)
        {
            audioSource.PlayOneShot(upgradeUISound, soundVolume);
        }
    }
    
    /// <summary>
    /// Setup references from manually assigned UI elements
    /// </summary>
    private void SetupManualReferences()
    {
        canvas = manualCanvas;
        
        // If manual canvas is not assigned, try to find one or create it
        if (canvas == null)
        {
            Debug.LogWarning("PlayerUpgradeUI: Manual Canvas not assigned. Searching for existing Canvas...");
            canvas = FindObjectOfType<Canvas>();
            
            if (canvas == null)
            {
                Debug.LogWarning("PlayerUpgradeUI: No Canvas found. Creating one automatically...");
                canvas = UpgradeUIFactory.CreateCanvas(transform);
            }
        }
        
        upgradePanel = manualUpgradePanel;
        titleText = manualTitleText;
        
        // Convert manual buttons to UpgradeButton components
        Button[] manualButtons = { manualButton1, manualButton2, manualButton3 };
        for (int i = 0; i < 3; i++)
        {
            if (manualButtons[i] != null)
            {
                upgradeButtons[i] = manualButtons[i].gameObject.GetComponent<UpgradeButton>();
                if (upgradeButtons[i] == null)
                {
                    upgradeButtons[i] = manualButtons[i].gameObject.AddComponent<UpgradeButton>();
                }
                upgradeButtons[i].Initialize(i);
                int index = i; // Capture loop variable
                upgradeButtons[i].AddClickListener(() => OnUpgradeButtonClicked(index));
            }
        }
        
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        
        if (enableTooltips)
        {
            SetupTooltip();
            for (int i = 0; i < 3; i++)
            {
                if (upgradeButtons[i] != null)
                {
                    upgradeButtons[i].AddHoverDetection(OnButtonHoverEnter, OnButtonHoverExit);
                }
            }
        }
        
        SetupConfirmationDialog();
    }
    
    /// <summary>
    /// Create all UI elements programmatically (auto-create mode)
    /// </summary>
    private void CreateUI()
    {
        canvas = UpgradeUIFactory.CreateCanvas(transform);
        upgradePanel = UpgradeUIFactory.CreateUpgradePanel(canvas.transform, panelColor);
        titleText = UpgradeUIFactory.CreateTitle(upgradePanel.transform, textColor);
        CreateUpgradeButtons();
        
        if (enableTooltips)
        {
            SetupTooltip();
        }
        
        SetupConfirmationDialog();
        
        upgradePanel.SetActive(false);
    }
    
    /// <summary>
    /// Create upgrade buttons
    /// </summary>
    private void CreateUpgradeButtons()
    {
        float buttonY = -120;
        float buttonSpacing = 90;
        
        for (int i = 0; i < 3; i++)
        {
            Vector2 position = new Vector2(0, buttonY - buttonSpacing * i);
            upgradeButtons[i] = UpgradeUIFactory.CreateUpgradeButton(
                upgradePanel.transform, 
                $"UpgradeButton{i}", 
                position, 
                buttonColor, 
                buttonHighlight, 
                textColor
            );
            
            upgradeButtons[i].Initialize(i);
            int index = i; // Capture loop variable
            upgradeButtons[i].AddClickListener(() => OnUpgradeButtonClicked(index));
            
            if (enableTooltips)
            {
                upgradeButtons[i].AddHoverDetection(OnButtonHoverEnter, OnButtonHoverExit);
            }
        }
    }
    
    /// <summary>
    /// Setup tooltip system
    /// </summary>
    private void SetupTooltip()
    {
        GameObject tooltipObj = new GameObject("TooltipManager");
        tooltipObj.transform.SetParent(transform, false);
        tooltip = tooltipObj.AddComponent<UpgradeTooltip>();
        tooltip.Create(canvas.transform, tooltipBackgroundColor, tooltipTextColor);
    }
    
    /// <summary>
    /// Setup confirmation dialog system
    /// </summary>
    private void SetupConfirmationDialog()
    {
        // Ensure canvas exists
        if (canvas == null)
        {
            Debug.LogError("PlayerUpgradeUI: Canvas is null! Cannot create confirmation dialog. Make sure to assign Manual Canvas in Inspector if using manual setup.");
            return;
        }
        
        GameObject dialogObj = new GameObject("ConfirmationDialog");
        dialogObj.transform.SetParent(transform, false);
        confirmationDialog = dialogObj.AddComponent<UpgradeConfirmationDialog>();
        
        // Set sprites before creating if available
        if (confirmationDialogBackground != null)
        {
            confirmationDialog.SetBackgroundSprite(confirmationDialogBackground);
        }
        if (confirmationConfirmButtonSprite != null)
        {
            confirmationDialog.SetConfirmButtonSprite(confirmationConfirmButtonSprite);
        }
        if (confirmationCancelButtonSprite != null)
        {
            confirmationDialog.SetCancelButtonSprite(confirmationCancelButtonSprite);
        }
        
        confirmationDialog.Create(canvas.transform, panelColor, buttonColor, buttonHighlight, textColor);
        confirmationDialog.OnConfirm += OnUpgradeConfirmed;
        confirmationDialog.OnCancel += OnUpgradeCancelled;
    }
    
    /// <summary>
    /// Handle button hover enter
    /// </summary>
    private void OnButtonHoverEnter(int buttonIndex)
    {
        if (upgradeManager == null || !enableTooltips || tooltip == null) return;
        
        var upgradeOptions = upgradeManager.GetCurrentUpgradeOptions();
        if (buttonIndex >= 0 && buttonIndex < upgradeOptions.Length && buttonIndex < upgradeButtons.Length)
        {
            string tooltipText = UpgradeTextProvider.GetTooltipText(upgradeOptions[buttonIndex]);
            RectTransform buttonRect = upgradeButtons[buttonIndex]?.RectTransform;
            tooltip.Show(tooltipText, buttonRect);
        }
    }
    
    /// <summary>
    /// Handle button hover exit
    /// </summary>
    private void OnButtonHoverExit()
    {
        if (!enableTooltips || tooltip == null) return;
        tooltip.Hide();
    }
    
    /// <summary>
    /// Update tooltip fade animation
    /// </summary>
    private void Update()
    {
        if (tooltip != null)
        {
            tooltip.UpdateFade();
        }
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
                string buttonText = UpgradeTextProvider.GetButtonText(upgradeOptions[i], upgradeManager);
                
                // Safety check: If button text is error/unknown, hide the button
                if (buttonText == "ERROR" || buttonText == "UNKNOWN")
                {
                    upgradeButtons[i].gameObject.SetActive(false);
                }
                else
                {
                    upgradeButtons[i].gameObject.SetActive(true);
                    upgradeButtons[i].SetText(buttonText);
                    
                    // Update button background image
                    Sprite sprite = spriteManager.GetSprite(upgradeOptions[i]);
                    upgradeButtons[i].SetSprite(sprite, buttonColor, buttonHighlight, new Color(0.5f, 0.7f, 1f, 1f));
                }
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
            PlayUpgradeUISound(); // Play sound when UI appears
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
        
        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }
   
    /// <summary>
    /// Handle upgrade button click - shows confirmation dialog
    /// </summary>
    private void OnUpgradeButtonClicked(int buttonIndex)
    {
        if (upgradeManager == null)
        {
            Debug.LogError("PlayerUpgradeUI: UpgradeManager is null!");
            return;
        }
        
        if (confirmationDialog == null)
        {
            Debug.LogError("PlayerUpgradeUI: ConfirmationDialog is null!");
            return;
        }
        
        var upgradeOptions = upgradeManager.GetCurrentUpgradeOptions();
        if (buttonIndex >= 0 && buttonIndex < upgradeOptions.Length)
        {
            pendingUpgradeType = upgradeOptions[buttonIndex];
            string confirmationText = GetConfirmationText(pendingUpgradeType);
            confirmationDialog.Show(confirmationText);
            
            // Hide tooltip when confirmation shows
            if (tooltip != null)
            {
                tooltip.Hide();
            }
        }
    }
    
    /// <summary>
    /// Get confirmation message for upgrade type
    /// </summary>
    private string GetConfirmationText(PlayerUpgradeData.UpgradeType upgradeType)
    {
        string upgradeName = UpgradeTextProvider.GetButtonText(upgradeType, upgradeManager).Split('\n')[0];
        return $"Confirm {upgradeName} upgrade?";
    }
    
    /// <summary>
    /// Handle upgrade confirmation
    /// </summary>
    private void OnUpgradeConfirmed()
    {
        if (upgradeManager != null)
        {
            upgradeManager.ApplyUpgrade(pendingUpgradeType);
        }
    }
    
    /// <summary>
    /// Handle upgrade cancellation
    /// </summary>
    private void OnUpgradeCancelled()
    {
        // Just close the dialog, player can choose again
    }
}
