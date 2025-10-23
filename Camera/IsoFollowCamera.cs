using UnityEngine;
using UnityEngine.SceneManagement;

public class IsoCameraFollow : MonoBehaviour
{
    private Transform[] targets;
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
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        if (players != null && players.Length > 0)
        {
            targets = new Transform[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                targets[i] = players[i].transform;
            }
        }
    }

    private void LateUpdate()
    {
        CleanupNullTargets();

        if (targets == null || targets.Length == 0) return;

        // Calculate center point of all targets
        Vector3 centerPoint = GetCenterPoint();
        
        // Follow center point smoothly
        Vector3 desiredPos = centerPoint;
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

    /// <summary>
    /// Calculate the center point of all targets
    /// </summary>
    private Vector3 GetCenterPoint()
    {
        if (targets == null || targets.Length == 0)
            return transform.position;

        if (targets.Length == 1)
            return targets[0].position;

        // Calculate average position
        Vector3 center = Vector3.zero;
        int validTargets = 0;

        foreach (Transform target in targets)
        {
            if (target != null)
            {
                center += target.position;
                validTargets++;
            }
        }

        if (validTargets > 0)
            center /= validTargets;

        return center;
    }

    /// <summary>
    /// Manually set camera targets
    /// </summary>
    public void SetTargets(Transform[] newTargets)
    {
        targets = newTargets;
    }

    /// <summary>
    /// Add a target to the camera
    /// </summary>
    public void AddTarget(Transform newTarget)
    {
        if (newTarget == null) return;

        if (targets == null || targets.Length == 0)
        {
            targets = new Transform[] { newTarget };
        }
        else
        {
            Transform[] newArray = new Transform[targets.Length + 1];
            targets.CopyTo(newArray, 0);
            newArray[targets.Length] = newTarget;
            targets = newArray;
        }
    }

    /// <summary>
    /// Remove a target from the camera
    /// </summary>
    public void RemoveTarget(Transform targetToRemove)
    {
        if (targets == null || targets.Length == 0) return;

        int newLength = 0;
        foreach (Transform t in targets)
        {
            if (t != targetToRemove) newLength++;
        }

        Transform[] newArray = new Transform[newLength];
        int index = 0;
        foreach (Transform t in targets)
        {
            if (t != targetToRemove)
            {
                newArray[index] = t;
                index++;
            }
        }

        targets = newArray;
    }

    /// <summary>
    /// Remove any destroyed or missing targets automatically
    /// </summary>
    private void CleanupNullTargets()
    {
        if (targets == null || targets.Length == 0) return;

        int validCount = 0;
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
                validCount++;
        }

        if (validCount != targets.Length)
        {
            Transform[] newArray = new Transform[validCount];
            int index = 0;
            foreach (var t in targets)
            {
                if (t != null)
                {
                    newArray[index] = t;
                    index++;
                }
            }
            targets = newArray;
        }
    }

}
