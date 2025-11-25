using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Coop Controller - Handles multiplayer lobby UI interactions with Photon networking
/// 
/// QUICK SETUP DOCUMENTATION:
/// ==========================
/// 1. Create a Canvas in your Coop scene (GameObject > UI > Canvas)
/// 2. Add this script to a GameObject with PhotonView component
/// 3. Create UI elements:
///    - InputField for player name (UI > Input Field - TextMeshPro)
///    - Button for Create Room (UI > Button - TextMeshPro)
///    - Button for Join Room (UI > Button - TextMeshPro)
///    - Button for Start Game (UI > Button - TextMeshPro) 
///    - Button for Back (UI > Button - TextMeshPro)
///    - InputField for room name (UI > Input Field - TextMeshPro)
///    - Text for connection status (UI > Text - TextMeshPro)
///    - Text for player list (UI > Text - TextMeshPro)
/// 4. Assign references in the Inspector
/// 5. Configure PhotonView to observe this script
/// 6. Set up Photon App ID in PhotonServerSettings
/// 7. Press Play to test!
/// 
/// NETWORKING FEATURES:
/// - Real-time player name synchronization via PhotonView
/// - Room creation and joining
/// - Player list updates
/// - Master client game start control
/// - Connection status monitoring
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class CoopController : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("UI References - Player Input")]
    [Tooltip("Reference to the player name input field")]
    [SerializeField] private TMP_InputField playerNameInput;
    
    [Tooltip("Reference to the room name input field")]
    [SerializeField] private TMP_InputField roomNameInput;

    [Header("UI References - Buttons")]
    [Tooltip("Reference to the Create Room button")]
    [SerializeField] private Button createRoomButton;
    
    [Tooltip("Reference to the Join Room button")]
    [SerializeField] private Button joinRoomButton;
    
    [Tooltip("Reference to the Ready button")]
    [SerializeField] private Button readyButton;
    
    [Tooltip("Reference to the Start Game button (Master Client only)")]
    [SerializeField] private Button startGameButton;
    
    [Tooltip("Reference to the Back button")]
    [SerializeField] private Button backButton;
    
    [Tooltip("Reference to the Leave Room button (shown when in a room)")]
    [SerializeField] private Button leaveRoomButton;

    [Header("UI References - Status")]
    [Tooltip("Text to show connection and room status")]
    [SerializeField] private TextMeshProUGUI statusText;
    
    [Tooltip("Text to show list of players in room")]
    [SerializeField] private TextMeshProUGUI playersListText;
    
    [Tooltip("Text to show room information")]
    [SerializeField] private TextMeshProUGUI roomInfoText;
    
    [Tooltip("Text to show ready status")]
    [SerializeField] private TextMeshProUGUI readyStatusText;

    [Header("Scene Settings")]
    [Tooltip("Name of the main game scene to load when Start Game is clicked")]
    [SerializeField] private string gameSceneName = "MainBase";
    
    [Tooltip("Name of the main menu scene to load when Back is clicked")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Room Settings")]
    [Tooltip("Maximum players allowed in a room")]
    [SerializeField] private int maxPlayersPerRoom = 4;
    
    [Tooltip("Default room name prefix")]
    [SerializeField] private string defaultRoomPrefix = "Room";
    
    [Header("Ready System Settings")]
    [Tooltip("Require all players to be ready before starting game")]
    [SerializeField] private bool requireAllPlayersReady = true;
    
    [Header("Auto-Setup")]
    [Tooltip("Automatically create UI if not assigned in Inspector")]
    [SerializeField] private bool autoSetupUI = true;

    [Header("Settings")]
    [Tooltip("Minimum character length for player name")]
    [SerializeField] private int minNameLength = 3;
    
    [Tooltip("Maximum character length for player name")]
    [SerializeField] private int maxNameLength = 20;
    
    [Tooltip("Minimum character length for room name")]
    [SerializeField] private int minRoomNameLength = 3;
    
    [Tooltip("Maximum character length for room name")]
    [SerializeField] private int maxRoomNameLength = 15;

    [Header("Audio Settings")]
    [Tooltip("Delay in seconds before executing button action (allows audio to play)")]
    [SerializeField] private float buttonClickDelay = 0.5f;

    [Header("Animation Settings")]
    [Tooltip("Duration of shake animation in seconds")]
    [SerializeField] private float shakeDuration = 0.5f;
    
    [Tooltip("Intensity of shake animation")]
    [SerializeField] private float shakeIntensity = 10f;

    // Networking state
    private PhotonView photonView;
    private string networkPlayerName = "";
    private Dictionary<int, string> playersInRoom = new Dictionary<int, string>();
    private Dictionary<int, bool> playersReadyStatus = new Dictionary<int, bool>();
    private bool isLocalPlayerReady = false;
    
    // UI auto-setup references
    private Canvas canvasReference;
    
    // Animation state tracking
    private bool isShaking = false;
    private Vector3 originalInputPosition;
    private Vector3 originalRoomInputPosition;
    private Vector3 originalButtonPosition;
    
    // Track if a button action is already in progress to prevent multiple clicks
    private bool isProcessingButtonClick = false;
    
    // Real-time input sync
    private float lastInputSyncTime = 0f;
    private const float INPUT_SYNC_RATE = 0.1f; // Sync 10 times per second

    #region Unity Lifecycle

    private void Start()
    {
        // Get PhotonView component
        photonView = GetComponent<PhotonView>();
        if (photonView == null)
        {
            Debug.LogError("CoopController: PhotonView component is missing!");
            return;
        }

        // Auto-setup UI if needed
        if (autoSetupUI)
        {
            AutoSetupUI();
        }
        
        // Initialize UI
        InitializeUI();
        
        // Connect to Photon if not already connected
        ConnectToPhoton();
        
        // Store original positions for animations
        StoreOriginalPositions();
        
        // Load saved player name
        LoadPlayerName();
        
        // Set initial status
        UpdateStatusText("Connecting to Photon...");
    }

    private void Update()
    {
        // Sync input field changes in real-time
        SyncInputFieldsRealTime();
        
        // Update UI state
        UpdateUIState();
    }

    #endregion

    #region UI Initialization

    private void InitializeUI()
    {
        // Player Name Input
        if (playerNameInput == null)
        {
            Debug.LogError("CoopController: Player Name Input is not assigned in the Inspector!");
        }
        else
        {
            playerNameInput.characterLimit = maxNameLength;
            playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
        }

        // Room Name Input  
        if (roomNameInput == null)
        {
            Debug.LogError("CoopController: Room Name Input is not assigned in the Inspector!");
        }
        else
        {
            roomNameInput.characterLimit = maxRoomNameLength;
            roomNameInput.onValueChanged.AddListener(OnRoomNameChanged);
            roomNameInput.text = $"{defaultRoomPrefix}{Random.Range(1000, 9999)}";
        }

        // Create Room Button
        if (createRoomButton == null)
        {
            Debug.LogError("CoopController: Create Room Button is not assigned in the Inspector!");
        }
        else
        {
            createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
        }

        // Join Room Button
        if (joinRoomButton == null)
        {
            Debug.LogError("CoopController: Join Room Button is not assigned in the Inspector!");
        }
        else
        {
            joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
        }

        // Ready Button
        if (readyButton == null)
        {
            Debug.LogError("CoopController: Ready Button is not assigned in the Inspector!");
        }
        else
        {
            readyButton.onClick.AddListener(OnReadyButtonClicked);
            readyButton.gameObject.SetActive(false); // Hidden until in room
        }
        
        // Start Game Button
        if (startGameButton == null)
        {
            Debug.LogError("CoopController: Start Game Button is not assigned in the Inspector!");
        }
        else
        {
            startGameButton.onClick.AddListener(OnStartGameButtonClicked);
            startGameButton.gameObject.SetActive(false); // Hidden until in room
        }

        // Back Button
        if (backButton == null)
        {
            Debug.LogError("CoopController: Back Button is not assigned in the Inspector!");
        }
        else
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        
        // Leave Room Button
        if (leaveRoomButton != null)
        {
            leaveRoomButton.onClick.AddListener(OnLeaveRoomButtonClicked);
            leaveRoomButton.gameObject.SetActive(false); // Hidden by default until in a room
        }

        // Optional: Ensure cursor is visible and unlocked
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void StoreOriginalPositions()
    {
        if (playerNameInput != null)
        {
            originalInputPosition = playerNameInput.transform.localPosition;
        }
        
        if (roomNameInput != null)
        {
            originalRoomInputPosition = roomNameInput.transform.localPosition;
        }
        
        if (createRoomButton != null)
        {
            originalButtonPosition = createRoomButton.transform.localPosition;
        }
    }

    #endregion

    #region Photon Connection

    private void ConnectToPhoton()
    {
        // If we're in a room or on game server, disconnect first
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("[CoopController] Leaving current room before reconnecting...");
            PhotonNetwork.LeaveRoom();
            return; // OnLeftRoom callback will reconnect
        }
        
        // If in lobby, leave it
        if (PhotonNetwork.InLobby)
        {
            Debug.Log("[CoopController] Leaving lobby before proceeding...");
            PhotonNetwork.LeaveLobby();
            return;
        }
        
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("[CoopController] Connecting to Photon (without lobby)...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("[CoopController] Already connected to Photon Master Server");
            UpdateStatusText("Connected to Photon. Ready to create or join room.");
        }
        else
        {
            Debug.Log("[CoopController] Connected but not ready. Waiting for Master Server...");
            UpdateStatusText("Connecting to Master Server...");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[CoopController] Connected to Photon Master Server");
        UpdateStatusText("Connected to Photon. Ready to create or join room.");
        
        // Set player nickname from input field if available
        if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
        {
            PhotonNetwork.NickName = playerNameInput.text.Trim();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[CoopController] Disconnected from Photon: {cause}");
        UpdateStatusText($"Disconnected: {cause}. Reconnecting...");
        
        // Try to reconnect unless it was intentional
        if (cause != DisconnectCause.DisconnectByClientLogic && cause != DisconnectCause.ApplicationQuit)
        {
            StartCoroutine(ReconnectWithDelay(1f));
        }
    }
    
    private IEnumerator ReconnectWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ConnectToPhoton();
    }

    #endregion

    #region Room Management

    private void OnCreateRoomButtonClicked()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(CreateRoomCoroutine());
    }

    private IEnumerator CreateRoomCoroutine()
    {
        isProcessingButtonClick = true;
        
        if (!ValidateInputs())
        {
            isProcessingButtonClick = false;
            yield break;
        }

        yield return new WaitForSeconds(buttonClickDelay);

        // Ensure we're connected to Master Server
        if (!PhotonNetwork.IsConnectedAndReady || PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[CoopController] Not ready to create room. Reconnecting to Master Server...");
            UpdateStatusText("Reconnecting to Master Server...");
            
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                yield return new WaitForSeconds(1f);
            }
            
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
            
            // Wait for connection to Master Server
            float timeout = 10f;
            float elapsed = 0f;
            while (!PhotonNetwork.IsConnectedAndReady && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                Debug.LogError("[CoopController] Failed to connect to Master Server");
                UpdateStatusText("Failed to connect. Please try again.");
                isProcessingButtonClick = false;
                yield break;
            }
        }

        string roomName = roomNameInput.text.Trim();
        string playerName = playerNameInput.text.Trim();
        
        // Set player nickname
        PhotonNetwork.NickName = playerName;
        
        // Create room options
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayersPerRoom,
            IsOpen = true,
            IsVisible = true
        };
        
        Debug.Log($"[CoopController] Creating room: {roomName} with player: {playerName}");
        UpdateStatusText($"Creating room '{roomName}'...");
        
        PhotonNetwork.CreateRoom(roomName, roomOptions);
        
        isProcessingButtonClick = false;
    }

    private void OnJoinRoomButtonClicked()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(JoinRoomCoroutine());
    }

    private IEnumerator JoinRoomCoroutine()
    {
        isProcessingButtonClick = true;
        
        if (!ValidateInputs())
        {
            isProcessingButtonClick = false;
            yield break;
        }

        yield return new WaitForSeconds(buttonClickDelay);

        // Ensure we're connected to Master Server
        if (!PhotonNetwork.IsConnectedAndReady || PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[CoopController] Not ready to join room. Reconnecting to Master Server...");
            UpdateStatusText("Reconnecting to Master Server...");
            
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
                yield return new WaitForSeconds(1f);
            }
            
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
            
            // Wait for connection to Master Server
            float timeout = 10f;
            float elapsed = 0f;
            while (!PhotonNetwork.IsConnectedAndReady && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                Debug.LogError("[CoopController] Failed to connect to Master Server");
                UpdateStatusText("Failed to connect. Please try again.");
                isProcessingButtonClick = false;
                yield break;
            }
        }

        string roomName = roomNameInput.text.Trim();
        string playerName = playerNameInput.text.Trim();
        
        // Set player nickname
        PhotonNetwork.NickName = playerName;
        
        Debug.Log($"[CoopController] Joining room: {roomName} with player: {playerName}");
        UpdateStatusText($"Joining room '{roomName}'...");
        
        PhotonNetwork.JoinRoom(roomName);
        
        isProcessingButtonClick = false;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[CoopController] Successfully joined room: {PhotonNetwork.CurrentRoom.Name}");
        UpdateStatusText($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
        
        // Update UI for in-room state
        createRoomButton.gameObject.SetActive(false);
        joinRoomButton.gameObject.SetActive(false);
        roomNameInput.interactable = false;
        
        // Show ready button
        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(true);
            UpdateReadyButtonText();
        }
        
        // Show start game button if master client (initially disabled)
        if (PhotonNetwork.IsMasterClient && startGameButton != null)
        {
            startGameButton.gameObject.SetActive(true);
            startGameButton.interactable = false; // Disabled until all ready
        }
        
        // Update room info
        UpdateRoomInfo();
        UpdatePlayersList();
        
        // Save player name
        SavePlayerName(PhotonNetwork.NickName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[CoopController] Failed to join room: {message}");
        UpdateStatusText($"Failed to join room: {message}");
        TriggerValidationShake();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[CoopController] Failed to create room: {message}");
        UpdateStatusText($"Failed to create room: {message}");
        TriggerValidationShake();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[CoopController] Left room");
        UpdateStatusText("Left room. Ready to create or join room.");
        
        // Reset UI for lobby state
        if (createRoomButton != null) createRoomButton.gameObject.SetActive(true);
        if (joinRoomButton != null) joinRoomButton.gameObject.SetActive(true);
        if (readyButton != null) readyButton.gameObject.SetActive(false);
        if (startGameButton != null) startGameButton.gameObject.SetActive(false);
        if (roomNameInput != null) roomNameInput.interactable = true;
        
        // Reset ready status
        isLocalPlayerReady = false;
        playersReadyStatus.Clear();
        
        // Clear room info
        UpdateRoomInfo();
        UpdatePlayersList();
        UpdateReadyStatus();
        
        // Reconnect to master server after leaving room if needed
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("[CoopController] Not connected to Master Server, reconnecting...");
            StartCoroutine(ReconnectWithDelay(0.5f));
        }
    }
    
    public override void OnLeftLobby()
    {
        Debug.Log("[CoopController] Left lobby successfully");
        // After leaving lobby, ensure we're ready to create/join rooms
        if (PhotonNetwork.IsConnectedAndReady)
        {
            UpdateStatusText("Connected to Photon. Ready to create or join room.");
        }
    }

    #endregion

    #region Player Management

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"[CoopController] Player joined: {newPlayer.NickName}");
        UpdatePlayersList();
        UpdateRoomInfo();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"[CoopController] Player left: {otherPlayer.NickName}");
        UpdatePlayersList();
        UpdateRoomInfo();
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log($"[CoopController] New master client: {newMasterClient.NickName}");
        
        // Show/hide start game button based on master client status
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }
    }

    private void UpdatePlayersList()
    {
        if (playersListText == null) return;

        if (PhotonNetwork.CurrentRoom == null)
        {
            playersListText.text = "Players: Not in room";
            return;
        }

        string playersList = "Players in room:\n";
        
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            string prefix = player.IsMasterClient ? "[HOST] " : "";
            string suffix = player.IsLocal ? " (You)" : "";
            
            // Show real-time input if this player is typing
            string playerName = player.NickName;
            if (playersInRoom.ContainsKey(player.ActorNumber))
            {
                string networkName = playersInRoom[player.ActorNumber];
                if (!string.IsNullOrEmpty(networkName) && networkName != player.NickName)
                {
                    playerName = $"{networkName} (typing...)";
                }
            }
            
            // Show ready status
            string readyStatus = "";
            if (playersReadyStatus.ContainsKey(player.ActorNumber) && playersReadyStatus[player.ActorNumber])
            {
                readyStatus = " ✓ READY";
            }
            else if (PhotonNetwork.CurrentRoom != null)
            {
                readyStatus = " ⏳ Not Ready";
            }
            
            playersList += $"{prefix}{playerName}{suffix}{readyStatus}\n";
        }
        
        playersListText.text = playersList.TrimEnd();
    }

    private void UpdateRoomInfo()
    {
        if (roomInfoText == null) return;

        if (PhotonNetwork.CurrentRoom == null)
        {
            roomInfoText.text = "";
            return;
        }

        string roomInfo = $"Room: {PhotonNetwork.CurrentRoom.Name}\n";
        roomInfo += $"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        
        roomInfoText.text = roomInfo;
    }

    #endregion

    #region Input Validation and Sync

    private void OnPlayerNameChanged(string value)
    {
        ValidateInputs();
        
        // Update network player name for real-time sync
        networkPlayerName = value;
    }

    private void OnRoomNameChanged(string value)
    {
        ValidateInputs();
    }

    private bool ValidateInputs()
    {
        if (playerNameInput == null || roomNameInput == null) return false;

        string playerName = playerNameInput.text.Trim();
        string roomName = roomNameInput.text.Trim();
        
        bool playerNameValid = playerName.Length >= minNameLength && playerName.Length <= maxNameLength;
        bool roomNameValid = roomName.Length >= minRoomNameLength && roomName.Length <= maxRoomNameLength;
        
        // Enable/disable buttons based on validation
        if (createRoomButton != null)
        {
            createRoomButton.interactable = playerNameValid && roomNameValid && PhotonNetwork.IsConnected;
        }
        
        if (joinRoomButton != null)
        {
            joinRoomButton.interactable = playerNameValid && roomNameValid && PhotonNetwork.IsConnected;
        }
        
        // Trigger shake for invalid input
        if (!playerNameValid && playerName.Length > 0)
        {
            TriggerValidationShake();
            return false;
        }
        
        if (!roomNameValid && roomName.Length > 0)
        {
            TriggerValidationShake();
            return false;
        }
        
        return playerNameValid && roomNameValid;
    }

    /// <summary>
    /// Sync input fields in real-time via PhotonView
    /// </summary>
    private void SyncInputFieldsRealTime()
    {
        if (Time.time - lastInputSyncTime < INPUT_SYNC_RATE) return;
        
        lastInputSyncTime = Time.time;
        
        if (playerNameInput != null && photonView != null && photonView.IsMine)
        {
            string currentName = playerNameInput.text;
            if (currentName != networkPlayerName)
            {
                networkPlayerName = currentName;
                // The IPunObservable will sync this automatically
            }
        }
    }

    /// <summary>
    /// IPunObservable implementation for real-time input sync
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send our data to other players
            stream.SendNext(networkPlayerName);
            stream.SendNext(isLocalPlayerReady);
        }
        else
        {
            // Receive data from other players
            string receivedName = (string)stream.ReceiveNext();
            bool receivedReadyStatus = (bool)stream.ReceiveNext();
            
            // Store the player's current input and ready status
            if (info.Sender != null)
            {
                playersInRoom[info.Sender.ActorNumber] = receivedName;
                playersReadyStatus[info.Sender.ActorNumber] = receivedReadyStatus;
                
                // Update players list to show real-time typing and ready status
                if (PhotonNetwork.CurrentRoom != null)
                {
                    UpdatePlayersList();
                    UpdateReadyStatus();
                    UpdateStartButtonState();
                }
            }
        }
    }

    #endregion

    #region Ready System

    private void OnReadyButtonClicked()
    {
        isLocalPlayerReady = !isLocalPlayerReady;
        UpdateReadyButtonText();
        UpdateReadyStatus();
        UpdateStartButtonState();
        
        Debug.Log($"[CoopController] Local player ready status: {isLocalPlayerReady}");
    }

    private void UpdateReadyButtonText()
    {
        if (readyButton == null) return;
        
        TextMeshProUGUI buttonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = isLocalPlayerReady ? "CANCEL READY" : "READY";
        }
        
        // Change button color based on ready status
        ColorBlock colors = readyButton.colors;
        colors.normalColor = isLocalPlayerReady ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.8f, 0.8f);
        readyButton.colors = colors;
    }

    private void UpdateReadyStatus()
    {
        if (readyStatusText == null) return;
        
        if (PhotonNetwork.CurrentRoom == null)
        {
            readyStatusText.text = "";
            return;
        }
        
        int readyCount = 0;
        int totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        
        // Count ready players by checking each player in the room
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsLocal)
            {
                if (isLocalPlayerReady) readyCount++;
            }
            else
            {
                if (playersReadyStatus.ContainsKey(player.ActorNumber) && 
                    playersReadyStatus[player.ActorNumber])
                {
                    readyCount++;
                }
            }
        }
        
        readyStatusText.text = $"Ready: {readyCount}/{totalPlayers}";
        
        if (readyCount == totalPlayers && totalPlayers > 0)
        {
            readyStatusText.text += " - ALL READY!";
            readyStatusText.color = new Color(0.2f, 1f, 0.2f); // Bright green
        }
        else
        {
            readyStatusText.color = new Color(1f, 1f, 0.2f); // Yellow
        }
    }

    private bool AreAllPlayersReady()
    {
        if (PhotonNetwork.CurrentRoom == null) return false;
        
        int totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        
        // Need at least 1 player to start
        if (totalPlayers == 0) return false;
        
        int readyCount = 0;
        
        // Check each player's ready status
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsLocal)
            {
                // Check local player's ready status
                if (isLocalPlayerReady)
                {
                    readyCount++;
                }
            }
            else
            {
                // Check remote player's ready status from synced dictionary
                if (playersReadyStatus.ContainsKey(player.ActorNumber) && 
                    playersReadyStatus[player.ActorNumber])
                {
                    readyCount++;
                }
            }
        }
        
        // All players must be ready
        bool allReady = readyCount == totalPlayers;
        
        if (!allReady)
        {
            Debug.Log($"[CoopController] Not all players ready: {readyCount}/{totalPlayers}");
        }
        
        return allReady;
    }

    private void UpdateStartButtonState()
    {
        if (startGameButton == null) return;
        
        bool allReady = AreAllPlayersReady();
        bool isMaster = PhotonNetwork.IsMasterClient;
        
        if (requireAllPlayersReady)
        {
            startGameButton.interactable = allReady && isMaster;
            
            // Update button text to show requirement
            TextMeshProUGUI buttonText = startGameButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                if (isMaster)
                {
                    if (allReady)
                    {
                        buttonText.text = "START GAME";
                        buttonText.color = Color.white;
                    }
                    else
                    {
                        buttonText.text = "WAITING FOR ALL PLAYERS...";
                        buttonText.color = new Color(0.7f, 0.7f, 0.7f);
                    }
                }
                else
                {
                    buttonText.text = "WAITING FOR HOST...";
                    buttonText.color = new Color(0.7f, 0.7f, 0.7f);
                }
            }
        }
        else
        {
            startGameButton.interactable = isMaster;
        }
    }

    #endregion

    #region Game Start

    private void OnStartGameButtonClicked()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        isProcessingButtonClick = true;
        
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[CoopController] Only master client can start the game!");
            UpdateStatusText("Only the room host can start the game.");
            isProcessingButtonClick = false;
            yield break;
        }
        
        if (requireAllPlayersReady && !AreAllPlayersReady())
        {
            int totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            int readyCount = 0;
            
            // Count ready players for detailed message
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            {
                if (player.IsLocal && isLocalPlayerReady) readyCount++;
                else if (!player.IsLocal && playersReadyStatus.ContainsKey(player.ActorNumber) && 
                         playersReadyStatus[player.ActorNumber]) readyCount++;
            }
            
            Debug.LogWarning($"[CoopController] Cannot start! Only {readyCount}/{totalPlayers} players are ready!");
            UpdateStatusText($"Cannot Start! Waiting for all players to be ready ({readyCount}/{totalPlayers})...");
            TriggerValidationShake();
            isProcessingButtonClick = false;
            yield break;
        }

        yield return new WaitForSeconds(buttonClickDelay);

        Debug.Log($"[CoopController] Starting multiplayer game...");
        UpdateStatusText("Starting game...");
        
        // Load game scene for all players
        PhotonNetwork.LoadLevel(gameSceneName);
        
        isProcessingButtonClick = false;
    }

    #endregion

    #region Navigation

    private void OnLeaveRoomButtonClicked()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(LeaveRoomCoroutine());
    }
    
    private IEnumerator LeaveRoomCoroutine()
    {
        isProcessingButtonClick = true;
        
        yield return new WaitForSeconds(buttonClickDelay);
        
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("[CoopController] Leaving room via Leave Room button...");
            UpdateStatusText("Leaving room...");
            
            PhotonNetwork.LeaveRoom();
            
            // Wait for room to be left
            float timeout = 5f;
            float elapsed = 0f;
            while (PhotonNetwork.InRoom && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            if (!PhotonNetwork.InRoom)
            {
                Debug.Log("[CoopController] Successfully left room");
                UpdateStatusText("Left room. Ready to create or join another room.");
            }
            else
            {
                Debug.LogWarning("[CoopController] Timeout while leaving room");
                UpdateStatusText("Error leaving room. Please try again.");
            }
        }
        else
        {
            Debug.LogWarning("[CoopController] Leave Room clicked but not in a room");
            UpdateStatusText("Not currently in a room.");
        }
        
        isProcessingButtonClick = false;
    }

    private void OnBackButtonClicked()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(BackButtonCoroutine());
    }

    private IEnumerator BackButtonCoroutine()
    {
        isProcessingButtonClick = true;
        
        yield return new WaitForSeconds(buttonClickDelay);

        Debug.Log("[CoopController] Returning to main menu...");
        UpdateStatusText("Leaving room and disconnecting...");
        
        // Leave room if in one
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("[CoopController] Leaving room...");
            PhotonNetwork.LeaveRoom();
            
            float timeout = 5f;
            float elapsed = 0f;
            while (PhotonNetwork.InRoom && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
        }
        
        // Disconnect from Photon to ensure clean state
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("[CoopController] Disconnecting from Photon...");
            PhotonNetwork.Disconnect();
            
            float timeout = 5f;
            float elapsed = 0f;
            while (PhotonNetwork.IsConnected && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
        }
        
        Debug.Log("[CoopController] Loading main menu...");
        
        // Load main menu scene
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.LoadScene(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        
        isProcessingButtonClick = false;
    }

    #endregion

    #region UI Updates

    private void UpdateUIState()
    {
        // Update connection-dependent buttons
        bool isConnected = PhotonNetwork.IsConnected;
        bool inRoom = PhotonNetwork.InRoom;
        
        if (createRoomButton != null)
        {
            createRoomButton.gameObject.SetActive(isConnected && !inRoom);
        }
        
        if (joinRoomButton != null)
        {
            joinRoomButton.gameObject.SetActive(isConnected && !inRoom);
        }
        
        // Show Leave Room button only when in a room
        if (leaveRoomButton != null)
        {
            leaveRoomButton.gameObject.SetActive(inRoom);
        }
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"Status: {message}";
        }
        
        Debug.Log($"[CoopController] {message}");
    }

    #endregion

    #region Animation

    private void TriggerValidationShake()
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeAnimation());
        }
    }

    private IEnumerator ShakeAnimation()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float percentComplete = elapsed / shakeDuration;
            float damper = 1.0f - Mathf.Clamp01(percentComplete);

            // Shake player name input
            if (playerNameInput != null)
            {
                float offsetX = Random.Range(-shakeIntensity, shakeIntensity) * damper;
                playerNameInput.transform.localPosition = originalInputPosition + new Vector3(offsetX, 0, 0);
            }

            // Shake room name input
            if (roomNameInput != null)
            {
                float offsetX = Random.Range(-shakeIntensity, shakeIntensity) * damper;
                roomNameInput.transform.localPosition = originalRoomInputPosition + new Vector3(offsetX, 0, 0);
            }

            // Shake button
            if (createRoomButton != null)
            {
                float offsetX = Random.Range(-shakeIntensity, shakeIntensity) * damper;
                float offsetY = Random.Range(-shakeIntensity, shakeIntensity) * damper;
                createRoomButton.transform.localPosition = originalButtonPosition + new Vector3(offsetX, offsetY, 0);
            }

            yield return null;
        }

        // Reset positions
        if (playerNameInput != null)
        {
            playerNameInput.transform.localPosition = originalInputPosition;
        }
        
        if (roomNameInput != null)
        {
            roomNameInput.transform.localPosition = originalRoomInputPosition;
        }
        
        if (createRoomButton != null)
        {
            createRoomButton.transform.localPosition = originalButtonPosition;
        }

        isShaking = false;
    }

    #endregion

    #region Player Preferences

    private void SavePlayerName(string playerName)
    {
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
        Debug.Log($"[CoopController] Player name saved: {playerName}");
    }

    private void LoadPlayerName()
    {
        if (playerNameInput != null && PlayerPrefs.HasKey("PlayerName"))
        {
            string savedName = PlayerPrefs.GetString("PlayerName");
            playerNameInput.text = savedName;
            networkPlayerName = savedName;
            Debug.Log($"[CoopController] Loaded saved player name: {savedName}");
        }
    }

    #endregion

    #region Public API

    public string GetPlayerName()
    {
        return playerNameInput != null ? playerNameInput.text.Trim() : string.Empty;
    }

    public void SetPlayerName(string name)
    {
        if (playerNameInput != null)
        {
            playerNameInput.text = name;
            networkPlayerName = name;
            ValidateInputs();
        }
    }

    public bool IsInRoom()
    {
        return PhotonNetwork.InRoom;
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public int GetPlayerCount()
    {
        return PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
    }

    #endregion

    #region Auto-Setup UI

    /// <summary>
    /// Automatically creates UI elements if not assigned in Inspector
    /// </summary>
    private void AutoSetupUI()
    {
        Debug.Log("[CoopController] Auto-setting up UI...");
        
        // Find or create Canvas
        canvasReference = FindObjectOfType<Canvas>();
        if (canvasReference == null)
        {
            GameObject canvasObj = new GameObject("CoopCanvas");
            canvasReference = canvasObj.AddComponent<Canvas>();
            canvasReference.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("[CoopController] Created new Canvas");
        }
        
        // Create main container
        GameObject mainContainer = new GameObject("CoopUIContainer");
        mainContainer.transform.SetParent(canvasReference.transform, false);
        RectTransform mainRect = mainContainer.AddComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.offsetMin = new Vector2(50, 50);
        mainRect.offsetMax = new Vector2(-50, -50);
        
        // Create Player Name Input (top-left)
        if (playerNameInput == null)
        {
            playerNameInput = CreateInputField(mainContainer.transform, "PlayerNameInput", "Enter your name...", 
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(300, 50));
        }
        
        // Create Room Name Input (top-center)
        if (roomNameInput == null)
        {
            roomNameInput = CreateInputField(mainContainer.transform, "RoomNameInput", "Room name...", 
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(300, 50));
        }
        
        // Create Status Text (top-right)
        if (statusText == null)
        {
            statusText = CreateTextLabel(mainContainer.transform, "StatusText", "Status: Initializing...", 
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -10), new Vector2(300, 50), TextAlignmentOptions.Right);
        }
        
        // Create Create Room Button (center-left)
        if (createRoomButton == null)
        {
            createRoomButton = CreateButton(mainContainer.transform, "CreateRoomButton", "CREATE ROOM", 
                new Vector2(0.25f, 0.6f), new Vector2(0.25f, 0.6f), Vector2.zero, new Vector2(200, 50));
        }
        
        // Create Join Room Button (center-right)
        if (joinRoomButton == null)
        {
            joinRoomButton = CreateButton(mainContainer.transform, "JoinRoomButton", "JOIN ROOM", 
                new Vector2(0.75f, 0.6f), new Vector2(0.75f, 0.6f), Vector2.zero, new Vector2(200, 50));
        }
        
        // Create Ready Button (center)
        if (readyButton == null)
        {
            readyButton = CreateButton(mainContainer.transform, "ReadyButton", "READY", 
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 50));
            readyButton.gameObject.SetActive(false);
        }
        
        // Create Start Game Button (center-bottom)
        if (startGameButton == null)
        {
            startGameButton = CreateButton(mainContainer.transform, "StartGameButton", "START GAME", 
                new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(250, 60));
            startGameButton.gameObject.SetActive(false);
            
            // Make start button stand out
            ColorBlock colors = startGameButton.colors;
            colors.normalColor = new Color(0.2f, 0.8f, 0.2f);
            startGameButton.colors = colors;
        }
        
        // Create Back Button (bottom-left)
        if (backButton == null)
        {
            backButton = CreateButton(mainContainer.transform, "BackButton", "BACK", 
                new Vector2(0, 0), new Vector2(0, 0), new Vector2(10, 10), new Vector2(150, 50));
        }
        
        // Create Players List (left side)
        if (playersListText == null)
        {
            playersListText = CreateTextLabel(mainContainer.transform, "PlayersListText", "Players: Not in room", 
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(250, 300), TextAlignmentOptions.TopLeft);
        }
        
        // Create Room Info (right side)
        if (roomInfoText == null)
        {
            roomInfoText = CreateTextLabel(mainContainer.transform, "RoomInfoText", "", 
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(250, 200), TextAlignmentOptions.TopRight);
        }
        
        // Create Ready Status (center-top under inputs)
        if (readyStatusText == null)
        {
            readyStatusText = CreateTextLabel(mainContainer.transform, "ReadyStatusText", "", 
                new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(300, 40), TextAlignmentOptions.Center);
            readyStatusText.fontSize = 20;
            readyStatusText.fontStyle = FontStyles.Bold;
        }
        
        Debug.Log("[CoopController] UI auto-setup complete!");
    }

    private TMP_InputField CreateInputField(Transform parent, string name, string placeholder, 
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        GameObject inputObj = new GameObject(name);
        inputObj.transform.SetParent(parent, false);
        
        RectTransform rect = inputObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x, anchorMax.y);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        
        Image bg = inputObj.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        TMP_InputField input = inputObj.AddComponent<TMP_InputField>();
        
        // Create text area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 5);
        textAreaRect.offsetMax = new Vector2(-10, -5);
        
        // Create placeholder
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(textArea.transform, false);
        RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 16;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
        placeholderText.alignment = TextAlignmentOptions.Left;
        
        // Create text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(textArea.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        
        input.textViewport = textAreaRect;
        input.textComponent = text;
        input.placeholder = placeholderText;
        
        return input;
    }

    private Button CreateButton(Transform parent, string name, string text, 
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        
        Image bg = buttonObj.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.3f, 0.3f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 18;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        return button;
    }

    private TextMeshProUGUI CreateTextLabel(Transform parent, string name, string text, 
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, TextAlignmentOptions alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x, anchorMax.y);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 16;
        textComponent.color = Color.white;
        textComponent.alignment = alignment;
        
        return textComponent;
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        // Clean up listeners
        if (playerNameInput != null)
        {
            playerNameInput.onValueChanged.RemoveListener(OnPlayerNameChanged);
        }
        
        if (roomNameInput != null)
        {
            roomNameInput.onValueChanged.RemoveListener(OnRoomNameChanged);
        }

        if (createRoomButton != null)
        {
            createRoomButton.onClick.RemoveListener(OnCreateRoomButtonClicked);
        }
        
        if (joinRoomButton != null)
        {
            joinRoomButton.onClick.RemoveListener(OnJoinRoomButtonClicked);
        }
        
        if (readyButton != null)
        {
            readyButton.onClick.RemoveListener(OnReadyButtonClicked);
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
        }
        
        if (leaveRoomButton != null)
        {
            leaveRoomButton.onClick.RemoveListener(OnLeaveRoomButtonClicked);
        }

        // Disconnect from Photon if still connected (safety cleanup)
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("[CoopController] OnDestroy: Disconnecting from Photon for cleanup");
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
            PhotonNetwork.Disconnect();
        }

        // Stop any running coroutines
        StopAllCoroutines();
    }

    #endregion
}