using UnityEngine;

public class ProjectileSlingshot : MonoBehaviour
{
    [SerializeField] private float lifeTime = 1f;
    [SerializeField] private LayerMask hitMask;

    private Rigidbody rb;
    private float damage;
    private bool isCriticalHit;
    private object source;

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
            
            // CRITICAL: This applies damage to the enemy!
            bool damageApplied = target.TakeDamage(damage, hitPoint, hitNormal, source);
            
            if (!damageApplied)
            {
                Debug.LogWarning($"{name}: Damage was not applied to {other.name} (invulnerable or dead)");
            }
            else if (isCriticalHit)
            {
                // Show critical hit indicator
                CriticalHitIndicator.ShowCritical(hitPoint);
            }
        }

        Destroy(gameObject);
    }
    
    public bool IsCriticalHit() => isCriticalHit;
}
