using UnityEngine;

/// <summary>
/// A projectile that orbits around the player like a windmill and damages enemies on contact.
/// </summary>
public class CirclingProjectile : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float rotationSpeed = 100f; // Visual rotation of the projectile itself
    
    private Transform playerTransform;
    private float orbitRadius;
    private float orbitSpeed;
    private float currentAngle;
    private float damage;
    private object source;
    private LayerMask enemyLayer;
    
    private bool isInitialized = false;

    /// <summary>
    /// Initialize the circling projectile with its parameters
    /// </summary>
    public void Initialize(Transform player, float radius, float speed, float angleOffset, float dmg, object src, LayerMask enemyMask)
    {
        playerTransform = player;
        orbitRadius = radius;
        orbitSpeed = speed;
        currentAngle = angleOffset;
        damage = dmg;
        source = src;
        enemyLayer = enemyMask;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || playerTransform == null)
        {
            return;
        }

        // Update orbit angle
        currentAngle += orbitSpeed * Time.deltaTime;
        
        // Keep angle in 0-360 range
        if (currentAngle >= 360f)
        {
            currentAngle -= 360f;
        }

        // Calculate position on the circle around the player
        float radians = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Cos(radians) * orbitRadius,
            0f, // Keep at player's Y level
            Mathf.Sin(radians) * orbitRadius
        );

        // Set position relative to player
        transform.position = playerTransform.position + offset;

        // Optional: Make the projectile face the direction of movement
        Vector3 tangentDirection = new Vector3(-Mathf.Sin(radians), 0f, Mathf.Cos(radians));
        if (tangentDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(tangentDirection);
        }

        // Add visual rotation (spinning effect)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized || other == null) return;
        
        // Check if the collided object is on the enemy layer
        if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

        // Try to get IDamageable component from the collided object or its parent
        var target = other.GetComponentInParent<IDamageable>();
        if (target != null && target.IsAlive)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitNormal = (other.transform.position - transform.position).normalized;
            
            // Apply damage to the enemy
            target.TakeDamage(damage, hitPoint, hitNormal, source);
        }
    }

    /// <summary>
    /// Update the orbit radius (useful for upgrades)
    /// </summary>
    public void SetOrbitRadius(float newRadius)
    {
        orbitRadius = newRadius;
    }

    /// <summary>
    /// Update the orbit speed
    /// </summary>
    public void SetOrbitSpeed(float newSpeed)
    {
        orbitSpeed = newSpeed;
    }

    /// <summary>
    /// Update the damage value
    /// </summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    private void OnDestroy()
    {
        // Cleanup if needed
    }
}
