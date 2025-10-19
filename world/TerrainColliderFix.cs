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
    }
}
