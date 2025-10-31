using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverPanelUI : MonoBehaviour
{
    [Header("UI Buttons")]
    [Tooltip("Reference to the Retry button.")]
    public Button retryButton;
    [Tooltip("Reference to the Quit button.")]
    public Button quitButton;

    [Header("Optional Sound")]
    public AudioSource deathSFX;

    [Header("Scene Settings")]
    [Tooltip("Scene to load when pressing Quit.")]
    public string mainMenuScene = "MainMenu"; // ← Set your actual main menu scene name here
    [Tooltip("Scene to load when pressing Retry.")]
    public string chooseMenuScene = "ChooseMenu"; // ← Scene for your character select menu

    private void Start()
    {
        // Set up button listeners
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetry);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);
    }

    private void OnEnable()
    {
        // Pause the game and optionally play a death sound
        Time.timeScale = 0f;
        if (deathSFX != null)
            deathSFX.Play();
    }

    private void OnDisable()
    {
        // Resume normal timescale when panel closes
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        // Clean up button listeners to prevent memory leaks
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetry);
        
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuit);
    }

    /// <summary>
    /// Called when the Retry button is clicked.
    /// Saves wave progress to leaderboards, then resumes the game and loads the character selection scene.
    /// </summary>
    public void OnRetry()
    {
        // Save current wave progress to leaderboards before retrying
        if (PhotonGameManager.Instance != null)
        {
            PhotonGameManager.Instance.SaveCurrentWaveToLeaderboard();
            Debug.Log("[GameOverPanelUI] Wave progress saved to leaderboard on retry.");
        }
        else
        {
            Debug.LogWarning("[GameOverPanelUI] PhotonGameManager instance not found. Wave progress not saved.");
        }

        // Resume and go to character selection
        Time.timeScale = 1f;
        SceneManager.LoadScene(chooseMenuScene);
    }

    /// <summary>
    /// Called when the Quit button is clicked.
    /// Saves wave progress to leaderboards, then resumes the game and loads the main menu scene.
    /// </summary>
    public void OnQuit()
    {
        // Save current wave progress to leaderboards before quitting
        if (PhotonGameManager.Instance != null)
        {
            PhotonGameManager.Instance.SaveCurrentWaveToLeaderboard();
            Debug.Log("[GameOverPanelUI] Wave progress saved to leaderboard on quit.");
        }
        else
        {
            Debug.LogWarning("[GameOverPanelUI] PhotonGameManager instance not found. Wave progress not saved.");
        }

        // Resume and go back to main menu
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}
