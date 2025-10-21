using UnityEngine;
using UnityEngine.UI;

public class Player2_ChargeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player2 player2;
    [SerializeField] private Player2_ChargedDashAttack dashAttack;
    
    [Header("UI Elements - Combined Bar")]
    [SerializeField] private GameObject barContainer;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetup = true;
    [SerializeField] private Vector2 uiSize = new Vector2(200, 8);
    [SerializeField] private Vector2 uiOffset = new Vector2(20, 20); // Offset from bottom-left corner
    
    [Header("Visual Settings")]
    [SerializeField] private Color chargingColor = new Color(1f, 0.8f, 0f, 0.9f); // Yellow - charging
    [SerializeField] private Color fullChargeColor = new Color(1f, 0.3f, 0f, 1f); // Bright Orange - full charge
    [SerializeField] private Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // Gray - on cooldown
    [SerializeField] private Color readyColor = new Color(0.3f, 1f, 0.3f, 0.9f); // Green - ready
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.5f); // Dark background
    [SerializeField] private AnimationCurve fillCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Canvas canvas;
    private bool isFullyCharged = false;
    
    private void Awake()
    {
        // Auto-find components if not assigned
        if (player2 == null)
            player2 = GetComponentInParent<Player2>();
        
        // If this is not Player2 or Player2 is not active, disable this component
        if (player2 == null || !IsPlayer2Active())
        {
            Debug.Log($"[Player2_ChargeUI] Disabling on {gameObject.name} - Not Player2 or Player2 not active");
            enabled = false;
            return;
        }
        
        if (dashAttack == null && player2 != null)
            dashAttack = player2.dashAttack;
    }
    
    private void Start()
    {
        // Double-check in Start in case character selection happened after Awake
        if (!enabled || player2 == null || !IsPlayer2Active())
        {
            // Disable component and don't setup UI
            enabled = false;
            return;
        }
        
        if (autoSetup)
        {
            SetupUI();
        }
        
        if (barContainer != null)
        {
            barContainer.SetActive(true);
        }
    }
    
    private void Update()
    {
        if (dashAttack == null) return;
        
        // Update the single bar based on current state
        UpdateBar();
    }
    
    private void UpdateBar()
    {
        if (fillImage == null) return;
        
        // Priority: Charging > Cooldown > Ready
        if (dashAttack.IsCharging)
        {
            // Show charge progress
            float progress = dashAttack.ChargeProgress;
            float curvedProgress = fillCurve.Evaluate(progress);
            fillImage.fillAmount = curvedProgress;
            
            // Change color when fully charged
            bool nowFullyCharged = progress >= 1f;
            if (nowFullyCharged != isFullyCharged)
            {
                isFullyCharged = nowFullyCharged;
            }
            
            // Pulse effect when fully charged
            if (isFullyCharged)
            {
                float pulse = Mathf.PingPong(Time.time * 3f, 1f);
                fillImage.color = Color.Lerp(chargingColor, fullChargeColor, pulse);
            }
            else
            {
                fillImage.color = Color.Lerp(chargingColor, fullChargeColor, progress);
            }
        }
        else if (dashAttack.IsOnCooldown)
        {
            // Show cooldown progress
            float progress = dashAttack.CooldownProgress;
            fillImage.fillAmount = progress;
            fillImage.color = Color.Lerp(cooldownColor, readyColor, progress);
            isFullyCharged = false;
        }
        else
        {
            // Ready to use
            fillImage.fillAmount = 1f;
            fillImage.color = readyColor;
            isFullyCharged = false;
        }
    }
    
    private void SetupUI()
    {
        // Find or create canvas
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Player2_ChargeCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Create single combined bar
        CreateBar();
        
        Debug.Log("[Player2_ChargeUI] Auto-setup complete with dash bar in bottom-left!");
    }
    
    private void CreateBar()
    {
        // Create bar container in bottom-left corner
        GameObject containerObj = new GameObject("DashBarContainer");
        containerObj.transform.SetParent(canvas.transform, false);
        barContainer = containerObj;
        
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        // Anchor to bottom-left
        containerRect.anchorMin = new Vector2(0f, 0f);
        containerRect.anchorMax = new Vector2(0f, 0f);
        containerRect.pivot = new Vector2(0f, 0f);
        containerRect.sizeDelta = uiSize;
        containerRect.anchoredPosition = uiOffset;
        
        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(containerObj.transform, false);
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        backgroundImage = bgObj.AddComponent<Image>();
        backgroundImage.color = backgroundColor;
        
        // Create fill image
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(containerObj.transform, false);
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = readyColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f; // Start full (ready)
    }
    
    /// <summary>
    /// Check if Player2 is the active character
    /// </summary>
    private bool IsPlayer2Active()
    {
        // Check CharacterSelectionManager
        if (CharacterSelectionManager.Instance != null)
        {
            return CharacterSelectionManager.Instance.SelectedCharacterIndex == 1;
        }
        
        // Fallback: check if gameObject is active and is Player2
        return gameObject.activeInHierarchy && player2 != null;
    }
    
    // Public method to manually setup if needed
    public void ManualSetup()
    {
        if (!IsPlayer2Active())
        {
            Debug.LogWarning("[Player2_ChargeUI] Cannot manually setup - Player2 is not active");
            return;
        }
        SetupUI();
    }
}
