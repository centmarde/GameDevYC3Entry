using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player skill that spawns projectiles circling around the player like a windmill.
/// Damages enemies on contact. Can be toggled on/off and upgraded.
/// </summary>
public class PlayerSkill_CirclingProjectiles : PlayerSkill_Base
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float orbitSpeed = 90f; // Degrees per second
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Upgrade Settings")]
    [SerializeField] private int defaultProjectileCount = 2;
    [SerializeField] private int maxProjectileCount = 8;
    
    // State
    private bool isActive = false;
    [Header("Runtime State")]
    [SerializeField] private bool isObtained = false; // Can be toggled in Inspector for testing
    private bool wasObtainedLastFrame = false; // Track state changes
    private int currentProjectileCount;
    private List<GameObject> activeProjectiles = new List<GameObject>();
    
    // Level-based upgrade system (1-10)
    [SerializeField] private int currentLevel = 0; // 0 = not obtained, 1-10 = skill levels
    private const int MAX_LEVEL = 10;
    
    // Base stats (level 1)
    private float baseDamage;
    private float baseRadius;
    private float baseSpeed;
    private int baseProjectileCount;

    protected override void Awake()
    {
        base.Awake();
        
        // Store base stats for level calculations
        baseDamage = projectileDamage;
        baseRadius = orbitRadius;
        baseSpeed = orbitSpeed;
        baseProjectileCount = defaultProjectileCount;
        currentProjectileCount = defaultProjectileCount;
    }
    
    private void Start()
    {
        // If isObtained is checked in Inspector, auto-activate the skill
        if (isObtained)
        {
            ActivateSkill();
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
                // Just obtained - activate skill and spawn projectiles
                ActivateSkill();
            }
            else
            {
                // Just lost - deactivate skill
                DeactivateSkill();
            }
            wasObtainedLastFrame = isObtained;
        }
    }

    /// <summary>
    /// Obtain the skill and auto-activate it (sets to Level 1)
    /// </summary>
    public void ObtainSkill()
    {
        if (isObtained)
        {
            return;
        }
        
        // Set the runtime-only flag (does NOT persist)
        isObtained = true;
        currentLevel = 1;
        ApplyLevelStats();
        ActivateSkill();
    }

    /// <summary>
    /// Activate the circling projectiles
    /// </summary>
    public void ActivateSkill()
    {
        if (isActive) return;
        
        if (!isObtained)
        {
            return;
        }

        isActive = true;
        SpawnProjectiles();
    }

    /// <summary>
    /// Deactivate and destroy all circling projectiles
    /// </summary>
    public void DeactivateSkill()
    {
        if (!isActive) return;
        
        isActive = false;
        DestroyAllProjectiles();
    }

    /// <summary>
    /// Spawn the circling projectiles around the player
    /// </summary>
    private void SpawnProjectiles()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        // Clear any existing projectiles first
        DestroyAllProjectiles();

        // Calculate angle spacing for even distribution
        float angleStep = 360f / currentProjectileCount;

        for (int i = 0; i < currentProjectileCount; i++)
        {
            // Calculate initial angle for this projectile
            float initialAngle = i * angleStep;

            // Spawn projectile at initial position
            float radians = initialAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(radians) * orbitRadius,
                0f,
                Mathf.Sin(radians) * orbitRadius
            );
            
            Vector3 spawnPosition = player.transform.position + offset;
            GameObject projectileObj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity, player.transform);
            
            // Initialize the projectile
            CirclingProjectile projectile = projectileObj.GetComponent<CirclingProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(
                    player.transform,
                    orbitRadius,
                    orbitSpeed,
                    initialAngle,
                    projectileDamage,
                    player,
                    enemyLayer
                );
            }
            else
            {
                Destroy(projectileObj);
                continue;
            }

            activeProjectiles.Add(projectileObj);
        }
    }

    /// <summary>
    /// Destroy all active projectiles
    /// </summary>
    private void DestroyAllProjectiles()
    {
        foreach (GameObject projectile in activeProjectiles)
        {
            if (projectile != null)
            {
                Destroy(projectile);
            }
        }
        activeProjectiles.Clear();
    }

    /// <summary>
    /// Update all projectiles with new parameters (useful after upgrades)
    /// </summary>
    private void UpdateAllProjectiles()
    {
        foreach (GameObject projectileObj in activeProjectiles)
        {
            if (projectileObj != null)
            {
                CirclingProjectile projectile = projectileObj.GetComponent<CirclingProjectile>();
                if (projectile != null)
                {
                    projectile.SetOrbitRadius(orbitRadius);
                    projectile.SetOrbitSpeed(orbitSpeed);
                    projectile.SetDamage(projectileDamage);
                }
            }
        }
    }

    #region Upgrade Methods

    /// <summary>
    /// Upgrade to the next level (increases all stats)
    /// Level 1: Base stats with 2 projectiles
    /// Level 2-10: Each level increases damage, radius, speed, and adds projectiles
    /// </summary>
    public void UpgradeLevel()
    {
        if (!isObtained || currentLevel >= MAX_LEVEL)
        {
            Debug.LogWarning($"[CirclingProjectiles] Cannot upgrade - Level: {currentLevel}, Obtained: {isObtained}");
            return;
        }

        currentLevel++;
        ApplyLevelStats();
        
        // Update projectiles if skill is active
        if (isActive)
        {
            SpawnProjectiles();
        }
        
        Debug.Log($"[CirclingProjectiles] Upgraded to Level {currentLevel} - Count: {currentProjectileCount}, Damage: {projectileDamage:F1}, Radius: {orbitRadius:F1}, Speed: {orbitSpeed:F0}");
    }
    
    /// <summary>
    /// Apply stats based on current level
    /// Each level increases all stats progressively
    /// </summary>
    private void ApplyLevelStats()
    {
        // Damage scaling: +5 damage per level
        projectileDamage = baseDamage + (currentLevel - 1) * 5f;
        
        // Radius scaling: +0.5 radius per level
        orbitRadius = baseRadius + (currentLevel - 1) * 0.5f;
        
        // Speed scaling: +15 degrees/sec per level
        orbitSpeed = baseSpeed + (currentLevel - 1) * 15f;
        
        // Projectile count: +1 every level, capped at maxProjectileCount
        currentProjectileCount = Mathf.Min(baseProjectileCount + (currentLevel - 1), maxProjectileCount);
    }

    #endregion

    #region Public Getters

    public bool IsObtained => isObtained;
    public bool IsActive => isActive;
    public int CurrentLevel => currentLevel;
    public int MaxLevel => MAX_LEVEL;
    public int CurrentProjectileCount => currentProjectileCount;
    public float CurrentDamage => projectileDamage;
    public float CurrentRadius => orbitRadius;
    public float CurrentSpeed => orbitSpeed;

    #endregion

    /// <summary>
    /// Reset the skill to its original state (used by ResetPlayerData)
    /// </summary>
    public void ResetSkill()
    {
        // Deactivate skill and destroy all projectiles
        isActive = false;
        isObtained = false; // Reset obtained state
        DestroyAllProjectiles();
        
        // Reset to default values from ScriptableObject or serialized fields
        if (Data != null)
        {
            currentProjectileCount = Data.defaultProjectileCount;
            projectileDamage = Data.projectileDamage;
            orbitRadius = Data.orbitRadius;
            orbitSpeed = Data.orbitSpeed;
        }
        else
        {
            // Fallback to serialized default values
            currentProjectileCount = defaultProjectileCount;
        }
        
        // Reset level
        currentLevel = 0;
    }

    private void OnDestroy()
    {
        DestroyAllProjectiles();
    }

    private void OnDisable()
    {
        DestroyAllProjectiles();
        isActive = false;
    }
}
