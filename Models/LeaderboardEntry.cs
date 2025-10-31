using System;
using UnityEngine;

/// <summary>
/// Leaderboard entry data structure
/// </summary>
[Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int currentWave;
    public int highestWave;
    public int totalKills;
    public bool isLocalPlayer;
    
    // Extended stats (optional)
    public int totalPlayTime;      // Total play time in seconds
    public int wavesCompleted;     // Total waves completed
    public int deathCount;         // Total deaths
    public float fastestWaveTime;  // Best wave completion time in seconds
    public string lastSaved;       // When this record was last updated
}
