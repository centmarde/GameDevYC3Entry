using UnityEngine;

public class ProjectileSlingshot : MonoBehaviour
{
    [SerializeField] private float lifeTime = 1f;
    [SerializeField] private LayerMask hitMask;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip launchSound;
    [SerializeField] [Range(0f, 1f)] private float launchSoundVolume = 0.7f;
    private static AudioSource audioSource;

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
        
        // Play launch sound effect
        PlayLaunchSound();
        
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

    /// <summary>
    /// Plays the launch sound effect once when projectile is released
    /// </summary>
    private void PlayLaunchSound()
    {
        if (launchSound == null) return;

        // Create shared AudioSource if it doesn't exist
        if (audioSource == null)
        {
            GameObject audioObj = new GameObject("ProjectileAudioSource");
            audioSource = audioObj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
            DontDestroyOnLoad(audioObj);
        }

        audioSource.PlayOneShot(launchSound, launchSoundVolume);
    }
}
