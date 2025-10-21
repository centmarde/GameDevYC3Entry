using UnityEngine;

/// <summary>
/// Debug utility for testing character selection system.
/// Add this to a GameObject and use the context menu options for testing.
/// </summary>
public class CharacterSelectionDebugger : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Select Player1")]
    public void DebugSelectPlayer1()
    {
        CharacterSelectionManager.Instance.SelectCharacter(0);
        Debug.Log("[CharacterSelectionDebugger] Forced selection to Player1");
    }

    [ContextMenu("Select Player2")]
    public void DebugSelectPlayer2()
    {
        CharacterSelectionManager.Instance.SelectCharacter(1);
        Debug.Log("[CharacterSelectionDebugger] Forced selection to Player2");
    }

    [ContextMenu("Reset Selection")]
    public void DebugResetSelection()
    {
        CharacterSelectionManager.Instance.ResetSelection();
        Debug.Log("[CharacterSelectionDebugger] Selection reset to default (Player1)");
    }

    [ContextMenu("Show Current Selection")]
    public void DebugShowSelection()
    {
        int current = CharacterSelectionManager.Instance.SelectedCharacterIndex;
        string characterName = current == 0 ? "Player1 (Warrior)" : "Player2 (Assassin)";
        Debug.Log($"[CharacterSelectionDebugger] Current selection: {characterName} (Index: {current})");
    }

    [ContextMenu("Clear All PlayerPrefs")]
    public void DebugClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[CharacterSelectionDebugger] All PlayerPrefs cleared!");
    }

    [ContextMenu("Load MainBase Scene")]
    public void DebugLoadMainBase()
    {
        CharacterSelectionManager.Instance.LoadMainBaseScene();
        Debug.Log("[CharacterSelectionDebugger] Loading MainBase scene...");
    }
#endif

    // Runtime debug UI (displayed in game)
    [Header("Debug UI Settings")]
    [SerializeField] private bool showDebugUI = false;
    [SerializeField] private KeyCode toggleDebugKey = KeyCode.F1;

    private void Update()
    {
        if (Input.GetKeyDown(toggleDebugKey))
        {
            showDebugUI = !showDebugUI;
        }
    }

    private void OnGUI()
    {
        if (!showDebugUI) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Box("Character Selection Debugger");

        GUILayout.Label($"Current Selection: {CharacterSelectionManager.Instance.SelectedCharacterIndex}");
        GUILayout.Label($"Character: {(CharacterSelectionManager.Instance.SelectedCharacterIndex == 0 ? "Player1" : "Player2")}");

        GUILayout.Space(10);

        if (GUILayout.Button("Select Player1"))
        {
            CharacterSelectionManager.Instance.SelectCharacter(0);
        }

        if (GUILayout.Button("Select Player2"))
        {
            CharacterSelectionManager.Instance.SelectCharacter(1);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Reset Selection"))
        {
            CharacterSelectionManager.Instance.ResetSelection();
        }

        if (GUILayout.Button("Load MainBase Scene"))
        {
            CharacterSelectionManager.Instance.LoadMainBaseScene();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Clear PlayerPrefs"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs cleared");
        }

        GUILayout.Space(10);
        GUILayout.Label($"Press {toggleDebugKey} to toggle this UI");

        GUILayout.EndArea();
    }
}
