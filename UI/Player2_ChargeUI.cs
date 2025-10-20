using UnityEngine;
using UnityEngine.UI;

public class Player2_ChargeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player2 player2;
    [SerializeField] private Player2_ChargedDashAttack dashAttack;
    
    [Header("UI Elements - Charge Bar")]
    [SerializeField] private GameObject chargeBarContainer;
    [SerializeField] private Image chargeFillImage;
    [SerializeField] private Image chargeBackgroundImage;
    
    [Header("UI Elements - Cooldown Bar")]
    [SerializeField] private GameObject cooldownBarContainer;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private Image cooldownBackgroundImage;
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetup = true;
    [SerializeField] private Vector2 chargeUIPosition = new Vector2(0, -80);
    [SerializeField] private Vector2 cooldownUIPosition = new Vector2(0, -95);
    [SerializeField] private Vector2 chargeUISize = new Vector2(150, 6);
    [SerializeField] private Vector2 cooldownUISize = new Vector2(150, 4);
    
    [Header("Visual Settings - Charge")]
    [SerializeField] private Color chargingColor = new Color(1f, 1f, 1f, 0.9f); // White
    [SerializeField] private Color fullChargeColor = new Color(1f, 0.3f, 0f, 1f); // Bright Orange
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.4f); // Subtle dark
    [SerializeField] private AnimationCurve fillCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Settings - Cooldown")]
    [SerializeField] private Color cooldownReadyColor = new Color(1f, 1f, 1f, 0.8f); // White/Ready
    [SerializeField] private Color cooldownNotReadyColor = new Color(0.3f, 0.3f, 0.3f, 0.6f); // Dark gray
    [SerializeField] private Color cooldownBackgroundColor = new Color(0f, 0f, 0f, 0.3f); // Very subtle
    
    private Canvas canvas;
    private bool isFullyCharged = false;
    
    private void Awake()
    {
        // Auto-find components if not assigned
        if (player2 == null)
            player2 = GetComponentInParent<Player2>();
        
        if (dashAttack == null && player2 != null)
            dashAttack = player2.dashAttack;
    }
    
    private void Start()
    {
        if (autoSetup)
        {
            SetupUI();
        }
        
        if (chargeBarContainer != null)
        {
            chargeBarContainer.SetActive(false);
        }
        
        if (cooldownBarContainer != null)
        {
            cooldownBarContainer.SetActive(true);
        }
    }
    
    private void Update()
    {
        if (dashAttack == null) return;
        
        bool shouldShowCharge = dashAttack.IsCharging;
        
        if (chargeBarContainer != null)
        {
            chargeBarContainer.SetActive(shouldShowCharge);
        }
        
        if (shouldShowCharge)
        {
            UpdateChargeBar();
        }
        
        // Always update cooldown bar
        UpdateCooldownBar();
    }
    
    private void UpdateChargeBar()
    {
        float progress = dashAttack.ChargeProgress;
        
        // Apply curve to make charge feel more satisfying
        float curvedProgress = fillCurve.Evaluate(progress);
        
        if (chargeFillImage != null)
        {
            chargeFillImage.fillAmount = curvedProgress;
            
            // Change color when fully charged
            bool nowFullyCharged = progress >= 1f;
            if (nowFullyCharged != isFullyCharged)
            {
                isFullyCharged = nowFullyCharged;
                if (isFullyCharged)
                {
                    // Flash effect when full
                    chargeFillImage.color = fullChargeColor;
                }
            }
            
            // Pulse effect when fully charged
            if (isFullyCharged)
            {
                float pulse = Mathf.PingPong(Time.time * 3f, 1f);
                chargeFillImage.color = Color.Lerp(chargingColor, fullChargeColor, pulse);
            }
            else
            {
                chargeFillImage.color = Color.Lerp(chargingColor, fullChargeColor, progress);
            }
        }
    }
    
    private void UpdateCooldownBar()
    {
        if (cooldownFillImage == null) return;
        
        float progress = dashAttack.CooldownProgress;
        cooldownFillImage.fillAmount = progress;
        
        // Change color based on ready state
        if (dashAttack.IsOnCooldown)
        {
            // Cooldown in progress - gray transitioning to green
            cooldownFillImage.color = Color.Lerp(cooldownNotReadyColor, cooldownReadyColor, progress);
        }
        else
        {
            // Ready to use - solid green
            cooldownFillImage.color = cooldownReadyColor;
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
        
        // Create charge bar
        CreateChargeBar();
        
        // Create cooldown bar
        CreateCooldownBar();
        
        Debug.Log("[Player2_ChargeUI] Auto-setup complete with charge and cooldown bars!");
    }
    
    private void CreateChargeBar()
    {
        // Create charge bar container
        GameObject containerObj = new GameObject("ChargeBarContainer");
        containerObj.transform.SetParent(canvas.transform, false);
        chargeBarContainer = containerObj;
        
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = chargeUISize;
        containerRect.anchoredPosition = chargeUIPosition;
        
        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(containerObj.transform, false);
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        chargeBackgroundImage = bgObj.AddComponent<Image>();
        chargeBackgroundImage.color = backgroundColor;
        
        // Create fill image
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(containerObj.transform, false);
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        chargeFillImage = fillObj.AddComponent<Image>();
        chargeFillImage.color = chargingColor;
        chargeFillImage.type = Image.Type.Filled;
        chargeFillImage.fillMethod = Image.FillMethod.Horizontal;
        chargeFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        chargeFillImage.fillAmount = 0f;
    }
    
    private void CreateCooldownBar()
    {
        // Create cooldown bar container
        GameObject containerObj = new GameObject("CooldownBarContainer");
        containerObj.transform.SetParent(canvas.transform, false);
        cooldownBarContainer = containerObj;
        
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = cooldownUISize;
        containerRect.anchoredPosition = cooldownUIPosition;
        
        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(containerObj.transform, false);
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        cooldownBackgroundImage = bgObj.AddComponent<Image>();
        cooldownBackgroundImage.color = cooldownBackgroundColor;
        
        // Create fill image
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(containerObj.transform, false);
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        cooldownFillImage = fillObj.AddComponent<Image>();
        cooldownFillImage.color = cooldownReadyColor;
        cooldownFillImage.type = Image.Type.Filled;
        cooldownFillImage.fillMethod = Image.FillMethod.Horizontal;
        cooldownFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        cooldownFillImage.fillAmount = 1f; // Start full (ready)
    }
    
    // Public method to manually setup if needed
    public void ManualSetup()
    {
        SetupUI();
    }
}
