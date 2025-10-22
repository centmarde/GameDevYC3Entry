using UnityEngine;

/// <summary>
/// Portal script that defines a spawn/teleport point.
/// Attach this to a portal prefab to mark it as a spawn location.
/// </summary>
public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private string portalName = "Portal";
    [SerializeField] private bool isActiveSpawnPoint = true;
    
    [Header("Spawn Configuration")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    
    [Header("Visual Settings")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private float gizmoRadius = 1f;

    private void Awake()
    {
        // If no spawn point is assigned, use this GameObject's transform
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }

    /// <summary>
    /// Gets the spawn position for this portal
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        return spawnPoint.position + spawnOffset;
    }

    /// <summary>
    /// Gets the spawn rotation for this portal
    /// </summary>
    public Quaternion GetSpawnRotation()
    {
        return spawnPoint.rotation;
    }

    /// <summary>
    /// Gets the spawn transform (position and rotation)
    /// </summary>
    public void GetSpawnTransform(out Vector3 position, out Quaternion rotation)
    {
        position = GetSpawnPosition();
        rotation = GetSpawnRotation();
    }

    /// <summary>
    /// Spawns a GameObject at this portal's spawn point
    /// </summary>
    /// <param name="prefab">The prefab to spawn</param>
    /// <returns>The spawned GameObject instance</returns>
    public GameObject SpawnObject(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError($"[Portal '{portalName}'] Cannot spawn null prefab!");
            return null;
        }

        Vector3 position = GetSpawnPosition();
        Quaternion rotation = GetSpawnRotation();
        
        GameObject spawnedObject = Instantiate(prefab, position, rotation);
        Debug.Log($"[Portal '{portalName}'] Spawned {prefab.name} at {position}");
        
        return spawnedObject;
    }

    /// <summary>
    /// Teleports a GameObject to this portal's spawn point
    /// </summary>
    /// <param name="target">The GameObject to teleport</param>
    public void TeleportObject(GameObject target)
    {
        if (target == null)
        {
            Debug.LogError($"[Portal '{portalName}'] Cannot teleport null object!");
            return;
        }

        Vector3 position = GetSpawnPosition();
        Quaternion rotation = GetSpawnRotation();
        
        target.transform.position = position;
        target.transform.rotation = rotation;
        
        Debug.Log($"[Portal '{portalName}'] Teleported {target.name} to {position}");
    }

    /// <summary>
    /// Teleports a Transform to this portal's spawn point
    /// </summary>
    /// <param name="target">The Transform to teleport</param>
    public void TeleportObject(Transform target)
    {
        if (target == null)
        {
            Debug.LogError($"[Portal '{portalName}'] Cannot teleport null transform!");
            return;
        }

        target.position = GetSpawnPosition();
        target.rotation = GetSpawnRotation();
        
        Debug.Log($"[Portal '{portalName}'] Teleported {target.name} to {target.position}");
    }

    /// <summary>
    /// Checks if this portal is currently active as a spawn point
    /// </summary>
    public bool IsActive()
    {
        return isActiveSpawnPoint && gameObject.activeInHierarchy;
    }

    /// <summary>
    /// Sets whether this portal is active as a spawn point
    /// </summary>
    public void SetActive(bool active)
    {
        isActiveSpawnPoint = active;
    }

    /// <summary>
    /// Gets the portal's name
    /// </summary>
    public string GetPortalName()
    {
        return portalName;
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(portalName))
        {
            portalName = gameObject.name;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Draws gizmos in the editor to visualize the spawn point
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Transform point = spawnPoint != null ? spawnPoint : transform;
        Vector3 position = point.position + spawnOffset;
        
        // Draw sphere at spawn position
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(position, gizmoRadius);
        
        // Draw forward direction indicator
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.8f);
        Gizmos.DrawLine(position, position + point.forward * (gizmoRadius * 2f));
        
        // Draw up direction indicator
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.5f);
        Gizmos.DrawLine(position, position + point.up * gizmoRadius);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        Transform point = spawnPoint != null ? spawnPoint : transform;
        Vector3 position = point.position + spawnOffset;
        
        // Draw more prominent gizmo when selected
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(position, gizmoRadius * 0.2f);
        
        // Draw coordinate axes
        Gizmos.color = Color.red;
        Gizmos.DrawLine(position, position + point.right * gizmoRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(position, position + point.up * gizmoRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(position, position + point.forward * gizmoRadius);
        
        // Draw label
        UnityEditor.Handles.Label(position + Vector3.up * (gizmoRadius + 0.5f), 
            $"Portal: {portalName}\n{(isActiveSpawnPoint ? "Active" : "Inactive")}");
    }
#endif
}
