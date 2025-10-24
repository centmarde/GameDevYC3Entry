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
    
    [Header("Extra Hand Skill Upgrades")]
    [SerializeField] private float extraHandDamagePerLevel = 2f;
    [SerializeField] private float extraHandIntervalReductionPerLevel = 0.2f;
    [SerializeField] private float extraHandRangePerLevel = 1f;
    
    [Header("Auto-Setup")]
    [SerializeField] private bool autoFindReferences = true;
    
    private Player[] players;
    private Player2[] player2s;
    private PlayerSkill_CirclingProjectiles[] circlingProjectilesSkills;
    private PlayerSkill_PushWave[] pushWaveSkills;
    private PlayerSkill_ExtraHand[] extraHandSkills;
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
        UpgradeCirclingProjectiles, // Level-based upgrade (1-10) that increases all stats
        UpgradePushWave, // Level-based upgrade (1-10) that increases radius, force, damage, reduces cooldown
        UpgradeExtraHand, // Level-based upgrade (1-10) that increases damage, fire rate, and range
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
        RefreshPlayerReferences();
    }
    
    /// <summary>
    /// Refresh player references (call this when players might have changed)
    /// </summary>
    private void RefreshPlayerReferences()
    {
        // Find all Player2 instances (including inactive)
        player2s = FindObjectsOfType<Player2>(true);
        Debug.Log($"[PlayerUpgradeManager] Found {player2s.Length} Player2 instances");
        
        // Find all Player instances (including inactive) - this will also find Player2 since it inherits from Player
        Player[] allPlayers = FindObjectsOfType<Player>(true);
        
        // Filter out Player2 instances to get only base Player
        System.Collections.Generic.List<Player> player1List = new System.Collections.Generic.List<Player>();
        foreach (Player p in allPlayers)
        {
            if (!(p is Player2))
            {
                player1List.Add(p);
            }
        }
        players = player1List.ToArray();
        Debug.Log($"[PlayerUpgradeManager] Found {players.Length} Player instances");
        
        // Find circling projectiles skills on all players
        System.Collections.Generic.List<PlayerSkill_CirclingProjectiles> circlingSkillsList = new System.Collections.Generic.List<PlayerSkill_CirclingProjectiles>();
        System.Collections.Generic.List<PlayerSkill_PushWave> pushWaveSkillsList = new System.Collections.Generic.List<PlayerSkill_PushWave>();
        System.Collections.Generic.List<PlayerSkill_ExtraHand> extraHandSkillsList = new System.Collections.Generic.List<PlayerSkill_ExtraHand>();
        
        foreach (Player p in allPlayers)
        {
            if (p != null)
            {
                var circlingSkill = p.GetComponent<PlayerSkill_CirclingProjectiles>();
                if (circlingSkill != null)
                {
                    circlingSkillsList.Add(circlingSkill);
                }
                
                var pushWaveSkill = p.GetComponent<PlayerSkill_PushWave>();
                if (pushWaveSkill != null)
                {
                    pushWaveSkillsList.Add(pushWaveSkill);
                }
                
                var extraHandSkill = p.GetComponent<PlayerSkill_ExtraHand>();
                if (extraHandSkill != null)
                {
                    extraHandSkillsList.Add(extraHandSkill);
                }
            }
        }
        
        circlingProjectilesSkills = circlingSkillsList.ToArray();
        pushWaveSkills = pushWaveSkillsList.ToArray();
        extraHandSkills = extraHandSkillsList.ToArray();
        Debug.Log($"[PlayerUpgradeManager] Found {circlingProjectilesSkills.Length} CirclingProjectiles skills, {pushWaveSkills.Length} PushWave skills, and {extraHandSkills.Length} ExtraHand skills");
        
        // Warn if Player2 is active but has no skills attached
        if (player2s != null && player2s.Length > 0)
        {
            foreach (var p2 in player2s)
            {
                if (p2 != null && p2.gameObject.activeInHierarchy)
                {
                    if (circlingSkillsList.Count == 0 && pushWaveSkillsList.Count == 0 && extraHandSkillsList.Count == 0)
                    {
                        Debug.LogWarning($"[PlayerUpgradeManager] Player2 '{p2.gameObject.name}' has NO skill components attached! Add PlayerSkill_ExtraHand, PlayerSkill_CirclingProjectiles, and PlayerSkill_PushWave components to Player2 in Unity Editor.", p2.gameObject);
                    }
                }
            }
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
            
            // Add skill upgrades for Player2 as well
            // Check if circling projectiles skill hasn't reached max level (10)
            if (circlingProjectilesSkills != null)
            {
                foreach (var skill in circlingProjectilesSkills)
                {
                    if (skill != null && skill.CurrentLevel < 10)
                    {
                        allUpgrades.Add(UpgradeType.UpgradeCirclingProjectiles);
                        break;
                    }
                }
            }
            
            // Check if push wave skill hasn't reached max level (10)
            if (pushWaveSkills != null)
            {
                foreach (var skill in pushWaveSkills)
                {
                    if (skill != null && skill.CurrentLevel < 10)
                    {
                        allUpgrades.Add(UpgradeType.UpgradePushWave);
                        break;
                    }
                }
            }
            
            // Check if extra hand skill hasn't reached max level (10)
            if (extraHandSkills != null)
            {
                foreach (var skill in extraHandSkills)
                {
                    if (skill != null && skill.ExtraHandLevel < 10)
                    {
                        allUpgrades.Add(UpgradeType.UpgradeExtraHand);
                        break;
                    }
                }
            }
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
            
            // Add circling projectiles upgrade by default
            // Check if skill hasn't reached max level (10)
            if (circlingProjectilesSkills != null)
            {
                foreach (var skill in circlingProjectilesSkills)
                {
                    if (skill != null && skill.CurrentLevel < 10)
                    {
                        allUpgrades.Add(UpgradeType.UpgradeCirclingProjectiles);
                        break;
                    }
                }
            }
            
            // Add push wave upgrade by default
            // Check if skill hasn't reached max level (10)
            if (pushWaveSkills != null)
            {
                foreach (var skill in pushWaveSkills)
                {
                    if (skill != null && skill.CurrentLevel < 10)
                    {
                        allUpgrades.Add(UpgradeType.UpgradePushWave);
                        break;
                    }
                }
            }
            
            // Add extra hand upgrade by default
            // Check if skill hasn't reached max level (10)
            if (extraHandSkills != null)
            {
                foreach (var skill in extraHandSkills)
                {
                    if (skill != null && skill.ExtraHandLevel < 10)
                    {
                        allUpgrades.Add(UpgradeType.UpgradeExtraHand);
                        break;
                    }
                }
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
        // Refresh player references in case players were spawned after Awake
        RefreshPlayerReferences();
        
        // Check which character is actually selected and active
        bool hasPlayer2 = IsPlayer2Active();
        bool hasPlayer1 = !hasPlayer2;
        
        Debug.Log($"[PlayerUpgradeManager] Applying upgrade: {upgradeType}, hasPlayer2: {hasPlayer2}, hasPlayer1: {hasPlayer1}");
        
        if (hasPlayer2 && player2Stats == null)
        {
            Debug.LogWarning("[PlayerUpgradeManager] Player2 is active but player2Stats is null!");
            return;
        }
        else if (hasPlayer1 && playerStats == null)
        {
            Debug.LogWarning("[PlayerUpgradeManager] Player1 is active but playerStats is null!");
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
                Debug.Log($"[PlayerUpgradeManager] Heal upgrade selected. hasPlayer2: {hasPlayer2}");
                
                if (hasPlayer2)
                {
                    Debug.Log($"[PlayerUpgradeManager] Healing Player2 instances. Count: {(player2s != null ? player2s.Length : 0)}");
                    if (player2s != null && player2s.Length > 0)
                    {
                        foreach (Player2 p in player2s)
                        {
                            if (p != null && p.gameObject.activeInHierarchy)
                            {
                                var health = p.GetComponent<Entity_Health>();
                                if (health != null)
                                {
                                    float currentHP = health.CurrentHealth;
                                    float maxHP = health.MaxHealth;
                                    health.Heal(maxHP);
                                    Debug.Log($"[PlayerUpgradeManager] Healed {p.name}: {currentHP} -> {health.CurrentHealth} (Max: {maxHP})");
                                }
                                else
                                {
                                    Debug.LogWarning($"[PlayerUpgradeManager] Player2 {p.name} has no Entity_Health component!");
                                }
                            }
                            else if (p != null)
                            {
                                Debug.LogWarning($"[PlayerUpgradeManager] Player2 {p.name} is not active in hierarchy!");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("[PlayerUpgradeManager] No Player2 instances found to heal!");
                    }
                }
                else
                {
                    Debug.Log($"[PlayerUpgradeManager] Healing Player instances. Count: {(players != null ? players.Length : 0)}");
                    if (players != null && players.Length > 0)
                    {
                        foreach (Player p in players)
                        {
                            if (p != null && p.gameObject.activeInHierarchy)
                            {
                                var health = p.GetComponent<Entity_Health>();
                                if (health != null)
                                {
                                    float currentHP = health.CurrentHealth;
                                    float maxHP = health.MaxHealth;
                                    health.Heal(maxHP);
                                    Debug.Log($"[PlayerUpgradeManager] Healed {p.name}: {currentHP} -> {health.CurrentHealth} (Max: {maxHP})");
                                }
                                else
                                {
                                    Debug.LogWarning($"[PlayerUpgradeManager] Player {p.name} has no Entity_Health component!");
                                }
                            }
                            else if (p != null)
                            {
                                Debug.LogWarning($"[PlayerUpgradeManager] Player {p.name} is not active in hierarchy!");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("[PlayerUpgradeManager] No Player instances found to heal!");
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
                
            case UpgradeType.UpgradeCirclingProjectiles:
                // Level-based upgrade that increases all stats
                if (circlingProjectilesSkills != null)
                {
                    foreach (var skill in circlingProjectilesSkills)
                    {
                        if (skill != null)
                        {
                            if (!skill.IsObtained)
                            {
                                // First time: Obtain the skill (Level 1)
                                skill.ObtainSkill();
                                Debug.Log($"[PlayerUpgradeManager] Obtained Circling Projectiles skill! Level: {skill.CurrentLevel}");
                            }
                            else if (skill.CurrentLevel < 10)
                            {
                                // Upgrade to next level (increases all stats)
                                skill.UpgradeLevel();
                                Debug.Log($"[PlayerUpgradeManager] Upgraded Circling Projectiles to Level {skill.CurrentLevel}");
                            }
                        }
                    }
                }
                break;
                
            case UpgradeType.UpgradePushWave:
                // Level-based upgrade that increases all stats
                if (pushWaveSkills != null)
                {
                    foreach (var skill in pushWaveSkills)
                    {
                        if (skill != null)
                        {
                            if (!skill.IsObtained)
                            {
                                // First time: Obtain the skill (Level 1)
                                skill.ObtainSkill();
                                Debug.Log($"[PlayerUpgradeManager] Obtained Push Wave skill! Level: {skill.CurrentLevel}");
                            }
                            else if (skill.CurrentLevel < 10)
                            {
                                // Upgrade to next level (increases all stats)
                                skill.UpgradeLevel();
                                Debug.Log($"[PlayerUpgradeManager] Upgraded Push Wave to Level {skill.CurrentLevel}");
                            }
                        }
                    }
                }
                break;
                
            case UpgradeType.UpgradeExtraHand:
                // Level-based upgrade that increases damage, fire rate, and range
                if (extraHandSkills != null)
                {
                    foreach (var skill in extraHandSkills)
                    {
                        if (skill != null)
                        {
                            if (!skill.IsObtained)
                            {
                                // First time: Obtain the skill (Level 1)
                                skill.ObtainSkill();
                                Debug.Log($"[PlayerUpgradeManager] Obtained Extra Hand skill! Level: {skill.ExtraHandLevel}");
                            }
                            else if (skill.ExtraHandLevel < 10)
                            {
                                // Upgrade to next level (increases damage, reduces interval, increases range)
                                skill.UpgradeLevel(extraHandDamagePerLevel, extraHandIntervalReductionPerLevel, extraHandRangePerLevel);
                                Debug.Log($"[PlayerUpgradeManager] Upgraded Extra Hand to Level {skill.ExtraHandLevel}");
                            }
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
    
    // Push Wave skill getters
    public int GetPushWaveLevel()
    {
        if (pushWaveSkills != null)
        {
            foreach (var skill in pushWaveSkills)
            {
                if (skill != null)
                    return skill.CurrentLevel;
            }
        }
        return 0;
    }
    
    public int GetPushWaveMaxLevel()
    {
        if (pushWaveSkills != null)
        {
            foreach (var skill in pushWaveSkills)
            {
                if (skill != null)
                    return skill.MaxLevel;
            }
        }
        return 10;
    }
    
    public float GetPushWaveRadius()
    {
        if (pushWaveSkills != null)
        {
            foreach (var skill in pushWaveSkills)
            {
                if (skill != null && skill.IsObtained)
                    return skill.CurrentRadius;
            }
        }
        return 0f;
    }
    
    public float GetPushWaveForce()
    {
        if (pushWaveSkills != null)
        {
            foreach (var skill in pushWaveSkills)
            {
                if (skill != null && skill.IsObtained)
                    return skill.CurrentForce;
            }
        }
        return 0f;
    }
    
    public float GetPushWaveDamage()
    {
        if (pushWaveSkills != null)
        {
            foreach (var skill in pushWaveSkills)
            {
                if (skill != null && skill.IsObtained)
                    return skill.CurrentDamage;
            }
        }
        return 0f;
    }
    
    public float GetPushWaveInterval()
    {
        if (pushWaveSkills != null)
        {
            foreach (var skill in pushWaveSkills)
            {
                if (skill != null && skill.IsObtained)
                    return skill.CurrentInterval;
            }
        }
        return 0f;
    }
    
    // Circling Projectiles skill getters
    public int GetCirclingProjectilesLevel()
    {
        if (circlingProjectilesSkills != null)
        {
            foreach (var skill in circlingProjectilesSkills)
            {
                if (skill != null)
                    return skill.CurrentLevel;
            }
        }
        return 0;
    }
    
    public int GetCirclingProjectilesMaxLevel()
    {
        if (circlingProjectilesSkills != null)
        {
            foreach (var skill in circlingProjectilesSkills)
            {
                if (skill != null)
                    return skill.MaxLevel;
            }
        }
        return 10;
    }
    
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
    
    // Extra Hand skill getters
    public int GetExtraHandLevel()
    {
        if (extraHandSkills != null)
        {
            foreach (var skill in extraHandSkills)
            {
                if (skill != null)
                    return skill.ExtraHandLevel;
            }
        }
        return 0;
    }
    
    public int GetExtraHandMaxLevel()
    {
        if (extraHandSkills != null)
        {
            foreach (var skill in extraHandSkills)
            {
                if (skill != null)
                    return skill.MaxLevel;
            }
        }
        return 10;
    }
    
    public float GetExtraHandDamage()
    {
        if (extraHandSkills != null)
        {
            foreach (var skill in extraHandSkills)
            {
                if (skill != null && skill.IsObtained)
                    return skill.CurrentDamage;
            }
        }
        return 0f;
    }
    
    public float GetExtraHandShootInterval()
    {
        if (extraHandSkills != null)
        {
            foreach (var skill in extraHandSkills)
            {
                if (skill != null && skill.IsObtained)
                    return skill.CurrentShootInterval;
            }
        }
        return 0f;
    }
    
    public float GetExtraHandRange()
    {
        if (extraHandSkills != null)
        {
            foreach (var skill in extraHandSkills)
            {
                if (skill != null && skill.IsObtained)
                    return skill.CurrentRange;
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
