using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Photon Connection Checker - Displays real-time connection status
/// 
/// QUICK SETUP DOCUMENTATION:
/// ==========================
/// 1. Make sure Photon PUN2 is installed in your project:
///    - Window > Package Manager > Search for "PUN 2 - FREE"
///    - Or download from Asset Store
/// 
/// 2. Configure Photon Settings:
///    - Window > Photon Unity Networking > PUN Wizard
///    - Enter your App ID or create new one
/// 
/// 3. Add UI Text element:
///    - Create Canvas if not exists (GameObject > UI > Canvas)
///    - Add Text - TextMeshPro (UI > Text - TextMeshPro)
///    - Position it where you want to see connection status (e.g., top-right corner)
/// 
/// 4. Attach this script:
///    - Create empty GameObject or use existing UI element
///    - Add this PhotonConnectionChecker component
///    - Drag your TextMeshPro text to "Status Text" field in Inspector
/// 
/// 5. Optional: Customize in Inspector:
///    - Enable/disable auto-connect on start
///    - Set custom text messages for each connection state
///    - Configure update interval
///    - Choose text colors for different states
/// 
/// 6. Press Play to test!
///    - The text will show current connection status
///    - Updates automatically as connection state changes
/// 
/// FEATURES:
/// - Real-time connection status display
/// - Color-coded states (Connected = Green, Disconnected = Red, etc.)
/// - Automatic connection attempts (optional)
/// - Ping display when connected
/// - Player count display
/// - Customizable messages and colors
/// </summary>
public class PhotonConnectionChecker : MonoBehaviour, IConnectionCallbacks
{
    [Header("UI References")]
    [Tooltip("TextMeshPro component to display connection status")]
    [SerializeField] private TextMeshProUGUI statusText;
    
    [Tooltip("Optional: Legacy UI Text component (use this OR TextMeshPro, not both)")]
    [SerializeField] private UnityEngine.UI.Text legacyStatusText;

    [Header("Connection Settings")]
    [Tooltip("Automatically connect to Photon on Start")]
    [SerializeField] private bool autoConnectOnStart = true;
    
    [Tooltip("How often to update the status text (in seconds)")]
    [SerializeField] private float updateInterval = 1f;
    
    [Tooltip("Show ping information when connected")]
    [SerializeField] private bool showPing = true;
    
    [Tooltip("Show player count when connected")]
    [SerializeField] private bool showPlayerCount = true;

    [Header("Status Messages")]
    [SerializeField] private string connectedMessage = "Connected";
    [SerializeField] private string connectingMessage = "Connecting...";
    [SerializeField] private string disconnectedMessage = "Disconnected";
    [SerializeField] private string connectingToMasterMessage = "Connecting to Master...";
    [SerializeField] private string joiningLobbyMessage = "Joining Lobby...";
    [SerializeField] private string errorMessage = "Connection Error";

    [Header("Status Colors")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color connectingColor = Color.yellow;
    [SerializeField] private Color disconnectedColor = Color.red;
    [SerializeField] private Color errorColor = new Color(1f, 0.5f, 0f); // Orange

    private float updateTimer = 0f;
    private ClientState lastState = ClientState.Disconnected;

    private void Start()
    {
        // Validate references
        if (statusText == null && legacyStatusText == null)
        {
            Debug.LogError("PhotonConnectionChecker: No status text assigned! Please assign either TextMeshPro or Legacy Text in the Inspector.");
            enabled = false;
            return;
        }

        // Register callbacks
        PhotonNetwork.AddCallbackTarget(this);

        // Auto-connect if enabled
        if (autoConnectOnStart && !PhotonNetwork.IsConnected)
        {
            ConnectToPhoton();
        }

        // Initial update
        UpdateStatusDisplay();
    }

    private void Update()
    {
        // Update status display at intervals
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateStatusDisplay();
        }

        // Check for state changes
        if (PhotonNetwork.NetworkClientState != lastState)
        {
            lastState = PhotonNetwork.NetworkClientState;
            UpdateStatusDisplay();
        }
    }

    /// <summary>
    /// Updates the status text with current connection information
    /// </summary>
    private void UpdateStatusDisplay()
    {
        string statusMessage = GetStatusMessage();
        Color statusColor = GetStatusColor();

        // Update TextMeshPro if assigned
        if (statusText != null)
        {
            statusText.text = statusMessage;
            statusText.color = statusColor;
        }

        // Update Legacy Text if assigned
        if (legacyStatusText != null)
        {
            legacyStatusText.text = statusMessage;
            legacyStatusText.color = statusColor;
        }
    }

    /// <summary>
    /// Gets the appropriate status message based on connection state
    /// </summary>
    private string GetStatusMessage()
    {
        string message = "";

        switch (PhotonNetwork.NetworkClientState)
        {
            case ClientState.Joined:
            case ClientState.ConnectedToMasterServer:
            case ClientState.JoinedLobby:
                message = connectedMessage;
                
                // Add ping info
                if (showPing)
                {
                    message += $" | Ping: {PhotonNetwork.GetPing()}ms";
                }
                
                // Add player count
                if (showPlayerCount)
                {
                    message += $" | Players: {PhotonNetwork.CountOfPlayers}";
                }
                break;

            case ClientState.Disconnected:
                message = disconnectedMessage;
                break;

            case ClientState.ConnectingToMasterServer:
            case ClientState.Authenticating:
                message = connectingToMasterMessage;
                break;

            case ClientState.ConnectingToGameServer:
            case ClientState.Joining:
                message = connectingMessage;
                break;

            case ClientState.JoiningLobby:
                message = joiningLobbyMessage;
                break;

            case ClientState.Disconnecting:
                message = "Disconnecting...";
                break;

            case ClientState.ConnectingToNameServer:
                message = "Connecting to Name Server...";
                break;

            default:
                message = $"Status: {PhotonNetwork.NetworkClientState}";
                break;
        }

        return message;
    }

    /// <summary>
    /// Gets the appropriate color based on connection state
    /// </summary>
    private Color GetStatusColor()
    {
        switch (PhotonNetwork.NetworkClientState)
        {
            case ClientState.Joined:
            case ClientState.ConnectedToMasterServer:
            case ClientState.JoinedLobby:
                return connectedColor;

            case ClientState.Disconnected:
                return disconnectedColor;

            case ClientState.ConnectingToMasterServer:
            case ClientState.Authenticating:
            case ClientState.ConnectingToGameServer:
            case ClientState.Joining:
            case ClientState.JoiningLobby:
            case ClientState.ConnectingToNameServer:
            case ClientState.Disconnecting:
                return connectingColor;

            default:
                return errorColor;
        }
    }

    /// <summary>
    /// Attempts to connect to Photon network
    /// </summary>
    public void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("PhotonConnectionChecker: Attempting to connect to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("PhotonConnectionChecker: Already connected to Photon.");
        }
    }

    /// <summary>
    /// Disconnects from Photon network
    /// </summary>
    public void DisconnectFromPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("PhotonConnectionChecker: Disconnecting from Photon...");
            PhotonNetwork.Disconnect();
        }
    }

    /// <summary>
    /// Public method to get connection status as bool
    /// </summary>
    public bool IsConnected()
    {
        return PhotonNetwork.IsConnected;
    }

    /// <summary>
    /// Public method to get current connection state
    /// </summary>
    public ClientState GetConnectionState()
    {
        return PhotonNetwork.NetworkClientState;
    }

    #region Photon Callbacks

    public void OnConnected()
    {
        Debug.Log("PhotonConnectionChecker: Connected to Photon.");
        UpdateStatusDisplay();
    }

    public void OnConnectedToMaster()
    {
        Debug.Log("PhotonConnectionChecker: Connected to Master Server.");
        UpdateStatusDisplay();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"PhotonConnectionChecker: Disconnected from Photon. Reason: {cause}");
        UpdateStatusDisplay();
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        // Optional: Handle region list if needed
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        // Optional: Handle custom authentication
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.LogWarning($"PhotonConnectionChecker: Custom authentication failed: {debugMessage}");
        UpdateStatusDisplay();
    }

    #endregion

    private void OnDestroy()
    {
        // Unregister callbacks
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void OnApplicationQuit()
    {
        // Disconnect on quit
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }
}
