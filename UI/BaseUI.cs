using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BaseUI : MonoBehaviour
{
    private void OnEnable()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.RegisterUI(gameObject);
    }

    private void OnDisable()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UnregisterUI(gameObject);
    }
}
