using UnityEngine;

/// <summary>
/// Configuration for Supabase connection
/// Store your Supabase project URL and API key here
/// </summary>
[CreateAssetMenu(fileName = "SupabaseConfig", menuName = "Game/Supabase Configuration")]
public class SupabaseConfig : ScriptableObject
{
    [Header("Supabase Project Settings")]
    [Tooltip("Your Supabase project URL (e.g., https://xxxxx.supabase.co)")]
    public string supabaseUrl = "https://pgpzamkedrwavoywbpsq.supabase.co";
    
    [Tooltip("Your Supabase anon/public API key")]
    [TextArea(3, 5)]
    public string supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InBncHphbWtlZHJ3YXZveXdicHNxIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjIxMTgxMzMsImV4cCI6MjA3NzY5NDEzM30.1vB4eUM24jfVt8lxSrNsOLuzhPKrnFqzThlT2rHxuiQ";
    
    [Header("Table Configuration")]
    [Tooltip("Name of the leaderboard table in Supabase")]
    public string leaderboardTableName = "wave_leaderboards";
    
    [Header("Connection Settings")]
    [Tooltip("Request timeout in seconds")]
    public float requestTimeout = 10f;
    
    [Tooltip("Enable retry on connection failure")]
    public bool enableRetry = true;
    
    [Tooltip("Maximum retry attempts")]
    public int maxRetries = 3;
    
    [Header("Debug Settings")]
    [Tooltip("Show detailed debug logs for Supabase operations")]
    public bool showDebugLogs = true;
    
    [Tooltip("Test mode - prevents actual API calls")]
    public bool testMode = false;
    
    /// <summary>
    /// Validate configuration
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(supabaseUrl) || supabaseUrl.Contains("your-project"))
        {
            Debug.LogError("SupabaseConfig: Invalid Supabase URL! Please set your project URL.");
            return false;
        }
        
        if (string.IsNullOrEmpty(supabaseAnonKey) || supabaseAnonKey.Contains("your-anon-key"))
        {
            Debug.LogError("SupabaseConfig: Invalid API key! Please set your anon key.");
            return false;
        }
        
        return true;
    }
}
