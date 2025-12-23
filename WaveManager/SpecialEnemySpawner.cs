using UnityEngine;

/// <summary>
/// Handles spawning of special enemies (Elite, Boss, Special Monsters)
/// </summary>
public class SpecialEnemySpawner
{
    private WaveManager waveManager;
    private EnemySpawnConfig spawnConfig;
    private SpawnPositionCalculator positionCalculator;
    private WavePlayerReferenceManager playerManager;
    private WaveAudioManager audioManager;
    
    // Special enemy tracking
    private bool hasSpawnedElite = false;
    private bool hasSpawnedBoss = false;
    private bool hasSpawnedSpecial = false;
    private int elitesSpawnedThisWave = 0;
    private int specialMonstersSpawnedThisWave = 0;
    private static bool bossSpawnedGlobally = false;
    
    // Wave stats
    private float currentHealthBonus = 0f;
    private float currentDamageBonus = 0f;
    
    /// <summary>
    /// Initialize the special enemy spawner
    /// </summary>
    public void Initialize(WaveManager waveManager, EnemySpawnConfig spawnConfig, 
                          SpawnPositionCalculator positionCalculator, WavePlayerReferenceManager playerManager, 
                          WaveAudioManager audioManager)
    {
        this.waveManager = waveManager;
        this.spawnConfig = spawnConfig;
        this.positionCalculator = positionCalculator;
        this.playerManager = playerManager;
        this.audioManager = audioManager;
    }
    
    /// <summary>
    /// Set current wave bonuses
    /// </summary>
    public void SetWaveBonuses(float healthBonus, float damageBonus)
    {
        currentHealthBonus = healthBonus;
        currentDamageBonus = damageBonus;
    }
    
    /// <summary>
    /// Reset special enemy flags for new wave
    /// </summary>
    public void ResetForNewWave()
    {
        hasSpawnedElite = false;
        hasSpawnedBoss = false;
        hasSpawnedSpecial = false;
        elitesSpawnedThisWave = 0;
        specialMonstersSpawnedThisWave = 0;
        Debug.Log("[SpecialEnemySpawner] Reset flags for new wave");
    }
    
    /// <summary>
    /// Check if an elite should spawn and spawn it
    /// </summary>
    public bool TrySpawnElite(int currentWave)
    {
        bool isEvenWave = currentWave > 0 && currentWave % 2 == 0;
        int eliteCountForWave = 1 + (currentWave / 5);
        
        if (elitesSpawnedThisWave < eliteCountForWave && isEvenWave)
        {
            Debug.Log($"[SpecialEnemySpawner] Attempting to spawn ELITE enemy #{elitesSpawnedThisWave + 1} on wave {currentWave}");
            
            if (SpawnEliteEnemy(currentWave))
            {
                elitesSpawnedThisWave++;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if a special monster should spawn and spawn it
    /// </summary>
    public bool TrySpawnSpecialMonster(int currentWave)
    {
        bool isOddWave = currentWave % 2 == 1;
        bool isBossWave = currentWave % 5 == 0;
        bool isWave5OrHigher = currentWave >= 5;
        int specialCountForWave = 1 + (currentWave / 6);
        
        if (specialMonstersSpawnedThisWave < specialCountForWave && isOddWave && !isBossWave && isWave5OrHigher &&
            spawnConfig.HasSpecialMonsterPrefabs())
        {
            Debug.Log($"[SpecialEnemySpawner] Attempting to spawn SPECIAL MONSTER #{specialMonstersSpawnedThisWave + 1} on wave {currentWave}");
            
            if (SpawnSpecialMonster(currentWave))
            {
                specialMonstersSpawnedThisWave++;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Try to spawn boss if conditions are met
    /// </summary>
    public bool TrySpawnBoss(int currentWave)
    {
        if (currentWave == 5 && !bossSpawnedGlobally && spawnConfig.HasBossPrefabs())
        {
            if (SpawnBoss(currentWave))
            {
                bossSpawnedGlobally = true;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Spawn an elite enemy with enhanced stats
    /// </summary>
    private bool SpawnEliteEnemy(int currentWave)
    {
        Debug.Log($"[SpecialEnemySpawner] ===== SPAWNING ELITE ENEMY - Wave {currentWave} =====");
        
        GameObject eliteEnemyPrefab = spawnConfig.GetRandomEliteEnemy();
        if (eliteEnemyPrefab == null)
        {
            Debug.LogError("[SpecialEnemySpawner] Failed to get elite enemy prefab!");
            return false;
        }
        
        // Get spawn transform
        positionCalculator.GetSpawnTransform(playerManager.GetPlayerTransform(), out Vector3 spawnPosition, out Quaternion spawnRotation);
        
        // Spawn the elite enemy
        GameObject eliteEnemy = Object.Instantiate(eliteEnemyPrefab, spawnPosition, spawnRotation);
        if (eliteEnemy == null)
        {
            Debug.LogError("[SpecialEnemySpawner] Failed to instantiate elite enemy!");
            return false;
        }
        
        // Configure elite enemy
        ConfigureEliteEnemy(eliteEnemy);
        
        // Register spawn
        RegisterEnemySpawn(eliteEnemy);
        
        Debug.Log($"[SpecialEnemySpawner] Spawned ELITE enemy on wave {currentWave} at {spawnPosition}");
        return true;
    }
    
    /// <summary>
    /// Configure elite enemy with enhanced stats and visual effects
    /// </summary>
    private void ConfigureEliteEnemy(GameObject eliteEnemy)
    {
        // Ensure enemy has the "Enemy" tag
        if (!eliteEnemy.CompareTag("Enemy"))
        {
            eliteEnemy.tag = "Enemy";
        }
        
        // Make elite enemy bigger (1.3x scale)
        eliteEnemy.transform.localScale *= 1.3f;
        
        // Apply red visual indicator
        ApplyColorTint(eliteEnemy, new Color(1f, 0.2f, 0.2f), new Color(1f, 0f, 0f) * 0.5f);
        
        // Apply 3x health and damage
        ApplyEliteStats(eliteEnemy);
    }
    
    /// <summary>
    /// Spawn a special monster with unique stats
    /// </summary>
    private bool SpawnSpecialMonster(int currentWave)
    {
        Debug.Log($"[SpecialEnemySpawner] ===== SPAWNING SPECIAL MONSTER - Wave {currentWave} =====");
        
        GameObject specialMonsterPrefab = spawnConfig.GetRandomSpecialMonsterPrefab();
        if (specialMonsterPrefab == null)
        {
            Debug.LogError("[SpecialEnemySpawner] Failed to get special monster prefab!");
            return false;
        }
        
        // Get spawn transform
        positionCalculator.GetSpawnTransform(playerManager.GetPlayerTransform(), out Vector3 spawnPosition, out Quaternion spawnRotation);
        
        // Spawn the special monster
        GameObject specialMonster = Object.Instantiate(specialMonsterPrefab, spawnPosition, spawnRotation);
        if (specialMonster == null)
        {
            Debug.LogError("[SpecialEnemySpawner] Failed to instantiate special monster!");
            return false;
        }
        
        // Configure special monster
        ConfigureSpecialMonster(specialMonster);
        
        // Register spawn
        RegisterEnemySpawn(specialMonster);
        
        Debug.Log($"[SpecialEnemySpawner] Spawned SPECIAL MONSTER on wave {currentWave} at {spawnPosition}");
        return true;
    }
    
    /// <summary>
    /// Configure special monster with enhanced stats
    /// </summary>
    private void ConfigureSpecialMonster(GameObject specialMonster)
    {
        // Ensure special monster has the "Enemy" tag
        if (!specialMonster.CompareTag("Enemy"))
        {
            specialMonster.tag = "Enemy";
        }
        
        // Make special monster bigger (1.5x scale)
        specialMonster.transform.localScale *= 1.5f;
        
        // Apply 2.5x health, damage, and 1.5x speed
        ApplySpecialMonsterStats(specialMonster);
    }
    
    /// <summary>
    /// Spawn a boss enemy
    /// </summary>
    private bool SpawnBoss(int currentWave)
    {
        Debug.Log($"[SpecialEnemySpawner] ===== SPAWNING BOSS - Wave {currentWave} =====");
        
        GameObject bossPrefab = spawnConfig.GetRandomBossPrefab();
        if (bossPrefab == null)
        {
            Debug.LogError("[SpecialEnemySpawner] Failed to get boss prefab!");
            return false;
        }
        
        // Get spawn transform
        positionCalculator.GetSpawnTransform(playerManager.GetPlayerTransform(), out Vector3 spawnPosition, out Quaternion spawnRotation);
        
        // Spawn the boss
        GameObject boss = Object.Instantiate(bossPrefab, spawnPosition, spawnRotation);
        if (boss == null)
        {
            Debug.LogError("[SpecialEnemySpawner] Failed to instantiate boss!");
            return false;
        }
        
        // Configure boss
        ConfigureBoss(boss);
        
        // Register spawn
        RegisterEnemySpawn(boss);
        
        // Play boss spawn sound
        audioManager.PlayBossSpawnSound();
        
        hasSpawnedBoss = true;
        Debug.Log($"[SpecialEnemySpawner] Spawned BOSS '{boss.name}' on wave {currentWave} at {spawnPosition}");
        return true;
    }
    
    /// <summary>
    /// Configure boss enemy
    /// </summary>
    private void ConfigureBoss(GameObject boss)
    {
        // Ensure boss has the "Enemy" tag
        if (!boss.CompareTag("Enemy"))
        {
            boss.tag = "Enemy";
        }
        
        // Apply level-scaled stat bonuses for boss
        ApplyStandardStatBonuses(boss);
        
        Debug.Log($"[SpecialEnemySpawner] Boss '{boss.name}' configured with level scaling");
    }
    
    /// <summary>
    /// Apply color tint to enemy renderers
    /// </summary>
    private void ApplyColorTint(GameObject enemy, Color colorTint, Color emissionColor)
    {
        Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                // Create new material instances to avoid modifying shared materials
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    newMaterials[i] = new Material(renderer.materials[i]);
                    
                    // Apply color tint
                    if (newMaterials[i].HasProperty("_Color"))
                    {
                        Color originalColor = newMaterials[i].GetColor("_Color");
                        newMaterials[i].SetColor("_Color", new Color(
                            originalColor.r * colorTint.r,
                            originalColor.g * colorTint.g,
                            originalColor.b * colorTint.b,
                            originalColor.a));
                    }
                    
                    if (newMaterials[i].HasProperty("_BaseColor"))
                    {
                        Color originalColor = newMaterials[i].GetColor("_BaseColor");
                        newMaterials[i].SetColor("_BaseColor", new Color(
                            originalColor.r * colorTint.r,
                            originalColor.g * colorTint.g,
                            originalColor.b * colorTint.b,
                            originalColor.a));
                    }
                    
                    // Add emission glow
                    if (newMaterials[i].HasProperty("_EmissionColor"))
                    {
                        newMaterials[i].EnableKeyword("_EMISSION");
                        newMaterials[i].SetColor("_EmissionColor", emissionColor);
                    }
                }
                renderer.materials = newMaterials;
            }
        }
    }
    
    /// <summary>
    /// Apply elite enemy stats (3x health and damage)
    /// </summary>
    private void ApplyEliteStats(GameObject enemy)
    {
        // Apply 3x health
        Entity_Health health = enemy.GetComponent<Entity_Health>();
        if (health != null)
        {
            float baseMaxHealth = health.MaxHealth;
            float eliteHealth = baseMaxHealth * 3f + currentHealthBonus;
            health.SetMaxHealth(eliteHealth);
            Debug.Log($"[SpecialEnemySpawner] Elite health set to {eliteHealth}");
        }
        
        // Apply 3x damage
        ApplyDamageMultiplier(enemy, 3f);
    }
    
    /// <summary>
    /// Apply special monster stats (2.5x health, damage, and 1.5x speed)
    /// </summary>
    private void ApplySpecialMonsterStats(GameObject enemy)
    {
        // Get player level for scaling
        int playerLevel = 1;
        if (ExperienceManager.Instance != null)
        {
            playerLevel = ExperienceManager.Instance.GetCurrentLevel();
        }
        
        // Calculate level-based scaling (25% health and 15% damage per level)
        float levelHealthMultiplier = 1f + (0.25f * (playerLevel - 1));
        
        // Apply 2.5x health base multiplier plus level scaling
        Entity_Health health = enemy.GetComponent<Entity_Health>();
        if (health != null)
        {
            float baseMaxHealth = health.MaxHealth;
            float specialHealth = (baseMaxHealth * 2.5f * levelHealthMultiplier) + currentHealthBonus;
            health.SetMaxHealth(specialHealth);
            Debug.Log($"[SpecialEnemySpawner] Special monster health scaled: {baseMaxHealth} -> {specialHealth} (Level {playerLevel} scaling: {levelHealthMultiplier:F2}x)");
        }
        
        // Apply 2.5x damage and 1.5x speed
        ApplyDamageMultiplier(enemy, 2.5f);
        ApplySpeedMultiplier(enemy, 1.5f);
    }
    
    /// <summary>
    /// Apply standard stat bonuses (current wave bonuses only)
    /// </summary>
    private void ApplyStandardStatBonuses(GameObject enemy)
    {
        // Get player level for scaling
        int playerLevel = 1;
        if (ExperienceManager.Instance != null)
        {
            playerLevel = ExperienceManager.Instance.GetCurrentLevel();
        }
        
        // Calculate level-based scaling for bosses (double health multiplier for boss waves)
        float levelHealthMultiplier = 1f + (0.25f * (playerLevel - 1));
        levelHealthMultiplier *= 2f; // Boss wave multiplier
        
        // Apply level scaling to boss health
        Entity_Health health = enemy.GetComponent<Entity_Health>();
        if (health != null)
        {
            float baseMaxHealth = health.MaxHealth;
            float scaledHealth = (baseMaxHealth * levelHealthMultiplier) + currentHealthBonus;
            health.SetMaxHealth(scaledHealth);
            Debug.Log($"[SpecialEnemySpawner] Boss health scaled: {baseMaxHealth} -> {scaledHealth} (Level {playerLevel}, Boss multiplier: {levelHealthMultiplier:F2}x)");
        }
        
        // Apply current wave damage bonus
        if (currentDamageBonus > 0f)
        {
            ApplyDamageBonus(enemy, currentDamageBonus);
        }
    }
    
    /// <summary>
    /// Apply damage multiplier to enemy
    /// </summary>
    private void ApplyDamageMultiplier(GameObject enemy, float multiplier)
    {
        EnemyStatModifier modifier = enemy.GetComponent<EnemyStatModifier>();
        if (modifier == null)
        {
            modifier = enemy.AddComponent<EnemyStatModifier>();
        }
        
        float baseDamage = 10f; // Base damage value
        modifier.damageBonus = (baseDamage * multiplier) + currentDamageBonus;
        Debug.Log($"[SpecialEnemySpawner] Applied damage multiplier {multiplier}x, total bonus: {modifier.damageBonus}");
    }
    
    /// <summary>
    /// Apply damage bonus to enemy
    /// </summary>
    private void ApplyDamageBonus(GameObject enemy, float bonus)
    {
        EnemyStatModifier modifier = enemy.GetComponent<EnemyStatModifier>();
        if (modifier == null)
        {
            modifier = enemy.AddComponent<EnemyStatModifier>();
        }
        
        modifier.damageBonus = bonus;
    }
    
    /// <summary>
    /// Apply speed multiplier to enemy
    /// </summary>
    private void ApplySpeedMultiplier(GameObject enemy, float multiplier)
    {
        EnemyStatModifier modifier = enemy.GetComponent<EnemyStatModifier>();
        if (modifier == null)
        {
            modifier = enemy.AddComponent<EnemyStatModifier>();
        }
        
        modifier.moveSpeedMultiplier = multiplier;
        Debug.Log($"[SpecialEnemySpawner] Applied speed multiplier: {multiplier}x");
    }
    
    /// <summary>
    /// Register enemy spawn with wave manager and add death tracker
    /// </summary>
    private void RegisterEnemySpawn(GameObject enemy)
    {
        // Add enemy death tracker
        EnemyDeathTracker deathTracker = enemy.GetComponent<EnemyDeathTracker>();
        if (deathTracker == null)
        {
            deathTracker = enemy.AddComponent<EnemyDeathTracker>();
        }
        
        // Register with wave manager
        if (waveManager != null)
        {
            waveManager.RegisterEnemySpawned();
        }
    }
    
    /// <summary>
    /// Reset global boss spawn flag (call when starting new game)
    /// </summary>
    public static void ResetGlobalBossFlag()
    {
        bossSpawnedGlobally = false;
        Debug.Log("[SpecialEnemySpawner] Global boss spawn flag reset");
    }
    
    // Getters for tracking
    public bool HasSpawnedElite() => hasSpawnedElite;
    public bool HasSpawnedBoss() => hasSpawnedBoss;
    public bool HasSpawnedSpecial() => hasSpawnedSpecial;
    public int GetElitesSpawnedThisWave() => elitesSpawnedThisWave;
    public int GetSpecialMonstersSpawnedThisWave() => specialMonstersSpawnedThisWave;
    public static bool IsBossSpawnedGlobally() => bossSpawnedGlobally;
}