using UnityEngine;

/// <summary>
/// A projectile that orbits around the player like a windmill at an elevated height.
/// Follows the player while maintaining its orbital path.
/// </summary>
public class CirclingProjectile : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private bool alignToXAxis = true; // Make the prefab lay flat (aligned on X-axis)
    [SerializeField] private float spinSpeed = 720f; // Spin speed (degrees per second)
    [SerializeField] private float spinSpeedVariation = 360f; // Random variation range for spin speed
    
    [Header("Shrink/Expand Settings")]
    [SerializeField] private bool enableRadiusPulse = true;
    [SerializeField] private float minRadiusMultiplier = 0.3f; // Shrink to 30% of max radius
    [SerializeField] private float radiusPulseSpeed = 2f; // How fast it shrinks/expands
    
    [Header("Orbit Height")]
    [SerializeField] private float orbitHeightOffset = 0.8f; // Height above ground (Y-axis elevation)
    
    [Header("Blood Splatter Effect")]
    [SerializeField] private bool enableBloodSplatter = true;
    [SerializeField] private Color bloodColor = new Color(0.8f, 0f, 0.2f, 0.8f); // Dark red blood
    [SerializeField] private int bloodParticleCount = 20;
    [SerializeField] private float bloodParticleDuration = 1f;
    
    private Transform playerTransform; // Player to follow
    private float orbitRadius;
    private float maxOrbitRadius; // Store the maximum radius
    private float orbitSpeed;
    private float currentAngle;
    private float damage;
    private object source;
    private LayerMask enemyLayer;
    
    private bool isInitialized = false;
    private float radiusPulseTimer = 0f;
    private float actualSpinSpeed; // The actual spin speed for this projectile instance

    /// <summary>
    /// Initialize the circling projectile with its parameters
    /// </summary>
    public void Initialize(Transform player, float radius, float speed, float angleOffset, float dmg, object src, LayerMask enemyMask)
    {
        // Store player reference to follow
        playerTransform = player;
        maxOrbitRadius = radius; // Store the maximum radius
        orbitRadius = radius;
        orbitSpeed = speed;
        currentAngle = angleOffset;
        damage = dmg;
        source = src;
        enemyLayer = enemyMask;
        isInitialized = true;
        
        // Start at a random phase for variety
        radiusPulseTimer = Random.Range(0f, Mathf.PI * 2f);
        
        // Align prefab to lay flat on X-axis if enabled
        if (alignToXAxis)
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        
        // Randomize spin speed for variety
        actualSpinSpeed = spinSpeed + Random.Range(-spinSpeedVariation, spinSpeedVariation);
        
        // 50% chance to spin in opposite direction
        if (Random.value > 0.5f)
        {
            actualSpinSpeed = -actualSpinSpeed;
        }
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

        // Update radius pulse (shrink in and out)
        if (enableRadiusPulse)
        {
            radiusPulseTimer += Time.deltaTime * radiusPulseSpeed;
            
            // Oscillate between minRadius and maxRadius using sine wave
            float pulse = Mathf.Sin(radiusPulseTimer) * 0.5f + 0.5f; // 0 to 1 range
            float minRadius = maxOrbitRadius * minRadiusMultiplier;
            orbitRadius = Mathf.Lerp(minRadius, maxOrbitRadius, pulse);
        }

        // Calculate position on the circle around the player at elevated height
        float radians = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Cos(radians) * orbitRadius,
            orbitHeightOffset, // Elevated above ground
            Mathf.Sin(radians) * orbitRadius
        );

        // Set position relative to player (follows player while orbiting at elevated height)
        transform.position = playerTransform.position + offset;
        
        // Spin the projectile like a throwing axe
        transform.Rotate(Vector3.forward, actualSpinSpeed * Time.deltaTime, Space.Self);
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
            
            // Create blood splatter effect on hit
            if (enableBloodSplatter)
            {
                CreateBloodSplatterEffect(hitPoint);
            }
        }
    }

    /// <summary>
    /// Update the orbit radius (useful for upgrades)
    /// </summary>
    public void SetOrbitRadius(float newRadius)
    {
        maxOrbitRadius = newRadius;
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
    
    /// <summary>
    /// Create blood splatter visual effect on enemy hit
    /// </summary>
    private void CreateBloodSplatterEffect(Vector3 hitPosition)
    {
        // Create particle system object
        GameObject effectObj = new GameObject("BloodSplatter_Effect");
        effectObj.transform.position = hitPosition;
        
        // Add and configure particle system
        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        
        // Main module - particles explode outward like blood splatter
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f); // Shorter lifetime for splatter
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f); // Fast initial burst
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f); // Varied sizes
        main.startColor = bloodColor;
        main.maxParticles = bloodParticleCount * 2; // More particles for splatter effect
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 2f; // Gravity pulls particles down like blood
        
        // Emission module - single burst for splatter
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)(bloodParticleCount * 2)) });
        
        // Shape module - emit in all directions like blood splatter
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f; // Small radius for tight origin
        shape.radiusThickness = 1f; // Emit from surface
        
        // Color over lifetime - blood darkens and fades
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(bloodColor, 0f), // Bright red
                new GradientColorKey(new Color(0.4f, 0f, 0.1f), 1f) // Dark blood red
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f) // Fade out
            }
        );
        colorOverLifetime.color = gradient;
        
        // Size over lifetime - shrink as blood droplets dissipate
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Velocity over lifetime - slow down as particles lose momentum
        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.speedModifier = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));
        
        // Renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_Color", bloodColor);
        renderer.material.EnableKeyword("_EMISSION");
        
        // Play particles
        ps.Play();
        
        // Destroy after particles finish
        Destroy(effectObj, 1.5f);
    }

    private void OnDestroy()
    {
        // Cleanup if needed
    }
}
