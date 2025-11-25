using UnityEngine;

/// <summary>
/// Spear Throw skill - spawns 3 projectiles in front of the player's aim direction
/// Level 1-10: Progressively increases damage, reduces cooldown, and improves projectile count
/// Level 1: 3 spears, Level 10: 5 spears with enhanced damage and speed
/// </summary>
public class PlayerSkill_SpearThrow : MonoBehaviour
{
    [Header("Spear Throw Settings")]
    [SerializeField] private GameObject spearProjectilePrefab; // Prefab for spear projectiles
    [SerializeField] private float baseSpearRange = 3f; // Base range in front of player
    [SerializeField] private float baseSpearDamage = 15f; // Base damage per spear
    [SerializeField] private float damagePerLevel = 3f; // Additional damage per level
    [SerializeField] private float baseCooldown = 2f; // Base cooldown in seconds
    [SerializeField] private float cooldownReductionPerLevel = 0.5f; // Cooldown reduction per level
    [SerializeField] private float baseProjectileSpeed = 12f; // Base projectile speed (reduced for boomerang)
    [SerializeField] private float speedIncreasePerLevel = 1f; // Speed increase per level (reduced)
    
    [Header("Spear Formation")]
    [SerializeField] private float spearSpread = 0.8f; // Distance between spears (legacy - now used for cone spread)
    [SerializeField] private int baseSpearCount = 3; // Base number of spears
    [SerializeField] private float baseConeAngle = 15f; // Base cone spread angle in degrees
    [SerializeField] private float coneAnglePerLevel = 5f; // Additional cone angle per level
    [SerializeField] private int projectilesPerLevel = 1; // Additional projectiles every level after first
    [SerializeField] private LayerMask enemyLayerMask = -1; // What layers spears can hit
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject spearThrowEffectPrefab; // Visual effect when throwing
    [SerializeField] private bool useAutoSetup = true; // Auto setup if prefab is null
    
    [Header("Audio")]
    [SerializeField] private AudioClip spearThrowSound;
    [SerializeField] [Range(0f, 1f)] private float audioVolume = 0.7f;
    
    [Header("Runtime State")]
    [SerializeField] private bool isObtained = false;
    private bool wasObtainedLastFrame = false;
    
    // Level tracking (1-10)
    [SerializeField] private int currentLevel = 0; // 0 = not obtained, 1-10 = skill levels
    private const int MAX_LEVEL = 10;
    
    // Current stats
    private float currentSpearDamage;
    private float currentCooldown;
    private float currentProjectileSpeed;
    private int currentSpearCount;
    private float currentConeAngle;
    
    // Player and combat references
    private Player player;
    private Player_Combat playerCombat;
    private AudioSource audioSource;
    
    // Cooldown management
    private float lastCastTime = -999f;
    private bool canCast = true;
    
    // Public accessors
    public bool IsObtained => isObtained;
    public int CurrentLevel => currentLevel;
    public int MaxLevel => MAX_LEVEL;
    public float CurrentDamage => currentSpearDamage;
    public float CurrentCooldown => currentCooldown;
    public int CurrentSpearCount => currentSpearCount;
    public bool CanCast => canCast && (Time.time - lastCastTime >= currentCooldown);
    
    private void Awake()
    {
        player = GetComponentInParent<Player>();
        playerCombat = GetComponentInParent<Player_Combat>();
        audioSource = GetComponent<AudioSource>();
        
        if (player == null)
        {
            Debug.LogWarning("[SpearThrow] Player reference is null! Make sure this script is attached to a child of a Player GameObject.");
        }
        
        if (playerCombat == null)
        {
            Debug.LogWarning("[SpearThrow] Player_Combat reference is null! Spear damage might not scale with player stats.");
        }
        
        // Initialize audio source
        if (audioSource == null && spearThrowSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
    }
    
    private void Start()
    {
        // Auto-setup if no prefab provided
        if (useAutoSetup && spearProjectilePrefab == null)
        {
            SetupDefaultSpearPrefab();
        }
        
        UpdateSkillStats();
    }
    
    private void Update()
    {
        // Handle skill activation (automatic casting every cooldown when obtained)
        if (isObtained && CanCast)
        {
            CastSpearThrow();
        }
        
        // Handle obtain state changes
        if (isObtained != wasObtainedLastFrame)
        {
            OnObtainedStateChanged();
            wasObtainedLastFrame = isObtained;
        }
    }
    
    /// <summary>
    /// Obtain the skill (unlock at level 1)
    /// </summary>
    public void ObtainSkill()
    {
        if (!isObtained)
        {
            isObtained = true;
            currentLevel = 1;
            UpdateSkillStats();
        }
    }
    
    /// <summary>
    /// Upgrade the skill to the next level
    /// </summary>
    public void UpgradeSkill()
    {
        if (isObtained && currentLevel < MAX_LEVEL)
        {
            currentLevel++;
            UpdateSkillStats();
        }
    }
    
    /// <summary>
    /// Update skill statistics based on current level
    /// </summary>
    private void UpdateSkillStats()
    {
        if (currentLevel <= 0) return;
        
        // Calculate current damage (scales with player damage if available)
        float baseDmg = baseSpearDamage + (damagePerLevel * (currentLevel - 1));
        if (player != null && player.Stats != null)
        {
            // Scale damage based on player's projectile damage
            float damageMultiplier = player.Stats.projectileDamage / 10f; // Normalize against base 10 damage
            currentSpearDamage = baseDmg * damageMultiplier;
        }
        else
        {
            currentSpearDamage = baseDmg;
        }
        
        // Calculate current cooldown
        currentCooldown = Mathf.Max(0.5f, baseCooldown - (cooldownReductionPerLevel * (currentLevel - 1)));
        
        // Calculate current projectile speed
        currentProjectileSpeed = baseProjectileSpeed + (speedIncreasePerLevel * (currentLevel - 1));
        
        // Calculate current spear count (increases every level)
        // Level 1: 3 spears, Level 2: 4 spears, etc.
        currentSpearCount = baseSpearCount + (currentLevel - 1) * projectilesPerLevel;
        currentSpearCount = Mathf.Min(currentSpearCount, baseSpearCount + 7); // Cap at reasonable limit
        
        // Calculate current cone angle (wider spread at higher levels)
        currentConeAngle = baseConeAngle + (coneAnglePerLevel * (currentLevel - 1));
        currentConeAngle = Mathf.Min(currentConeAngle, 60f); // Cap at 60 degrees total spread
    }
    
    /// <summary>
    /// Cast the spear throw skill
    /// </summary>
    private void CastSpearThrow()
    {
        if (!isObtained || !CanCast || spearProjectilePrefab == null) return;
        
        // Safety check - ensure spear count is valid
        if (currentSpearCount <= 0)
        {
            UpdateSkillStats();
        }
        
        if (currentSpearCount <= 0) return;
        
        // Get player aim direction
        Vector3 aimDirection = GetPlayerAimDirection();
        Vector3 spawnPosition = transform.position + aimDirection * 1f; // Spawn 1 unit in front
        
        // Spawn spears in formation
        SpawnSpearFormation(spawnPosition, aimDirection);
        
        // Play audio and visual effects
        PlaySpearThrowEffects();
        
        // Update cooldown
        lastCastTime = Time.time;
        canCast = true;
    }
    
    /// <summary>
    /// Spawn spears in cone formation in front of the player
    /// </summary>
    private void SpawnSpearFormation(Vector3 centerPosition, Vector3 direction)
    {
        if (currentSpearCount <= 0 || spearProjectilePrefab == null) return;
        
        // For single spear, shoot straight
        if (currentSpearCount == 1)
        {
            CreateSpearProjectile(centerPosition, direction);
            return;
        }
        
        // Calculate angle step between spears for cone formation
        float halfConeAngle = currentConeAngle * 0.5f;
        float angleStep = currentConeAngle / (currentSpearCount - 1);
        
        for (int i = 0; i < currentSpearCount; i++)
        {
            // Calculate angle for this spear (-halfCone to +halfCone)
            float angle = -halfConeAngle + (i * angleStep);
            
            // Rotate the base direction by the calculated angle around Y axis
            Vector3 spearDirection = Quaternion.AngleAxis(angle, Vector3.up) * direction;
            
            // Create spear projectile
            CreateSpearProjectile(centerPosition, spearDirection);
        }
    }
    
    /// <summary>
    /// Create a single spear projectile with given direction
    /// </summary>
    private void CreateSpearProjectile(Vector3 position, Vector3 direction)
    {
        if (spearProjectilePrefab == null) return;
        
        // Create spear projectile
        GameObject spear = Instantiate(spearProjectilePrefab, position, Quaternion.LookRotation(direction));
        spear.SetActive(true);
        
        // Configure spear projectile
        var projectile = spear.GetComponent<ProjectileSlingshot>();
        if (projectile != null)
        {
            projectile.SetHitMask(enemyLayerMask);
            
            // Configure for boomerang behavior - spear projectile will handle its own destruction
            var spearComponent = spear.GetComponent<SpearProjectile>();
            if (spearComponent != null)
            {
                // Let the SpearProjectile handle destruction when it returns to player
                projectile.SetLifeTime(10f); // Fallback safety timeout
            }
            else
            {
                // Fallback for spears without SpearProjectile component
                projectile.SetLifeTime(4f);
            }
            
            // Calculate damage (with potential critical hit)
            float finalDamage = currentSpearDamage;
            bool isCritical = false;
            
            if (player != null && player.Stats != null)
            {
                isCritical = player.Stats.RollCriticalHit();
                if (isCritical)
                {
                    finalDamage *= player.Stats.criticalDamageMultiplier;
                }
            }
            
            // Launch the spear
            Vector3 velocity = direction * currentProjectileSpeed;
            projectile.Launch(velocity, finalDamage, player, isCritical);
        }
        else
        {
            // Destroy the created object if no ProjectileSlingshot component
            if (spear != null) Destroy(spear);
        }
    }
    
    /// <summary>
    /// Get the player's aim direction
    /// </summary>
    private Vector3 GetPlayerAimDirection()
    {
        if (player == null) return transform.forward;
        
        // Try to get direction from player input or movement
        Vector3 moveDirection = Vector3.zero;
        
        // Check if player has movement component
        var playerMovement = player.GetComponent<Player_Movement>();
        if (playerMovement != null)
        {
            moveDirection = playerMovement.lastMoveDir;
        }
        
        // Fallback to transform forward if no movement input
        if (moveDirection.magnitude < 0.1f)
        {
            moveDirection = transform.forward;
        }
        
        // Ensure direction is normalized and on horizontal plane
        moveDirection.y = 0f;
        return moveDirection.normalized;
    }
    
    /// <summary>
    /// Play audio and visual effects for spear throw
    /// </summary>
    private void PlaySpearThrowEffects()
    {
        // Play sound effect
        if (audioSource != null && spearThrowSound != null)
        {
            audioSource.PlayOneShot(spearThrowSound, audioVolume);
        }
        
        // Play visual effect
        if (spearThrowEffectPrefab != null)
        {
            GameObject effect = Instantiate(spearThrowEffectPrefab, transform.position, transform.rotation);
            Destroy(effect, 2f); // Auto-cleanup effect
        }
    }
    
    /// <summary>
    /// Setup default spear prefab if none provided
    /// </summary>
    private void SetupDefaultSpearPrefab()
    {
        // Try to find existing projectile prefab in resources or scene
        var existingProjectile = FindObjectOfType<ProjectileSlingshot>();
        if (existingProjectile != null)
        {
            spearProjectilePrefab = existingProjectile.gameObject;
        }
        else
        {
            CreateBasicSpearPrefab();
        }
    }
    
    private void CreateBasicSpearPrefab()
    {
        // Create a basic visible spear prefab
        GameObject basicSpear = new GameObject("BasicSpear");
        
        // Add basic components
        basicSpear.AddComponent<ProjectileSlingshot>();
        basicSpear.AddComponent<Rigidbody>().isKinematic = true;
        basicSpear.AddComponent<SphereCollider>().isTrigger = true;
        
        // Add a visible primitive
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "SpearVisual";
        visual.transform.SetParent(basicSpear.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
        visual.transform.localRotation = Quaternion.Euler(0, 0, 90); // Orient like spear
        
        // Make it yellow so it's visible
        var renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = Color.yellow;
        }
        
        spearProjectilePrefab = basicSpear;
    }
    
    /// <summary>
    /// Called when obtained state changes
    /// </summary>
    private void OnObtainedStateChanged()
    {
        // Skill state changed - can add effects here if needed
    }
    
    /// <summary>
    /// Force update stats (called when player stats change)
    /// </summary>
    public void ForceUpdateStats()
    {
        UpdateSkillStats();
    }
    
    /// <summary>
    /// Manual test method for debugging - call this from inspector or console
    /// </summary>
    [ContextMenu("Test Spear Throw")]
    public void TestSpearThrow()
    {
        if (!isObtained)
        {
            ObtainSkill();
        }
        
        CastSpearThrow();
    }

    // Debug information
    private void OnDrawGizmos()
    {
        if (!isObtained) return;
        
        // Draw spear throw range and formation
        Vector3 aimDirection = GetPlayerAimDirection();
        Vector3 centerPosition = transform.position + aimDirection * 1f;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPosition, 0.2f);
        
        // Draw cone formation
        if (currentSpearCount > 0)
        {
            Gizmos.color = Color.red;
            
            if (currentSpearCount == 1)
            {
                // Single spear - straight line
                Gizmos.DrawLine(centerPosition, centerPosition + aimDirection * baseSpearRange);
            }
            else
            {
                // Multiple spears - cone formation
                float halfConeAngle = currentConeAngle * 0.5f;
                float angleStep = currentConeAngle / (currentSpearCount - 1);
                
                for (int i = 0; i < currentSpearCount; i++)
                {
                    float angle = -halfConeAngle + (i * angleStep);
                    Vector3 spearDirection = Quaternion.AngleAxis(angle, Vector3.up) * aimDirection;
                    
                    Gizmos.DrawLine(centerPosition, centerPosition + spearDirection * baseSpearRange);
                    Gizmos.DrawWireSphere(centerPosition + spearDirection * 0.5f, 0.1f);
                }
                
                // Draw cone outline
                Gizmos.color = Color.yellow;
                Vector3 leftEdge = Quaternion.AngleAxis(-halfConeAngle, Vector3.up) * aimDirection;
                Vector3 rightEdge = Quaternion.AngleAxis(halfConeAngle, Vector3.up) * aimDirection;
                Gizmos.DrawLine(centerPosition, centerPosition + leftEdge * baseSpearRange);
                Gizmos.DrawLine(centerPosition, centerPosition + rightEdge * baseSpearRange);
            }
        }
    }
}