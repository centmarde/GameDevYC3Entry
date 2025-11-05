using UnityEngine;
using UnityEngine.Events;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Activation")]
    [SerializeField] private bool autoStartWaves = true;
    [Tooltip("If false, waves will only start when StartWavesManually() is called or trigger is activated")]
    [SerializeField] private bool requireTriggerActivation = false;
    [Tooltip("If true, player must collide with an object to start waves")]
    [SerializeField] private bool wavesActivated = false;
    [Tooltip("Pauses wave spawning and progression when true")]
    [SerializeField] private bool wavesPaused = false;
    
    [Header("Wave Settings")]
    [SerializeField] private int startingWave = 1;
    [SerializeField] private bool waitForLevelUpToAdvance = true; // Wait for player to level up before next wave
    [SerializeField] private bool unlimitedSpawning = true; // Spawn unlimited enemies per wave
    [SerializeField] private int minEnemiesAlive = 10; // Minimum enemies to maintain (when below, spawn more)
    [SerializeField] private int spawnBatchSize = 5; // How many enemies to spawn per batch
    [SerializeField] private float spawnCheckInterval = 2f; // How often to check if we need to spawn more
    [SerializeField] private int baseEnemyCount = 5; // Base number of enemies in first wave (used for initial spawn)
    [SerializeField] private int enemyIncreasePerWave = 3; // How many more enemies per wave
    [SerializeField] private float timeBetweenWaves = 10f; // Time between waves (only used if not waiting for level up)
    
    [Header("Stat Scaling")]
    [SerializeField] private bool useStatScaling = true;
    [SerializeField] private int waveIntervalForScaling = 5; // Every X waves, stats increase
    [SerializeField] private float healthIncreasePerInterval = 10f; // Health increase per interval
    [SerializeField] private float damageIncreasePerInterval = 10f; // Damage increase per interval
    
    [Header("Minion Stat Scaling")]
    [Tooltip("Enable additional stat scaling for minion enemies")]
    [SerializeField] private bool useMinionStatScaling = true;
    [Tooltip("Move speed increase per scaling interval (percentage, 0.1 = 10% faster)")]
    [SerializeField] private float moveSpeedIncreasePerInterval = 0.05f; // 5% faster per interval
    [Tooltip("Attack cooldown reduction per interval (percentage, 0.1 = 10% faster attacks)")]
    [SerializeField] private float attackCooldownReductionPerInterval = 0.05f; // 5% faster per interval
    
    [Header("Dynamic Wave Configuration")]
    [SerializeField] private bool useDynamicScaling = true;
    [Tooltip("If enabled, enemy count scales: baseEnemyCount + (currentWave - 1) * enemyIncreasePerWave")]
    [SerializeField] private AnimationCurve difficultyScaling; // Optional curve for non-linear scaling
    
    [Header("References")]
    [SerializeField] private WaveSpawner[] waveSpawners;
    [SerializeField] private WaveUI waveUI;
    
    [Header("Audio Settings")]
    [Tooltip("Array of sounds to play randomly when a wave starts")]
    [SerializeField] private AudioClip[] waveStartSounds;
    [Tooltip("Volume for the wave start sound")]
    [SerializeField] [Range(0f, 1f)] private float waveStartSoundVolume = 0.7f;
    
    [Header("Player Detection")]
    [Tooltip("If enabled, automatically finds the active player (Player1 or Player2)")]
    [SerializeField] private bool autoDetectPlayer = true;
    [Tooltip("Manually assign a player if auto-detect is disabled")]
    [SerializeField] private GameObject manualPlayerReference;
    
    [Header("Wave Events")]
    public UnityEvent<int> OnWaveStart; // Triggered when a wave starts (passes wave number)
    public UnityEvent<int> OnWaveComplete; // Triggered when a wave completes (passes wave number)
    public UnityEvent<int> OnAllEnemiesCleared; // Triggered when all enemies are defeated (passes wave number)
    
    private int currentWave = 0;
    private GameObject activePlayer;
    private int enemiesInCurrentWave = 0;
    private int enemiesAlive = 0;
    private bool waveInProgress = false;
    private bool spawningComplete = false;
    private bool allEnemiesCleared = false;
    private float timeSinceWaveEnd = 0f;
    private bool waitingForLevelUp = false;
    private int playerLevelAtWaveStart = 0;
    private float lastSpawnCheckTime = 0f;
    private bool levelUpSubscribed = false;
    
    // Stat scaling tracking
    private float currentHealthBonus = 0f;
    private float currentDamageBonus = 0f;
    private float currentMoveSpeedMultiplier = 1f;
    private float currentAttackCooldownMultiplier = 1f;
    
    // Audio
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Find references if not assigned (but don't fail if not found yet)
        if (waveSpawners == null || waveSpawners.Length == 0)
        {
            WaveSpawner spawner = GetComponent<WaveSpawner>();
            if (spawner != null)
            {
                waveSpawners = new WaveSpawner[] { spawner };
            }
            else
            {
                // Only find active spawners (false = excludeInactive)
                WaveSpawner[] foundSpawners = FindObjectsOfType<WaveSpawner>(false);
                if (foundSpawners != null && foundSpawners.Length > 0)
                {
                    waveSpawners = foundSpawners;
                }
            }
            
            if (waveSpawners == null || waveSpawners.Length == 0)
            {
                Debug.LogWarning("[WaveManager] No active spawners found in Awake - will search again when player spawns");
            }
            else
            {
                Debug.Log($"[WaveManager] Found {waveSpawners.Length} spawner(s) in Awake");
            }
        }
        
        if (waveUI == null)
            waveUI = FindObjectOfType<WaveUI>();
        
        // Initialize difficulty curve if not set
        if (difficultyScaling == null || difficultyScaling.length == 0)
        {
            difficultyScaling = AnimationCurve.Linear(1, 1, 10, 3);
        }
        
        currentWave = startingWave - 1; // Will increment to startingWave on first wave
        allEnemiesCleared = true; // Allow first wave to start
        
        // Set initial activation state
        if (autoStartWaves && !requireTriggerActivation)
        {
            wavesActivated = true;
        }
        
        // Setup AudioSource for wave sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound for wave announcements
        
        // Detect active player
        DetectActivePlayer();
        
        // Subscribe to level up event if waiting for level up is enabled
        if (waitForLevelUpToAdvance && ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnLevelUp += HandlePlayerLevelUp;
        }
    }
    
    private void Start()
    {
        // Re-check player detection in Start (in case PlayerSpawnManager runs after Awake)
        if (activePlayer == null)
        {
            DetectActivePlayer();
        }
        
        // Refresh spawner search after player is spawned
        if ((waveSpawners == null || waveSpawners.Length == 0) && activePlayer != null)
        {
            Debug.Log("[WaveManager] Searching for spawners after player spawn...");
            RefreshSpawnerReferences();
        }
        
        // Pass player reference to all WaveSpawners
        if (activePlayer != null && waveSpawners != null && waveSpawners.Length > 0)
        {
            foreach (WaveSpawner spawner in waveSpawners)
            {
                if (spawner != null)
                {
                    spawner.UpdatePlayerReference(activePlayer.transform);
                }
            }
            Debug.Log($"[WaveManager] Passed player reference to {waveSpawners.Length} WaveSpawner(s): {activePlayer.name}");
        }
        
        // Start first wave after a short delay (only if auto-start is enabled)
        if (autoStartWaves && wavesActivated)
        {
            Invoke(nameof(StartNextWave), 2f);
        }

        // Ensure we subscribe to ExperienceManager.OnLevelUp even if ExperienceManager wasn't ready in Awake
        if (waitForLevelUpToAdvance && !levelUpSubscribed)
        {
            if (ExperienceManager.Instance != null)
            {
                ExperienceManager.Instance.OnLevelUp += HandlePlayerLevelUp;
                levelUpSubscribed = true;
                Debug.Log("[WaveManager] Subscribed to ExperienceManager.OnLevelUp in Start");
            }
            else
            {
                // Start a coroutine to wait for ExperienceManager to become available
                StartCoroutine(SubscribeWhenExperienceReady());
            }
        }
    }

    private System.Collections.IEnumerator SubscribeWhenExperienceReady()
    {
        while (ExperienceManager.Instance == null)
        {
            yield return null;
        }

        if (!levelUpSubscribed)
        {
            ExperienceManager.Instance.OnLevelUp += HandlePlayerLevelUp;
            levelUpSubscribed = true;
            Debug.Log("[WaveManager] Subscribed to ExperienceManager.OnLevelUp (late)");
        }
    }
    
    /// <summary>
    /// Detects which player is active (Player1 or Player2) based on character selection
    /// </summary>
    private void DetectActivePlayer()
    {
        if (!autoDetectPlayer)
        {
            activePlayer = manualPlayerReference;
            if (activePlayer != null)
            {
                Debug.Log($"[WaveManager] Using manually assigned player: {activePlayer.name}");
            }
            return;
        }
        
        // Method 1: Try to get from CharacterSelectionManager and PlayerSpawnManager
        PlayerSpawnManager spawnManager = FindObjectOfType<PlayerSpawnManager>();
        if (spawnManager != null)
        {
            activePlayer = spawnManager.GetActivePlayer();
            if (activePlayer != null)
            {
                Debug.Log($"[WaveManager] Detected active player from PlayerSpawnManager: {activePlayer.name}");
                return;
            }
        }
        
        // Method 2: Look for Player1 or Player2 in scene
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        
        // Check which one exists
        if (player1 != null && player2 == null)
        {
            activePlayer = player1;
            Debug.Log("[WaveManager] Detected Player1 as active player");
        }
        else if (player2 != null && player1 == null)
        {
            activePlayer = player2;
            Debug.Log("[WaveManager] Detected Player2 as active player");
        }
        else if (player1 != null && player2 != null)
        {
            // Both exist - check which one is active
            if (player1.activeInHierarchy && !player2.activeInHierarchy)
            {
                activePlayer = player1;
                Debug.Log("[WaveManager] Both players exist, Player1 is active");
            }
            else if (player2.activeInHierarchy && !player1.activeInHierarchy)
            {
                activePlayer = player2;
                Debug.Log("[WaveManager] Both players exist, Player2 is active");
            }
            else
            {
                // Both active, use CharacterSelectionManager to decide
                if (CharacterSelectionManager.Instance != null)
                {
                    int selectedIndex = CharacterSelectionManager.Instance.SelectedCharacterIndex;
                    activePlayer = selectedIndex == 0 ? player1 : player2;
                    Debug.Log($"[WaveManager] Both players active, using selection: {activePlayer.name}");
                }
                else
                {
                    // Default to player1
                    activePlayer = player1;
                    Debug.LogWarning("[WaveManager] Both players active, defaulting to Player1");
                }
            }
        }
        
        // Method 3: Try finding by component type
        if (activePlayer == null)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                activePlayer = player.gameObject;
                Debug.Log($"[WaveManager] Detected player by component: {activePlayer.name}");
            }
        }
        
        // Final fallback
        if (activePlayer == null)
        {
            Debug.LogWarning("[WaveManager] Could not detect active player! Waves may not function correctly.");
        }
    }
    
    private void Update()
    {
        // Don't update if waves are paused
        if (wavesPaused) return;
        
        // Check if we need to spawn more enemies (unlimited spawning mode)
        if (waveInProgress && unlimitedSpawning && !waitingForLevelUp)
        {
            if (Time.time - lastSpawnCheckTime >= spawnCheckInterval)
            {
                lastSpawnCheckTime = Time.time;
                CheckAndSpawnMoreEnemies();
            }
        }
        
        // Check if spawning is complete but wave is still in progress (enemies still alive)
        if (waveInProgress && spawningComplete && !allEnemiesCleared)
        {
            CheckEnemiesAlive();
        }
        
        // If waiting for level up, check if player has leveled up
        if (waitingForLevelUp && waitForLevelUpToAdvance)
        {
            // Level up event will handle wave progression
            return;
        }
        
        // If all enemies cleared and wave ended, count down to next wave
        if (!waveInProgress && allEnemiesCleared && currentWave > 0 && wavesActivated)
        {
            if (waitForLevelUpToAdvance)
            {
                // Don't auto-advance, wait for level up
                return;
            }
            
            timeSinceWaveEnd += Time.deltaTime;
            
            if (timeSinceWaveEnd >= timeBetweenWaves)
            {
                StartNextWave();
            }
        }
    }
    
    /// <summary>
    /// Start the next wave
    /// </summary>
    public void StartNextWave()
    {
        // Don't start if paused or not activated
        if (wavesPaused)
        {
            return;
        }
        
        if (!wavesActivated)
        {
            return;
        }
        
        // Don't start new wave if current wave is in progress or enemies still alive
        if (waveInProgress || !allEnemiesCleared) 
        {
            return;
        }
        
        currentWave++;
        
        // Update stat bonuses based on wave number
        if (useStatScaling)
        {
            UpdateStatBonuses();
        }
        
        // Store player's current level at wave start
        if (ExperienceManager.Instance != null)
        {
            playerLevelAtWaveStart = ExperienceManager.Instance.GetCurrentLevel();
        }
        
        enemiesInCurrentWave = CalculateEnemyCount(currentWave);
        Debug.Log($"[WaveManager] Wave {currentWave} starting - Initial enemies: {enemiesInCurrentWave}, Base: {baseEnemyCount}, Unlimited: {unlimitedSpawning}, WaitForLevelUp: {waitForLevelUpToAdvance}, PlayerLevel: {playerLevelAtWaveStart}");
        
        enemiesAlive = 0;
        waveInProgress = true;
        spawningComplete = !unlimitedSpawning; // If unlimited, spawning never "completes"
        allEnemiesCleared = false;
        timeSinceWaveEnd = 0f;
        waitingForLevelUp = false;
        lastSpawnCheckTime = Time.time;
        
        // Notify UI
        if (waveUI != null)
        {
            waveUI.ShowWaveAnnouncement(currentWave);
        }
        
        // Play wave start sound
        PlayWaveStartSound();
        
        // Trigger event
        OnWaveStart?.Invoke(currentWave);
        
        // Refresh spawners if not found or all null/inactive
        if (waveSpawners == null || waveSpawners.Length == 0)
        {
            Debug.LogWarning("[WaveManager] No spawners assigned, searching now...");
            RefreshSpawnerReferences();
        }
        
        // Start spawning with current stat bonuses across all spawners
        if (waveSpawners != null && waveSpawners.Length > 0)
        {
            // Count active spawners (check enabled component, not just hierarchy)
            int activeSpawnerCount = 0;
            foreach (WaveSpawner spawner in waveSpawners)
            {
                if (spawner != null && spawner.enabled && spawner.gameObject.activeInHierarchy)
                {
                    activeSpawnerCount++;
                }
            }
            
            if (activeSpawnerCount == 0)
            {
                Debug.Log($"[WaveManager] No active spawners found! Spawners array length: {waveSpawners.Length}");
                
                // Debug each spawner
                for (int i = 0; i < waveSpawners.Length; i++)
                {
                    WaveSpawner spawner = waveSpawners[i];
                    if (spawner == null)
                    {
                        Debug.Log($"  Spawner[{i}]: NULL");
                    }
                    else
                    {
                        Debug.Log($"  Spawner[{i}]: {spawner.gameObject.name} - Active: {spawner.gameObject.activeInHierarchy}, Enabled: {spawner.enabled}");
                    }
                }
                
                // Try to refresh spawners one more time
                Debug.Log("[WaveManager] Attempting to refresh spawner references...");
                RefreshSpawnerReferences();
                
                // Recount after refresh
                activeSpawnerCount = 0;
                if (waveSpawners != null)
                {
                    foreach (WaveSpawner spawner in waveSpawners)
                    {
                        if (spawner != null && spawner.enabled && spawner.gameObject.activeInHierarchy)
                        {
                            activeSpawnerCount++;
                        }
                    }
                }
                
                if (activeSpawnerCount == 0)
                {
                    Debug.LogError($"[WaveManager] Still no active spawners after refresh. Cannot start wave.");
                    return;
                }
                else
                {
                    Debug.Log($"[WaveManager] Found {activeSpawnerCount} active spawner(s) after refresh!");
                }
            }
            
            // Distribute enemies evenly across spawners
            int enemiesPerSpawner = enemiesInCurrentWave / activeSpawnerCount;
            int remainderEnemies = enemiesInCurrentWave % activeSpawnerCount;
            
            Debug.Log($"[WaveManager] Distributing {enemiesInCurrentWave} enemies across {activeSpawnerCount} spawner(s): {enemiesPerSpawner} each + {remainderEnemies} remainder");
            
            int spawnerIndex = 0;
            foreach (WaveSpawner spawner in waveSpawners)
            {
                if (spawner != null && spawner.enabled && spawner.gameObject.activeInHierarchy)
                {
                    // Give extra enemy to first spawners if there's a remainder
                    int enemiesToGive = enemiesPerSpawner + (spawnerIndex < remainderEnemies ? 1 : 0);
                    
                    Debug.Log($"[WaveManager] Starting spawner {spawnerIndex} ({spawner.gameObject.name}) with {enemiesToGive} enemies (HP+{currentHealthBonus}, DMG+{currentDamageBonus}, SPD×{currentMoveSpeedMultiplier:F2}, ATK×{currentAttackCooldownMultiplier:F2}) on wave {currentWave}");
                    spawner.StartWave(enemiesToGive, currentHealthBonus, currentDamageBonus, currentWave, currentMoveSpeedMultiplier, currentAttackCooldownMultiplier);
                    spawnerIndex++;
                }
            }
            
            // Check for spawning completion
            Invoke(nameof(CheckSpawningCompletion), 1f);
        }
        else
        {
            Debug.LogError($"[WaveManager] No spawners available! Cannot start wave.");
        }
    }
    
    /// <summary>
    /// Calculate how many enemies should spawn this wave
    /// </summary>
    private int CalculateEnemyCount(int wave)
    {
        if (useDynamicScaling)
        {
            // Linear scaling: baseEnemyCount + (wave - 1) * enemyIncreasePerWave
            int linearCount = baseEnemyCount + (wave - 1) * enemyIncreasePerWave;
            
            // Optional: Apply curve multiplier for non-linear scaling
            if (difficultyScaling != null && difficultyScaling.length > 0)
            {
                float multiplier = difficultyScaling.Evaluate(wave);
                int finalCount = Mathf.RoundToInt(linearCount * multiplier);
                Debug.Log($"[WaveManager] Enemy count calculation - Wave: {wave}, Linear: {linearCount}, Multiplier: {multiplier:F2}, Final: {finalCount}");
                return finalCount;
            }
            
            Debug.Log($"[WaveManager] Enemy count calculation - Wave: {wave}, Linear (no curve): {linearCount}");
            return linearCount;
        }
        else
        {
            // Simple: just return base count
            Debug.Log($"[WaveManager] Enemy count calculation - Wave: {wave}, Static: {baseEnemyCount} (scaling disabled)");
            return baseEnemyCount;
        }
    }
    
    /// <summary>
    /// Check if all enemies have been spawned
    /// </summary>
    private void CheckSpawningCompletion()
    {
        bool allSpawnersComplete = true;
        
        if (waveSpawners != null && waveSpawners.Length > 0)
        {
            foreach (WaveSpawner spawner in waveSpawners)
            {
                if (spawner != null && spawner.IsSpawning())
                {
                    allSpawnersComplete = false;
                    break;
                }
            }
        }
        
        if (allSpawnersComplete)
        {
            spawningComplete = true;
            OnWaveComplete?.Invoke(currentWave);
            
            // Notify Supabase leaderboard manager
            if (SupabaseLeaderboardManager.Instance != null)
            {
                // This is handled automatically by OnWaveComplete listener in SupabaseLeaderboardManager
                Debug.Log("[WaveManager] Wave complete notification sent to SupabaseLeaderboardManager");
            }
        }
        else
        {
            // Keep checking
            Invoke(nameof(CheckSpawningCompletion), 1f);
        }
    }
    
    /// <summary>
    /// Check how many enemies are still alive in the scene
    /// </summary>
    private void CheckEnemiesAlive()
    {
        // Find all enemies with the "Enemy" tag or component
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemiesAlive = enemies.Length;
        
        // If no enemies left, wave is cleared
        if (enemiesAlive == 0)
        {
            AllEnemiesCleared();
        }
    }
    
    /// <summary>
    /// Called when all enemies from the wave are cleared
    /// </summary>
    private void AllEnemiesCleared()
    {
        if (allEnemiesCleared) return; // Prevent multiple calls
        
        allEnemiesCleared = true;
        
        // Trigger event
        OnAllEnemiesCleared?.Invoke(currentWave);
        
        // Note: We don't end the wave or wait here anymore
        // Wave only ends when player levels up (handled in HandlePlayerLevelUp)
    }
    
    /// <summary>
    /// Handle player level up event
    /// </summary>
    private void HandlePlayerLevelUp(int newLevel)
    {
        Debug.Log($"[WaveManager] Player leveled up to {newLevel}!");
        
        // Advance wave if player leveled up since wave started (don't need to wait for enemies cleared)
        if (waveInProgress && newLevel > playerLevelAtWaveStart)
        {
            Debug.Log($"[WaveManager] Player leveled up from {playerLevelAtWaveStart} to {newLevel}. Starting next wave!");
            
            // Don't clear enemies - let them stack from previous waves
            
            // End current wave and start next
            waveInProgress = false;
            allEnemiesCleared = true;
            waitingForLevelUp = false;
            
            StartNextWave();
        }
    }
    
    /// <summary>
    /// Check if we need to spawn more enemies and spawn them if needed
    /// </summary>
    private void CheckAndSpawnMoreEnemies()
    {
        if (!waveInProgress || !unlimitedSpawning) return;
        
        // Count current enemies
        CheckEnemiesAlive();
        
        // If below minimum threshold, spawn more
        if (enemiesAlive < minEnemiesAlive)
        {
            int enemiesToSpawn = spawnBatchSize;
            Debug.Log($"[WaveManager] Enemy count ({enemiesAlive}) below minimum ({minEnemiesAlive}). Spawning {enemiesToSpawn} more enemies...");
            
            // Spawn through all spawners
            if (waveSpawners != null && waveSpawners.Length > 0)
            {
                int activeSpawnerCount = 0;
                foreach (WaveSpawner spawner in waveSpawners)
                {
                    if (spawner != null && spawner.enabled && spawner.gameObject.activeInHierarchy)
                    {
                        activeSpawnerCount++;
                    }
                }
                
                if (activeSpawnerCount > 0)
                {
                    int enemiesPerSpawner = enemiesToSpawn / activeSpawnerCount;
                    int remainderEnemies = enemiesToSpawn % activeSpawnerCount;
                    
                    int spawnerIndex = 0;
                    foreach (WaveSpawner spawner in waveSpawners)
                    {
                        if (spawner != null && spawner.enabled && spawner.gameObject.activeInHierarchy)
                        {
                            int count = enemiesPerSpawner + (spawnerIndex < remainderEnemies ? 1 : 0);
                            spawner.StartWave(count, currentHealthBonus, currentDamageBonus, currentWave, currentMoveSpeedMultiplier, currentAttackCooldownMultiplier);
                            spawnerIndex++;
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Clear all remaining enemies from the scene
    /// </summary>
    private void ClearRemainingEnemies()
    {
        GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        int clearedCount = remainingEnemies.Length;
        
        foreach (GameObject enemy in remainingEnemies)
        {
            Destroy(enemy);
        }
        
        enemiesAlive = 0;
        Debug.Log($"[WaveManager] Cleared {clearedCount} remaining enemies from previous wave");
    }
    
    /// <summary>
    /// Register an enemy spawn (call this from enemy spawn logic)
    /// </summary>
    public void RegisterEnemySpawned()
    {
        enemiesAlive++;
    }
    
    /// <summary>
    /// Register an enemy death (call this from enemy death logic)
    /// </summary>
    public void RegisterEnemyKilled()
    {
        enemiesAlive--;
        
        // Also notify Supabase leaderboard manager
        if (SupabaseLeaderboardManager.Instance != null)
        {
            SupabaseLeaderboardManager.Instance.RegisterKill();
        }
        
        if (enemiesAlive <= 0 && spawningComplete)
        {
            AllEnemiesCleared();
        }
    }
    
    /// <summary>
    /// Force start next wave (for testing or manual triggers) - USE WITH CAUTION
    /// </summary>
    public void ForceNextWave()
    {
        if (waveSpawners != null && waveSpawners.Length > 0)
        {
            foreach (WaveSpawner spawner in waveSpawners)
            {
                if (spawner != null)
                {
                    spawner.StopSpawning();
                }
            }
        }
        
        // Clear all remaining enemies
        GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in remainingEnemies)
        {
            Destroy(enemy);
        }
        
        waveInProgress = false;
        allEnemiesCleared = true;
        enemiesAlive = 0;
        
        StartNextWave();
    }
    
    /// <summary>
    /// Update stat bonuses based on current wave
    /// </summary>
    private void UpdateStatBonuses()
    {
        // Calculate how many scaling intervals have passed
        // Waves 1-4 = 0 intervals (no bonus)
        // Wave 5 = 1 interval (+10 HP, +10 DMG)
        // Wave 10 = 2 intervals (+20 HP, +20 DMG), etc.
        int intervals = currentWave / waveIntervalForScaling;
        
        currentHealthBonus = intervals * healthIncreasePerInterval;
        currentDamageBonus = intervals * damageIncreasePerInterval;
        
        // Update minion-specific stat multipliers
        if (useMinionStatScaling)
        {
            // Move speed: 1.0 + (intervals * 0.05) = 1.0, 1.05, 1.10, etc.
            currentMoveSpeedMultiplier = 1f + (intervals * moveSpeedIncreasePerInterval);
            
            // Attack cooldown: 1.0 - (intervals * 0.05) = 1.0, 0.95, 0.90, etc. (faster attacks)
            // Clamp to minimum of 0.5 (can't go faster than 50% original cooldown)
            currentAttackCooldownMultiplier = Mathf.Max(0.5f, 1f - (intervals * attackCooldownReductionPerInterval));
            
            Debug.Log($"[WaveManager] Wave {currentWave} stat scaling - Intervals: {intervals}, HP+{currentHealthBonus}, DMG+{currentDamageBonus}, MoveSpeed×{currentMoveSpeedMultiplier:F2}, AttackCD×{currentAttackCooldownMultiplier:F2}");
        }
    }
    
    // Getters for UI and other systems
    public int GetCurrentWave() => currentWave;
    public int GetEnemiesInCurrentWave() => enemiesInCurrentWave;
    public int GetEnemiesAlive() => enemiesAlive;
    public bool IsWaveInProgress() => waveInProgress;
    public bool IsSpawningComplete() => spawningComplete;
    public bool AreAllEnemiesCleared() => allEnemiesCleared;
    public float GetTimeUntilNextWave() => Mathf.Max(0, timeBetweenWaves - timeSinceWaveEnd);
    public bool AreWavesActivated() => wavesActivated;
    public bool AreWavesPaused() => wavesPaused;
    public float GetCurrentHealthBonus() => currentHealthBonus;
    public float GetCurrentDamageBonus() => currentDamageBonus;
    public float GetCurrentMoveSpeedMultiplier() => currentMoveSpeedMultiplier;
    public float GetCurrentAttackCooldownMultiplier() => currentAttackCooldownMultiplier;
    public GameObject GetActivePlayer() => activePlayer;
    
    /// <summary>
    /// Manually set the active player (useful for custom setups)
    /// </summary>
    public void SetActivePlayer(GameObject player)
    {
        activePlayer = player;
        Debug.Log($"[WaveManager] Active player manually set to: {(player != null ? player.name : "null")}");
        
        // Refresh spawners when player is set
        if (player != null)
        {
            RefreshSpawnerReferences();
        }
    }
    
    /// <summary>
    /// Force re-detection of active player
    /// </summary>
    public void RefreshPlayerDetection()
    {
        DetectActivePlayer();
    }
    
    /// <summary>
    /// Activate the wave system (call this from trigger or manually)
    /// </summary>
    public void ActivateWaves()
    {
        if (wavesActivated)
        {
            return;
        }
        
        wavesActivated = true;
        
        // Start first wave immediately
        if (!waveInProgress && allEnemiesCleared)
        {
            Invoke(nameof(StartNextWave), 1f);
        }
    }
    
    /// <summary>
    /// Deactivate the wave system
    /// </summary>
    public void DeactivateWaves()
    {
        wavesActivated = false;
    }
    
    /// <summary>
    /// Pause wave progression and spawning
    /// </summary>
    public void PauseWaves()
    {
        wavesPaused = true;
        
        // Pause spawning if in progress
        if (waveSpawners != null && waveSpawners.Length > 0)
        {
            foreach (WaveSpawner spawner in waveSpawners)
            {
                if (spawner != null && spawner.IsSpawning())
                {
                    spawner.StopSpawning();
                }
            }
        }
    }
    
    /// <summary>
    /// Resume wave progression and spawning
    /// </summary>
    public void ResumeWaves()
    {
        wavesPaused = false;
    }
    
    /// <summary>
    /// Toggle pause state
    /// </summary>
    public void TogglePause()
    {
        if (wavesPaused)
            ResumeWaves();
        else
            PauseWaves();
    }
    
    /// <summary>
    /// Refresh the spawner references (useful when player spawns at runtime)
    /// </summary>
    public void RefreshSpawnerReferences()
    {
        // Method 1: Try to find on active player if we have one
        if (activePlayer != null && activePlayer.activeInHierarchy)
        {
            // Search only in active player's children (includeInactive = false)
            WaveSpawner[] playerSpawners = activePlayer.GetComponentsInChildren<WaveSpawner>(false);
            if (playerSpawners != null && playerSpawners.Length > 0)
            {
                waveSpawners = playerSpawners;
                Debug.Log($"[WaveManager] Found {waveSpawners.Length} spawner(s) on active player: {activePlayer.name}");
                return;
            }
        }
        
        // Method 2: Search on this WaveManager object
        WaveSpawner spawner = GetComponent<WaveSpawner>();
        if (spawner != null && spawner.gameObject.activeInHierarchy)
        {
            waveSpawners = new WaveSpawner[] { spawner };
            Debug.Log($"[WaveManager] Found spawner on WaveManager object");
            return;
        }
        
        // Method 3: Search scene for ONLY active spawners
        WaveSpawner[] allSpawners = FindObjectsOfType<WaveSpawner>(false); // false = only active
        if (allSpawners != null && allSpawners.Length > 0)
        {
            // Filter to only include spawners on active GameObjects
            System.Collections.Generic.List<WaveSpawner> activeSpawners = new System.Collections.Generic.List<WaveSpawner>();
            foreach (WaveSpawner s in allSpawners)
            {
                if (s != null && s.gameObject.activeInHierarchy && s.enabled)
                {
                    activeSpawners.Add(s);
                }
            }
            
            if (activeSpawners.Count > 0)
            {
                waveSpawners = activeSpawners.ToArray();
                Debug.Log($"[WaveManager] Found {waveSpawners.Length} active spawner(s) in scene");
            }
            else
            {
                Debug.LogWarning($"[WaveManager] Found {allSpawners.Length} spawner(s) but none are active/enabled");
                waveSpawners = new WaveSpawner[0];
            }
        }
        else
        {
            Debug.LogWarning($"[WaveManager] No spawners found in scene at all");
            waveSpawners = new WaveSpawner[0];
        }
        
        // Pass player reference to newly found spawners
        if (activePlayer != null && waveSpawners != null && waveSpawners.Length > 0)
        {
            foreach (WaveSpawner spawner2 in waveSpawners)
            {
                if (spawner2 != null)
                {
                    spawner2.UpdatePlayerReference(activePlayer.transform);
                }
            }
        }
    }
    
    /// <summary>
    /// Update the player reference dynamically
    /// </summary>
    public void UpdatePlayerReference(GameObject newPlayer)
    {
        activePlayer = newPlayer;
        
        // Also update all WaveSpawners' references
        if (waveSpawners != null && waveSpawners.Length > 0 && newPlayer != null)
        {
            foreach (WaveSpawner spawner in waveSpawners)
            {
                if (spawner != null)
                {
                    spawner.UpdatePlayerReference(newPlayer.transform);
                }
            }
            Debug.Log($"[WaveManager] Updated {waveSpawners.Length} WaveSpawner(s) player reference to: {newPlayer.name}");
        }
        
        Debug.Log($"[WaveManager] Player reference updated to: {newPlayer.name}");
    }
    
    /// <summary>
    /// Plays a random wave start sound effect from the array
    /// </summary>
    private void PlayWaveStartSound()
    {
        if (audioSource != null && waveStartSounds != null && waveStartSounds.Length > 0)
        {
            // Pick a random sound from the array
            AudioClip randomSound = waveStartSounds[Random.Range(0, waveStartSounds.Length)];
            if (randomSound != null)
            {
                audioSource.PlayOneShot(randomSound, waveStartSoundVolume);
            }
        }
    }
    
    /// <summary>
    /// Cleanup subscriptions
    /// </summary>
    private void OnDestroy()
    {
        if (waitForLevelUpToAdvance && ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnLevelUp -= HandlePlayerLevelUp;
        }
    }
    
    /// <summary>
    /// Reset WaveManager to initial state for fresh game start
    /// Called when quitting to main menu or retrying
    /// </summary>
    public void ResetWaveManager()
    {
        Debug.Log("[WaveManager] Resetting WaveManager to initial state...");
        
        // Reset wave state
        currentWave = startingWave;
        enemiesInCurrentWave = 0;
        enemiesAlive = 0;
        waveInProgress = false;
        spawningComplete = false;
        allEnemiesCleared = false;
        timeSinceWaveEnd = 0f;
        
        // Reset stat bonuses
        currentHealthBonus = 0f;
        currentDamageBonus = 0f;
        currentMoveSpeedMultiplier = 1f;
        currentAttackCooldownMultiplier = 1f;
        
        // Reset activation state
        wavesActivated = false;
        wavesPaused = false;
        
        // Clear player reference
        activePlayer = null;
        
        // Reset spawners
        if (waveSpawners != null)
        {
            foreach (WaveSpawner spawner in waveSpawners)
            {
                if (spawner != null)
                {
                    spawner.ResetSpawner();
                }
            }
        }
        
        // Reset UI
        if (waveUI != null)
        {
            waveUI.ResetWaveUI();
        }
        
        // Stop any audio
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        Debug.Log("[WaveManager] WaveManager reset complete");
    }
}
