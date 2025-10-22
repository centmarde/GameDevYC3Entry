﻿using System.Collections;
using UnityEngine;

public class Enemy : Entity, ITargetable
{
    // ========= Stats / Targeting =========
    [SerializeField] private EnemyStatData_SO enemyStats;
    protected virtual EnemyStatData_SO Stats => enemyStats;
    public float AttackDamage => enemyStats != null ? enemyStats.attackDamage : 10f;
    public float AttackRadius => enemyStats != null ? enemyStats.attackRadius : 2f;
    public float AttackRange => enemyStats != null ? enemyStats.attackRange : 2f;
    public float AttackCooldown => enemyStats != null ? enemyStats.attackCooldown : 1f;


    public Enemy_Movement movement {  get; private set; }  

    public Enemy_Combat combat { get; private set; }


    [SerializeField] private Transform aimPoint;
    public Transform Transform => transform;
    public Vector3 AimPoint => aimPoint != null ? aimPoint.position : transform.position + Vector3.up * 1.2f;
    public bool IsAlive => health != null && health.IsAlive;

    private Entity_Health health;
    private EnemyDeathTracker deathTracker;
    private bool isDead = false;

    private Player player;



    public Enemy_MoveState moveState { get; private set; }
    public Enemy_ChaseState chaseState { get; private set; }
    public Enemy_ReturnHomeState returnHomeState { get; private set; }
    public Enemy_IdleState idleState { get; private set; }
    public Enemy_MeleeAttackState meleeAttackState { get; private set; }




    protected override void Awake()
    {
        base.Awake();

        combat = GetComponent<Enemy_Combat>();
        movement = GetComponent<Enemy_Movement>();
        health = GetComponent<Entity_Health>();
        deathTracker = GetComponent<EnemyDeathTracker>();

        // Check if enemyStats is assigned before using it
        if (Stats == null)
        {
            enabled = false; // Disable component to prevent further errors
            return;
        }

        if (health != null) health.SetMaxHealth(Stats.maxHealth);

        var pGO = GameObject.FindWithTag("Player");

        if (pGO != null) player = pGO.GetComponent<Player>();
        if (combat != null) combat.SetTarget(player != null ? player.transform : null);

        if (movement != null)
        {
            movement.Init(Stats, transform.position, transform.forward, player != null ? player.transform : null);
        }

        idleState = new Enemy_IdleState(this, stateMachine, "isIdle");
        moveState = new Enemy_MoveState(this, stateMachine, "isMoving");
        chaseState = new Enemy_ChaseState(this, stateMachine, "isChasing");
        returnHomeState = new Enemy_ReturnHomeState(this, stateMachine, "isChasing");
        meleeAttackState = new Enemy_MeleeAttackState(this, stateMachine, "isAttacking");
    }

    protected override void Start()
    {
        base.Start();
        if (Stats != null && stateMachine != null && idleState != null)
        {
            stateMachine.Initialize(idleState);
        }
    }

    protected override void Update()
    {
        // Don't update state machine if dead
        if (!isDead)
        {
            base.Update();
        }
    }


    public override void EntityDeath()
    {
        if (isDead) return; // Prevent multiple death calls
        isDead = true;

        base.EntityDeath(); // stop motion, trigger anim if any

        // Disable components to prevent updates
        if (combat != null) combat.enabled = false;
        if (movement != null) movement.enabled = false;

        // Set death animation boolean BEFORE exiting state
        if (anim != null)
        {
            anim.SetBool("isDead", true);
        }

        // Stop the state machine from interfering with animations
        // Note: We don't change state, we just let isDead flag prevent updates
        if (stateMachine != null && stateMachine.currentState != null)
        {
            // Exit current state to stop its animation bools
            stateMachine.currentState.Exit();
        }

        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // Prevent physics interactions
        }

        // IMPORTANT: Notify wave manager about enemy death
        if (deathTracker != null)
        {
            deathTracker.NotifyDeath();
        }

        // Start shrink effect and destroy after animation
        StartCoroutine(ShrinkAndDestroy());
    }

    private IEnumerator ShrinkAndDestroy()
    {
        float delayBeforeDecay = 1.5f; // Wait 1.5 seconds before starting decay
        float decayDuration = 1.5f; // Duration of the decay/sink effect
        
        // Wait before starting the decay effect
        yield return new WaitForSeconds(delayBeforeDecay);

        // Create particle effect (wrapped in try-catch for build safety)
        GameObject particleObj = null;
        try
        {
            particleObj = CreateGlowingParticles();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to create death particles: {e.Message}");
        }

        // Store original position
        Vector3 startPosition = transform.position;
        float sinkDepth = 2f; // How far into the ground the enemy sinks
        float elapsed = 0f;

        // Get all renderers to fade them out
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
        // Decay/sink into the ground
        while (elapsed < decayDuration)
        {
            // Safety check: if object is already destroyed, exit
            if (this == null || gameObject == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / decayDuration;
            
            // Ease-in decay (accelerating downward)
            float easeT = t * t;
            
            // Sink into the ground
            transform.position = new Vector3(
                startPosition.x,
                startPosition.y - (easeT * sinkDepth),
                startPosition.z
            );

            // Update particle position to follow enemy
            if (particleObj != null)
            {
                particleObj.transform.position = transform.position + Vector3.up * 0.5f;
            }

            // Fade out all materials
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        if (mat != null && mat.HasProperty("_Color"))
                        {
                            Color color = mat.color;
                            color.a = 1f - easeT; // Fade from 1 to 0
                            mat.color = color;
                        }
                    }
                }
            }

            yield return null;
        }
        
        // Destroy particle effect
        if (particleObj != null)
        {
            Destroy(particleObj, 0.5f);
        }
        
        // Final safety check before destroying
        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private GameObject CreateGlowingParticles()
    {
        GameObject particleObj = new GameObject("DeathParticles");
        particleObj.transform.position = transform.position + Vector3.up * 0.5f;
        particleObj.transform.parent = null; // Don't parent to enemy so it stays visible

        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.startLifetime = 0.8f;
        main.startSpeed = 2f;
        main.startSize = 0.15f;
        main.startColor = new Color(1f, 0.6f, 0.2f, 1f); // Orange glow
        main.maxParticles = 20;
        main.loop = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 15f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0.0f), // Bright yellow-orange
                new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0.5f), // Orange
                new GradientColorKey(new Color(0.8f, 0.2f, 0.0f), 1.0f)  // Deep red-orange
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0));

        // Try to enable lights on particles (may fail in some builds)
        try
        {
            var lights = ps.lights;
            lights.enabled = true;
            lights.ratio = 0.3f; // 30% of particles emit light
            lights.useRandomDistribution = true;
            lights.maxLights = 6; // Limit for performance
            
            // Create a point light prefab for particles
            GameObject lightTemplate = new GameObject("ParticleLight");
            lightTemplate.transform.parent = particleObj.transform;
            Light pointLight = lightTemplate.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = new Color(1f, 0.6f, 0.2f); // Orange light
            pointLight.intensity = 0.8f;
            pointLight.range = 2.5f;
            pointLight.shadows = LightShadows.Soft; // Enable soft shadows
            pointLight.shadowStrength = 0.8f;
            pointLight.shadowBias = 0.05f;
            pointLight.shadowNormalBias = 0.4f;
            
            lights.light = pointLight;
            
            // Use intensity over lifetime to fade lights
            lights.useParticleColor = true;
            lights.intensityMultiplier = 1f; // Base intensity multiplier
            lights.rangeMultiplier = 1f; // Base range multiplier
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to create particle lights: {e.Message}");
        }

        // Renderer settings for glow
        var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
        
        // Try to find shader, fallback to default if not found
        Shader particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader == null)
        {
            particleShader = Shader.Find("Mobile/Particles/Additive");
        }
        if (particleShader == null)
        {
            particleShader = Shader.Find("Legacy Shaders/Particles/Additive");
        }
        if (particleShader == null)
        {
            // Final fallback - use default particle shader
            particleShader = Shader.Find("Particles/Additive");
        }
        
        Color particleColor = new Color(1f, 0.6f, 0.2f, 1f); // Orange color
        
        if (particleShader != null)
        {
            renderer.material = new Material(particleShader);
            
            // Try multiple property names to ensure color is set
            if (renderer.material.HasProperty("_Color"))
            {
                renderer.material.SetColor("_Color", particleColor);
            }
            if (renderer.material.HasProperty("_TintColor"))
            {
                renderer.material.SetColor("_TintColor", particleColor);
            }
            if (renderer.material.HasProperty("_EmissionColor"))
            {
                renderer.material.SetColor("_EmissionColor", particleColor);
            }
            
            // Enable emission if available
            if (renderer.material.HasProperty("_EmissionEnabled"))
            {
                renderer.material.SetFloat("_EmissionEnabled", 1f);
            }
        }
        else
        {
            // No shader found, use default material color
            renderer.material.color = particleColor;
        }
        
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        return particleObj;
    }

    private void OnDrawGizmos()
    {
        if (Stats == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Stats.aggroRadius);
    }
}
