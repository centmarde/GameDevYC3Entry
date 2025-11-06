using UnityEngine;


[CreateAssetMenu(menuName = "Dagitab/Stats/Player Stats Data", fileName = "PlayerStatsData - ")]

public class Player_DataSO : ScriptableObject
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float projectileDamage = 10f;
    public float meleeAttackRange = 1.5f;
    public float rangeAttackRange = 6f;
    public float deathDelay = 0.1f;
    public float moveSpeed = 5f;    
    public float turnSpeed = 10f;
    public float currentSpeedMultiplier = 1.0f;

    //projectile offsets for ranged attacks
    public float muzzleForwardOffset = 0.25f;
    public float muzzleHeightOffset = -0.3f;
    public float DirectionEpsilon = 0.0001f;
    
    [Header("Normal Attack Stats")]
    public float normalAttackSpeed = 50f;
    public float normalAttackRange = 15f;
    
    [Header("Scatter Attack Stats")]
    public int scatterPelletCount = 5;
    public float scatterSpreadAngle = 30f;
    public float scatterPelletSpeed = 30f;
    public float scatterAttackRange = 8f;
    public float scatterProjectileLifetime = 0.25f;
    
    [Header("Charged Attack Stats")]
    public float chargedAttackSpeed = 50f;
    public float chargedAttackRange = 25f;
    public float chargedMaxChargeTime = 2f;
    public float chargedMinChargeMultiplier = 1f;
    public float chargedMaxChargeMultiplier = 2.5f;
    
    [Header("Critical Hit System")]
    [Range(0f, 100f)]
    public float criticalChance = 2f; // Percentage chance for critical hit
    [Range(1f, 5f)]
    public float criticalDamageMultiplier = 2f; // Multiplier for critical damage
    
    [Header("Evasion System")]
    [Range(0f, 100f)]
    public float evasionChance = 5f; // Percentage chance to evade incoming damage
    
    [Header("Defense System")]
    [Range(0f, 100f)]
    public float defense = 0f; // Raw damage absorption amount
    
    [Header("Upgrade Levels")]
    public int damageUpgradeLevel = 0;
    public int maxHealthUpgradeLevel = 0;
    public int criticalChanceUpgradeLevel = 0;
    public int criticalDamageUpgradeLevel = 0;
    public int evasionUpgradeLevel = 0;
    public int defenseUpgradeLevel = 0;
    public const int MaxUpgradeLevel = 10;
    
    /// <summary>
    /// Calculate if an attack is a critical hit
    /// </summary>
    public bool RollCriticalHit()
    {
        return Random.Range(0f, 100f) < criticalChance;
    }
    
    /// <summary>
    /// Calculate final damage with critical hit consideration
    /// </summary>
    public float CalculateDamage(bool isCritical)
    {
        return isCritical ? projectileDamage * criticalDamageMultiplier : projectileDamage;
    }
    
    /// <summary>
    /// Calculate if an attack is evaded
    /// </summary>
    public bool RollEvasion()
    {
        return Random.Range(0f, 100f) < evasionChance;
    }
    
    /// <summary>
    /// Calculate defense absorption from raw damage with percentage reduction
    /// </summary>
    public float CalculateDefenseAbsorption(float incomingDamage)
    {
        // First absorb raw damage up to defense value
        float rawAbsorption = Mathf.Min(defense, incomingDamage);
        float remainingDamage = incomingDamage - rawAbsorption;
        
        // Then reduce remaining damage by 5% per defense level (up to 25% max at level 5+)
        float percentageReduction = Mathf.Min(defenseUpgradeLevel * 5f, 25f) / 100f;
        float percentageAbsorption = remainingDamage * percentageReduction;
        
        float totalAbsorption = rawAbsorption + percentageAbsorption;
        return Mathf.Min(totalAbsorption, incomingDamage); // Never absorb more than incoming damage
    }
}
