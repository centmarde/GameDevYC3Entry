using UnityEngine;

/// <summary>
/// Player defensive skill that absorbs incoming damage and reflects it back to enemies.
/// Passive skill with level-based damage absorption (1-10).
/// Independent skill with no ScriptableObject dependency.
/// </summary>
public class PlayerSkill_Defense : MonoBehaviour
{
    [Header("Defense Settings")]
    [SerializeField] private float damageAbsorptionPercent = 20f; // Base 20% at level 1
    [SerializeField] private float absorptionChance = 100f; // Always 100% chance
    [SerializeField] private float reflectDamagePercent = 100f; // 100% of absorbed damage is reflected back
    
    [Header("Reflection Settings")]
    [SerializeField] private float reflectionRadius = 5f; // Radius to find nearby enemies for reflection
    [SerializeField] private LayerMask enemyLayer; // Enemy layer mask
    [SerializeField] private bool useTagFallback = true; // Use tag if layer not set
    [SerializeField] private string enemyTag = "Enemy"; // Enemy tag for fallback
    
    [Header("Runtime State")]
    [SerializeField] private bool isObtained = false;
    private bool wasObtainedLastFrame = false;
    
    // Level-based upgrade system (1-10)
    [SerializeField] private int currentLevel = 0; // 0 = not obtained, 1-10 = skill levels
    private const int MAX_LEVEL = 10;
    
    // Base stats (level 1)
    private const float BASE_ABSORPTION = 20f; // 20% at level 1
    private const float MAX_ABSORPTION = 80f; // 80% at level 10
    
    // Public accessors
    public bool IsObtained => isObtained;
    public int CurrentLevel => currentLevel;
    public float DamageAbsorptionPercent => damageAbsorptionPercent;
    public float AbsorptionChance => absorptionChance;
    
    private void Start()
    {
        if (isObtained)
        {
            currentLevel = 1;
            ApplyLevelStats();
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
            }
            else
            {
                currentLevel = 0;
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
        
        Debug.Log($"[Defense] Skill obtained at Level 1! Absorption: {damageAbsorptionPercent:F1}%");
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
            Debug.LogWarning($"[Defense] Already at MAX level ({MAX_LEVEL})");
            return;
        }
        
        currentLevel++;
        ApplyLevelStats();
        
        Debug.Log($"[Defense] Upgraded to Level {currentLevel} - Absorption: {damageAbsorptionPercent:F1}%");
    }
    
    /// <summary>
    /// Apply stats based on current level
    /// </summary>
    private void ApplyLevelStats()
    {
        if (currentLevel <= 0)
        {
            damageAbsorptionPercent = 0f;
            return;
        }
        
        // Linear scaling from 20% (level 1) to 80% (level 10)
        // Formula: absorption = BASE_ABSORPTION + (level - 1) * increment
        // Where increment = (MAX_ABSORPTION - BASE_ABSORPTION) / (MAX_LEVEL - 1)
        float increment = (MAX_ABSORPTION - BASE_ABSORPTION) / (MAX_LEVEL - 1);
        damageAbsorptionPercent = BASE_ABSORPTION + (currentLevel - 1) * increment;
    }
    
    /// <summary>
    /// Process incoming damage and return the absorbed amount
    /// Called by the player's damage system
    /// </summary>
    /// <param name="incomingDamage">The original damage amount</param>
    /// <param name="hitPosition">Position where damage was dealt (for UI indicator)</param>
    /// <param name="damageSource">The source of the damage (optional, used to reflect damage back)</param>
    /// <returns>The amount of damage absorbed</returns>
    public float ProcessIncomingDamage(float incomingDamage, Vector3 hitPosition, object damageSource = null)
    {
        if (!isObtained || currentLevel <= 0)
        {
            return 0f; // No absorption if skill not obtained
        }
        
        // 100% chance to absorb
        if (Random.Range(0f, 100f) <= absorptionChance)
        {
            float absorbedDamage = incomingDamage * (damageAbsorptionPercent / 100f);
            
            // Show absorption indicator
            DefenseAbsorbIndicator.ShowAbsorption(hitPosition, absorbedDamage);
            
            // Reflect damage back to attacker or nearby enemies
            ReflectDamage(absorbedDamage, hitPosition, damageSource);
            
            Debug.Log($"[Defense] Absorbed {absorbedDamage:F1} damage ({damageAbsorptionPercent:F1}% of {incomingDamage:F1})");
            
            return absorbedDamage;
        }
        
        return 0f;
    }
    
    /// <summary>
    /// Reflect absorbed damage back to enemies
    /// </summary>
    private void ReflectDamage(float absorbedDamage, Vector3 hitPosition, object damageSource)
    {
        float reflectDamage = absorbedDamage * (reflectDamagePercent / 100f);
        
        if (reflectDamage <= 0f)
        {
            return;
        }
        
        // Try to damage the source first if it's an IDamageable
        if (damageSource != null && damageSource is IDamageable sourceDamageable)
        {
            if (sourceDamageable.IsAlive)
            {
                Vector3 reflectDirection = (transform.position - hitPosition).normalized;
                sourceDamageable.TakeDamage(reflectDamage, hitPosition, reflectDirection, this);
                Debug.Log($"[Defense] Reflected {reflectDamage:F1} damage back to attacker!");
                return;
            }
        }
        
        // If no valid source, find nearby enemies to reflect damage to
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, reflectionRadius, enemyLayer);
        
        if (nearbyColliders.Length == 0 && useTagFallback)
        {
            // Fallback to tag-based search if layer search found nothing
            nearbyColliders = Physics.OverlapSphere(transform.position, reflectionRadius);
        }
        
        // Find the closest enemy and reflect damage
        IDamageable closestEnemy = null;
        float closestDistance = float.MaxValue;
        Vector3 closestEnemyPosition = Vector3.zero;
        
        foreach (Collider col in nearbyColliders)
        {
            // Check if it's an enemy (by tag if using fallback)
            if (useTagFallback && enemyLayer == 0)
            {
                if (!col.CompareTag(enemyTag))
                    continue;
            }
            
            // Try to get IDamageable component
            IDamageable enemy = col.GetComponentInParent<IDamageable>();
            if (enemy != null && enemy.IsAlive)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                    closestEnemyPosition = col.transform.position;
                }
            }
        }
        
        // Reflect damage to closest enemy
        if (closestEnemy != null)
        {
            Vector3 reflectDirection = (closestEnemyPosition - transform.position).normalized;
            closestEnemy.TakeDamage(reflectDamage, closestEnemyPosition, reflectDirection, this);
            Debug.Log($"[Defense] Reflected {reflectDamage:F1} damage to nearby enemy!");
        }
    }
    
    /// <summary>
    /// Get current stats info
    /// </summary>
    public void PrintStats()
    {
        Debug.Log($"[Defense] Level {currentLevel} - Absorption: {damageAbsorptionPercent:F1}%, Chance: {absorptionChance:F0}%");
    }
}
