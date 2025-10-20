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
    
    [Header("Blink & Dash Movement")]
    public float blinkDistance = 5f;
    public float blinkCooldown = 1f;
    [Tooltip("Speed used for both blink and dash attack movements")]
    public float blinkDashSpeed = 50f;
    
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
}
