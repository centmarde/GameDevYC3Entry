using UnityEngine;

public class TerrainColliderFix : MonoBehaviour
{
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
                Debug.Log("Removed MeshCollider from terrain - MeshCollider is not supported on terrain objects.");
            }
        }
        
        // Ensure we have a TerrainCollider
        TerrainCollider terrainCollider = GetComponent<TerrainCollider>();
        if (terrainCollider == null)
        {
            terrainCollider = gameObject.AddComponent<TerrainCollider>();
            Debug.Log("Added TerrainCollider to terrain object.");
        }
        
        // Refresh the TerrainCollider (disable then enable)
        terrainCollider.enabled = false;
        terrainCollider.enabled = true;
    }
}
