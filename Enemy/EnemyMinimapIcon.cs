using UnityEngine;

public class EnemyMinimapIcon : MonoBehaviour
{
    [Header("Minimap Settings")]
    public Sprite minimapSprite;

    private MinimapIconManager minimap;
    private bool registered = false;

    private void Start()
    {
        minimap = FindFirstObjectByType<MinimapIconManager>();

        if (minimap != null)
        {
            minimap.RegisterEnemy(transform, minimapSprite);
            registered = true;
        }
    }

    private void OnDestroy()
    {
        if (registered && minimap != null)
        {
            minimap.UnregisterEnemy(transform);
        }
    }
}
