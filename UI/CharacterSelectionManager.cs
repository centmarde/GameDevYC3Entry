using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages character selection and stores the selected character class.
/// This is a singleton that persists across scenes.
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    private static CharacterSelectionManager instance;
    public static CharacterSelectionManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("CharacterSelectionManager");
                instance = go.AddComponent<CharacterSelectionManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Character Selection")]
    [SerializeField] private int selectedCharacterIndex = 0; // 0 = Player1, 1 = Player2
    
    public int SelectedCharacterIndex => selectedCharacterIndex;
    
    private const string SELECTED_CHARACTER_KEY = "SelectedCharacter";

    private void Awake()
    {
        // Implement singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSelectedCharacter();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets the selected character and saves it to PlayerPrefs
    /// </summary>
    /// <param name="characterIndex">0 for Player1, 1 for Player2</param>
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex > 1)
        {
            Debug.LogWarning($"[CharacterSelectionManager] Invalid character index: {characterIndex}. Must be 0 or 1.");
            return;
        }

        selectedCharacterIndex = characterIndex;
        SaveSelectedCharacter();
        
        string characterName = characterIndex == 0 ? "Player1" : "Player2";
        Debug.Log($"[CharacterSelectionManager] Selected character: {characterName} (Index: {characterIndex})");
    }

    /// <summary>
    /// Loads the MainBase scene after character selection
    /// </summary>
    public void LoadMainBaseScene()
    {
        Debug.Log($"[CharacterSelectionManager] Loading MainBase scene with character index: {selectedCharacterIndex}");
        SceneManager.LoadScene("MainBase");
    }

    /// <summary>
    /// Saves the selected character to PlayerPrefs
    /// </summary>
    private void SaveSelectedCharacter()
    {
        PlayerPrefs.SetInt(SELECTED_CHARACTER_KEY, selectedCharacterIndex);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the selected character from PlayerPrefs
    /// </summary>
    private void LoadSelectedCharacter()
    {
        selectedCharacterIndex = PlayerPrefs.GetInt(SELECTED_CHARACTER_KEY, 0);
        Debug.Log($"[CharacterSelectionManager] Loaded character index: {selectedCharacterIndex}");
    }

    /// <summary>
    /// Resets the character selection (useful for debugging)
    /// </summary>
    public void ResetSelection()
    {
        selectedCharacterIndex = 0;
        SaveSelectedCharacter();
        Debug.Log("[CharacterSelectionManager] Character selection reset to Player1");
    }
}
