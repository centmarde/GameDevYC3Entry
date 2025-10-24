using UnityEngine;

/// <summary>
/// Vampire Aura skill - passively restores health when dealing damage to enemies
/// Level 1-10: Progressively heals more based on damage dealt (lifesteal percentage)
/// Level 1: 5% lifesteal, Level 10: 14% lifesteal (heal 5-14% of damage dealt)
/// Independent skill with no ScriptableObject dependency
/// </summary>
public class PlayerSkill_VampireAura : MonoBehaviour
{
    [Header("Vampire Aura Settings")]
    [SerializeField] private float baseLifestealPercentage = 5f; // % of damage healed at level 1 (5%)
    [SerializeField] private float lifestealPercentagePerLevel = 1f; // Additional % per level (1% per level)
    
    [Header("Visual Effects")]
    [SerializeField] private Color auraColor = new Color(0.8f, 0f, 0.2f, 0.8f); // Dark red aura
    [SerializeField] private int particleCount = 20;
    [SerializeField] private float particleDuration = 1f;
    
    [Header("Runtime State")]
    [SerializeField] private bool isObtained = false;
    private bool wasObtainedLastFrame = false;
    
    // Level tracking (1-10)
    [SerializeField] private int currentLevel = 0; // 0 = not obtained, 1-10 = skill levels
    private const int MAX_LEVEL = 10;
    
    // Current stats
    private float currentLifestealPercentage;
    
    // Player reference
    private Player player;
    
    // Public accessors
    public bool IsObtained => isObtained;
    public int CurrentLevel => currentLevel;
    public int MaxLevel => MAX_LEVEL;
    public float CurrentHealPercentage => currentLifestealPercentage; // Keep name for compatibility
    
    private void Awake()
    {
        player = GetComponentInParent<Player>();
        
        if (player == null)
        {
            Debug.LogWarning("[VampireAura] Player reference is null! Make sure this script is attached to a child of a Player GameObject.");
        }
    }
    
    private void Start()
    {
        if (isObtained)
        {
            currentLevel = 1;
            ApplyLevelStats();
            RegisterDamageListener();
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
                RegisterDamageListener();
            }
            else
            {
                currentLevel = 0;
                UnregisterDamageListener();
            }
            wasObtainedLastFrame = isObtained;
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
        RegisterDamageListener();
        
        Debug.Log($"[VampireAura] Skill obtained at Level 1! Lifesteal: {currentLifestealPercentage}% of damage dealt");
    }
    
    /// <summary>
    /// Upgrade the skill to the next level
    /// </summary>
    public void UpgradeSkill()
    {
        if (!isObtained)
        {
            ObtainSkill();
            return;
        }
        
        if (currentLevel >= MAX_LEVEL)
        {
            Debug.LogWarning($"[VampireAura] Already at MAX level ({MAX_LEVEL})");
            return;
        }
        
        currentLevel++;
        ApplyLevelStats();
        
        Debug.Log($"[VampireAura] Upgraded to Level {currentLevel}! Lifesteal: {currentLifestealPercentage}% of damage dealt");
    }
    
    /// <summary>
    /// Apply stats based on current level
    /// Progressive scaling: Level 1 = 5%, Level 10 = 14% (5 + 9*1)
    /// </summary>
    private void ApplyLevelStats()
    {
        currentLifestealPercentage = baseLifestealPercentage + (currentLevel - 1) * lifestealPercentagePerLevel;
        Debug.Log($"[VampireAura] Level {currentLevel} - Lifesteal: {currentLifestealPercentage}% of damage dealt");
    }
    
    /// <summary>
    /// Register to listen for damage dealt events
    /// </summary>
    private void RegisterDamageListener()
    {
        // Register with DamageEventBroadcaster to receive damage events from player
        DamageEventBroadcaster.OnPlayerDamageDealt += OnDamageDealt;
        Debug.Log("[VampireAura] Registered damage listener");
    }
    
    /// <summary>
    /// Unregister from damage dealt events
    /// </summary>
    private void UnregisterDamageListener()
    {
        DamageEventBroadcaster.OnPlayerDamageDealt -= OnDamageDealt;
    }
    
    /// <summary>
    /// Called when the player deals damage to an enemy
    /// </summary>
    private void OnDamageDealt(float damageAmount, Vector3 hitPosition, object damageSource)
    {
        if (!isObtained || player == null) return;
        
        // Only heal from this player's damage
        if (damageSource == null) return;
        
        // Check if damage source is from this player or their projectiles
        Component sourceComponent = damageSource as Component;
        Player sourcePlayer = null;
        
        if (sourceComponent != null)
        {
            sourcePlayer = sourceComponent.GetComponentInParent<Player>();
        }
        else if (damageSource is Player)
        {
            sourcePlayer = damageSource as Player;
        }
        
        // Only trigger lifesteal for this player's damage
        if (sourcePlayer != player) return;
        
        // Get player's health component
        Entity_Health health = player.GetComponent<Entity_Health>();
        if (health != null)
        {
            // Calculate heal amount based on percentage of damage dealt (lifesteal)
            float healAmount = damageAmount * (currentLifestealPercentage / 100f);
            
            // Heal the player
            health.Heal(healAmount);
            Debug.Log($"[VampireAura] Dealt {damageAmount:F1} damage! Healed {healAmount:F1} HP ({currentLifestealPercentage}% lifesteal, Level {currentLevel})");
            
            // Show heal amount as UI text
            ShowLifestealUI(healAmount, hitPosition);
            
            // Show blood splatter visual effect
            CreateBloodSplatterEffect(hitPosition);
        }
    }
    
    /// <summary>
    /// Show lifesteal heal amount as floating UI text
    /// </summary>
    private void ShowLifestealUI(float healAmount, Vector3 position)
    {
        // Show green heal number at hit position
        DamageNumberUI.ShowHeal(healAmount, position);
    }
    
    /// <summary>
    /// Create blood splatter visual effect on damage hit
    /// </summary>
    private void CreateBloodSplatterEffect(Vector3 hitPosition)
    {
        if (player == null) return;
        
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
        main.startColor = auraColor;
        main.maxParticles = particleCount * 2; // More particles for splatter effect
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 2f; // Gravity pulls particles down like blood
        
        // Emission module - single burst for splatter
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)(particleCount * 2)) });
        
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
                new GradientColorKey(auraColor, 0f), // Bright red
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
        renderer.material.SetColor("_Color", auraColor);
        renderer.material.EnableKeyword("_EMISSION");
        
        // Play particles
        ps.Play();
        
        // Destroy after particles finish
        Destroy(effectObj, 1.5f);
    }
    
    /// <summary>
    /// Reset the skill to its original state
    /// </summary>
    public void ResetSkill()
    {
        isObtained = false;
        currentLevel = 0;
        currentLifestealPercentage = 0f;
        UnregisterDamageListener();
    }
    
    private void OnDestroy()
    {
        UnregisterDamageListener();
    }
}
