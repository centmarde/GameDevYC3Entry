using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterDatabaseUI : MonoBehaviour
{
    public static MonsterDatabaseUI Instance;   // ← add this

    [Header("Grid Setup")]
    public Transform gridContainer;             // Assign MonsterGrid
    public GameObject monsterButtonPrefab;      // Assign MonsterButton prefab

    [Header("Display")]
    public MonsterEntryDisplayUI displayUI;     // Assign the right-side display panel

    [Header("Visuals")]
    public Sprite unknownSprite;                // Optional fallback

    private List<MonsterEntry> allEntries = new List<MonsterEntry>();
    private readonly List<GameObject> activeButtons = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    public void ShowEntry(MonsterEntry entry)
    {
        if (displayUI != null && entry != null)
            displayUI.ShowEntry(entry);
    }

    private void OnEnable()
    {
        PopulateGrid();
    }

    private void OnDisable()
    {
        ClearGrid();
    }

    private void PopulateGrid()
    {
        ClearGrid();

        var book = MonsterBookManager.Instance;
        if (book == null || book.allEntries == null || book.allEntries.Count == 0)
        {
            Debug.LogWarning("[MonsterDatabaseUI] No monster entries found!");
            return;
        }

        allEntries = book.allEntries;

        foreach (var entry in allEntries)
        {
            GameObject buttonObj = Instantiate(monsterButtonPrefab, gridContainer);
            activeButtons.Add(buttonObj);

            // --- assign the entry to this button ---
            MonsterButtonUI buttonUI = buttonObj.GetComponent<MonsterButtonUI>();
            if (buttonUI != null)
                buttonUI.Setup(entry);

            // Find the icon image (child named "Icon" in prefab)
            Image iconImage = buttonObj.transform.Find("Icon").GetComponent<Image>();

            if (entry.image != null)
                iconImage.sprite = entry.image;
            else if (unknownSprite != null)
                iconImage.sprite = unknownSprite;

            iconImage.color = Color.white;

            // Assign click listener
            Button btn = buttonObj.GetComponent<Button>();
            btn.onClick.AddListener(buttonUI.OnClick); // uses MonsterButtonUI’s method
        }


        // Auto-select first entry
        if (allEntries.Count > 0 && displayUI != null)
            displayUI.ShowEntry(allEntries[0]);
    }

    private void ClearGrid()
    {
        for (int i = 0; i < activeButtons.Count; i++)
        {
            if (activeButtons[i] != null)
                Destroy(activeButtons[i]);
        }
        activeButtons.Clear();
    }

    private void OnMonsterSelected(MonsterEntry entry)
    {
        if (displayUI != null && entry != null)
            displayUI.ShowEntry(entry);
    }
}
