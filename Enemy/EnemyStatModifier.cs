using UnityEngine;

/// <summary>
/// Component that applies runtime stat bonuses to enemies spawned during waves.
/// This is automatically added by the WaveSpawner when wave-based scaling is active.
/// </summary>
public class EnemyStatModifier : MonoBehaviour
{
    [Header("Wave Scaling Bonuses")]
    [Tooltip("Additional damage added to this enemy's attacks")]
    public float damageBonus = 0f;
    
    [Tooltip("Additional health added to this enemy (applied at spawn)")]
    public float healthBonus = 0f;
    
    [Header("Minion Wave Multipliers")]
    [Tooltip("Move speed multiplier for minions (1.0 = normal, 1.5 = 50% faster)")]
    [Range(0.5f, 2f)]
    public float moveSpeedMultiplier = 1f;
    
    [Tooltip("Attack cooldown multiplier for minions (1.0 = normal, 0.5 = 2x faster attacks)")]
    [Range(0.5f, 1.5f)]
    public float attackCooldownMultiplier = 1f;
    
    private Enemy_Minions minion;
    private Enemy_Minions_Movement minionMovement;
    private bool statsApplied = false;
    
    private void Start()
    {
        // Check if this is a minion and apply multipliers
        minion = GetComponent<Enemy_Minions>();
        if (minion != null && !statsApplied)
        {
            ApplyMinionStatMultipliers();
        }
    }
    
    /// <summary>
    /// Apply stat multipliers to minion
    /// </summary>
    private void ApplyMinionStatMultipliers()
    {
        if (minion == null || minion.MinionStats == null) return;
        
        minionMovement = GetComponent<Enemy_Minions_Movement>();
        
        // Apply move speed multiplier
        if (moveSpeedMultiplier != 1f && minionMovement != null)
        {
            // Modify the base move speed in the movement component
            float originalSpeed = minion.MinionStats.moveSpeed;
            float newSpeed = originalSpeed * moveSpeedMultiplier;
            
            // Note: Since we can't directly modify SO values, we'll store the multiplier
            // and the movement component will use GetModifiedMoveSpeed() method
            Debug.Log($"[EnemyStatModifier] {gameObject.name} move speed: {originalSpeed} -> {newSpeed} (×{moveSpeedMultiplier:F2})");
        }
        
        // Attack cooldown is handled by GetModifiedAttackCooldown() when damage is dealt
        if (attackCooldownMultiplier != 1f)
        {
            Debug.Log($"[EnemyStatModifier] {gameObject.name} attack cooldown multiplier: ×{attackCooldownMultiplier:F2}");
        }
        
        statsApplied = true;
    }
    
    /// <summary>
    /// Get the total damage including bonuses
    /// </summary>
    /// <param name="baseDamage">The base damage value from the enemy's stats</param>
    /// <returns>Total damage including bonuses</returns>
    public float GetModifiedDamage(float baseDamage)
    {
        return baseDamage + damageBonus;
    }
    
    /// <summary>
    /// Get the modified move speed for minions
    /// </summary>
    /// <param name="baseMoveSpeed">Base move speed from stats</param>
    /// <returns>Modified move speed</returns>
    public float GetModifiedMoveSpeed(float baseMoveSpeed)
    {
        return baseMoveSpeed * moveSpeedMultiplier;
    }
    
    /// <summary>
    /// Get the modified attack cooldown for minions
    /// </summary>
    /// <param name="baseAttackCooldown">Base attack cooldown from stats</param>
    /// <returns>Modified attack cooldown</returns>
    public float GetModifiedAttackCooldown(float baseAttackCooldown)
    {
        return baseAttackCooldown * attackCooldownMultiplier;
    }
    
    /// <summary>
    /// Check if this enemy has any stat modifiers active
    /// </summary>
    public bool HasModifiers()
    {
        return damageBonus > 0f || healthBonus > 0f || moveSpeedMultiplier != 1f || attackCooldownMultiplier != 1f;
    }
}
