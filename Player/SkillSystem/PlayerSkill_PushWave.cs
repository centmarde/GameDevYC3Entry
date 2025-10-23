using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player skill that creates a force wave pushing enemies away from the player.
/// Automatically triggers at intervals. Level-based upgrade system (1-10).
/// Independent skill with no ScriptableObject dependency.
/// </summary>
public class PlayerSkill_PushWave : MonoBehaviour
{
    [Header("Push Wave Settings")]
    [SerializeField] private float pushRadius = 3f;
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float pushDamage = 4f;
    [SerializeField] private float autoActivateInterval = 4f; // Auto-activation interval (starts at 4s, scales to 2s at max level)
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Visual Effects")]
    [SerializeField] private Color particleColor = new Color(0.3f, 0.7f, 1f, 0.8f);
    [SerializeField] private int particleCount = 10;
    [SerializeField] private float lightIntensity = 0.2f;
    [SerializeField] private float lightRange = 2f;
    
    [Header("Runtime State")]
    [SerializeField] private bool isObtained = false;
    private bool wasObtainedLastFrame = false;
    
    // Level-based upgrade system (1-10)
    [SerializeField] private int currentLevel = 0; // 0 = not obtained, 1-10 = skill levels
    private const int MAX_LEVEL = 10;
    
    // Base stats (level 1)
    private float baseRadius;
    private float baseForce;
    private float baseDamage;
    private float baseInterval;
    
    // Auto-activation tracking
    private float lastActivationTime = -999f;
    private bool canActivate => Time.time >= lastActivationTime + autoActivateInterval;
    
    private void Awake()
    {
        // Store base stats for level calculations
        baseRadius = pushRadius;
        baseForce = pushForce;
        baseDamage = pushDamage;
        baseInterval = autoActivateInterval;
    }
    
    private void Start()
    {
        if (isObtained)
        {
            currentLevel = 1;
            ApplyLevelStats();
        }
        wasObtainedLastFrame = isObtained;
    }
    
    private void Update()
    {
        // Check if isObtained was toggled in Inspector during Play Mode
        if (isObtained != wasObtainedLastFrame)
        {
            if (isObtained)
            {
                currentLevel = 1;
                ApplyLevelStats();
            }
            else
            {
                currentLevel = 0;
            }
            wasObtainedLastFrame = isObtained;
        }
        
        // Auto-activate when skill is obtained and ready
        if (isObtained && canActivate)
        {
            ActivatePushWave();
        }
    }
    
    /// <summary>
    /// Obtain the skill (sets to Level 1)
    /// </summary>
    public void ObtainSkill()
    {
        if (isObtained)
        {
            return;
        }
        
        isObtained = true;
        currentLevel = 1;
        ApplyLevelStats();
        
        Debug.Log($"[PushWave] Skill obtained at Level 1!");
    }
    
    /// <summary>
    /// Activate the push wave skill
    /// </summary>
    public void ActivatePushWave()
    {
        if (!isObtained || !canActivate)
        {
            return;
        }
        
        lastActivationTime = Time.time;
        
        // Find all enemies in radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pushRadius, enemyLayer);
        
        List<GameObject> pushedEnemies = new List<GameObject>();
        
        foreach (Collider col in hitColliders)
        {
            // Get the root enemy object
            var enemy = col.GetComponentInParent<IDamageable>();
            if (enemy != null && enemy.IsAlive)
            {
                // Don't push the same enemy multiple times if it has multiple colliders
                if (!pushedEnemies.Contains(col.transform.root.gameObject))
                {
                    pushedEnemies.Add(col.transform.root.gameObject);
                    
                    // Calculate push direction (away from player)
                    Vector3 pushDirection = (col.transform.position - transform.position).normalized;
                    Vector3 hitPoint = col.ClosestPoint(transform.position);
                    
                    // Apply damage
                    enemy.TakeDamage(pushDamage, hitPoint, -pushDirection, this);
                    
                    // Apply force if the enemy has a rigidbody
                    Rigidbody rb = col.GetComponentInParent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
                    }
                }
            }
        }
        
        // Visual effect
        CreatePushWaveEffect();
        
        Debug.Log($"[PushWave] Auto-activated! Pushed {pushedEnemies.Count} enemies. Radius: {pushRadius:F1}, Force: {pushForce:F1}, Damage: {pushDamage:F1}, Next in: {autoActivateInterval:F1}s");
    }
    
    /// <summary>
    /// Create visual effect - fireflies circling the player
    /// </summary>
    private void CreatePushWaveEffect()
    {
        // Create particle system object
        GameObject effectObj = new GameObject("Fireflies_Effect");
        effectObj.transform.position = transform.position;
        effectObj.transform.SetParent(transform); // Parent to player so it moves with them
        
        // Add and configure particle system
        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        
        // Main module - continuous emission for circling effect
        var main = ps.main;
        main.startLifetime = 1.5f; // Longer lifetime for circling
        main.startSpeed = 0f; // No initial speed, we'll use velocity over lifetime
        main.startSize = 0.12f; // Small firefly size
        main.startColor = particleColor;
        main.maxParticles = particleCount;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;
        
        // Emission module - burst emission
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, particleCount) });
        
        // Shape module - spawn in a ring around player
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = pushRadius * 0.5f; // Start at half the push radius
        shape.radiusThickness = 0.3f;
        shape.arc = 360f;
        shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
        
        // Velocity over lifetime - make them circle using manual animation
        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = false; // Disable - we'll use custom animation instead
        
        // Color over lifetime (glow pulse effect)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(particleColor, 0f),
                new GradientColorKey(Color.Lerp(particleColor, Color.white, 0.3f), 0.3f),
                new GradientColorKey(particleColor, 0.7f),
                new GradientColorKey(particleColor, 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.2f),
                new GradientAlphaKey(1f, 0.8f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
        
        // Size over lifetime (pulse)
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(0.7f, 1f);
        sizeCurve.AddKey(1f, 0.3f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_Color", particleColor);
        renderer.material.EnableKeyword("_EMISSION");
        
        // Play particles first
        ps.Play();
        
        // Animate particles and lights together with same motion
        StartCoroutine(AnimateFirefliesWithLights(effectObj.transform, ps));
        
        // Destroy after animation
        Destroy(effectObj, 2f);
    }
    
    /// <summary>
    /// Animate both particles and lights together with synchronized motion
    /// </summary>
    private IEnumerator AnimateFirefliesWithLights(Transform effectTransform, ParticleSystem ps)
    {
        float duration = 1.5f;
        float elapsed = 0f;
        
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
        
        // Store motion data for each firefly (shared between particle and light)
        float[] fireflyAngles = new float[particleCount];
        float[] fireflyRadii = new float[particleCount];
        float[] rotationSpeeds = new float[particleCount];
        Vector3[] rotationAxes = new Vector3[particleCount];
        float[] heightOffsets = new float[particleCount]; // Random height for scatter
        float[] bobSpeeds = new float[particleCount]; // Individual bob speeds
        float[] wobblePhases = new float[particleCount]; // Random wobble phases
        
        // Create lights array
        GameObject[] lightObjects = new GameObject[particleCount];
        Light[] lights = new Light[particleCount];
        
        yield return new WaitForSeconds(0.05f); // Wait for particles to spawn
        
        int activeParticleCount = ps.GetParticles(particles);
        
        // Initialize each firefly (particle + light pair)
        for (int i = 0; i < activeParticleCount; i++)
        {
            fireflyAngles[i] = Random.Range(0f, 360f); // Random starting angle
            fireflyRadii[i] = Random.Range(pushRadius * 0.3f, pushRadius * 1.2f); // Wider radius range for more scatter
            rotationSpeeds[i] = Random.Range(180f, 360f); // Much faster rotation speeds
            heightOffsets[i] = Random.Range(-1f, 1f); // Random height offset for vertical scatter
            bobSpeeds[i] = Random.Range(4f, 8f); // Faster bobbing speeds
            wobblePhases[i] = Random.Range(0f, Mathf.PI * 2f); // Random starting phase for wobble
            
            // More varied orbital axes for better scatter
            rotationAxes[i] = new Vector3(
                Random.Range(-0.6f, 0.6f), // Increased tilt range
                Random.Range(0.7f, 1f), // Slightly varied Y component
                Random.Range(-0.6f, 0.6f)  // Increased tilt range
            ).normalized;
            
            // Create light for this firefly
            GameObject lightObj = new GameObject($"FireflyLight_{i}");
            lightObj.transform.SetParent(effectTransform);
            
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = particleColor;
            light.intensity = lightIntensity * Random.Range(0.7f, 1.3f);
            light.range = lightRange;
            light.renderMode = LightRenderMode.ForcePixel;
            
            lightObjects[i] = lightObj;
            lights[i] = light;
        }
        
        // Animate particles and lights together
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            
            activeParticleCount = ps.GetParticles(particles);
            
            for (int i = 0; i < activeParticleCount; i++)
            {
                // Update angle
                fireflyAngles[i] += rotationSpeeds[i] * Time.deltaTime;
                
                // Calculate base circular position with more dynamic radius
                float angleRad = fireflyAngles[i] * Mathf.Deg2Rad;
                
                // Add radius wobble for scatter effect while maintaining orbit
                float radiusWobble = Mathf.Sin(elapsed * 2f + wobblePhases[i]) * pushRadius * 0.3f;
                float currentRadius = fireflyRadii[i] + radiusWobble;
                
                Vector3 baseCirclePos = new Vector3(
                    Mathf.Cos(angleRad) * currentRadius,
                    0f,
                    Mathf.Sin(angleRad) * currentRadius
                );
                
                // Apply orbital rotation with varied axis
                Quaternion axisRotation = Quaternion.AngleAxis(fireflyAngles[i] * 0.5f, rotationAxes[i]);
                Vector3 rotatedPos = axisRotation * baseCirclePos;
                
                // Individual vertical bobbing with varied speeds and heights
                float bobPhase = elapsed * bobSpeeds[i] + wobblePhases[i];
                float verticalOffset = heightOffsets[i] + Mathf.Sin(bobPhase) * 0.5f;
                
                // Add subtle random drift for organic scatter
                float driftX = Mathf.Sin(elapsed * 1.5f + wobblePhases[i]) * 0.3f;
                float driftZ = Mathf.Cos(elapsed * 1.8f + wobblePhases[i] * 0.7f) * 0.3f;
                
                Vector3 newPos = transform.position + rotatedPos + new Vector3(driftX, verticalOffset, driftZ);
                
                // Update particle position
                particles[i].position = newPos;
                
                // Update light position (same as particle)
                if (lightObjects[i] != null)
                {
                    lightObjects[i].transform.position = newPos;
                    
                    // Pulse light intensity with individual timing
                    float baseLightIntensity = lightIntensity * Random.Range(0.7f, 1.3f);
                    lights[i].intensity = baseLightIntensity * (0.7f + Mathf.Sin(elapsed * Random.Range(6f, 10f) + wobblePhases[i]) * 0.3f);
                    
                    // Fade out at the end
                    if (elapsed / duration > 0.7f)
                    {
                        float fadeT = (elapsed / duration - 0.7f) / 0.3f;
                        lights[i].intensity *= Mathf.Lerp(1f, 0f, fadeT);
                    }
                }
            }
            
            ps.SetParticles(particles, activeParticleCount);
            
            yield return null;
        }
        
        // Clean up lights
        for (int i = 0; i < lightObjects.Length; i++)
        {
            if (lightObjects[i] != null)
            {
                Destroy(lightObjects[i]);
            }
        }
    }
    
    #region Upgrade Methods
    
    /// <summary>
    /// Upgrade to the next level (increases all stats)
    /// Level 1: Base stats
    /// Level 2-10: Each level increases radius, force, damage, and reduces cooldown
    /// </summary>
    public void UpgradeLevel()
    {
        if (!isObtained || currentLevel >= MAX_LEVEL)
        {
            Debug.LogWarning($"[PushWave] Cannot upgrade - Level: {currentLevel}, Obtained: {isObtained}");
            return;
        }
        
        currentLevel++;
        ApplyLevelStats();
        
        Debug.Log($"[PushWave] Upgraded to Level {currentLevel} - Radius: {pushRadius:F1}, Force: {pushForce:F1}, Damage: {pushDamage:F1}, Interval: {autoActivateInterval:F1}s");
    }
    
    /// <summary>
    /// Apply stats based on current level
    /// Each level increases all stats progressively
    /// Interval: 4s (level 1) → 2s (level 10)
    /// </summary>
    private void ApplyLevelStats()
    {
        // Radius scaling: +0.2m per level
        pushRadius = baseRadius + (currentLevel - 1) * 0.2f;
        
        // Force scaling: +1 force per level (lower base, slower growth)
        pushForce = baseForce + (currentLevel - 1) * 1f;
        
        // Damage scaling: +1 damage per level (lower base, slower growth)
        pushDamage = baseDamage + (currentLevel - 1) * 1f;
        
        // Interval reduction: 4s → 2s over 9 levels (-0.222s per level)
        autoActivateInterval = baseInterval - (currentLevel - 1) * 0.222f;
        
        Debug.Log($"[PushWave] Level {currentLevel} - Radius: {pushRadius:F2}m, Force: {pushForce:F1}, Damage: {pushDamage:F1}, Interval: {autoActivateInterval:F2}s");
    }
    
    #endregion
    
    #region Public Getters
    
    public bool IsObtained => isObtained;
    public int CurrentLevel => currentLevel;
    public int MaxLevel => MAX_LEVEL;
    public float CurrentRadius => pushRadius;
    public float CurrentForce => pushForce;
    public float CurrentDamage => pushDamage;
    public float CurrentInterval => autoActivateInterval;
    public float TimeUntilNextActivation => Mathf.Max(0f, (lastActivationTime + autoActivateInterval) - Time.time);
    public bool CanActivate => canActivate;
    
    #endregion
    
    /// <summary>
    /// Reset the skill to its original state
    /// </summary>
    public void ResetSkill()
    {
        isObtained = false;
        currentLevel = 0;
        lastActivationTime = -999f;
        
        // Reset to base stats
        pushRadius = baseRadius;
        pushForce = baseForce;
        pushDamage = baseDamage;
        autoActivateInterval = baseInterval;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualize push radius in editor
        if (isObtained)
        {
            Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, pushRadius);
        }
    }
}
