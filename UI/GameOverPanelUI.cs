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
    /// Resets MainBase scene state and reloads for a fresh attempt.
    /// </summary>
    public void OnRetry()
    {
        // Wave progress already saved on player death - no need to save again

        // Use SceneResetManager for proper state reset before retry
        if (SceneResetManager.Instance != null)
        {
            SceneResetManager.Instance.RetryWithReset();
        }
        else
        {
            // Fallback: Direct scene reload without reset
            Debug.LogWarning("[GameOverPanelUI] SceneResetManager not found! Using fallback scene reload.");
            Time.timeScale = 1f;
            SceneManager.LoadScene(chooseMenuScene);
        }
    }

    /// <summary>
    /// Called when the Quit button is clicked.
    /// Resets MainBase scene state and returns to main menu for a fresh start.
    /// </summary>
    public void OnQuit()
    {
        // Wave progress already saved on player death - no need to save again

        // Use SceneResetManager for complete state reset before returning to main menu
        if (SceneResetManager.Instance != null)
        {
            SceneResetManager.Instance.QuitToMainMenuWithReset();
        }
        else
        {
            // Fallback: Direct scene load without reset
            Debug.LogWarning("[GameOverPanelUI] SceneResetManager not found! Using fallback scene load.");
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuScene);
        }
    }
}
