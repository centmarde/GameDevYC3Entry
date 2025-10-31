using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Partial - Leaderboard and cloud data aggregation helpers
/// </summary>
public partial class PhotonGameManager
{
    /// <summary>
    /// Get all players and their unique name records for the leaderboard
    /// Returns best record for each unique name (name-based leaderboard)
    /// </summary>
    public List<LeaderboardEntry> GetLeaderboardData()
    {
        List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();

        if (!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom == null)
        {
            LogDebug("Not in a room, cannot get leaderboard data");
            return leaderboard;
        }

        LogDebug($"Getting leaderboard data from {PhotonNetwork.PlayerList.Length} players in room...");

        // Dictionary to track best record per name
        Dictionary<string, LeaderboardEntry> bestRecordsByName = new Dictionary<string, LeaderboardEntry>();
        int totalCloudPropsFound = 0;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            var props = player.CustomProperties;
            LogDebug($"Player {player.NickName} (IsLocal: {player.IsLocal}) has {props.Count} custom properties");
            
            // Look for all name-based cloud records in player properties
            foreach (var property in props)
            {
                string key = property.Key.ToString();
                
                // Find name-based highest wave properties
                if (key.StartsWith("Cloud_") && key.EndsWith("_HighestWave"))
                {
                    totalCloudPropsFound++;
                    // Extract name key from property key (Cloud_{nameKey}_HighestWave)
                    string nameKey = key.Substring(6, key.Length - 18); // Remove "Cloud_" prefix and "_HighestWave" suffix
                    
                    // Get the original name
                    string nameProperty = $"Cloud_{nameKey}_Name";
                    string playerName = props.ContainsKey(nameProperty) ? props[nameProperty].ToString() : nameKey;
                    
                    int highestWave = (int)property.Value;
                    
                    LogDebug($"Found cloud data: {playerName} (nameKey: {nameKey}) - Highest Wave: {highestWave}");
                    
                    // Check if this is the best record for this name
                    if (!bestRecordsByName.ContainsKey(playerName) || 
                        bestRecordsByName[playerName].highestWave < highestWave)
                    {
                        // Get additional stats for this name
                        int currentWave = props.ContainsKey($"Cloud_{nameKey}_CurrentWave") ? 
                            (int)props[$"Cloud_{nameKey}_CurrentWave"] : 0;
                        int totalKills = props.ContainsKey($"Cloud_{nameKey}_TotalKills") ? 
                            (int)props[$"Cloud_{nameKey}_TotalKills"] : 0;
                        
                        // Get extended stats (with fallbacks)
                        int totalPlayTime = props.ContainsKey($"Cloud_{nameKey}_TotalPlayTime") ? 
                            (int)props[$"Cloud_{nameKey}_TotalPlayTime"] : 0;
                        int wavesCompleted = props.ContainsKey($"Cloud_{nameKey}_WavesCompleted") ? 
                            (int)props[$"Cloud_{nameKey}_WavesCompleted"] : 0;
                        int deathCount = props.ContainsKey($"Cloud_{nameKey}_DeathCount") ? 
                            (int)props[$"Cloud_{nameKey}_DeathCount"] : 0;
                        float fastestWaveTime = props.ContainsKey($"Cloud_{nameKey}_FastestWave") ? 
                            (float)props[$"Cloud_{nameKey}_FastestWave"] : 0f;
                        string lastSaved = props.ContainsKey($"Cloud_{nameKey}_LastSaved") ? 
                            props[$"Cloud_{nameKey}_LastSaved"].ToString() : "";

                        bestRecordsByName[playerName] = new LeaderboardEntry
                        {
                            playerName = playerName,
                            currentWave = currentWave,
                            highestWave = highestWave,
                            totalKills = totalKills,
                            isLocalPlayer = player.IsLocal && playerName == this.playerName,
                            totalPlayTime = totalPlayTime,
                            wavesCompleted = wavesCompleted,
                            deathCount = deathCount,
                            fastestWaveTime = fastestWaveTime,
                            lastSaved = lastSaved
                        };
                        
                        LogDebug($"✓ Added/Updated leaderboard entry for {playerName}: Wave {highestWave} (Current: {currentWave})");
                    }
                }
            }
        }

        // Convert dictionary to list
        leaderboard = bestRecordsByName.Values.ToList();

        // Sort by highest wave (descending), then by current wave
        leaderboard.Sort((a, b) =>
        {
            int compare = b.highestWave.CompareTo(a.highestWave);
            if (compare == 0)
                compare = b.currentWave.CompareTo(a.currentWave);
            return compare;
        });

        LogDebug($"Generated name-based leaderboard with {leaderboard.Count} unique names from {totalCloudPropsFound} cloud properties");
        return leaderboard;
    }

    /// <summary>
    /// Get ALL historical leaderboard data from ALL players and ALL sessions
    /// This shows every name record that has ever been saved to Photon Cloud
    /// </summary>
    public List<LeaderboardEntry> GetAllHistoricalLeaderboardData()
    {
        List<LeaderboardEntry> allHistoricalData = new List<LeaderboardEntry>();

        if (!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom == null)
        {
            LogDebug("Not in a room, cannot get historical leaderboard data");
            return allHistoricalData;
        }

        LogDebug($"Getting ALL historical data from {PhotonNetwork.PlayerList.Length} players in room...");

        // Dictionary to ensure we get the BEST record for each unique name across ALL players
        Dictionary<string, LeaderboardEntry> allNameRecords = new Dictionary<string, LeaderboardEntry>();
        int totalHistoricalRecords = 0;

        // Search through ALL players (current and historical)
        foreach (var player in PhotonNetwork.PlayerList)
        {
            var props = player.CustomProperties;
            LogDebug($"Scanning player {player.NickName} (IsLocal: {player.IsLocal}) - {props.Count} properties");
            
            // Look for ALL name-based cloud records in this player's properties
            foreach (var property in props)
            {
                string key = property.Key.ToString();
                
                // Find ALL name-based highest wave properties (from any session)
                if (key.StartsWith("Cloud_") && key.EndsWith("_HighestWave"))
                {
                    totalHistoricalRecords++;
                    
                    // Extract name key from property key
                    string nameKey = key.Substring(6, key.Length - 18);
                    
                    // Get the original name
                    string nameProperty = $"Cloud_{nameKey}_Name";
                    string historicalPlayerName = props.ContainsKey(nameProperty) ? props[nameProperty].ToString() : nameKey;
                    
                    int highestWave = (int)property.Value;
                    
                    LogDebug($"Found historical record: '{historicalPlayerName}' (nameKey: {nameKey}) - Wave: {highestWave}");
                    
                    // Check if this is the BEST record for this name across ALL players
                    if (!allNameRecords.ContainsKey(historicalPlayerName) || 
                        allNameRecords[historicalPlayerName].highestWave < highestWave)
                    {
                        // Get all available stats for this name
                        int currentWave = props.ContainsKey($"Cloud_{nameKey}_CurrentWave") ? 
                            (int)props[$"Cloud_{nameKey}_CurrentWave"] : 0;
                        int totalKills = props.ContainsKey($"Cloud_{nameKey}_TotalKills") ? 
                            (int)props[$"Cloud_{nameKey}_TotalKills"] : 0;
                        int totalPlayTime = props.ContainsKey($"Cloud_{nameKey}_TotalPlayTime") ? 
                            (int)props[$"Cloud_{nameKey}_TotalPlayTime"] : 0;
                        int wavesCompleted = props.ContainsKey($"Cloud_{nameKey}_WavesCompleted") ? 
                            (int)props[$"Cloud_{nameKey}_WavesCompleted"] : 0;
                        int deathCount = props.ContainsKey($"Cloud_{nameKey}_DeathCount") ? 
                            (int)props[$"Cloud_{nameKey}_DeathCount"] : 0;
                        float fastestWaveTime = props.ContainsKey($"Cloud_{nameKey}_FastestWave") ? 
                            (float)props[$"Cloud_{nameKey}_FastestWave"] : 0f;
                        string lastSaved = props.ContainsKey($"Cloud_{nameKey}_LastSaved") ? 
                            props[$"Cloud_{nameKey}_LastSaved"].ToString() : "Unknown";

                        // Determine if this is the current local player's active name
                        bool isCurrentLocalPlayer = player.IsLocal && historicalPlayerName == this.playerName;
                        
                        allNameRecords[historicalPlayerName] = new LeaderboardEntry
                        {
                            playerName = historicalPlayerName,
                            currentWave = currentWave,
                            highestWave = highestWave,
                            totalKills = totalKills,
                            isLocalPlayer = isCurrentLocalPlayer,
                            totalPlayTime = totalPlayTime,
                            wavesCompleted = wavesCompleted,
                            deathCount = deathCount,
                            fastestWaveTime = fastestWaveTime,
                            lastSaved = lastSaved
                        };
                        
                        LogDebug($"✓ Added historical record: {historicalPlayerName} - Wave {highestWave}, Last Saved: {lastSaved}");
                    }
                }
            }
        }

        // Convert to list
        allHistoricalData = allNameRecords.Values.ToList();

        // Sort by highest wave (descending), then by last saved date (most recent first)
        allHistoricalData.Sort((a, b) =>
        {
            int compare = b.highestWave.CompareTo(a.highestWave);
            if (compare == 0)
            {
                // Secondary sort by last saved (more recent first)
                if (System.DateTime.TryParse(a.lastSaved, out System.DateTime dateA) && 
                    System.DateTime.TryParse(b.lastSaved, out System.DateTime dateB))
                {
                    compare = dateB.CompareTo(dateA);
                }
                else
                {
                    // Fallback to current wave if dates can't be parsed
                    compare = b.currentWave.CompareTo(a.currentWave);
                }
            }
            return compare;
        });

        LogDebug($"Generated COMPLETE historical leaderboard: {allHistoricalData.Count} unique names from {totalHistoricalRecords} total records");
        LogDebug($"Historical names found: {string.Join(", ", allHistoricalData.Select(x => $"{x.playerName}(W{x.highestWave})"))}");
        
        return allHistoricalData;
    }

    /// <summary>
    /// Get player's rank on the leaderboard (1-based)
    /// </summary>
    public int GetPlayerRank()
    {
        List<LeaderboardEntry> leaderboard = GetLeaderboardData();
        
        for (int i = 0; i < leaderboard.Count; i++)
        {
            if (leaderboard[i].isLocalPlayer)
            {
                return i + 1; // 1-based rank
            }
        }

        return -1; // Not found
    }

    /// <summary>
    /// Force refresh cloud data and update leaderboard
    /// Call this if leaderboard data seems stale
    /// </summary>
    public void RefreshCloudDataAndLeaderboard()
    {
        LogDebug("=== REFRESHING CLOUD DATA AND LEADERBOARD ===");
        
        if (!PhotonNetwork.IsConnected || !isInLobby)
        {
            LogDebug("Cannot refresh - not connected or not in lobby");
            return;
        }
        
        // Force reload from cloud
        LoadPlayerStatsFromCloud();
        
        // Log all current cloud properties for debugging
        LogCloudProperties();
        
        LogDebug("Cloud data refresh completed");
    }
    
    /// <summary>
    /// Debug method to log all cloud properties for all players
    /// </summary>
    private void LogCloudProperties()
    {
        LogDebug("=== CURRENT CLOUD PROPERTIES ===");
        
        foreach (var player in PhotonNetwork.PlayerList)
        {
            var props = player.CustomProperties;
            LogDebug($"Player {player.NickName} (IsLocal: {player.IsLocal}): {props.Count} properties");
            
            foreach (var prop in props)
            {
                if (prop.Key.ToString().StartsWith("Cloud_"))
                {
                    LogDebug($"  {prop.Key}: {prop.Value}");
                }
            }
        }
    }

    /// <summary>
    /// Check if cloud data exists for this player
    /// </summary>
    public bool HasCloudData()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
            return false;

        string cloudHighestWaveKey = "Cloud_" + HIGHEST_WAVE_KEY;
        return PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(cloudHighestWaveKey);
    }
}
