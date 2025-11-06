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
    [SerializeField] private float spearSpread = 0.8f; // Distance between spears
    [SerializeField] private int baseSpearCount = 3; // Base number of spears
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
        
        // Calculate current spear count (bonus spear every 3 levels: 3 → 4 → 5)
        // Level 1-2: 3 spears, Level 3-5: 4 spears, Level 6+: 5 spears
        int bonusSpears = (currentLevel - 1) / 3; // 0 at levels 1-2, 1 at levels 3-5, 2 at levels 6+
        currentSpearCount = baseSpearCount + bonusSpears;
        currentSpearCount = Mathf.Min(currentSpearCount, 5); // Cap at 5 spears max
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
    /// Spawn spears in formation in front of the player
    /// </summary>
    private void SpawnSpearFormation(Vector3 centerPosition, Vector3 direction)
    {
        if (currentSpearCount <= 0 || spearProjectilePrefab == null) return;
        
        // Calculate perpendicular direction for spread
        Vector3 rightDirection = Vector3.Cross(direction, Vector3.up).normalized;
        
        // Calculate starting offset for centering the formation
        float totalWidth = (currentSpearCount - 1) * spearSpread;
        float startOffset = -totalWidth * 0.5f;
        
        for (int i = 0; i < currentSpearCount; i++)
        {
            // Calculate position for this spear
            float offsetFromCenter = startOffset + (i * spearSpread);
            Vector3 spearPosition = centerPosition + rightDirection * offsetFromCenter;
            
            // Create spear projectile
            GameObject spear = Instantiate(spearProjectilePrefab, spearPosition, Quaternion.LookRotation(direction));
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
        
        // Draw spear positions
        if (currentSpearCount > 0)
        {
            Vector3 rightDirection = Vector3.Cross(aimDirection, Vector3.up).normalized;
            float totalWidth = (currentSpearCount - 1) * spearSpread;
            float startOffset = -totalWidth * 0.5f;
            
            Gizmos.color = Color.red;
            for (int i = 0; i < currentSpearCount; i++)
            {
                float offsetFromCenter = startOffset + (i * spearSpread);
                Vector3 spearPosition = centerPosition + rightDirection * offsetFromCenter;
                Gizmos.DrawWireSphere(spearPosition, 0.1f);
                Gizmos.DrawLine(spearPosition, spearPosition + aimDirection * baseSpearRange);
            }
        }
    }
}