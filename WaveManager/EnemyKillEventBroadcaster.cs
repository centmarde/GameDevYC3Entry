using UnityEngine;
using System;

/// <summary>
/// Broadcasts enemy kill events to any listening systems (like Vampire Aura skill)
/// This should be a singleton in the scene that receives notifications from EnemyDeathTracker
/// </summary>
public class EnemyKillEventBroadcaster : MonoBehaviour
{
    public static EnemyKillEventBroadcaster Instance { get; private set; }
    
    // Event that fires when an enemy is killed
    public event Action<Vector3> OnEnemyKilled;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[EnemyKillEventBroadcaster] Multiple instances detected! Destroying duplicate.");
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Call this method when an enemy is killed
    /// </summary>
    /// <param name="enemyPosition">Position where the enemy died</param>
    public void BroadcastEnemyKill(Vector3 enemyPosition)
    {
        OnEnemyKilled?.Invoke(enemyPosition);
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
