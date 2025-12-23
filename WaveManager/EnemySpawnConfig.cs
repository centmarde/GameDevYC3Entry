using UnityEngine;

/// <summary>
/// Handles enemy group configuration and selection logic
/// </summary>
[System.Serializable]
public class EnemySpawnConfig
{
    [Header("Enemy Group Settings")]
    [Tooltip("Define groups of enemies to spawn. Each group can have multiple enemy types with spawn weights.")]
    [SerializeField] private EnemyGroup[] enemyGroups;
    
    [Header("Special Enemy Settings")]
    [Tooltip("Boss enemies to spawn every 5th wave (5, 10, 15, etc.)")]
    [SerializeField] private GameObject[] bossPrefabs;
    [Tooltip("Special monster prefabs to spawn on odd waves (excluding boss waves)")]
    [SerializeField] private GameObject[] specialMonsterPrefabs;
    
    private EnemyGroup currentWaveGroup = null;
    
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
    
    /// <summary>
    /// Validate enemy configuration
    /// </summary>
    public bool IsValid()
    {
        return enemyGroups != null && enemyGroups.Length > 0;
    }
    
    /// <summary>
    /// Select a random enemy group for the wave
    /// </summary>
    public EnemyGroup SelectWaveGroup()
    {
        if (!IsValid()) return null;
        
        int randomGroupIndex = Random.Range(0, enemyGroups.Length);
        currentWaveGroup = enemyGroups[randomGroupIndex];
        
        if (currentWaveGroup == null || currentWaveGroup.enemies == null || currentWaveGroup.enemies.Length == 0)
        {
            Debug.LogError($"[EnemySpawnConfig] Selected enemy group {randomGroupIndex} is invalid or has no enemies!");
            return null;
        }
        
        Debug.Log($"[EnemySpawnConfig] Selected group: {currentWaveGroup.groupName}");
        return currentWaveGroup;
    }
    
    /// <summary>
    /// Get the currently selected wave group
    /// </summary>
    public EnemyGroup GetCurrentWaveGroup()
    {
        return currentWaveGroup;
    }
    
    /// <summary>
    /// Get a random enemy from the current wave group
    /// </summary>
    public GameObject GetRandomEnemyFromCurrentGroup()
    {
        if (currentWaveGroup == null)
        {
            Debug.LogError("[EnemySpawnConfig] No wave group selected!");
            return null;
        }
        
        return currentWaveGroup.GetRandomEnemy();
    }
    
    /// <summary>
    /// Get a random elite enemy from any group
    /// </summary>
    public GameObject GetRandomEliteEnemy()
    {
        if (!IsValid()) return null;
        
        // Pick a random group
        int randomGroupIndex = Random.Range(0, enemyGroups.Length);
        EnemyGroup selectedGroup = enemyGroups[randomGroupIndex];
        
        if (selectedGroup == null || selectedGroup.enemies == null || selectedGroup.enemies.Length == 0)
        {
            Debug.LogError($"[EnemySpawnConfig] Elite spawn failed: Selected enemy group {randomGroupIndex} is invalid!");
            return null;
        }
        
        Debug.Log($"[EnemySpawnConfig] Elite from group: {selectedGroup.groupName}");
        return selectedGroup.GetRandomEnemy();
    }
    
    /// <summary>
    /// Get a random boss prefab
    /// </summary>
    public GameObject GetRandomBossPrefab()
    {
        if (bossPrefabs == null || bossPrefabs.Length == 0)
        {
            Debug.LogWarning("[EnemySpawnConfig] No boss prefabs available!");
            return null;
        }
        
        return bossPrefabs[Random.Range(0, bossPrefabs.Length)];
    }
    
    /// <summary>
    /// Get a random special monster prefab
    /// </summary>
    public GameObject GetRandomSpecialMonsterPrefab()
    {
        if (specialMonsterPrefabs == null || specialMonsterPrefabs.Length == 0)
        {
            Debug.LogWarning("[EnemySpawnConfig] No special monster prefabs available!");
            return null;
        }
        
        return specialMonsterPrefabs[Random.Range(0, specialMonsterPrefabs.Length)];
    }
    
    /// <summary>
    /// Check if boss prefabs are available
    /// </summary>
    public bool HasBossPrefabs()
    {
        return bossPrefabs != null && bossPrefabs.Length > 0;
    }
    
    /// <summary>
    /// Check if special monster prefabs are available
    /// </summary>
    public bool HasSpecialMonsterPrefabs()
    {
        return specialMonsterPrefabs != null && specialMonsterPrefabs.Length > 0;
    }
    
    /// <summary>
    /// Reset current wave group selection
    /// </summary>
    public void ResetWaveGroup()
    {
        currentWaveGroup = null;
    }
    
    /// <summary>
    /// Get enemy groups for inspector access
    /// </summary>
    public EnemyGroup[] GetEnemyGroups()
    {
        return enemyGroups;
    }
    
    /// <summary>
    /// Get boss prefabs for inspector access
    /// </summary>
    public GameObject[] GetBossPrefabs()
    {
        return bossPrefabs;
    }
    
    /// <summary>
    /// Get special monster prefabs for inspector access
    /// </summary>
    public GameObject[] GetSpecialMonsterPrefabs()
    {
        return specialMonsterPrefabs;
    }
}