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
    [SerializeField] private int baseEnemyCount = 5; // Base number of enemies in first wave
    [SerializeField] private int enemyIncreasePerWave = 3; // How many more enemies per wave
    [SerializeField] private float timeBetweenWaves = 10f; // Time between waves
    
    [Header("Stat Scaling")]
    [SerializeField] private bool useStatScaling = true;
    [SerializeField] private int waveIntervalForScaling = 5; // Every X waves, stats increase
    [SerializeField] private float healthIncreasePerInterval = 10f; // Health increase per interval
    [SerializeField] private float damageIncreasePerInterval = 10f; // Damage increase per interval
    
    [Header("Dynamic Wave Configuration")]
    [SerializeField] private bool useDynamicScaling = true;
    [Tooltip("If enabled, enemy count scales: baseEnemyCount + (currentWave - 1) * enemyIncreasePerWave")]
    [SerializeField] private AnimationCurve difficultyScaling; // Optional curve for non-linear scaling
    
    [Header("References")]
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private WaveUI waveUI;
    
    [Header("Wave Events")]
    public UnityEvent<int> OnWaveStart; // Triggered when a wave starts (passes wave number)
    public UnityEvent<int> OnWaveComplete; // Triggered when a wave completes (passes wave number)
    public UnityEvent<int> OnAllEnemiesCleared; // Triggered when all enemies are defeated (passes wave number)
    
    private int currentWave = 0;
    private int enemiesInCurrentWave = 0;
    private int enemiesAlive = 0;
    private bool waveInProgress = false;
    private bool spawningComplete = false;
    private bool allEnemiesCleared = false;
    private float timeSinceWaveEnd = 0f;
    
    // Stat scaling tracking
    private float currentHealthBonus = 0f;
    private float currentDamageBonus = 0f;
    
    private void Awake()
    {
        // Find references if not assigned
        if (waveSpawner == null)
            waveSpawner = GetComponent<WaveSpawner>();
        
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
    }
    
    private void Start()
    {
        // Start first wave after a short delay (only if auto-start is enabled)
        if (autoStartWaves && wavesActivated)
        {
            Invoke(nameof(StartNextWave), 2f);
        }
    }
    
    private void Update()
    {
        // Don't update if waves are paused
        if (wavesPaused) return;
        
        // Check if spawning is complete but wave is still in progress (enemies still alive)
        if (waveInProgress && spawningComplete && !allEnemiesCleared)
        {
            CheckEnemiesAlive();
        }
        
        // If all enemies cleared and wave ended, count down to next wave
        if (!waveInProgress && allEnemiesCleared && currentWave > 0 && wavesActivated)
        {
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
        
        enemiesInCurrentWave = CalculateEnemyCount(currentWave);
        enemiesAlive = 0;
        waveInProgress = true;
        spawningComplete = false;
        allEnemiesCleared = false;
        timeSinceWaveEnd = 0f;
        
        // Notify UI
        if (waveUI != null)
        {
            waveUI.ShowWaveAnnouncement(currentWave);
        }
        
        // Trigger event
        OnWaveStart?.Invoke(currentWave);
        
        // Start spawning with current stat bonuses
        if (waveSpawner != null)
        {
            waveSpawner.StartWave(enemiesInCurrentWave, currentHealthBonus, currentDamageBonus);
            // Check for spawning completion
            Invoke(nameof(CheckSpawningCompletion), 1f);
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
                return Mathf.RoundToInt(linearCount * multiplier);
            }
            
            return linearCount;
        }
        else
        {
            // Simple: just return base count
            return baseEnemyCount;
        }
    }
    
    /// <summary>
    /// Check if all enemies have been spawned
    /// </summary>
    private void CheckSpawningCompletion()
    {
        if (waveSpawner != null && !waveSpawner.IsSpawning())
        {
            spawningComplete = true;
            OnWaveComplete?.Invoke(currentWave);
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
        waveInProgress = false;
        
        // Trigger event
        OnAllEnemiesCleared?.Invoke(currentWave);
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
        if (waveSpawner != null)
        {
            waveSpawner.StopSpawning();
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
        if (waveSpawner != null && waveSpawner.IsSpawning())
        {
            waveSpawner.StopSpawning();
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
}
