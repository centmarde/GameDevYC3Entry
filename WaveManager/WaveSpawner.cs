using UnityEngine;

/// <summary>
/// Main wave spawner coordinator that manages enemy spawning using modular components
/// </summary>
public class WaveSpawner : MonoBehaviour
{
    [Header("Modular Components")]
    [SerializeField] private EnemySpawnConfig spawnConfig = new EnemySpawnConfig();
    [SerializeField] private SpawnPositionCalculator positionCalculator = new SpawnPositionCalculator();
    [SerializeField] private WavePlayerReferenceManager playerManager = new WavePlayerReferenceManager();
    [SerializeField] private WaveAudioManager audioManager = new WaveAudioManager();
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnDelay = 0.5f;
    
    [Header("Boss Wave Settings")]
    [Tooltip("Boss waves occur every N waves (e.g., 5 = waves 5, 10, 15, etc.)")]
    [SerializeField] private int bossWaveInterval = 5;
    
    [Tooltip("Multiplier for enemy count on boss waves")]
    [SerializeField] private float bossWaveCountMultiplier = 2f;
    
    [Header("Level Scaling Settings")]
    [Tooltip("Health multiplier per player level (e.g., 0.25 = 25% more health per level)")]
    [SerializeField] private float healthPerLevel = 0.25f;
    
    [Tooltip("Damage multiplier per player level")]
    [SerializeField] private float damagePerLevel = 0.15f;
    
    [Tooltip("Boss wave health multiplier (applied on wave 5, 10, 15, etc.)")]
    [SerializeField] private float bossWaveHealthMultiplier = 2f;
    
    [Header("References")]
    [SerializeField] private WaveManager waveManager;
    
    // Core spawning state
    private int enemiesSpawned = 0;
    private int enemiesToSpawn = 0;
    private bool isSpawning = false;
    private int currentWaveNumber = 0;
    
    // Component managers
    private SpecialEnemySpawner specialSpawner;
    private EnemyStatManager statManager;

    
    private void Awake()
    {
        // Auto-find WaveManager if not assigned
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
        }
        
        // Initialize modular components
        InitializeComponents();
    }
    
    /// <summary>
    /// Initialize all modular components
    /// </summary>
    private void InitializeComponents()
    {
        // Initialize player reference manager
        playerManager.Initialize(waveManager);
        
        // Initialize audio manager
        audioManager.Initialize(gameObject);
        
        // Initialize stat manager
        statManager = new EnemyStatManager();
        
        // Initialize special enemy spawner
        specialSpawner = new SpecialEnemySpawner();
        specialSpawner.Initialize(waveManager, spawnConfig, positionCalculator, playerManager, audioManager);
        
        Debug.Log("[WaveSpawner] All modular components initialized");
    }
    
    private void Update()
    {
        // Update player reference management
        playerManager.UpdatePlayerReference();
    }
    
    /// <summary>
    /// Public method to update player reference (useful if player changes)
    /// </summary>
    public void UpdatePlayerReference(Transform newPlayerTransform)
    {
        playerManager.SetPlayerReference(newPlayerTransform);
    }
    
    /// <summary>
    /// Start spawning enemies for the wave
    /// </summary>
    public void StartWave(int enemyCount, float healthBonus = 0f, float damageBonus = 0f, int waveNumber = 0, float moveSpeedMultiplier = 1f, float attackCooldownMultiplier = 1f)
    {
        if (isSpawning) 
        {
            Debug.LogWarning("[WaveSpawner] Already spawning, ignoring StartWave call");
            return;
        }
        
        // Ensure any previous invocations are cancelled
        CancelInvoke(nameof(SpawnEnemy));
        
        // Get current wave number
        currentWaveNumber = GetCurrentWaveNumber(waveNumber);
        
        // Check if this is a boss wave and apply multiplier
        bool isBossWave = (currentWaveNumber % bossWaveInterval == 0);
        if (isBossWave)
        {
            enemyCount = Mathf.RoundToInt(enemyCount * bossWaveCountMultiplier);
            Debug.Log($"[WaveSpawner] BOSS WAVE {currentWaveNumber}! Spawning {enemyCount} enemies (doubled)");
        }
        else
        {
            Debug.Log($"[WaveSpawner] Starting wave {currentWaveNumber} with {enemyCount} enemies");
        }
        
        // Validate enemy count to prevent infinite loops
        if (enemyCount <= 0)
        {
            Debug.LogError("[WaveSpawner] Invalid enemy count: " + enemyCount);
            return;
        }
        
        if (enemyCount > 1000)
        {
            Debug.LogError("[WaveSpawner] Enemy count too high (>1000): " + enemyCount + ". This could cause performance issues.");
            return;
        }
        
        // Initialize components for new wave
        InitializeWave(healthBonus, damageBonus, moveSpeedMultiplier, attackCooldownMultiplier, enemyCount);
        
        // Validate configuration
        if (!ValidateWaveConfiguration()) 
        {
            // Reset spawning state on validation failure
            isSpawning = false;
            return;
        }
        
        // Try to spawn boss first if conditions are met
        specialSpawner.TrySpawnBoss(currentWaveNumber);
        
        // Start spawning regular enemies with minimum delay protection
        float safeSpawnDelay = Mathf.Max(spawnDelay, 0.1f); // Minimum 0.1s delay
        InvokeRepeating(nameof(SpawnEnemy), 0f, safeSpawnDelay);
    }
    
    /// <summary>
    /// Get current wave number from parameter or WaveManager
    /// </summary>
    private int GetCurrentWaveNumber(int waveNumber)
    {
        if (waveNumber > 0)
        {
            return waveNumber;
        }
        else if (waveManager != null)
        {
            return waveManager.GetCurrentWave();
        }
        else
        {
            Debug.LogWarning("[WaveSpawner] WaveManager is null and no wave number provided! Special spawning will not work correctly.");
            return 0;
        }
    }
    
    /// <summary>
    /// Initialize wave components and settings
    /// </summary>
    private void InitializeWave(float healthBonus, float damageBonus, float moveSpeedMultiplier, float attackCooldownMultiplier, int enemyCount)
    {
        // Set stat bonuses
        statManager.SetWaveBonuses(healthBonus, damageBonus, moveSpeedMultiplier, attackCooldownMultiplier);
        specialSpawner.SetWaveBonuses(healthBonus, damageBonus);
        
        // Reset special spawner flags
        specialSpawner.ResetForNewWave();
        
        // Select enemy group for this wave
        spawnConfig.SelectWaveGroup();
        
        // Set spawn counts
        enemiesToSpawn = enemyCount;
        enemiesSpawned = 0;
        isSpawning = true;
    }
    
    /// <summary>
    /// Validate wave configuration before starting
    /// </summary>
    private bool ValidateWaveConfiguration()
    {
        // Validate enemy configuration
        if (!spawnConfig.IsValid())
        {
            Debug.LogError("[WaveSpawner] Cannot start wave - enemy configuration is invalid!");
            return false;
        }
        
        // Ensure player reference is valid
        if (!playerManager.EnsurePlayerReference())
        {
            Debug.LogError("[WaveSpawner] Cannot start wave - no valid player reference found!");
            return false;
        }
        
        // Validate spawn mode requirements
        if (!positionCalculator.ValidateSpawnMode(playerManager.GetPlayerTransform()))
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Spawns a single enemy (regular or special based on wave conditions)
    /// </summary>
    private void SpawnEnemy()
    {
        // Safety check to prevent runaway spawning
        if (!isSpawning)
        {
            CancelInvoke(nameof(SpawnEnemy));
            Debug.LogWarning("[WaveSpawner] SpawnEnemy called but not in spawning state, cancelling");
            return;
        }
        
        if (enemiesSpawned >= enemiesToSpawn)
        {
            CancelInvoke(nameof(SpawnEnemy));
            isSpawning = false;
            Debug.Log($"[WaveSpawner] Wave complete - spawned {enemiesSpawned}/{enemiesToSpawn} enemies");
            return;
        }
        
        // Safety check for spawn overflow
        if (enemiesSpawned > enemiesToSpawn * 2)
        {
            CancelInvoke(nameof(SpawnEnemy));
            isSpawning = false;
            Debug.LogError($"[WaveSpawner] Emergency stop - spawned too many enemies: {enemiesSpawned}/{enemiesToSpawn}");
            return;
        }
        
        // Try to spawn special enemies first
        if (TrySpawnSpecialEnemies()) return;
        
        // Spawn regular enemy
        SpawnRegularEnemy();
    }
    
    /// <summary>
    /// Try to spawn special enemies (elite or special monsters)
    /// </summary>
    private bool TrySpawnSpecialEnemies()
    {
        // Try to spawn elite enemy
        if (specialSpawner.TrySpawnElite(currentWaveNumber))
        {
            enemiesSpawned++;
            return true;
        }
        
        // Try to spawn special monster
        if (specialSpawner.TrySpawnSpecialMonster(currentWaveNumber))
        {
            enemiesSpawned++;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Spawn a regular enemy from the current wave group
    /// </summary>
    private void SpawnRegularEnemy()
    {
        // Get enemy prefab from current wave group
        GameObject enemyPrefab = spawnConfig.GetRandomEnemyFromCurrentGroup();
        if (enemyPrefab == null)
        {
            Debug.LogError($"[WaveSpawner] Failed to get enemy prefab from current group!");
            HandleSpawnFailure();
            return;
        }
        
        // Get spawn transform
        positionCalculator.GetSpawnTransform(playerManager.GetPlayerTransform(), out Vector3 spawnPosition, out Quaternion spawnRotation);
        
        // Spawn the enemy
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, spawnRotation);
        if (spawnedEnemy == null)
        {
            Debug.LogError($"[WaveSpawner] Instantiate returned null for prefab: {enemyPrefab.name}");
            HandleSpawnFailure();
            return;
        }
        
        // Configure the spawned enemy
        ConfigureSpawnedEnemy(spawnedEnemy);
        
        enemiesSpawned++;
        Debug.Log($"[WaveSpawner] Successfully spawned regular enemy {enemiesSpawned}/{enemiesToSpawn}: {spawnedEnemy.name} at {spawnPosition}");
    }
    
    /// <summary>
    /// Configure a newly spawned enemy
    /// </summary>
    private void ConfigureSpawnedEnemy(GameObject spawnedEnemy)
    {
        // Ensure enemy has the "Enemy" tag
        if (!spawnedEnemy.CompareTag("Enemy"))
        {
            spawnedEnemy.tag = "Enemy";
        }
        
        // Check if this is a boss wave
        bool isBossWave = (currentWaveNumber % bossWaveInterval == 0);
        
        // Get player level for scaling
        int playerLevel = 1;
        if (ExperienceManager.Instance != null)
        {
            playerLevel = ExperienceManager.Instance.GetCurrentLevel();
        }
        
        // Apply level-scaled stat bonuses
        ApplyLevelScaledBonuses(spawnedEnemy, playerLevel, isBossWave);
        
        // Apply regular stat bonuses
        statManager.ApplyStatBonuses(spawnedEnemy);
        
        // Add enemy death tracker
        EnemyDeathTracker deathTracker = spawnedEnemy.GetComponent<EnemyDeathTracker>();
        if (deathTracker == null)
        {
            deathTracker = spawnedEnemy.AddComponent<EnemyDeathTracker>();
        }
        
        // Register with wave manager
        if (waveManager != null)
        {
            waveManager.RegisterEnemySpawned();
        }
    }
    
    /// <summary>
    /// Apply level-based scaling to enemy health and stats
    /// </summary>
    private void ApplyLevelScaledBonuses(GameObject enemy, int playerLevel, bool isBossWave)
    {
        if (enemy == null) return;
        
        Entity_Health health = enemy.GetComponent<Entity_Health>();
        if (health == null) return;
        
        // Calculate level scaling
        float levelHealthMultiplier = 1f + (healthPerLevel * (playerLevel - 1));
        
        // Apply boss wave multiplier
        if (isBossWave)
        {
            levelHealthMultiplier *= bossWaveHealthMultiplier;
        }
        
        // Scale the enemy's health
        float baseHealth = health.MaxHealth;
        float scaledHealth = baseHealth * levelHealthMultiplier;
        
        health.SetMaxHealth(scaledHealth);
        
        Debug.Log($"[WaveSpawner] Scaled {enemy.name} health: {baseHealth:F0} -> {scaledHealth:F0} (Level {playerLevel}, Boss Wave: {isBossWave})");
    }
    
    /// <summary>
    /// Handle spawn failure (increment counter and check if should stop)
    /// </summary>
    private void HandleSpawnFailure()
    {
        enemiesSpawned++;
        
        if (enemiesSpawned >= enemiesToSpawn)
        {
            CancelInvoke(nameof(SpawnEnemy));
            isSpawning = false;
            Debug.LogError($"[WaveSpawner] Stopping spawn loop due to repeated failures");
        }
    }

    
    /// <summary>
    /// Check if the spawner is currently spawning enemies
    /// </summary>
    public bool IsSpawning()
    {
        return isSpawning;
    }
    
    /// <summary>
    /// Stop spawning immediately
    /// </summary>
    public void StopSpawning()
    {
        CancelInvoke(nameof(SpawnEnemy));
        isSpawning = false;
        spawnConfig.ResetWaveGroup(); // Clear the current wave group
        
        Debug.Log($"[WaveSpawner] Spawning stopped. Final count: {enemiesSpawned}/{enemiesToSpawn}");
    }
    
    /// <summary>
    /// Visualize spawn points in editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Try to get player transform for visualization
        Transform target = null;
        
        if (Application.isPlaying)
        {
            target = playerManager.GetPlayerTransform();
        }
        
        // If no player found, try to find one by tag
        if (target == null && Application.isPlaying)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }
        
        // Draw spawn gizmos using position calculator
        positionCalculator.DrawSpawnGizmos(target, transform);
    }
    

    
    /// <summary>
    /// Reset spawner to initial state for fresh game start
    /// </summary>
    public void ResetSpawner()
    {
        Debug.Log($"[WaveSpawner] Resetting spawner '{gameObject.name}' to initial state...");
        
        // Reset global boss spawn flag
        SpecialEnemySpawner.ResetGlobalBossFlag();
        
        // Stop any spawning coroutines
        StopAllCoroutines();
        
        // Reset spawn state
        enemiesSpawned = 0;
        enemiesToSpawn = 0;
        isSpawning = false;
        currentWaveNumber = 0;
        
        // Reset modular components
        spawnConfig.ResetWaveGroup();
        playerManager.ResetPlayerReference();
        statManager.ResetBonuses();
        audioManager.Reset();
        
        Debug.Log($"[WaveSpawner] Spawner '{gameObject.name}' reset complete");
    }
    
    // Public accessors for modular components (for inspector and external access)
    
    /// <summary>
    /// Get the enemy spawn configuration component
    /// </summary>
    public EnemySpawnConfig GetSpawnConfig() => spawnConfig;
    
    /// <summary>
    /// Get the spawn position calculator component
    /// </summary>
    public SpawnPositionCalculator GetPositionCalculator() => positionCalculator;
    
    /// <summary>
    /// Get the player reference manager component
    /// </summary>
    public WavePlayerReferenceManager GetPlayerManager() => playerManager;
    
    /// <summary>
    /// Get the wave audio manager component
    /// </summary>
    public WaveAudioManager GetAudioManager() => audioManager;
    
    /// <summary>
    /// Get the special enemy spawner component
    /// </summary>
    public SpecialEnemySpawner GetSpecialSpawner() => specialSpawner;
    
    /// <summary>
    /// Get the enemy stat manager component
    /// </summary>
    public EnemyStatManager GetStatManager() => statManager;
    
    /// <summary>
    /// Get current wave number
    /// </summary>
    public int GetCurrentWaveNumber() => currentWaveNumber;
    
    /// <summary>
    /// Get spawn progress (enemies spawned / total enemies)
    /// </summary>
    public (int spawned, int total) GetSpawnProgress() => (enemiesSpawned, enemiesToSpawn);
    
    /// <summary>
    /// Check if current wave is a boss wave
    /// </summary>
    public bool IsBossWave() => (currentWaveNumber % bossWaveInterval == 0);
    
    /// <summary>
    /// Get the player's current level for UI display
    /// </summary>
    public int GetPlayerLevel()
    {
        return ExperienceManager.Instance != null ? ExperienceManager.Instance.GetCurrentLevel() : 1;
    }
    
    /// <summary>
    /// Get the next boss wave number
    /// </summary>
    public int GetNextBossWave()
    {
        int nextBossWave = ((currentWaveNumber / bossWaveInterval) + 1) * bossWaveInterval;
        return nextBossWave;
    }
    
    /// <summary>
    /// Get level scaling information for UI
    /// </summary>
    public (float healthMultiplier, float damageMultiplier, bool isBossWave) GetCurrentScalingInfo()
    {
        int playerLevel = GetPlayerLevel();
        bool isBossWave = IsBossWave();
        
        float healthMultiplier = 1f + (healthPerLevel * (playerLevel - 1));
        if (isBossWave) healthMultiplier *= bossWaveHealthMultiplier;
        
        float damageMultiplier = 1f + (damagePerLevel * (playerLevel - 1));
        
        return (healthMultiplier, damageMultiplier, isBossWave);
    }
    
    /// <summary>
    /// Emergency cleanup method to stop all spawning operations
    /// Use this if you encounter thread group or infinite spawning issues
    /// </summary>
    public void EmergencyStopAll()
    {
        Debug.LogWarning("[WaveSpawner] EMERGENCY STOP - Cleaning up all spawning operations");
        
        // Cancel all invocations
        CancelInvoke();
        StopAllCoroutines();
        
        // Reset all state
        isSpawning = false;
        enemiesSpawned = 0;
        enemiesToSpawn = 0;
        currentWaveNumber = 0;
        
        // Reset components
        spawnConfig.ResetWaveGroup();
        statManager.ResetBonuses();
        
        Debug.Log("[WaveSpawner] Emergency stop completed");
    }
}
