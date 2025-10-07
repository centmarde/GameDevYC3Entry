using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Controller for the Slow Motion skill display
/// Shows duration bar, cooldown, and activation feedback
/// </summary>
public class SlowMotionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerSkill_Manager skillManager;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject slowMotionPanel;           // Main container
    [SerializeField] private Image durationFillBar;                // Fill bar showing remaining duration
    [SerializeField] private Image cooldownFillBar;                // Fill bar showing cooldown progress
    [SerializeField] private TextMeshProUGUI keyDisplayText;       // Shows which key to press (e.g., "X")
    [SerializeField] private TextMeshProUGUI statusText;           // Shows "READY", "ACTIVE", or cooldown time
    [SerializeField] private Image iconImage;                      // Icon for the skill
    
    [Header("Visual Feedback")]
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color activeColor = Color.cyan;
    [SerializeField] private Color cooldownColor = Color.gray;
    [SerializeField] private Color durationBarColor = new Color(0, 1, 1, 0.8f); // Cyan
    
    [Header("Animation")]
    [SerializeField] private bool pulseWhenActive = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseScale = 1.2f;
    
    private Vector3 originalScale;
    private CanvasGroup panelCanvasGroup;

    private void Awake()
    {
        // Auto-find skill manager if not assigned
        if (skillManager == null)
        {
            skillManager = FindObjectOfType<PlayerSkill_Manager>();
        }

        // Get or add canvas group for fade effects
        panelCanvasGroup = slowMotionPanel?.GetComponent<CanvasGroup>();
        if (slowMotionPanel != null && panelCanvasGroup == null)
        {
            panelCanvasGroup = slowMotionPanel.AddComponent<CanvasGroup>();
        }

        if (slowMotionPanel != null)
        {
            originalScale = slowMotionPanel.transform.localScale;
        }
        
        // Set initial colors
        if (durationFillBar != null)
        {
            durationFillBar.color = durationBarColor;
        }
    }

    private void Start()
    {
        // Display the key binding
        if (keyDisplayText != null && skillManager != null)
        {
            keyDisplayText.text = skillManager.GetSlowMotionKeyName();
        }

        UpdateUI();
    }

    private void Update()
    {
        if (skillManager == null) return;

        UpdateUI();
        
        // Pulse effect when active
        if (pulseWhenActive && skillManager.IsSlowMotionActive() && slowMotionPanel != null)
        {
            float pulse = 1f + (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f) * (pulseScale - 1f);
            slowMotionPanel.transform.localScale = originalScale * pulse;
        }
        else if (slowMotionPanel != null)
        {
            slowMotionPanel.transform.localScale = originalScale;
        }
    }

    private void UpdateUI()
    {
        bool isActive = skillManager.IsSlowMotionActive();
        bool isOnCooldown = skillManager.IsOnCooldown();
        
        // Update status text
        if (statusText != null)
        {
            if (isActive)
            {
                statusText.text = "ACTIVE";
                statusText.color = activeColor;
            }
            else if (isOnCooldown)
            {
                float remainingCooldown = skillManager.GetRemainingCooldown();
                statusText.text = $"{remainingCooldown:F1}s";
                statusText.color = cooldownColor;
            }
            else
            {
                statusText.text = "READY";
                statusText.color = readyColor;
            }
        }

        // Update duration fill bar (shows during active)
        if (durationFillBar != null)
        {
            if (isActive)
            {
                float durationPercent = skillManager.GetRemainingDurationPercent();
                durationFillBar.fillAmount = durationPercent;
                durationFillBar.gameObject.SetActive(true);
            }
            else
            {
                durationFillBar.gameObject.SetActive(false);
            }
        }

        // Update cooldown fill bar
        if (cooldownFillBar != null)
        {
            if (isOnCooldown)
            {
                float cooldownPercent = 1f - skillManager.GetCooldownPercent();
                cooldownFillBar.fillAmount = cooldownPercent;
                cooldownFillBar.gameObject.SetActive(true);
            }
            else
            {
                cooldownFillBar.fillAmount = 1f;
                cooldownFillBar.gameObject.SetActive(false);
            }
        }

        // Update icon color/alpha based on state
        if (iconImage != null)
        {
            if (isActive)
            {
                iconImage.color = activeColor;
            }
            else if (isOnCooldown)
            {
                iconImage.color = cooldownColor;
            }
            else
            {
                iconImage.color = readyColor;
            }
        }

        // Fade panel when not ready or active
        if (panelCanvasGroup != null)
        {
            if (isActive)
            {
                panelCanvasGroup.alpha = 1f;
            }
            else if (isOnCooldown)
            {
                panelCanvasGroup.alpha = 0.7f;
            }
            else
            {
                panelCanvasGroup.alpha = 0.85f;
            }
        }
    }
}
