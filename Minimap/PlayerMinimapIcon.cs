using UnityEngine;

public class PlayerMinimapIcon : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private bool rotateWithPlayer = true;

    private RectTransform iconRect;

    private void Awake()
    {
        iconRect = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            Player foundPlayer = FindFirstObjectByType<Player>();
            if (foundPlayer != null)
                player = foundPlayer.transform;
            else
                return;
        }

        if (rotateWithPlayer)
        {
            // Rotate only the icon — not the minimap itself
            iconRect.localRotation = Quaternion.Euler(0f, 0f, -player.eulerAngles.y);
        }
    }
}
