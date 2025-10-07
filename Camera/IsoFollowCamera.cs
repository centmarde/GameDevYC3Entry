using UnityEngine;
using UnityEngine.SceneManagement;

public class IsoFollowCamera : MonoBehaviour
{
    private Transform target;
    private float smoothSpeed = 5f;

    private void Awake()
    {

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
    }
}
