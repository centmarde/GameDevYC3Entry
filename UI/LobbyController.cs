using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Lobby Controller - Handles lobby UI interactions
/// 
/// QUICK SETUP DOCUMENTATION:
/// ==========================
/// 1. Create a Canvas in your Lobby scene (GameObject > UI > Canvas)
/// 2. Add this script to the Canvas or create an empty GameObject and attach it
/// 3. Create UI elements:
///    - InputField for player name (UI > Input Field - TextMeshPro)
///    - Button for Play Game (UI > Button - TextMeshPro)
///    - Button for Back (UI > Button - TextMeshPro)
/// 4. Assign references in the Inspector:
///    - Drag the InputField to the "Player Name Input" field
///    - Drag the Play Game button to the "Play Game Button" field
///    - Drag the Back button to the "Back Button" field
/// 5. Configure scene names:
///    - Set "Game Scene Name" to your main game scene (default: "MainBase")
///    - Set "Main Menu Scene Name" to your main menu scene (default: "MainMenu")
/// 6. Add scenes to Build Settings:
///    - File > Build Settings
///    - Add "MainBase" and "MainMenu" scenes to "Scenes in Build" list
/// 7. Press Play to test!
/// 
/// STYLING TIPS:
/// - Position input field at the top-center of the lobby
/// - Place Play Game button prominently in the center
/// - Place Back button in the bottom-left corner
/// - Use TextMeshPro for better text quality
/// - Add placeholder text to the input field (e.g., "Enter your name...")
/// </summary>
public class LobbyController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Reference to the player name input field")]
    [SerializeField] private TMP_InputField playerNameInput;
    
    [Tooltip("Reference to the Play Game button")]
    [SerializeField] private Button playGameButton;
    
    [Tooltip("Reference to the Back button")]
    [SerializeField] private Button backButton;

    [Header("Scene Settings")]
    [Tooltip("Name of the main game scene to load when Play Game is clicked")]
    [SerializeField] private string gameSceneName = "MainBase";
    
    [Tooltip("Name of the main menu scene to load when Back is clicked")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Settings")]
    [Tooltip("Minimum character length for player name")]
    [SerializeField] private int minNameLength = 3;
    
    [Tooltip("Maximum character length for player name")]
    [SerializeField] private int maxNameLength = 20;

    [Header("Audio Settings")]
    [Tooltip("Delay in seconds before executing button action (allows audio to play)")]
    [SerializeField] private float buttonClickDelay = 0.5f;

    [Header("Animation Settings")]
    [Tooltip("Duration of shake animation in seconds")]
    [SerializeField] private float shakeDuration = 0.5f;
    
    [Tooltip("Intensity of shake animation")]
    [SerializeField] private float shakeIntensity = 10f;
    
    [Tooltip("Distance the button moves away from cursor when disabled")]
    [SerializeField] private float evadeDistance = 50f;
    
    [Tooltip("Speed of button evasion animation")]
    [SerializeField] private float evadeSpeed = 300f;

    // Animation state tracking
    private bool isShaking = false;
    private Vector3 originalInputPosition;
    private Vector3 originalButtonPosition;
    private bool isEvading = false;
    private Coroutine evadeCoroutine;
    
    // Track if a button action is already in progress to prevent multiple clicks
    private bool isProcessingButtonClick = false;

    private void Start()
    {
        // Validate references
        if (playerNameInput == null)
        {
            Debug.LogError("LobbyController: Player Name Input is not assigned in the Inspector!");
        }
        else
        {
            // Set character limit
            playerNameInput.characterLimit = maxNameLength;
            
            // Add listener for input validation
            playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
        }

        if (playGameButton == null)
        {
            Debug.LogError("LobbyController: Play Game Button is not assigned in the Inspector!");
        }
        else
        {
            playGameButton.onClick.AddListener(OnPlayGameButtonClicked);
            
            // Initially disable if input is empty
            ValidatePlayButton();
        }

        if (backButton == null)
        {
            Debug.LogError("LobbyController: Back Button is not assigned in the Inspector!");
        }
        else
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        // Optional: Ensure cursor is visible and unlocked in lobby
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Store original positions for animations
        if (playerNameInput != null)
        {
            originalInputPosition = playerNameInput.transform.localPosition;
        }
        
        if (playGameButton != null)
        {
            originalButtonPosition = playGameButton.transform.localPosition;
        }
        
        // Load saved player name if available
        LoadPlayerName();
    }

    private void Update()
    {
        // Handle button evasion when disabled and cursor is near
        if (playGameButton != null && !playGameButton.interactable)
        {
            HandleButtonEvasion();
        }
        else if (playGameButton != null && playGameButton.interactable && !isEvading)
        {
            // Return button to original position when enabled
            ReturnButtonToOriginalPosition();
        }
    }

    /// <summary>
    /// Called when the player name input value changes
    /// Validates the input and updates the play button state
    /// </summary>
    /// <param name="value">Current input value</param>
    private void OnPlayerNameChanged(string value)
    {
        bool wasValid = playGameButton != null && playGameButton.interactable;
        ValidatePlayButton();
        bool isNowValid = playGameButton != null && playGameButton.interactable;
        
        // Trigger shake if validation failed and user tried to enter invalid input
        if (!isNowValid && value.Length > 0 && value.Length < minNameLength)
        {
            TriggerValidationShake();
        }
    }

    /// <summary>
    /// Validates if the play button should be enabled
    /// </summary>
    private void ValidatePlayButton()
    {
        if (playGameButton != null && playerNameInput != null)
        {
            string playerName = playerNameInput.text.Trim();
            bool isValid = playerName.Length >= minNameLength && playerName.Length <= maxNameLength;
            playGameButton.interactable = isValid;
            
            // If becoming invalid, start evasion behavior
            if (!isValid && !isEvading)
            {
                isEvading = true;
            }
            else if (isValid)
            {
                isEvading = false;
            }
        }
    }

    /// <summary>
    /// Triggers shake animation on input field and button for validation feedback
    /// </summary>
    private void TriggerValidationShake()
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeAnimation());
        }
    }

    /// <summary>
    /// Shake animation coroutine for input field and button
    /// </summary>
    private IEnumerator ShakeAnimation()
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float percentComplete = elapsed / shakeDuration;
            float damper = 1.0f - Mathf.Clamp01(percentComplete);

            // Shake input field
            if (playerNameInput != null)
            {
                float offsetX = Random.Range(-shakeIntensity, shakeIntensity) * damper;
                playerNameInput.transform.localPosition = originalInputPosition + new Vector3(offsetX, 0, 0);
            }

            // Shake button
            if (playGameButton != null)
            {
                float offsetX = Random.Range(-shakeIntensity, shakeIntensity) * damper;
                float offsetY = Random.Range(-shakeIntensity, shakeIntensity) * damper;
                playGameButton.transform.localPosition = originalButtonPosition + new Vector3(offsetX, offsetY, 0);
            }

            yield return null;
        }

        // Reset positions
        if (playerNameInput != null)
        {
            playerNameInput.transform.localPosition = originalInputPosition;
        }
        
        if (playGameButton != null && playGameButton.interactable)
        {
            playGameButton.transform.localPosition = originalButtonPosition;
        }

        isShaking = false;
    }

    /// <summary>
    /// Handles button evasion behavior when disabled and cursor approaches
    /// </summary>
    private void HandleButtonEvasion()
    {
        if (playGameButton == null) return;

        RectTransform buttonRect = playGameButton.GetComponent<RectTransform>();
        if (buttonRect == null) return;

        // Get mouse position in screen space (compatible with new Input System)
        Vector2 mousePosition = Vector2.zero;
        
        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            mousePosition = Mouse.current.position.ReadValue();
        }
        #else
        mousePosition = Input.mousePosition;
        #endif
        
        // Convert button position to screen space
        Vector3[] buttonCorners = new Vector3[4];
        buttonRect.GetWorldCorners(buttonCorners);
        Vector2 buttonCenter = Camera.main != null 
            ? (Vector2)Camera.main.WorldToScreenPoint(buttonRect.position)
            : (Vector2)buttonRect.position;

        // Calculate distance from mouse to button
        float distance = Vector2.Distance(mousePosition, buttonCenter);
        
        // If cursor is close enough, evade
        if (distance < evadeDistance * 2f)
        {
            // Calculate direction away from cursor
            Vector2 evadeDirection = (buttonCenter - mousePosition).normalized;
            
            // Calculate target position
            Vector3 targetPosition = originalButtonPosition + new Vector3(evadeDirection.x, evadeDirection.y, 0) * evadeDistance;
            
            // Smoothly move towards target position
            buttonRect.localPosition = Vector3.MoveTowards(
                buttonRect.localPosition,
                targetPosition,
                evadeSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// Returns button to its original position smoothly
    /// </summary>
    private void ReturnButtonToOriginalPosition()
    {
        if (playGameButton == null) return;

        RectTransform buttonRect = playGameButton.GetComponent<RectTransform>();
        if (buttonRect == null) return;

        // Smoothly return to original position
        if (Vector3.Distance(buttonRect.localPosition, originalButtonPosition) > 0.1f)
        {
            buttonRect.localPosition = Vector3.MoveTowards(
                buttonRect.localPosition,
                originalButtonPosition,
                evadeSpeed * Time.deltaTime
            );
        }
        else
        {
            buttonRect.localPosition = originalButtonPosition;
        }
    }

    /// <summary>
    /// Called when the Play Game button is clicked
    /// Saves the player name and loads the main game scene
    /// </summary>
    private void OnPlayGameButtonClicked()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(PlayGameButtonClickCoroutine());
    }
    
    private IEnumerator PlayGameButtonClickCoroutine()
    {
        isProcessingButtonClick = true;
        
        if (playerNameInput == null)
        {
            Debug.LogError("Cannot start game: Player Name Input is null!");
            isProcessingButtonClick = false;
            yield break;
        }

        string playerName = playerNameInput.text.Trim();
        
        // Validate player name
        if (playerName.Length < minNameLength)
        {
            Debug.LogWarning($"Player name must be at least {minNameLength} characters long!");
            TriggerValidationShake();
            isProcessingButtonClick = false;
            yield break;
        }
        
        if (playerName.Length > maxNameLength)
        {
            Debug.LogWarning($"Player name must be at most {maxNameLength} characters long!");
            TriggerValidationShake();
            isProcessingButtonClick = false;
            yield break;
        }

        Debug.Log($"Play Game button clicked - waiting {buttonClickDelay}s for audio...");
        
        yield return new WaitForSeconds(buttonClickDelay);
        
        // Save player name to PlayerPrefs
        SavePlayerName(playerName);
        
        // Save player name to PhotonGameManager
        if (PhotonGameManager.Instance != null)
        {
            PhotonGameManager.Instance.SetPlayerName(playerName);
        }
        
        Debug.Log($"Starting game with player name: {playerName}");
        Debug.Log($"Loading scene: {gameSceneName}");
        
        // For MainBase scene, use SplashLoading screen due to heavy content
        if (gameSceneName == "MainBase")
        {
            Debug.Log($"Loading MainBase with SplashLoading screen...");
            yield return StartCoroutine(LoadSceneWithSplashScreen(gameSceneName));
        }
        // Use LoadingScreen if available for other scenes
        else if (LoadingScreen.Instance != null)
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
    /// Called when the Back button is clicked
    /// Returns to the main menu
    /// </summary>
    private void OnBackButtonClicked()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(BackButtonClickCoroutine());
    }
    
    private IEnumerator BackButtonClickCoroutine()
    {
        isProcessingButtonClick = true;
        Debug.Log($"Back button clicked - waiting {buttonClickDelay}s for audio...");
        
        yield return new WaitForSeconds(buttonClickDelay);
        
        Debug.Log($"Returning to main menu: {mainMenuSceneName}");
        
        // Use LoadingScreen if available, otherwise load normally
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

    /// <summary>
    /// Saves the player name to PlayerPrefs
    /// </summary>
    /// <param name="playerName">Name to save</param>
    private void SavePlayerName(string playerName)
    {
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
        Debug.Log($"Player name saved: {playerName}");
    }

    /// <summary>
    /// Loads the saved player name from PlayerPrefs
    /// </summary>
    private void LoadPlayerName()
    {
        if (playerNameInput != null && PlayerPrefs.HasKey("PlayerName"))
        {
            string savedName = PlayerPrefs.GetString("PlayerName");
            playerNameInput.text = savedName;
            Debug.Log($"Loaded saved player name: {savedName}");
        }
    }

    /// <summary>
    /// Public method to get the current player name
    /// </summary>
    /// <returns>The trimmed player name from the input field</returns>
    public string GetPlayerName()
    {
        return playerNameInput != null ? playerNameInput.text.Trim() : string.Empty;
    }

    /// <summary>
    /// Public method to set the player name programmatically
    /// </summary>
    /// <param name="name">Name to set</param>
    public void SetPlayerName(string name)
    {
        if (playerNameInput != null)
        {
            playerNameInput.text = name;
            ValidatePlayButton();
        }
    }

    /// <summary>
    /// Public method to manually trigger validation shake (can be called from external scripts)
    /// </summary>
    public void TriggerShake()
    {
        TriggerValidationShake();
    }
    
    /// <summary>
    /// Loads a scene with SplashLoading screen overlay for heavy scenes like MainBase
    /// </summary>
    private IEnumerator LoadSceneWithSplashScreen(string sceneName)
    {
        Debug.Log($"[LobbyController] Loading SplashLoading screen...");
        
        // First, load the splash loading scene additively
        AsyncOperation loadSplash = SceneManager.LoadSceneAsync("SplashLoading", LoadSceneMode.Additive);
        
        if (loadSplash != null)
        {
            // Wait for splash screen to load
            while (!loadSplash.isDone)
            {
                yield return null;
            }
            
            Debug.Log($"[LobbyController] SplashLoading screen loaded, now loading {sceneName}...");
        }
        
        // Small delay to ensure splash screen is visible
        yield return new WaitForSeconds(0.2f);
        
        // Now load target scene
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(sceneName);
        
        if (loadScene != null)
        {
            loadScene.allowSceneActivation = false;
            
            // Show loading progress
            while (loadScene.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(loadScene.progress / 0.9f);
                Debug.Log($"[LobbyController] Loading {sceneName}: {progress * 100:F1}%");
                yield return null;
            }
            
            // Minimum loading time to show splash screen (optional)
            yield return new WaitForSeconds(0.5f);
            
            // Activate the scene
            Debug.Log($"[LobbyController] {sceneName} loaded, activating scene...");
            loadScene.allowSceneActivation = true;
            
            // Wait for scene to fully activate
            while (!loadScene.isDone)
            {
                yield return null;
            }
            
            Debug.Log($"[LobbyController] {sceneName} scene fully loaded!");
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (playerNameInput != null)
        {
            playerNameInput.onValueChanged.RemoveListener(OnPlayerNameChanged);
        }

        if (playGameButton != null)
        {
            playGameButton.onClick.RemoveListener(OnPlayGameButtonClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
        }

        // Stop any running coroutines
        StopAllCoroutines();
    }
}
