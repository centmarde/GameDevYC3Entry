using UnityEngine;

/// <summary>
/// Handles spawn position calculations for different spawn modes
/// </summary>
[System.Serializable]
public class SpawnPositionCalculator
{
    [Header("Spawn Mode")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.CircularAroundPlayer;
    
    [Header("Manual Spawn Points (Manual Mode Only)")]
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("Circular Spawn Settings (Circular Mode Only)")]
    [SerializeField] private float spawnDistance = 15f;
    [SerializeField] private int spawnDirections = 8;
    [SerializeField] private bool useRandomOffset = true;
    [SerializeField] private float randomOffsetRange = 3f;
    
    public enum SpawnMode
    {
        Manual,
        CircularAroundPlayer
    }
    
    /// <summary>
    /// Calculate spawn position and rotation based on current mode
    /// </summary>
    public void GetSpawnTransform(Transform playerTransform, out Vector3 position, out Quaternion rotation)
    {
        if (spawnMode == SpawnMode.Manual)
        {
            GetManualSpawnTransform(out position, out rotation);
        }
        else // CircularAroundPlayer
        {
            GetCircularSpawnTransform(playerTransform, out position, out rotation);
        }
    }
    
    /// <summary>
    /// Get spawn transform for manual mode
    /// </summary>
    private void GetManualSpawnTransform(out Vector3 position, out Quaternion rotation)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[SpawnPositionCalculator] Manual mode requires spawn points!");
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return;
        }
        
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomSpawnIndex];
        position = spawnPoint.position;
        rotation = spawnPoint.rotation;
    }
    
    /// <summary>
    /// Get spawn transform for circular mode around player
    /// </summary>
    private void GetCircularSpawnTransform(Transform playerTransform, out Vector3 position, out Quaternion rotation)
    {
        if (playerTransform == null)
        {
            Debug.LogError("[SpawnPositionCalculator] Circular mode requires player transform!");
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return;
        }
        
        position = CalculateCircularSpawnPosition(playerTransform.position);
        
        // Make enemy face towards player's current position
        Vector3 directionToPlayer = playerTransform.position - position;
        if (directionToPlayer != Vector3.zero)
        {
            rotation = Quaternion.LookRotation(directionToPlayer);
        }
        else
        {
            rotation = Quaternion.identity;
        }
    }
    
    /// <summary>
    /// Calculate a spawn position around the player in a circular pattern
    /// </summary>
    private Vector3 CalculateCircularSpawnPosition(Vector3 playerPosition)
    {
        // Pick a random direction (one of the cardinal/diagonal directions)
        int directionIndex = Random.Range(0, spawnDirections);
        float angle = (360f / spawnDirections) * directionIndex;
        
        // Convert angle to radians
        float angleRad = angle * Mathf.Deg2Rad;
        
        // Calculate base position
        Vector3 offset = new Vector3(
            Mathf.Cos(angleRad) * spawnDistance,
            0f,
            Mathf.Sin(angleRad) * spawnDistance
        );
        
        // Add random offset for variation
        if (useRandomOffset)
        {
            offset += new Vector3(
                Random.Range(-randomOffsetRange, randomOffsetRange),
                0f,
                Random.Range(-randomOffsetRange, randomOffsetRange)
            );
        }
        
        return playerPosition + offset;
    }
    
    /// <summary>
    /// Validate spawn mode requirements
    /// </summary>
    public bool ValidateSpawnMode(Transform playerTransform)
    {
        if (spawnMode == SpawnMode.Manual)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[SpawnPositionCalculator] Manual mode requires spawn points!");
                return false;
            }
        }
        else if (spawnMode == SpawnMode.CircularAroundPlayer)
        {
            if (playerTransform == null)
            {
                Debug.LogError("[SpawnPositionCalculator] Circular mode requires player transform!");
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Draw gizmos for spawn visualization
    /// </summary>
    public void DrawSpawnGizmos(Transform playerTransform, Transform fallbackTransform)
    {
        if (spawnMode == SpawnMode.CircularAroundPlayer)
        {
            Transform target = playerTransform ?? fallbackTransform;
            
            if (target != null)
            {
                // Draw spawn circle
                Gizmos.color = Color.yellow;
                DrawCircle(target.position, spawnDistance, 32);
                
                // Draw spawn direction indicators
                Gizmos.color = Color.red;
                for (int i = 0; i < spawnDirections; i++)
                {
                    float angle = (360f / spawnDirections) * i * Mathf.Deg2Rad;
                    Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                    Vector3 spawnPos = target.position + direction * spawnDistance;
                    
                    Gizmos.DrawWireSphere(spawnPos, 1f);
                    Gizmos.DrawLine(target.position, spawnPos);
                }
                
                // Draw random offset range
                if (useRandomOffset)
                {
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                    DrawCircle(target.position, spawnDistance + randomOffsetRange, 32);
                    DrawCircle(target.position, spawnDistance - randomOffsetRange, 32);
                }
            }
        }
        else if (spawnMode == SpawnMode.Manual && spawnPoints != null)
        {
            // Draw manual spawn points
            Gizmos.color = Color.green;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 1f);
                }
            }
        }
    }
    
    /// <summary>
    /// Helper method to draw a circle
    /// </summary>
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
    
    // Getters for inspector access
    public SpawnMode GetSpawnMode() => spawnMode;
    public Transform[] GetSpawnPoints() => spawnPoints;
    public float GetSpawnDistance() => spawnDistance;
    public int GetSpawnDirections() => spawnDirections;
    public bool GetUseRandomOffset() => useRandomOffset;
    public float GetRandomOffsetRange() => randomOffsetRange;
}