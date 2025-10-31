using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Wave Leaderboard UI - Displays player wave progress from Photon
/// Shows all players in the lobby and their current/highest waves
/// </summary>
public class WaveLeaderboardUI : MonoBehaviour
{
    [Header("Navigation")]
    [Tooltip("Back button to return to previous scene")]
    [SerializeField] private Button backButton;
    
    [Tooltip("Scene to load when back button is clicked (leave empty to use previous scene)")]
    [SerializeField] private string backSceneName = "MainMenu";
    
    [Tooltip("Sort order for the back button canvas (higher = front)")]
    [SerializeField] private int backButtonSortOrder = 100;
    
    [Header("UI References")]
    [Tooltip("Parent container for leaderboard entries")]
    [SerializeField] private Transform leaderboardContainer;
    
    [Tooltip("Prefab for a single leaderboard entry")]
    [SerializeField] private GameObject leaderboardEntryPrefab;
    
    [Tooltip("Text to show when no players are online")]
    [SerializeField] private TextMeshProUGUI noPlayersText;
    
    [Tooltip("Title text (optional)")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Audio Settings")]
    [Tooltip("Delay in seconds before executing button action (allows audio to play)")]
    [SerializeField] private float buttonClickDelay = 0.5f;

    [Header("Update Settings")]
    [Tooltip("How often to refresh the leaderboard (seconds)")]
    [SerializeField] private float updateInterval = 2f;
    
    [Tooltip("Automatically refresh on enable")]
    [SerializeField] private bool autoRefreshOnEnable = true;

    [Header("Styling")]
    [Tooltip("Highlight color for local player")]
    [SerializeField] private Color localPlayerColor = new Color(1f, 0.9f, 0.5f); // Gold
    
    [Tooltip("Normal color for other players")]
    [SerializeField] private Color normalColor = Color.white;

    [Header("Display Options")]
    [Tooltip("Maximum number of entries to show (0 = show all)")]
    [SerializeField] private int maxEntriesToShow = 10;
    
    [Tooltip("Show player rank numbers")]
    [SerializeField] private bool showRankNumbers = true;
    
    [Tooltip("Show current wave")]
    [SerializeField] private bool showCurrentWave = true;
    
    [Tooltip("Show highest wave reached")]
    [SerializeField] private bool showHighestWave = true;

    private float updateTimer = 0f;
    private List<GameObject> activeEntries = new List<GameObject>();
    private bool hasValidConfiguration = false;
    
    // Track if a button action is already in progress to prevent multiple clicks
    private bool isProcessingButtonClick = false;

    private void Awake()
    {
        // Validate configuration on startup
        ValidateConfiguration();
    }

    private void Start()
    {
        // Setup back button
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            EnsureButtonIsClickable();
        }
        else
        {
            Debug.LogWarning("WaveLeaderboardUI: Back button not assigned in inspector!");
        }
    }

    /// <summary>
    /// Validate that all required references are assigned
    /// </summary>
    private void ValidateConfiguration()
    {
        bool isValid = true;
        System.Text.StringBuilder errors = new System.Text.StringBuilder();
        errors.AppendLine("WaveLeaderboardUI Configuration Errors:");

        if (leaderboardContainer == null)
        {
            errors.AppendLine("❌ Leaderboard Container is not assigned!");
            errors.AppendLine("   → Assign the 'Content' GameObject from your ScrollView");
            isValid = false;
        }

        if (leaderboardEntryPrefab == null)
        {
            errors.AppendLine("❌ Leaderboard Entry Prefab is not assigned!");
            errors.AppendLine("   → Assign the prefab at: Assets/Prefabs/UI/LeaderboardEntryPrefab.prefab");
            errors.AppendLine("   → Or run: Tools → Photon Game Manager → Setup Wizard");
            isValid = false;
        }

        if (noPlayersText == null)
        {
            errors.AppendLine("⚠️ No Players Text is not assigned (optional but recommended)");
        }

        if (backButton == null)
        {
            errors.AppendLine("⚠️ Back Button is not assigned (optional)");
        }

        hasValidConfiguration = isValid;

        if (!isValid)
        {
            Debug.LogError(errors.ToString(), this);
            Debug.LogError("WaveLeaderboardUI: Please assign missing references in the Inspector!", this);
        }
        else
        {
            Debug.Log("WaveLeaderboardUI: Configuration validated successfully! ✓");
        }
    }

    /// <summary>
    /// Ensure the back button is rendered on top and clickable
    /// </summary>
    private void EnsureButtonIsClickable()
    {
        if (backButton == null) return;

        // Make sure the button itself is interactable
        backButton.interactable = true;

        // Ensure the button has a CanvasGroup configured properly
        CanvasGroup buttonCanvasGroup = backButton.GetComponent<CanvasGroup>();
        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.interactable = true;
            buttonCanvasGroup.blocksRaycasts = true;
            buttonCanvasGroup.ignoreParentGroups = true; // Ignore parent blocking
        }

        // Get or add Canvas component for override sorting
        Canvas buttonCanvas = backButton.GetComponent<Canvas>();
        if (buttonCanvas == null)
        {
            buttonCanvas = backButton.gameObject.AddComponent<Canvas>();
        }
        
        // Enable override sorting and set very high sort order
        buttonCanvas.overrideSorting = true;
        buttonCanvas.sortingOrder = backButtonSortOrder;
        
        // Ensure GraphicRaycaster exists for click detection
        GraphicRaycaster raycaster = backButton.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = backButton.gameObject.AddComponent<GraphicRaycaster>();
        }
        raycaster.ignoreReversedGraphics = true;
        raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

        // Disable raycast blocking on parent panels/images
        DisableParentRaycastBlocking(backButton.transform);

        // Move button to front in hierarchy
        backButton.transform.SetAsLastSibling();

        Debug.Log($"WaveLeaderboardUI: Back button configured - Sort Order: {backButtonSortOrder}, Interactable: {backButton.interactable}");
    }

    /// <summary>
    /// Disable raycast blocking on parent Image components
    /// </summary>
    private void DisableParentRaycastBlocking(Transform child)
    {
        Transform parent = child.parent;
        while (parent != null)
        {
            Image parentImage = parent.GetComponent<Image>();
            if (parentImage != null)
            {
                // Store original raycast target state
                bool wasTarget = parentImage.raycastTarget;
                parentImage.raycastTarget = false;
                
                if (wasTarget)
                {
                    Debug.Log($"WaveLeaderboardUI: Disabled raycast blocking on parent: {parent.name}");
                }
            }
            
            parent = parent.parent;
        }
    }

    private void OnEnable()
    {
        if (autoRefreshOnEnable)
        {
            RefreshLeaderboard();
        }
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;
        
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            RefreshLeaderboard();
        }
    }

    /// <summary>
    /// Refresh the leaderboard display
    /// </summary>
    public void RefreshLeaderboard()
    {
        // Check if configuration is valid before attempting to refresh
        if (!hasValidConfiguration)
        {
            return; // Silently skip if configuration is invalid (error already logged in Awake)
        }

        if (PhotonGameManager.Instance == null)
        {
            ShowNoPlayers("Game Manager not found");
            return;
        }

        if (!PhotonGameManager.Instance.IsConnectedToPhoton())
        {
            ShowNoPlayers("Not connected to server");
            return;
        }

        if (!PhotonGameManager.Instance.IsInLobby())
        {
            ShowNoPlayers("Not in lobby");
            return;
        }

        // Get leaderboard data
        List<LeaderboardEntry> leaderboard = PhotonGameManager.Instance.GetLeaderboardData();

        if (leaderboard == null || leaderboard.Count == 0)
        {
            ShowNoPlayers("No players online");
            return;
        }

        // Hide no players message
        if (noPlayersText != null)
        {
            noPlayersText.gameObject.SetActive(false);
        }

        // Clear existing entries
        ClearEntries();

        // Determine how many entries to show
        int entriesToShow = maxEntriesToShow > 0 ? Mathf.Min(maxEntriesToShow, leaderboard.Count) : leaderboard.Count;

        // Create new entries
        for (int i = 0; i < entriesToShow; i++)
        {
            CreateLeaderboardEntry(leaderboard[i], i + 1);
        }
    }

    /// <summary>
    /// Create a single leaderboard entry
    /// </summary>
    private void CreateLeaderboardEntry(LeaderboardEntry data, int rank)
    {
        if (leaderboardEntryPrefab == null || leaderboardContainer == null)
        {
            Debug.LogError("WaveLeaderboardUI: Cannot create entry - missing references! Check Awake() errors.", this);
            return;
        }

        GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
        activeEntries.Add(entry);

        // Find text components (assumes prefab has these children)
        TextMeshProUGUI rankText = entry.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI currentWaveText = entry.transform.Find("CurrentWaveText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI highestWaveText = entry.transform.Find("HighestWaveText")?.GetComponent<TextMeshProUGUI>();
        Image background = entry.GetComponent<Image>();

        // Set rank
        if (rankText != null && showRankNumbers)
        {
            rankText.text = $"#{rank}";
        }

        // Set name
        if (nameText != null)
        {
            nameText.text = data.playerName;
            
            // Add indicator for local player
            if (data.isLocalPlayer)
            {
                nameText.text += " (You)";
            }
        }

        // Set current wave
        if (currentWaveText != null && showCurrentWave)
        {
            currentWaveText.text = $"Current: Wave {data.currentWave}";
        }
        else if (currentWaveText != null)
        {
            currentWaveText.gameObject.SetActive(false);
        }

        // Set highest wave
        if (highestWaveText != null && showHighestWave)
        {
            highestWaveText.text = $"Best: Wave {data.highestWave}";
        }
        else if (highestWaveText != null)
        {
            highestWaveText.gameObject.SetActive(false);
        }

        // Highlight local player
        if (data.isLocalPlayer && background != null)
        {
            background.color = localPlayerColor;
        }
        else if (background != null)
        {
            background.color = normalColor;
        }

        // Apply color to all text
        Color textColor = data.isLocalPlayer ? Color.black : Color.white;
        if (rankText != null) rankText.color = textColor;
        if (nameText != null) nameText.color = textColor;
        if (currentWaveText != null) currentWaveText.color = textColor;
        if (highestWaveText != null) highestWaveText.color = textColor;
    }

    /// <summary>
    /// Clear all leaderboard entries
    /// </summary>
    private void ClearEntries()
    {
        foreach (GameObject entry in activeEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        activeEntries.Clear();
    }

    /// <summary>
    /// Show "no players" message
    /// </summary>
    private void ShowNoPlayers(string message)
    {
        ClearEntries();
        
        if (noPlayersText != null)
        {
            noPlayersText.gameObject.SetActive(true);
            noPlayersText.text = message;
        }
    }

    /// <summary>
    /// Force an immediate refresh
    /// </summary>
    public void ForceRefresh()
    {
        updateTimer = 0f;
        RefreshLeaderboard();
    }

    /// <summary>
    /// Handle back button click
    /// </summary>
    private void OnBackButtonClicked()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(BackButtonClickCoroutine());
    }
    
    private System.Collections.IEnumerator BackButtonClickCoroutine()
    {
        isProcessingButtonClick = true;
        
        Debug.Log("=== BACK BUTTON CLICKED ===", this);
        Debug.Log($"WaveLeaderboardUI: Back button clicked at {System.DateTime.Now:HH:mm:ss}");
        Debug.Log($"WaveLeaderboardUI: Waiting {buttonClickDelay}s for audio...");
        Debug.Log($"WaveLeaderboardUI: Current scene = '{SceneManager.GetActiveScene().name}'");
        Debug.Log($"WaveLeaderboardUI: Target scene = '{backSceneName}'");
        
        yield return new WaitForSeconds(buttonClickDelay);
        
        if (!string.IsNullOrEmpty(backSceneName))
        {
            // Load specified scene
            Debug.Log($"WaveLeaderboardUI: ✓ Loading scene '{backSceneName}'...");
            
            try
            {
                SceneManager.LoadScene(backSceneName);
                Debug.Log($"WaveLeaderboardUI: Scene load initiated successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"WaveLeaderboardUI: Failed to load scene '{backSceneName}': {e.Message}");
                Debug.LogError($"Make sure '{backSceneName}' is added to Build Settings!", this);
            }
        }
        else
        {
            // Try to go back to previous scene (if build index > 0)
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            Debug.Log($"WaveLeaderboardUI: No target scene specified, using build index navigation");
            Debug.Log($"WaveLeaderboardUI: Current build index = {currentSceneIndex}");
            
            if (currentSceneIndex > 0)
            {
                Debug.Log($"WaveLeaderboardUI: ✓ Loading previous scene at build index {currentSceneIndex - 1}");
                SceneManager.LoadScene(currentSceneIndex - 1);
            }
            else
            {
                Debug.LogWarning("WaveLeaderboardUI: ⚠️ No back scene specified and already at first scene!");
                Debug.LogWarning("Assign a 'Back Scene Name' in the inspector or add scenes to Build Settings!");
            }
        }
        
        isProcessingButtonClick = false;
    }

    /// <summary>
    /// Public method to navigate back (can be called from UI button OnClick() event)
    /// Use this in the Inspector: Button → OnClick() → Add WaveLeaderboardUI.NavigateBack
    /// </summary>
    public void NavigateBack()
    {
        Debug.Log("WaveLeaderboardUI: NavigateBack() called via OnClick() event", this);
        OnBackButtonClicked();
    }

    /// <summary>
    /// Navigate to a specific scene by name (can be called from UI button OnClick() event)
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void NavigateToScene(string sceneName)
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(NavigateToSceneCoroutine(sceneName));
    }
    
    private System.Collections.IEnumerator NavigateToSceneCoroutine(string sceneName)
    {
        isProcessingButtonClick = true;
        
        Debug.Log($"WaveLeaderboardUI: NavigateToScene('{sceneName}') called via OnClick() event", this);
        
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("WaveLeaderboardUI: Scene name is empty!");
            isProcessingButtonClick = false;
            yield break;
        }

        Debug.Log($"WaveLeaderboardUI: Waiting {buttonClickDelay}s for audio...");
        yield return new WaitForSeconds(buttonClickDelay);

        try
        {
            Debug.Log($"WaveLeaderboardUI: Loading scene '{sceneName}'...");
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"WaveLeaderboardUI: Failed to load scene '{sceneName}': {e.Message}");
            Debug.LogError($"Make sure '{sceneName}' is added to Build Settings!", this);
        }
        
        isProcessingButtonClick = false;
    }

    /// <summary>
    /// Navigate to MainMenu scene (convenience method for OnClick() events)
    /// </summary>
    public void NavigateToMainMenu()
    {
        if (isProcessingButtonClick) return;
        Debug.Log("WaveLeaderboardUI: NavigateToMainMenu() called via OnClick() event", this);
        NavigateToScene("MainMenu");
    }

    /// <summary>
    /// Navigate to previous scene by build index (can be called from UI button OnClick() event)
    /// </summary>
    public void NavigateToPreviousScene()
    {
        if (isProcessingButtonClick) return;
        StartCoroutine(NavigateToPreviousSceneCoroutine());
    }
    
    private System.Collections.IEnumerator NavigateToPreviousSceneCoroutine()
    {
        isProcessingButtonClick = true;
        
        Debug.Log("WaveLeaderboardUI: NavigateToPreviousScene() called via OnClick() event", this);
        Debug.Log($"WaveLeaderboardUI: Waiting {buttonClickDelay}s for audio...");
        
        yield return new WaitForSeconds(buttonClickDelay);
        
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        if (currentSceneIndex > 0)
        {
            Debug.Log($"WaveLeaderboardUI: Loading previous scene at build index {currentSceneIndex - 1}");
            SceneManager.LoadScene(currentSceneIndex - 1);
        }
        else
        {
            Debug.LogWarning("WaveLeaderboardUI: Already at first scene (index 0)!");
        }
        
        isProcessingButtonClick = false;
    }

    private void OnDisable()
    {
        ClearEntries();
        
        // Cleanup button listener
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
        }
    }
}
