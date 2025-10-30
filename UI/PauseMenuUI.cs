using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject aboutPanel;
    public GameObject monsterDatabasePanel; // 👈 add this line

    private void OnEnable()
    {
        Time.timeScale = 0f; // pause game when active
        if (aboutPanel) aboutPanel.SetActive(false);
        if (monsterDatabasePanel) monsterDatabasePanel.SetActive(false);
        if (mainPanel) mainPanel.SetActive(true);

    }

    private void OnDisable()
    {
        Time.timeScale = 1f; // resume game when closed
    }

    public void OnContinueButton()
    {
        gameObject.SetActive(false); // closes this UI
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

    // 👇 new section for Monster Database navigation
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

    public void OnQuitButton()
    {
        Time.timeScale = 1f;

        // --- Reset the PlayerSpawnManager session flag ---
        var spawner = FindObjectOfType<PlayerSpawnManager>();
        if (spawner != null)
        {
            spawner.DespawnPlayer();
            typeof(PlayerSpawnManager)
                .GetField("playerSpawnedThisSession", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.SetValue(null, false);
        }

        SceneManager.LoadScene("MainMenu");
    }
}
