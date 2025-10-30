using UnityEngine;

public class MinimapFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target; // Player
    [SerializeField] private Vector3 offset = new Vector3(0, 50, 0);
    [SerializeField] private bool rotateWithPlayer = true;

    private bool wasFogEnabled;

    private void OnPreRender()
    {
        wasFogEnabled = RenderSettings.fog;
        RenderSettings.fog = false;
    }

    private void OnPostRender()
    {
        RenderSettings.fog = wasFogEnabled;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            // Try to find player dynamically when it spawns
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
                target = player.transform;
            else
                return;
        }

        // Follow player position
        transform.position = target.position + offset;

        // Optionally rotate to match player heading
        if (rotateWithPlayer)
            transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
        else
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
