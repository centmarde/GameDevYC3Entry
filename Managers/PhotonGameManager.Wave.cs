using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Partial - Wave tracking and scene/session reset logic
/// </summary>
public partial class PhotonGameManager
{
    /// <summary>
    /// Called when a wave starts (from WaveManager event)
    /// </summary>
    private void OnWaveStarted(int waveNumber)
    {
        currentWaveReached = waveNumber;
        
        // Update highest wave if current is higher
        if (currentWaveReached > highestWaveReached)
        {
            highestWaveReached = currentWaveReached;
            // Only save locally during runtime, don't push to Photon yet
            SaveHighestWaveLocally(highestWaveReached);
        }

        // Don't update Photon properties during gameplay - only on death
        // UpdatePlayerProperties(); // intentionally deferred to death

        LogDebug($"Wave {waveNumber} started. Highest wave: {highestWaveReached} (stored locally)");
    }

    /// <summary>
    /// Called when a wave completes (from WaveManager event)
    /// </summary>
    private void OnWaveCompleted(int waveNumber)
    {
        LogDebug($"Wave {waveNumber} completed");
        
        // Track wave completion
        AddWaveCompletion();
        
        // Update fastest wave time if WaveManager provides timing data
        if (waveManager != null)
        {
            // TODO: Get wave completion time from WaveManager
            // Example: float waveTime = waveManager.GetLastWaveCompletionTime();
            // UpdateFastestWaveTime(waveTime);
        }
    }

    /// <summary>
    /// Dynamically find and connect to WaveManager (for when it's on player prefab)
    /// </summary>
    private void TryFindWaveManager()
    {
        // Search for WaveManager in the scene
        WaveManager foundWaveManager = FindObjectOfType<WaveManager>();
        
        if (foundWaveManager != null && foundWaveManager != waveManager)
        {
            Debug.Log($"[PhotonGameManager] ✓ WaveManager found dynamically: {foundWaveManager.gameObject.name}");
            
            // Unsubscribe from old manager if it exists
            if (waveManager != null)
            {
                waveManager.OnWaveStart.RemoveListener(OnWaveStarted);
                waveManager.OnWaveComplete.RemoveListener(OnWaveCompleted);
                Debug.Log("[PhotonGameManager] Unsubscribed from old WaveManager");
            }
            
            // Connect to new WaveManager
            waveManager = foundWaveManager;
            
            // Subscribe to wave events
            waveManager.OnWaveStart.AddListener(OnWaveStarted);
            waveManager.OnWaveComplete.AddListener(OnWaveCompleted);
            
            Debug.Log($"[PhotonGameManager] ✓ Successfully connected to WaveManager on {waveManager.gameObject.name}");
            
            // Get current wave if available
            int currentWaveFromManager = waveManager.GetCurrentWave();
            if (currentWaveFromManager > 0)
            {
                Debug.Log($"[PhotonGameManager] Syncing with current wave: {currentWaveFromManager}");
                currentWaveReached = currentWaveFromManager;
                
                if (currentWaveReached > highestWaveReached)
                {
                    highestWaveReached = currentWaveReached;
                    SaveHighestWaveLocally(highestWaveReached);
                }
            }
        }
    }

    /// <summary>
    /// Force reconnection to WaveManager (call this when player spawns)
    /// </summary>
    public void ForceConnectToWaveManager()
    {
        Debug.Log("[PhotonGameManager] Force connecting to WaveManager...");
        TryFindWaveManager();
        
        if (waveManager != null)
        {
            Debug.Log("[PhotonGameManager] ✓ Force connection successful!");
        }
        else
        {
            Debug.LogWarning("[PhotonGameManager] ⚠️ Force connection failed - WaveManager still not found");
        }
    }

    /// <summary>
    /// Reset current session state while preserving leaderboard data
    /// Used when quitting to main menu for a fresh start
    /// </summary>
    public void ResetCurrentSession()
    {
        LogDebug("Resetting PhotonGameManager session state...");
        
        // Reset current wave to 0 (but keep highest wave for leaderboards)
        currentWaveReached = 0;
        
        // Update room properties to reflect reset state
        UpdatePlayerProperties();
        
        // Disconnect from current WaveManager (will reconnect when new player spawns)
        if (waveManager != null)
        {
            waveManager.OnWaveStart.RemoveListener(OnWaveStarted);
            waveManager.OnWaveComplete.RemoveListener(OnWaveCompleted);
            waveManager = null;
        }
        
        LogDebug($"Session reset complete. Current wave: {currentWaveReached}, Highest wave preserved: {highestWaveReached}");
    }
}
