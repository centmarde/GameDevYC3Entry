using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapIconManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform minimapRect;   // RawImage RectTransform from MinimapUI
    [SerializeField] private Transform player;            // Player transform (auto-detected if left empty)
    [SerializeField] private Camera minimapCamera;        // Minimap Camera (top-down)
    [SerializeField] private GameObject enemyIconPrefab;  // Prefab for enemy icons

    private readonly List<Transform> enemies = new List<Transform>();
    private readonly List<Image> enemyIcons = new List<Image>();

    private void Start()
    {
        // Auto-detect player if not assigned
        if (player == null)
        {
            Player foundPlayer = FindFirstObjectByType<Player>();
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }
    }

    /// <summary>
    /// Called by EnemyMinimapIcon when an enemy spawns
    /// </summary>
    public void RegisterEnemy(Transform enemyTransform, Sprite iconSprite)
    {
        if (enemyTransform == null || enemyIconPrefab == null)
        {
            Debug.LogWarning("[MinimapIconManager] Missing enemy reference or prefab!");
            return;
        }

        enemies.Add(enemyTransform);
        GameObject iconObj = Instantiate(enemyIconPrefab, transform);
        Image iconImage = iconObj.GetComponent<Image>();
        iconImage.sprite = iconSprite;
        enemyIcons.Add(iconImage);
    }

    /// <summary>
    /// Called by EnemyMinimapIcon when an enemy is destroyed
    /// </summary>
    public void UnregisterEnemy(Transform enemyTransform)
    {
        int index = enemies.IndexOf(enemyTransform);
        if (index >= 0)
        {
            Destroy(enemyIcons[index].gameObject);
            enemyIcons.RemoveAt(index);
            enemies.RemoveAt(index);
        }
    }

    private void Update()
    {
        if (minimapCamera == null || minimapRect == null) return;

        for (int i = 0; i < enemies.Count; i++)
        {
            Transform enemy = enemies[i];
            Image icon = enemyIcons[i];

            if (enemy == null)
            {
                icon.enabled = false;
                continue;
            }

            // Convert enemy world position to viewport (0–1) coordinates relative to minimap camera
            Vector3 viewportPos = minimapCamera.WorldToViewportPoint(enemy.position);

            // Only show icon if visible in minimap camera view
            bool isVisible = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;
            icon.enabled = isVisible;

            if (isVisible)
            {
                // Convert to local minimap space
                Vector3 localPos = new Vector3(
                    (viewportPos.x - 0.5f) * minimapRect.rect.width,
                    (viewportPos.y - 0.5f) * minimapRect.rect.height,
                    0
                );

                // Apply to the icon’s local position
                icon.rectTransform.localPosition = localPos;


            }
        }
    }
}
