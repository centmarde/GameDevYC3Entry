using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/// <summary>
/// Photon Game Manager - Manages single static lobby and player data
/// Integrates with WaveManager to track and share wave progress on leaderboards
/// 
/// QUICK SETUP DOCUMENTATION:
/// ==========================
/// See "PhotonGameManagerSetup.md" for complete step-by-step guide
/// 
/// KEY FEATURES:
/// - Single static lobby (no room creation/joining needed)
/// - Saves player name from LobbyController to Photon
/// - Syncs wave progress to leaderboards
/// - Persistent player stats across sessions stored in Photon Cloud
/// - Automatic reconnection handling
/// - Dual-layer storage: Room properties (session) + Cloud properties (persistent)
/// 
/// CLOUD STORAGE SYSTEM:
/// ====================
/// This manager uses TWO types of storage for maximum reliability:
/// 
/// 1. ROOM PROPERTIES (Session-based):
///    - Stored via SetCustomProperties on LocalPlayer
///    - Visible to other players in the same room
///    - Lost when player leaves the room
///    - Used for real-time leaderboard display
/// 
/// 2. CLOUD PROPERTIES (Persistent):
///    - Stored via Player Account Custom Properties (prefixed with "Cloud_")
///    - Persists across sessions and room changes
///    - Stored in Photon Cloud servers
///    - Automatically loaded on connection
///    - Backed up to local PlayerPrefs
/// 
/// Data is saved to cloud on:
/// - Wave start/completion
/// - Player death (via GameOverPanelUI)
/// - Leaving room
/// - Application quit
/// - Manual save via SaveCurrentWaveToLeaderboard()
/// </summary>
public class PhotonGameManager : MonoBehaviourPunCallbacks
{
    public static PhotonGameManager Instance { get; private set; }

    [Header("Lobby Settings")]
    [Tooltip("Name of the static lobby room everyone joins")]
    [SerializeField] private string lobbyRoomName = "GlobalLobby";
    
    [Tooltip("Maximum players in the lobby")]
    [SerializeField] private int maxPlayersInLobby = 100;
    
    [Tooltip("Automatically connect on Start")]
    [SerializeField] private bool autoConnectOnStart = true;

    [Header("Player Data")]
    [Tooltip("Current player's display name (set from LobbyController)")]
    [SerializeField] private string playerName = "Player";
    
    [Tooltip("Reference to WaveManager to track wave progress")]
    [SerializeField] private WaveManager waveManager;

    [Header("UI References (Optional)")]
    [Tooltip("Status text to show connection/lobby state")]
    [SerializeField] private TextMeshProUGUI statusText;
    
    [Tooltip("Text to show current room player count")]
    [SerializeField] private TextMeshProUGUI playerCountText;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Photon state
    private bool isConnecting = false;
    private bool isInLobby = false;
    private int currentWaveReached = 0;
    private int highestWaveReached = 0;

    // Player stats keys (for saving to Photon) - Name-based system
    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string HIGHEST_WAVE_KEY = "HighestWave";
    private const string CURRENT_WAVE_KEY = "CurrentWave";
    private const string TOTAL_KILLS_KEY = "TotalKills";
    
    // Name-based cloud storage keys
    private const string NAME_BASED_PREFIX = "Name_";

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Find WaveManager if not assigned
        if (waveManager == null)
        {
            Debug.Log("[PhotonGameManager] WaveManager not assigned in inspector, searching for it...");
            waveManager = FindObjectOfType<WaveManager>();
            
            if (waveManager != null)
            {
                Debug.Log($"[PhotonGameManager] ✓ WaveManager found: {waveManager.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("[PhotonGameManager] ⚠️ WaveManager NOT FOUND yet - will search continuously (may be on player prefab)");
            }
        }
        else
        {
            Debug.Log($"[PhotonGameManager] ✓ WaveManager already assigned: {waveManager.gameObject.name}");
        }

        // Load saved player name and highest wave from previous session
        LoadPlayerName();
        LoadHighestWave();
    }

    private void Start()
    {
        // Set Photon settings
        PhotonNetwork.AutomaticallySyncScene = false; // We use static lobby, no scene sync needed
        PhotonNetwork.SendRate = 20; // Updates per second
        PhotonNetwork.SerializationRate = 10; // Sync rate for custom properties

        LogDebug("=== Photon Cloud Storage System Active ===");
        LogDebug($"Player: {playerName} | Highest Wave: {highestWaveReached}");
        LogDebug("Leaderboard records will be saved to Photon Cloud");

        if (autoConnectOnStart)
        {
            ConnectToPhoton();
        }

        // Subscribe to wave events if WaveManager exists
        if (waveManager != null)
        {
            Debug.Log("[PhotonGameManager] Subscribing to WaveManager events...");
            waveManager.OnWaveStart.AddListener(OnWaveStarted);
            waveManager.OnWaveComplete.AddListener(OnWaveCompleted);
            Debug.Log($"[PhotonGameManager] ✓ Successfully subscribed to wave events on {waveManager.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[PhotonGameManager] ⚠️ WaveManager not found yet - will auto-connect when player spawns");
        }
    }

    private void Update()
    {
        // Update UI
        UpdateStatusUI();
        
        // Continuously check for WaveManager if not found yet
        if (waveManager == null)
        {
            TryFindWaveManager();
        }
    }

    #region Photon Connection

    /// <summary>
    /// Connect to Photon and join the static lobby
    /// </summary>
    public void ConnectToPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            LogDebug("Already connected to Photon");
            JoinOrCreateLobby();
            return;
        }

        if (isConnecting)
        {
            LogDebug("Already attempting to connect");
            return;
        }

        isConnecting = true;
        LogDebug("Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// Disconnect from Photon
    /// </summary>
    public void DisconnectFromPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            LogDebug("Disconnecting from Photon");
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnConnectedToMaster()
    {
        isConnecting = false;
        LogDebug($"Connected to Photon Master Server. Player ID: {PhotonNetwork.LocalPlayer.UserId}");
        
        // Set player nickname
        PhotonNetwork.NickName = playerName;
        
        // Load player stats from Photon Cloud (persistent across sessions)
        LoadPlayerStatsFromCloud();
        
        // Join or create the static lobby
        JoinOrCreateLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnecting = false;
        isInLobby = false;
        LogDebug($"Disconnected from Photon. Reason: {cause}");

        // Attempt to reconnect after a delay
        if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            Invoke(nameof(ConnectToPhoton), 5f);
        }
    }

    /// <summary>
    /// Join or create the static lobby room
    /// </summary>
    private void JoinOrCreateLobby()
    {
        LogDebug($"Attempting to join/create lobby: {lobbyRoomName}");

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayersInLobby,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom(lobbyRoomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        isInLobby = true;
        LogDebug($"Joined lobby: {PhotonNetwork.CurrentRoom.Name}. Players in room: {PhotonNetwork.CurrentRoom.PlayerCount}");

        // Set initial player custom properties
        UpdatePlayerProperties();
    }

    public override void OnLeftRoom()
    {
        isInLobby = false;
        
        // Save final stats to cloud before leaving
        SavePlayerStatsToCloud();
        
        LogDebug("Left the lobby room - stats saved to cloud");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        LogDebug($"Failed to join room: {message}");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        LogDebug($"Player joined lobby: {newPlayer.NickName}");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        LogDebug($"Player left lobby: {otherPlayer.NickName}");
    }

    #endregion

    #region Player Data Management

    /// <summary>
    /// Set the player name (call this from LobbyController)
    /// </summary>
    public void SetPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            LogDebug("Cannot set empty player name");
            return;
        }

        playerName = name;
        
        // Save to PlayerPrefs
        PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
        PlayerPrefs.Save();

        // Update Photon nickname
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.NickName = playerName;
            UpdatePlayerProperties();
        }

        LogDebug($"Player name set to: {playerName}");
    }

    /// <summary>
    /// Load saved player name from PlayerPrefs
    /// </summary>
    private void LoadPlayerName()
    {
        if (PlayerPrefs.HasKey(PLAYER_NAME_KEY))
        {
            playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY);
            LogDebug($"Loaded player name: {playerName}");
        }
    }

    /// <summary>
    /// Update player's custom properties in Photon (room session) - Name-based identification
    /// </summary>
    private void UpdatePlayerProperties()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
            return;

        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { PLAYER_NAME_KEY, playerName },
            { CURRENT_WAVE_KEY, currentWaveReached },
            { HIGHEST_WAVE_KEY, highestWaveReached },
            { TOTAL_KILLS_KEY, GetTotalKills() },
            { NAME_BASED_PREFIX + "ID", playerName.ToLower().Replace(" ", "") }
        };

        // Update room properties (visible to other players in current session)
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        
        // Also save to Photon Cloud (persistent across sessions)
        SavePlayerStatsToCloud();
        
        LogDebug($"Updated player properties: {playerName}, Wave: {currentWaveReached}, Highest: {highestWaveReached}");
    }

    /// <summary>
    /// Get total kills (you can implement this based on your kill tracking system)
    /// </summary>
    private int GetTotalKills()
    {
        // TODO: Implement kill tracking
        // For now, return 0 or integrate with your existing kill counter
        return 0;
    }

    #endregion

    #region Wave Progress Tracking

    /// <summary>
    /// Called when a wave starts (from WaveManager event)
    /// </summary>
    private void OnWaveStarted(int waveNumber)
    {
        currentWaveReached = waveNumber;
        
        // Update highest wave if current is higher
        if (currentWaveReached > highestWaveReached)
        {
            highestWaveReached = currentWaveReached;
            // Only save locally during runtime, don't push to Photon yet
            SaveHighestWaveLocally(highestWaveReached);
        }

        // Don't update Photon properties during gameplay - only on death
        // UpdatePlayerProperties(); // REMOVED - only update on death

        LogDebug($"Wave {waveNumber} started. Highest wave: {highestWaveReached} (stored locally)");
    }

    /// <summary>
    /// Called when a wave completes (from WaveManager event)
    /// </summary>
    private void OnWaveCompleted(int waveNumber)
    {
        LogDebug($"Wave {waveNumber} completed");
        // Additional logic can be added here if needed
    }

    /// <summary>
    /// Save highest wave to PlayerPrefs (persistent across sessions)
    /// </summary>
    private void SaveHighestWave(int wave)
    {
        PlayerPrefs.SetInt(HIGHEST_WAVE_KEY, wave);
        PlayerPrefs.Save();
        LogDebug($"Saved highest wave: {wave}");
    }

    /// <summary>
    /// Save highest wave locally only (runtime storage, not synced to Photon)
    /// </summary>
    private void SaveHighestWaveLocally(int wave)
    {
        // Just update the runtime variable, don't sync to Photon
        highestWaveReached = wave;
        // Optionally save to PlayerPrefs as backup
        PlayerPrefs.SetInt(HIGHEST_WAVE_KEY + "_Runtime", wave);
        LogDebug($"Saved highest wave locally (runtime): {wave}");
    }

    /// <summary>
    /// Load highest wave from PlayerPrefs
    /// </summary>
    public void LoadHighestWave()
    {
        if (PlayerPrefs.HasKey(HIGHEST_WAVE_KEY))
        {
            highestWaveReached = PlayerPrefs.GetInt(HIGHEST_WAVE_KEY);
            LogDebug($"Loaded highest wave: {highestWaveReached}");
        }
    }

    /// <summary>
    /// Reset player stats (for testing or new game)
    /// </summary>
    public void ResetPlayerStats()
    {
        currentWaveReached = 0;
        highestWaveReached = 0;
        PlayerPrefs.DeleteKey(HIGHEST_WAVE_KEY);
        PlayerPrefs.Save();
        UpdatePlayerProperties();
        LogDebug("Player stats reset");
    }

    /// <summary>
    /// Push runtime wave data to Photon Cloud - ONLY called on player death
    /// This is the ONLY method that syncs local data to Photon
    /// </summary>
    public void SaveCurrentWaveToLeaderboard()
    {
        // Get current wave from WaveManager if available
        if (waveManager != null)
        {
            int waveFromManager = waveManager.GetCurrentWave();
            Debug.Log($"[PhotonGameManager] Getting wave from WaveManager: {waveFromManager} (was: {currentWaveReached})");
            currentWaveReached = waveFromManager;
        }
        else
        {
            Debug.LogError("[PhotonGameManager] ❌ WaveManager is NULL when trying to get current wave! Using cached value: " + currentWaveReached);
        }

        // Update highest wave if current is higher
        if (currentWaveReached > highestWaveReached)
        {
            highestWaveReached = currentWaveReached;
        }
        
        // Save to local PlayerPrefs first
        SaveHighestWave(highestWaveReached);

        // NOW push to Photon Cloud (only happens on death)
        UpdatePlayerProperties();
        
        // Ensure cloud save is completed
        SavePlayerStatsToCloud();

        LogDebug($"DEATH: Pushed wave progress to Photon Cloud - Current Wave: {currentWaveReached}, Highest Wave: {highestWaveReached}");
    }

    /// <summary>
    /// Save player stats to Photon Cloud (persistent across sessions) - Name-based storage
    /// Each name gets its own cloud record
    /// </summary>
    private void SavePlayerStatsToCloud()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
        {
            LogDebug("Cannot save to cloud - not connected to Photon");
            return;
        }

        // Create name-based cloud properties
        string nameKey = playerName.ToLower().Replace(" ", ""); // Normalize name for key
        
        ExitGames.Client.Photon.Hashtable cloudProperties = new ExitGames.Client.Photon.Hashtable
        {
            { $"Cloud_{nameKey}_Name", playerName }, // Original name with formatting
            { $"Cloud_{nameKey}_HighestWave", highestWaveReached },
            { $"Cloud_{nameKey}_CurrentWave", currentWaveReached },
            { $"Cloud_{nameKey}_TotalKills", GetTotalKills() },
            { $"Cloud_{nameKey}_LastSaved", System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
            { "Cloud_ActivePlayerName", playerName } // Current active name for quick lookup
        };

        // Save to player's account (persists across sessions)
        PhotonNetwork.LocalPlayer.SetCustomProperties(cloudProperties);
        
        // Also save to local PlayerPrefs as backup
        SaveHighestWave(highestWaveReached);

        LogDebug($"Saved to Photon Cloud (Name-based) - Name: {playerName} (Key: {nameKey}), Highest Wave: {highestWaveReached}");
    }

    /// <summary>
    /// Load player stats from Photon Cloud based on current name
    /// Each name loads its own record
    /// </summary>
    private void LoadPlayerStatsFromCloud()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
        {
            LogDebug("Cannot load from cloud - not connected to Photon");
            return;
        }

        var cloudProps = PhotonNetwork.LocalPlayer.CustomProperties;
        
        // Load based on current player name
        string nameKey = playerName.ToLower().Replace(" ", ""); // Normalize name for key

        // Try to load data for this specific name
        string cloudHighestWaveKey = $"Cloud_{nameKey}_HighestWave";
        if (cloudProps.ContainsKey(cloudHighestWaveKey))
        {
            int cloudHighestWave = (int)cloudProps[cloudHighestWaveKey];
            
            // Use cloud value for this name
            highestWaveReached = cloudHighestWave;
            SaveHighestWave(highestWaveReached); // Update local storage
            LogDebug($"Loaded name-based cloud data - Name: {playerName} (Key: {nameKey}), Highest Wave: {highestWaveReached}");
        }
        else
        {
            LogDebug($"No cloud data found for name '{playerName}' - starting fresh");
            highestWaveReached = 0; // Start fresh for this name
        }

        // Try to load current wave for this name
        string cloudCurrentWaveKey = $"Cloud_{nameKey}_CurrentWave";
        if (cloudProps.ContainsKey(cloudCurrentWaveKey))
        {
            currentWaveReached = (int)cloudProps[cloudCurrentWaveKey];
        }

        LogDebug($"Loaded stats for '{playerName}': Highest Wave {highestWaveReached}, Current Wave {currentWaveReached}");
    }

    #endregion

    #region Leaderboard Data

    /// <summary>
    /// Get all players and their unique name records for the leaderboard
    /// Returns best record for each unique name (name-based leaderboard)
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboardData()
    {
        List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();

        if (!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom == null)
        {
            LogDebug("Not in a room, cannot get leaderboard data");
            return leaderboard;
        }

        // Dictionary to track best record per name
        Dictionary<string, LeaderboardEntry> bestRecordsByName = new Dictionary<string, LeaderboardEntry>();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            var props = player.CustomProperties;
            
            // Look for all name-based cloud records in player properties
            foreach (var property in props)
            {
                string key = property.Key.ToString();
                
                // Find name-based highest wave properties
                if (key.StartsWith("Cloud_") && key.EndsWith("_HighestWave"))
                {
                    // Extract name key from property key (Cloud_{nameKey}_HighestWave)
                    string nameKey = key.Substring(6, key.Length - 18); // Remove "Cloud_" prefix and "_HighestWave" suffix
                    
                    // Get the original name
                    string nameProperty = $"Cloud_{nameKey}_Name";
                    string playerName = props.ContainsKey(nameProperty) ? props[nameProperty].ToString() : nameKey;
                    
                    int highestWave = (int)property.Value;
                    
                    // Check if this is the best record for this name
                    if (!bestRecordsByName.ContainsKey(playerName) || 
                        bestRecordsByName[playerName].highestWave < highestWave)
                    {
                        // Get additional stats for this name
                        int currentWave = props.ContainsKey($"Cloud_{nameKey}_CurrentWave") ? 
                            (int)props[$"Cloud_{nameKey}_CurrentWave"] : 0;
                        int totalKills = props.ContainsKey($"Cloud_{nameKey}_TotalKills") ? 
                            (int)props[$"Cloud_{nameKey}_TotalKills"] : 0;

                        bestRecordsByName[playerName] = new LeaderboardEntry
                        {
                            playerName = playerName,
                            currentWave = currentWave,
                            highestWave = highestWave,
                            totalKills = totalKills,
                            isLocalPlayer = player.IsLocal && playerName == this.playerName
                        };
                    }
                }
            }
        }

        // Convert dictionary to list
        leaderboard = bestRecordsByName.Values.ToList();

        // Sort by highest wave (descending), then by current wave
        leaderboard.Sort((a, b) =>
        {
            int compare = b.highestWave.CompareTo(a.highestWave);
            if (compare == 0)
                compare = b.currentWave.CompareTo(a.currentWave);
            return compare;
        });

        LogDebug($"Generated name-based leaderboard with {leaderboard.Count} unique names");
        return leaderboard;
    }

    /// <summary>
    /// Get player's rank on the leaderboard (1-based)
    /// </summary>
    public int GetPlayerRank()
    {
        List<LeaderboardEntry> leaderboard = GetLeaderboardData();
        
        for (int i = 0; i < leaderboard.Count; i++)
        {
            if (leaderboard[i].isLocalPlayer)
            {
                return i + 1; // 1-based rank
            }
        }

        return -1; // Not found
    }

    #endregion

    #region UI Updates

    /// <summary>
    /// Update status UI text
    /// </summary>
    private void UpdateStatusUI()
    {
        if (statusText != null)
        {
            if (isInLobby)
            {
                statusText.text = $"Lobby: {lobbyRoomName}";
            }
            else if (isConnecting)
            {
                statusText.text = "Connecting...";
            }
            else if (PhotonNetwork.IsConnected)
            {
                statusText.text = "Connected";
            }
            else
            {
                statusText.text = "Disconnected";
            }
        }

        if (playerCountText != null && PhotonNetwork.CurrentRoom != null)
        {
            playerCountText.text = $"Players Online: {PhotonNetwork.CurrentRoom.PlayerCount}";
        }
    }

    #endregion

    #region Public Getters

    public string GetPlayerName() => playerName;
    public int GetCurrentWave() => currentWaveReached;
    public int GetHighestWave() => highestWaveReached;
    public bool IsInLobby() => isInLobby;
    public bool IsConnectedToPhoton() => PhotonNetwork.IsConnected;
    public int GetPlayerCount() => PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
    public string GetLobbyName() => lobbyRoomName;

    /// <summary>
    /// Manually trigger a save to Photon Cloud (for testing or forced saves)
    /// </summary>
    public void ManualSaveToCloud()
    {
        SavePlayerStatsToCloud();
        LogDebug("Manual cloud save triggered");
    }

    /// <summary>
    /// Manually trigger a load from Photon Cloud (for testing or refresh)
    /// </summary>
    public void ManualLoadFromCloud()
    {
        LoadPlayerStatsFromCloud();
        LogDebug("Manual cloud load triggered");
    }

    /// <summary>
    /// Force reconnection to WaveManager (call this when player spawns)
    /// </summary>
    public void ForceConnectToWaveManager()
    {
        Debug.Log("[PhotonGameManager] Force connecting to WaveManager...");
        TryFindWaveManager();
        
        if (waveManager != null)
        {
            Debug.Log("[PhotonGameManager] ✓ Force connection successful!");
        }
        else
        {
            Debug.LogWarning("[PhotonGameManager] ⚠️ Force connection failed - WaveManager still not found");
        }
    }

    /// <summary>
    /// Check if cloud data exists for this player
    /// </summary>
    public bool HasCloudData()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
            return false;

        string cloudHighestWaveKey = "Cloud_" + HIGHEST_WAVE_KEY;
        return PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(cloudHighestWaveKey);
    }

    /// <summary>
    /// Get detailed cloud storage status for debugging
    /// </summary>
    public string GetCloudStorageStatus()
    {
        if (!PhotonNetwork.IsConnected)
            return "Not connected to Photon";

        if (PhotonNetwork.LocalPlayer == null)
            return "Local player not initialized";

        var cloudProps = PhotonNetwork.LocalPlayer.CustomProperties;
        string cloudHighestWaveKey = "Cloud_" + HIGHEST_WAVE_KEY;
        string cloudPlayerNameKey = "Cloud_" + PLAYER_NAME_KEY;

        bool hasCloudData = cloudProps.ContainsKey(cloudHighestWaveKey);
        int cloudWave = hasCloudData ? (int)cloudProps[cloudHighestWaveKey] : 0;
        string cloudName = cloudProps.ContainsKey(cloudPlayerNameKey) 
            ? (string)cloudProps[cloudPlayerNameKey] : "N/A";
        string lastSaved = cloudProps.ContainsKey("LastSaved") 
            ? (string)cloudProps["LastSaved"] : "Never";

        return $"Cloud Storage Status:\n" +
               $"- Has Data: {hasCloudData}\n" +
               $"- Name: {cloudName}\n" +
               $"- Highest Wave: {cloudWave}\n" +
               $"- Last Saved: {lastSaved}\n" +
               $"- Local Highest Wave: {highestWaveReached}";
    }

    #endregion

    #region Debugging

    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[PhotonGameManager] {message}");
        }
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// DEBUG: Check WaveManager connection status
    /// </summary>
    [ContextMenu("Debug WaveManager Connection")]
    public void DebugWaveManagerConnection()
    {
        Debug.Log("[PhotonGameManager] === WAVEMANAGER DEBUG ====");
        Debug.Log($"WaveManager assigned: {(waveManager != null ? "✓ YES" : "❌ NO")}");
        
        if (waveManager != null)
        {
            Debug.Log($"WaveManager GameObject: {waveManager.gameObject.name}");
            Debug.Log($"WaveManager enabled: {waveManager.enabled}");
            Debug.Log($"WaveManager active: {waveManager.gameObject.activeInHierarchy}");
            Debug.Log($"Current Wave from WaveManager: {waveManager.GetCurrentWave()}");
            Debug.Log($"WaveManager OnWaveStart listeners: {(waveManager.OnWaveStart != null ? waveManager.OnWaveStart.GetPersistentEventCount().ToString() : "NULL")}");
        }
        else
        {
            Debug.LogError("WaveManager is NULL! Searching in scene...");
            
            WaveManager[] allWaveManagers = FindObjectsOfType<WaveManager>();
            Debug.Log($"Found {allWaveManagers.Length} WaveManager(s) in scene:");
            
            for (int i = 0; i < allWaveManagers.Length; i++)
            {
                var wm = allWaveManagers[i];
                Debug.Log($"  [{i}] {wm.gameObject.name} - Active: {wm.gameObject.activeInHierarchy} - Enabled: {wm.enabled}");
            }
            
            if (allWaveManagers.Length > 0)
            {
                Debug.LogWarning("WaveManagers exist but not connected to PhotonGameManager! Please assign in inspector.");
            }
        }
        
        Debug.Log($"PhotonGameManager currentWaveReached: {currentWaveReached}");
        Debug.Log($"PhotonGameManager highestWaveReached: {highestWaveReached}");
        Debug.Log("[PhotonGameManager] === END DEBUG ====");
    }

    /// <summary>
    /// Dynamically find and connect to WaveManager (for when it's on player prefab)
    /// </summary>
    private void TryFindWaveManager()
    {
        // Search for WaveManager in the scene
        WaveManager foundWaveManager = FindObjectOfType<WaveManager>();
        
        if (foundWaveManager != null && foundWaveManager != waveManager)
        {
            Debug.Log($"[PhotonGameManager] ✓ WaveManager found dynamically: {foundWaveManager.gameObject.name}");
            
            // Unsubscribe from old manager if it exists
            if (waveManager != null)
            {
                waveManager.OnWaveStart.RemoveListener(OnWaveStarted);
                waveManager.OnWaveComplete.RemoveListener(OnWaveCompleted);
                Debug.Log("[PhotonGameManager] Unsubscribed from old WaveManager");
            }
            
            // Connect to new WaveManager
            waveManager = foundWaveManager;
            
            // Subscribe to wave events
            waveManager.OnWaveStart.AddListener(OnWaveStarted);
            waveManager.OnWaveComplete.AddListener(OnWaveCompleted);
            
            Debug.Log($"[PhotonGameManager] ✓ Successfully connected to WaveManager on {waveManager.gameObject.name}");
            
            // Get current wave if available
            int currentWaveFromManager = waveManager.GetCurrentWave();
            if (currentWaveFromManager > 0)
            {
                Debug.Log($"[PhotonGameManager] Syncing with current wave: {currentWaveFromManager}");
                currentWaveReached = currentWaveFromManager;
                
                if (currentWaveReached > highestWaveReached)
                {
                    highestWaveReached = currentWaveReached;
                    SaveHighestWaveLocally(highestWaveReached);
                }
            }
        }
    }

    #endregion
    
    #region Scene Reset Methods

    /// <summary>
    /// Reset current session state while preserving leaderboard data
    /// Used when quitting to main menu for a fresh start
    /// </summary>
    public void ResetCurrentSession()
    {
        LogDebug("Resetting PhotonGameManager session state...");
        
        // Reset current wave to 0 (but keep highest wave for leaderboards)
        currentWaveReached = 0;
        
        // Update room properties to reflect reset state
        UpdatePlayerProperties();
        
        // Disconnect from current WaveManager (will reconnect when new player spawns)
        if (waveManager != null)
        {
            waveManager.OnWaveStart.RemoveListener(OnWaveStarted);
            waveManager.OnWaveComplete.RemoveListener(OnWaveCompleted);
            waveManager = null;
        }
        
        LogDebug($"Session reset complete. Current wave: {currentWaveReached}, Highest wave preserved: {highestWaveReached}");
    }

    #endregion

    private void OnApplicationQuit()
    {
        // Save final stats to cloud before application closes
        SavePlayerStatsToCloud();
        LogDebug("Application quitting - saving final stats to Photon Cloud");
    }

    private void OnDestroy()
    {
        // Save stats one last time before object destruction
        SavePlayerStatsToCloud();
        
        // Unsubscribe from wave events
        if (waveManager != null)
        {
            waveManager.OnWaveStart.RemoveListener(OnWaveStarted);
            waveManager.OnWaveComplete.RemoveListener(OnWaveCompleted);
        }
    }
}

/// <summary>
/// Leaderboard entry data structure
/// </summary>
[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int currentWave;
    public int highestWave;
    public int totalKills;
    public bool isLocalPlayer;
}
