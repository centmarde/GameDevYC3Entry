using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wave Leaderboard UI - Displays player wave progress from Photon
/// Shows all players in the lobby and their current/highest waves
/// </summary>
public class WaveLeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent container for leaderboard entries")]
    [SerializeField] private Transform leaderboardContainer;
    
    [Tooltip("Prefab for a single leaderboard entry")]
    [SerializeField] private GameObject leaderboardEntryPrefab;
    
    [Tooltip("Text to show when no players are online")]
    [SerializeField] private TextMeshProUGUI noPlayersText;
    
    [Tooltip("Title text (optional)")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Update Settings")]
    [Tooltip("How often to refresh the leaderboard (seconds)")]
    [SerializeField] private float updateInterval = 2f;
    
    [Tooltip("Automatically refresh on enable")]
    [SerializeField] private bool autoRefreshOnEnable = true;

    [Header("Styling")]
    [Tooltip("Highlight color for local player")]
    [SerializeField] private Color localPlayerColor = new Color(1f, 0.9f, 0.5f); // Gold
    
    [Tooltip("Normal color for other players")]
    [SerializeField] private Color normalColor = Color.white;

    [Header("Display Options")]
    [Tooltip("Maximum number of entries to show (0 = show all)")]
    [SerializeField] private int maxEntriesToShow = 10;
    
    [Tooltip("Show player rank numbers")]
    [SerializeField] private bool showRankNumbers = true;
    
    [Tooltip("Show current wave")]
    [SerializeField] private bool showCurrentWave = true;
    
    [Tooltip("Show highest wave reached")]
    [SerializeField] private bool showHighestWave = true;

    private float updateTimer = 0f;
    private List<GameObject> activeEntries = new List<GameObject>();

    private void OnEnable()
    {
        if (autoRefreshOnEnable)
        {
            RefreshLeaderboard();
        }
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;
        
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            RefreshLeaderboard();
        }
    }

    /// <summary>
    /// Refresh the leaderboard display
    /// </summary>
    public void RefreshLeaderboard()
    {
        if (PhotonGameManager.Instance == null)
        {
            ShowNoPlayers("Game Manager not found");
            return;
        }

        if (!PhotonGameManager.Instance.IsConnectedToPhoton())
        {
            ShowNoPlayers("Not connected to server");
            return;
        }

        if (!PhotonGameManager.Instance.IsInLobby())
        {
            ShowNoPlayers("Not in lobby");
            return;
        }

        // Get leaderboard data
        List<LeaderboardEntry> leaderboard = PhotonGameManager.Instance.GetLeaderboardData();

        if (leaderboard == null || leaderboard.Count == 0)
        {
            ShowNoPlayers("No players online");
            return;
        }

        // Hide no players message
        if (noPlayersText != null)
        {
            noPlayersText.gameObject.SetActive(false);
        }

        // Clear existing entries
        ClearEntries();

        // Determine how many entries to show
        int entriesToShow = maxEntriesToShow > 0 ? Mathf.Min(maxEntriesToShow, leaderboard.Count) : leaderboard.Count;

        // Create new entries
        for (int i = 0; i < entriesToShow; i++)
        {
            CreateLeaderboardEntry(leaderboard[i], i + 1);
        }
    }

    /// <summary>
    /// Create a single leaderboard entry
    /// </summary>
    private void CreateLeaderboardEntry(LeaderboardEntry data, int rank)
    {
        if (leaderboardEntryPrefab == null || leaderboardContainer == null)
        {
            Debug.LogError("WaveLeaderboardUI: Leaderboard entry prefab or container not assigned!");
            return;
        }

        GameObject entry = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
        activeEntries.Add(entry);

        // Find text components (assumes prefab has these children)
        TextMeshProUGUI rankText = entry.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI currentWaveText = entry.transform.Find("CurrentWaveText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI highestWaveText = entry.transform.Find("HighestWaveText")?.GetComponent<TextMeshProUGUI>();
        Image background = entry.GetComponent<Image>();

        // Set rank
        if (rankText != null && showRankNumbers)
        {
            rankText.text = $"#{rank}";
        }

        // Set name
        if (nameText != null)
        {
            nameText.text = data.playerName;
            
            // Add indicator for local player
            if (data.isLocalPlayer)
            {
                nameText.text += " (You)";
            }
        }

        // Set current wave
        if (currentWaveText != null && showCurrentWave)
        {
            currentWaveText.text = $"Current: Wave {data.currentWave}";
        }
        else if (currentWaveText != null)
        {
            currentWaveText.gameObject.SetActive(false);
        }

        // Set highest wave
        if (highestWaveText != null && showHighestWave)
        {
            highestWaveText.text = $"Best: Wave {data.highestWave}";
        }
        else if (highestWaveText != null)
        {
            highestWaveText.gameObject.SetActive(false);
        }

        // Highlight local player
        if (data.isLocalPlayer && background != null)
        {
            background.color = localPlayerColor;
        }
        else if (background != null)
        {
            background.color = normalColor;
        }

        // Apply color to all text
        Color textColor = data.isLocalPlayer ? Color.black : Color.white;
        if (rankText != null) rankText.color = textColor;
        if (nameText != null) nameText.color = textColor;
        if (currentWaveText != null) currentWaveText.color = textColor;
        if (highestWaveText != null) highestWaveText.color = textColor;
    }

    /// <summary>
    /// Clear all leaderboard entries
    /// </summary>
    private void ClearEntries()
    {
        foreach (GameObject entry in activeEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        activeEntries.Clear();
    }

    /// <summary>
    /// Show "no players" message
    /// </summary>
    private void ShowNoPlayers(string message)
    {
        ClearEntries();
        
        if (noPlayersText != null)
        {
            noPlayersText.gameObject.SetActive(true);
            noPlayersText.text = message;
        }
    }

    /// <summary>
    /// Force an immediate refresh
    /// </summary>
    public void ForceRefresh()
    {
        updateTimer = 0f;
        RefreshLeaderboard();
    }

    private void OnDisable()
    {
        ClearEntries();
    }
}
