using UnityEngine;

public class Player_ScatterRangeAttack : Player_RangeAttack
{
    [Header("Scatter Damage Falloff")]
    [SerializeField] private float pointBlankRange = 3f; // Distance for double damage (point blank)
    [SerializeField] private float maxDamageRange = 15f; // Distance for full damage
    [SerializeField] private float minDamageRange = 30f; // Distance where damage becomes minimum
    [SerializeField] private float minDamageMultiplier = 0.2f; // Minimum damage at max range (20% of base)
    [SerializeField] private float pointBlankMultiplier = 3f; // Damage multiplier at point blank range (200%)

    // Override to use scatter-specific range from Player_DataSO
    public override float AttackRange => player.Stats.scatterAttackRange;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void FireProjectile()
    {
        Debug.Log($"[ScatterRangeAttack] FireProjectile called on Instance {GetInstanceID()} - firing {player.Stats.scatterPelletCount} pellets");
        
        if (projectile == null || muzzle == null)
        {
            Debug.LogError($"[ScatterRangeAttack] Cannot fire - Projectile: {projectile}, Muzzle: {muzzle}");
            return;
        }

        // Safety check: use muzzle forward if cached direction is invalid
        Vector3 baseDir = cachedDirection.sqrMagnitude > DirectionEpsilon 
            ? cachedDirection 
            : muzzle.forward;
        baseDir.Normalize();

        // Calculate base damage and speed with buffs from Player_DataSO
        // Scatter damage is base projectile damage divided by pellet count
        float basePelletDamage = (player.Stats.projectileDamage + damageBonus) / player.Stats.scatterPelletCount;
        float pelletSpeed = player.Stats.scatterPelletSpeed + speedBonus;

        // Roll once for critical hit for entire scatter group
        bool hasCritical = player.Stats.RollCriticalHit();
        int criticalPelletIndex = hasCritical ? Random.Range(0, player.Stats.scatterPelletCount) : -1;

        for (int i = 0; i < player.Stats.scatterPelletCount; i++)
        {
            // Only the randomly selected pellet in this group can be critical
            bool isCritical = (i == criticalPelletIndex);
            float pelletDamage = isCritical ? basePelletDamage * player.Stats.criticalDamageMultiplier : basePelletDamage;
            
            // Slight random Y-axis rotation to simulate spread
            Quaternion spreadRot = Quaternion.Euler(0f, Random.Range(-player.Stats.scatterSpreadAngle, player.Stats.scatterSpreadAngle), 0f);
            Vector3 dir = spreadRot * baseDir;

            Vector3 spawnPos = muzzle.position + dir.normalized * muzzleForwardOffset;
            spawnPos.y -= muzzleHeightOffset;

            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            ProjectileSlingshot p = Instantiate(projectile, spawnPos, rot);
            p.transform.SetParent(null, true);

            var projCol = p.GetComponent<Collider>();
            if (projCol && player)
            {
                foreach (var c in player.GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(projCol, c, true);
            }

            // Add damage falloff component to handle distance-based damage
            ScatterPelletDamageFalloff falloff = p.gameObject.AddComponent<ScatterPelletDamageFalloff>();
            falloff.Initialize(spawnPos, pointBlankRange, maxDamageRange, minDamageRange, minDamageMultiplier, pointBlankMultiplier);

            p.Launch(dir.normalized * pelletSpeed, pelletDamage, this, isCritical);
        }
    }
}

/// <summary>
/// Component that handles damage falloff based on distance traveled for scatter pellets
/// Attach this to each scatter pellet to apply distance-based damage reduction
/// </summary>
public class ScatterPelletDamageFalloff : MonoBehaviour
{
    private Vector3 spawnPosition;
    private float pointBlankRange;
    private float maxDamageRange;
    private float minDamageRange;
    private float minDamageMultiplier;
    private float pointBlankMultiplier;
    private float baseDamage;
    
    public void Initialize(Vector3 spawnPos, float pbRange, float maxRange, float minRange, float minMultiplier, float pbMultiplier)
    {
        spawnPosition = spawnPos;
        pointBlankRange = pbRange;
        maxDamageRange = maxRange;
        minDamageRange = minRange;
        minDamageMultiplier = minMultiplier;
        pointBlankMultiplier = pbMultiplier;
        
        ProjectileSlingshot projectile = GetComponent<ProjectileSlingshot>();
        if (projectile != null)
        {
            baseDamage = projectile.GetDamage();
        }
    }
    
    /// <summary>
    /// Calculate and return the damage multiplier based on current distance traveled
    /// Called by the projectile when it hits something
    /// </summary>
    public float GetDamageMultiplier()
    {
        float distanceTraveled = Vector3.Distance(spawnPosition, transform.position);
        return CalculateDamageFalloff(distanceTraveled);
    }
    
    private float CalculateDamageFalloff(float distance)
    {
        // Point blank range - DOUBLE damage
        if (distance <= pointBlankRange)
        {
            return pointBlankMultiplier;
        }
        
        // Transition from point blank to full damage
        if (distance <= maxDamageRange)
        {
            // Linear interpolation from point blank multiplier to 1.0
            float t = (distance - pointBlankRange) / (maxDamageRange - pointBlankRange);
            return Mathf.Lerp(pointBlankMultiplier, 1f, t);
        }
        
        // Minimum damage at long range
        if (distance >= minDamageRange)
        {
            return minDamageMultiplier;
        }
        
        // Linear interpolation between max and min range
        float t2 = (distance - maxDamageRange) / (minDamageRange - maxDamageRange);
        return Mathf.Lerp(1f, minDamageMultiplier, t2);
    }
}
