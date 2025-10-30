using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterBookUI : MonoBehaviour
{
    public static MonsterBookUI Instance;

    [Header("UI Elements")]
    public GameObject rootPanel;
    public Button closeButton;

    [Header("Display")]
    public MonsterEntryDisplayUI displayUI;   // Shared display component

    [Header("Settings")]
    public float autoCloseDelay = 4f;

    private void Awake()
    {
        Instance = this;
        rootPanel.SetActive(false);
    }

    public void Show(MonsterEntry entry)
    {
        if (entry == null) return;

        rootPanel.SetActive(true);
        Time.timeScale = 0f;

        displayUI.ShowEntry(entry); // use shared display

        // Auto close after a few seconds
        StartCoroutine(AutoClose());
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSecondsRealtime(autoCloseDelay);
        Close();
    }

    public void Close()
    {
        rootPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
