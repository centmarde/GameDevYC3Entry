using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages leaderboard data with three-tier system:
/// 1. Local (current session)
/// 2. Photon Cloud (lobby players)
/// 3. Supabase (persistent offline/global scores)
/// </summary>
public class SupabaseLeaderboardManager : MonoBehaviour
{
    public static SupabaseLeaderboardManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private SupabaseConfig config;
    
    // WaveManager is auto-detected at runtime (NO inspector assignment needed)
    private WaveManager waveManager;
    
    // Property to get WaveManager (auto-find if not set)
    private WaveManager WaveManagerInstance
    {
        get
        {
            if (waveManager == null)
            {
                waveManager = FindObjectOfType<WaveManager>();
            }
            return waveManager;
        }
    }
    
    [Header("Player Identity")]
    [Tooltip("Unique player ID (auto-generated if empty)")]
    [SerializeField] private string playerId;
    
    [Tooltip("Player display name (loaded from PlayerPrefs/LobbyController)")]
    [SerializeField] private string playerName = "Player";
    
    [Tooltip("Automatically load player name from PlayerPrefs on start")]
    [SerializeField] private bool autoLoadPlayerName = true;
    
    [Header("Sync Settings")]
    [Tooltip("Auto-save to Supabase when wave completes")]
    [SerializeField] private bool autoSaveOnWaveComplete = true;
    
    [Tooltip("Auto-save interval in seconds (0 = disabled)")]
    [SerializeField] private float autoSaveInterval = 60f;
    
    [Tooltip("Save to Supabase when game quits")]
    [SerializeField] private bool saveOnQuit = true;
    
    [Header("Leaderboard Settings")]
    [Tooltip("How many top scores to fetch from Supabase")]
    [SerializeField] private int topScoresLimit = 100;
    
    [Tooltip("Cache duration in seconds before refetching")]
    [SerializeField] private float cacheExpirationTime = 300f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Local data
    private LeaderboardEntry localEntry;
    private List<SupabaseLeaderboardEntry> cachedSupabaseScores = new List<SupabaseLeaderboardEntry>();
    private float lastCacheTime = 0f;
    private float autoSaveTimer = 0f;
    private bool isSaving = false;
    
    // Stats tracking
    private int sessionKills = 0;
    private int sessionDeaths = 0;
    private int sessionWavesCompleted = 0;
    private float sessionStartTime = 0f;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Generate player ID if not set
        if (string.IsNullOrEmpty(playerId))
        {
            playerId = LoadOrGeneratePlayerId();
        }
        
        // Load player name from PlayerPrefs if auto-load is enabled
        if (autoLoadPlayerName)
        {
            LoadPlayerNameFromPrefs();
        }
        
        // Initialize local entry
        localEntry = new LeaderboardEntry
        {
            playerName = playerName,
            currentWave = 0,
            highestWave = LoadLocalHighestWave(),
            totalKills = 0,
            isLocalPlayer = true
        };
        
        sessionStartTime = Time.time;
    }
    
    private void Start()
    {
        // Auto-detect WaveManager (NO inspector assignment needed)
        waveManager = FindObjectOfType<WaveManager>();
        
        if (waveManager == null)
        {
            Debug.LogError("[Supabase] WaveManager NOT FOUND! Wave tracking will not work. Ensure WaveManager is in the scene.");
        }
        else
        {
            LogDebug($"WaveManager detected: {waveManager.gameObject.name}");
            
            // Subscribe to wave events
            waveManager.OnWaveComplete.AddListener(OnWaveCompleted);
            waveManager.OnAllEnemiesCleared.AddListener(OnWaveCleared);
            
            // Sync initial wave state
            int currentWave = waveManager.GetCurrentWave();
            if (currentWave > 0)
            {
                localEntry.currentWave = currentWave;
                LogDebug($"Initial wave synced: {currentWave}");
            }
        }
        
        // Load initial data from Supabase
        StartCoroutine(LoadTopScoresFromSupabase());
        
        LogDebug($"Initialized - Player: {playerName} (ID: {playerId})");
    }
    
    private void Update()
    {
        // Auto-save timer
        if (autoSaveInterval > 0)
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= autoSaveInterval && !isSaving)
            {
                autoSaveTimer = 0f;
                SaveToSupabase();
            }
        }
    }
    
    #region Player Identity
    
    /// <summary>
    /// Load or generate a unique player ID
    /// </summary>
    private string LoadOrGeneratePlayerId()
    {
        string savedId = PlayerPrefs.GetString("Supabase_PlayerId", "");
        
        if (string.IsNullOrEmpty(savedId))
        {
            // Generate new ID: device ID + timestamp for uniqueness
            savedId = $"{SystemInfo.deviceUniqueIdentifier}_{DateTime.UtcNow.Ticks}";
            PlayerPrefs.SetString("Supabase_PlayerId", savedId);
            PlayerPrefs.Save();
            LogDebug($"Generated new Player ID: {savedId}");
        }
        
        return savedId;
    }
    
    /// <summary>
    /// Update player name
    /// </summary>
    public void SetPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            LogDebug("Attempted to set empty player name, ignoring.");
            return;
        }
        
        playerName = name;
        if (localEntry != null)
        {
            localEntry.playerName = name;
        }
        
        // Also save to PlayerPrefs for persistence
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
        
        LogDebug($"Player name updated to: {name}");
    }
    
    /// <summary>
    /// Load player name from PlayerPrefs
    /// </summary>
    private void LoadPlayerNameFromPrefs()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            string savedName = PlayerPrefs.GetString("PlayerName");
            if (!string.IsNullOrEmpty(savedName))
            {
                playerName = savedName;
                LogDebug($"Loaded player name from PlayerPrefs: {savedName}");
            }
        }
    }
    
    /// <summary>
    /// Get current player name
    /// </summary>
    public string GetPlayerName()
    {
        return playerName;
    }
    
    #endregion
    
    #region Wave Event Handlers
    
    /// <summary>
    /// Called when a wave completes (all enemies spawned)
    /// </summary>
    private void OnWaveCompleted(int waveNumber)
    {
        Debug.Log($"[Supabase] ✓✓✓ OnWaveCompleted EVENT FIRED! Wave: {waveNumber}");
        
        // Sync with WaveManager's current wave to ensure accuracy
        if (waveManager != null)
        {
            int actualWave = waveManager.GetCurrentWave();
            Debug.Log($"[Supabase] Synced with WaveManager: Event wave={waveNumber}, Actual wave={actualWave}");
            waveNumber = actualWave;
        }
        else
        {
            Debug.LogWarning($"[Supabase] WaveManager is NULL during OnWaveCompleted!");
        }
        
        // Update local entry
        localEntry.currentWave = waveNumber;
        if (waveNumber > localEntry.highestWave)
        {
            localEntry.highestWave = waveNumber;
            SaveLocalHighestWave(waveNumber);
        }
        
        sessionWavesCompleted++;
        localEntry.wavesCompleted = sessionWavesCompleted;
        
        // Log all current stats
        Debug.Log($"[Supabase] [Wave Stats] Current: {localEntry.currentWave}, Highest: {localEntry.highestWave}, Kills: {localEntry.totalKills}, WavesCompleted: {localEntry.wavesCompleted}, Deaths: {localEntry.deathCount}");
    }
    
    /// <summary>
    /// Called when all enemies from a wave are cleared
    /// </summary>
    private void OnWaveCleared(int waveNumber)
    {
        LogDebug($"Wave {waveNumber} cleared! All enemies defeated.");
        
        // Auto-save to Supabase
        if (autoSaveOnWaveComplete)
        {
            SaveToSupabase();
        }
    }
    
    /// <summary>
    /// Register enemy kill
    /// </summary>
    public void RegisterKill()
    {
        sessionKills++;
        localEntry.totalKills = sessionKills;
        
        // CRITICAL FIX: Sync current wave on EVERY kill since wave events aren't firing reliably
        if (waveManager != null)
        {
            int currentWave = waveManager.GetCurrentWave();
            if (currentWave != localEntry.currentWave || currentWave > localEntry.highestWave)
            {
                localEntry.currentWave = currentWave;
                if (currentWave > localEntry.highestWave)
                {
                    localEntry.highestWave = currentWave;
                    SaveLocalHighestWave(currentWave);
                    Debug.Log($"[Supabase] NEW HIGHEST WAVE: {currentWave}!");
                }
                LogDebug($"[Supabase] Wave synced on kill: Now on wave {currentWave}");
            }
        }
        
        if (showDebugLogs && sessionKills % 10 == 0) // Log every 10 kills to avoid spam
        {
            LogDebug($"[Kill Tracker] Total kills: {sessionKills}");
        }
    }
    
    /// <summary>
    /// Register player death
    /// </summary>
    public void RegisterDeath()
    {
        sessionDeaths++;
        localEntry.deathCount = sessionDeaths;
        LogDebug($"[Death Tracker] Total deaths: {sessionDeaths}");
    }
    
    #endregion
    
    #region Supabase Operations
    
    /// <summary>
    /// Save current progress to Supabase
    /// </summary>
    public void SaveToSupabase()
    {
        if (isSaving)
        {
            LogDebug("Already saving, skipping...");
            return;
        }
        
        if (SupabaseClient.Instance == null || !SupabaseClient.Instance.IsConfigured())
        {
            Debug.LogWarning("SupabaseClient not configured! Cannot save to database.");
            return;
        }
        
        StartCoroutine(SaveToSupabaseCoroutine());
    }
    
    private IEnumerator SaveToSupabaseCoroutine()
    {
        isSaving = true;
        
        // Sync current wave from WaveManager before saving
        if (WaveManagerInstance != null)
        {
            int currentWave = WaveManagerInstance.GetCurrentWave();
            localEntry.currentWave = currentWave;
            if (currentWave > localEntry.highestWave)
            {
                localEntry.highestWave = currentWave;
                SaveLocalHighestWave(currentWave);
            }
            LogDebug($"Synced current wave before save: {currentWave}");
        }
        
        // Update total play time
        localEntry.totalPlayTime = Mathf.RoundToInt(Time.time - sessionStartTime);
        
        // Log current stats before conversion
        LogDebug($"[Pre-Save] Current: {localEntry.currentWave}, Highest: {localEntry.highestWave}, Kills: {localEntry.totalKills}, WavesCompleted: {localEntry.wavesCompleted}, PlayTime: {localEntry.totalPlayTime}s, Deaths: {localEntry.deathCount}");
        
        // Convert to Supabase format
        SupabaseLeaderboardEntry supabaseEntry = SupabaseLeaderboardEntry.FromLeaderboardEntry(
            localEntry, 
            playerId, 
            SystemInfo.deviceUniqueIdentifier
        );
        
        // Log converted data
        LogDebug($"[Converted] current_wave: {supabaseEntry.current_wave}, highest_wave: {supabaseEntry.highest_wave}, total_kills: {supabaseEntry.total_kills}, waves_completed: {supabaseEntry.waves_completed}, total_play_time: {supabaseEntry.total_play_time}, death_count: {supabaseEntry.death_count}");
        
        // Check if player already has a record (using player_name as key)
        string sanitizedName = Uri.EscapeDataString(playerName);
        string checkQuery = $"player_name=eq.{sanitizedName}";
        bool recordExists = false;
        
        yield return SupabaseClient.Instance.Get(config.leaderboardTableName, checkQuery, (response, success) =>
        {
            if (success && !string.IsNullOrEmpty(response) && response != "[]")
            {
                recordExists = true;
                LogDebug($"Existing record found for player '{playerName}', will update...");
            }
        });
        
        // Prepare request body
        string jsonBody = JsonUtility.ToJson(new SupabaseLeaderboardRequest
        {
            player_name = supabaseEntry.player_name,
            player_id = supabaseEntry.player_id,
            current_wave = supabaseEntry.current_wave,
            highest_wave = supabaseEntry.highest_wave,
            total_kills = supabaseEntry.total_kills,
            total_play_time = supabaseEntry.total_play_time,
            waves_completed = supabaseEntry.waves_completed,
            death_count = supabaseEntry.death_count,
            fastest_wave_time = supabaseEntry.fastest_wave_time,
            device_id = supabaseEntry.device_id,
            game_version = supabaseEntry.game_version
        });
        
        LogDebug($"[JSON Body] {jsonBody}");
        
        if (recordExists)
        {
            // Update existing record
            yield return SupabaseClient.Instance.Patch(config.leaderboardTableName, checkQuery, jsonBody, (response, success) =>
            {
                if (success)
                {
                    LogDebug($"✓ Successfully updated leaderboard! Wave: {localEntry.currentWave}, Highest: {localEntry.highestWave}");
                }
                else
                {
                    Debug.LogError($"Failed to update leaderboard: {response}");
                }
            });
        }
        else
        {
            // Insert new record
            yield return SupabaseClient.Instance.Post(config.leaderboardTableName, jsonBody, (response, success) =>
            {
                if (success)
                {
                    LogDebug($"✓ Successfully saved new leaderboard entry! Wave: {localEntry.currentWave}");
                }
                else
                {
                    Debug.LogError($"Failed to save leaderboard: {response}");
                }
            });
        }
        
        isSaving = false;
    }
    
    /// <summary>
    /// Load top scores from Supabase
    /// </summary>
    public IEnumerator LoadTopScoresFromSupabase()
    {
        if (SupabaseClient.Instance == null || !SupabaseClient.Instance.IsConfigured())
        {
            Debug.LogWarning("SupabaseClient not configured!");
            yield break;
        }
        
        // Query: Get top X scores ordered by highest_wave descending
        string query = $"select=*&order=highest_wave.desc,current_wave.desc&limit={topScoresLimit}";
        
        yield return SupabaseClient.Instance.Get(config.leaderboardTableName, query, (response, success) =>
        {
            if (success)
            {
                try
                {
                    // Parse JSON array
                    cachedSupabaseScores = ParseLeaderboardArray(response);
                    lastCacheTime = Time.time;
                    
                    LogDebug($"✓ Loaded {cachedSupabaseScores.Count} scores from Supabase");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse Supabase response: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Failed to load scores: {response}");
            }
        });
    }
    
    /// <summary>
    /// Parse JSON array of leaderboard entries
    /// </summary>
    private List<SupabaseLeaderboardEntry> ParseLeaderboardArray(string json)
    {
        List<SupabaseLeaderboardEntry> entries = new List<SupabaseLeaderboardEntry>();
        
        // Unity's JsonUtility doesn't support arrays directly, so we wrap it
        string wrappedJson = "{\"items\":" + json + "}";
        SupabaseLeaderboardArrayWrapper wrapper = JsonUtility.FromJson<SupabaseLeaderboardArrayWrapper>(wrappedJson);
        
        if (wrapper != null && wrapper.items != null)
        {
            entries = wrapper.items;
        }
        
        return entries;
    }
    
    #endregion
    
    #region Combined Leaderboard (Local + Photon + Supabase)
    
    /// <summary>
    /// Get combined leaderboard from all sources
    /// Priority: Local > Photon > Supabase
    /// </summary>
    public List<LeaderboardEntry> GetCombinedLeaderboard()
    {
        List<LeaderboardEntry> combined = new List<LeaderboardEntry>();
        
        // 1. Add local player
        combined.Add(localEntry);
        
        // 2. Add Photon lobby players (if connected)
        if (PhotonGameManager.Instance != null && PhotonGameManager.Instance.IsInLobby())
        {
            List<LeaderboardEntry> photonScores = PhotonGameManager.Instance.GetLeaderboardData();
            if (photonScores != null)
            {
                // Exclude local player (already added)
                combined.AddRange(photonScores.Where(e => !e.isLocalPlayer));
            }
        }
        
        // 3. Add Supabase scores (offline/global players)
        // Refresh cache if expired
        if (Time.time - lastCacheTime > cacheExpirationTime)
        {
            StartCoroutine(LoadTopScoresFromSupabase());
        }
        
        // Convert Supabase entries to LeaderboardEntry
        foreach (var supabaseEntry in cachedSupabaseScores)
        {
            // Skip if player is already in list (from Photon or local)
            if (combined.Any(e => e.playerName == supabaseEntry.player_name))
                continue;
            
            combined.Add(supabaseEntry.ToLeaderboardEntry());
        }
        
        // Sort by highest wave (descending), then by current wave
        combined = combined
            .OrderByDescending(e => e.highestWave)
            .ThenByDescending(e => e.currentWave)
            .ToList();
        
        return combined;
    }
    
    /// <summary>
    /// Get only Supabase (persistent) scores
    /// </summary>
    public List<LeaderboardEntry> GetSupabaseScoresOnly()
    {
        return cachedSupabaseScores
            .Select(e => e.ToLeaderboardEntry())
            .ToList();
    }
    
    #endregion
    
    #region Local Storage
    
    private int LoadLocalHighestWave()
    {
        return PlayerPrefs.GetInt("LocalHighestWave", 0);
    }
    
    private void SaveLocalHighestWave(int wave)
    {
        PlayerPrefs.SetInt("LocalHighestWave", wave);
        PlayerPrefs.Save();
    }
    
    #endregion
    
    #region Public Getters
    
    public LeaderboardEntry GetLocalEntry() => localEntry;
    public int GetCurrentWave() => localEntry.currentWave;
    public int GetHighestWave() => localEntry.highestWave;
    public int GetTotalKills() => localEntry.totalKills;
    public bool IsSaving() => isSaving;
    
    #endregion
    
    private void OnApplicationQuit()
    {
        if (saveOnQuit && !isSaving)
        {
            // Force synchronous save (note: coroutines don't work in OnApplicationQuit)
            // This is a best-effort save
            LogDebug("Saving to Supabase on quit...");
            SaveToSupabase();
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (waveManager != null)
        {
            waveManager.OnWaveComplete.RemoveListener(OnWaveCompleted);
            waveManager.OnAllEnemiesCleared.RemoveListener(OnWaveCleared);
        }
    }
    
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[SupabaseLeaderboardManager] {message}");
        }
    }
    
    /// <summary>
    /// Get full hierarchy path of GameObject for debugging
    /// </summary>
    private string GetGameObjectPath(GameObject obj)
    {
        if (obj == null) return "<null>";
        
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
}

/// <summary>
/// Helper class to deserialize JSON array
/// </summary>
[Serializable]
public class SupabaseLeaderboardArrayWrapper
{
    public List<SupabaseLeaderboardEntry> items;
}
