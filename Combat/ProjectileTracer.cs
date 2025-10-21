using UnityEngine;

/// <summary>
/// Visual tracer effect for projectiles with dynamic color changes.
/// Similar to Player2_ChargeUI but for projectile trails.
/// </summary>
[RequireComponent(typeof(ProjectileSlingshot))]
public class ProjectileTracer : MonoBehaviour
{
    [Header("Tracer Components")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Light glowLight;
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetup = true;
    [SerializeField] private float trailTime = 0.3f;
    [SerializeField] private float trailWidth = 0.1f;
    [SerializeField] private float lineWidth = 0.05f;
    
    [Header("Visual Settings - Based on Charge/State")]
    [SerializeField] private Color normalColor = new Color(0.3f, 1f, 0.3f, 0.9f); // Green - normal shot
    [SerializeField] private Color chargingColor = new Color(1f, 0.8f, 0f, 0.9f); // Yellow - charging
    [SerializeField] private Color fullChargeColor = new Color(1f, 0.3f, 0f, 1f); // Orange - full charge
    [SerializeField] private Color criticalColor = new Color(1f, 0f, 0f, 1f); // Red - critical hit
    [SerializeField] private Color scatterColor = new Color(0.5f, 0.5f, 1f, 0.9f); // Blue - scatter shot
    
    [Header("Glow Settings")]
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private float glowRange = 3f;
    [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0.3f);
    
    [Header("Pulse Effect")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 5f;
    [SerializeField] private float pulseIntensity = 0.3f;
    
    private ProjectileSlingshot projectile;
    private Color currentColor;
    private float pulseTimer;
    private bool isFullCharge;
    private bool isCritical;
    private float lifetimeProgress;
    
    private void Awake()
    {
        projectile = GetComponent<ProjectileSlingshot>();
        
        if (autoSetup)
        {
            SetupTracer();
        }
    }
    
    private void Start()
    {
        // Determine initial color based on projectile type
        DetermineInitialColor();
        ApplyColor(currentColor);
    }
    
    private void Update()
    {
        if (enablePulse)
        {
            UpdatePulseEffect();
        }
        
        // Fade intensity over projectile lifetime
        lifetimeProgress += Time.deltaTime;
        UpdateIntensityOverLifetime();
    }
    
    private void SetupTracer()
    {
        // Setup Trail Renderer
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }
        
        trailRenderer.time = trailTime;
        trailRenderer.startWidth = trailWidth;
        trailRenderer.endWidth = trailWidth * 0.3f;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = normalColor;
        trailRenderer.endColor = new Color(normalColor.r, normalColor.g, normalColor.b, 0f);
        trailRenderer.numCornerVertices = 2;
        trailRenderer.numCapVertices = 2;
        trailRenderer.alignment = LineAlignment.View;
        trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trailRenderer.receiveShadows = false;
        
        // Setup Line Renderer (for more solid look)
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth * 0.5f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = normalColor;
        lineRenderer.endColor = new Color(normalColor.r, normalColor.g, normalColor.b, 0.5f);
        lineRenderer.numCornerVertices = 2;
        lineRenderer.numCapVertices = 2;
        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.positionCount = 2;
        
        // Setup Glow Light
        if (enableGlow && glowLight == null)
        {
            GameObject lightObj = new GameObject("ProjectileGlow");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            glowLight = lightObj.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.intensity = glowIntensity;
            glowLight.range = glowRange;
            glowLight.color = normalColor;
            glowLight.shadows = LightShadows.None;
        }
    }
    
    private void DetermineInitialColor()
    {
        // Check if this is a critical hit projectile
        isCritical = projectile != null && projectile.IsCriticalHit();
        
        if (isCritical)
        {
            currentColor = criticalColor;
            return;
        }
        
        // Check projectile type by name or component
        if (gameObject.name.Contains("Scatter") || GetComponent<ScatterPelletDamageFalloff>() != null)
        {
            currentColor = scatterColor;
            return;
        }
        
        // Check if this is a charged shot (could check damage value or other indicator)
        if (projectile != null)
        {
            float damage = projectile.GetDamage();
            // If damage is significantly higher than base, it's likely a charged shot
            // This threshold should be adjusted based on your game's damage values
            if (damage > 30f) // Assume base damage is around 20
            {
                isFullCharge = true;
                currentColor = fullChargeColor;
                return;
            }
        }
        
        // Default to normal color
        currentColor = normalColor;
    }
    
    private void ApplyColor(Color color)
    {
        // Apply to trail
        if (trailRenderer != null)
        {
            trailRenderer.startColor = color;
            trailRenderer.endColor = new Color(color.r, color.g, color.b, 0f);
        }
        
        // Apply to line
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = new Color(color.r, color.g, color.b, 0.5f);
        }
        
        // Apply to glow light
        if (glowLight != null)
        {
            glowLight.color = color;
        }
    }
    
    private void UpdatePulseEffect()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;
        
        Color targetColor = currentColor;
        
        // Pulse effect for full charge (like the UI)
        if (isFullCharge || isCritical)
        {
            float pulse = Mathf.PingPong(pulseTimer, 1f);
            targetColor = Color.Lerp(currentColor, currentColor * 1.5f, pulse * pulseIntensity);
        }
        else
        {
            // Subtle pulse for normal shots
            float pulse = Mathf.Sin(pulseTimer) * 0.5f + 0.5f;
            targetColor = Color.Lerp(currentColor * 0.9f, currentColor * 1.1f, pulse * pulseIntensity);
        }
        
        ApplyColor(targetColor);
    }
    
    private void UpdateIntensityOverLifetime()
    {
        if (glowLight != null)
        {
            float normalizedLifetime = lifetimeProgress / 1f; // Assuming 1 second lifetime
            float intensity = intensityCurve.Evaluate(normalizedLifetime) * glowIntensity;
            glowLight.intensity = intensity;
        }
    }
    
    /// <summary>
    /// Public method to set the tracer color from external scripts
    /// </summary>
    public void SetTracerColor(Color color)
    {
        currentColor = color;
        ApplyColor(color);
    }
    
    /// <summary>
    /// Set tracer to charging state
    /// </summary>
    public void SetChargingState(float chargeProgress)
    {
        isFullCharge = chargeProgress >= 1f;
        currentColor = Color.Lerp(chargingColor, fullChargeColor, chargeProgress);
        ApplyColor(currentColor);
    }
    
    /// <summary>
    /// Set tracer to critical hit state
    /// </summary>
    public void SetCriticalHit()
    {
        isCritical = true;
        currentColor = criticalColor;
        ApplyColor(currentColor);
    }
    
    /// <summary>
    /// Update line renderer positions for beam effect
    /// </summary>
    private void LateUpdate()
    {
        if (lineRenderer != null)
        {
            // Create a short line from projectile backwards along velocity
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
            {
                Vector3 start = transform.position;
                Vector3 end = transform.position - rb.linearVelocity.normalized * 0.5f;
                
                lineRenderer.SetPosition(0, start);
                lineRenderer.SetPosition(1, end);
            }
            else
            {
                // Fallback if no rigidbody velocity
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, transform.position - transform.forward * 0.5f);
            }
        }
    }
}
