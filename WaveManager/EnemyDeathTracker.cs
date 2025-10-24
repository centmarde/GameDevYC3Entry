using UnityEngine;

/// <summary>
/// Automatically tracks enemy death and notifies WaveManager.
/// This component is automatically added to spawned enemies.
/// Call NotifyDeath() from your enemy death/health script when the enemy dies.
/// </summary>
public class EnemyDeathTracker : MonoBehaviour
{
    private WaveManager waveManager;
    private bool hasNotifiedDeath = false;
    
    private void Awake()
    {
        // Find wave manager in scene
        waveManager = FindObjectOfType<WaveManager>();
    }
    
    /// <summary>
    /// Call this method when the enemy dies.
    /// This should be called from your enemy death/health script.
    /// </summary>
    public void NotifyDeath()
    {
        if (hasNotifiedDeath) return; // Prevent multiple notifications
        
        hasNotifiedDeath = true;
        
        if (waveManager != null)
        {
            waveManager.RegisterEnemyKilled();
        }
        
        // Broadcast kill event for skills like Vampire Aura
        if (EnemyKillEventBroadcaster.Instance != null)
        {
            EnemyKillEventBroadcaster.Instance.BroadcastEnemyKill(transform.position);
        }
    }
    
    /// <summary>
    /// Automatically notify on destroy (fallback)
    /// </summary>
    private void OnDestroy()
    {
        // If enemy is destroyed but death wasn't notified, notify now
        if (!hasNotifiedDeath && waveManager != null)
        {
            NotifyDeath();
        }
    }
    
    /// <summary>
    /// Optional: Automatically detect death by health component
    /// You can customize this based on your enemy health system
    /// </summary>
    private void Update()
    {
        // Example: Check if enemy has a health component that reached 0
        // Uncomment and customize based on your health system:
        
        /*
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null && health.IsDead() && !hasNotifiedDeath)
        {
            NotifyDeath();
        }
        */
    }
}
