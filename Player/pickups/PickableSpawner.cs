using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns pickable items at designated spawn points across the map.
/// Can spawn items randomly with a chance percentage.
/// </summary>
public class PickableSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Array of pickable prefabs to randomly choose from when spawning")]
    [SerializeField] private GameObject[] pickablePrefabs;
    
    [Tooltip("Chance (0-100%) that a pickable will spawn at this location")]
    [Range(0f, 100f)]
    [SerializeField] private float spawnChance = 50f;
    
    [Tooltip("Height offset above spawn point (prevents spawning underground)")]
    [SerializeField] private float spawnHeightOffset = 1.0f;
    
    [Tooltip("Scale multiplier for spawned pickables (3 = 3x larger)")]
    [SerializeField] private float scaleMultiplier = 3f;
    
    [Header("Respawn Settings")]
    [Tooltip("Should pickables respawn after being collected?")]
    [SerializeField] private bool enableRespawn = true;
    
    [Tooltip("Time in seconds before a new pickable respawns")]
    [SerializeField] private float respawnTime = 15f;
    
    [Header("Wave-Based Spawning")]
    [Tooltip("Should pickables only spawn after certain wave numbers?")]
    [SerializeField] private bool useWaveBasedSpawning = false;
    
    [Tooltip("Spawn chance increases by this percentage per wave")]
    [SerializeField] private float spawnChanceIncreasePerWave = 5f;
    
    [Tooltip("Maximum spawn chance cap")]
    [SerializeField] private float maxSpawnChance = 90f;
    
    [Header("Container System (Barrel/Crate)")]
    [Tooltip("Use a container that must be destroyed before revealing pickable")]
    [SerializeField] private bool useContainer = false;
    
    [Tooltip("Container prefab (barrel, crate, etc.) to spawn")]
    [SerializeField] private GameObject containerPrefab;
    
    [Tooltip("Should container spawn immediately on start?")]
    [SerializeField] private bool spawnContainerOnStart = true;
    
    [Tooltip("Offset for container position relative to spawn point")]
    [SerializeField] private Vector3 containerPositionOffset = Vector3.zero;
    
    [Header("Proximity Spawning (Optimization)")]
    [Tooltip("Only spawn when player is within this distance")]
    [SerializeField] private bool useProximitySpawning = true;
    
    [Tooltip("Distance from player required to spawn pickable")]
    [SerializeField] private float spawnProximityRange = 50f;
    
    [Tooltip("How often to check player distance (seconds)")]
    [SerializeField] private float proximityCheckInterval = 2f;
    
    [Header("Visual Helpers")]
    [Tooltip("Show spawn point in editor")]
    [SerializeField] private bool showGizmo = true;
    
    [SerializeField] private Color gizmoColor = Color.cyan;

    private GameObject currentPickable;
    private GameObject currentContainer;
    private bool hasSpawned = false;
    private float currentSpawnChance;
    private WaveManager waveManager;
    private bool isPlayerNearby = false;
    private GameObject activePlayer;
    private bool containerDestroyed = false;

    private void Start()
    {
        currentSpawnChance = spawnChance;
        
        //Debug.Log($"[PickableSpawner] {gameObject.name} - Initialized (Respawn: {enableRespawn}, RespawnTime: {respawnTime}s)");
        
        // Find WaveManager if wave-based spawning is enabled
        if (useWaveBasedSpawning)
        {
            waveManager = FindObjectOfType<WaveManager>();
            if (waveManager == null)
            {
                //Debug.LogWarning($"[PickableSpawner] {gameObject.name} has wave-based spawning enabled but no WaveManager found in scene!");
            }
        }
        
        // Find player
        FindPlayer();
        
        // Start proximity checking if enabled
        if (useProximitySpawning)
        {
            InvokeRepeating(nameof(CheckPlayerProximity), 0f, proximityCheckInterval);
        }
        else
        {
            // Attempt initial spawn immediately if proximity spawning is disabled
            // Container will only spawn if the spawn chance succeeds
            AttemptSpawn();
        }
    }
    
    private void FindPlayer()
    {
        // Try to find Player or Player2
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            activePlayer = player.gameObject;
        }
    }
    
    private void CheckPlayerProximity()
    {
        // Find player if not cached
        if (activePlayer == null)
        {
            FindPlayer();
        }
        
        // Check if player is nearby
        if (activePlayer != null)
        {
            float distance = Vector3.Distance(transform.position, activePlayer.transform.position);
            bool wasNearby = isPlayerNearby;
            isPlayerNearby = distance <= spawnProximityRange;
            
            // If player just entered range
            if (!wasNearby && isPlayerNearby)
            {
                // Attempt spawn (container only spawns if spawn chance succeeds)
                if (currentPickable == null && currentContainer == null)
                {
                    AttemptSpawn();
                }
            }
        }
    }

    private void AttemptSpawn()
    {
        Debug.Log($"[PickableSpawner] {gameObject.name} - AttemptSpawn() called");
        
        // Don't spawn if one already exists
        if (currentPickable != null)
        {
            Debug.Log($"[PickableSpawner] {gameObject.name} - Spawn blocked: pickable already exists");
            return;
        }
        
        // Don't spawn if container already exists
        if (currentContainer != null)
        {
            Debug.Log($"[PickableSpawner] {gameObject.name} - Spawn blocked: container already exists");
            return;
        }
        
        // If using container system and container was destroyed, wait for pickable to spawn first
        if (useContainer && containerDestroyed)
        {
            Debug.Log($"[PickableSpawner] {gameObject.name} - Spawn blocked: container destroyed, pickable should spawn");
            return;
        }
        
        // Check proximity if enabled
        if (useProximitySpawning && !isPlayerNearby)
        {
            Debug.Log($"[PickableSpawner] {gameObject.name} - Spawn blocked: player not nearby (useProximity={useProximitySpawning}, isNearby={isPlayerNearby})");
            return;
        }

        // Check if we have prefabs
        if (pickablePrefabs == null || pickablePrefabs.Length == 0)
        {
            //Debug.LogWarning($"[PickableSpawner] {gameObject.name} has no pickable prefabs assigned!");
            return;
        }

        // Calculate spawn chance (with wave modifier if enabled)
        float finalSpawnChance = currentSpawnChance;
        
        if (useWaveBasedSpawning && waveManager != null)
        {
            int currentWave = waveManager.GetCurrentWave();
            finalSpawnChance = Mathf.Min(spawnChance + (spawnChanceIncreasePerWave * currentWave), maxSpawnChance);
        }

        // Roll for spawn
        float roll = Random.Range(0f, 100f);
        
        Debug.Log($"[PickableSpawner] {gameObject.name} - Spawn roll: {roll:F1}% vs {finalSpawnChance:F1}% chance (useContainer={useContainer})");
        
        if (roll <= finalSpawnChance)
        {
            Debug.Log($"[PickableSpawner] {gameObject.name} - ✓ SPAWN SUCCESS!");
            
            // Spawn succeeded! Now spawn container (if enabled) or pickable directly
            if (useContainer)
            {
                SpawnContainerWithPickable();
            }
            else
            {
                SpawnPickable();
            }
        }
        else
        {
            Debug.Log($"[PickableSpawner] {gameObject.name} failed spawn roll ({roll:F1}% vs {finalSpawnChance:F1}% chance)");
            
            // Schedule retry if respawn is enabled
            if (enableRespawn)
            {
                Debug.Log($"[PickableSpawner] {gameObject.name} - Scheduling retry in {respawnTime} seconds");
                Invoke(nameof(AttemptSpawn), respawnTime);
            }
            else
            {
                Debug.Log($"[PickableSpawner] {gameObject.name} - Respawn disabled, will not retry");
            }
        }
    }

    private void SpawnPickable()
    {
        // Randomly select a prefab from the array
        GameObject selectedPrefab = pickablePrefabs[Random.Range(0, pickablePrefabs.Length)];
        
        // Calculate spawn position with height offset
        Vector3 spawnPosition = transform.position + Vector3.up * spawnHeightOffset;
        
        //Debug.Log($"[PickableSpawner] 📍 Spawning {selectedPrefab.name} at spawner '{gameObject.name}' (Position: {spawnPosition})");
        
        // Spawn the pickable at this location
        currentPickable = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        currentPickable.name = $"{selectedPrefab.name}_Spawned";
        
        // Scale the pickable
        currentPickable.transform.localScale = selectedPrefab.transform.localScale * scaleMultiplier;
        
        // Ensure it's active and visible
        currentPickable.SetActive(true);
        
        // Check if it has renderers
        //Renderer[] renderers = currentPickable.GetComponentsInChildren<Renderer>();
        //int visibleRenderers = 0;
        //foreach (Renderer r in renderers)
        //{
        //    if (r.enabled) visibleRenderers++;
        //}
        
        hasSpawned = true;
        
        //Debug.Log($"[PickableSpawner] ✓ SPAWNED SUCCESSFULLY: {selectedPrefab.name} at {gameObject.name}");
        //Debug.Log($"  └─ World Position: {currentPickable.transform.position}");
        //Debug.Log($"  └─ Local Scale: {currentPickable.transform.localScale}");
        //Debug.Log($"  └─ Active: {currentPickable.activeSelf}");
        //Debug.Log($"  └─ Renderers: {renderers.Length} total, {visibleRenderers} visible");
        //Debug.Log($"  └─ Layer: {LayerMask.LayerToName(currentPickable.layer)}");

        // Start monitoring for pickup
        StartCoroutine(MonitorPickable());
    }

    /// <summary>
    /// Spawn pickable from destroyed container with bounce animation and delayed interaction
    /// </summary>
    private void SpawnPickableFromContainer()
    {
        // Randomly select a prefab from the array
        GameObject selectedPrefab = pickablePrefabs[Random.Range(0, pickablePrefabs.Length)];
        
        // Calculate spawn position at container location
        Vector3 spawnPosition = transform.position + Vector3.up * spawnHeightOffset;
        
        // Spawn the pickable at this location
        currentPickable = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        currentPickable.name = $"{selectedPrefab.name}_FromContainer";
        
        // Scale the pickable
        currentPickable.transform.localScale = selectedPrefab.transform.localScale * scaleMultiplier;
        
        // Ensure it's active and visible
        currentPickable.SetActive(true);
        
        hasSpawned = true;
        
        Debug.Log($"[PickableSpawner] 🎁 PICKABLE SPAWNED FROM CONTAINER: {selectedPrefab.name}!");
        
        // Trigger bounce animation and delayed interaction on the pickable
        PlayerPickable pickable = currentPickable.GetComponent<PlayerPickable>();
        if (pickable != null)
        {
            pickable.PlayBounceAnimation(1.5f);
        }
        
        PlayerPowerup powerup = currentPickable.GetComponent<PlayerPowerup>();
        if (powerup != null)
        {
            powerup.PlayBounceAnimation(1.5f);
        }

        // Start monitoring for pickup
        StartCoroutine(MonitorPickable());
    }

    private System.Collections.IEnumerator MonitorPickable()
    {
        //Debug.Log($"[PickableSpawner] {gameObject.name} - Started monitoring pickable");
        
        // Wait until the pickable is destroyed (picked up or despawned)
        while (currentPickable != null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // Pickable was destroyed
        //Debug.Log($"[PickableSpawner] {gameObject.name} - Pickable was collected or despawned");

        // Schedule respawn if enabled
        if (enableRespawn)
        {
            //Debug.Log($"[PickableSpawner] {gameObject.name} - Respawn ENABLED: Scheduling respawn in {respawnTime} seconds");
            
            if (useContainer)
            {
                // Reset container system for respawn
                ResetContainer();
            }
            else
            {
                // Directly respawn pickable
                Invoke(nameof(AttemptSpawn), respawnTime);
            }
        }
        else
        {
            //Debug.LogWarning($"[PickableSpawner] {gameObject.name} - Respawn DISABLED: Will not respawn pickable");
        }
    }

    /// <summary>
    /// Manually trigger a spawn attempt (useful for events)
    /// </summary>
    public void TriggerSpawn()
    {
        AttemptSpawn();
    }

    /// <summary>
    /// Force spawn a pickable immediately (ignores spawn chance)
    /// </summary>
    public void ForceSpawn()
    {
        if (currentPickable != null)
        {
            Destroy(currentPickable);
        }
        
        SpawnPickable();
    }

    /// <summary>
    /// Clear any existing pickable at this spawn point
    /// </summary>
    public void ClearPickable()
    {
        if (currentPickable != null)
        {
            Destroy(currentPickable);
            currentPickable = null;
        }
    }

    /// <summary>
    /// Force spawn a specific prefab by index (ignores spawn chance and random selection)
    /// </summary>
    public void ForceSpawnSpecific(int prefabIndex)
    {
        if (pickablePrefabs == null || prefabIndex < 0 || prefabIndex >= pickablePrefabs.Length)
        {
            //Debug.LogWarning($"[PickableSpawner] Invalid prefab index: {prefabIndex}");
            return;
        }

        if (currentPickable != null)
        {
            Destroy(currentPickable);
        }
        
        GameObject selectedPrefab = pickablePrefabs[prefabIndex];
        currentPickable = Instantiate(selectedPrefab, transform.position, Quaternion.identity);
        currentPickable.name = $"{selectedPrefab.name}_Spawned_Forced";
        
        hasSpawned = true;
        StartCoroutine(MonitorPickable());
    }

    /// <summary>
    /// Spawn the container (barrel, crate, etc.) - only called when spawn chance succeeds
    /// </summary>
    private void SpawnContainerWithPickable()
    {
        if (containerPrefab == null)
        {
            //Debug.LogWarning($"[PickableSpawner] {gameObject.name} - No container prefab assigned! Spawning pickable directly.");
            SpawnPickable();
            return;
        }

        if (currentContainer != null)
        {
            //Debug.Log($"[PickableSpawner] {gameObject.name} - Container already exists");
            return;
        }

        // Calculate container spawn position
        Vector3 containerPosition = transform.position + containerPositionOffset;

        // Spawn the container
        currentContainer = Instantiate(containerPrefab, containerPosition, Quaternion.identity);
        currentContainer.name = $"{containerPrefab.name}_Container";

        // Get or add DestructibleContainer component
        DestructibleContainer destructible = currentContainer.GetComponent<DestructibleContainer>();
        if (destructible == null)
        {
            destructible = currentContainer.AddComponent<DestructibleContainer>();
        }

        // Register callback for when container is destroyed
        destructible.OnContainerDestroyed += HandleContainerDestroyed;

        // Mark that this container has a pickable waiting inside
        containerDestroyed = false;

        Debug.Log($"[PickableSpawner] {gameObject.name} - 🛢️ CONTAINER SPAWNED at {containerPosition} with pickable inside!");
    }

    /// <summary>
    /// Called when the container is destroyed
    /// </summary>
    private void HandleContainerDestroyed()
    {
        //Debug.Log($"[PickableSpawner] {gameObject.name} - Container destroyed! Spawning pickable...");
        
        containerDestroyed = true;
        currentContainer = null;

        // Spawn the pickable item immediately (no spawn chance check - container already spawned)
        SpawnPickableFromContainer();
    }

    /// <summary>
    /// Reset the container system for respawn
    /// </summary>
    private void ResetContainer()
    {
        containerDestroyed = false;
        
        if (enableRespawn)
        {
            // Attempt spawn after delay (spawn chance will be rolled again)
            Invoke(nameof(AttemptSpawn), respawnTime);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        // Draw spawn point indicator
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw a vertical line for easier visibility
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        // Draw more detailed gizmo when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.3f);
        
        // Draw spawn chance text (would need Unity Editor extension for proper text)
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}
