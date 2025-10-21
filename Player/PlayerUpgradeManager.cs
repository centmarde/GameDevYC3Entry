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
    
    private Player[] players;
    private Player2[] player2s;
    private PlayerSkill_CirclingProjectiles[] circlingProjectilesSkills;
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
        // Find all Player2 instances
        if (player2s == null || player2s.Length == 0)
        {
            player2s = FindObjectsOfType<Player2>();
        }
        
        // Find all Player instances
        if (players == null || players.Length == 0)
        {
            players = FindObjectsOfType<Player>();
        }
        
        // Get stats from first available player
        if (player2s != null && player2s.Length > 0)
        {
            if (player2Stats == null)
            {
                player2Stats = player2s[0].Stats;
            }
        }
        else if (players != null && players.Length > 0)
        {
            if (playerStats == null)
            {
                playerStats = players[0].Stats;
            }
        }
        
        // Find circling projectiles skills on all players
        if (players != null && players.Length > 0)
        {
            circlingProjectilesSkills = new PlayerSkill_CirclingProjectiles[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                {
                    circlingProjectilesSkills[i] = players[i].GetComponent<PlayerSkill_CirclingProjectiles>();
                }
            }
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
        
        // Check which character is actually selected and active
        bool hasPlayer2 = IsPlayer2Active();
        bool hasPlayer1 = !hasPlayer2;
        
        if (hasPlayer2)
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
        if (hasPlayer1 && circlingProjectilesSkills != null)
        {
            // Check if any player has the skill obtained
            bool skillObtained = false;
            PlayerSkill_CirclingProjectiles activeSkill = null;
            
            foreach (var skill in circlingProjectilesSkills)
            {
                if (skill != null && skill.IsObtained)
                {
                    skillObtained = true;
                    activeSkill = skill;
                    break;
                }
            }
            
            if (skillObtained && activeSkill != null)
            {
                // Add skill upgrades ONLY if skill is obtained
                if (activeSkill.CurrentProjectileCount < 8)
                {
                    allUpgrades.Add(UpgradeType.UpgradeProjectileCount);
                }
                allUpgrades.Add(UpgradeType.UpgradeProjectileDamage);
                allUpgrades.Add(UpgradeType.UpgradeProjectileRadius);
                allUpgrades.Add(UpgradeType.UpgradeProjectileSpeed);
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
        // Check which character is actually selected and active
        bool hasPlayer2 = IsPlayer2Active();
        bool hasPlayer1 = !hasPlayer2;
        
        if (hasPlayer2 && player2Stats == null)
        {
            return;
        }
        else if (hasPlayer1 && playerStats == null)
        {
            return;
        }
        
        switch (upgradeType)
        {
            case UpgradeType.Damage:
                if (hasPlayer2)
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
                if (hasPlayer2)
                {
                    player2Stats.maxHealth += maxHealthUpgradeAmount;
                }
                else
                {
                    playerStats.maxHealth += maxHealthUpgradeAmount;
                }
                // Increase max health for all players
                Player[] allPlayers = hasPlayer2 ? player2s : players;
                if (allPlayers != null)
                {
                    foreach (Player p in allPlayers)
                    {
                        if (p != null)
                        {
                            var health = p.GetComponent<Entity_Health>();
                            if (health != null)
                            {
                                health.IncreaseMaxHealth(maxHealthUpgradeAmount, false);
                            }
                        }
                    }
                }
                break;
                
            case UpgradeType.Heal:
                // Heal all players to full health
                Player[] playersToHeal = hasPlayer2 ? player2s : players;
                if (playersToHeal != null)
                {
                    foreach (Player p in playersToHeal)
                    {
                        if (p != null)
                        {
                            var health = p.GetComponent<Entity_Health>();
                            if (health != null)
                            {
                                float maxHP = health.MaxHealth;
                                health.Heal(maxHP);
                            }
                        }
                    }
                }
                break;
                
            case UpgradeType.CriticalChance:
                if (hasPlayer2)
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
                if (hasPlayer2)
                {
                    player2Stats.criticalDamageMultiplier += criticalDamageUpgradeAmount;
                }
                else
                {
                    playerStats.criticalDamageMultiplier += criticalDamageUpgradeAmount;
                }
                break;
                
            case UpgradeType.Evasion:
                if (hasPlayer2)
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
                if (circlingProjectilesSkills != null)
                {
                    foreach (var skill in circlingProjectilesSkills)
                    {
                        if (skill != null && !skill.IsObtained)
                        {
                            skill.ObtainSkill();
                        }
                    }
                }
                break;
                
            case UpgradeType.UpgradeProjectileCount:
                if (circlingProjectilesSkills != null)
                {
                    foreach (var skill in circlingProjectilesSkills)
                    {
                        if (skill != null && skill.IsObtained)
                        {
                            skill.UpgradeProjectileCount();
                        }
                    }
                }
                break;
                
            case UpgradeType.UpgradeProjectileDamage:
                if (circlingProjectilesSkills != null)
                {
                    foreach (var skill in circlingProjectilesSkills)
                    {
                        if (skill != null && skill.IsObtained)
                        {
                            skill.UpgradeDamage(skillProjectileDamageUpgrade);
                        }
                    }
                }
                break;
                
            case UpgradeType.UpgradeProjectileRadius:
                if (circlingProjectilesSkills != null)
                {
                    foreach (var skill in circlingProjectilesSkills)
                    {
                        if (skill != null && skill.IsObtained)
                        {
                            skill.UpgradeRadius(skillRadiusUpgrade);
                        }
                    }
                }
                break;
                
            case UpgradeType.UpgradeProjectileSpeed:
                if (circlingProjectilesSkills != null)
                {
                    foreach (var skill in circlingProjectilesSkills)
                    {
                        if (skill != null && skill.IsObtained)
                        {
                            skill.UpgradeSpeed(skillSpeedUpgrade);
                        }
                    }
                }
                break;
                
            // Player2 specific upgrades
            case UpgradeType.UpgradeBlinkDistance:
                if (hasPlayer2 && player2Stats != null)
                {
                    player2Stats.blinkDistance += blinkDistanceUpgradeAmount;
                    Debug.Log($"[PlayerUpgradeManager] Blink/Dash distance upgraded to {player2Stats.blinkDistance}");
                }
                break;
                
            case UpgradeType.ReduceBlinkCooldown:
                if (hasPlayer2 && player2Stats != null)
                {
                    player2Stats.blinkCooldown -= blinkCooldownReduction;
                    player2Stats.blinkCooldown = Mathf.Max(player2Stats.blinkCooldown, 0.5f); // Min 0.5s cooldown
                    Debug.Log($"[PlayerUpgradeManager] Blink cooldown reduced to {player2Stats.blinkCooldown}s");
                }
                break;
                
            case UpgradeType.ReduceDashCooldown:
                if (hasPlayer2 && player2Stats != null)
                {
                    player2Stats.dashAttackCooldown -= dashCooldownReduction;
                    player2Stats.dashAttackCooldown = Mathf.Max(player2Stats.dashAttackCooldown, 0.3f); // Min 0.3s cooldown
                    Debug.Log($"[PlayerUpgradeManager] Dash cooldown reduced to {player2Stats.dashAttackCooldown}s");
                }
                break;
                
            case UpgradeType.UpgradeBlinkDashSpeed:
                if (hasPlayer2 && player2Stats != null)
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
    
    public float GetCurrentDamage() => IsPlayer2Active() ? (player2Stats != null ? player2Stats.projectileDamage : 0f) : (playerStats != null ? playerStats.projectileDamage : 0f);
    public float GetCurrentHealth() => IsPlayer2Active() ? (player2Stats != null ? player2Stats.maxHealth : 0f) : (playerStats != null ? playerStats.maxHealth : 0f);
    public float GetCurrentCriticalChance() => IsPlayer2Active() ? (player2Stats != null ? player2Stats.criticalChance : 0f) : (playerStats != null ? playerStats.criticalChance : 0f);
    public float GetCurrentCriticalDamage() => IsPlayer2Active() ? (player2Stats != null ? player2Stats.criticalDamageMultiplier : 0f) : (playerStats != null ? playerStats.criticalDamageMultiplier : 0f);
    public float GetCurrentEvasion() => IsPlayer2Active() ? (player2Stats != null ? player2Stats.evasionChance : 0f) : (playerStats != null ? playerStats.evasionChance : 0f);
    
    // Player2 specific getters
    public float GetBlinkDistanceUpgradeAmount() => blinkDistanceUpgradeAmount;
    public float GetBlinkCooldownReduction() => blinkCooldownReduction;
    public float GetDashCooldownReduction() => dashCooldownReduction;
    public float GetBlinkDashSpeedUpgrade() => blinkDashSpeedUpgrade;
    
    public float GetCurrentBlinkDistance() => IsPlayer2Active() && player2Stats != null ? player2Stats.blinkDistance : 0f;
    public float GetCurrentBlinkCooldown() => IsPlayer2Active() && player2Stats != null ? player2Stats.blinkCooldown : 0f;
    public float GetCurrentDashCooldown() => IsPlayer2Active() && player2Stats != null ? player2Stats.dashAttackCooldown : 0f;
    public float GetCurrentBlinkDashSpeed() => IsPlayer2Active() && player2Stats != null ? player2Stats.blinkDashSpeed : 0f;
    
    // Circling Projectiles skill getters
    public float GetSkillDamageUpgradeAmount() => skillProjectileDamageUpgrade;
    public float GetSkillRadiusUpgradeAmount() => skillRadiusUpgrade;
    public float GetSkillSpeedUpgradeAmount() => skillSpeedUpgrade;
    
    // Return 0 if skill not obtained or component is null
    public int GetCurrentProjectileCount()
    {
        if (circlingProjectilesSkills != null)
        {
            foreach (var skill in circlingProjectilesSkills)
            {
                if (skill != null && skill.IsObtained)
                    return skill.CurrentProjectileCount;
            }
        }
        return 0;
    }
        
    public float GetCurrentProjectileDamage()
    {
        if (circlingProjectilesSkills != null)
        {
            foreach (var skill in circlingProjectilesSkills)
            {
                if (skill != null && skill.IsObtained)
                    return skill.CurrentDamage;
            }
        }
        return 0f;
    }
        
    public float GetCurrentProjectileRadius()
    {
        if (circlingProjectilesSkills != null)
        {
            foreach (var skill in circlingProjectilesSkills)
            {
                if (skill != null && skill.IsObtained)
                    return skill.CurrentRadius;
            }
        }
        return 0f;
    }
        
    public float GetCurrentProjectileSpeed()
    {
        if (circlingProjectilesSkills != null)
        {
            foreach (var skill in circlingProjectilesSkills)
            {
                if (skill != null && skill.IsObtained)
                    return skill.CurrentSpeed;
            }
        }
        return 0f;
    }
    
    public UpgradeType[] GetCurrentUpgradeOptions() => currentUpgradeOptions;
    
    /// <summary>
    /// Check if Player2 is the active character
    /// </summary>
    private bool IsPlayer2Active()
    {
        // Method 1: Check CharacterSelectionManager
        if (CharacterSelectionManager.Instance != null)
        {
            int selectedIndex = CharacterSelectionManager.Instance.SelectedCharacterIndex;
            if (selectedIndex == 1)
                return true;
            if (selectedIndex == 0)
                return false;
        }
        
        // Method 2: Check if Player2 instances exist and Player1 doesn't
        if (player2s != null && player2s.Length > 0)
        {
            // Check if any Player2 is active in hierarchy
            foreach (var p2 in player2s)
            {
                if (p2 != null && p2.gameObject.activeInHierarchy)
                    return true;
            }
        }
        
        // Method 3: Check by finding active player type in scene
        Player2 activePlayer2 = FindObjectOfType<Player2>();
        if (activePlayer2 != null && activePlayer2.gameObject.activeInHierarchy)
            return true;
        
        // Default to Player1
        return false;
    }
}
