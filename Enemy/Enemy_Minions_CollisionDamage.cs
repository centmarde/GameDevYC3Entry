using UnityEngine;
using System.Collections;

/// <summary>
/// Collision-based damage component for minions.
/// Deals damage to player on collision contact and triggers attack animation.
/// </summary>
[RequireComponent(typeof(Enemy_Minions))]
public class Enemy_Minions_CollisionDamage : MonoBehaviour
{
    [Header("Collision Damage Settings")]
    [Tooltip("Layer mask for what can be damaged (should include Player layer)")]
    [SerializeField] private LayerMask damageableLayer;
    
    [Header("Current Stats (Read-Only)")]
    [Tooltip("Base collision damage from ScriptableObject")]
    [SerializeField] private float baseAttackDamage = 1f;
    
    [Tooltip("Final collision damage including wave scaling bonuses")]
    [SerializeField] private float currentCollisionDamage = 10f;
    
    [Tooltip("Attack cooldown in seconds (time between damage ticks)")]
    [SerializeField] private float currentAttackCooldown = 1f;
    
    [Tooltip("Damage bonus from wave scaling")]
    [SerializeField] private float waveDamageBonus = 0f;

    private Enemy_Minions minion;
    private float lastDamageTime = -999f;
    private bool isCollidingWithPlayer = false;
    
    // Internal stats
    private float attackDamage = 1f;
    private float attackCooldown = 1f;
    private bool playAttackAnimOnCollision = true;

    private void Awake()
    {
        minion = GetComponent<Enemy_Minions>();
        
        if (minion == null)
        {
            Debug.LogError($"[Enemy_Minions_CollisionDamage] No Enemy_Minions component found on {gameObject.name}!", this);
            enabled = false;
            return;
        }
        
        // Load settings from ScriptableObject if available
        if (minion.MinionStats != null)
        {
            // Use attackDamage and attackCooldown from base EnemyStatData_SO
            attackDamage = minion.MinionStats.attackDamage;
            baseAttackDamage = attackDamage;
            attackCooldown = minion.MinionStats.attackCooldown;
            playAttackAnimOnCollision = minion.MinionStats.playAttackAnimOnCollision;
            
            // Apply wave scaling bonuses
            EnemyStatModifier statModifier = GetComponent<EnemyStatModifier>();
            if (statModifier != null)
            {
                // Apply attack cooldown multiplier
                attackCooldown = statModifier.GetModifiedAttackCooldown(attackCooldown);
                
                // Get damage bonus for display
                waveDamageBonus = statModifier.damageBonus;
            }
            
            // Update Inspector display values
            currentAttackCooldown = attackCooldown;
            currentCollisionDamage = attackDamage + waveDamageBonus;
            
            Debug.Log($"[Enemy_Minions_CollisionDamage] {gameObject.name} configured: Base {baseAttackDamage} + Bonus {waveDamageBonus} = {currentCollisionDamage} damage, {currentAttackCooldown}s cooldown, playAnim: {playAttackAnimOnCollision}");
        }
        else
        {
            Debug.LogWarning($"[Enemy_Minions_CollisionDamage] {gameObject.name} has no MinionStats, using default collision damage settings: {attackDamage} dmg");
            baseAttackDamage = attackDamage;
            currentCollisionDamage = attackDamage;
            currentAttackCooldown = attackCooldown;
        }
        
        // Validate layer mask
        if (damageableLayer == 0)
        {
            Debug.LogError($"[Enemy_Minions_CollisionDamage] ✗ CRITICAL: Damageable Layer Mask is not set! Collision damage will NOT work. Please set it in the Inspector to include the Player layer.", this);
        }
        else
        {
            Debug.Log($"[Enemy_Minions_CollisionDamage] Layer mask configured: {damageableLayer.value}");
        }
        
        // Validate animator
        if (minion.anim == null)
        {
            Debug.LogWarning($"[Enemy_Minions_CollisionDamage] No animator found on {gameObject.name} - attack animations will not play");
        }
    }

    private void Update()
    {
        // Ensure attack animation is only active when colliding
        if (minion != null && minion.anim != null)
        {
            bool shouldAttack = isCollidingWithPlayer && playAttackAnimOnCollision;
            
            // Only update if the state changed to avoid unnecessary SetBool calls
            if (minion.anim.GetBool("isAttacking") != shouldAttack)
            {
                minion.anim.SetBool("isAttacking", shouldAttack);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        int objectLayer = collision.gameObject.layer;
        string layerName = LayerMask.LayerToName(objectLayer);
        
        Debug.Log($"[Enemy_Minions_CollisionDamage] {gameObject.name} collided with {collision.gameObject.name} (Layer: {objectLayer} '{layerName}')");
        
        // Check if we collided with something on the damageable layer
        int collisionLayerMask = 1 << objectLayer;
        bool isDamageable = (collisionLayerMask & damageableLayer.value) != 0;
        
        Debug.Log($"[Enemy_Minions_CollisionDamage] Object on layer {objectLayer} ({layerName}), LayerMask bit: {collisionLayerMask}, Damageable mask value: {damageableLayer.value}, Match: {isDamageable}");
        
        if (isDamageable)
        {
            isCollidingWithPlayer = true;
            Debug.Log($"[Enemy_Minions_CollisionDamage] Valid collision detected! Attempting damage...");
            
            // Try to damage immediately on contact
            TryDealDamage(collision.gameObject);
            
            // Note: Attack animation is now handled automatically in Update()
        }
        else
        {
            Debug.LogWarning($"[Enemy_Minions_CollisionDamage] Collision with {collision.gameObject.name} ignored - not on damageable layer. Make sure Layer Mask is configured correctly!");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Check if we're still colliding with something damageable
        if (((1 << collision.gameObject.layer) & damageableLayer.value) != 0)
        {
            isCollidingWithPlayer = true;
            
            // Deal damage over time while in contact
            if (Time.time >= lastDamageTime + attackCooldown)
            {
                TryDealDamage(collision.gameObject);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Check if we stopped colliding with something damageable
        if (((1 << collision.gameObject.layer) & damageableLayer.value) != 0)
        {
            isCollidingWithPlayer = false;
        }
    }

    /// <summary>
    /// Attempt to deal damage to the collided object
    /// </summary>
    private void TryDealDamage(GameObject target)
    {
        Debug.Log($"[Enemy_Minions_CollisionDamage] TryDealDamage called on {target.name}");
        
        // Get health component from target
        Entity_Health health = target.GetComponent<Entity_Health>();
        if (health == null)
        {
            health = target.GetComponentInParent<Entity_Health>();
            Debug.Log($"[Enemy_Minions_CollisionDamage] Health component search: {(health != null ? "Found in parent" : "NOT FOUND")}");
        }
        else
        {
            Debug.Log($"[Enemy_Minions_CollisionDamage] Health component found directly on {target.name}");
        }

        if (health != null && health.IsAlive)
        {
            // Calculate final damage using attackDamage from stats (with wave scaling bonuses)
            float finalDamage = attackDamage;
            
            EnemyStatModifier statModifier = GetComponent<EnemyStatModifier>();
            if (statModifier != null)
            {
                finalDamage = statModifier.GetModifiedDamage(finalDamage);
                
                // Update Inspector display with current damage
                currentCollisionDamage = finalDamage;
                waveDamageBonus = statModifier.damageBonus;
                
                Debug.Log($"[Enemy_Minions_CollisionDamage] Damage modified by EnemyStatModifier: {attackDamage} -> {finalDamage}");
            }

            // Use the full attack damage per hit (not per second)
            float damageAmount = finalDamage;
            
            Debug.Log($"[Enemy_Minions_CollisionDamage] Attempting to deal {damageAmount} damage (base: {baseAttackDamage}, bonus: +{waveDamageBonus}, cooldown: {currentAttackCooldown}s)");
            
            // Deal damage (Entity_Health will handle defense absorption automatically)
            Vector3 hitPoint = transform.position;
            Vector3 hitNormal = (target.transform.position - transform.position).normalized;
            
            // Check player defense before dealing damage (for logging purposes)
            float playerDefense = GetPlayerDefenseValue(target);
            int defenseLevel = GetPlayerDefenseLevel(target);
            float expectedAbsorption = CalculateExpectedAbsorption(target, damageAmount);
            float expectedDamage = damageAmount - expectedAbsorption;
            
            if (playerDefense > 0)
            {
                Debug.Log($"[Enemy_Minions_CollisionDamage] Target defense: {playerDefense} raw + {Mathf.Min(defenseLevel * 10, 50)}% reduction (Level {defenseLevel}), Expected absorption: {expectedAbsorption:F1}, Expected damage: {expectedDamage:F1}");
            }
            
            bool damageApplied = health.TakeDamage(damageAmount, hitPoint, hitNormal, minion);
            if (damageApplied)
            {
                lastDamageTime = Time.time;
                if (playerDefense > 0)
                {
                    Debug.Log($"[Enemy_Minions_CollisionDamage] ✓ Attack processed: {damageAmount} → {expectedDamage:F1} damage to {target.name} (absorbed {expectedAbsorption:F1} via defense)");
                }
                else
                {
                    Debug.Log($"[Enemy_Minions_CollisionDamage] ✓ Successfully dealt {damageAmount} collision damage to {target.name}");
                }
            }
            else
            {
                if (expectedAbsorption >= damageAmount)
                {
                    Debug.Log($"[Enemy_Minions_CollisionDamage] ⚡ Attack completely negated! Defense absorbed all {damageAmount} damage ({playerDefense} raw + {Mathf.Min(defenseLevel * 10, 50)}% reduction)");
                }
                else
                {
                    Debug.LogWarning($"[Enemy_Minions_CollisionDamage] ✗ Failed to deal damage to {target.name} - TakeDamage returned false");
                }
            }
        }
        else if (health == null)
        {
            Debug.LogError($"[Enemy_Minions_CollisionDamage] ✗ No Entity_Health component found on {target.name} or its parent!");
        }
        else if (!health.IsAlive)
        {
            Debug.Log($"[Enemy_Minions_CollisionDamage] Target {target.name} is already dead");
        }
    }



    /// <summary>
    /// Get current collision status (for debugging)
    /// </summary>
    public bool IsCollidingWithPlayer => isCollidingWithPlayer;
    
    /// <summary>
    /// Get the defense value of the target player (for logging purposes)
    /// </summary>
    private float GetPlayerDefenseValue(GameObject target)
    {
        // Check if target is Player1
        Player player1 = target.GetComponent<Player>();
        if (player1 != null && player1.Stats != null)
        {
            return player1.Stats.defense;
        }
        
        // Check if target is Player2
        Player2 player2 = target.GetComponent<Player2>();
        if (player2 != null && player2.Stats != null)
        {
            return player2.Stats.defense;
        }
        
        return 0f; // No defense found or not a player
    }
    
    /// <summary>
    /// Get the defense upgrade level of the target player
    /// </summary>
    private int GetPlayerDefenseLevel(GameObject target)
    {
        // Check if target is Player1
        Player player1 = target.GetComponent<Player>();
        if (player1 != null && player1.Stats != null)
        {
            return player1.Stats.defenseUpgradeLevel;
        }
        
        // Check if target is Player2
        Player2 player2 = target.GetComponent<Player2>();
        if (player2 != null && player2.Stats != null)
        {
            return player2.Stats.defenseUpgradeLevel;
        }
        
        return 0; // No defense found or not a player
    }
    
    /// <summary>
    /// Calculate expected damage absorption using the same logic as player stats
    /// </summary>
    private float CalculateExpectedAbsorption(GameObject target, float incomingDamage)
    {
        // Check if target is Player1
        Player player1 = target.GetComponent<Player>();
        if (player1 != null && player1.Stats != null)
        {
            return player1.Stats.CalculateDefenseAbsorption(incomingDamage);
        }
        
        // Check if target is Player2
        Player2 player2 = target.GetComponent<Player2>();
        if (player2 != null && player2.Stats != null)
        {
            return player2.Stats.CalculateDefenseAbsorption(incomingDamage);
        }
        
        return 0f; // No defense found or not a player
    }

    private void OnDrawGizmos()
    {
        // Draw collision damage indicator when colliding
        if (isCollidingWithPlayer && Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
