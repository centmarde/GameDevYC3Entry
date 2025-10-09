using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject[] enemyPrefabs; // Array of enemy GameObjects to spawn
    
    [Header("Spawn Mode")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.CircularAroundPlayer;
    [Tooltip("Reference to player transform (auto-finds if not set)")]
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
    
    public enum SpawnMode
    {
        Manual,                    // Use manually placed spawn points
        CircularAroundPlayer      // Auto-generate spawn points around player
    }
    
    private void Awake()
    {
        // Auto-find player if not assigned
        if (playerTransform == null && spawnMode == SpawnMode.CircularAroundPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("WaveSpawner: Player not found! Please assign player transform or tag your player as 'Player'");
            }
        }
        
        // Auto-find WaveManager if not assigned
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
        }
    }
    
    /// <summary>
    /// Start spawning enemies for the wave
    /// </summary>
    /// <param name="enemyCount">Number of enemies to spawn in this wave</param>
    public void StartWave(int enemyCount)
    {
        if (isSpawning) return;
        
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("WaveSpawner: No enemy prefabs assigned!");
            return;
        }
        
        // Validate spawn mode requirements
        if (spawnMode == SpawnMode.Manual)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("WaveSpawner: No spawn points assigned for Manual mode!");
                return;
            }
        }
        else if (spawnMode == SpawnMode.CircularAroundPlayer)
        {
            if (playerTransform == null)
            {
                Debug.LogError("WaveSpawner: Player transform not found for Circular mode!");
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
    /// Each spawn randomly selects an enemy type from the enemyPrefabs array.
    /// The array is never modified - it just picks a random index each time.
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemiesSpawned >= enemiesToSpawn)
        {
            CancelInvoke(nameof(SpawnEnemy));
            isSpawning = false;
            return;
        }
        
        // Randomly select an enemy type from the array (array remains unchanged)
        // Each spawn gets a fresh random pick, allowing for variety
        int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyPrefab = enemyPrefabs[randomEnemyIndex];
        
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
            
            Debug.Log($"Spawned {enemyPrefab.name} (enemy {enemiesSpawned}/{enemiesToSpawn}) at {spawnPosition}");
        }
        else
        {
            Debug.LogWarning($"WaveSpawner: Invalid enemy prefab!");
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
