using UnityEngine;


[CreateAssetMenu(menuName = "Dagitab/Stats/Player Stats Data", fileName = "PlayerStatsData - ")]

public class Player_DataSO : ScriptableObject
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float projectileSpeed = 10f;
    public float projectileDamage = 25f;
    public float meleeAttackRange = 1.5f;
    public float rangeAttackRange = 6f;
    public float deathDelay = 0.1f;
    public float moveSpeed = 5f;    
    public float turnSpeed = 1000f;
    public float currentSpeedMultiplier = 1.0f;
    
    [Header("Critical Hit System")]
    [Range(0f, 100f)]
    public float criticalChance = 15f; // Percentage chance for critical hit
    [Range(1f, 5f)]
    public float criticalDamageMultiplier = 2f; // Multiplier for critical damage
    
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
}
