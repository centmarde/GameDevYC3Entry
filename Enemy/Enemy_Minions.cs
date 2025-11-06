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

    /// <summary>
    /// Override to drop multiple experience orbs (1-5 random)
    /// </summary>
    protected override void DropExperienceOrb()
    {
        if (experienceOrbPrefab == null)
        {
            // No orb prefab assigned, skip dropping
            return;
        }

        // Random number of orbs between 1 and 5
        int orbCount = Random.Range(1, 6); // Range is inclusive min, exclusive max (so 1-5)
        
        Debug.Log($"[Enemy_Minions] {gameObject.name} dropping {orbCount} experience orbs");

        for (int i = 0; i < orbCount; i++)
        {
            try
            {
                // Spawn orb above the enemy's position with slight random offset
                Vector3 dropPosition = transform.position + Vector3.up * 1.5f;
                // Add small random offset to spread orbs slightly
                dropPosition += new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
                
                GameObject orb = Instantiate(experienceOrbPrefab, dropPosition, Random.rotation);
                
                // Add upward and random directional force for ragdoll effect
                Rigidbody orbRb = orb.GetComponent<Rigidbody>();
                if (orbRb != null)
                {
                    // Upward force with slight random horizontal spread
                    Vector3 randomDirection = new Vector3(
                        Random.Range(-1f, 1f),
                        1f,
                        Random.Range(-1f, 1f)
                    ).normalized;
                    
                    orbRb.AddForce(randomDirection * Random.Range(2f, 4f), ForceMode.Impulse);
                    
                    // Add random torque for tumbling effect (smoother)
                    orbRb.AddTorque(new Vector3(
                        Random.Range(-3f, 3f),
                        Random.Range(-3f, 3f),
                        Random.Range(-3f, 3f)
                    ), ForceMode.Impulse);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Enemy_Minions] Failed to drop experience orb {i + 1}/{orbCount}: {e.Message}");
            }
        }
    }
}
