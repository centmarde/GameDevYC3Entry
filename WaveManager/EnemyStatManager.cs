using UnityEngine;

/// <summary>
/// Manages stat bonuses and modifications for spawned enemies
/// </summary>
[System.Serializable]
public class EnemyStatManager
{
    private float currentHealthBonus = 0f;
    private float currentDamageBonus = 0f;
    private float currentMoveSpeedMultiplier = 1f;
    private float currentAttackCooldownMultiplier = 1f;
    
    /// <summary>
    /// Set current wave stat bonuses
    /// </summary>
    public void SetWaveBonuses(float healthBonus, float damageBonus, float moveSpeedMultiplier = 1f, float attackCooldownMultiplier = 1f)
    {
        currentHealthBonus = healthBonus;
        currentDamageBonus = damageBonus;
        currentMoveSpeedMultiplier = moveSpeedMultiplier;
        currentAttackCooldownMultiplier = attackCooldownMultiplier;
        
        Debug.Log($"[EnemyStatManager] Wave bonuses set - Health: +{healthBonus}, Damage: +{damageBonus}, Speed: ×{moveSpeedMultiplier}, AttackCD: ×{attackCooldownMultiplier}");
    }
    
    /// <summary>
    /// Apply stat bonuses to a spawned enemy
    /// </summary>
    public void ApplyStatBonuses(GameObject enemyObject)
    {
        bool hasAnyBonus = currentHealthBonus != 0f || currentDamageBonus != 0f || 
                          currentMoveSpeedMultiplier != 1f || currentAttackCooldownMultiplier != 1f;
        if (!hasAnyBonus) return;
        
        // Get the Enemy component
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy == null) return;
        
        // Check if this is a minion
        Enemy_Minions minion = enemyObject.GetComponent<Enemy_Minions>();
        bool isMinion = minion != null;
        
        // Apply health bonus
        ApplyHealthBonus(enemyObject);
        
        // Apply damage bonus and minion-specific stats
        ApplyDamageAndMinionBonuses(enemyObject, isMinion);
        
        if (isMinion)
        {
            Debug.Log($"[EnemyStatManager] Applied minion stats to {enemyObject.name}: MoveSpeed×{currentMoveSpeedMultiplier:F2}, AttackCD×{currentAttackCooldownMultiplier:F2}, HP+{currentHealthBonus}, DMG+{currentDamageBonus}");
        }
        else
        {
            Debug.Log($"[EnemyStatManager] Applied stats to {enemyObject.name}: HP+{currentHealthBonus}, DMG+{currentDamageBonus}");
        }
    }
    
    /// <summary>
    /// Apply health bonus to enemy
    /// </summary>
    private void ApplyHealthBonus(GameObject enemyObject)
    {
        if (currentHealthBonus <= 0f) return;
        
        Entity_Health health = enemyObject.GetComponent<Entity_Health>();
        if (health != null)
        {
            float newMaxHealth = health.MaxHealth + currentHealthBonus;
            health.SetMaxHealth(newMaxHealth); // SetMaxHealth already sets currentHealth to maxHealth
            Debug.Log($"[EnemyStatManager] Applied health bonus +{currentHealthBonus} to {enemyObject.name}, new health: {newMaxHealth}");
        }
    }
    
    /// <summary>
    /// Apply damage bonus and minion-specific stat bonuses
    /// </summary>
    private void ApplyDamageAndMinionBonuses(GameObject enemyObject, bool isMinion)
    {
        if (currentDamageBonus <= 0f && !isMinion) return;
        
        // Get or add EnemyStatModifier component
        EnemyStatModifier modifier = enemyObject.GetComponent<EnemyStatModifier>();
        if (modifier == null && (currentDamageBonus > 0f || isMinion))
        {
            modifier = enemyObject.AddComponent<EnemyStatModifier>();
        }
        
        if (modifier == null) return;
        
        // Apply damage bonus
        if (currentDamageBonus > 0f)
        {
            modifier.damageBonus = currentDamageBonus;
        }
        
        // Apply minion-specific stat multipliers
        if (isMinion)
        {
            modifier.moveSpeedMultiplier = currentMoveSpeedMultiplier;
            modifier.attackCooldownMultiplier = currentAttackCooldownMultiplier;
        }
    }
    
    /// <summary>
    /// Apply custom stat bonuses to an enemy (for special enemies)
    /// </summary>
    public void ApplyCustomBonuses(GameObject enemyObject, float healthMultiplier = 1f, float damageMultiplier = 1f, 
                                   float moveSpeedMultiplier = 1f, float attackCooldownMultiplier = 1f)
    {
        // Apply custom health multiplier plus wave bonus
        if (healthMultiplier != 1f || currentHealthBonus > 0f)
        {
            Entity_Health health = enemyObject.GetComponent<Entity_Health>();
            if (health != null)
            {
                float baseMaxHealth = health.MaxHealth;
                float newHealth = (baseMaxHealth * healthMultiplier) + currentHealthBonus;
                health.SetMaxHealth(newHealth);
                Debug.Log($"[EnemyStatManager] Applied custom health multiplier {healthMultiplier}x + bonus {currentHealthBonus}, new health: {newHealth}");
            }
        }
        
        // Apply custom damage and other multipliers
        if (damageMultiplier != 1f || moveSpeedMultiplier != 1f || attackCooldownMultiplier != 1f || currentDamageBonus > 0f)
        {
            EnemyStatModifier modifier = enemyObject.GetComponent<EnemyStatModifier>();
            if (modifier == null)
            {
                modifier = enemyObject.AddComponent<EnemyStatModifier>();
            }
            
            // Apply damage bonus (base damage * multiplier + wave bonus)
            if (damageMultiplier != 1f || currentDamageBonus > 0f)
            {
                float baseDamage = 10f; // You may want to read this from the enemy prefab
                modifier.damageBonus = (baseDamage * damageMultiplier) + currentDamageBonus;
                Debug.Log($"[EnemyStatManager] Applied damage multiplier {damageMultiplier}x + bonus {currentDamageBonus}, total bonus: {modifier.damageBonus}");
            }
            
            // Apply movement and attack speed multipliers
            if (moveSpeedMultiplier != 1f)
            {
                modifier.moveSpeedMultiplier = moveSpeedMultiplier;
                Debug.Log($"[EnemyStatManager] Applied move speed multiplier: {moveSpeedMultiplier}x");
            }
            
            if (attackCooldownMultiplier != 1f)
            {
                modifier.attackCooldownMultiplier = attackCooldownMultiplier;
                Debug.Log($"[EnemyStatManager] Applied attack cooldown multiplier: {attackCooldownMultiplier}x");
            }
        }
    }
    
    /// <summary>
    /// Reset all stat bonuses
    /// </summary>
    public void ResetBonuses()
    {
        currentHealthBonus = 0f;
        currentDamageBonus = 0f;
        currentMoveSpeedMultiplier = 1f;
        currentAttackCooldownMultiplier = 1f;
        Debug.Log("[EnemyStatManager] Stat bonuses reset to default values");
    }
    
    /// <summary>
    /// Check if there are any active bonuses
    /// </summary>
    public bool HasActiveBonuses()
    {
        return currentHealthBonus != 0f || currentDamageBonus != 0f || 
               currentMoveSpeedMultiplier != 1f || currentAttackCooldownMultiplier != 1f;
    }
    
    // Getters for current bonuses
    public float GetCurrentHealthBonus() => currentHealthBonus;
    public float GetCurrentDamageBonus() => currentDamageBonus;
    public float GetCurrentMoveSpeedMultiplier() => currentMoveSpeedMultiplier;
    public float GetCurrentAttackCooldownMultiplier() => currentAttackCooldownMultiplier;
}