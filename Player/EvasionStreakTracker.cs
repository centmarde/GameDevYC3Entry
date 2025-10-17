using UnityEngine;
using System.Collections;

/// <summary>
/// Tracks consecutive evasions and provides bonuses for streaks
/// This is an OPTIONAL enhancement to the evasion system
/// </summary>
public class EvasionStreakTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Entity_Health entityHealth;
    [SerializeField] private Player_DataSO playerStats;
    
    [Header("Streak Settings")]
    [SerializeField] private int streakRequirement = 3; // Evasions needed for a streak
    [SerializeField] private float streakTimeout = 5f; // Seconds before streak resets
    
    [Header("Streak Bonuses")]
    [SerializeField] private bool enableBonuses = true;
    [SerializeField] private float bonusDamagePercent = 10f; // +10% damage per streak
    [SerializeField] private float bonusDuration = 5f; // Seconds the bonus lasts
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showStreakMessages = true;
    [SerializeField] private Color streakColor = new Color(1f, 0.84f, 0f); // Gold
    
    [Header("Statistics (Read Only)")]
    [SerializeField] private int currentStreak = 0;
    [SerializeField] private int longestStreak = 0;
    [SerializeField] private int totalEvasions = 0;
    [SerializeField] private int totalHits = 0;
    [SerializeField] private int streakBonusesEarned = 0;
    [SerializeField] private bool bonusActive = false;
    
    private float lastEvasionTime;
    private Coroutine bonusCoroutine;
    private float originalDamage;
    
    // Events
    public event System.Action<int> OnStreakAchieved;
    public event System.Action OnStreakBroken;
    
    private void Awake()
    {
        if (entityHealth == null)
            entityHealth = GetComponent<Entity_Health>();
            
        if (playerStats == null && TryGetComponent<Player>(out var player))
            playerStats = player.Stats;
    }
    
    private void OnEnable()
    {
        if (entityHealth != null)
        {
            entityHealth.OnEvaded += HandleEvasion;
            entityHealth.OnDamaged += HandleHit;
        }
        
        if (playerStats != null)
            originalDamage = playerStats.projectileDamage;
    }
    
    private void OnDisable()
    {
        if (entityHealth != null)
        {
            entityHealth.OnEvaded -= HandleEvasion;
            entityHealth.OnDamaged -= HandleHit;
        }
        
        // Remove any active bonuses
        if (bonusActive && playerStats != null)
        {
            playerStats.projectileDamage = originalDamage;
            bonusActive = false;
        }
    }
    
    private void HandleEvasion(float damage, Vector3 hitPoint, Vector3 hitNormal, object source)
    {
        totalEvasions++;
        currentStreak++;
        lastEvasionTime = Time.time;
        
        // Update longest streak
        if (currentStreak > longestStreak)
            longestStreak = currentStreak;
        
        // Check if we hit a streak milestone
        if (currentStreak > 0 && currentStreak % streakRequirement == 0)
        {
            OnStreakAchieved?.Invoke(currentStreak);
            ActivateStreakBonus();
        }
        
        if (showStreakMessages)
        {
            string message = $"<color=#{ColorUtility.ToHtmlStringRGB(streakColor)}>EVADE STREAK: {currentStreak}!</color>";
            Debug.Log(message);
        }
    }
    
    private void HandleHit(float damage, Vector3 hitPoint, Vector3 hitNormal, object source)
    {
        totalHits++;
        
        // Reset streak if hit
        if (currentStreak > 0)
        {
            if (showStreakMessages)
                Debug.Log($"<color=red>Streak broken at {currentStreak}!</color>");
                
            OnStreakBroken?.Invoke();
            currentStreak = 0;
        }
    }
    
    private void Update()
    {
        // Check if streak should timeout
        if (currentStreak > 0 && Time.time - lastEvasionTime > streakTimeout)
        {
            if (showStreakMessages)
                Debug.Log($"<color=yellow>Streak timeout at {currentStreak}</color>");
                
            currentStreak = 0;
            OnStreakBroken?.Invoke();
        }
    }
    
    /// <summary>
    /// Activate temporary damage bonus from streak
    /// </summary>
    private void ActivateStreakBonus()
    {
        if (!enableBonuses || playerStats == null)
            return;
        
        streakBonusesEarned++;
        
        // Stop existing bonus if any
        if (bonusCoroutine != null)
            StopCoroutine(bonusCoroutine);
        
        bonusCoroutine = StartCoroutine(BonusCoroutine());
    }
    
    private IEnumerator BonusCoroutine()
    {
        // Apply bonus
        if (!bonusActive)
        {
            originalDamage = playerStats.projectileDamage;
            bonusActive = true;
        }
        
        float bonusMultiplier = 1f + (bonusDamagePercent / 100f);
        playerStats.projectileDamage = originalDamage * bonusMultiplier;
        
        if (showStreakMessages)
        {
            Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(streakColor)}>⚡ STREAK BONUS ACTIVE! +{bonusDamagePercent}% Damage for {bonusDuration}s</color>");
        }
        
        // Wait for duration
        yield return new WaitForSeconds(bonusDuration);
        
        // Remove bonus
        if (playerStats != null)
            playerStats.projectileDamage = originalDamage;
            
        bonusActive = false;
        
        if (showStreakMessages)
            Debug.Log("Streak bonus expired");
    }
    
    /// <summary>
    /// Get statistics about evasion performance
    /// </summary>
    public EvasionStats GetStats()
    {
        return new EvasionStats
        {
            currentStreak = currentStreak,
            longestStreak = longestStreak,
            totalEvasions = totalEvasions,
            totalHits = totalHits,
            totalAttempts = totalEvasions + totalHits,
            evasionRate = GetEvasionRate(),
            streakBonusesEarned = streakBonusesEarned
        };
    }
    
    /// <summary>
    /// Calculate actual evasion rate from statistics
    /// </summary>
    public float GetEvasionRate()
    {
        int total = totalEvasions + totalHits;
        return total > 0 ? (totalEvasions / (float)total) * 100f : 0f;
    }
    
    /// <summary>
    /// Reset all statistics
    /// </summary>
    public void ResetStats()
    {
        currentStreak = 0;
        longestStreak = 0;
        totalEvasions = 0;
        totalHits = 0;
        streakBonusesEarned = 0;
        
        Debug.Log("Evasion streak stats reset");
    }
    
    // Struct to hold statistics
    public struct EvasionStats
    {
        public int currentStreak;
        public int longestStreak;
        public int totalEvasions;
        public int totalHits;
        public int totalAttempts;
        public float evasionRate;
        public int streakBonusesEarned;
        
        public override string ToString()
        {
            return $"Current Streak: {currentStreak}\n" +
                   $"Longest Streak: {longestStreak}\n" +
                   $"Total Evasions: {totalEvasions}\n" +
                   $"Total Hits: {totalHits}\n" +
                   $"Evasion Rate: {evasionRate:F1}%\n" +
                   $"Bonuses Earned: {streakBonusesEarned}";
        }
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Print Statistics")]
    private void PrintStats()
    {
        Debug.Log($"=== EVASION STREAK STATISTICS ===\n{GetStats()}");
    }
    
    [ContextMenu("Reset Statistics")]
    private void ResetStatsMenu()
    {
        ResetStats();
    }
    #endif
}
