using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    public GameObject pauseMenuPrefab;
    public GameObject pauseMenuInstance;

    [Header("UI Root")]
    public Canvas mainCanvas;

    [Header("Game Over UI")]
    public GameObject gameOverPanelPrefab;
    private GameObject gameOverPanelInstance;
    [SerializeField] private float fadeDuration = 0.75f;

    private Stack<GameObject> openUIs = new Stack<GameObject>();
    private PlayerInputSet input;
    private bool isGameOverShown = false;

    private void Awake()
    {
        Instance = this;
        input = new PlayerInputSet();
        input.Enable();
        input.Player.Cancel.performed += ctx => OnCancelPressed();
    }

    private void OnDestroy()
    {
        input.Player.Cancel.performed -= ctx => OnCancelPressed();
        input.Disable();
    }

    private void OnCancelPressed()
    {
        // Prevent pause when Game Over is active
        if (isGameOverShown)
            return;

        if (openUIs.Count > 0)
        {
            CloseTopUI();
        }
        else
        {
            OpenPauseMenu();
        }
    }

    private void OpenPauseMenu()
    {
        if (pauseMenuInstance == null)
        {
            if (pauseMenuPrefab != null)
            {
                pauseMenuInstance = Instantiate(pauseMenuPrefab);
            }
            else
            {
                Debug.LogWarning("No Pause Menu prefab assigned to UIManager.");
                return;
            }
        }

        pauseMenuInstance.SetActive(true);
    }

    // ============================================================
    // GAME OVER SYSTEM
    // ============================================================

    public void ShowGameOverPanel()
    {
        if (isGameOverShown) return;
        isGameOverShown = true;

        // Disable pause menu if open
        if (pauseMenuInstance && pauseMenuInstance.activeSelf)
            pauseMenuInstance.SetActive(false);

        // Create instance if needed
        if (gameOverPanelInstance == null)
        {
            if (gameOverPanelPrefab != null)
            {
                gameOverPanelInstance = Instantiate(gameOverPanelPrefab, mainCanvas.transform);
            }
            else
            {
                Debug.LogWarning("No Game Over panel prefab assigned to UIManager.");
                return;
            }
        }

        gameOverPanelInstance.SetActive(true);
        Time.timeScale = 0f;

        // Optional fade-in
        CanvasGroup cg = gameOverPanelInstance.GetComponent<CanvasGroup>();
        if (cg == null) cg = gameOverPanelInstance.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        StartCoroutine(FadeInGameOver(cg));
    }

    private IEnumerator FadeInGameOver(CanvasGroup cg)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    public void OnRetryPressed()
    {
        Time.timeScale = 1f;
        isGameOverShown = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenuPressed()
    {
        Time.timeScale = 1f;
        isGameOverShown = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }

    // ============================================================
    // STACK MANAGEMENT
    // ============================================================

    public void RegisterUI(GameObject ui)
    {
        if (!openUIs.Contains(ui))
            openUIs.Push(ui);
    }

    public void UnregisterUI(GameObject ui)
    {
        if (openUIs.Contains(ui))
        {
            var temp = new Stack<GameObject>();
            while (openUIs.Count > 0)
            {
                var top = openUIs.Pop();
                if (top != ui) temp.Push(top);
            }
            while (temp.Count > 0)
                openUIs.Push(temp.Pop());
        }
    }

    public void CloseTopUI()
    {
        if (openUIs.Count > 0)
        {
            GameObject topUI = openUIs.Pop();
            topUI.SetActive(false);
        }

        if (openUIs.Count == 0)
            Time.timeScale = 1f;
    }

    public bool AnyUIOpen() => openUIs.Count > 0;
}
