using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Loading Screen - Displays a loading screen during scene transitions
/// 
/// QUICK SETUP:
/// 1. Create UI elements in your scene:
///    - Canvas > Panel (full screen, dark background)
///    - Add TextMeshPro for "Loading..." text
///    - Add Slider for progress bar (optional)
///    - Add Image for loading icon/spinner (optional)
/// 2. Attach this script to the Panel
/// 3. Assign references in Inspector
/// 4. Call LoadingScreen.LoadScene("SceneName") instead of SceneManager.LoadScene()
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image loadingIcon;

    [Header("Loading Settings")]
    [SerializeField] private float minimumLoadTime = 1f;
    [SerializeField] private float iconRotationSpeed = 200f;
    [SerializeField] private bool showProgressBar = true;
    [SerializeField] private bool showLoadingIcon = true;

    [Header("Loading Messages")]
    [SerializeField] private string[] loadingMessages = new string[]
    {
        "Loading...",
        "Preparing Adventure...",
        "Loading Assets...",
        "Almost There..."
    };

    private bool isLoading = false;

    private void Awake()
    {
        // Singleton pattern - only one loading screen exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Hide loading screen at start
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    private void Update()
    {
        // Rotate loading icon if active
        if (isLoading && loadingIcon != null && showLoadingIcon)
        {
            loadingIcon.transform.Rotate(0f, 0f, -iconRotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Load a scene with loading screen
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public static void LoadScene(string sceneName)
    {
        if (Instance != null)
        {
            Instance.StartCoroutine(Instance.LoadSceneAsync(sceneName));
        }
        else
        {
            Debug.LogWarning("LoadingScreen instance not found! Loading scene normally.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Asynchronously loads the scene with progress updates
    /// </summary>
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;

        // Show loading panel
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Initialize progress bar
        if (progressBar != null && showProgressBar)
        {
            progressBar.value = 0f;
            progressBar.gameObject.SetActive(true);
        }

        // Initialize loading icon
        if (loadingIcon != null && showLoadingIcon)
        {
            loadingIcon.gameObject.SetActive(true);
        }

        // Set initial loading message
        UpdateLoadingText(0);

        // Wait a frame to ensure UI is visible
        yield return null;

        // Start loading the scene
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // Don't activate immediately

        float elapsedTime = 0f;
        float progress = 0f;

        // Update progress while loading
        while (!asyncLoad.isDone)
        {
            elapsedTime += Time.deltaTime;

            // AsyncOperation progress goes from 0 to 0.9
            // We'll map it to 0 to 1 for display
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Update progress bar
            if (progressBar != null && showProgressBar)
            {
                progressBar.value = progress;
            }

            // Update loading message based on progress
            UpdateLoadingText(progress);

            // When loading is almost complete (0.9 means ready)
            if (asyncLoad.progress >= 0.9f)
            {
                // Ensure minimum load time for smooth experience
                if (elapsedTime >= minimumLoadTime)
                {
                    // Fill progress bar to 100%
                    if (progressBar != null && showProgressBar)
                    {
                        progressBar.value = 1f;
                    }

                    // Final loading message
                    if (loadingText != null)
                    {
                        loadingText.text = "Ready!";
                    }

                    yield return new WaitForSeconds(0.3f); // Brief pause
                    asyncLoad.allowSceneActivation = true; // Activate the scene
                }
            }

            yield return null;
        }

        // Hide loading screen
        isLoading = false;
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    /// <summary>
    /// Updates the loading text based on progress
    /// </summary>
    private void UpdateLoadingText(float progress)
    {
        if (loadingText == null) return;

        // Select message based on progress
        int messageIndex = Mathf.FloorToInt(progress * (loadingMessages.Length - 1));
        messageIndex = Mathf.Clamp(messageIndex, 0, loadingMessages.Length - 1);

        loadingText.text = loadingMessages[messageIndex];

        // Optionally add percentage
        if (showProgressBar)
        {
            loadingText.text += $"\n{Mathf.FloorToInt(progress * 100)}%";
        }
    }

    /// <summary>
    /// Show loading screen manually
    /// </summary>
    public void Show()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
    }

    /// <summary>
    /// Hide loading screen manually
    /// </summary>
    public void Hide()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
}
