using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class MonsterDexUI : MonoBehaviour
{
    public static MonsterDexUI Instance;
    [Header("Display")]
    public MonsterEntryDisplayUI displayUI;


    [Header("UI Elements")]
    public GameObject rootPanel;

    [Header("Navigation")]
    public Button nextButton;
    public Button prevButton;
    public TMP_Text pageCounter;

    [Header("Fallbacks")]
    public Sprite unknownSprite;

    private List<MonsterEntry> allEntries;
    private int currentIndex = 0;
    private bool isOpen = false;

    private void Awake()
    {
        Instance = this;
        rootPanel.SetActive(false);
    }

    private void Start()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);

        if (prevButton != null)
            prevButton.onClick.AddListener(PrevPage);
    }

    public void Open()
    {
        allEntries = MonsterBookManager.Instance.allEntries;
        if (allEntries == null || allEntries.Count == 0)
        {
            Debug.LogWarning("[MonsterDexUI] No entries found in MonsterBookManager!");
            return;
        }

        isOpen = true;
        rootPanel.SetActive(true);
        Time.timeScale = 0f;
        currentIndex = Mathf.Clamp(currentIndex, 0, allEntries.Count - 1);
        UpdatePage();
    }

    public void Close()
    {
        if (!isOpen) return;

        isOpen = false;
        rootPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void NextPage()
    {
        if (allEntries == null || allEntries.Count == 0) return;
        currentIndex = (currentIndex + 1) % allEntries.Count;
        UpdatePage();
    }

    private void PrevPage()
    {
        if (allEntries == null || allEntries.Count == 0) return;
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = allEntries.Count - 1;
        UpdatePage();
    }

    private void UpdatePage()
    {
        if (allEntries == null || allEntries.Count == 0) return;

        MonsterEntry entry = allEntries[currentIndex];
        if (entry == null) return;

        displayUI.ShowEntry(entry);

        // Update page counter
        if (pageCounter)
            pageCounter.text = $"{currentIndex + 1}/{allEntries.Count}";
    }
}
