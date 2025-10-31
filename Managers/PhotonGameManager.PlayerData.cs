using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Partial - Player data and cloud save/load logic for PhotonGameManager
/// </summary>
public partial class PhotonGameManager
{
    /// <summary>
    /// Set the player name (call this from LobbyController)
    /// </summary>
    public void SetPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            LogDebug("Cannot set empty player name");
            return;
        }

        playerName = name;
        
        // Save to PlayerPrefs
        PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
        PlayerPrefs.Save();

        // Update Photon nickname
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.NickName = playerName;
            UpdatePlayerProperties();
        }

        LogDebug($"Player name set to: {playerName}");
    }

    /// <summary>
    /// Load saved player name from PlayerPrefs
    /// </summary>
    private void LoadPlayerName()
    {
        if (PlayerPrefs.HasKey(PLAYER_NAME_KEY))
        {
            playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY);
            LogDebug($"Loaded player name: {playerName}");
        }
    }

    /// <summary>
    /// Update player's custom properties in Photon (room session) - Name-based identification
    /// </summary>
    private void UpdatePlayerProperties()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null || !isInLobby)
            return;

        // Don't try to update properties while disconnecting
        if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Disconnecting ||
            PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Disconnected)
            return;

        var playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { PLAYER_NAME_KEY, playerName },
            { CURRENT_WAVE_KEY, currentWaveReached },
            { HIGHEST_WAVE_KEY, highestWaveReached },
            { TOTAL_KILLS_KEY, GetTotalKills() },
            { NAME_BASED_PREFIX + "ID", playerName.ToLower().Replace(" ", "") }
        };

        // Update room properties (visible to other players in current session)
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        
        // Also save to Photon Cloud (persistent across sessions)
        SavePlayerStatsToCloud();
        
        LogDebug($"Updated player properties: {playerName}, Wave: {currentWaveReached}, Highest: {highestWaveReached}");
    }

    /// <summary>
    /// Get total kills (you can implement this based on your kill tracking system)
    /// </summary>
    private int GetTotalKills()
    {
        return PlayerPrefs.GetInt($"TotalKills_{playerName}", 0);
    }

    /// <summary>
    /// Get total play time in seconds
    /// </summary>
    private int GetTotalPlayTime()
    {
        return PlayerPrefs.GetInt($"TotalPlayTime_{playerName}", 0);
    }

    /// <summary>
    /// Get total waves completed across all sessions
    /// </summary>
    private int GetWavesCompleted()
    {
        return PlayerPrefs.GetInt($"WavesCompleted_{playerName}", 0);
    }

    /// <summary>
    /// Get death count across all sessions
    /// </summary>
    private int GetDeathCount()
    {
        return PlayerPrefs.GetInt($"DeathCount_{playerName}", 0);
    }

    /// <summary>
    /// Get fastest wave completion time in seconds
    /// </summary>
    private float GetFastestWaveTime()
    {
        return PlayerPrefs.GetFloat($"FastestWave_{playerName}", 0f);
    }

    /// <summary>
    /// Increment total kills (call when enemy is killed)
    /// </summary>
    public void AddKill()
    {
        int currentKills = GetTotalKills() + 1;
        PlayerPrefs.SetInt($"TotalKills_{playerName}", currentKills);
        LogDebug($"Kill count updated: {currentKills}");
    }

    /// <summary>
    /// Add play time (call this regularly or on session end)
    /// </summary>
    public void AddPlayTime(int seconds)
    {
        int currentTime = GetTotalPlayTime() + seconds;
        PlayerPrefs.SetInt($"TotalPlayTime_{playerName}", currentTime);
    }

    /// <summary>
    /// Increment wave completion count (call when wave is completed)
    /// </summary>
    public void AddWaveCompletion()
    {
        int currentWaves = GetWavesCompleted() + 1;
        PlayerPrefs.SetInt($"WavesCompleted_{playerName}", currentWaves);
        LogDebug($"Waves completed updated: {currentWaves}");
    }

    /// <summary>
    /// Increment death count (call when player dies)
    /// </summary>
    public void AddDeath()
    {
        int currentDeaths = GetDeathCount() + 1;
        PlayerPrefs.SetInt($"DeathCount_{playerName}", currentDeaths);
        LogDebug($"Death count updated: {currentDeaths}");
    }

    /// <summary>
    /// Update fastest wave time if current time is better
    /// </summary>
    public void UpdateFastestWaveTime(float waveTimeSeconds)
    {
        float currentBest = GetFastestWaveTime();
        if (currentBest == 0f || waveTimeSeconds < currentBest)
        {
            PlayerPrefs.SetFloat($"FastestWave_{playerName}", waveTimeSeconds);
            LogDebug($"New fastest wave time: {waveTimeSeconds}s");
        }
    }

    /// <summary>
    /// Save highest wave to PlayerPrefs (persistent across sessions)
    /// </summary>
    private void SaveHighestWave(int wave)
    {
        PlayerPrefs.SetInt(HIGHEST_WAVE_KEY, wave);
        PlayerPrefs.Save();
        LogDebug($"Saved highest wave: {wave}");
    }

    /// <summary>
    /// Save highest wave locally only (runtime storage, not synced to Photon)
    /// </summary>
    private void SaveHighestWaveLocally(int wave)
    {
        // Just update the runtime variable, don't sync to Photon
        highestWaveReached = wave;
        // Optionally save to PlayerPrefs as backup
        PlayerPrefs.SetInt(HIGHEST_WAVE_KEY + "_Runtime", wave);
        LogDebug($"Saved highest wave locally (runtime): {wave}");
    }

    /// <summary>
    /// Load highest wave from PlayerPrefs
    /// </summary>
    public void LoadHighestWave()
    {
        if (PlayerPrefs.HasKey(HIGHEST_WAVE_KEY))
        {
            highestWaveReached = PlayerPrefs.GetInt(HIGHEST_WAVE_KEY);
            LogDebug($"Loaded highest wave: {highestWaveReached}");
        }
    }

    /// <summary>
    /// Reset player stats (for testing or new game)
    /// </summary>
    public void ResetPlayerStats()
    {
        currentWaveReached = 0;
        highestWaveReached = 0;
        PlayerPrefs.DeleteKey(HIGHEST_WAVE_KEY);
        PlayerPrefs.Save();
        UpdatePlayerProperties();
        LogDebug("Player stats reset");
    }

    /// <summary>
    /// Push runtime wave data to Photon Cloud - ONLY called on player death
    /// This is the ONLY method that syncs local data to Photon
    /// </summary>
    public void SaveCurrentWaveToLeaderboard()
    {
        // Track death count
        AddDeath();
        
        // Get current wave from WaveManager if available
        if (waveManager != null)
        {
            int waveFromManager = waveManager.GetCurrentWave();
            Debug.Log($"[PhotonGameManager] Getting wave from WaveManager: {waveFromManager} (was: {currentWaveReached})");
            currentWaveReached = waveFromManager;
        }
        else
        {
            Debug.LogError("[PhotonGameManager] ❌ WaveManager is NULL when trying to get current wave! Using cached value: " + currentWaveReached);
        }

        // Update highest wave if current is higher
        if (currentWaveReached > highestWaveReached)
        {
            highestWaveReached = currentWaveReached;
        }
        
        // Save to local PlayerPrefs first
        SaveHighestWave(highestWaveReached);

        // NOW push to Photon Cloud (only happens on death)
        UpdatePlayerProperties();
        
        // Ensure cloud save is completed
        SavePlayerStatsToCloud();

        LogDebug($"DEATH: Pushed wave progress to Photon Cloud - Current Wave: {currentWaveReached}, Highest Wave: {highestWaveReached}, Deaths: {GetDeathCount()}");
    }

    /// <summary>
    /// Save player stats to Photon Cloud (persistent across sessions) - Name-based storage
    /// Each name gets its own cloud record
    /// </summary>
    private void SavePlayerStatsToCloud()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null || !isInLobby)
        {
            LogDebug("Cannot save to cloud - not connected to Photon or not in lobby");
            return;
        }

        // Don't try to save while disconnecting
        if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Disconnecting ||
            PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Disconnected)
        {
            LogDebug("Cannot save to cloud - client is disconnecting");
            return;
        }

        // Create name-based cloud properties
        string nameKey = playerName.ToLower().Replace(" ", ""); // Normalize name for key
        
        var cloudProperties = new ExitGames.Client.Photon.Hashtable
        {
            { $"Cloud_{nameKey}_Name", playerName }, // Original name with formatting
            { $"Cloud_{nameKey}_HighestWave", highestWaveReached },
            { $"Cloud_{nameKey}_CurrentWave", currentWaveReached },
            { $"Cloud_{nameKey}_TotalKills", GetTotalKills() },
            { $"Cloud_{nameKey}_LastSaved", System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
            { $"Cloud_{nameKey}_TotalPlayTime", GetTotalPlayTime() }, // New: Track play time
            { $"Cloud_{nameKey}_WavesCompleted", GetWavesCompleted() }, // New: Total waves completed
            { $"Cloud_{nameKey}_DeathCount", GetDeathCount() }, // New: Death counter
            { $"Cloud_{nameKey}_FastestWave", GetFastestWaveTime() }, // New: Best wave completion time
            { "Cloud_ActivePlayerName", playerName } // Current active name for quick lookup
        };

        // Save to player's account (persists across sessions)
        PhotonNetwork.LocalPlayer.SetCustomProperties(cloudProperties);
        
        // Also save to local PlayerPrefs as backup
        SaveHighestWave(highestWaveReached);

        LogDebug($"Saved to Photon Cloud (Name-based) - Name: {playerName} (Key: {nameKey}), Highest Wave: {highestWaveReached}");
    }

    /// <summary>
    /// Load player stats from Photon Cloud based on current name
    /// Each name loads its own record
    /// </summary>
    private void LoadPlayerStatsFromCloud()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
        {
            LogDebug("Cannot load from cloud - not connected to Photon");
            return;
        }

        if (!isInLobby)
        {
            LogDebug("Cannot load from cloud - not in room yet");
            return;
        }

        var cloudProps = PhotonNetwork.LocalPlayer.CustomProperties;
        LogDebug($"Loading cloud data for '{playerName}'. Available properties: {cloudProps.Count}");
        
        // Load based on current player name
        string nameKey = playerName.ToLower().Replace(" ", ""); // Normalize name for key
        LogDebug($"Looking for cloud data with nameKey: '{nameKey}'");

        // Try to load data for this specific name
        string cloudHighestWaveKey = $"Cloud_{nameKey}_HighestWave";
        if (cloudProps.ContainsKey(cloudHighestWaveKey))
        {
            int cloudHighestWave = (int)cloudProps[cloudHighestWaveKey];
            
            // Use cloud value for this name
            highestWaveReached = cloudHighestWave;
            SaveHighestWave(highestWaveReached); // Update local storage
            LogDebug($"✓ Loaded name-based cloud data - Name: {playerName} (Key: {nameKey}), Highest Wave: {highestWaveReached}");
        }
        else
        {
            // Try loading from local PlayerPrefs as fallback
            if (PlayerPrefs.HasKey(HIGHEST_WAVE_KEY))
            {
                highestWaveReached = PlayerPrefs.GetInt(HIGHEST_WAVE_KEY);
                LogDebug($"⚠️ No cloud data found for name '{playerName}' - using local backup: {highestWaveReached}");
            }
            else
            {
                LogDebug($"❌ No cloud or local data found for name '{playerName}' - starting fresh");
                highestWaveReached = 0; // Start fresh for this name
            }
        }

        // Try to load current wave for this name
        string cloudCurrentWaveKey = $"Cloud_{nameKey}_CurrentWave";
        if (cloudProps.ContainsKey(cloudCurrentWaveKey))
        {
            currentWaveReached = (int)cloudProps[cloudCurrentWaveKey];
        }
        else
        {
            currentWaveReached = 0; // Reset current wave on new session
        }

        LogDebug($"Final loaded stats for '{playerName}': Highest Wave {highestWaveReached}, Current Wave {currentWaveReached}");
    }
}
