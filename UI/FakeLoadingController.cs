using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FakeLoadingController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image splashImage;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Loading Settings")]
    [Tooltip("How long the fake loading should take in seconds")]
    [SerializeField] private float fakeLoadingDuration = 5f;

    [Tooltip("Scene to load after fake loading completes")]
    [SerializeField] private string nextSceneName = "IntroScene"; // your slideshow scene

    private void Start()
    {
        // Check if this is a retry/reset scenario and skip fake loading
        if (ShouldSkipFakeLoading())
        {
            Debug.Log("[FakeLoadingController] Skipping fake loading - direct scene load detected");
            SceneManager.LoadScene(nextSceneName);
            return;
        }
        
        StartCoroutine(FakeLoadingRoutine());
    }
    
    private bool ShouldSkipFakeLoading()
    {
        // Skip fake loading if we detect this is a retry/reset
        // This can be detected by checking for specific player prefs or static flags
        return PlayerPrefs.GetInt("SkipFakeLoading", 0) == 1;
    }

    private IEnumerator FakeLoadingRoutine()
    {
        float elapsed = 0f;

        while (elapsed < fakeLoadingDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fakeLoadingDuration);

            // Update UI
            if (loadingBar != null)
                loadingBar.value = progress;

            if (loadingText != null)
                loadingText.text = $"Game Loading... {Mathf.RoundToInt(progress * 100)}%";

            yield return null;
        }

        // Optional: short delay before switching
        yield return new WaitForSeconds(0.5f);

        // Load your intro scene
        SceneManager.LoadScene(nextSceneName);
    }
}
