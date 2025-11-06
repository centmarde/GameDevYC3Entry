using UnityEngine;

/// <summary>
/// Component for destructible containers (barrels, crates) that reveal pickables when destroyed.
/// Can be destroyed by player attacks or other damage sources.
/// </summary>
public class DestructibleContainer : MonoBehaviour, IDamageable
{
    [Header("Container Health")]
    [Tooltip("Health of the container")]
    [SerializeField] private float maxHealth = 10f;
    
    private float currentHealth;

    [Header("Visual Feedback")]
    [Tooltip("Particle effect when container is destroyed (optional)")]
    [SerializeField] private GameObject destructionEffectPrefab;
    
    [Tooltip("Sound to play when destroyed (optional)")]
    [SerializeField] private AudioClip destructionSound;
    
    [Tooltip("Mesh to hide when destroyed (will destroy entire GameObject if not set)")]
    [SerializeField] private GameObject meshObject;

    [Header("Physics")]
    [Tooltip("Apply explosion force to nearby rigidbodies")]
    [SerializeField] private bool applyExplosionForce = false;
    
    [Tooltip("Explosion force strength")]
    [SerializeField] private float explosionForce = 300f;
    
    [Tooltip("Explosion radius")]
    [SerializeField] private float explosionRadius = 5f;

    // Event called when container is destroyed
    public event System.Action OnContainerDestroyed;

    private bool isDestroyed = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public bool TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal, object source)
    {
        if (isDestroyed) return false;

        currentHealth -= damage;

        //Debug.Log($"[DestructibleContainer] {gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            DestroyContainer(hitPoint);
            return true;
        }

        return true;
    }

    private void DestroyContainer(Vector3 hitPoint)
    {
        if (isDestroyed) return;
        
        isDestroyed = true;

        //Debug.Log($"[DestructibleContainer] {gameObject.name} destroyed!");

        // Play destruction effects
        PlayDestructionEffects(hitPoint);

        // Apply explosion force if enabled
        if (applyExplosionForce)
        {
            ApplyExplosion(hitPoint);
        }

        // Notify listeners (PickableSpawner)
        OnContainerDestroyed?.Invoke();

        // Destroy the container
        if (meshObject != null)
        {
            // Hide mesh but keep GameObject for a moment (for effects)
            meshObject.SetActive(false);
            Destroy(gameObject, 0.5f);
        }
        else
        {
            // Destroy entire GameObject
            Destroy(gameObject);
        }
    }

    private void PlayDestructionEffects(Vector3 position)
    {
        // Spawn particle effect
        if (destructionEffectPrefab != null)
        {
            GameObject effect = Instantiate(destructionEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        // Play sound
        if (destructionSound != null)
        {
            AudioSource.PlayClipAtPoint(destructionSound, position);
        }
    }

    private void ApplyExplosion(Vector3 explosionPosition)
    {
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
        
        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
            }
        }
    }

    /// <summary>
    /// Destroy the container immediately (for testing or scripted events)
    /// </summary>
    public void DestroyImmediate()
    {
        DestroyContainer(transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        if (applyExplosionForce)
        {
            // Draw explosion radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }

    // IDamageable interface (some systems might need this)
    public bool IsAlive => !isDestroyed;
}
