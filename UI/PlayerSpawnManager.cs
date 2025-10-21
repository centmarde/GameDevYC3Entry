using UnityEngine;

/// <summary>
/// Manages player characters in the MainBase scene.
/// Supports two modes:
/// 1. Spawn Mode: Spawns the selected player from prefabs
/// 2. Scene Mode: Destroys the unselected player that's already in the scene
/// </summary>
public class PlayerSpawnManager : MonoBehaviour
{
    [Header("Mode Selection")]
    [SerializeField] private PlayerSetupMode setupMode = PlayerSetupMode.SpawnFromPrefab;
    
    [Header("Spawn Mode - Player Prefabs (if using Spawn Mode)")]
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Scene Mode - Existing Players (if using Scene Mode)")]
    [SerializeField] private GameObject player1InScene;
    [SerializeField] private GameObject player2InScene;

    [Header("Settings")]
    [SerializeField] private bool executeOnStart = true;

    private GameObject activePlayer;

    public enum PlayerSetupMode
    {
        SpawnFromPrefab,    // Spawn selected player from prefab
        DestroyUnselected   // Destroy unselected player already in scene
    }

    private void Start()
    {
        if (executeOnStart)
        {
            SetupPlayer();
        }
    }

    /// <summary>
    /// Main method to setup the player based on the selected mode
    /// </summary>
    public void SetupPlayer()
    {
        int selectedIndex = CharacterSelectionManager.Instance.SelectedCharacterIndex;

        if (setupMode == PlayerSetupMode.SpawnFromPrefab)
        {
            SpawnSelectedPlayer(selectedIndex);
        }
        else
        {
            DestroyUnselectedPlayer(selectedIndex);
        }
    }

    /// <summary>
    /// Spawns the selected player from prefab
    /// </summary>
    private void SpawnSelectedPlayer(int selectedIndex)
    {
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

        // Destroy existing player if any
        if (activePlayer != null)
        {
            Destroy(activePlayer);
        }

        // Spawn the player
        activePlayer = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        activePlayer.name = selectedIndex == 0 ? "Player1" : "Player2";

        string characterName = selectedIndex == 0 ? "Player1 (Warrior)" : "Player2 (Assassin)";
        Debug.Log($"[PlayerSpawnManager] Spawned {characterName} at {spawnPosition}");
    }

    /// <summary>
    /// Destroys the unselected player that's already in the scene
    /// </summary>
    private void DestroyUnselectedPlayer(int selectedIndex)
    {
        Debug.Log($"[PlayerSpawnManager] DestroyUnselectedPlayer called with index: {selectedIndex}");
        Debug.Log($"[PlayerSpawnManager] Player1InScene: {(player1InScene != null ? player1InScene.name : "NULL")}");
        Debug.Log($"[PlayerSpawnManager] Player2InScene: {(player2InScene != null ? player2InScene.name : "NULL")}");

        if (player1InScene == null || player2InScene == null)
        {
            Debug.LogError("[PlayerSpawnManager] ERROR: Both Player1 and Player2 must be assigned in Inspector!");
            Debug.LogError($"[PlayerSpawnManager] Player1InScene is {(player1InScene == null ? "NULL" : "assigned")}");
            Debug.LogError($"[PlayerSpawnManager] Player2InScene is {(player2InScene == null ? "NULL" : "assigned")}");
            return;
        }

        if (selectedIndex == 0)
        {
            // Player1 selected, destroy Player2
            activePlayer = player1InScene;
            
            Debug.Log($"[PlayerSpawnManager] Player1 selected. Destroying Player2: {player2InScene.name}");
            Debug.Log($"[PlayerSpawnManager] Player2 GameObject active state: {player2InScene.activeInHierarchy}");
            
            Destroy(player2InScene);
            player2InScene = null; // Clear reference immediately
            
            Debug.Log("[PlayerSpawnManager] Player2 destroyed successfully");
            
            if (activePlayer != null)
            {
                activePlayer.name = "Player1 (Active)";
                Debug.Log($"[PlayerSpawnManager] Player1 '{activePlayer.name}' is now active");
            }
        }
        else
        {
            // Player2 selected, destroy Player1
            activePlayer = player2InScene;
            
            Debug.Log($"[PlayerSpawnManager] Player2 selected. Destroying Player1: {player1InScene.name}");
            Debug.Log($"[PlayerSpawnManager] Player1 GameObject active state: {player1InScene.activeInHierarchy}");
            
            Destroy(player1InScene);
            player1InScene = null; // Clear reference immediately
            
            Debug.Log("[PlayerSpawnManager] Player1 destroyed successfully");
            
            if (activePlayer != null)
            {
                activePlayer.name = "Player2 (Active)";
                Debug.Log("[PlayerSpawnManager] Player2 is now active");
            }
        }
    }

    /// <summary>
    /// Returns the currently active player GameObject
    /// </summary>
    public GameObject GetActivePlayer()
    {
        return activePlayer;
    }

    /// <summary>
    /// Manually switch to spawn mode and spawn a player
    /// </summary>
    public void SpawnSelectedPlayer()
    {
        setupMode = PlayerSetupMode.SpawnFromPrefab;
        SetupPlayer();
    }

    /// <summary>
    /// Manually switch to destroy mode and remove unselected player
    /// </summary>
    public void DestroyUnselectedPlayer()
    {
        setupMode = PlayerSetupMode.DestroyUnselected;
        SetupPlayer();
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
        if (setupMode == PlayerSetupMode.SpawnFromPrefab)
        {
            if (player1Prefab == null || player2Prefab == null)
            {
                Debug.LogWarning("[PlayerSpawnManager] Spawn Mode requires both Player1 and Player2 prefabs!");
            }
            if (spawnPoint == null)
            {
                Debug.LogWarning("[PlayerSpawnManager] Spawn Mode: No spawn point assigned. Will spawn at (0,0,0)");
            }
        }
        else if (setupMode == PlayerSetupMode.DestroyUnselected)
        {
            if (player1InScene == null || player2InScene == null)
            {
                Debug.LogWarning("[PlayerSpawnManager] Scene Mode requires both Player1 and Player2 to be assigned from the scene!");
            }
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Draws gizmos in the editor to show spawn point
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (setupMode == PlayerSetupMode.SpawnFromPrefab && spawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPoint.position, 1f);
            Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward * 2f);
            UnityEditor.Handles.Label(spawnPoint.position + Vector3.up * 2f, "Player Spawn Point");
        }
        else if (setupMode == PlayerSetupMode.DestroyUnselected)
        {
            if (player1InScene != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(player1InScene.transform.position, 0.5f);
                UnityEditor.Handles.Label(player1InScene.transform.position + Vector3.up * 2f, "Player1");
            }
            if (player2InScene != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(player2InScene.transform.position, 0.5f);
                UnityEditor.Handles.Label(player2InScene.transform.position + Vector3.up * 2f, "Player2");
            }
        }
    }
#endif
}
