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
    [SerializeField] private Player2_DataSO player2Stats;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private PlayerUpgradeUI upgradeUI;
    
    [Header("Upgrade Values")]
    [SerializeField] private float damageUpgradeAmount = 5f;
    [SerializeField] private float maxHealthUpgradeAmount = 20f;
    [SerializeField] private float healAmount = 30f; // Amount of health restored when choosing Heal upgrade
    [SerializeField] private float criticalChanceUpgradeAmount = 5f;
    [SerializeField] private float criticalDamageUpgradeAmount = 0.25f;
    [SerializeField] private float evasionChanceUpgradeAmount = 3f; // Amount to increase evasion chance
    
    [Header("Player2 Specific Upgrades")]
    [SerializeField] private float blinkDistanceUpgradeAmount = 1f;
    [SerializeField] private float blinkCooldownReduction = 0.3f;
    [SerializeField] private float dashCooldownReduction = 0.2f;
    [SerializeField] private float blinkDashSpeedUpgrade = 3f;
    
    [Header("Circling Projectiles Skill Upgrades")]
    [SerializeField] private float skillProjectileDamageUpgrade = 5f;
    [SerializeField] private float skillRadiusUpgrade = 0.5f;
    [SerializeField] private float skillSpeedUpgrade = 15f;
    
    [Header("Auto-Setup")]
    [SerializeField] private bool autoFindReferences = true;
    
    private Player player;
    private Player2 player2;
    private bool isPlayer2;
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
        UpgradeProjectileSpeed,
        // Player2 specific upgrades
        UpgradeBlinkDistance,
        ReduceBlinkCooldown,
        ReduceDashCooldown,
        UpgradeBlinkDashSpeed
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
        // Find player (try Player2 first)
        if (player2 == null)
        {
            player2 = FindObjectOfType<Player2>();
        }
        
        if (player2 != null)
        {
            isPlayer2 = true;
            player = player2; // Player2 inherits from Player
            
            // Get Player2 stats
            if (player2Stats == null)
            {
                player2Stats = player2.Stats;
            }
        }
        else
        {
            // Find regular player
            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }
            
            isPlayer2 = false;
            
            // Get player stats from player
            if (playerStats == null && player != null)
            {
                playerStats = player.Stats;
            }
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
        // Get all possible upgrade types based on player type
        var allUpgrades = new System.Collections.Generic.List<UpgradeType>();
        
        if (isPlayer2)
        {
            // Player2 upgrade pool - focused on melee/dash combat
            allUpgrades.Add(UpgradeType.Damage); // Increases dash damage
            allUpgrades.Add(UpgradeType.MaxHealth);
            allUpgrades.Add(UpgradeType.Heal);
            allUpgrades.Add(UpgradeType.CriticalChance);
            allUpgrades.Add(UpgradeType.CriticalDamage);
            allUpgrades.Add(UpgradeType.Evasion);
            allUpgrades.Add(UpgradeType.UpgradeBlinkDistance);
            allUpgrades.Add(UpgradeType.ReduceBlinkCooldown);
            allUpgrades.Add(UpgradeType.ReduceDashCooldown);
            allUpgrades.Add(UpgradeType.UpgradeBlinkDashSpeed);
        }
        else
        {
            // Player1 upgrade pool - ranged combat focused
            allUpgrades.Add(UpgradeType.Damage);
            allUpgrades.Add(UpgradeType.MaxHealth);
            allUpgrades.Add(UpgradeType.Heal);
            allUpgrades.Add(UpgradeType.CriticalChance);
            allUpgrades.Add(UpgradeType.CriticalDamage);
            allUpgrades.Add(UpgradeType.Evasion);
        }
        
        // Add circling projectiles upgrades only for Player1
        if (!isPlayer2)
        {
            // Check if skill is obtained (from component state, not ScriptableObject)
            bool skillObtained = circlingProjectilesSkill != null && circlingProjectilesSkill.IsObtained;
            
            if (skillObtained)
            {
                // Add skill upgrades ONLY if skill is obtained
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
            }
        }
        
        // Shuffle and pick 3
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, allUpgrades.Count);
            currentUpgradeOptions[i] = allUpgrades[randomIndex];
            allUpgrades.RemoveAt(randomIndex);
        }
    }
    
    /// <summary>
    /// Apply the selected upgrade
    /// </summary>
    public void ApplyUpgrade(UpgradeType upgradeType)
    {
        if (isPlayer2 && player2Stats == null)
        {
            return;
        }
        else if (!isPlayer2 && playerStats == null)
        {
            return;
        }
        
        switch (upgradeType)
        {
            case UpgradeType.Damage:
                if (isPlayer2)
                {
                    player2Stats.projectileDamage += damageUpgradeAmount;
                    Debug.Log($"[PlayerUpgradeManager] Player2 Damage upgraded to {player2Stats.projectileDamage} (Dash damage will scale accordingly)");
                }
                else
                {
                    playerStats.projectileDamage += damageUpgradeAmount;
                }
                break;
                
            case UpgradeType.MaxHealth:
                if (isPlayer2)
                {
                    player2Stats.maxHealth += maxHealthUpgradeAmount;
                }
                else
                {
                    playerStats.maxHealth += maxHealthUpgradeAmount;
                }
                // Only increase max health, do NOT heal
                if (player != null)
                {
                    var health = player.GetComponent<Entity_Health>();
                    if (health != null)
                    {
                        health.IncreaseMaxHealth(maxHealthUpgradeAmount, false); // Increase max only, don't heal
                    }
                }
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
                break;
                
            case UpgradeType.CriticalChance:
                if (isPlayer2)
                {
                    player2Stats.criticalChance += criticalChanceUpgradeAmount;
                    player2Stats.criticalChance = Mathf.Min(player2Stats.criticalChance, 100f);
                }
                else
                {
                    playerStats.criticalChance += criticalChanceUpgradeAmount;
                    playerStats.criticalChance = Mathf.Min(playerStats.criticalChance, 100f);
                }
                break;
                
            case UpgradeType.CriticalDamage:
                if (isPlayer2)
                {
                    player2Stats.criticalDamageMultiplier += criticalDamageUpgradeAmount;
                }
                else
                {
                    playerStats.criticalDamageMultiplier += criticalDamageUpgradeAmount;
                }
                break;
                
            case UpgradeType.Evasion:
                if (isPlayer2)
                {
                    player2Stats.evasionChance += evasionChanceUpgradeAmount;
                    player2Stats.evasionChance = Mathf.Min(player2Stats.evasionChance, 100f);
                }
                else
                {
                    playerStats.evasionChance += evasionChanceUpgradeAmount;
                    playerStats.evasionChance = Mathf.Min(playerStats.evasionChance, 100f);
                }
                break;
                
            case UpgradeType.UnlockCirclingProjectiles:
                if (circlingProjectilesSkill != null && !circlingProjectilesSkill.IsObtained)
                {
                    circlingProjectilesSkill.ObtainSkill();
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
                
            // Player2 specific upgrades
            case UpgradeType.UpgradeBlinkDistance:
                if (isPlayer2 && player2Stats != null)
                {
                    player2Stats.blinkDistance += blinkDistanceUpgradeAmount;
                    Debug.Log($"[PlayerUpgradeManager] Blink/Dash distance upgraded to {player2Stats.blinkDistance}");
                }
                break;
                
            case UpgradeType.ReduceBlinkCooldown:
                if (isPlayer2 && player2Stats != null)
                {
                    player2Stats.blinkCooldown -= blinkCooldownReduction;
                    player2Stats.blinkCooldown = Mathf.Max(player2Stats.blinkCooldown, 0.5f); // Min 0.5s cooldown
                    Debug.Log($"[PlayerUpgradeManager] Blink cooldown reduced to {player2Stats.blinkCooldown}s");
                }
                break;
                
            case UpgradeType.ReduceDashCooldown:
                if (isPlayer2 && player2Stats != null)
                {
                    player2Stats.dashAttackCooldown -= dashCooldownReduction;
                    player2Stats.dashAttackCooldown = Mathf.Max(player2Stats.dashAttackCooldown, 0.3f); // Min 0.3s cooldown
                    Debug.Log($"[PlayerUpgradeManager] Dash cooldown reduced to {player2Stats.dashAttackCooldown}s");
                }
                break;
                
            case UpgradeType.UpgradeBlinkDashSpeed:
                if (isPlayer2 && player2Stats != null)
                {
                    player2Stats.blinkDashSpeed += blinkDashSpeedUpgrade;
                    Debug.Log($"[PlayerUpgradeManager] Blink/Dash speed upgraded to {player2Stats.blinkDashSpeed}");
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
    
    public float GetCurrentDamage() => isPlayer2 ? (player2Stats != null ? player2Stats.projectileDamage : 0f) : (playerStats != null ? playerStats.projectileDamage : 0f);
    public float GetCurrentHealth() => isPlayer2 ? (player2Stats != null ? player2Stats.maxHealth : 0f) : (playerStats != null ? playerStats.maxHealth : 0f);
    public float GetCurrentCriticalChance() => isPlayer2 ? (player2Stats != null ? player2Stats.criticalChance : 0f) : (playerStats != null ? playerStats.criticalChance : 0f);
    public float GetCurrentCriticalDamage() => isPlayer2 ? (player2Stats != null ? player2Stats.criticalDamageMultiplier : 0f) : (playerStats != null ? playerStats.criticalDamageMultiplier : 0f);
    public float GetCurrentEvasion() => isPlayer2 ? (player2Stats != null ? player2Stats.evasionChance : 0f) : (playerStats != null ? playerStats.evasionChance : 0f);
    
    // Player2 specific getters
    public float GetBlinkDistanceUpgradeAmount() => blinkDistanceUpgradeAmount;
    public float GetBlinkCooldownReduction() => blinkCooldownReduction;
    public float GetDashCooldownReduction() => dashCooldownReduction;
    public float GetBlinkDashSpeedUpgrade() => blinkDashSpeedUpgrade;
    
    public float GetCurrentBlinkDistance() => isPlayer2 && player2Stats != null ? player2Stats.blinkDistance : 0f;
    public float GetCurrentBlinkCooldown() => isPlayer2 && player2Stats != null ? player2Stats.blinkCooldown : 0f;
    public float GetCurrentDashCooldown() => isPlayer2 && player2Stats != null ? player2Stats.dashAttackCooldown : 0f;
    public float GetCurrentBlinkDashSpeed() => isPlayer2 && player2Stats != null ? player2Stats.blinkDashSpeed : 0f;
    
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
