using System.Collections.Generic;
using UnityEngine;

public class MonsterBookManager : MonoBehaviour
{
    public static MonsterBookManager Instance;

    [Header("Codex Settings")]
    [Tooltip("If true, discoveries are saved between sessions (Dagitab). If false, reset every run (Pactborn).")]
    public bool persistentUnlocks = false;

    [Header("UI References")]
    public MonsterBookUI discoveryPopup;   // 👈 shows up when a monster is discovered
    public MonsterDexUI monsterDexUI;      // 👈 full browsable codex
    public List<MonsterEntry> allEntries;  // 👈 all monsters in your world

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitializeCodex();
    }

    private void InitializeCodex()
    {
        foreach (var entry in allEntries)
        {
            if (persistentUnlocks)
            {
                // Load previously discovered entries
                entry.discovered = PlayerPrefs.GetInt(entry.entryName, 0) == 1;
            }
            else
            {
                // Reset discoveries for new runs
                entry.discovered = false;
            }
        }
    }

    /// <summary>
    /// Unlocks a new monster entry when discovered.
    /// </summary>
    public void UnlockEntry(MonsterEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("[MonsterBookManager] Tried to unlock a null entry!");
            return;
        }

        // Already discovered — skip
        if (entry.discovered)
        {
            Debug.Log($"[MonsterBookManager] {entry.entryName} already discovered.");
            return;
        }

        // Mark as discovered
        entry.discovered = true;

        // Save persistent discoveries
        if (persistentUnlocks)
        {
            PlayerPrefs.SetInt(entry.entryName, 1);
            PlayerPrefs.Save();
        }

        Debug.Log($"[MonsterBookManager] New entry unlocked: {entry.entryName}");

        // Show discovery popup
        if (discoveryPopup != null)
            discoveryPopup.Show(entry);
        else
            Debug.LogWarning("[MonsterBookManager] No discoveryPopup assigned!");

        // Optionally refresh the full codex view if open
        if (monsterDexUI != null && monsterDexUI.gameObject.activeSelf)
            monsterDexUI.Open(); // reinitialize to refresh data
    }

    /// <summary>
    /// Resets all discoveries.
    /// </summary>
    public void ResetCodex()
    {
        foreach (var entry in allEntries)
        {
            entry.discovered = false;
            PlayerPrefs.DeleteKey(entry.entryName);
        }

        PlayerPrefs.Save();
        Debug.Log("[MonsterBookManager] Codex reset complete.");

        // Refresh dex UI if open
        if (monsterDexUI != null && monsterDexUI.gameObject.activeSelf)
            monsterDexUI.Open();
    }

    /// <summary>
    /// Manually open the Monster Dex UI.
    /// </summary>
    public void OpenDex()
    {
        if (monsterDexUI != null)
            monsterDexUI.Open();
    }

    /// <summary>
    /// Closes the Monster Dex UI.
    /// </summary>
    public void CloseDex()
    {
        if (monsterDexUI != null)
            monsterDexUI.Close();
    }
}
