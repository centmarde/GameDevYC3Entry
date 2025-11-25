using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// Main Menu Controller - Handles main menu UI interactions
/// 
/// QUICK SETUP DOCUMENTATION:
/// ==========================
/// 1. Create a Canvas in your MainMenu scene (GameObject > UI > Canvas)
/// 2. Add this script to the Canvas or create an empty GameObject and attach it
/// 3. Create UI elements:
///    - Image for background (UI > Image) - stretch to full screen
///    - Button for Start (UI > Button - TextMeshPro)
///    - Button for Leaderboards (UI > Button - TextMeshPro)
///    - Button for Quit (UI > Button - TextMeshPro)
/// 4. Assign references in the Inspector:
///    - Drag the Start button to the "Start Button" field
///    - Drag the Leaderboards button to the "Leaderboards Button" field
///    - Drag the Quit button to the "Quit Button" field
///    - (Optional) Drag background image to "Background Image" field
/// 5. Set your background image:
///    - Import your image into Unity (Assets/Graphics or Assets/UI folder)
///    - Set Texture Type to "Sprite (2D and UI)" in Inspector
///    - Drag the sprite to the Image component's "Source Image" field
/// 6. Add "MainBase" scene to Build Settings:
///    - File > Build Settings
///    - Add "MainBase" scene to "Scenes in Build" list
/// 7. Press Play to test!
/// 
/// STYLING TIPS:
/// - For background: Set Image component's Image Type to "Simple" and stretch anchors
/// - For buttons: Use TextMeshPro for better text quality
/// - Position buttons in center-bottom area of screen
/// - Add hover effects using Button's Transition settings
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Reference to the Start Game button")]
    [SerializeField] private Button startButton;
    
    [Tooltip("Reference to the Leaderboards button")]
    [SerializeField] private Button leaderboardsButton;
    
    [Tooltip("Reference to the Coop button")]
    [SerializeField] private Button coopButton;
    
    [Tooltip("Reference to the Quit Game button")]
    [SerializeField] private Button quitButton;
    
    [Tooltip("Optional: Reference to background image for runtime customization")]
    [SerializeField] private Image backgroundImage;

    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load when Start is clicked")]
    [SerializeField] private string gameSceneName = "Lobby";
    
    [Tooltip("Name of the leaderboards scene to load")]
    [SerializeField] private string leaderboardsSceneName = "LeaderBoards";
    
    [Tooltip("Name of the coop scene to load")]
    [SerializeField] private string coopSceneName = "Coop";

    [Header("Audio Settings")]
    [Tooltip("Delay in seconds before executing button action (allows audio to play)")]
    [SerializeField] private float buttonClickDelay = 0.5f;

    // Track if a button action is already in progress to prevent multiple clicks
    private bool isProcessingButtonClick = false;

    private void Start()
    {
        // Validate references
        if (startButton == null)
        {
            Debug.LogError("MainMenuController: Start Button is not assigned in the Inspector!");
        }
        else
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (leaderboardsButton == null)
        {
            Debug.LogError("MainMenuController: Leaderboards Button is not assigned in the Inspector!");
        }
        else
        {
            leaderboardsButton.onClick.AddListener(OnLeaderboardsButtonClicked);
        }
        
        if (coopButton == null)
        {
            Debug.LogError("MainMenuController: Coop Button is not assigned in the Inspector!");
        }
        else
        {
            coopButton.onClick.AddListener(OnCoopButtonClicked);
        }
        
        if (quitButton == null)
        {
            Debug.LogError("MainMenuController: Quit Button is not assigned in the Inspector!");
        }
        else
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }

        // Optional: Ensure cursor is visible and unlocked in menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Reconnect to Photon if needed (for GameManager)
        ReconnectToPhoton();
    }
    
    /// <summary>
    /// Reconnects to Photon when returning to main menu
    /// This ensures GameManager can properly initialize when entering game scenes
    /// </summary>
    private void ReconnectToPhoton()
    {
        // If Photon is disconnected or in a bad state, reconnect
        if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Disconnected ||
            PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.PeerCreated)
        {
            Debug.Log("[MainMenuController] Reconnecting to Photon for GameManager...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.IsConnected)
        {
            Debug.Log($"[MainMenuController] Already connected to Photon. State: {PhotonNetwork.NetworkClientState}");
            // If in lobby, leave it to free up resources
            if (PhotonNetwork.InLobby)
            {
                Debug.Log("[MainMenuController] Leaving lobby...");
                PhotonNetwork.LeaveLobby();
            }
        }
        else
        {
            Debug.Log($"[MainMenuController] Photon connection state: {PhotonNetwork.NetworkClientState}");
        }
    }

    /// <summary>
    /// Called when the Start button is clicked
    /// Loads the main game scene with loading screen
    /// </summary>
    private void OnStartButtonClicked()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(StartButtonClickCoroutine());
    }

    private IEnumerator StartButtonClickCoroutine()
    {
        isProcessingButtonClick = true;
        Debug.Log($"Start button clicked - waiting {buttonClickDelay}s for audio...");
        
        yield return new WaitForSeconds(buttonClickDelay);
        
        Debug.Log($"Loading scene: {gameSceneName}");
        
        // Use LoadingScreen if available, otherwise load normally
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.LoadScene(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
        
        isProcessingButtonClick = false;
    }

    /// <summary>
    /// Called when the Leaderboards button is clicked
    /// Loads the leaderboards scene
    /// </summary>
    private void OnLeaderboardsButtonClicked()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(LeaderboardsButtonClickCoroutine());
    }

    private IEnumerator LeaderboardsButtonClickCoroutine()
    {
        isProcessingButtonClick = true;
        Debug.Log($"Leaderboards button clicked - waiting {buttonClickDelay}s for audio...");
        
        yield return new WaitForSeconds(buttonClickDelay);
        
        Debug.Log($"Loading scene: {leaderboardsSceneName}");
        
        // Use LoadingScreen if available, otherwise load normally
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.LoadScene(leaderboardsSceneName);
        }
        else
        {
            SceneManager.LoadScene(leaderboardsSceneName);
        }
        
        isProcessingButtonClick = false;
    }

    /// <summary>
    /// Called when the Coop button is clicked
    /// Loads the coop scene
    /// </summary>
    private void OnCoopButtonClicked()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(CoopButtonClickCoroutine());
    }

    private IEnumerator CoopButtonClickCoroutine()
    {
        isProcessingButtonClick = true;
        Debug.Log($"Coop button clicked - waiting {buttonClickDelay}s for audio...");
        
        yield return new WaitForSeconds(buttonClickDelay);
        
        Debug.Log($"Loading scene: {coopSceneName}");
        
        // Use LoadingScreen if available, otherwise load normally
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.LoadScene(coopSceneName);
        }
        else
        {
            SceneManager.LoadScene(coopSceneName);
        }
        
        isProcessingButtonClick = false;
    }

    /// <summary>
    /// Called when the Quit button is clicked
    /// Exits the application
    /// </summary>
    private void OnQuitButtonClicked()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(QuitButtonClickCoroutine());
    }

    private IEnumerator QuitButtonClickCoroutine()
    {
        isProcessingButtonClick = true;
        Debug.Log($"Quit button clicked - waiting {buttonClickDelay}s for audio...");
        
        yield return new WaitForSeconds(buttonClickDelay);
        
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
        // If running in the Unity Editor, stop playing
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // If running as a build, quit the application
        Application.Quit();
        #endif
        
        isProcessingButtonClick = false;
    }

    /// <summary>
    /// Optional: Method to change background image at runtime
    /// </summary>
    /// <param name="newBackground">New sprite to use as background</param>
    public void SetBackgroundImage(Sprite newBackground)
    {
        if (backgroundImage != null && newBackground != null)
        {
            backgroundImage.sprite = newBackground;
        }
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
        
        if (leaderboardsButton != null)
        {
            leaderboardsButton.onClick.RemoveListener(OnLeaderboardsButtonClicked);
        }

        if (coopButton != null)
        {
            coopButton.onClick.RemoveListener(OnCoopButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }
    }
}
