using UnityEngine;

public partial class PhotonGameManager
{
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[PhotonGameManager] {message}");
        }
    }

    /// <summary>
    /// DEBUG: Refresh cloud data and leaderboard
    /// </summary>
    [ContextMenu("Refresh Cloud Data and Leaderboard")]
    public void DebugRefreshCloudData()
    {
        RefreshCloudDataAndLeaderboard();
    }

    /// <summary>
    /// DEBUG: Display all historical leaderboard data
    /// </summary>
    [ContextMenu("Debug Historical Leaderboard")]
    public void DebugHistoricalLeaderboard()
    {
        var historicalData = GetAllHistoricalLeaderboardData();
        LogDebug("=== COMPLETE HISTORICAL LEADERBOARD ===");
        
        for (int i = 0; i < historicalData.Count; i++)
        {
            var entry = historicalData[i];
            LogDebug($"#{i + 1}: {entry.playerName} - Wave {entry.highestWave} " +
                    $"(Kills: {entry.totalKills}, Deaths: {entry.deathCount}, " +
                    $"Completed: {entry.wavesCompleted}, Last: {entry.lastSaved})");
        }
    }

    /// <summary>
    /// DEBUG: Check WaveManager connection status
    /// </summary>
    [ContextMenu("Debug WaveManager Connection")]
    public void DebugWaveManagerConnection()
    {
        Debug.Log("[PhotonGameManager] === WAVEMANAGER DEBUG ==== ");
        Debug.Log($"WaveManager assigned: {(waveManager != null ? "✓ YES" : "❌ NO")}");
        
        if (waveManager != null)
        {
            Debug.Log($"WaveManager GameObject: {waveManager.gameObject.name}");
            Debug.Log($"WaveManager enabled: {waveManager.enabled}");
            Debug.Log($"WaveManager active: {waveManager.gameObject.activeInHierarchy}");
            Debug.Log($"Current Wave from WaveManager: {waveManager.GetCurrentWave()}");
            Debug.Log($"WaveManager OnWaveStart listeners: {(waveManager.OnWaveStart != null ? waveManager.OnWaveStart.GetPersistentEventCount().ToString() : "NULL")}");
        }
        else
        {
            Debug.LogError("WaveManager is NULL! Searching in scene...");
            
            WaveManager[] allWaveManagers = FindObjectsOfType<WaveManager>();
            Debug.Log($"Found {allWaveManagers.Length} WaveManager(s) in scene:");
            
            for (int i = 0; i < allWaveManagers.Length; i++)
            {
                var wm = allWaveManagers[i];
                Debug.Log($"  [{i}] {wm.gameObject.name} - Active: {wm.gameObject.activeInHierarchy} - Enabled: {wm.enabled}");
            }
            
            if (allWaveManagers.Length > 0)
            {
                Debug.LogWarning("WaveManagers exist but not connected to PhotonGameManager! Please assign in inspector.");
            }
        }
        
        Debug.Log($"PhotonGameManager currentWaveReached: {currentWaveReached}");
        Debug.Log($"PhotonGameManager highestWaveReached: {highestWaveReached}");
        Debug.Log("[PhotonGameManager] === END DEBUG ====");
    }
}
