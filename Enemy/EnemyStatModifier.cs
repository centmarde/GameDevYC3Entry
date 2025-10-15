using UnityEngine;

/// <summary>
/// Component that applies runtime stat bonuses to enemies spawned during waves.
/// This is automatically added by the WaveSpawner when wave-based scaling is active.
/// </summary>
public class EnemyStatModifier : MonoBehaviour
{
    [Header("Stat Bonuses")]
    [Tooltip("Additional damage added to this enemy's attacks")]
    public float damageBonus = 0f;
    
    [Tooltip("Additional health added to this enemy (applied at spawn)")]
    public float healthBonus = 0f;
    
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
    /// Check if this enemy has any stat modifiers active
    /// </summary>
    public bool HasModifiers()
    {
        return damageBonus > 0f || healthBonus > 0f;
    }
}
