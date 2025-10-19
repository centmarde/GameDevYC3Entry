using UnityEngine;
using UnityEngine.SceneManagement;

public class IsoCameraFollow : MonoBehaviour
{
    private Transform target;
    private Camera mainCam;
    private float smoothSpeed = 5f;

    [Header("Zoom Settings")]
    [SerializeField] private float defaultOrthoSize = 6f;
    [SerializeField] private float zoomOutMax = 9f;
    [SerializeField] private float zoomLerpSpeed = 5f;

    private float targetOrthoSize;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        mainCam = GetComponentInChildren<Camera>();
        if (mainCam != null)
            mainCam.orthographicSize = defaultOrthoSize;

        targetOrthoSize = defaultOrthoSize;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
            target = player.transform;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Follow player smoothly
        Vector3 desiredPos = target.position;
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPos;

        // Apply zoom smoothly
        if (mainCam != null)
        {
            mainCam.orthographicSize = Mathf.Lerp(
                mainCam.orthographicSize,
                targetOrthoSize,
                zoomLerpSpeed * Time.deltaTime
            );
        }
    }


    public void SetZoom(float normalizedZoom)
    {
        float eased = 1f - Mathf.Pow(1f - normalizedZoom, 2f);
        targetOrthoSize = Mathf.Lerp(defaultOrthoSize, zoomOutMax, eased);
    }

    public void ResetZoom()
    {
        targetOrthoSize = defaultOrthoSize;
    }
}
