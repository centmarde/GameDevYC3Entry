using UnityEngine;

public class ProjectileSlingshot : MonoBehaviour
{
    [SerializeField] private float lifeTime = 1f;
    [SerializeField] private LayerMask hitMask;

    private Rigidbody rb;
    private float damage;
    private bool isCriticalHit;
    private object source;
    
    /// <summary>
    /// Set the hit mask for this projectile (used when creating projectiles at runtime)
    /// </summary>
    public void SetHitMask(LayerMask mask)
    {
        hitMask = mask;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        else
        {
            Debug.LogError($"{name}: ProjectileSlingshot requires a Rigidbody component!");
        }

        // make sure collider is trigger
        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError($"{name}: ProjectileSlingshot requires a Collider component!");
        }
    }

    public void Launch(Vector3 velocity, float dmg, object src, bool isCritical = false)
    {
        damage = dmg;
        source = src;
        isCriticalHit = isCritical;
        
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
        else
        {
            Debug.LogError($"{name}: Cannot launch projectile - Rigidbody is null!");
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        
        // only react to layers we care about
        if ((hitMask.value & (1 << other.gameObject.layer)) == 0) return;

        var target = other.GetComponentInParent<IDamageable>();
        if (target != null && target.IsAlive)
        {
            Vector3 hitPoint = transform.position;
            Vector3 hitNormal = rb != null ? -rb.linearVelocity.normalized : Vector3.back;
            
            // Apply damage falloff if this is a scatter pellet
            float finalDamage = damage;
            var damageFalloff = GetComponent<ScatterPelletDamageFalloff>();
            if (damageFalloff != null)
            {
                float multiplier = damageFalloff.GetDamageMultiplier();
                finalDamage = damage * multiplier;
            }
            
            // CRITICAL: This applies damage to the enemy!
            bool damageApplied = target.TakeDamage(finalDamage, hitPoint, hitNormal, source);
            
            if (!damageApplied)
            {
                Debug.LogWarning($"{name}: Damage was not applied to {other.name} (invulnerable or dead)");
            }
            else
            {
                // Show damage number UI
                DamageNumberUI.ShowDamage(finalDamage, hitPoint, isCriticalHit);
                
                // Broadcast damage dealt event for skills like Vampire Aura
                DamageEventBroadcaster.BroadcastPlayerDamage(finalDamage, hitPoint, source);
            }
        }

        Destroy(gameObject);
    }
    
    public bool IsCriticalHit() => isCriticalHit;
    
    /// <summary>
    /// Get the current damage value of the projectile
    /// </summary>
    public float GetDamage() => damage;
    
    /// <summary>
    /// Set the damage value of the projectile (used for distance-based falloff)
    /// </summary>
    public void SetDamage(float newDamage) => damage = newDamage;
}
