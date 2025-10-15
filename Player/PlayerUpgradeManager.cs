using UnityEngine;
using System;

/// <summary>
/// Manages player stat upgrades after each wave completion
/// Integrates with WaveManager and updates Player_DataSO in real-time
/// </summary>
public class PlayerUpgradeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player_DataSO playerStats;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private PlayerUpgradeUI upgradeUI;
    
    [Header("Upgrade Values")]
    [SerializeField] private float damageUpgradeAmount = 5f;
    [SerializeField] private float healthUpgradeAmount = 20f;
    [SerializeField] private float criticalChanceUpgradeAmount = 5f;
    [SerializeField] private float criticalDamageUpgradeAmount = 0.25f;
    
    [Header("Auto-Setup")]
    [SerializeField] private bool autoFindReferences = true;
    
    private Player player;
    private bool upgradePending = false;
    private UpgradeType[] currentUpgradeOptions = new UpgradeType[3];
    
    public enum UpgradeType
    {
        Damage,
        Health,
        CriticalChance,
        CriticalDamage
    }
    
    private void Awake()
    {
        if (autoFindReferences)
        {
            SetupReferences();
        }
    }
    
    private void Start()
    {
        // Subscribe to wave completion event
        if (waveManager != null)
        {
            waveManager.OnAllEnemiesCleared.AddListener(OnWaveCleared);
        }
        else
        {
            Debug.LogWarning("PlayerUpgradeManager: WaveManager not assigned!");
        }
        
        // Hide upgrade UI at start
        if (upgradeUI != null)
        {
            upgradeUI.HideUpgradePanel();
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from wave events
        if (waveManager != null)
        {
            waveManager.OnAllEnemiesCleared.RemoveListener(OnWaveCleared);
        }
    }
    
    /// <summary>
    /// Auto-setup references if not assigned
    /// </summary>
    private void SetupReferences()
    {
        // Find player
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }
        
        // Get player stats from player
        if (playerStats == null && player != null)
        {
            playerStats = player.Stats;
        }
        
        // Find wave manager
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
        }
        
        // Find or create upgrade UI
        if (upgradeUI == null)
        {
            upgradeUI = FindObjectOfType<PlayerUpgradeUI>();
            
            if (upgradeUI == null)
            {
                // Create upgrade UI
                GameObject uiObj = new GameObject("PlayerUpgradeUI");
                upgradeUI = uiObj.AddComponent<PlayerUpgradeUI>();
            }
        }
        
        // Setup UI with this manager
        if (upgradeUI != null)
        {
            upgradeUI.SetupWithManager(this);
        }
    }
    
    /// <summary>
    /// Called when a wave is cleared
    /// </summary>
    private void OnWaveCleared(int waveNumber)
    {
        Debug.Log($"PlayerUpgradeManager: Wave {waveNumber} cleared! Showing upgrade options...");
        
        // Generate 3 random upgrade options
        GenerateRandomUpgrades();
        
        // Pause wave progression
        if (waveManager != null)
        {
            waveManager.PauseWaves();
        }
        
        // Show upgrade UI
        upgradePending = true;
        if (upgradeUI != null)
        {
            upgradeUI.ShowUpgradePanel();
        }
        
        // Pause game
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Generate 3 random unique upgrade options
    /// </summary>
    private void GenerateRandomUpgrades()
    {
        // Get all possible upgrade types
        var allUpgrades = new System.Collections.Generic.List<UpgradeType>
        {
            UpgradeType.Damage,
            UpgradeType.Health,
            UpgradeType.CriticalChance,
            UpgradeType.CriticalDamage
        };
        
        // Shuffle and pick 3
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, allUpgrades.Count);
            currentUpgradeOptions[i] = allUpgrades[randomIndex];
            allUpgrades.RemoveAt(randomIndex);
        }
        
        Debug.Log($"Upgrade options: {currentUpgradeOptions[0]}, {currentUpgradeOptions[1]}, {currentUpgradeOptions[2]}");
    }
    
    /// <summary>
    /// Apply the selected upgrade
    /// </summary>
    public void ApplyUpgrade(UpgradeType upgradeType)
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerUpgradeManager: Cannot apply upgrade - playerStats is null!");
            return;
        }
        
        switch (upgradeType)
        {
            case UpgradeType.Damage:
                playerStats.projectileDamage += damageUpgradeAmount;
                Debug.Log($"Damage upgraded! New damage: {playerStats.projectileDamage}");
                break;
                
            case UpgradeType.Health:
                playerStats.maxHealth += healthUpgradeAmount;
                // Also increase player's max health and heal to full
                if (player != null)
                {
                    var health = player.GetComponent<Entity_Health>();
                    if (health != null)
                    {
                        health.IncreaseMaxHealth(healthUpgradeAmount, true); // Increase max and heal to full
                    }
                }
                Debug.Log($"Max Health upgraded! New max health: {playerStats.maxHealth}");
                break;
                
            case UpgradeType.CriticalChance:
                playerStats.criticalChance += criticalChanceUpgradeAmount;
                playerStats.criticalChance = Mathf.Min(playerStats.criticalChance, 100f); // Cap at 100%
                Debug.Log($"Critical Chance upgraded! New crit chance: {playerStats.criticalChance}%");
                break;
                
            case UpgradeType.CriticalDamage:
                playerStats.criticalDamageMultiplier += criticalDamageUpgradeAmount;
                Debug.Log($"Critical Damage upgraded! New crit multiplier: {playerStats.criticalDamageMultiplier}x");
                break;
        }
        
        // Hide UI
        if (upgradeUI != null)
        {
            upgradeUI.HideUpgradePanel();
        }
        
        // Resume wave progression
        upgradePending = false;
        if (waveManager != null)
        {
            waveManager.ResumeWaves();
        }
        
        // Unpause game
        Time.timeScale = 1f;
    }
    
    // Public getters for upgrade amounts (for UI display)
    public float GetDamageUpgradeAmount() => damageUpgradeAmount;
    public float GetHealthUpgradeAmount() => healthUpgradeAmount;
    public float GetCriticalChanceUpgradeAmount() => criticalChanceUpgradeAmount;
    public float GetCriticalDamageUpgradeAmount() => criticalDamageUpgradeAmount;
    
    public float GetCurrentDamage() => playerStats != null ? playerStats.projectileDamage : 0f;
    public float GetCurrentHealth() => playerStats != null ? playerStats.maxHealth : 0f;
    public float GetCurrentCriticalChance() => playerStats != null ? playerStats.criticalChance : 0f;
    public float GetCurrentCriticalDamage() => playerStats != null ? playerStats.criticalDamageMultiplier : 0f;
    
    public UpgradeType[] GetCurrentUpgradeOptions() => currentUpgradeOptions;
}
