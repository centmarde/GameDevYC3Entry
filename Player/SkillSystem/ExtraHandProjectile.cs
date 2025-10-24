using UnityEngine;

/// <summary>
/// Projectile for Extra Hand skill - flies toward target with visual trail effect
/// Uses ProjectileSlingshot for damage and ProjectileTracer for visuals
/// Features snake-like wavy motion
/// </summary>
[RequireComponent(typeof(ProjectileSlingshot))]
[RequireComponent(typeof(ProjectileTracer))]
public class ExtraHandProjectile : MonoBehaviour
{
    [Header("Snake Motion Settings")]
    [SerializeField] private float waveAmplitude = 0.5f; // How far the projectile waves side-to-side
    [SerializeField] private float waveFrequency = 3f; // How fast the wave oscillates
    [SerializeField] private bool enableSnakeMotion = true;
    
    private ProjectileSlingshot slingshot;
    private ProjectileTracer tracer;
    private Rigidbody rb;
    
    // Snake motion state
    private Vector3 baseDirection;
    private float baseSpeed;
    private float waveTimer;
    private Vector3 lastPosition;
    
    private void Awake()
    {
        // Get required components
        slingshot = GetComponent<ProjectileSlingshot>();
        tracer = GetComponent<ProjectileTracer>();
        rb = GetComponent<Rigidbody>();
        
        // Validate components
        if (slingshot == null)
        {
            Debug.LogError("[ExtraHandProjectile] ProjectileSlingshot component is missing!");
        }
        
        if (rb == null)
        {
            Debug.LogError("[ExtraHandProjectile] Rigidbody component is missing!");
        }
        
        // Configure tracer for Extra Hand style (green theme with snake-like behavior)
        if (tracer != null)
        {
            tracer.SetTracerColor(new Color(0f, 1f, 0f, 1f)); // Green color for Extra Hand
            tracer.SetSnakeTrail(1.0f, 0.15f); // Longer trail for snake-like effect
        }
        else
        {
            Debug.LogWarning("[ExtraHandProjectile] ProjectileTracer component is missing - visual effects will not work!");
        }
    }
    
    /// <summary>
    /// Enable or disable snake motion effect
    /// </summary>
    public void SetSnakeMotionEnabled(bool enabled)
    {
        enableSnakeMotion = enabled;
    }
    
    /// <summary>
    /// Initialize the projectile with its parameters
    /// </summary>
    public void Initialize(Vector3 direction, float speed, float damage, float maxDistance, object source, LayerMask enemyMask)
    {
        // Store base movement parameters
        baseDirection = direction.normalized;
        baseSpeed = speed;
        lastPosition = transform.position;
        
        // Validate direction
        if (baseDirection == Vector3.zero)
        {
            Debug.LogWarning("[ExtraHandProjectile] Direction is zero! Using forward as fallback.");
            baseDirection = transform.forward;
        }
        
        // Calculate velocity
        Vector3 velocity = baseDirection * speed;
        
        // Calculate lifetime based on max distance and speed
        float lifetime = maxDistance / speed;
        
        // Launch the projectile using ProjectileSlingshot
        if (slingshot != null)
        {
            slingshot.Launch(velocity, damage, source, isCritical: false);
        }
        else
        {
            Debug.LogError("[ExtraHandProjectile] Cannot launch - ProjectileSlingshot is null!");
        }
        
        // Destroy after max distance is reached
        Destroy(gameObject, lifetime);
    }
    
    private void FixedUpdate()
    {
        if (!enableSnakeMotion || rb == null) return;
        
        // Don't apply snake motion until initialized
        if (baseDirection == Vector3.zero || baseSpeed == 0f) return;
        
        // Update wave timer
        waveTimer += Time.fixedDeltaTime * waveFrequency;
        
        // Calculate perpendicular direction for wave motion
        Vector3 perpendicular = Vector3.Cross(baseDirection, Vector3.up).normalized;
        
        // If the projectile is moving up/down, use forward as the perpendicular axis
        if (Vector3.Dot(baseDirection, Vector3.up) > 0.9f || Vector3.Dot(baseDirection, Vector3.up) < -0.9f)
        {
            perpendicular = Vector3.Cross(baseDirection, Vector3.forward).normalized;
        }
        
        // Calculate wave offset using sine wave
        float waveOffset = Mathf.Sin(waveTimer) * waveAmplitude;
        
        // Apply wave motion perpendicular to main direction
        Vector3 targetVelocity = baseDirection * baseSpeed + perpendicular * waveOffset * waveFrequency;
        
        // Smoothly adjust velocity to create snake-like motion
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 5f);
        
        // Rotate projectile to face movement direction for better visual effect
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }
}
