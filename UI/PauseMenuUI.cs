using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject aboutPanel;
    public GameObject monsterDatabasePanel;
    public GameObject tutorialPanel; // 👈 NEW

    private void OnEnable()
    {
        Time.timeScale = 0f; // pause game
        if (aboutPanel) aboutPanel.SetActive(false);
        if (monsterDatabasePanel) monsterDatabasePanel.SetActive(false);
        if (tutorialPanel) tutorialPanel.SetActive(false);
        if (mainPanel) mainPanel.SetActive(true);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f; // resume game
    }

    // === Main menu navigation ===
    public void OnContinueButton()
    {
        gameObject.SetActive(false);
    }

    public void OnAboutButton()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (aboutPanel) aboutPanel.SetActive(true);
    }

    public void OnCloseAbout()
    {
        if (aboutPanel) aboutPanel.SetActive(false);
        if (mainPanel) mainPanel.SetActive(true);
    }

    // === Monster Database ===
    public void OnOpenMonsterDatabase()
    {
        if (aboutPanel) aboutPanel.SetActive(false);
        if (monsterDatabasePanel) monsterDatabasePanel.SetActive(true);
    }

    public void OnCloseMonsterDatabase()
    {
        if (monsterDatabasePanel) monsterDatabasePanel.SetActive(false);
        if (aboutPanel) aboutPanel.SetActive(true);
    }

    // === Tutorial navigation ===
    public void OnOpenTutorial()
    {
        if (aboutPanel) aboutPanel.SetActive(false);
        if (tutorialPanel) tutorialPanel.SetActive(true);
    }

    public void OnCloseTutorial()
    {
        if (tutorialPanel) tutorialPanel.SetActive(false);
        if (aboutPanel) aboutPanel.SetActive(true);
    }

    // === Quit to main menu ===
    public void OnQuitButton()
    {
        Time.timeScale = 1f;

        var spawner = FindObjectOfType<PlayerSpawnManager>();
        if (spawner != null)
        {
            spawner.DespawnPlayer();
            typeof(PlayerSpawnManager)
                .GetField("playerSpawnedThisSession",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.SetValue(null, false);
        }

        SceneManager.LoadScene("MainMenu");
    }
}
