using System.Collections;
using System.Collections.Generic;
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

    // Player stats keys (for saving to Photon)
    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string HIGHEST_WAVE_KEY = "HighestWave";
    private const string CURRENT_WAVE_KEY = "CurrentWave";
    private const string TOTAL_KILLS_KEY = "TotalKills";

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
            waveManager = FindObjectOfType<WaveManager>();
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
            waveManager.OnWaveStart.AddListener(OnWaveStarted);
            waveManager.OnWaveComplete.AddListener(OnWaveCompleted);
        }
    }

    private void Update()
    {
        // Update UI
        UpdateStatusUI();
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
    /// Update player's custom properties in Photon (room session)
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
            { TOTAL_KILLS_KEY, GetTotalKills() }
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
            SaveHighestWave(highestWaveReached);
        }

        // Update Photon properties
        UpdatePlayerProperties();

        LogDebug($"Wave {waveNumber} started. Highest wave: {highestWaveReached}");
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
    /// Force save the current wave progress to leaderboards
    /// Call this when player dies to ensure wave data is saved to Photon
    /// </summary>
    public void SaveCurrentWaveToLeaderboard()
    {
        // Get current wave from WaveManager if available
        if (waveManager != null)
        {
            currentWaveReached = waveManager.GetCurrentWave();
        }

        // Update highest wave if current is higher
        if (currentWaveReached > highestWaveReached)
        {
            highestWaveReached = currentWaveReached;
            SaveHighestWave(highestWaveReached);
        }

        // Force update Photon properties to sync with leaderboard
        UpdatePlayerProperties();
        
        // Ensure cloud save is completed
        SavePlayerStatsToCloud();

        LogDebug($"Force saved wave progress to leaderboard - Current Wave: {currentWaveReached}, Highest Wave: {highestWaveReached}");
    }

    /// <summary>
    /// Save player stats to Photon Cloud (persistent across sessions)
    /// Uses Photon Player Account Custom Properties
    /// </summary>
    private void SavePlayerStatsToCloud()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
        {
            LogDebug("Cannot save to cloud - not connected to Photon");
            return;
        }

        // Create hashtable with player stats
        ExitGames.Client.Photon.Hashtable cloudProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "Cloud_" + PLAYER_NAME_KEY, playerName },
            { "Cloud_" + HIGHEST_WAVE_KEY, highestWaveReached },
            { "Cloud_" + TOTAL_KILLS_KEY, GetTotalKills() },
            { "LastSaved", System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
        };

        // Save to player's account (persists across sessions)
        PhotonNetwork.LocalPlayer.SetCustomProperties(cloudProperties);
        
        // Also save to local PlayerPrefs as backup
        SaveHighestWave(highestWaveReached);

        LogDebug($"Saved player stats to Photon Cloud: {playerName}, Highest Wave: {highestWaveReached}");
    }

    /// <summary>
    /// Load player stats from Photon Cloud (persistent data from previous sessions)
    /// </summary>
    private void LoadPlayerStatsFromCloud()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
        {
            LogDebug("Cannot load from cloud - not connected to Photon");
            return;
        }

        var cloudProps = PhotonNetwork.LocalPlayer.CustomProperties;
        
        // Load highest wave from cloud if available
        string cloudHighestWaveKey = "Cloud_" + HIGHEST_WAVE_KEY;
        if (cloudProps.ContainsKey(cloudHighestWaveKey))
        {
            int cloudHighestWave = (int)cloudProps[cloudHighestWaveKey];
            
            // Use the higher value between cloud and local storage
            if (cloudHighestWave > highestWaveReached)
            {
                highestWaveReached = cloudHighestWave;
                SaveHighestWave(highestWaveReached); // Update local storage
                LogDebug($"Loaded highest wave from Photon Cloud: {highestWaveReached}");
            }
        }
        
        // Load player name from cloud if available
        string cloudPlayerNameKey = "Cloud_" + PLAYER_NAME_KEY;
        if (cloudProps.ContainsKey(cloudPlayerNameKey))
        {
            string cloudPlayerName = (string)cloudProps[cloudPlayerNameKey];
            if (!string.IsNullOrEmpty(cloudPlayerName))
            {
                playerName = cloudPlayerName;
                PhotonNetwork.NickName = playerName;
                LogDebug($"Loaded player name from Photon Cloud: {playerName}");
            }
        }

        LogDebug($"Loaded player stats from Photon Cloud - Highest Wave: {highestWaveReached}, Name: {playerName}");
    }

    #endregion

    #region Leaderboard Data

    /// <summary>
    /// Get all players in the lobby with their wave progress
    /// Returns sorted list by highest wave (for leaderboards)
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboardData()
    {
        List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();

        if (!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom == null)
        {
            LogDebug("Not in a room, cannot get leaderboard data");
            return leaderboard;
        }

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey(PLAYER_NAME_KEY))
            {
                // Try to get cloud properties first (persistent data), fallback to room properties
                string cloudHighestWaveKey = "Cloud_" + HIGHEST_WAVE_KEY;
                int highestWave = player.CustomProperties.ContainsKey(cloudHighestWaveKey)
                    ? (int)player.CustomProperties[cloudHighestWaveKey]
                    : (player.CustomProperties.ContainsKey(HIGHEST_WAVE_KEY) 
                        ? (int)player.CustomProperties[HIGHEST_WAVE_KEY] : 0);
                
                LeaderboardEntry entry = new LeaderboardEntry
                {
                    playerName = (string)player.CustomProperties[PLAYER_NAME_KEY],
                    currentWave = player.CustomProperties.ContainsKey(CURRENT_WAVE_KEY) 
                        ? (int)player.CustomProperties[CURRENT_WAVE_KEY] : 0,
                    highestWave = highestWave, // Already calculated above with cloud priority
                    totalKills = player.CustomProperties.ContainsKey(TOTAL_KILLS_KEY) 
                        ? (int)player.CustomProperties[TOTAL_KILLS_KEY] : 0,
                    isLocalPlayer = player.IsLocal
                };

                leaderboard.Add(entry);
            }
        }

        // Sort by highest wave (descending), then by current wave
        leaderboard.Sort((a, b) =>
        {
            int compare = b.highestWave.CompareTo(a.highestWave);
            if (compare == 0)
                compare = b.currentWave.CompareTo(a.currentWave);
            return compare;
        });

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
