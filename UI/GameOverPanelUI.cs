using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanelUI : MonoBehaviour
{
    [Header("Optional Sound")]
    public AudioSource deathSFX;

    [Header("Scene Settings")]
    [Tooltip("Scene to load when pressing Quit.")]
    public string mainMenuScene = "MainMenu"; // ← Set your actual main menu scene name here
    [Tooltip("Scene to load when pressing Retry.")]
    public string chooseMenuScene = "ChooseMenu"; // ← Scene for your character select menu

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

    // WILL SOLVE THIS SOON , MUCH BETTER WITH A GAME MANAGER HANDLING SCENES I THINK?? - LLOYD SAME SA PAUSEMENU UI NA PAG MU QUIT 

    //public void OnRetry()
    //{
    //    // Resume and go to character selection
    //    Time.timeScale = 1f;
    //    SceneManager.LoadScene(chooseMenuScene);
    //}

    //public void OnQuit()
    //{
    //    // Resume and go back to main menu
    //    Time.timeScale = 1f;
    //    SceneManager.LoadScene(mainMenuScene);
    //}
}
