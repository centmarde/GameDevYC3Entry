using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player_RangeAttackController : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private bool autoCreateUI = true;
    [SerializeField] private TextMeshProUGUI attackLabelText; // For manual setup

    [Header("UI Appearance")]
    [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Vector2 panelSize = new Vector2(200f, 60f);
    [SerializeField] private Vector2 panelOffset = new Vector2(-20f, 20f); // Offset from bottom-right corner

    private Canvas canvas;
    private GameObject uiPanel;
    private List<Player_RangeAttack> attackVariants = new List<Player_RangeAttack>();
    private int currentIndex = 0;

    // Optional: cooldown between scroll switches (prevents rapid cycling)
    [SerializeField] private float switchCooldown = 0.2f;
    private float lastSwitchTime = 0f;

    public Player_RangeAttack CurrentAttack =>
        attackVariants.Count > 0 ? attackVariants[currentIndex] : null;

    private void Awake()
    {
        // Automatically find all attached attack types
        attackVariants.AddRange(GetComponents<Player_RangeAttack>());
        
        // Sort alphabetically for consistent order
        attackVariants.Sort((a, b) => 
        {
            return a.GetType().Name.CompareTo(b.GetType().Name);
        });
        
        // Set default to first attack
        currentIndex = 0;

        if (autoCreateUI)
        {
            CreateUI();
        }
    }

    private void Start()
    {
        // Ensure UI is initialized even if created in Awake
        if (attackLabelText != null)
        {
            UpdateAttackLabel();
        }
        else if (autoCreateUI)
        {
            Debug.LogWarning("[RangeAttackController] UI was not created properly in Awake!");
        }
    }

    //  Called from Input System (bind this to your Scroll action)
    public void OnScroll(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || attackVariants.Count <= 1) return;

        Vector2 scroll = ctx.ReadValue<Vector2>();
        float scrollY = scroll.y;

        // Apply a small cooldown to prevent over-scrolling
        if (Time.time - lastSwitchTime < switchCooldown)
            return;

        if (scrollY > 0f)
            CyclePrevious();
        else if (scrollY < 0f)
            CycleNext();

        lastSwitchTime = Time.time;
    }


    public void OnSwitchAttackType(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        CycleNext();
    }

    private void CycleNext()
    {
        currentIndex = (currentIndex + 1) % attackVariants.Count;
        UpdateAttackLabel();
    }

    private void CyclePrevious()
    {
        currentIndex = (currentIndex - 1 + attackVariants.Count) % attackVariants.Count;
        UpdateAttackLabel();
    }

    private void UpdateAttackLabel()
    {
        if (attackLabelText && CurrentAttack != null)
        {
            string attackName = GetReadableAttackName(CurrentAttack);
            attackLabelText.text = $"{attackName}";
        }
    }

    private string GetReadableAttackName(Player_RangeAttack attack)
    {
        if (attack.GetType() == typeof(Player_RangeAttack))
            return "Normal Shot";
        else if (attack is Player_ChargedRangeAttack)
            return "Charged Shot";
        else if (attack is Player_ScatterRangeAttack)
            return "Scatter Shot";
        else
            return attack.GetType().Name.Replace("Player_", "").Replace("RangeAttack", "");
    }

    private void CreateUI()
    {
        // Always create a new dedicated canvas for attack type UI
        GameObject canvasObj = new GameObject("AttackTypeCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Ensure it's on top of other UI
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create panel in bottom-right corner
        uiPanel = new GameObject("AttackTypePanel");
        uiPanel.transform.SetParent(canvas.transform, false);

        Image panelImage = uiPanel.AddComponent<Image>();
        panelImage.color = panelColor;

        RectTransform panelRect = uiPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = panelSize;
        panelRect.anchorMin = new Vector2(1f, 0f); // Bottom-right
        panelRect.anchorMax = new Vector2(1f, 0f); // Bottom-right
        panelRect.pivot = new Vector2(1f, 0f); // Bottom-right
        panelRect.anchoredPosition = panelOffset;

        // Create text label
        GameObject textObj = new GameObject("AttackLabel");
        textObj.transform.SetParent(uiPanel.transform, false);

        attackLabelText = textObj.AddComponent<TextMeshProUGUI>();
        attackLabelText.text = "Normal Shot";
        attackLabelText.color = textColor;
        attackLabelText.fontSize = 18;
        attackLabelText.alignment = TextAlignmentOptions.Center;
        attackLabelText.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 10f);
        textRect.offsetMax = new Vector2(-10f, -10f);
        
        Debug.Log($"[RangeAttackController] UI Created - Attack Count: {attackVariants.Count}, Current: {CurrentAttack?.GetType().Name}");
    }
}
