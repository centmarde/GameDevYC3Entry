using UnityEngine;

[CreateAssetMenu(menuName = "Dagitab/Stats/Player2 Stats Data", fileName = "Player2StatsData - ")]
public class Player2_DataSO : ScriptableObject
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float projectileDamage = 10f; // Base damage used for dash attack calculations
    public float meleeAttackRange = 2f;
    public float deathDelay = 0.1f;
    public float moveSpeed = 5f;    
    public float turnSpeed = 10f;
    public float currentSpeedMultiplier = 1.0f;
    
    [Header("Charged Dash Attack Stats")]
    [Tooltip("Multiplier applied to projectileDamage for dash base damage")]
    public float dashDamageMultiplier = 2f;
    public float chargedMaxChargeTime = 2f;
    public float chargedMinChargeMultiplier = 1f;
    public float chargedMaxChargeMultiplier = 2.5f;
    public float dashAttackCooldown = 1.5f;
    
    [Header("Dash Attack Hit Detection")]
    public float dashAttackRadius = 1.5f;
    public LayerMask damageableMask;
    
    [Header("Critical Hit System")]
    [Range(0f, 100f)]
    public float criticalChance = 2f;
    [Range(1f, 5f)]
    public float criticalDamageMultiplier = 2f;
    
    [Header("Evasion System")]
    [Range(0f, 100f)]
    public float evasionChance = 5f;
    
    [Header("Defense System")]
    [Range(0f, 100f)]
    public float defense = 0f; // Raw damage absorption amount
    
    [Header("Blink & Dash Movement")]
    public float blinkDistance = 5f;
    public float blinkCooldown = 1f;
    [Tooltip("Speed used for both blink and dash attack movements")]
    public float blinkDashSpeed = 50f;
    
    [Header("Upgrade Levels")]
    public int damageUpgradeLevel = 0;
    public int maxHealthUpgradeLevel = 0;
    public int criticalChanceUpgradeLevel = 0;
    public int criticalDamageUpgradeLevel = 0;
    public int evasionUpgradeLevel = 0;
    public int defenseUpgradeLevel = 0;
    public int blinkDistanceUpgradeLevel = 0;
    public int blinkCooldownUpgradeLevel = 0;
    public int dashCooldownUpgradeLevel = 0;
    public int blinkDashSpeedUpgradeLevel = 0;
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
    public float CalculateDamage(float baseDamage, bool isCritical)
    {
        return isCritical ? baseDamage * criticalDamageMultiplier : baseDamage;
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
