using UnityEngine;

/// <summary>
/// Manages player reference tracking and updating for wave spawning
/// </summary>
[System.Serializable]
public class WavePlayerReferenceManager
{
    [Header("Player Reference Settings")]
    [Tooltip("Player transform reference (automatically set by WaveManager)")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("If true, continuously searches for player if not found (useful for runtime-spawned players)")]
    [SerializeField] private bool autoFindPlayer = true;
    [Tooltip("How often to refresh player reference (in seconds). Set to 0 to check every spawn.")]
    [SerializeField] private float playerRefreshInterval = 0f;
    
    private float lastPlayerRefreshTime = 0f;
    private WaveManager waveManager;
    
    /// <summary>
    /// Initialize with wave manager reference
    /// </summary>
    public void Initialize(WaveManager waveManager)
    {
        this.waveManager = waveManager;
        
        // Try to find player on initialization if auto-find is enabled
        if (autoFindPlayer && playerTransform == null)
        {
            RefreshPlayerReference();
        }
    }
    
    /// <summary>
    /// Update player reference management (call from Update)
    /// </summary>
    public void UpdatePlayerReference()
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
    public void SetPlayerReference(Transform newPlayerTransform)
    {
        playerTransform = newPlayerTransform;
        Debug.Log($"[WavePlayerReferenceManager] Player reference updated to: {(newPlayerTransform != null ? newPlayerTransform.name : "NULL")}");
    }
    
    /// <summary>
    /// Get current player transform
    /// </summary>
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }
    
    /// <summary>
    /// Check if player reference is valid and active
    /// </summary>
    public bool IsPlayerReferenceValid()
    {
        return playerTransform != null && playerTransform.gameObject.activeInHierarchy;
    }
    
    /// <summary>
    /// Ensure player reference is available before spawning
    /// </summary>
    public bool EnsurePlayerReference()
    {
        // Always refresh player reference before spawning if auto-find is enabled
        if (autoFindPlayer && (playerTransform == null || !playerTransform.gameObject.activeInHierarchy))
        {
            RefreshPlayerReference();
        }
        
        return IsPlayerReferenceValid();
    }
    
    /// <summary>
    /// Refreshes the player reference by searching for active player in scene
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
                Debug.Log($"[WavePlayerReferenceManager] Found player via WaveManager: {player.name}");
                return;
            }
        }
        
        // Method 2: Try to find by tag
        GameObject playerByTag = GameObject.FindGameObjectWithTag("Player");
        if (playerByTag != null && playerByTag.activeInHierarchy)
        {
            playerTransform = playerByTag.transform;
            Debug.Log($"[WavePlayerReferenceManager] Found player by tag: {playerByTag.name} at position {playerTransform.position}");
            return;
        }
        
        // Method 3: Try to find by name
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        
        if (player1 != null && player1.activeInHierarchy)
        {
            playerTransform = player1.transform;
            Debug.Log($"[WavePlayerReferenceManager] Found Player1 at position {playerTransform.position}");
            return;
        }
        
        if (player2 != null && player2.activeInHierarchy)
        {
            playerTransform = player2.transform;
            Debug.Log($"[WavePlayerReferenceManager] Found Player2 at position {playerTransform.position}");
            return;
        }
        
        // Method 4: Find by component type
        Player playerComponent = Object.FindObjectOfType<Player>();
        if (playerComponent != null && playerComponent.gameObject.activeInHierarchy)
        {
            playerTransform = playerComponent.transform;
            Debug.Log($"[WavePlayerReferenceManager] Found player by component: {playerComponent.gameObject.name} at position {playerTransform.position}");
            return;
        }
        
        // If we reach here, no player was found
        if (playerTransform == null)
        {
            Debug.LogWarning("[WavePlayerReferenceManager] Could not find any active player in scene!");
        }
    }
    
    /// <summary>
    /// Reset player reference
    /// </summary>
    public void ResetPlayerReference()
    {
        playerTransform = null;
        lastPlayerRefreshTime = 0f;
        Debug.Log("[WavePlayerReferenceManager] Player reference reset");
    }
    
    /// <summary>
    /// Get current player position (returns Vector3.zero if no player)
    /// </summary>
    public Vector3 GetPlayerPosition()
    {
        if (playerTransform != null)
        {
            return playerTransform.position;
        }
        return Vector3.zero;
    }
    
    // Getters for inspector access
    public bool GetAutoFindPlayer() => autoFindPlayer;
    public float GetPlayerRefreshInterval() => playerRefreshInterval;
}