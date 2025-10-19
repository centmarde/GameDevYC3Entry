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
    
    // Upgrade levels
    private int projectileCountLevel = 0;
    private int damageLevel = 0;
    private int radiusLevel = 0;
    private int speedLevel = 0;

    protected override void Awake()
    {
        base.Awake();
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
    /// Obtain the skill and auto-activate it
    /// </summary>
    public void ObtainSkill()
    {
        if (isObtained)
        {
            return;
        }
        
        // Set the runtime-only flag (does NOT persist)
        isObtained = true;
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
    /// Upgrade: Add more projectiles
    /// </summary>
    public void UpgradeProjectileCount()
    {
        if (currentProjectileCount >= maxProjectileCount)
        {
            return;
        }

        projectileCountLevel++;
        currentProjectileCount++;
        
        // Respawn projectiles if skill is active
        if (isActive)
        {
            SpawnProjectiles();
        }
    }

    /// <summary>
    /// Upgrade: Increase damage
    /// </summary>
    public void UpgradeDamage(float damageIncrease = 5f)
    {
        damageLevel++;
        projectileDamage += damageIncrease;
        
        // Update existing projectiles
        if (isActive)
        {
            UpdateAllProjectiles();
        }
    }

    /// <summary>
    /// Upgrade: Increase orbit radius
    /// </summary>
    public void UpgradeRadius(float radiusIncrease = 0.5f)
    {
        radiusLevel++;
        orbitRadius += radiusIncrease;
        
        // Update existing projectiles
        if (isActive)
        {
            UpdateAllProjectiles();
        }
    }

    /// <summary>
    /// Upgrade: Increase orbit speed
    /// </summary>
    public void UpgradeSpeed(float speedIncrease = 15f)
    {
        speedLevel++;
        orbitSpeed += speedIncrease;
        
        // Update existing projectiles
        if (isActive)
        {
            UpdateAllProjectiles();
        }
    }

    #endregion

    #region Public Getters

    public bool IsObtained => isObtained;
    public bool IsActive => isActive;
    public int CurrentProjectileCount => currentProjectileCount;
    public float CurrentDamage => projectileDamage;
    public float CurrentRadius => orbitRadius;
    public float CurrentSpeed => orbitSpeed;
    
    public int ProjectileCountLevel => projectileCountLevel;
    public int DamageLevel => damageLevel;
    public int RadiusLevel => radiusLevel;
    public int SpeedLevel => speedLevel;

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
        
        // Reset upgrade levels
        projectileCountLevel = 0;
        damageLevel = 0;
        radiusLevel = 0;
        speedLevel = 0;
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
