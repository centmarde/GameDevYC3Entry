using UnityEngine;
using TMPro;

/// <summary>
/// Minimalist UI Controller for Slow Motion skill - Text-Only Display
/// Shows only essential status text with subtle visual feedback
/// </summary>
public class SlowMotionUI_Minimal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerSkill_Manager skillManager;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI keyHintText; // Optional
    
    [Header("Colors - Subtle & Minimal")]
    [SerializeField] private Color readyColor = new Color(1f, 1f, 1f, 0.7f);     // White, semi-transparent
    [SerializeField] private Color activeColor = new Color(0f, 1f, 1f, 0.9f);    // Cyan, slightly brighter
    [SerializeField] private Color cooldownColor = new Color(1f, 1f, 1f, 0.3f);  // White, very faded
    
    [Header("Animation - Optional")]
    [SerializeField] private bool fadeInOut = true;
    [SerializeField] private float fadeSpeed = 2f;
    
    private Color targetColor;
    
    private void Start()
    {
        // Auto-find skill manager
        if (skillManager == null)
        {
            skillManager = FindObjectOfType<PlayerSkill_Manager>();
            
            if (skillManager == null)
            {
                Debug.LogWarning("SlowMotionUI_Minimal: Could not find PlayerSkill_Manager in scene");
            }
        }
        
        // Initialize key hint
        if (keyHintText != null && skillManager != null)
        {
            keyHintText.text = $"[{skillManager.GetSlowMotionKeyName()}]";
        }
        
        targetColor = readyColor;
    }
    
    private void Update()
    {
        if (skillManager == null || statusText == null) return;
        
        UpdateStatus();
        
        // Smooth color transition
        if (fadeInOut)
        {
            statusText.color = Color.Lerp(statusText.color, targetColor, Time.deltaTime * fadeSpeed);
        }
        else
        {
            statusText.color = targetColor;
        }
    }
    
    private void UpdateStatus()
    {
        if (skillManager.IsSlowMotionActive())
        {
            // Show remaining duration as percentage or countdown
            // Since we don't have direct duration access, calculate from percent
            float durationPercent = skillManager.GetRemainingDurationPercent();
            statusText.text = "ACTIVE";
            targetColor = activeColor;
        }
        else if (skillManager.IsOnCooldown())
        {
            // Show cooldown countdown
            float cooldown = skillManager.GetRemainingCooldown();
            statusText.text = $"{cooldown:F1}s";
            targetColor = cooldownColor;
        }
        else
        {
            // Ready state
            statusText.text = "Press X to Activate";
            targetColor = readyColor;
        }
    }
}
