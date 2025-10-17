using UnityEngine;

/// <summary>
/// Optional visual component for circling projectiles.
/// Adds pulsing glow, trail, and particle effects.
/// </summary>
[RequireComponent(typeof(CirclingProjectile))]
public class CirclingProjectileVisual : MonoBehaviour
{
    [Header("Glow Effect")]
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private Light glowLight;
    [SerializeField] private float glowIntensityMin = 1f;
    [SerializeField] private float glowIntensityMax = 2f;
    [SerializeField] private float glowPulseSpeed = 2f;
    
    [Header("Rotation Effect")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 360f;
    
    [Header("Scale Pulse")]
    [SerializeField] private bool enableScalePulse = false;
    [SerializeField] private Vector3 baseScale = Vector3.one;
    [SerializeField] private float scalePulseAmount = 0.1f;
    [SerializeField] private float scalePulseSpeed = 3f;
    
    [Header("Trail Effect")]
    [SerializeField] private TrailRenderer trail;
    
    [Header("Particle Effect")]
    [SerializeField] private ParticleSystem particles;

    private float pulseTimer;

    private void Start()
    {
        // Auto-find components if not assigned
        if (glowLight == null)
        {
            glowLight = GetComponentInChildren<Light>();
        }
        
        if (trail == null)
        {
            trail = GetComponentInChildren<TrailRenderer>();
        }
        
        if (particles == null)
        {
            particles = GetComponentInChildren<ParticleSystem>();
        }

        pulseTimer = Random.Range(0f, Mathf.PI * 2f); // Random start phase
    }

    private void Update()
    {
        pulseTimer += Time.deltaTime;

        // Pulsing glow effect
        if (enableGlow && glowLight != null)
        {
            float pulse = Mathf.Sin(pulseTimer * glowPulseSpeed) * 0.5f + 0.5f;
            glowLight.intensity = Mathf.Lerp(glowIntensityMin, glowIntensityMax, pulse);
        }

        // Scale pulsing effect
        if (enableScalePulse)
        {
            float pulse = Mathf.Sin(pulseTimer * scalePulseSpeed) * scalePulseAmount;
            transform.localScale = baseScale + Vector3.one * pulse;
        }
    }

    /// <summary>
    /// Set the color of all visual elements
    /// </summary>
    public void SetColor(Color color)
    {
        // Set light color
        if (glowLight != null)
        {
            glowLight.color = color;
        }

        // Set trail color
        if (trail != null)
        {
            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
        }

        // Set particle color
        if (particles != null)
        {
            var main = particles.main;
            main.startColor = color;
        }

        // Set material color if available
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.SetColor("_Color", color);
            renderer.material.SetColor("_EmissionColor", color * 2f);
        }
    }

    /// <summary>
    /// Play hit effect when projectile hits an enemy
    /// </summary>
    public void PlayHitEffect()
    {
        if (particles != null)
        {
            particles.Play();
        }
        
        // Could add sound effect here
    }
}
