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
/// - Persistent player stats across sessions
/// - Automatic reconnection handling
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

        // Load saved player name
        LoadPlayerName();
    }

    private void Start()
    {
        // Set Photon settings
        PhotonNetwork.AutomaticallySyncScene = false; // We use static lobby, no scene sync needed
        PhotonNetwork.SendRate = 20; // Updates per second
        PhotonNetwork.SerializationRate = 10; // Sync rate for custom properties

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
        LogDebug("Left the lobby room");
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
    /// Update player's custom properties in Photon
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

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
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
                LeaderboardEntry entry = new LeaderboardEntry
                {
                    playerName = (string)player.CustomProperties[PLAYER_NAME_KEY],
                    currentWave = player.CustomProperties.ContainsKey(CURRENT_WAVE_KEY) 
                        ? (int)player.CustomProperties[CURRENT_WAVE_KEY] : 0,
                    highestWave = player.CustomProperties.ContainsKey(HIGHEST_WAVE_KEY) 
                        ? (int)player.CustomProperties[HIGHEST_WAVE_KEY] : 0,
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

    private void OnDestroy()
    {
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
