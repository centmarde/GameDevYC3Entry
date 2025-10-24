using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Auto-shooting skill that targets and shoots projectiles at nearby enemies
/// Can be upgraded to level 10 with improved damage, fire rate, and range
/// Independent skill that doesn't require PlayerSkill_DataSO
/// </summary>
public class PlayerSkill_ExtraHand : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab; // Optional - will auto-create if null
    [SerializeField] private Transform projectileSpawnPoint; // Optional spawn point, defaults to player position
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Auto-Created Projectile Settings")]
    [SerializeField] private bool autoCreateProjectile = true;
    [SerializeField] private float projectileSize = 0.3f;
    [SerializeField] private Color projectileColor = new Color(0f, 1f, 0f, 1f); // Green instead of cyan
    
    [Header("Multi-Projectile Settings")]
    [SerializeField] private int baseProjectileCount = 1;
    [SerializeField] private float spreadAngle = 15f; // Angle between projectiles when multiple
    
    [Header("Base Skill Settings")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseShootInterval = 2f;
    [SerializeField] private float baseRange = 12f;
    
    [Header("Per-Level Upgrades")]
    [SerializeField] private float damagePerLevel = 10f;
    [SerializeField] private float shootIntervalReductionPerLevel = 0.2f; // Reduces interval (faster shooting)
    [SerializeField] private float rangePerLevel = 1f;
    [SerializeField] private float minShootInterval = 0.5f; // Minimum fire rate cap
    
    [Header("Visual Settings")]
    [SerializeField] private bool showDebugRay = true;
    [SerializeField] private Color debugRayColor = Color.cyan;
    
    // State
    private bool isActive = false;
    [Header("Runtime State")]
    [SerializeField] private bool isObtained = false;
    private bool wasObtainedLastFrame = false;
    
    // Current stats (modified by upgrades)
    private float currentDamage;
    private float currentShootInterval;
    private float currentRange;
    private int currentProjectileCount;
    
    // Unified level tracking (max 10)
    private int extraHandLevel = 0;
    private const int MAX_LEVEL = 10;
    
    // Shooting state
    private float lastShotTime = -999f;
    private Coroutine shootingCoroutine;
    
    // Player reference
    private Player player;
    
    private void Awake()
    {
        // Only run in play mode
        if (!Application.isPlaying) return;
        
        // Get player reference
        player = GetComponentInParent<Player>();
        
        InitializeStats();
    }
    
    private void Start()
    {
        // Only run in play mode
        if (!Application.isPlaying) return;
        
        // Validate player reference exists (set by base class Awake)
        if (player == null)
        {
            Debug.LogWarning("[ExtraHand] Player reference is null! Make sure this script is attached to a child of a Player GameObject.");
        }
        
        if (isObtained)
        {
            ActivateSkill();
        }
        wasObtainedLastFrame = isObtained;
    }
    
    private void Update()
    {
        // Only run in play mode
        if (!Application.isPlaying) return;
        
        // Check if isObtained was toggled in Inspector during Play Mode
        if (isObtained != wasObtainedLastFrame)
        {
            if (isObtained)
            {
                ActivateSkill();
            }
            else
            {
                DeactivateSkill();
            }
            wasObtainedLastFrame = isObtained;
        }
    }
    
    /// <summary>
    /// Initialize skill stats to base values
    /// </summary>
    private void InitializeStats()
    {
        currentDamage = baseDamage;
        currentShootInterval = baseShootInterval;
        currentRange = baseRange;
        currentProjectileCount = baseProjectileCount;
    }
    
    /// <summary>
    /// Obtain the skill and auto-activate it
    /// </summary>
    public void ObtainSkill()
    {
        if (isObtained)
        {
            return;
        }
        
        isObtained = true;
        ActivateSkill();
        Debug.Log("[ExtraHand] Skill obtained and activated!");
    }
    
    /// <summary>
    /// Activate the auto-shooting skill
    /// </summary>
    public void ActivateSkill()
    {
        if (isActive) return;
        
        if (!isObtained)
        {
            return;
        }
        
        isActive = true;
        
        // Start auto-shooting coroutine
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
        }
        shootingCoroutine = StartCoroutine(AutoShootCoroutine());
        
        Debug.Log($"[ExtraHand] Skill activated - Damage: {currentDamage}, Interval: {currentShootInterval}s, Range: {currentRange}m");
    }
    
    /// <summary>
    /// Deactivate the auto-shooting skill
    /// </summary>
    public void DeactivateSkill()
    {
        if (!isActive) return;
        
        isActive = false;
        
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }
    
    /// <summary>
    /// Auto-shooting coroutine that continuously shoots at enemies
    /// </summary>
    private IEnumerator AutoShootCoroutine()
    {
        while (isActive)
        {
            // Wait for shoot interval
            yield return new WaitForSeconds(currentShootInterval);
            
            // Find multiple targets (one per projectile)
            List<Transform> targets = FindNearestEnemies(currentProjectileCount);
            if (targets.Count > 0)
            {
                ShootAtTargets(targets);
            }
        }
    }
    
    /// <summary>
    /// Find the nearest enemy within range
    /// </summary>
    private Transform FindNearestEnemy()
    {
        List<Transform> enemies = FindNearestEnemies(1);
        return enemies.Count > 0 ? enemies[0] : null;
    }
    
    /// <summary>
    /// Find multiple nearest enemies within range (one per projectile)
    /// </summary>
    private List<Transform> FindNearestEnemies(int count)
    {
        List<Transform> enemies = new List<Transform>();
        
        if (player == null) return enemies;
        
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, currentRange, enemyLayer);
        
        // Create a list of all alive enemies with their distances
        List<(Transform enemy, float distance)> enemyList = new List<(Transform, float)>();
        
        foreach (Collider col in hitColliders)
        {
            // Check if enemy is alive
            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                float distance = Vector3.Distance(player.transform.position, col.transform.position);
                enemyList.Add((col.transform, distance));
            }
        }
        
        // Sort by distance (closest first)
        enemyList.Sort((a, b) => a.distance.CompareTo(b.distance));
        
        // Take the requested number of enemies
        int enemyCount = Mathf.Min(count, enemyList.Count);
        for (int i = 0; i < enemyCount; i++)
        {
            enemies.Add(enemyList[i].enemy);
        }
        
        return enemies;
    }
    
    /// <summary>
    /// Shoot a projectile at the target (legacy method for single target)
    /// </summary>
    private void ShootAtTarget(Transform target)
    {
        List<Transform> targets = new List<Transform> { target };
        ShootAtTargets(targets);
    }
    
    /// <summary>
    /// Shoot projectiles at multiple targets (one projectile per target)
    /// </summary>
    private void ShootAtTargets(List<Transform> targets)
    {
        if (player == null)
        {
            Debug.LogWarning("[ExtraHand] Player reference is null!");
            return;
        }
        
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[ExtraHand] No targets to shoot at!");
            return;
        }
        
        // Determine spawn position
        Vector3 spawnPosition = projectileSpawnPoint != null 
            ? projectileSpawnPoint.position 
            : player.transform.position + Vector3.up * 2.0f; // Spawn above player at head/top level
        
        // Fire one projectile per target
        int projectilesToFire = Mathf.Min(currentProjectileCount, targets.Count);
        
        for (int i = 0; i < projectilesToFire; i++)
        {
            Transform target = targets[i];
            
            // Calculate direction to this specific target
            Vector3 direction = (target.position - spawnPosition).normalized;
            
            // Create and launch projectile
            GameObject projectileObj = CreateProjectile(spawnPosition, direction);
            
            if (projectileObj == null)
            {
                Debug.LogWarning("[ExtraHand] Failed to create projectile!");
                continue;
            }
            
            // Initialize projectile
            ExtraHandProjectile projectile = projectileObj.GetComponent<ExtraHandProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(direction, projectileSpeed, currentDamage, currentRange, player, enemyLayer);
            }
            else
            {
                Debug.LogWarning("[ExtraHand] Projectile missing ExtraHandProjectile component!");
                Destroy(projectileObj);
                continue;
            }
        }
        
        lastShotTime = Time.time;
    }
    
    /// <summary>
    /// Create a projectile - either from prefab or auto-generate one
    /// </summary>
    private GameObject CreateProjectile(Vector3 position, Vector3 direction)
    {
        GameObject projectileObj;
        
        // Use prefab if assigned
        if (projectilePrefab != null)
        {
            projectileObj = Instantiate(projectilePrefab, position, Quaternion.LookRotation(direction));
            return projectileObj;
        }
        
        // Auto-create projectile if no prefab
        if (!autoCreateProjectile)
        {
            Debug.LogWarning("[ExtraHand] No projectile prefab assigned and auto-create is disabled!");
            return null;
        }
        
        // Create base projectile object
        projectileObj = new GameObject("ExtraHandProjectile");
        projectileObj.transform.position = position;
        projectileObj.transform.rotation = Quaternion.LookRotation(direction);
        
        // Add visual sphere
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.transform.SetParent(projectileObj.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * projectileSize;
        
        // Remove the collider from the visual (we'll add our own)
        Destroy(visual.GetComponent<Collider>());
        
        // Make it glow
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetColor("_Color", projectileColor);
            mat.SetColor("_EmissionColor", projectileColor * 2f);
            mat.EnableKeyword("_EMISSION");
            renderer.material = mat;
        }
        
        // Add Rigidbody FIRST (required by ExtraHandProjectile)
        Rigidbody rb = projectileObj.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Add Collider (trigger)
        SphereCollider col = projectileObj.AddComponent<SphereCollider>();
        col.radius = projectileSize * 1.2f;
        col.isTrigger = true;
        
        // Add ProjectileSlingshot BEFORE ExtraHandProjectile (required component)
        ProjectileSlingshot slingshot = projectileObj.AddComponent<ProjectileSlingshot>();
        slingshot.SetHitMask(enemyLayer); // Set the enemy layer mask
        
        // Add ProjectileTracer BEFORE ExtraHandProjectile (required component)
        ProjectileTracer tracer = projectileObj.AddComponent<ProjectileTracer>();
        
        // Add ExtraHandProjectile script LAST (after all its dependencies)
        ExtraHandProjectile extraHandProj = projectileObj.AddComponent<ExtraHandProjectile>();
        
        // Disable snake motion for auto-created projectiles
        extraHandProj.SetSnakeMotionEnabled(false);
        
        return projectileObj;
    }
    
    #region Upgrade Methods
    
    /// <summary>
    /// Unified level-based upgrade that increases ALL stats
    /// </summary>
    public void UpgradeLevel(float damageIncrease, float intervalReduction, float rangeIncrease)
    {
        if (extraHandLevel >= MAX_LEVEL)
        {
            Debug.LogWarning("[ExtraHand] Already at max level!");
            return;
        }
        
        // Increment level
        extraHandLevel++;
        
        // Increase damage
        currentDamage += damageIncrease;
        
        // Reduce shoot interval (faster shooting)
        currentShootInterval = Mathf.Max(currentShootInterval - intervalReduction, minShootInterval);
        
        // Increase range
        currentRange += rangeIncrease;
        
        // Add +1 projectile on even levels (2, 4, 6, 8, 10)
        if (extraHandLevel % 2 == 0)
        {
            currentProjectileCount++;
            Debug.Log($"[ExtraHand] Added projectile! Now firing {currentProjectileCount} projectiles");
        }
        
        // Restart shooting coroutine with new interval if active
        if (isActive)
        {
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
            }
            shootingCoroutine = StartCoroutine(AutoShootCoroutine());
        }
        
        Debug.Log($"[ExtraHand] Level {extraHandLevel}: Damage={currentDamage:F1}, Interval={currentShootInterval:F2}s, Range={currentRange:F1}m, Projectiles={currentProjectileCount}");
    }
    
    #endregion
    
    #region Public Getters
    
    public bool IsObtained => isObtained;
    public bool IsActive => isActive;
    public float CurrentDamage => currentDamage;
    public float CurrentShootInterval => currentShootInterval;
    public float CurrentRange => currentRange;
    public int CurrentProjectileCount => currentProjectileCount;
    public int ExtraHandLevel => extraHandLevel;
    public int MaxLevel => MAX_LEVEL;
    
    #endregion
    
    /// <summary>
    /// Reset the skill to its original state (used by ResetPlayerData)
    /// Resets isObtained but PRESERVES upgrade level progress
    /// </summary>
    public void ResetSkill()
    {
        isActive = false;
        isObtained = false; // Reset obtained state - must unlock again
        
        // Stop shooting
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
        
        // Reset to base stats
        InitializeStats();
        
        // PRESERVE upgrade level - this carries progress across resets
        
        Debug.Log($"[ExtraHand] Skill reset - Level: {extraHandLevel} (preserved), IsObtained: {isObtained} (reset)");
    }
    
    /// <summary>
    /// Completely reset everything including upgrade progress (for game restart)
    /// </summary>
    public void HardResetSkill()
    {
        isActive = false;
        isObtained = false;
        
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
        
        InitializeStats();
        extraHandLevel = 0;
        
        Debug.Log("[ExtraHand] Hard reset complete - All progress cleared");
    }
    
    private void OnDisable()
    {
        DeactivateSkill();
    }
    
    private void OnDestroy()
    {
        DeactivateSkill();
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (player != null && isObtained)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(player.transform.position, currentRange);
        }
    }
}
