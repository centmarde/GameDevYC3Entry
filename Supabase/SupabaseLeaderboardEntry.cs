using System;
using UnityEngine;

/// <summary>
/// Supabase-specific leaderboard entry with additional fields for persistence
/// Matches the database schema in Supabase
/// </summary>
[Serializable]
public class SupabaseLeaderboardEntry
{
    // Primary fields
    public string id;                    // UUID from Supabase
    public string player_name;           // Player's display name (UNIQUE KEY - primary identifier)
    public string player_id;             // Additional identifier (device ID or custom ID)
    public int current_wave;             // Current wave reached
    public int highest_wave;             // Highest wave ever reached
    public int total_kills;              // Total enemies killed
    
    // Extended stats
    public int total_play_time;          // Total play time in seconds
    public int waves_completed;          // Total waves completed
    public int death_count;              // Total deaths
    public float fastest_wave_time;      // Best wave completion time
    
    // Metadata
    public string created_at;            // When record was created (ISO timestamp)
    public string updated_at;            // Last update timestamp
    public string device_id;             // Device identifier for user tracking
    public string game_version;          // Game version for this record
    
    // Ranking (calculated server-side or client-side)
    public int rank;                     // Player's rank position
    
    /// <summary>
    /// Convert from local LeaderboardEntry to Supabase format
    /// </summary>
    public static SupabaseLeaderboardEntry FromLeaderboardEntry(LeaderboardEntry entry, string playerId, string deviceId = null)
    {
        return new SupabaseLeaderboardEntry
        {
            player_name = entry.playerName,
            player_id = playerId,
            current_wave = entry.currentWave,
            highest_wave = entry.highestWave,
            total_kills = entry.totalKills,
            total_play_time = entry.totalPlayTime,
            waves_completed = entry.wavesCompleted,
            death_count = entry.deathCount,
            fastest_wave_time = entry.fastestWaveTime,
            device_id = deviceId ?? SystemInfo.deviceUniqueIdentifier,
            game_version = Application.version,
            updated_at = DateTime.UtcNow.ToString("o")
        };
    }
    
    /// <summary>
    /// Convert to local LeaderboardEntry format
    /// </summary>
    public LeaderboardEntry ToLeaderboardEntry(bool isLocalPlayer = false)
    {
        return new LeaderboardEntry
        {
            playerName = player_name,
            currentWave = current_wave,
            highestWave = highest_wave,
            totalKills = total_kills,
            isLocalPlayer = isLocalPlayer,
            totalPlayTime = total_play_time,
            wavesCompleted = waves_completed,
            deathCount = death_count,
            fastestWaveTime = fastest_wave_time,
            lastSaved = updated_at
        };
    }
}

/// <summary>
/// Response wrapper for Supabase API calls
/// </summary>
[Serializable]
public class SupabaseResponse<T>
{
    public bool success;
    public string message;
    public T data;
    public int statusCode;
}

/// <summary>
/// Request body for inserting/updating leaderboard entry
/// </summary>
[Serializable]
public class SupabaseLeaderboardRequest
{
    public string player_name;
    public string player_id;
    public int current_wave;
    public int highest_wave;
    public int total_kills;
    public int total_play_time;
    public int waves_completed;
    public int death_count;
    public float fastest_wave_time;
    public string device_id;
    public string game_version;
}
