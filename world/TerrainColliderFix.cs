using UnityEngine;

public class TerrainColliderFix : MonoBehaviour
{
    [Header("FX Settings")]
    [SerializeField] private GameObject fxPrefab;
    [SerializeField] private Transform fxSpawnPoint;
    [SerializeField] private bool spawnFXOnAwake = false;
    
    private GameObject spawnedFX;
    
    void Awake()
    {
        // Get all colliders on the terrain
        Collider[] colliders = GetComponents<Collider>();
        
        // Remove any MeshColliders (not supported on terrain)
        foreach (Collider col in colliders)
        {
            if (col is MeshCollider)
            {
                DestroyImmediate(col);
            }
        }
        
        // Ensure we have a TerrainCollider
        TerrainCollider terrainCollider = GetComponent<TerrainCollider>();
        if (terrainCollider == null)
        {
            terrainCollider = gameObject.AddComponent<TerrainCollider>();
        }
        
        // Refresh the TerrainCollider (disable then enable)
        terrainCollider.enabled = false;
        terrainCollider.enabled = true;
        
        // Spawn FX if enabled
        if (spawnFXOnAwake && fxPrefab != null)
        {
            SpawnFX();
        }
    }
    
    /// <summary>
    /// Spawns the FX prefab at the specified spawn point or at this object's position
    /// </summary>
    public void SpawnFX()
    {
        if (fxPrefab == null)
        {
            Debug.LogWarning("FX Prefab is not assigned in TerrainColliderFix!");
            return;
        }
        
        // Destroy existing FX if any
        if (spawnedFX != null)
        {
            Destroy(spawnedFX);
        }
        
        // Determine spawn position and rotation
        Vector3 spawnPosition = fxSpawnPoint != null ? fxSpawnPoint.position : transform.position;
        Quaternion spawnRotation = fxSpawnPoint != null ? fxSpawnPoint.rotation : transform.rotation;
        
        // Instantiate the FX prefab
        spawnedFX = Instantiate(fxPrefab, spawnPosition, spawnRotation);
        
        // Optionally parent to this object or the spawn point
        if (fxSpawnPoint != null)
        {
            spawnedFX.transform.SetParent(fxSpawnPoint);
        }
        else
        {
            spawnedFX.transform.SetParent(transform);
        }
    }
    
    /// <summary>
    /// Destroys the currently spawned FX
    /// </summary>
    public void DestroyFX()
    {
        if (spawnedFX != null)
        {
            Destroy(spawnedFX);
            spawnedFX = null;
        }
    }
    
    void OnDestroy()
    {
        // Clean up spawned FX when this object is destroyed
        if (spawnedFX != null)
        {
            Destroy(spawnedFX);
        }
    }
}
