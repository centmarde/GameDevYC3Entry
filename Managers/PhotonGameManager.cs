using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;


public partial class PhotonGameManager : MonoBehaviourPunCallbacks
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
        
        // Join or create the static lobby (cloud data will be loaded after joining room)
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

        // Load player stats from Photon Cloud (now that we're in a room)
        LoadPlayerStatsFromCloud();

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

    // The large Player Data, Wave Tracking, Leaderboard, UI and Debug methods
    // have been moved to partial class files to keep this file concise.
    // See the partial files in the same folder (PhotonGameManager.*.cs)

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
        
        // Unsubscribe from wave events (if any)
        if (waveManager != null)
        {
            waveManager.OnWaveStart.RemoveListener(OnWaveStarted);
            waveManager.OnWaveComplete.RemoveListener(OnWaveCompleted);
        }
    }

    #endregion
}
