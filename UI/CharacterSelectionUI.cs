using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the UI for character selection in the ChooseMenu scene.
/// Manages button clicks and visual feedback for character cards.
/// </summary>
public class CharacterSelectionUI : MonoBehaviour
{
    [Header("Character Card Buttons")]
    [SerializeField] private Button player1CardButton;
    [SerializeField] private Button player2CardButton;

    [Header("Card Highlights (Optional)")]
    [SerializeField] private GameObject player1Highlight;
    [SerializeField] private GameObject player2Highlight;

    [Header("Character Names (Optional)")]
    [SerializeField] private TextMeshProUGUI player1NameText;
    [SerializeField] private TextMeshProUGUI player2NameText;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip characterSelectSound;
    private AudioSource audioSource;

    private int selectedCharacter = -1; // -1 = none, 0 = Player1, 1 = Player2

    private void Awake()
    {
        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // Setup button listeners
        if (player1CardButton != null)
        {
            player1CardButton.onClick.AddListener(() => OnCharacterCardClicked(0));
        }
        else
        {
            Debug.LogError("[CharacterSelectionUI] Player1 Card Button is not assigned!");
        }

        if (player2CardButton != null)
        {
            player2CardButton.onClick.AddListener(() => OnCharacterCardClicked(1));
        }
        else
        {
            Debug.LogError("[CharacterSelectionUI] Player2 Card Button is not assigned!");
        }

        // Set character names if texts are assigned
        if (player1NameText != null)
        {
            player1NameText.text = "Warrior";
        }

        if (player2NameText != null)
        {
            player2NameText.text = "Assassin";
        }

        // Initialize highlights (hide all at start)
        UpdateHighlights();
    }

    /// <summary>
    /// Called when a character card button is clicked
    /// </summary>
    /// <param name="characterIndex">0 for Player1, 1 for Player2</param>
    private void OnCharacterCardClicked(int characterIndex)
    {
        // Play button click sound
        PlaySound(buttonClickSound);

        // Update selected character
        selectedCharacter = characterIndex;
        UpdateHighlights();

        string characterName = characterIndex == 0 ? "Warrior (Player1)" : "Assassin (Player2)";
        Debug.Log($"[CharacterSelectionUI] Selected: {characterName}");

        // Save selection and load next scene
        CharacterSelectionManager.Instance.SelectCharacter(characterIndex);
        
        // Play character select sound
        PlaySound(characterSelectSound);

        // Load MainBase scene after a short delay
        Invoke(nameof(LoadMainBase), 0.5f);
    }

    /// <summary>
    /// Updates the visual highlights for selected character
    /// </summary>
    private void UpdateHighlights()
    {
        if (player1Highlight != null)
        {
            player1Highlight.SetActive(selectedCharacter == 0);
        }

        if (player2Highlight != null)
        {
            player2Highlight.SetActive(selectedCharacter == 1);
        }
    }

    /// <summary>
    /// Plays an audio clip if available
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Loads the MainBase scene
    /// </summary>
    private void LoadMainBase()
    {
        CharacterSelectionManager.Instance.LoadMainBaseScene();
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (player1CardButton != null)
        {
            player1CardButton.onClick.RemoveAllListeners();
        }

        if (player2CardButton != null)
        {
            player2CardButton.onClick.RemoveAllListeners();
        }
    }
}
