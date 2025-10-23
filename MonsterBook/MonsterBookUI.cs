using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterBookUI : MonoBehaviour
{
    public static MonsterBookUI Instance;

    [Header("UI Elements")]
    public GameObject rootPanel;       // The main panel that holds the book visuals
    public Image entryImage;           // The creature image
    public TMP_Text entryName;         // Creature name
    public TMP_Text description;       // Description or lore
    public TMP_Text region;            // Region text
    public TMP_Text location;          // Location text
    public TMP_Text category;          // Category text (Cultural / Creature / Artifact)
    public Button closeButton;         // Button to close the Codex

    private bool isOpen = false;
    private Player player;             // cached player reference

    private void Awake()
    {
        Instance = this;
        rootPanel.SetActive(false);
    }

    private void Start()
    {
        // Cache the player once
        player = FindFirstObjectByType<Player>();

        // Hook up the close button
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    public void Show(MonsterEntry entry)
    {
        if (isOpen) return;
        isOpen = true;

        rootPanel.SetActive(true);

        // Fill in all the info from the entry
        if (entryImage != null) entryImage.sprite = entry.image;
        if (entryName != null) entryName.text = entry.entryName;
        if (description != null) description.text = entry.description;
        if (region != null) region.text = $"Region: {entry.region}";
        if (location != null) location.text = $"Location: {entry.location}";

        // Pause the game
        Time.timeScale = 0f;

        // Lock player input while popup is open
        //if (player != null)
        //    player.SetInputLock(true);

        Debug.Log($"[CodexBookUI] Showing entry: {entry.entryName}");
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        rootPanel.SetActive(false);

        // Resume game
        Time.timeScale = 1f;

        // Unlock player input
        //if (player != null)
        //    player.SetInputLock(false);

        Debug.Log("[CodexBookUI] Closed popup and resumed game.");
    }
}
