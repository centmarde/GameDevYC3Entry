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
    [SerializeField] private float maxHealthUpgradeAmount = 20f;
    [SerializeField] private float healAmount = 30f; // Amount of health restored when choosing Heal upgrade
    [SerializeField] private float criticalChanceUpgradeAmount = 5f;
    [SerializeField] private float criticalDamageUpgradeAmount = 0.25f;
    [SerializeField] private float evasionChanceUpgradeAmount = 3f; // Amount to increase evasion chance
    
    [Header("Circling Projectiles Skill Upgrades")]
    [SerializeField] private float skillProjectileDamageUpgrade = 5f;
    [SerializeField] private float skillRadiusUpgrade = 0.5f;
    [SerializeField] private float skillSpeedUpgrade = 15f;
    
    [Header("Auto-Setup")]
    [SerializeField] private bool autoFindReferences = true;
    
    private Player player;
    private PlayerSkill_CirclingProjectiles circlingProjectilesSkill;
    private bool upgradePending = false;
    private UpgradeType[] currentUpgradeOptions = new UpgradeType[3];
    
    public enum UpgradeType
    {
        Damage,
        MaxHealth,
        Heal,
        CriticalChance,
        CriticalDamage,
        Evasion,
        UnlockCirclingProjectiles,
        UpgradeProjectileCount,
        UpgradeProjectileDamage,
        UpgradeProjectileRadius,
        UpgradeProjectileSpeed
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
        
        // Find circling projectiles skill
        if (circlingProjectilesSkill == null && player != null)
        {
            circlingProjectilesSkill = player.GetComponent<PlayerSkill_CirclingProjectiles>();
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
            UpgradeType.MaxHealth,
            UpgradeType.Heal,
            UpgradeType.CriticalChance,
            UpgradeType.CriticalDamage,
            UpgradeType.Evasion
        };
        
        // Check if skill is obtained (from component state, not ScriptableObject)
        bool skillObtained = circlingProjectilesSkill != null && circlingProjectilesSkill.IsObtained;
        
        Debug.Log($"[UPGRADE DEBUG] Skill Component: {circlingProjectilesSkill != null}, isObtained: {(circlingProjectilesSkill?.IsObtained ?? false)}");
        
        if (skillObtained)
        {
            // Add skill upgrades ONLY if skill is obtained
            Debug.Log("[UPGRADE DEBUG] Skill IS obtained - Adding skill upgrade options");
            if (circlingProjectilesSkill.CurrentProjectileCount < 8)
            {
                allUpgrades.Add(UpgradeType.UpgradeProjectileCount);
            }
            allUpgrades.Add(UpgradeType.UpgradeProjectileDamage);
            allUpgrades.Add(UpgradeType.UpgradeProjectileRadius);
            allUpgrades.Add(UpgradeType.UpgradeProjectileSpeed);
        }
        else
        {
            // Skill NOT obtained - Do NOT add unlock option (reserved for future special unlock)
            Debug.Log("[UPGRADE DEBUG] Skill NOT obtained - Unlock option hidden (reserved for future)");
        }
        
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
                
            case UpgradeType.MaxHealth:
                playerStats.maxHealth += maxHealthUpgradeAmount;
                // Only increase max health, do NOT heal
                if (player != null)
                {
                    var health = player.GetComponent<Entity_Health>();
                    if (health != null)
                    {
                        health.IncreaseMaxHealth(maxHealthUpgradeAmount, false); // Increase max only, don't heal
                    }
                }
                Debug.Log($"Max Health upgraded! New max health: {playerStats.maxHealth}");
                break;
                
            case UpgradeType.Heal:
                // Heal player to full health
                if (player != null)
                {
                    var health = player.GetComponent<Entity_Health>();
                    if (health != null)
                    {
                        float maxHP = health.MaxHealth; // Use public property
                        health.Heal(maxHP); // Heal to full health
                    }
                }
                Debug.Log($"Player healed to full health!");
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
                
            case UpgradeType.Evasion:
                playerStats.evasionChance += evasionChanceUpgradeAmount;
                playerStats.evasionChance = Mathf.Min(playerStats.evasionChance, 100f); // Cap at 100%
                Debug.Log($"Evasion upgraded! New evasion chance: {playerStats.evasionChance}%");
                break;
                
            case UpgradeType.UnlockCirclingProjectiles:
                if (circlingProjectilesSkill != null && !circlingProjectilesSkill.IsObtained)
                {
                    circlingProjectilesSkill.ObtainSkill();
                    Debug.Log("Circling Projectiles Skill Unlocked!");
                }
                break;
                
            case UpgradeType.UpgradeProjectileCount:
                if (circlingProjectilesSkill != null && circlingProjectilesSkill.IsObtained)
                {
                    circlingProjectilesSkill.UpgradeProjectileCount();
                }
                break;
                
            case UpgradeType.UpgradeProjectileDamage:
                if (circlingProjectilesSkill != null && circlingProjectilesSkill.IsObtained)
                {
                    circlingProjectilesSkill.UpgradeDamage(skillProjectileDamageUpgrade);
                }
                break;
                
            case UpgradeType.UpgradeProjectileRadius:
                if (circlingProjectilesSkill != null && circlingProjectilesSkill.IsObtained)
                {
                    circlingProjectilesSkill.UpgradeRadius(skillRadiusUpgrade);
                }
                break;
                
            case UpgradeType.UpgradeProjectileSpeed:
                if (circlingProjectilesSkill != null && circlingProjectilesSkill.IsObtained)
                {
                    circlingProjectilesSkill.UpgradeSpeed(skillSpeedUpgrade);
                }
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
    public float GetMaxHealthUpgradeAmount() => maxHealthUpgradeAmount;
    public float GetHealAmount() => healAmount;
    public float GetCriticalChanceUpgradeAmount() => criticalChanceUpgradeAmount;
    public float GetCriticalDamageUpgradeAmount() => criticalDamageUpgradeAmount;
    public float GetEvasionChanceUpgradeAmount() => evasionChanceUpgradeAmount;
    
    public float GetCurrentDamage() => playerStats != null ? playerStats.projectileDamage : 0f;
    public float GetCurrentHealth() => playerStats != null ? playerStats.maxHealth : 0f;
    public float GetCurrentCriticalChance() => playerStats != null ? playerStats.criticalChance : 0f;
    public float GetCurrentCriticalDamage() => playerStats != null ? playerStats.criticalDamageMultiplier : 0f;
    public float GetCurrentEvasion() => playerStats != null ? playerStats.evasionChance : 0f;
    
    // Circling Projectiles skill getters
    public float GetSkillDamageUpgradeAmount() => skillProjectileDamageUpgrade;
    public float GetSkillRadiusUpgradeAmount() => skillRadiusUpgrade;
    public float GetSkillSpeedUpgradeAmount() => skillSpeedUpgrade;
    
    // Return 0 if skill not obtained or component is null
    public int GetCurrentProjectileCount() => 
        (circlingProjectilesSkill != null && circlingProjectilesSkill.IsObtained) 
        ? circlingProjectilesSkill.CurrentProjectileCount : 0;
        
    public float GetCurrentProjectileDamage() => 
        (circlingProjectilesSkill != null && circlingProjectilesSkill.IsObtained) 
        ? circlingProjectilesSkill.CurrentDamage : 0f;
        
    public float GetCurrentProjectileRadius() => 
        (circlingProjectilesSkill != null && circlingProjectilesSkill.IsObtained) 
        ? circlingProjectilesSkill.CurrentRadius : 0f;
        
    public float GetCurrentProjectileSpeed() => 
        (circlingProjectilesSkill != null && circlingProjectilesSkill.IsObtained) 
        ? circlingProjectilesSkill.CurrentSpeed : 0f;
    
    public UpgradeType[] GetCurrentUpgradeOptions() => currentUpgradeOptions;
}
