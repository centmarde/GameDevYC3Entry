using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private bool executeOnStart = true;
    [SerializeField] private Portal spawnPortal;
    [Tooltip("If no portal is assigned, will try to find a Portal in the scene")]
    [SerializeField] private bool autoFindPortal = true;

    private GameObject activePlayer;
    private bool hasSpawned = false; // Prevent multiple spawns
    private static PlayerSpawnManager instance; // Singleton to prevent duplicate spawning
    private static bool playerSpawnedThisSession = false; // Prevent respawns across scene loads

    private void Awake()
    {
        Debug.Log($"[PlayerSpawnManager] ===== AWAKE CALLED =====");
        Debug.Log($"[PlayerSpawnManager] GameObject: {gameObject.name}");
        Debug.Log($"[PlayerSpawnManager] playerSpawnedThisSession (static) = {playerSpawnedThisSession}");
        
        // Check for duplicate PlayerSpawnManager instances
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"[PlayerSpawnManager] Duplicate PlayerSpawnManager detected! Destroying {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // Subscribe to scene loaded events to ensure static flags are always reset
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        Debug.Log($"[PlayerSpawnManager] ✅ Awake complete on {gameObject.name}");
    }

    private void Start()
    {
        Debug.Log($"[PlayerSpawnManager] ===== START METHOD CALLED =====");
        Debug.Log($"[PlayerSpawnManager] GameObject: {gameObject.name}");
        Debug.Log($"[PlayerSpawnManager] executeOnStart: {executeOnStart}");
        Debug.Log($"[PlayerSpawnManager] hasSpawned: {hasSpawned}");
        Debug.Log($"[PlayerSpawnManager] playerSpawnedThisSession: {playerSpawnedThisSession}");
        Debug.Log($"[PlayerSpawnManager] activePlayer: {(activePlayer != null ? activePlayer.name : "NULL")}");
        Debug.Log($"[PlayerSpawnManager] Player1 Prefab: {(player1Prefab != null ? player1Prefab.name : "NULL")}");
        Debug.Log($"[PlayerSpawnManager] Player2 Prefab: {(player2Prefab != null ? player2Prefab.name : "NULL")}");
        
        // Try to find portal if auto-find is enabled and no portal assigned
        if (autoFindPortal && spawnPortal == null)
        {
            spawnPortal = FindObjectOfType<Portal>();
            if (spawnPortal != null)
            {
                Debug.Log($"[PlayerSpawnManager] Auto-found portal: {spawnPortal.GetPortalName()}");
            }
            else
            {
                Debug.LogWarning("[PlayerSpawnManager] No Portal found in scene! Player will spawn at origin.");
            }
        }
        else if (spawnPortal != null)
        {
            Debug.Log($"[PlayerSpawnManager] Portal already assigned: {spawnPortal.GetPortalName()}");
        }
        
        Debug.Log($"[PlayerSpawnManager] Checking spawn conditions...");
        Debug.Log($"[PlayerSpawnManager] - executeOnStart: {executeOnStart}");
        Debug.Log($"[PlayerSpawnManager] - hasSpawned: {hasSpawned}");
        Debug.Log($"[PlayerSpawnManager] - playerSpawnedThisSession: {playerSpawnedThisSession}");
        Debug.Log($"[PlayerSpawnManager] - CharacterSelectionManager exists: {CharacterSelectionManager.Instance != null}");
        if (CharacterSelectionManager.Instance != null)
        {
            Debug.Log($"[PlayerSpawnManager] - Selected Character Index: {CharacterSelectionManager.Instance.SelectedCharacterIndex}");
        }
        
        // Only spawn if enabled, not spawned yet, and not spawned this session
        if (executeOnStart && !hasSpawned && !playerSpawnedThisSession)
        {
            Debug.Log($"[PlayerSpawnManager] ✅ CONDITIONS MET FOR SPAWNING! Attempting to spawn player...");
            SpawnSelectedPlayer();
        }
        else
        {
            Debug.LogError($"[PlayerSpawnManager] ❌ SPAWN CONDITIONS NOT MET!");
            Debug.LogError($"[PlayerSpawnManager] - executeOnStart: {executeOnStart} (needs: true)");
            Debug.LogError($"[PlayerSpawnManager] - hasSpawned: {hasSpawned} (needs: false)");
            Debug.LogError($"[PlayerSpawnManager] - playerSpawnedThisSession: {playerSpawnedThisSession} (needs: false)");
            
            if (playerSpawnedThisSession)
            {
                Debug.LogError($"[PlayerSpawnManager] ❗ Player marked as already spawned this session! This suggests ResetGlobalSpawnFlags() was not called or failed.");
            }
        }
        
        Debug.Log($"[PlayerSpawnManager] ===== START METHOD COMPLETE =====");
    }
    
    private void OnDestroy()
    {
        // Clear instance when destroyed
        if (instance == this)
        {
            instance = null;
        }
        
        // Unsubscribe from scene events
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called when any scene loads - ensures static flags are always reset
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[PlayerSpawnManager] OnSceneLoaded: {scene.name} - Forcing static flag reset");
        Debug.Log($"[PlayerSpawnManager] BEFORE Scene Load Reset: playerSpawnedThisSession = {playerSpawnedThisSession}");
        playerSpawnedThisSession = false;
        Debug.Log($"[PlayerSpawnManager] AFTER Scene Load Reset: playerSpawnedThisSession = {playerSpawnedThisSession}");
    }

    /// <summary>
    /// Spawns the selected player from prefab
    /// </summary>
    public void SpawnSelectedPlayer()
    {
        Debug.Log($"[PlayerSpawnManager] ===== SPAWN SELECTED PLAYER CALLED =====");
        
        // Check if already spawned
        if (hasSpawned || playerSpawnedThisSession)
        {
            Debug.LogError($"[PlayerSpawnManager] ❌ SPAWN BLOCKED - Already spawned a player!");
            Debug.LogError($"[PlayerSpawnManager] - hasSpawned: {hasSpawned}");
            Debug.LogError($"[PlayerSpawnManager] - playerSpawnedThisSession: {playerSpawnedThisSession}");
            Debug.LogError($"[PlayerSpawnManager] - Current active player: {(activePlayer != null ? activePlayer.name : "NULL")}");
            Debug.LogError($"[PlayerSpawnManager] If this is unexpected after reset, check SceneResetManager.ResetPlayerSpawnState()");
            return;
        }
        
        Debug.Log($"[PlayerSpawnManager] ✅ Spawn checks passed, proceeding with spawn...");
        
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
        string prefabName = selectedIndex == 0 ? "Player1" : "Player2";

        Debug.Log($"[PlayerSpawnManager] Selected prefab: {prefabName} (index {selectedIndex})");
        Debug.Log($"[PlayerSpawnManager] Prefab reference: {(prefabToSpawn != null ? prefabToSpawn.name : "NULL")}");

        if (prefabToSpawn == null)
        {
            Debug.LogError($"[PlayerSpawnManager] ❌ CRITICAL ERROR: {prefabName} prefab is not assigned in inspector!");
            Debug.LogError($"[PlayerSpawnManager] Please assign the prefabs in the PlayerSpawnManager component:");
            Debug.LogError($"[PlayerSpawnManager] - Player1 Prefab: {(player1Prefab != null ? player1Prefab.name : "NOT ASSIGNED")}");
            Debug.LogError($"[PlayerSpawnManager] - Player2 Prefab: {(player2Prefab != null ? player2Prefab.name : "NOT ASSIGNED")}");
            Debug.LogError($"[PlayerSpawnManager] Cannot spawn player without prefab reference!");
            return;
        }
        
        Debug.Log($"[PlayerSpawnManager] ✅ Prefab validation passed: {prefabToSpawn.name}");

        // Get spawn position and rotation from portal (or use origin as fallback)
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;
        
        if (spawnPortal != null && spawnPortal.IsActive())
        {
            spawnPortal.GetSpawnTransform(out spawnPosition, out spawnRotation);
            Debug.Log($"[PlayerSpawnManager] Using Portal '{spawnPortal.GetPortalName()}' spawn point at {spawnPosition}");
        }
        else
        {
            Debug.LogWarning("[PlayerSpawnManager] No active portal found! Spawning at origin (0,0,0)");
        }

        // Destroy existing player if any (shouldn't happen due to check above, but just in case)
        if (activePlayer != null)
        {
            Debug.Log($"[PlayerSpawnManager] Destroying existing player: {activePlayer.name}");
            Destroy(activePlayer);
            activePlayer = null;
        }

        // Spawn the player at portal location
        Debug.Log($"[PlayerSpawnManager] Calling Instantiate with prefab: {prefabToSpawn.name} at position: {spawnPosition}");
        activePlayer = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        
        if (activePlayer == null)
        {
            Debug.LogError($"[PlayerSpawnManager] ❌ INSTANTIATE FAILED! activePlayer is NULL after Instantiate call!");
            Debug.LogError($"[PlayerSpawnManager] - Prefab: {(prefabToSpawn != null ? prefabToSpawn.name : "NULL")}");
            Debug.LogError($"[PlayerSpawnManager] - Position: {spawnPosition}");
            Debug.LogError($"[PlayerSpawnManager] - Rotation: {spawnRotation}");
            return;
        }
        
        Debug.Log($"[PlayerSpawnManager] ✅ Instantiate SUCCESS! Created: {activePlayer.name} (ID: {activePlayer.GetInstanceID()})");
        
        activePlayer.name = selectedIndex == 0 ? "Player1" : "Player2";
        
        // Tag the player so other systems can find it
        if (!activePlayer.CompareTag("Player"))
        {
            activePlayer.tag = "Player";
            Debug.Log($"[PlayerSpawnManager] Set player tag to 'Player'");
        }
        else
        {
            Debug.Log($"[PlayerSpawnManager] Player already has 'Player' tag");
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
    /// Spawns the player at a specific portal
    /// </summary>
    /// <param name="portal">The portal to spawn at</param>
    public void SpawnPlayerAtPortal(Portal portal)
    {
        if (portal == null)
        {
            Debug.LogError("[PlayerSpawnManager] Cannot spawn at null portal!");
            return;
        }
        
        spawnPortal = portal;
        SpawnSelectedPlayer();
    }

    /// <summary>
    /// Reset global spawn flags (static method for cross-scene reset)
    /// </summary>
    public static void ResetGlobalSpawnFlags()
    {
        Debug.Log($"[PlayerSpawnManager] ResetGlobalSpawnFlags() called - BEFORE: playerSpawnedThisSession = {playerSpawnedThisSession}");
        playerSpawnedThisSession = false;
        Debug.Log($"[PlayerSpawnManager] ResetGlobalSpawnFlags() called - AFTER: playerSpawnedThisSession = {playerSpawnedThisSession}");
        Debug.Log("[PlayerSpawnManager] ✅ Global spawn flags reset complete");
    }

    /// <summary>
    /// Reset spawn state for fresh game start
    /// Allows new player selection and spawning
    /// </summary>
    public void ResetSpawnState()
    {
        Debug.Log("[PlayerSpawnManager] Resetting spawn state...");
        
        // Destroy active player if exists
        if (activePlayer != null)
        {
            Destroy(activePlayer);
            activePlayer = null;
        }
        
        // Reset instance flags to allow fresh spawning
        hasSpawned = false;
        
        // Reset static flag too (redundant but safe)
        playerSpawnedThisSession = false;
        
        Debug.Log("[PlayerSpawnManager] Spawn state reset complete - ready for new character selection");
    }

    /// <summary>
    /// Force spawn player (useful after scene resets)
    /// </summary>
    public void ForceSpawnPlayer()
    {
        Debug.Log("[PlayerSpawnManager] ===== FORCE SPAWNING PLAYER =====");
        Debug.Log($"[PlayerSpawnManager] BEFORE Force Reset - hasSpawned: {hasSpawned}, playerSpawnedThisSession: {playerSpawnedThisSession}");
        
        // Reset flags first - be extra aggressive
        hasSpawned = false;
        playerSpawnedThisSession = false;
        
        Debug.Log($"[PlayerSpawnManager] AFTER Force Reset - hasSpawned: {hasSpawned}, playerSpawnedThisSession: {playerSpawnedThisSession}");
        
        // Clear any existing player
        if (activePlayer != null)
        {
            Debug.Log($"[PlayerSpawnManager] Destroying existing player: {activePlayer.name}");
            Destroy(activePlayer);
            activePlayer = null;
        }
        
        // Double-check flags are still reset (in case something overrode them)
        if (hasSpawned || playerSpawnedThisSession)
        {
            Debug.LogError($"[PlayerSpawnManager] ❌ FLAGS RESET FAILED! hasSpawned: {hasSpawned}, playerSpawnedThisSession: {playerSpawnedThisSession}");
            // Force them again
            hasSpawned = false;
            playerSpawnedThisSession = false;
            Debug.LogError($"[PlayerSpawnManager] Force reset flags again: hasSpawned: {hasSpawned}, playerSpawnedThisSession: {playerSpawnedThisSession}");
        }
        
        Debug.Log("[PlayerSpawnManager] Force spawn - calling SpawnSelectedPlayer()...");
        // Force spawn
        SpawnSelectedPlayer();
    }
    
    /// <summary>
    /// Validates that the PlayerSpawnManager is properly configured
    /// </summary>
    public bool IsProperlyConfigured()
    {
        bool isConfigured = true;
        
        if (player1Prefab == null)
        {
            Debug.LogError("[PlayerSpawnManager] Player1 prefab is not assigned!");
            isConfigured = false;
        }
        
        if (player2Prefab == null)
        {
            Debug.LogError("[PlayerSpawnManager] Player2 prefab is not assigned!");
            isConfigured = false;
        }
        
        if (CharacterSelectionManager.Instance == null)
        {
            Debug.LogError("[PlayerSpawnManager] CharacterSelectionManager not found!");
            isConfigured = false;
        }
        
        Debug.Log($"[PlayerSpawnManager] Configuration check: {(isConfigured ? "✅ VALID" : "❌ INVALID")}");
        return isConfigured;
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
        
        if (autoFindPortal && spawnPortal == null)
        {
            // This will only run in editor, not at runtime
            spawnPortal = FindObjectOfType<Portal>();
        }
    }
}
