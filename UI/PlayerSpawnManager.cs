using UnityEngine;

/// <summary>
/// Manages player characters in the MainBase scene.
/// Spawns the selected player from prefabs at the designated spawn point.
/// </summary>
public class PlayerSpawnManager : MonoBehaviour
{
    [Header("Player Prefabs")]
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool executeOnStart = true;

    private GameObject activePlayer;
    private bool hasSpawned = false; // Prevent multiple spawns
    private static PlayerSpawnManager instance; // Singleton to prevent duplicate spawning
    private static bool playerSpawnedThisSession = false; // Prevent respawns across scene loads

    private void Awake()
    {
        // Check for duplicate PlayerSpawnManager instances
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"[PlayerSpawnManager] Duplicate PlayerSpawnManager detected! Destroying {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        Debug.Log($"[PlayerSpawnManager] Awake called on {gameObject.name}");
    }

    private void Start()
    {
        Debug.Log($"[PlayerSpawnManager] Start called on {gameObject.name}, executeOnStart: {executeOnStart}, hasSpawned: {hasSpawned}, playerSpawnedThisSession: {playerSpawnedThisSession}");
        
        // Only spawn if enabled, not spawned yet, and not spawned this session
        if (executeOnStart && !hasSpawned && !playerSpawnedThisSession)
        {
            SpawnSelectedPlayer();
        }
        else if (playerSpawnedThisSession)
        {
            Debug.LogWarning($"[PlayerSpawnManager] Player already spawned this session! Skipping spawn to prevent duplicates.");
        }
    }
    
    private void OnDestroy()
    {
        // Clear instance when destroyed
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    /// Spawns the selected player from prefab
    /// </summary>
    public void SpawnSelectedPlayer()
    {
        // Check if already spawned
        if (hasSpawned || playerSpawnedThisSession)
        {
            Debug.LogWarning($"[PlayerSpawnManager] Already spawned a player! Ignoring duplicate spawn request.");
            Debug.LogWarning($"[PlayerSpawnManager] hasSpawned: {hasSpawned}, playerSpawnedThisSession: {playerSpawnedThisSession}");
            Debug.LogWarning($"[PlayerSpawnManager] Current active player: {(activePlayer != null ? activePlayer.name : "NULL")}");
            return;
        }
        
        // Check if a player already exists in the scene
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            Debug.LogWarning($"[PlayerSpawnManager] Player already exists in scene: {existingPlayer.name}. Skipping spawn.");
            activePlayer = existingPlayer;
            hasSpawned = true;
            playerSpawnedThisSession = true;
            return;
        }

        int selectedIndex = CharacterSelectionManager.Instance.SelectedCharacterIndex;
        Debug.Log($"[PlayerSpawnManager] ===== SPAWNING PLAYER =====");
        Debug.Log($"[PlayerSpawnManager] SpawnSelectedPlayer called. Selected index: {selectedIndex}");
        
        // Determine which prefab to spawn
        GameObject prefabToSpawn = selectedIndex == 0 ? player1Prefab : player2Prefab;

        if (prefabToSpawn == null)
        {
            Debug.LogError($"[PlayerSpawnManager] Player prefab for index {selectedIndex} is not assigned!");
            return;
        }

        // Determine spawn position
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        // Destroy existing player if any (shouldn't happen due to check above, but just in case)
        if (activePlayer != null)
        {
            Debug.Log($"[PlayerSpawnManager] Destroying existing player: {activePlayer.name}");
            Destroy(activePlayer);
            activePlayer = null;
        }

        // Spawn the player
        activePlayer = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        activePlayer.name = selectedIndex == 0 ? "Player1" : "Player2";
        
        // Tag the player so other systems can find it
        if (!activePlayer.CompareTag("Player"))
        {
            activePlayer.tag = "Player";
            Debug.Log($"[PlayerSpawnManager] Set player tag to 'Player'");
        }
        
        hasSpawned = true;
        playerSpawnedThisSession = true; // Mark as spawned for this session

        string characterName = selectedIndex == 0 ? "Player1 (Ranger)" : "Player2 (Assassin)";
        Debug.Log($"[PlayerSpawnManager] ===== SPAWN COMPLETE =====");
        Debug.Log($"[PlayerSpawnManager] Successfully spawned {characterName} at {spawnPosition}");
        Debug.Log($"[PlayerSpawnManager] Active player instance ID: {activePlayer.GetInstanceID()}");
        Debug.Log($"[PlayerSpawnManager] playerSpawnedThisSession set to TRUE");
        
        // Setup camera follow and validate animator
        SetupPlayerComponents();
    }
    
    /// <summary>
    /// Sets up camera follow and validates player components after spawning
    /// </summary>
    private void SetupPlayerComponents()
    {
        if (activePlayer == null)
        {
            Debug.LogError("[PlayerSpawnManager] Cannot setup components - activePlayer is null!");
            return;
        }
        
        // Find and setup camera to follow player
        IsoCameraFollow isoCamera = FindObjectOfType<IsoCameraFollow>();
        if (isoCamera != null)
        {
            // Set the spawned player as the camera target
            Transform[] newTargets = new Transform[] { activePlayer.transform };
            isoCamera.SetTargets(newTargets);
            Debug.Log($"[PlayerSpawnManager] Setup IsoCameraFollow to track {activePlayer.name}");
        }
        else
        {
            Debug.LogWarning("[PlayerSpawnManager] No IsoCameraFollow found in scene! Camera will not follow player.");
        }
        
        // Validate and reinitialize animator
        Player player = activePlayer.GetComponent<Player>();
        if (player != null)
        {
            Debug.Log($"[PlayerSpawnManager] Found Player component, checking animator...");
            
            // Give the player a frame to initialize
            StartCoroutine(ValidatePlayerAfterFrame(player));
        }
        else
        {
            Debug.LogWarning($"[PlayerSpawnManager] No Player component found on {activePlayer.name}!");
        }
    }
    
    /// <summary>
    /// Validates player components after giving it a frame to initialize
    /// </summary>
    private System.Collections.IEnumerator ValidatePlayerAfterFrame(Player player)
    {
        yield return null; // Wait one frame
        
        if (player == null || player.gameObject == null)
        {
            Debug.LogWarning("[PlayerSpawnManager] Player was destroyed before validation!");
            yield break;
        }
        
        // Check if animator is set up
        if (player.anim == null)
        {
            Debug.LogWarning($"[PlayerSpawnManager] Player animator is NULL after spawn! Attempting to reinitialize...");
            player.ReinitializeAnimator();
            
            if (player.anim != null)
            {
                Debug.Log($"[PlayerSpawnManager] ✅ Animator successfully reinitialized: {player.anim.gameObject.name}");
            }
            else
            {
                Debug.LogError($"[PlayerSpawnManager] ❌ Failed to initialize animator for {player.gameObject.name}!");
                LogAnimatorDebugInfo(player);
            }
        }
        else
        {
            // Animator exists, but validate it's properly configured
            bool isValid = true;
            
            if (!player.anim.enabled)
            {
                Debug.LogWarning($"[PlayerSpawnManager] Animator is disabled! Enabling it...");
                player.anim.enabled = true;
                isValid = false;
            }
            
            if (player.anim.runtimeAnimatorController == null)
            {
                Debug.LogError($"[PlayerSpawnManager] ❌ RuntimeAnimatorController is NULL! Animations will not play!");
                isValid = false;
                LogAnimatorDebugInfo(player);
            }
            else if (!player.anim.gameObject.activeInHierarchy)
            {
                Debug.LogError($"[PlayerSpawnManager] ❌ Animator GameObject is not active!");
                isValid = false;
            }
            else if (isValid)
            {
                Debug.Log($"[PlayerSpawnManager] ✅ Animator validated: {player.anim.gameObject.name}, Controller: {player.anim.runtimeAnimatorController.name}");
            }
        }
    }
    
    /// <summary>
    /// Logs detailed animator debug information when issues are detected
    /// </summary>
    private void LogAnimatorDebugInfo(Player player)
    {
        Debug.Log($"=== Animator Debug Info for {player.gameObject.name} ===");
        
        Animator[] allAnimators = player.GetComponentsInChildren<Animator>(true);
        Debug.Log($"  Total Animators found: {allAnimators.Length}");
        
        for (int i = 0; i < allAnimators.Length; i++)
        {
            Animator anim = allAnimators[i];
            Debug.Log($"  [{i}] {anim.gameObject.name}:");
            Debug.Log($"      - Active: {anim.gameObject.activeInHierarchy}");
            Debug.Log($"      - Enabled: {anim.enabled}");
            Debug.Log($"      - Controller: {(anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "NULL")}");
        }
        
        Debug.Log($"===========================================");
    }

    /// <summary>
    /// Returns the currently active player GameObject
    /// </summary>
    public GameObject GetActivePlayer()
    {
        return activePlayer;
    }

    /// <summary>
    /// Despawns/destroys the current active player
    /// </summary>
    public void DespawnPlayer()
    {
        if (activePlayer != null)
        {
            Destroy(activePlayer);
            activePlayer = null;
            Debug.Log("[PlayerSpawnManager] Player despawned");
        }
    }

    /// <summary>
    /// Validates the setup in the Inspector
    /// </summary>
    private void OnValidate()
    {
        if (player1Prefab == null || player2Prefab == null)
        {
            Debug.LogWarning("[PlayerSpawnManager] Both Player1 and Player2 prefabs must be assigned!");
        }
        if (spawnPoint == null)
        {
            Debug.LogWarning("[PlayerSpawnManager] No spawn point assigned. Will spawn at (0,0,0)");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Draws gizmos in the editor to show spawn point
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPoint.position, 1f);
            Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward * 2f);
            UnityEditor.Handles.Label(spawnPoint.position + Vector3.up * 2f, "Player Spawn Point");
        }
    }
#endif
}
