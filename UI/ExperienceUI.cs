using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for displaying experience bar and level.
/// Auto-creates UI if not manually set up in the scene.
/// </summary>
public class ExperienceUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private GameObject levelUpEffect;

    [Header("Settings")]
    [SerializeField] private Color fillColor = new Color(0.3f, 0.8f, 1f); // Cyan
    [SerializeField] private Color levelUpColor = Color.yellow;
    [SerializeField] private float levelUpEffectDuration = 1f;

    private Canvas canvas;
    private RectTransform barBackground;
    private bool isLevelUpAnimating = false;

    private void Awake()
    {
        // If UI elements are not assigned, auto-create them
        if (fillBar == null || levelText == null)
        {
            AutoSetupUI();
        }
    }

    /// <summary>
    /// Automatically create the experience bar UI at the top of the screen
    /// </summary>
    private void AutoSetupUI()
    {
        Debug.Log("[ExperienceUI] Auto-setting up UI elements...");

        // Find or create Canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
            {
                canvas = canvasObj.GetComponent<Canvas>();
            }

            if (canvas == null)
            {
                // Create new canvas
                canvasObj = new GameObject("ExperienceCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("[ExperienceUI] Created new Canvas");
            }
        }

        // Create experience bar container
        GameObject barContainer = new GameObject("ExperienceBarContainer");
        barContainer.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = barContainer.AddComponent<RectTransform>();
        
        // Position at top of screen, stretched horizontally
        containerRect.anchorMin = new Vector2(0, 1); // Top-left
        containerRect.anchorMax = new Vector2(1, 1); // Top-right
        containerRect.pivot = new Vector2(0.5f, 1);
        containerRect.anchoredPosition = new Vector2(0, -10); // 10 pixels from top
        containerRect.sizeDelta = new Vector2(-40, 30); // 20 pixel margin on each side, 30 pixels tall

        // Create background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(barContainer.transform, false);
        
        RectTransform bgRect = backgroundObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = backgroundObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark semi-transparent background

        barBackground = bgRect;

        // Create fill bar
        GameObject fillObj = new GameObject("FillBar");
        fillObj.transform.SetParent(backgroundObj.transform, false);
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1); // Will grow from left
        fillRect.pivot = Vector2.zero;
        fillRect.offsetMin = new Vector2(2, 2); // 2 pixel padding
        fillRect.offsetMax = new Vector2(-2, -2);

        fillBar = fillObj.AddComponent<Image>();
        fillBar.color = fillColor;
        fillBar.type = Image.Type.Filled;
        fillBar.fillMethod = Image.FillMethod.Horizontal;
        fillBar.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillBar.fillAmount = 0f; // Start empty
        
        Debug.Log("[ExperienceUI] Created fill bar - Initial fillAmount: 0");

        // Create level text
        GameObject levelTextObj = new GameObject("LevelText");
        levelTextObj.transform.SetParent(barContainer.transform, false);
        
        RectTransform levelRect = levelTextObj.AddComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0, 0.5f);
        levelRect.anchorMax = new Vector2(0, 0.5f);
        levelRect.pivot = new Vector2(0, 0.5f);
        levelRect.anchoredPosition = new Vector2(10, 0);
        levelRect.sizeDelta = new Vector2(100, 30);

        levelText = levelTextObj.AddComponent<TextMeshProUGUI>();
        levelText.text = "Level 1";
        levelText.fontSize = 18;
        levelText.fontStyle = FontStyles.Bold;
        levelText.color = Color.white;
        levelText.alignment = TextAlignmentOptions.Left;

        // Create experience text
        GameObject expTextObj = new GameObject("ExperienceText");
        expTextObj.transform.SetParent(barContainer.transform, false);
        
        RectTransform expRect = expTextObj.AddComponent<RectTransform>();
        expRect.anchorMin = new Vector2(0.5f, 0.5f);
        expRect.anchorMax = new Vector2(0.5f, 0.5f);
        expRect.pivot = new Vector2(0.5f, 0.5f);
        expRect.anchoredPosition = Vector2.zero;
        expRect.sizeDelta = new Vector2(200, 30);

        experienceText = expTextObj.AddComponent<TextMeshProUGUI>();
        experienceText.text = "0 / 100";
        experienceText.fontSize = 16;
        experienceText.color = Color.white;
        experienceText.alignment = TextAlignmentOptions.Center;

        Debug.Log("[ExperienceUI] UI auto-setup complete!");
    }

    /// <summary>
    /// Update the experience bar display
    /// </summary>
    public void UpdateExperienceBar(int currentExp, int requiredExp, int level)
    {
        if (fillBar != null)
        {
            float fillAmount = requiredExp > 0 ? (float)currentExp / requiredExp : 0f;
            fillBar.fillAmount = Mathf.Clamp01(fillAmount);
            
            // Ensure the fill bar is set to Filled type and Horizontal fill
            if (fillBar.type != Image.Type.Filled)
            {
                fillBar.type = Image.Type.Filled;
                fillBar.fillMethod = Image.FillMethod.Horizontal;
                fillBar.fillOrigin = (int)Image.OriginHorizontal.Left;
            }
        }

        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }

        if (experienceText != null)
        {
            experienceText.text = $"{currentExp} / {requiredExp}";
        }
    }

    /// <summary>
    /// Show visual effect when player levels up
    /// </summary>
    public void ShowLevelUpEffect()
    {
        if (isLevelUpAnimating) return;
        
        StartCoroutine(LevelUpAnimation());
    }

    private System.Collections.IEnumerator LevelUpAnimation()
    {
        isLevelUpAnimating = true;

        // Store original colors
        Color originalFillColor = fillBar != null ? fillBar.color : fillColor;
        Color originalTextColor = levelText != null ? levelText.color : Color.white;

        float elapsed = 0f;

        // Flash effect
        while (elapsed < levelUpEffectDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / levelUpEffectDuration;

            // Ping-pong flash
            Color flashColor = Color.Lerp(originalFillColor, levelUpColor, Mathf.PingPong(t * 4, 1));

            if (fillBar != null)
            {
                fillBar.color = flashColor;
            }

            if (levelText != null)
            {
                levelText.color = Color.Lerp(originalTextColor, levelUpColor, Mathf.PingPong(t * 4, 1));
            }

            yield return null;
        }

        // Restore original colors
        if (fillBar != null)
        {
            fillBar.color = originalFillColor;
        }

        if (levelText != null)
        {
            levelText.color = originalTextColor;
        }

        isLevelUpAnimating = false;
    }

    /// <summary>
    /// Set the fill color of the experience bar
    /// </summary>
    public void SetFillColor(Color color)
    {
        fillColor = color;
        if (fillBar != null)
        {
            fillBar.color = color;
        }
    }
}
