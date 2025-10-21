using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Enemy Group Settings")]
    [Tooltip("Define groups of enemies to spawn. Each group can have multiple enemy types with spawn weights.")]
    [SerializeField] private EnemyGroup[] enemyGroups;
    
    [Header("Spawn Mode")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.CircularAroundPlayer;
    [Tooltip("Player transform reference (automatically set by WaveManager)")]
    [SerializeField] private Transform playerTransform;
    
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
    private EnemyGroup currentWaveGroup = null; // The group selected for this wave
    
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
    }
    
    /// <summary>
    /// Public method to update player reference (useful if player changes)
    /// </summary>
    public void UpdatePlayerReference(Transform newPlayerTransform)
    {
        playerTransform = newPlayerTransform;
        Debug.Log($"[WaveSpawner] Player reference updated to: {newPlayerTransform.name}");
    }
    
    /// <summary>
    /// Start spawning enemies for the wave
    /// </summary>
    /// <param name="enemyCount">Number of enemies to spawn in this wave</param>
    /// <param name="healthBonus">Health bonus to apply to spawned enemies</param>
    /// <param name="damageBonus">Damage bonus to apply to spawned enemies</param>
    public void StartWave(int enemyCount, float healthBonus = 0f, float damageBonus = 0f)
    {
        if (isSpawning) return;
        
        currentHealthBonus = healthBonus;
        currentDamageBonus = damageBonus;
        
        // Validate enemy configuration
        if (enemyGroups == null || enemyGroups.Length == 0)
        {
            return;
        }
        
        // Select ONE group for this entire wave
        int randomGroupIndex = Random.Range(0, enemyGroups.Length);
        currentWaveGroup = enemyGroups[randomGroupIndex];
        
        // Validate spawn mode requirements
        if (spawnMode == SpawnMode.Manual)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return;
            }
        }
        else if (spawnMode == SpawnMode.CircularAroundPlayer)
        {
            if (playerTransform == null)
            {
                return;
            }
        }
        
        enemiesToSpawn = enemyCount;
        enemiesSpawned = 0;
        isSpawning = true;
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
            return;
        }
        
        // Get a random enemy from the current wave's group (using weights)
        GameObject enemyPrefab = null;
        
        if (currentWaveGroup != null)
        {
            enemyPrefab = currentWaveGroup.GetRandomEnemy();
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
            // Calculate spawn position around player
            spawnPosition = GetCircularSpawnPosition();
            spawnRotation = Quaternion.LookRotation(playerTransform.position - spawnPosition);
        }
        
        // Spawn the enemy
        if (enemyPrefab != null)
        {
            GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, spawnRotation);
            
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
        }
    }
    
    /// <summary>
    /// Calculate a spawn position around the player in a circular pattern
    /// </summary>
    private Vector3 GetCircularSpawnPosition()
    {
        if (playerTransform == null) return Vector3.zero;
        
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
        
        // Return world position
        return playerTransform.position + offset;
    }
    
    /// <summary>
    /// Apply stat bonuses to a spawned enemy
    /// </summary>
    private void ApplyStatBonuses(GameObject enemyObject)
    {
        if (currentHealthBonus == 0f && currentDamageBonus == 0f) return;
        
        // Get the Enemy component
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy == null) return;
        
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
        
        // Apply damage bonus by adding a modifier component
        if (currentDamageBonus > 0f)
        {
            Enemy_Combat combat = enemyObject.GetComponent<Enemy_Combat>();
            if (combat != null)
            {
                // Add a component to track damage bonus
                EnemyStatModifier modifier = enemyObject.GetComponent<EnemyStatModifier>();
                if (modifier == null)
                {
                    modifier = enemyObject.AddComponent<EnemyStatModifier>();
                }
                modifier.damageBonus = currentDamageBonus;
            }
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
        currentWaveGroup = null; // Clear the current wave group
    }
    
    /// <summary>
    /// Visualize spawn points in editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (spawnMode == SpawnMode.CircularAroundPlayer)
        {
            Transform target = playerTransform != null ? playerTransform : transform;
            
            // Draw spawn circle
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
}
