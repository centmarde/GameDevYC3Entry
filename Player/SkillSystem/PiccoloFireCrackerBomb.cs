using System.Collections;
using UnityEngine;

/// <summary>
/// Explosive bomb for Piccolo FireCracker skill
/// Stays in place for a duration then explodes, damaging nearby enemies
/// </summary>
public class PiccoloFireCrackerBomb : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Color explosionColor = new Color(1f, 0.5f, 0f, 1f); // Orange explosion
    [SerializeField] private int explosionParticleCount = 50;
    [SerializeField] private bool useAutoExplosionFX = true;
    
    [Header("Area Indicator")]
    [SerializeField] private bool showAreaIndicator = true;
    [SerializeField] private Color areaIndicatorColor = new Color(1f, 0.3f, 0f, 0.4f);
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip[] explosionSounds; // Multiple explosion sounds for variety
    [SerializeField] [Range(0f, 1f)] private float explosionVolume = 0.8f;
    [SerializeField] [Range(0.8f, 1.2f)] private float explosionPitchMin = 0.9f;
    [SerializeField] [Range(0.8f, 1.2f)] private float explosionPitchMax = 1.1f;
    private AudioSource audioSource;
    
    private float explosionTime;
    private float damage;
    private float areaRadius;
    private object source;
    private LayerMask enemyLayer;
    private GameObject explosionPrefab;
    private GameObject areaIndicator;
    
    private bool isInitialized = false;
    private bool hasExploded = false;
    private float timer = 0f;
    
    /// <summary>
    /// Initialize the bomb with its parameters
    /// </summary>
    public void Initialize(float timeToExplode, float explosionDamage, float explosionRadius, object damageSource, LayerMask enemies, GameObject explosionEffect = null)
    {
        explosionTime = timeToExplode;
        damage = explosionDamage;
        areaRadius = explosionRadius;
        source = damageSource;
        enemyLayer = enemies;
        explosionPrefab = explosionEffect;
        
        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // Full 3D sound for explosion
            audioSource.minDistance = 5f;
            audioSource.maxDistance = 30f;
        }
        
        isInitialized = true;
        
        // Create area indicator
        if (showAreaIndicator)
        {
            CreateAreaIndicator();
        }
        
        // Start countdown
        StartCoroutine(ExplodeAfterDelay());
        
        // Start pulsing visual effect
        StartCoroutine(PulseEffect());
    }
    
    /// <summary>
    /// Coroutine that explodes the bomb after the delay
    /// </summary>
    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explosionTime);
        
        if (!hasExploded)
        {
            // Play explosion sound
            PlayExplosionSound();
            
            Explode();
        }
    }
    
    /// <summary>
    /// Pulsing visual effect as countdown progresses
    /// </summary>
    private IEnumerator PulseEffect()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) yield break;
        
        Vector3 originalScale = transform.localScale;
        float pulseSpeed = 3f;
        
        while (!hasExploded && isInitialized)
        {
            // Pulse faster as explosion time approaches
            float progress = timer / explosionTime;
            float pulseAmount = Mathf.Lerp(0.1f, 0.3f, progress);
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed * (1f + progress)) * pulseAmount;
            
            transform.localScale = originalScale * pulse;
            
            // Increase glow as time progresses
            if (renderer.material != null)
            {
                Color emissionColor = explosionColor * Mathf.Lerp(2f, 5f, progress);
                renderer.material.SetColor("_EmissionColor", emissionColor);
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
    }
    
    /// <summary>
    /// Trigger the explosion
    /// </summary>
    private void Explode()
    {
        if (hasExploded) return;
        
        hasExploded = true;
        
        Debug.Log($"[PiccoloFireCrackerBomb] BOOM! Damage: {damage}, Radius: {areaRadius}m");
        
        // Find and damage all enemies in radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, areaRadius, enemyLayer);
        
        int enemiesHit = 0;
        foreach (Collider col in hitColliders)
        {
            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                // Calculate hit point and normal (direction from explosion to enemy)
                Vector3 hitPoint = col.ClosestPoint(transform.position);
                Vector3 hitNormal = (col.transform.position - transform.position).normalized;
                
                // Apply damage
                damageable.TakeDamage(damage, hitPoint, hitNormal, source);
                enemiesHit++;
                
                // Show damage number at enemy position
                DamageNumberUI.ShowDamage(damage, col.transform.position + Vector3.up * 1.5f, false);
                
                Debug.Log($"[PiccoloFireCrackerBomb] Hit enemy: {col.gameObject.name} for {damage} damage");
            }
        }
        
        Debug.Log($"[PiccoloFireCrackerBomb] Explosion hit {enemiesHit} enemies");
        
        // Create explosion VFX
        CreateExplosionEffect();
        
        // Destroy the bomb
        Destroy(gameObject, 0.5f); // Small delay to let effects play
    }
    
    /// <summary>
    /// Create visual explosion effect
    /// </summary>
    private void CreateExplosionEffect()
    {
        // Use prefab if provided
        if (explosionPrefab != null)
        {
            GameObject explosionInstance = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosionInstance, 3f); // Destroy after 3 seconds
            
            // Hide the bomb visual
            Renderer bombRenderer = GetComponent<Renderer>();
            if (bombRenderer != null)
            {
                bombRenderer.enabled = false;
            }
            return;
        }
        
        if (!useAutoExplosionFX) return;
        
        // Create explosion particle system
        GameObject explosionObj = new GameObject("FireCracker_Explosion");
        explosionObj.transform.position = transform.position;
        
        ParticleSystem ps = explosionObj.AddComponent<ParticleSystem>();
        
        // Main module - explosive burst
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        main.startColor = new ParticleSystem.MinMaxGradient(explosionColor, Color.yellow);
        main.maxParticles = explosionParticleCount * 2;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.5f;
        
        // Emission - single burst
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { 
            new ParticleSystem.Burst(0f, (short)explosionParticleCount) 
        });
        
        // Shape - sphere explosion
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        shape.radiusThickness = 1f;
        
        // Color over lifetime
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.yellow, 0f),
                new GradientColorKey(explosionColor, 0.3f),
                new GradientColorKey(Color.red, 0.7f),
                new GradientColorKey(Color.black, 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
        
        // Size over lifetime
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.5f, 1.5f);
        sizeCurve.AddKey(1f, 0.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Renderer
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_Color", explosionColor);
        renderer.material.EnableKeyword("_EMISSION");
        
        // Play and destroy
        ps.Play();
        Destroy(explosionObj, 2f);
        
        // Hide the bomb visual
        Renderer bombVisual = GetComponent<Renderer>();
        if (bombVisual != null)
        {
            bombVisual.enabled = false;
        }
    }
    
    /// <summary>
    /// Create visual indicator for explosion area on the ground
    /// </summary>
    private void CreateAreaIndicator()
    {
        // Create a flat disc to show explosion radius
        areaIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        areaIndicator.name = "ExplosionAreaIndicator";
        
        // Remove collider
        Destroy(areaIndicator.GetComponent<Collider>());
        
        // Position it on the ground slightly below bomb
        areaIndicator.transform.position = new Vector3(transform.position.x, 0.05f, transform.position.z);
        areaIndicator.transform.localScale = new Vector3(areaRadius * 2f, 0.02f, areaRadius * 2f);
        
        // Setup material for URP
        Renderer indicatorRenderer = areaIndicator.GetComponent<Renderer>();
        if (indicatorRenderer != null)
        {
            // Try URP/Lit shader first, fallback to Unlit/Color
            Shader targetShader = Shader.Find("Universal Render Pipeline/Lit");
            if (targetShader == null)
            {
                targetShader = Shader.Find("Unlit/Color");
            }
            if (targetShader == null)
            {
                targetShader = Shader.Find("Standard");
            }
            
            Material mat = new Material(targetShader);
            
            // Set base color with proper property names for URP
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", areaIndicatorColor);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", areaIndicatorColor);
            }
            
            // Setup transparency for URP
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1); // Transparent
                mat.SetFloat("_Blend", 0); // Alpha blend
            }
            
            // Disable Z-Write for transparency
            if (mat.HasProperty("_ZWrite"))
            {
                mat.SetFloat("_ZWrite", 0);
            }
            
            // Setup render queue
            mat.renderQueue = 3000;
            
            // Add emission for glow
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", explosionColor * 0.3f);
                mat.EnableKeyword("_EMISSION");
            }
            
            indicatorRenderer.material = mat;
        }
        
        // Parent to bomb
        areaIndicator.transform.SetParent(transform);
        
        // Start smooth fade-in animation instead of flickering pulse
        StartCoroutine(FadeInAreaIndicator());
    }
    
    /// <summary>
    /// Smooth fade-in for area indicator (no flickering)
    /// </summary>
    private IEnumerator FadeInAreaIndicator()
    {
        if (areaIndicator == null) yield break;
        
        Renderer indicatorRenderer = areaIndicator.GetComponent<Renderer>();
        if (indicatorRenderer == null) yield break;
        
        Material mat = indicatorRenderer.material;
        if (mat == null) yield break;
        
        // Fade in smoothly over 0.3 seconds
        float fadeInTime = 0.3f;
        float elapsed = 0f;
        
        Color targetColor = areaIndicatorColor;
        Color startColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
        
        while (elapsed < fadeInTime)
        {
            if (areaIndicator == null || mat == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInTime;
            Color currentColor = Color.Lerp(startColor, targetColor, t);
            
            // Set color with proper property names
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", currentColor);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", currentColor);
            }
            
            yield return null;
        }
        
        // Ensure final color is set
        if (mat != null)
        {
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", targetColor);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", targetColor);
            }
        }
        
        // Now start gentle brightness increase as countdown progresses
        StartCoroutine(GradualBrightnessIncrease());
    }
    
    /// <summary>
    /// Gradually increase brightness as explosion approaches (smooth, no flicker)
    /// </summary>
    private IEnumerator GradualBrightnessIncrease()
    {
        if (areaIndicator == null) yield break;
        
        Renderer indicatorRenderer = areaIndicator.GetComponent<Renderer>();
        if (indicatorRenderer == null) yield break;
        
        Material mat = indicatorRenderer.material;
        if (mat == null) yield break;
        
        Color baseColor = areaIndicatorColor;
        
        while (!hasExploded && areaIndicator != null && mat != null)
        {
            float progress = Mathf.Clamp01(timer / explosionTime);
            
            // Smoothly increase opacity from base to brighter (no pulsing)
            float alpha = Mathf.Lerp(baseColor.a, Mathf.Min(baseColor.a * 1.5f, 0.8f), progress);
            Color targetColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            
            // Update color
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", targetColor);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", targetColor);
            }
            
            // Gradually increase emission (smooth)
            if (mat.HasProperty("_EmissionColor"))
            {
                float emissionIntensity = Mathf.Lerp(0.3f, 1.0f, progress);
                Color emissionColor = explosionColor * emissionIntensity;
                mat.SetColor("_EmissionColor", emissionColor);
            }
            
            yield return new WaitForSeconds(0.1f); // Update every 0.1s instead of every frame
        }
    }
    
    // Debug visualization
    private void OnDrawGizmos()
    {
        if (isInitialized && !hasExploded)
        {
            // Draw sphere for explosion radius
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, areaRadius);
            
            // Draw ground circle
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            DrawCircle(transform.position, areaRadius, 32);
        }
    }
    
    /// <summary>
    /// Draw a circle gizmo on the ground
    /// </summary>
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
    
    /// <summary>
    /// Play explosion sound with pitch variation
    /// </summary>
    private void PlayExplosionSound()
    {
        if (audioSource == null) return;
        
        AudioClip clipToPlay = null;
        
        // Use array if available, otherwise use single clip
        if (explosionSounds != null && explosionSounds.Length > 0)
        {
            clipToPlay = explosionSounds[Random.Range(0, explosionSounds.Length)];
        }
        else if (explosionSound != null)
        {
            clipToPlay = explosionSound;
        }
        
        if (clipToPlay != null)
        {
            // Random pitch for variety
            float randomPitch = Random.Range(explosionPitchMin, explosionPitchMax);
            audioSource.pitch = randomPitch;
            audioSource.PlayOneShot(clipToPlay, explosionVolume);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up area indicator
        if (areaIndicator != null)
        {
            Destroy(areaIndicator);
        }
    }
}
