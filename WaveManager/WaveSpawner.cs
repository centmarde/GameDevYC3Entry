using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Enemy Group Settings")]
    [Tooltip("Define groups of enemies to spawn. Each group can have multiple enemy types with spawn weights.")]
    [SerializeField] private EnemyGroup[] enemyGroups;
    
    [Header("Special Enemy Settings")]
    [Tooltip("Boss enemies to spawn every 5th wave (5, 10, 15, etc.)")]
    [SerializeField] private GameObject[] bossPrefabs;
    [Tooltip("Audio clip to play when a boss spawns")]
    [SerializeField] private AudioClip bossSpawnSound;
    [Tooltip("Volume for boss spawn sound")]
    [SerializeField] [Range(0f, 1f)] private float bossSpawnSoundVolume = 1f;
    
    [Header("Spawn Mode")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.CircularAroundPlayer;
    [Tooltip("Player transform reference (automatically set by WaveManager)")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("If true, continuously searches for player if not found (useful for runtime-spawned players)")]
    [SerializeField] private bool autoFindPlayer = true;
    [Tooltip("How often to refresh player reference (in seconds). Set to 0 to check every spawn.")]
    [SerializeField] private float playerRefreshInterval = 0f;
    
    [Header("Manual Spawn Points (Manual Mode Only)")]
    [SerializeField] private Transform[] spawnPoints; // Array of spawn point transforms
    
    [Header("Circular Spawn Settings (Circular Mode Only)")]
    [SerializeField] private float spawnDistance = 15f; // Distance from player to spawn
    [SerializeField] private int spawnDirections = 8; // Number of spawn directions (N, NE, E, SE, etc.)
    [SerializeField] private bool useRandomOffset = true; // Add random offset to spawn positions
    [SerializeField] private float randomOffsetRange = 3f; // Range for random position offset
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnDelay = 0.5f; // Delay between each enemy spawn
    
    [Header("References")]
    [SerializeField] private WaveManager waveManager;
    
    private int enemiesSpawned = 0;
    private int enemiesToSpawn = 0;
    private bool isSpawning = false;
    private float currentHealthBonus = 0f;
    private float currentDamageBonus = 0f;
    private float currentMoveSpeedMultiplier = 1f;
    private float currentAttackCooldownMultiplier = 1f;
    private EnemyGroup currentWaveGroup = null; // The group selected for this wave
    private float lastPlayerRefreshTime = 0f; // Track when we last refreshed player reference
    private int currentWaveNumber = 0; // Track current wave for special spawning logic
    private bool hasSpawnedElite = false; // Track if elite has spawned this wave
    private bool hasSpawnedBoss = false; // Track if boss has spawned this wave
    private int elitesSpawnedThisWave = 0; // Track how many elites spawned this wave
    private AudioSource audioSource; // Audio source for boss spawn sounds
    private static bool bossSpawnedGlobally = false; // Track if boss has been spawned across all waves
    
    public enum SpawnMode
    {
        Manual,                    // Use manually placed spawn points
        CircularAroundPlayer      // Auto-generate spawn points around player
    }
    
    [System.Serializable]
    public class EnemyGroup
    {
        [Tooltip("Name of this enemy group for organization")]
        public string groupName = "Enemy Group";
        
        [Tooltip("Array of enemy prefabs in this group")]
        public GameObject[] enemies;
        
        [Tooltip("Spawn weights for each enemy (higher = more likely to spawn). Leave empty for equal weights.")]
        public float[] spawnWeights;
        
        /// <summary>
        /// Get a random enemy from this group based on weights
        /// </summary>
        public GameObject GetRandomEnemy()
        {
            if (enemies == null || enemies.Length == 0) return null;
            
            // If no weights specified, use equal probability
            if (spawnWeights == null || spawnWeights.Length != enemies.Length)
            {
                return enemies[Random.Range(0, enemies.Length)];
            }
            
            // Calculate total weight
            float totalWeight = 0f;
            foreach (float weight in spawnWeights)
            {
                totalWeight += weight;
            }
            
            // Pick random value within total weight
            float randomValue = Random.Range(0f, totalWeight);
            
            // Find which enemy this value corresponds to
            float currentWeight = 0f;
            for (int i = 0; i < enemies.Length; i++)
            {
                currentWeight += spawnWeights[i];
                if (randomValue <= currentWeight)
                {
                    return enemies[i];
                }
            }
            
            // Fallback (shouldn't happen)
            return enemies[enemies.Length - 1];
        }
    }
    
    private void Awake()
    {
        // Auto-find WaveManager if not assigned
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
        }
        
        // Try to find player on awake if auto-find is enabled
        if (autoFindPlayer && playerTransform == null)
        {
            RefreshPlayerReference();
        }
        
        // Setup AudioSource for boss spawn sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }
    
    private void Update()
    {
        // Continuously check for player if auto-find is enabled and player is missing
        if (autoFindPlayer && playerTransform == null)
        {
            RefreshPlayerReference();
        }
        // Or refresh periodically if interval is set
        else if (autoFindPlayer && playerRefreshInterval > 0f)
        {
            if (Time.time - lastPlayerRefreshTime >= playerRefreshInterval)
            {
                RefreshPlayerReference();
                lastPlayerRefreshTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// Public method to update player reference (useful if player changes)
    /// </summary>
    public void UpdatePlayerReference(Transform newPlayerTransform)
    {
        playerTransform = newPlayerTransform;
        Debug.Log($"[WaveSpawner] Player reference updated to: {(newPlayerTransform != null ? newPlayerTransform.name : "NULL")}");
    }
    
    /// <summary>
    /// Attempts to re-acquire player reference from WaveManager
    /// </summary>
    private void UpdatePlayerReferenceFromWaveManager()
    {
        if (waveManager != null)
        {
            GameObject player = waveManager.GetActivePlayer();
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log($"[WaveSpawner] Re-acquired player reference from WaveManager: {player.name}");
            }
        }
    }
    
    /// <summary>
    /// Refreshes the player reference by searching for active player in scene
    /// This is called automatically if autoFindPlayer is enabled
    /// </summary>
    private void RefreshPlayerReference()
    {
        // Method 1: Try to get from WaveManager first
        if (waveManager != null)
        {
            GameObject player = waveManager.GetActivePlayer();
            if (player != null && player.activeInHierarchy)
            {
                playerTransform = player.transform;
                return;
            }
        }
        
        // Method 2: Try to find by tag
        GameObject playerByTag = GameObject.FindGameObjectWithTag("Player");
        if (playerByTag != null && playerByTag.activeInHierarchy)
        {
            playerTransform = playerByTag.transform;
            Debug.Log($"[WaveSpawner] Found player by tag: {playerByTag.name} at position {playerTransform.position}");
            return;
        }
        
        // Method 3: Try to find by name
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        
        if (player1 != null && player1.activeInHierarchy)
        {
            playerTransform = player1.transform;
            Debug.Log($"[WaveSpawner] Found Player1 at position {playerTransform.position}");
            return;
        }
        
        if (player2 != null && player2.activeInHierarchy)
        {
            playerTransform = player2.transform;
            Debug.Log($"[WaveSpawner] Found Player2 at position {playerTransform.position}");
            return;
        }
        
        // Method 4: Find by component type
        Player playerComponent = FindObjectOfType<Player>();
        if (playerComponent != null && playerComponent.gameObject.activeInHierarchy)
        {
            playerTransform = playerComponent.transform;
            Debug.Log($"[WaveSpawner] Found player by component: {playerComponent.gameObject.name} at position {playerTransform.position}");
            return;
        }
    }
    
    /// <summary>
    /// Start spawning enemies for the wave
    /// </summary>
    /// <param name="enemyCount">Number of enemies to spawn in this wave</param>
    /// <param name="healthBonus">Health bonus to apply to spawned enemies</param>
    /// <param name="damageBonus">Damage bonus to apply to spawned enemies</param>
    /// <param name="waveNumber">The current wave number (optional, will try to get from WaveManager if not provided)</param>
    /// <param name="moveSpeedMultiplier">Move speed multiplier for minions (1.0 = normal speed)</param>
    /// <param name="attackCooldownMultiplier">Attack cooldown multiplier for minions (1.0 = normal, 0.5 = 2x faster)</param>
    public void StartWave(int enemyCount, float healthBonus = 0f, float damageBonus = 0f, int waveNumber = 0, float moveSpeedMultiplier = 1f, float attackCooldownMultiplier = 1f)
    {
        // Get current wave number - use parameter if provided, otherwise get from WaveManager
        if (waveNumber > 0)
        {
            currentWaveNumber = waveNumber;
            Debug.Log($"[WaveSpawner] StartWave called with wave number parameter: {currentWaveNumber}, Enemy Count: {enemyCount}");
        }
        else if (waveManager != null)
        {
            currentWaveNumber = waveManager.GetCurrentWave();
            Debug.Log($"[WaveSpawner] StartWave called - Got wave number from WaveManager: {currentWaveNumber}, Enemy Count: {enemyCount}");
        }
        else
        {
            Debug.LogWarning("[WaveSpawner] WaveManager is null and no wave number provided! Elite/Boss spawning will not work correctly.");
            currentWaveNumber = 0;
        }
        
        // Reset special spawn flags
        hasSpawnedElite = false;
        hasSpawnedBoss = false;
        elitesSpawnedThisWave = 0;
        Debug.Log($"[WaveSpawner] Reset spawn flags - Elite: {hasSpawnedElite}, Boss: {hasSpawnedBoss}");
        if (isSpawning) return;
        
        currentHealthBonus = healthBonus;
        currentDamageBonus = damageBonus;
        currentMoveSpeedMultiplier = moveSpeedMultiplier;
        currentAttackCooldownMultiplier = attackCooldownMultiplier;
        
        // Validate enemy configuration
        if (enemyGroups == null || enemyGroups.Length == 0)
        {
            Debug.LogError("[WaveSpawner] Cannot start wave - enemyGroups is null or empty!");
            return;
        }
        
        // Select ONE group for this entire wave
        int randomGroupIndex = Random.Range(0, enemyGroups.Length);
        currentWaveGroup = enemyGroups[randomGroupIndex];
        
        // Validate the selected group
        if (currentWaveGroup == null || currentWaveGroup.enemies == null || currentWaveGroup.enemies.Length == 0)
        {
            Debug.LogError($"[WaveSpawner] Selected enemy group {randomGroupIndex} is invalid or has no enemies!");
            return;
        }
        
        Debug.Log($"[WaveSpawner] Starting wave with {enemyCount} enemies from group '{currentWaveGroup.groupName}'");
        
        // Validate spawn mode requirements
        if (spawnMode == SpawnMode.Manual)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[WaveSpawner] Cannot start wave - Manual mode requires spawn points!");
                return;
            }
        }
        else if (spawnMode == SpawnMode.CircularAroundPlayer)
        {
            if (playerTransform == null)
            {
                Debug.LogError("[WaveSpawner] Cannot start wave - Circular mode requires player transform!");
                RefreshPlayerReference();
                if (playerTransform == null)
                {
                    Debug.LogError("[WaveSpawner] Player transform still null after refresh attempt!");
                    return;
                }
            }
        }
        
        enemiesToSpawn = enemyCount;
        enemiesSpawned = 0;
        isSpawning = true;
        
        // Spawn boss ONLY on wave 5 (once per game)
        if (currentWaveNumber == 5 && !bossSpawnedGlobally && bossPrefabs != null && bossPrefabs.Length > 0)
        {
            SpawnBoss();
            bossSpawnedGlobally = true;
        }
        
        InvokeRepeating(nameof(SpawnEnemy), 0f, spawnDelay);
    }
    
    /// <summary>
    /// Spawns a single enemy at a random spawn point.
    /// Uses group system if enabled, otherwise falls back to legacy enemyPrefabs array.
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemiesSpawned >= enemiesToSpawn)
        {
            CancelInvoke(nameof(SpawnEnemy));
            isSpawning = false;
            Debug.Log($"[WaveSpawner] Wave complete - spawned {enemiesSpawned}/{enemiesToSpawn} enemies");
            return;
        }
        
        // Check if we should spawn an elite enemy on even waves (2, 4, 6, etc.)
        // Elite count increases by 1 every 5 waves: Wave 2-4 = 1 elite, Wave 5-9 = 2 elites, Wave 10-14 = 3 elites, etc.
        bool isEvenWave = currentWaveNumber > 0 && currentWaveNumber % 2 == 0;
        int eliteCountForWave = 1 + (currentWaveNumber / 5); // +1 elite every 5 waves
        Debug.Log($"[WaveSpawner] SpawnEnemy check - Wave: {currentWaveNumber}, IsEven: {isEvenWave}, ElitesSpawned: {elitesSpawnedThisWave}/{eliteCountForWave}, EnemiesSpawned: {enemiesSpawned}/{enemiesToSpawn}");
        
        if (elitesSpawnedThisWave < eliteCountForWave && isEvenWave && enemyGroups != null && enemyGroups.Length > 0)
        {
            Debug.Log($"[WaveSpawner] Attempting to spawn ELITE enemy #{elitesSpawnedThisWave + 1} on wave {currentWaveNumber}");
            SpawnEliteEnemy();
            elitesSpawnedThisWave++;
            enemiesSpawned++;
            return;
        }
        
        // Get a random enemy from the current wave's group (using weights)
        GameObject enemyPrefab = null;
        
        if (currentWaveGroup != null)
        {
            enemyPrefab = currentWaveGroup.GetRandomEnemy();
        }
        
        // Critical: If we can't get a prefab, this is a serious error
        if (enemyPrefab == null)
        {
            Debug.LogError($"[WaveSpawner] Failed to get enemy prefab! Group: {(currentWaveGroup != null ? currentWaveGroup.groupName : "NULL")}, Spawned: {enemiesSpawned}/{enemiesToSpawn}");
            
            // Still increment to prevent infinite loop
            enemiesSpawned++;
            
            // If we've failed too many times, stop spawning
            if (enemiesSpawned >= enemiesToSpawn)
            {
                CancelInvoke(nameof(SpawnEnemy));
                isSpawning = false;
                Debug.LogError($"[WaveSpawner] Stopping spawn loop due to repeated failures");
            }
            return;
        }
        
        // Get spawn position based on mode
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        
        if (spawnMode == SpawnMode.Manual)
        {
            // Use manual spawn points
            int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomSpawnIndex];
            spawnPosition = spawnPoint.position;
            spawnRotation = spawnPoint.rotation;
        }
        else // CircularAroundPlayer
        {
            // Calculate spawn position around player's CURRENT position
            spawnPosition = GetCircularSpawnPosition();
            
            // Make enemy face towards player's CURRENT position
            if (playerTransform != null)
            {
                Vector3 directionToPlayer = playerTransform.position - spawnPosition;
                if (directionToPlayer != Vector3.zero)
                {
                    spawnRotation = Quaternion.LookRotation(directionToPlayer);
                }
                else
                {
                    spawnRotation = Quaternion.identity;
                }
            }
            else
            {
                spawnRotation = Quaternion.identity;
            }
        }

        // Spawn the enemy (enemyPrefab null check already done above)
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, spawnRotation);

        

        if (spawnedEnemy == null)
        {
            Debug.LogError($"[WaveSpawner] Instantiate returned null for prefab: {enemyPrefab.name}");
            enemiesSpawned++;
            return;
        }


        // Ensure enemy has the "Enemy" tag
        if (!spawnedEnemy.CompareTag("Enemy"))
        {
            spawnedEnemy.tag = "Enemy";
        }
        
        // Apply stat bonuses
        ApplyStatBonuses(spawnedEnemy);
        
        // Add enemy death tracker component if not present
        EnemyDeathTracker deathTracker = spawnedEnemy.GetComponent<EnemyDeathTracker>();
        if (deathTracker == null)
        {
            deathTracker = spawnedEnemy.AddComponent<EnemyDeathTracker>();
        }
        
        // Register the spawn with wave manager
        if (waveManager != null)
        {
            waveManager.RegisterEnemySpawned();
        }
        
        enemiesSpawned++;
        Debug.Log($"[WaveSpawner] Successfully spawned enemy {enemiesSpawned}/{enemiesToSpawn}: {spawnedEnemy.name} at {spawnPosition}");
    }
    
    /// <summary>
    /// Calculate a spawn position around the player in a circular pattern
    /// Uses the player's CURRENT real-time position
    /// </summary>
    private Vector3 GetCircularSpawnPosition()
    {
        // CRITICAL: Always refresh player reference before spawning if auto-find is enabled
        if (autoFindPlayer && (playerTransform == null || !playerTransform.gameObject.activeInHierarchy))
        {
            RefreshPlayerReference();
        }
        
        // Validate player transform
        if (playerTransform == null)
        {
            Debug.LogError("[WaveSpawner] No player transform available! Cannot calculate spawn position.");
            return Vector3.zero;
        }
        
        // Get the player's CURRENT real-time position (this reads the transform every time!)
        Vector3 currentPlayerPosition = playerTransform.position;
        
        // Pick a random direction (one of the cardinal/diagonal directions)
        int directionIndex = Random.Range(0, spawnDirections);
        float angle = (360f / spawnDirections) * directionIndex;
        
        // Convert angle to radians
        float angleRad = angle * Mathf.Deg2Rad;
        
        // Calculate base position
        Vector3 offset = new Vector3(
            Mathf.Cos(angleRad) * spawnDistance,
            0f,
            Mathf.Sin(angleRad) * spawnDistance
        );
        
        // Add random offset for variation
        if (useRandomOffset)
        {
            offset += new Vector3(
                Random.Range(-randomOffsetRange, randomOffsetRange),
                0f,
                Random.Range(-randomOffsetRange, randomOffsetRange)
            );
        }
        
        // Return world position based on CURRENT player position
        Vector3 spawnPos = currentPlayerPosition + offset;
        
        return spawnPos;
    }
    
    /// <summary>
    /// Apply stat bonuses to a spawned enemy (includes minion-specific scaling)
    /// </summary>
    private void ApplyStatBonuses(GameObject enemyObject)
    {
        bool hasAnyBonus = currentHealthBonus != 0f || currentDamageBonus != 0f || 
                          currentMoveSpeedMultiplier != 1f || currentAttackCooldownMultiplier != 1f;
        if (!hasAnyBonus) return;
        
        // Get the Enemy component
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy == null) return;
        
        // Check if this is a minion
        Enemy_Minions minion = enemyObject.GetComponent<Enemy_Minions>();
        bool isMinion = minion != null;
        
        // Apply health bonus
        if (currentHealthBonus > 0f)
        {
            Entity_Health health = enemyObject.GetComponent<Entity_Health>();
            if (health != null)
            {
                float newMaxHealth = health.MaxHealth + currentHealthBonus;
                health.SetMaxHealth(newMaxHealth); // SetMaxHealth already sets currentHealth to maxHealth
            }
        }
        
        // Get or add EnemyStatModifier component
        EnemyStatModifier modifier = enemyObject.GetComponent<EnemyStatModifier>();
        if (modifier == null && (currentDamageBonus > 0f || isMinion))
        {
            modifier = enemyObject.AddComponent<EnemyStatModifier>();
        }
        
        // Apply damage bonus
        if (currentDamageBonus > 0f && modifier != null)
        {
            modifier.damageBonus = currentDamageBonus;
        }
        
        // Apply minion-specific stat multipliers
        if (isMinion && modifier != null)
        {
            modifier.moveSpeedMultiplier = currentMoveSpeedMultiplier;
            modifier.attackCooldownMultiplier = currentAttackCooldownMultiplier;
            
            Debug.Log($"[WaveSpawner] Applied minion stats to {enemyObject.name}: MoveSpeed×{currentMoveSpeedMultiplier:F2}, AttackCD×{currentAttackCooldownMultiplier:F2}, HP+{currentHealthBonus}, DMG+{currentDamageBonus}");
        }
    }
    
    /// <summary>
    /// Spawn an elite enemy with 3x health and 3x damage
    /// Picks a random enemy from the enemyGroups array
    /// </summary>
    private void SpawnEliteEnemy()
    {
        Debug.Log($"[WaveSpawner] ===== SPAWNING ELITE ENEMY - Wave {currentWaveNumber} =====");
        
        // Validate enemy groups
        if (enemyGroups == null || enemyGroups.Length == 0)
        {
            Debug.LogError("[WaveSpawner] ELITE SPAWN FAILED: No enemy groups available!");
            return;
        }
        
        Debug.Log($"[WaveSpawner] Enemy groups count: {enemyGroups.Length}");
        
        // Pick a random group
        int randomGroupIndex = Random.Range(0, enemyGroups.Length);
        EnemyGroup selectedGroup = enemyGroups[randomGroupIndex];
        
        if (selectedGroup == null || selectedGroup.enemies == null || selectedGroup.enemies.Length == 0)
        {
            Debug.LogError($"[WaveSpawner] ELITE SPAWN FAILED: Selected enemy group {randomGroupIndex} is invalid!");
            return;
        }
        
        Debug.Log($"[WaveSpawner] Selected group: {selectedGroup.groupName}, Enemies in group: {selectedGroup.enemies.Length}");
        
        // Get a random enemy from the selected group
        GameObject eliteEnemyPrefab = selectedGroup.GetRandomEnemy();
        
        if (eliteEnemyPrefab == null)
        {
            Debug.LogError("[WaveSpawner] ELITE SPAWN FAILED: Failed to get random enemy from group!");
            return;
        }
        
        Debug.Log($"[WaveSpawner] Selected elite prefab: {eliteEnemyPrefab.name}");
        
        // Get spawn position and rotation
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        
        if (spawnMode == SpawnMode.Manual)
        {
            int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomSpawnIndex];
            spawnPosition = spawnPoint.position;
            spawnRotation = spawnPoint.rotation;
        }
        else // CircularAroundPlayer
        {
            spawnPosition = GetCircularSpawnPosition();
            if (playerTransform != null)
            {
                Vector3 directionToPlayer = playerTransform.position - spawnPosition;
                if (directionToPlayer != Vector3.zero)
                {
                    spawnRotation = Quaternion.LookRotation(directionToPlayer);
                }
                else
                {
                    spawnRotation = Quaternion.identity;
                }
            }
            else
            {
                spawnRotation = Quaternion.identity;
            }
        }
        
        // Spawn the elite enemy
        GameObject eliteEnemy = Instantiate(eliteEnemyPrefab, spawnPosition, spawnRotation);
        
        if (eliteEnemy == null)
        {
            Debug.LogError("[WaveSpawner] Failed to spawn elite enemy!");
            return;
        }
        
        // Ensure enemy has the "Enemy" tag
        if (!eliteEnemy.CompareTag("Enemy"))
        {
            eliteEnemy.tag = "Enemy";
        }
        
        // Make elite enemy much bigger (2x scale)
        eliteEnemy.transform.localScale *= 1.3f;
        
        // Apply red visual indicator to all renderers (create new material instances to avoid affecting prefab)
        Renderer[] renderers = eliteEnemy.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                // Create new material instances to avoid modifying the shared materials
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    // Create a new instance of the material
                    newMaterials[i] = new Material(renderer.materials[i]);
                    
                    // Apply red tint by modifying the base color
                    if (newMaterials[i].HasProperty("_Color"))
                    {
                        Color originalColor = newMaterials[i].GetColor("_Color");
                        newMaterials[i].SetColor("_Color", new Color(originalColor.r * 1f, originalColor.g * 0.2f, originalColor.b * 0.2f, originalColor.a));
                    }
                    
                    // Try different color properties that Unity shaders might use
                    if (newMaterials[i].HasProperty("_BaseColor"))
                    {
                        Color originalColor = newMaterials[i].GetColor("_BaseColor");
                        newMaterials[i].SetColor("_BaseColor", new Color(originalColor.r * 1f, originalColor.g * 0.2f, originalColor.b * 0.2f, originalColor.a));
                    }
                    
                    // Add emission glow for visibility
                    if (newMaterials[i].HasProperty("_EmissionColor"))
                    {
                        newMaterials[i].EnableKeyword("_EMISSION");
                        newMaterials[i].SetColor("_EmissionColor", new Color(1f, 0f, 0f) * 0.5f);
                    }
                }
                renderer.materials = newMaterials;
            }
        }
        
        // Apply 3x health and 3x damage
        Entity_Health health = eliteEnemy.GetComponent<Entity_Health>();
        if (health != null)
        {
            float baseMaxHealth = health.MaxHealth;
            float eliteHealth = baseMaxHealth * 3f + currentHealthBonus;
            health.SetMaxHealth(eliteHealth);
            Debug.Log($"[WaveSpawner] Elite enemy health set to {eliteHealth} (base: {baseMaxHealth}, 3x multiplier + bonus: {currentHealthBonus})");
        }
        
        // Apply damage bonus
        Enemy_Combat combat = eliteEnemy.GetComponent<Enemy_Combat>();
        if (combat != null)
        {
            EnemyStatModifier modifier = eliteEnemy.GetComponent<EnemyStatModifier>();
            if (modifier == null)
            {
                modifier = eliteEnemy.AddComponent<EnemyStatModifier>();
            }
            // Get base damage and multiply by 3, then add wave bonus
            float baseDamage = 10f; // You may want to read this from the enemy prefab
            modifier.damageBonus = (baseDamage * 3f) + currentDamageBonus;
            Debug.Log($"[WaveSpawner] Elite enemy damage bonus set to {modifier.damageBonus}");
        }
        
        // Add enemy death tracker
        EnemyDeathTracker deathTracker = eliteEnemy.GetComponent<EnemyDeathTracker>();
        if (deathTracker == null)
        {
            deathTracker = eliteEnemy.AddComponent<EnemyDeathTracker>();
        }
        
        // Register with wave manager
        if (waveManager != null)
        {
            waveManager.RegisterEnemySpawned();
        }
        
        Debug.Log($"[WaveSpawner] Spawned ELITE enemy on wave {currentWaveNumber} at {spawnPosition}");
    }
    
    /// <summary>
    /// Spawn a boss enemy (called on waves 5, 10, 15, etc.)
    /// </summary>
    private void SpawnBoss()
    {
        if (bossPrefabs == null || bossPrefabs.Length == 0)
        {
            Debug.LogWarning("[WaveSpawner] Boss prefabs array is empty!");
            return;
        }
        
        // Select a random boss from the array
        GameObject bossPrefab = bossPrefabs[Random.Range(0, bossPrefabs.Length)];
        
        if (bossPrefab == null)
        {
            Debug.LogError("[WaveSpawner] Selected boss prefab is null!");
            return;
        }
        
        // Get spawn position and rotation
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        
        if (spawnMode == SpawnMode.Manual)
        {
            int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomSpawnIndex];
            spawnPosition = spawnPoint.position;
            spawnRotation = spawnPoint.rotation;
        }
        else // CircularAroundPlayer
        {
            spawnPosition = GetCircularSpawnPosition();
            if (playerTransform != null)
            {
                Vector3 directionToPlayer = playerTransform.position - spawnPosition;
                if (directionToPlayer != Vector3.zero)
                {
                    spawnRotation = Quaternion.LookRotation(directionToPlayer);
                }
                else
                {
                    spawnRotation = Quaternion.identity;
                }
            }
            else
            {
                spawnRotation = Quaternion.identity;
            }
        }
        
        // Spawn the boss
        GameObject boss = Instantiate(bossPrefab, spawnPosition, spawnRotation);
        
        if (boss == null)
        {
            Debug.LogError("[WaveSpawner] Failed to spawn boss!");
            return;
        }
        
        // Ensure boss has the "Enemy" tag
        if (!boss.CompareTag("Enemy"))
        {
            boss.tag = "Enemy";
        }
        
        // Apply stat bonuses if any
        ApplyStatBonuses(boss);
        
        // Add enemy death tracker
        EnemyDeathTracker deathTracker = boss.GetComponent<EnemyDeathTracker>();
        if (deathTracker == null)
        {
            deathTracker = boss.AddComponent<EnemyDeathTracker>();
        }
        
        // Register with wave manager
        if (waveManager != null)
        {
            waveManager.RegisterEnemySpawned();
        }
        
        // Play boss spawn sound
        if (audioSource != null && bossSpawnSound != null)
        {
            audioSource.PlayOneShot(bossSpawnSound, bossSpawnSoundVolume);
            Debug.Log($"[WaveSpawner] Playing boss spawn audio: {bossSpawnSound.name}");
        }
        
        hasSpawnedBoss = true;
        Debug.Log($"[WaveSpawner] Spawned BOSS '{boss.name}' on wave {currentWaveNumber} at {spawnPosition}");
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
        currentWaveGroup = null; // Clear the current wave group
    }
    
    /// <summary>
    /// Visualize spawn points in editor
    /// Uses real-time player position if available
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (spawnMode == SpawnMode.CircularAroundPlayer)
        {
            // Try to get real-time player position
            Transform target = playerTransform;
            
            // If no player transform, try to find one
            if (target == null && Application.isPlaying)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    target = playerObj.transform;
                }
            }
            
            // Fallback to this transform
            if (target == null)
            {
                target = transform;
            }
            
            // Draw spawn circle using CURRENT position
            Gizmos.color = Color.yellow;
            DrawCircle(target.position, spawnDistance, 32);
            
            // Draw spawn direction indicators
            Gizmos.color = Color.red;
            for (int i = 0; i < spawnDirections; i++)
            {
                float angle = (360f / spawnDirections) * i * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Vector3 spawnPos = target.position + direction * spawnDistance;
                
                Gizmos.DrawWireSphere(spawnPos, 1f);
                Gizmos.DrawLine(target.position, spawnPos);
            }
            
            // Draw random offset range
            if (useRandomOffset)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                DrawCircle(target.position, spawnDistance + randomOffsetRange, 32);
                DrawCircle(target.position, spawnDistance - randomOffsetRange, 32);
            }
        }
        else if (spawnMode == SpawnMode.Manual && spawnPoints != null)
        {
            // Draw manual spawn points
            Gizmos.color = Color.green;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 1f);
                }
            }
        }
    }
    
    /// <summary>
    /// Helper method to draw a circle in the editor
    /// </summary>
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
    
    /// <summary>
    /// Reset spawner to initial state for fresh game start
    /// </summary>
    public void ResetSpawner()
    {
        // Reset boss spawn flag
        bossSpawnedGlobally = false;
        Debug.Log($"[WaveSpawner] Resetting spawner '{gameObject.name}' to initial state...");
        
        // Stop any spawning coroutines
        StopAllCoroutines();
        
        // Reset spawn state
        enemiesSpawned = 0;
        enemiesToSpawn = 0;
        isSpawning = false;
        
        // Reset bonuses
        currentHealthBonus = 0f;
        currentDamageBonus = 0f;
        
        // Clear current wave group
        currentWaveGroup = null;
        
        // Reset player reference tracking
        lastPlayerRefreshTime = 0f;
        playerTransform = null; // Will be reassigned when new player spawns
        
        // Reset wave tracking and special spawn flags
        currentWaveNumber = 0;
        hasSpawnedElite = false;
        hasSpawnedBoss = false;
        elitesSpawnedThisWave = 0;
        
        Debug.Log($"[WaveSpawner] Spawner '{gameObject.name}' reset complete");
    }
}
