using UnityEngine;
using TMPro;

/// <summary>
/// Displays player stats including evasion in the UI
/// </summary>
public class PlayerStatsDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private TextMeshProUGUI statsText;
    
    [Header("Display Settings")]
    [SerializeField] private bool showHealth = true;
    [SerializeField] private bool showDamage = true;
    [SerializeField] private bool showCritical = true;
    [SerializeField] private bool showEvasion = true;
    [SerializeField] private bool updateEveryFrame = false;
    [SerializeField] private float updateInterval = 0.5f;
    
    private float lastUpdateTime;
    private Entity_Health playerHealth;
    
    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<Player>();
            
        if (player != null)
            playerHealth = player.GetComponent<Entity_Health>();
    }
    
    private void Start()
    {
        UpdateDisplay();
    }
    
    private void Update()
    {
        if (updateEveryFrame)
        {
            UpdateDisplay();
        }
        else if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    private void UpdateDisplay()
    {
        if (statsText == null || player == null || player.Stats == null)
            return;
            
        string display = "<b>PLAYER STATS</b>\n\n";
        
        if (showHealth && playerHealth != null)
        {
            display += $"<color=#FF6B6B>❤</color> Health: {playerHealth.CurrentHealth:F0}/{playerHealth.MaxHealth:F0}\n";
        }
        
        if (showDamage)
        {
            display += $"<color=#FFD93D>⚔</color> Damage: {player.Stats.projectileDamage:F1}\n";
        }
        
        if (showCritical)
        {
            display += $"<color=#FF8B42>💥</color> Crit Chance: {player.Stats.criticalChance:F1}%\n";
            display += $"<color=#FF8B42>⚡</color> Crit Damage: {player.Stats.criticalDamageMultiplier:F2}x\n";
        }
        
        if (showEvasion)
        {
            string evasionColor = GetEvasionColor(player.Stats.evasionChance);
            display += $"<color={evasionColor}>🌀</color> Evasion: {player.Stats.evasionChance:F1}%\n";
        }
        
        statsText.text = display;
    }
    
    /// <summary>
    /// Get color based on evasion value (higher = more blue)
    /// </summary>
    private string GetEvasionColor(float evasion)
    {
        if (evasion >= 50f)
            return "#00D9FF"; // Bright cyan
        else if (evasion >= 25f)
            return "#6EC1E4"; // Light blue
        else if (evasion >= 10f)
            return "#87CEEB"; // Sky blue
        else
            return "#B0E0E6"; // Powder blue
    }
    
    /// <summary>
    /// Force an immediate update of the display
    /// </summary>
    public void ForceUpdate()
    {
        UpdateDisplay();
    }
    
    /// <summary>
    /// Show a temporary stat highlight (for upgrades)
    /// </summary>
    public void HighlightStat(string statName, float duration = 2f)
    {
        StartCoroutine(HighlightRoutine(statName, duration));
    }
    
    private System.Collections.IEnumerator HighlightRoutine(string statName, float duration)
    {
        // Simple pulse effect
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time for paused upgrades
            yield return null;
        }
        
        UpdateDisplay();
    }
}
