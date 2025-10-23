using System.Collections.Generic;
using UnityEngine;

public class MonsterBookManager : MonoBehaviour
{
    public static MonsterBookManager Instance;

    [Header("Codex Settings")]
    [Tooltip("Dagitab = true (persistent). Pactborn = false (reset each run).")]
    public bool persistentUnlocks = false;

    [Header("References")]
    public MonsterBookUI monsterBookUI;               // popup for new discoveries
    //public FullCodexArchiveUI fullCodexArchiveUI; // full codex menu
    public List<MonsterEntry> allEntries;           // all entries

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (persistentUnlocks)
        {
            // Load saved entries (Dagitab)
            foreach (var entry in allEntries)
                entry.discovered = PlayerPrefs.GetInt(entry.entryName, 0) == 1;
        }
        else
        {
            // Reset each run (Pactborn)
            foreach (var entry in allEntries)
                entry.discovered = false;
        }

        //fullCodexArchiveUI?.RefreshList(); // make sure archive starts synced
    }

    public void UnlockEntry(MonsterEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("[MonsterBookManager] Tried to unlock a null entry!");
            return;
        }

        if (entry.discovered)
            return; // already discovered

        entry.discovered = true;
        Debug.Log($"[MonsterBookManager] New Codex entry unlocked: {entry.entryName}");

        // Save if persistent
        if (persistentUnlocks)
        {
            PlayerPrefs.SetInt(entry.entryName, 1);
            PlayerPrefs.Save();
        }

        // Show popup UI
        monsterBookUI?.Show(entry);

        // Update full archive list
        //fullCodexArchiveUI?.RefreshList();
    }

    public void ResetCodex()
    {
        foreach (var entry in allEntries)
        {
            entry.discovered = false;
            PlayerPrefs.DeleteKey(entry.entryName);
        }

        PlayerPrefs.Save();
        //fullCodexArchiveUI?.RefreshList();

        Debug.Log("[CodexManager] Codex reset complete.");
    }
}
