using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MonsterArchiveUI : MonoBehaviour
{
    public static MonsterArchiveUI Instance;

    [Header("UI References")]
    public GameObject rootPanel;
    public Transform entryContainer;
    public GameObject entryButtonPrefab;
    public Image entryImage;           // The creature image
    public TMP_Text detailName;
    public TMP_Text detailDescription;
    public Button closeButton;

    public void RefreshList()
    {
        // For now, leave it empty or add a debug line
        Debug.Log("[MonsterArchiveUI] Refreshing monster list...");
    }

    public void Open() { }
    
    public void Close() { }
}
