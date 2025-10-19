using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimalist UI Controller for Slow Motion skill - Single Line Indicator
/// Ultra-clean horizontal line that shows/hides based on skill state
/// Perfect for immersive, cinematic gameplay
/// </summary>
public class SlowMotionUI_Line : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerSkill_Manager skillManager;
    [SerializeField] private Image lineIndicator;
    
    [Header("Visual Settings")]
    [SerializeField] private float lineHeight = 3f;
    [SerializeField] private float fadeSpeed = 3f;
    
    [Header("Colors")]
    [SerializeField] private Color activeColor = new Color(0f, 1f, 1f, 0.8f);    // Cyan when active
    [SerializeField] private Color cooldownColor = new Color(1f, 1f, 1f, 0.3f);  // White faded during cooldown
    [SerializeField] private Color hiddenColor = new Color(1f, 1f, 1f, 0f);      // Transparent when ready
    
    [Header("Behavior")]
    [SerializeField] private bool hideWhenReady = true;      // Hide line when skill is ready
    [SerializeField] private bool showDuringCooldown = true; // Show cooldown progress
    
    private Color targetColor;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        rectTransform = lineIndicator?.GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            // Set line height
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, lineHeight);
        }
    }
    
    private void Start()
    {
        // Auto-find skill manager
        if (skillManager == null)
        {
            skillManager = FindObjectOfType<PlayerSkill_Manager>();
        }
        
        // Ensure image type is set to Filled
        if (lineIndicator != null && lineIndicator.type != Image.Type.Filled)
        {
            lineIndicator.type = Image.Type.Filled;
            lineIndicator.fillMethod = Image.FillMethod.Horizontal;
            lineIndicator.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
        
        targetColor = hiddenColor;
    }
    
    private void Update()
    {
        if (skillManager == null || lineIndicator == null) return;
        
        UpdateLineIndicator();
        
        // Smooth color fade
        lineIndicator.color = Color.Lerp(lineIndicator.color, targetColor, Time.deltaTime * fadeSpeed);
    }
    
    private void UpdateLineIndicator()
    {
        if (skillManager.IsSlowMotionActive())
        {
            // Show duration as fill amount (depleting)
            float fill = skillManager.GetRemainingDurationPercent();
            lineIndicator.fillAmount = fill;
            targetColor = activeColor;
        }
        else if (showDuringCooldown && skillManager.IsOnCooldown())
        {
            // Show cooldown progress (filling up)
            float fill = 1f - skillManager.GetCooldownPercent();
            lineIndicator.fillAmount = fill;
            targetColor = cooldownColor;
        }
        else
        {
            // Hide or show full when ready
            if (hideWhenReady)
            {
                targetColor = hiddenColor;
            }
            else
            {
                lineIndicator.fillAmount = 1f;
                targetColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.3f);
            }
        }
    }
}
