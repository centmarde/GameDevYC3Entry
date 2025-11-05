using UnityEngine;

/// <summary>
/// Movement component for minions that allows movement during attacks.
/// This overrides the base Enemy_Movement to keep moving while attacking.
/// </summary>
public class Enemy_Minions_Movement : Enemy_Movement
{
    private float attackMoveSpeedMultiplier = 0.6f; // Default to 60% speed while attacking
    private float aggressionMultiplier = 1.2f; // Default aggression multiplier

    /// <summary>
    /// Set the speed multiplier applied during attacks
    /// </summary>
    public void SetAttackMoveSpeedMultiplier(float multiplier)
    {
        attackMoveSpeedMultiplier = Mathf.Clamp01(multiplier);
        Debug.Log($"[Enemy_Minions_Movement] Attack move speed set to {attackMoveSpeedMultiplier * 100}%");
    }

    /// <summary>
    /// Set the aggression multiplier for movement speed
    /// </summary>
    public void SetAggressionMultiplier(float multiplier)
    {
        aggressionMultiplier = Mathf.Clamp(multiplier, 0.5f, 2f);
        Debug.Log($"[Enemy_Minions_Movement] Aggression multiplier set to {aggressionMultiplier}x");
    }

    /// <summary>
    /// Check if we're currently in an attack state
    /// </summary>
    private bool IsInAttackState()
    {
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null && enemy.meleeAttackState != null)
        {
            // Access stateMachine through reflection or check animation state
            Animator anim = enemy.anim;
            if (anim != null)
            {
                // Check if the "isAttacking" animation boolean is active
                return anim.GetBool("isAttacking");
            }
        }
        return false;
    }

    /// <summary>
    /// Override MoveToPlayer to allow movement during attacks
    /// Minions keep moving toward player even while attacking
    /// </summary>
    public new void MoveToPlayer()
    {
        Transform player = GetPlayer();
        if (!player) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 to = player.position - transform.position;
        to.y = 0f;
        float distance = to.magnitude;

        // Calculate movement speed based on attack state and aggression
        float baseMoveSpeed = Stats.moveSpeed * aggressionMultiplier;
        
        // Apply wave scaling move speed multiplier if present
        EnemyStatModifier statModifier = GetComponent<EnemyStatModifier>();
        if (statModifier != null)
        {
            baseMoveSpeed = statModifier.GetModifiedMoveSpeed(baseMoveSpeed);
        }
        
        float currentSpeed = baseMoveSpeed;
        bool isAttacking = IsInAttackState();
        
        if (isAttacking)
        {
            currentSpeed *= attackMoveSpeedMultiplier;
            
            // While attacking, keep moving but don't get too close
            // Stop at minimum distance to avoid pushing through player
            if (distance > 0.5f)
            {
                Vector3 step = to.normalized * currentSpeed * Time.deltaTime;
                rb.MovePosition(rb.position + step);
            }
            else
            {
                // Stop movement when very close during attack
                rb.linearVelocity = Vector3.zero;
            }
        }
        else
        {
            // Not attacking - move toward player normally
            // Stop further away to allow attack to trigger
            if (distance > 1.0f)
            {
                Vector3 step = to.normalized * currentSpeed * Time.deltaTime;
                rb.MovePosition(rb.position + step);
            }
            else
            {
                // Slow down when approaching attack range
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
}
