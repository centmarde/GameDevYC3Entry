using UnityEngine;
using UnityEngine.UI;

public class MonsterButtonUI : MonoBehaviour
{
    private MonsterEntry assignedEntry;

    // assign the entry when the button is created
    public void Setup(MonsterEntry entry)
    {
        assignedEntry = entry;
    }

    // called by the button’s OnClick
    public void OnClick()
    {
        if (assignedEntry == null)
        {
            Debug.LogWarning("[MonsterButtonUI] No entry assigned to this button!");
            return;
        }

        // show the selected monster in the right-side panel
        MonsterDatabaseUI.Instance.ShowEntry(assignedEntry);
    }
}
