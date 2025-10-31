using System.Collections;
using UnityEngine;

/// <summary>
/// WaveProgressManager - Handles local runtime storage of wave progress
/// Only pushes to Photon Cloud when the player dies
/// Independent of any UI buttons or user actions
/// </summary>
public class WaveProgressManager : MonoBehaviour
{
    public static WaveProgressManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private WaveManager waveManager;

    [Header("Runtime Data")]
    [SerializeField] private int currentWaveRuntime = 0;
    [SerializeField] private int highestWaveRuntime = 0;
    [SerializeField] private int totalKillsRuntime = 0;
    [SerializeField] private float survivalTimeRuntime = 0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Events
    public delegate void WaveProgressUpdated(int currentWave, int highestWave);
    public event WaveProgressUpdated OnWaveProgressUpdated;

    private bool isTrackingTime = false;
    private float sessionStartTime = 0f;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Find WaveManager if not assigned
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
        }
    }

    private void Start()
    {
        // Subscribe to wave events
        if (waveManager != null)
        {
            waveManager.OnWaveStart.AddListener(OnWaveStarted);
            waveManager.OnWaveComplete.AddListener(OnWaveCompleted);
            LogDebug("Subscribed to WaveManager events");
        }
        else
        {
            LogDebug("WaveManager not found! Please assign in inspector or ensure it exists in scene", true);
        }

        // Start session timer
        StartSurvivalTracking();
    }

    private void Update()
    {
        // Update survival time
        if (isTrackingTime)
        {
            survivalTimeRuntime = Time.time - sessionStartTime;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (waveManager != null)
        {
            waveManager.OnWaveStart.RemoveListener(OnWaveStarted);
            waveManager.OnWaveComplete.RemoveListener(OnWaveCompleted);
        }
    }

    #region Wave Tracking

    /// <summary>
    /// Called when a new wave starts
    /// </summary>
    private void OnWaveStarted(int waveNumber)
    {
        currentWaveRuntime = waveNumber;
        
        // Update highest wave if needed
        if (currentWaveRuntime > highestWaveRuntime)
        {
            highestWaveRuntime = currentWaveRuntime;
        }

        LogDebug($"Wave Started: {waveNumber} | Highest: {highestWaveRuntime}");
        
        // Fire event for any UI that needs updating
        OnWaveProgressUpdated?.Invoke(currentWaveRuntime, highestWaveRuntime);
    }

    /// <summary>
    /// Called when a wave is completed
    /// </summary>
    private void OnWaveCompleted(int waveNumber)
    {
        LogDebug($"Wave Completed: {waveNumber}");
    }

    /// <summary>
    /// Start tracking survival time
    /// </summary>
    public void StartSurvivalTracking()
    {
        sessionStartTime = Time.time;
        isTrackingTime = true;
        LogDebug("Started survival time tracking");
    }

    /// <summary>
    /// Stop tracking survival time
    /// </summary>
    public void StopSurvivalTracking()
    {
        isTrackingTime = false;
        LogDebug($"Stopped survival tracking. Total time: {survivalTimeRuntime:F1} seconds");
    }

    #endregion

    #region Kill Tracking

    /// <summary>
    /// Increment kill counter (call this from enemy death events)
    /// </summary>
    public void RecordKill()
    {
        totalKillsRuntime++;
        LogDebug($"Kill recorded. Total: {totalKillsRuntime}");
    }

    #endregion

    #region Player Death Handling

    /// <summary>
    /// Called when the player dies - pushes all runtime data to Photon Cloud
    /// </summary>
    public void OnPlayerDeath()
    {
        LogDebug($"Player Death Detected! Pushing to Photon Cloud...");
        
        // Stop survival tracking
        StopSurvivalTracking();

        // Push to Photon Cloud via PhotonGameManager
        if (PhotonGameManager.Instance != null)
        {
            // Save the runtime wave data
            PhotonGameManager.Instance.SaveCurrentWaveToLeaderboard();
            
            LogDebug($"Pushed to Photon - Wave: {currentWaveRuntime}, Highest: {highestWaveRuntime}, Kills: {totalKillsRuntime}, Time: {survivalTimeRuntime:F1}s");
        }
        else
        {
            LogDebug("PhotonGameManager not found! Saving locally only.", true);
            
            // Fallback: Save to PlayerPrefs
            SaveToLocalStorage();
        }
    }

    /// <summary>
    /// Save progress to local storage as fallback
    /// </summary>
    private void SaveToLocalStorage()
    {
        PlayerPrefs.SetInt("LastWaveReached", currentWaveRuntime);
        PlayerPrefs.SetInt("HighestWave", highestWaveRuntime);
        PlayerPrefs.SetInt("TotalKills", totalKillsRuntime);
        PlayerPrefs.SetFloat("SurvivalTime", survivalTimeRuntime);
        PlayerPrefs.Save();
        
        LogDebug("Saved to local storage (PlayerPrefs)");
    }

    #endregion

    #region Public Getters

    public int GetCurrentWaveRuntime() => currentWaveRuntime;
    public int GetHighestWaveRuntime() => highestWaveRuntime;
    public int GetTotalKillsRuntime() => totalKillsRuntime;
    public float GetSurvivalTimeRuntime() => survivalTimeRuntime;

    /// <summary>
    /// Force update current wave (use if WaveManager events are not firing)
    /// </summary>
    public void ForceUpdateCurrentWave()
    {
        if (waveManager != null)
        {
            currentWaveRuntime = waveManager.GetCurrentWave();
            
            if (currentWaveRuntime > highestWaveRuntime)
            {
                highestWaveRuntime = currentWaveRuntime;
            }
            
            OnWaveProgressUpdated?.Invoke(currentWaveRuntime, highestWaveRuntime);
            LogDebug($"Force updated wave: {currentWaveRuntime}");
        }
    }

    #endregion

    #region Debug

    private void LogDebug(string message, bool isError = false)
    {
        if (!showDebugLogs) return;

        if (isError)
        {
            Debug.LogError($"[WaveProgressManager] {message}");
        }
        else
        {
            Debug.Log($"[WaveProgressManager] {message}");
        }
    }

    /// <summary>
    /// Debug method to simulate player death
    /// </summary>
    [ContextMenu("Simulate Player Death")]
    private void SimulatePlayerDeath()
    {
        LogDebug("Simulating player death...");
        OnPlayerDeath();
    }

    #endregion
}