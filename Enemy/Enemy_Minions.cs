using System.Collections;
using UnityEngine;

/// <summary>
/// Minion variant of Enemy that uses collision-based damage instead of traditional combat.
/// Usage: Attach this component instead of Enemy component to create a minion enemy.
/// Minions chase the player and deal damage through physical collision.
/// Does NOT require Enemy_Combat or Enemy_MeleeAttack components.
/// Use EnemyMinionStatData_SO for stats to access minion-specific settings.
/// </summary>
[RequireComponent(typeof(Enemy_Minions_Movement))]
[RequireComponent(typeof(Enemy_Minions_CollisionDamage))]
public class Enemy_Minions : Enemy
{
    [Header("Minion Stats")]
    [SerializeField] private EnemyMinionStatData_SO minionStats;
    
    private Enemy_Minions_Movement minionMovement;

    /// <summary>
    /// Override Stats to use EnemyMinionStatData_SO if available
    /// </summary>
    protected override EnemyStatData_SO Stats => minionStats != null ? minionStats : base.Stats;

    /// <summary>
    /// Get minion-specific stats
    /// </summary>
    public EnemyMinionStatData_SO MinionStats => minionStats;
    
    /// <summary>
    /// Override movement property to return minion movement
    /// </summary>
    public new Enemy_Minions_Movement movement => minionMovement;

    protected override void Awake()
    {
        // Get minion movement before base.Awake
        minionMovement = GetComponent<Enemy_Minions_Movement>();
        
        base.Awake();
        
        // Ensure rigidbody is NOT kinematic for collision detection
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            Debug.Log($"[Enemy_Minions] {gameObject.name} Rigidbody configured: isKinematic={rb.isKinematic}, collisionMode={rb.collisionDetectionMode}");
        }
        else
        {
            Debug.LogError($"[Enemy_Minions] {gameObject.name} has NO RIGIDBODY! Collision damage will not work!", this);
        }
        
        // Disable combat system since minions use collision damage
        if (combat != null)
        {
            combat.enabled = false;
            Debug.Log($"[Enemy_Minions] {gameObject.name} disabled Enemy_Combat - using collision damage instead");
        }
        
        // Configure minion movement with stats
        if (minionMovement != null && minionStats != null)
        {
            minionMovement.SetAttackMoveSpeedMultiplier(minionStats.attackMoveSpeedMultiplier);
            minionMovement.SetAggressionMultiplier(minionStats.aggressionMultiplier);
            Debug.Log($"[Enemy_Minions] {gameObject.name} initialized with {minionStats.attackMoveSpeedMultiplier * 100}% attack move speed and {minionStats.aggressionMultiplier}x aggression");
        }
        else if (minionMovement != null)
        {
            // Fallback to default values if no minion stats assigned
            minionMovement.SetAttackMoveSpeedMultiplier(0.6f);
            Debug.LogWarning($"[Enemy_Minions] {gameObject.name} has no EnemyMinionStatData_SO assigned! Using default settings.");
        }
    }

    /// <summary>
    /// Get the minion movement component
    /// </summary>
    public Enemy_Minions_Movement GetMinionMovement() => minionMovement;
    
    /// <summary>
    /// Override to prevent normal attack state transitions - minions use collision damage
    /// </summary>
    protected override void Start()
    {
        base.Start();
        
        // Force minions to stay in chase state only (no attack state needed)
        // They deal damage through collision instead
        if (stateMachine != null && chaseState != null)
        {
            stateMachine.Initialize(chaseState);
            Debug.Log($"[Enemy_Minions] {gameObject.name} initialized in chase-only mode (collision damage)");
        }
    }
}
