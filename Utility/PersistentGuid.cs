using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PersistentGuid : MonoBehaviour
{
    [SerializeField] private string guid;
    public string Value => guid;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(guid))
        {
            guid = System.Guid.NewGuid().ToString("N");
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}
