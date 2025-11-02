using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example usage of Supabase Leaderboard integration
/// Attach this to a GameObject to test the system
/// </summary>
public class SupabaseLeaderboardExample : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runTestOnStart = false;
    [SerializeField] private string testPlayerName = "TestPlayer";
    
    private void Start()
    {
        if (runTestOnStart)
        {
            RunTests();
        }
    }
    
    /// <summary>
    /// Run a series of tests to verify Supabase integration
    /// </summary>
    [ContextMenu("Run Supabase Tests")]
    public void RunTests()
    {
        Debug.Log("=== Supabase Leaderboard Tests ===");
        
        // Test 1: Check if managers exist
        if (SupabaseLeaderboardManager.Instance == null)
        {
            Debug.LogError("❌ SupabaseLeaderboardManager not found in scene!");
            return;
        }
        Debug.Log("✓ SupabaseLeaderboardManager found");
        
        if (SupabaseClient.Instance == null)
        {
            Debug.LogError("❌ SupabaseClient not found in scene!");
            return;
        }
        Debug.Log("✓ SupabaseClient found");
        
        // Test 2: Check configuration
        if (!SupabaseClient.Instance.IsConfigured())
        {
            Debug.LogError("❌ SupabaseClient is not properly configured!");
            Debug.LogError("Please create a SupabaseConfig asset and assign it to SupabaseClient");
            return;
        }
        Debug.Log("✓ SupabaseClient is configured");
        
        // Test 3: Set player name
        SupabaseLeaderboardManager.Instance.SetPlayerName(testPlayerName);
        Debug.Log($"✓ Set player name to: {testPlayerName}");
        
        // Test 4: Simulate some gameplay
        SimulateGameplay();
        
        // Test 5: Save to Supabase
        SupabaseLeaderboardManager.Instance.SaveToSupabase();
        Debug.Log("✓ Triggered save to Supabase");
        
        // Test 6: Load top scores
        StartCoroutine(SupabaseLeaderboardManager.Instance.LoadTopScoresFromSupabase());
        Debug.Log("✓ Triggered load from Supabase");
        
        Debug.Log("=== Tests Complete ===");
        Debug.Log("Check Supabase dashboard in ~5 seconds to see your record!");
    }
    
    /// <summary>
    /// Simulate some gameplay data
    /// </summary>
    private void SimulateGameplay()
    {
        // Simulate killing 10 enemies
        for (int i = 0; i < 10; i++)
        {
            SupabaseLeaderboardManager.Instance.RegisterKill();
        }
        Debug.Log("✓ Registered 10 kills");
        
        // Simulate a death
        SupabaseLeaderboardManager.Instance.RegisterDeath();
        Debug.Log("✓ Registered 1 death");
    }
    
    /// <summary>
    /// Get and display combined leaderboard
    /// </summary>
    [ContextMenu("Display Combined Leaderboard")]
    public void DisplayCombinedLeaderboard()
    {
        if (SupabaseLeaderboardManager.Instance == null)
        {
            Debug.LogError("SupabaseLeaderboardManager not found!");
            return;
        }
        
        List<LeaderboardEntry> leaderboard = SupabaseLeaderboardManager.Instance.GetCombinedLeaderboard();
        
        Debug.Log("=== Combined Leaderboard (Local + Photon + Supabase) ===");
        Debug.Log($"Total Entries: {leaderboard.Count}");
        
        for (int i = 0; i < Mathf.Min(10, leaderboard.Count); i++)
        {
            LeaderboardEntry entry = leaderboard[i];
            string indicator = entry.isLocalPlayer ? " ← YOU" : "";
            Debug.Log($"#{i + 1} {entry.playerName}: Wave {entry.highestWave} (Current: {entry.currentWave}){indicator}");
        }
    }
    
    /// <summary>
    /// Get and display Supabase-only leaderboard
    /// </summary>
    [ContextMenu("Display Supabase Leaderboard")]
    public void DisplaySupabaseLeaderboard()
    {
        if (SupabaseLeaderboardManager.Instance == null)
        {
            Debug.LogError("SupabaseLeaderboardManager not found!");
            return;
        }
        
        List<LeaderboardEntry> leaderboard = SupabaseLeaderboardManager.Instance.GetSupabaseScoresOnly();
        
        Debug.Log("=== Supabase Leaderboard (Persistent Scores) ===");
        Debug.Log($"Total Entries: {leaderboard.Count}");
        
        for (int i = 0; i < Mathf.Min(10, leaderboard.Count); i++)
        {
            LeaderboardEntry entry = leaderboard[i];
            Debug.Log($"#{i + 1} {entry.playerName}: Wave {entry.highestWave} (Current: {entry.currentWave})");
        }
    }
    
    /// <summary>
    /// Force save current progress
    /// </summary>
    [ContextMenu("Force Save to Supabase")]
    public void ForceSave()
    {
        if (SupabaseLeaderboardManager.Instance == null)
        {
            Debug.LogError("SupabaseLeaderboardManager not found!");
            return;
        }
        
        SupabaseLeaderboardManager.Instance.SaveToSupabase();
        Debug.Log("Saving to Supabase...");
    }
    
    /// <summary>
    /// Force load from Supabase
    /// </summary>
    [ContextMenu("Force Load from Supabase")]
    public void ForceLoad()
    {
        if (SupabaseLeaderboardManager.Instance == null)
        {
            Debug.LogError("SupabaseLeaderboardManager not found!");
            return;
        }
        
        StartCoroutine(SupabaseLeaderboardManager.Instance.LoadTopScoresFromSupabase());
        Debug.Log("Loading from Supabase...");
    }
    
    /// <summary>
    /// Display local player stats
    /// </summary>
    [ContextMenu("Display Local Stats")]
    public void DisplayLocalStats()
    {
        if (SupabaseLeaderboardManager.Instance == null)
        {
            Debug.LogError("SupabaseLeaderboardManager not found!");
            return;
        }
        
        LeaderboardEntry local = SupabaseLeaderboardManager.Instance.GetLocalEntry();
        
        Debug.Log("=== Local Player Stats ===");
        Debug.Log($"Name: {local.playerName}");
        Debug.Log($"Current Wave: {local.currentWave}");
        Debug.Log($"Highest Wave: {local.highestWave}");
        Debug.Log($"Total Kills: {local.totalKills}");
        Debug.Log($"Deaths: {local.deathCount}");
        Debug.Log($"Waves Completed: {local.wavesCompleted}");
        Debug.Log($"Play Time: {local.totalPlayTime}s");
    }
}
