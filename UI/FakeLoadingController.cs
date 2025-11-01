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

    [Header("Navigation Settings")]
    [Tooltip("Default scene to load if no specific target is set")]
    [SerializeField] private string defaultNextScene = "IntroScene";

    private void Start()
    {
        // Check if this is a retry/reset scenario and skip fake loading
        if (ShouldSkipFakeLoading())
        {
            Debug.Log("[FakeLoadingController] Skipping fake loading - loading target scene directly");
            string targetScene = GetTargetScene();
            SceneManager.LoadScene(targetScene);
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

    private string GetTargetScene()
    {
        // Check if a specific target scene was set via PlayerPrefs (dynamic navigation)
        string targetScene = PlayerPrefs.GetString("TargetSceneAfterLoading", "");
        
        if (!string.IsNullOrEmpty(targetScene))
        {
            Debug.Log($"[FakeLoadingController] Target scene from prefs: {targetScene}");
            // Clear the pref after reading it
            PlayerPrefs.DeleteKey("TargetSceneAfterLoading");
            PlayerPrefs.Save();
            return targetScene;
        }
        
        // Return empty string to signal "just unload, don't navigate"
        return "";
    }
    
    private bool IsUsedAsLoadingCover()
    {
        // Check if this splash screen was loaded additively (as a loading cover)
        // In that case, the active scene is the one we're covering (like MainMenu)
        Scene activeScene = SceneManager.GetActiveScene();
        Scene thisScene = gameObject.scene;
        
        // If we're not the active scene and loaded additively, we're just a cover
        return thisScene.name == "SplashLoading" && activeScene.name != "SplashLoading";
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

        // Check if we're being used as a loading cover (additively loaded)
        if (IsUsedAsLoadingCover())
        {
            // We're just a loading cover, unload ourselves instead of navigating
            Debug.Log("[FakeLoadingController] Used as loading cover - unloading splash screen");
            SceneManager.UnloadSceneAsync("SplashLoading");
        }
        else
        {
            // We're the main scene, navigate to target scene
            string targetScene = GetTargetScene();
            
            if (!string.IsNullOrEmpty(targetScene))
            {
                Debug.Log($"[FakeLoadingController] Loading scene: {targetScene}");
                SceneManager.LoadScene(targetScene);
            }
            else
            {
                Debug.Log($"[FakeLoadingController] No target scene set, loading default: {defaultNextScene}");
                SceneManager.LoadScene(defaultNextScene);
            }
        }
    }
}
