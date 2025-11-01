using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages scene reset functionality to provide fresh game starts
/// Resets all game state when transitioning from MainBase back to MainMenu
/// </summary>
public class SceneResetManager : MonoBehaviour
{
    public static SceneResetManager Instance { get; private set; }

    [Header("Scene Reset Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mainBaseSceneName = "MainBase";
    [SerializeField] private float resetDelay = 0.5f; // Delay before scene transition

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Don't destroy on load so it persists across scenes
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Resets MainBase scene state and returns to main menu for a fresh start
    /// Call this when the player wants to quit to main menu
    /// </summary>
    public void QuitToMainMenuWithReset()
    {
        Debug.Log("[SceneResetManager] Starting quit to main menu with scene reset...");
        StartCoroutine(PerformQuitWithReset());
    }

    private IEnumerator PerformQuitWithReset()
    {
        // Resume time scale before reset operations
        Time.timeScale = 1f;

        // 1. Reset PhotonGameManager state (preserve leaderboard data but reset session)
        ResetPhotonGameManagerState();

        // 2. Reset WaveManager state
        ResetWaveManagerState();

        // 3. Clear any active enemies and objects
        ClearActiveGameObjects();

        // 4. Reset UI states
        ResetUIStates();

        // 5. Clear player spawn state so new characters can be selected
        ResetPlayerSpawnState();

        // 5.5. Force reset static flags again right before scene load (double-check)
        Debug.Log("[SceneResetManager] Final static flag reset before scene load...");
        PlayerSpawnManager.ResetGlobalSpawnFlags();

        // 6. Clear any persistent audio
        ResetAudioState();

        // Wait a moment for all resets to complete
        yield return new WaitForSeconds(resetDelay);

        Debug.Log("[SceneResetManager] Scene reset complete. Loading main menu...");

        // Unload SplashLoading scene if it's currently loaded to prevent redirect
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == "SplashLoading")
            {
                Debug.Log("[SceneResetManager] Unloading SplashLoading scene...");
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        // Clear any target scene for splash loading to prevent unwanted redirects
        PlayerPrefs.DeleteKey("TargetSceneAfterLoading");
        PlayerPrefs.SetInt("SkipFakeLoading", 0);
        PlayerPrefs.Save();

        // 7. Load main menu scene directly (always use "MainMenu" to avoid SplashLoading redirect)
        SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// Called when returning to MainBase from main menu to ensure player spawns
    /// </summary>
    public void OnReturnToMainBase()
    {
        Debug.Log("[SceneResetManager] Returned to MainBase - ensuring player spawned...");
        StartCoroutine(DelayedPlayerSpawnCheck());
    }
    
    private IEnumerator DelayedPlayerSpawnCheck()
    {
        // Wait for scene to fully load
        yield return new WaitForSeconds(0.5f);
        EnsurePlayerSpawned();
    }

    /// <summary>
    /// Reset PhotonGameManager session state (keep leaderboard data intact)
    /// </summary>
    private void ResetPhotonGameManagerState()
    {
        if (PhotonGameManager.Instance != null)
        {
            Debug.Log("[SceneResetManager] Resetting PhotonGameManager session state...");
            
            // Reset current wave but preserve highest wave for leaderboards
            PhotonGameManager.Instance.ResetCurrentSession();
        }
    }

    /// <summary>
    /// Reset WaveManager to initial state
    /// </summary>
    private void ResetWaveManagerState()
    {
        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            Debug.Log("[SceneResetManager] Resetting WaveManager state...");
            waveManager.ResetWaveManager();
        }
    }

    /// <summary>
    /// Clear all active enemies, projectiles, and temporary game objects
    /// </summary>
    private void ClearActiveGameObjects()
    {
        Debug.Log("[SceneResetManager] Clearing active game objects...");

        // Clear enemies
        Entity[] enemies = FindObjectsOfType<Entity>();
        foreach (Entity enemy in enemies)
        {
            if (enemy != null && !enemy.CompareTag("Player"))
            {
                Destroy(enemy.gameObject);
            }
        }

        // Clear projectiles and combat objects
        ProjectileTracer[] projectileTracers = FindObjectsOfType<ProjectileTracer>();
        foreach (ProjectileTracer tracer in projectileTracers)
        {
            if (tracer != null)
            {
                Destroy(tracer.gameObject);
            }
        }
        
        ProjectileSlingshot[] projectileSlingsshots = FindObjectsOfType<ProjectileSlingshot>();
        foreach (ProjectileSlingshot slingshot in projectileSlingsshots)
        {
            if (slingshot != null)
            {
                Destroy(slingshot.gameObject);
            }
        }
        
        CirclingProjectile[] circlingProjectiles = FindObjectsOfType<CirclingProjectile>();
        foreach (CirclingProjectile projectile in circlingProjectiles)
        {
            if (projectile != null)
            {
                Destroy(projectile.gameObject);
            }
        }

        // Clear any temporary effects or particles
        ParticleSystem[] particles = FindObjectsOfType<ParticleSystem>();
        foreach (ParticleSystem particle in particles)
        {
            if (particle != null && particle.CompareTag("Temporary"))
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    /// <summary>
    /// Reset UI states and close any open panels
    /// </summary>
    private void ResetUIStates()
    {
        Debug.Log("[SceneResetManager] Resetting UI states...");

        // Close any open UI panels
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ResetUIState();
        }

        // Reset pause state
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Reset PlayerSpawnManager state to allow fresh character selection
    /// </summary>
    private void ResetPlayerSpawnState()
    {
        Debug.Log("[SceneResetManager] ===== RESETTING PLAYER SPAWN STATE =====");

        // Reset static spawning flags globally before scene transition
        Debug.Log("[SceneResetManager] Calling PlayerSpawnManager.ResetGlobalSpawnFlags()...");
        PlayerSpawnManager.ResetGlobalSpawnFlags();
        
        PlayerSpawnManager spawnManager = FindObjectOfType<PlayerSpawnManager>();
        if (spawnManager != null)
        {
            Debug.Log($"[SceneResetManager] Found PlayerSpawnManager: {spawnManager.gameObject.name}");
            Debug.Log("[SceneResetManager] Calling spawnManager.ResetSpawnState()...");
            spawnManager.ResetSpawnState();
        }
        else
        {
            Debug.LogWarning("[SceneResetManager] ⚠️ No PlayerSpawnManager found in current scene for reset!");
        }
        
        Debug.Log("[SceneResetManager] ===== PLAYER SPAWN STATE RESET COMPLETE =====");
    }

    /// <summary>
    /// Stop any persistent audio
    /// </summary>
    private void ResetAudioState()
    {
        Debug.Log("[SceneResetManager] Resetting audio state...");

        // Stop all audio sources that might be playing background music or effects
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    /// <summary>
    /// Retry current level with reset (for retry functionality)
    /// </summary>
    public void RetryWithReset()
    {
        Debug.Log("[SceneResetManager] Starting retry with scene reset...");
        StartCoroutine(PerformRetryWithReset());
    }

    private IEnumerator PerformRetryWithReset()
    {
        Debug.Log($"[SceneResetManager] PerformRetryWithReset - Current Scene: {SceneManager.GetActiveScene().name}");
        
        // Resume time scale
        Time.timeScale = 1f;

        // Reset all game states (same as quit but stay in same scene)
        ResetPhotonGameManagerState();
        ResetWaveManagerState();
        ClearActiveGameObjects();
        ResetUIStates();
        ResetPlayerSpawnState();
        ResetAudioState();

        // Wait for reset to complete
        yield return new WaitForSeconds(resetDelay);

        // Final static flag reset before scene reload
        Debug.Log("[SceneResetManager] Final static flag reset before retry scene reload...");
        PlayerSpawnManager.ResetGlobalSpawnFlags();

        Debug.Log("[SceneResetManager] Scene reset complete. Loading MainBase with splash screen...");

        // Set flag to skip fake loading on retry
        PlayerPrefs.SetInt("SkipFakeLoading", 1);
        PlayerPrefs.Save();

        // Load splash screen first to cover the scene transition
        Debug.Log("[SceneResetManager] Loading SplashLoading screen...");
        AsyncOperation loadSplash = SceneManager.LoadSceneAsync("SplashLoading", LoadSceneMode.Additive);
        
        if (loadSplash != null)
        {
            while (!loadSplash.isDone)
            {
                yield return null;
            }
            Debug.Log("[SceneResetManager] SplashLoading screen loaded");
        }
        
        // Small delay to ensure splash screen is visible
        yield return new WaitForSeconds(0.2f);
        
        // Always load MainBase scene for retry (not current scene)
        Debug.Log("[SceneResetManager] Now loading MainBase scene...");
        AsyncOperation loadMainBase = SceneManager.LoadSceneAsync("MainBase");
        
        if (loadMainBase != null)
        {
            loadMainBase.allowSceneActivation = false;
            
            // Show loading progress
            while (loadMainBase.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(loadMainBase.progress / 0.9f);
                Debug.Log($"[SceneResetManager] Loading MainBase: {progress * 100:F1}%");
                yield return null;
            }
            
            // Minimum loading time to show splash screen
            yield return new WaitForSeconds(0.3f);
            
            // Activate the scene
            Debug.Log("[SceneResetManager] MainBase loaded, activating scene...");
            loadMainBase.allowSceneActivation = true;
            
            while (!loadMainBase.isDone)
            {
                yield return null;
            }
            
            Debug.Log("[SceneResetManager] MainBase scene fully loaded!");
        }
        
        // Wait a moment after scene load to ensure player spawning
        yield return new WaitForSeconds(0.5f);
        EnsurePlayerSpawned();
    }
    
    /// <summary>
    /// Ensures player is spawned after scene reset (fallback mechanism)
    /// </summary>
    private void EnsurePlayerSpawned()
    {
        Debug.Log("[SceneResetManager] ===== CHECKING PLAYER SPAWN STATUS =====");
        
        // Check if player exists in scene
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer == null)
        {
            Debug.LogWarning("[SceneResetManager] ❌ No player found after scene reset! Attempting to force spawn...");
            
            PlayerSpawnManager spawnManager = FindObjectOfType<PlayerSpawnManager>();
            if (spawnManager != null)
            {
                Debug.Log($"[SceneResetManager] Found PlayerSpawnManager: {spawnManager.gameObject.name}");
                
                // Check if it's properly configured first
                if (!spawnManager.IsProperlyConfigured())
                {
                    Debug.LogError("[SceneResetManager] ❌ PlayerSpawnManager is not properly configured! Check inspector assignments.");
                    return;
                }
                
                Debug.Log("[SceneResetManager] Calling ForceSpawnPlayer()...");
                spawnManager.ForceSpawnPlayer();
                
                // Start coroutine to verify force spawn worked after a delay
                StartCoroutine(VerifyForceSpawnResult());
            }
            else
            {
                Debug.LogError("[SceneResetManager] ❌ PlayerSpawnManager not found! Cannot force spawn player.");
                Debug.LogError("[SceneResetManager] Available objects in scene:");
                PlayerSpawnManager[] allSpawners = FindObjectsOfType<PlayerSpawnManager>();
                Debug.LogError($"[SceneResetManager] Found {allSpawners.Length} PlayerSpawnManager(s):");
                foreach (var spawner in allSpawners)
                {
                    Debug.LogError($"  - {spawner.gameObject.name} (active: {spawner.gameObject.activeInHierarchy})");
                }
            }
        }
        else
        {
            Debug.Log($"[SceneResetManager] ✅ Player successfully spawned after reset: {existingPlayer.name}");
        }
        
        Debug.Log("[SceneResetManager] ===== PLAYER SPAWN CHECK COMPLETE =====");
    }
    
    private IEnumerator VerifyForceSpawnResult()
    {
        yield return new WaitForSeconds(0.2f);
        GameObject newPlayer = GameObject.FindGameObjectWithTag("Player");
        if (newPlayer != null)
        {
            Debug.Log($"[SceneResetManager] ✅ Force spawn successful: {newPlayer.name}");
        }
        else
        {
            Debug.LogError("[SceneResetManager] ❌ Force spawn FAILED! Still no player found!");
        }
    }
}