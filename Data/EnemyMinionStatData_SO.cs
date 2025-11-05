using UnityEngine;

[CreateAssetMenu(menuName = "Dagitab/Stats/Enemy Minion Stats Data", fileName = "EnemyMinionStatsData - ")]

public class EnemyMinionStatData_SO : EnemyStatData_SO
{
    [Header("Minion Specific Settings")]
    [Tooltip("Speed multiplier while attacking (1.0 = full speed, 0.5 = half speed, 0.0 = stop like regular enemy)")]
    [Range(0f, 1f)]
    public float attackMoveSpeedMultiplier = 0.6f;

    [Header("Minion Behavior")]
    [Tooltip("If true, minion will try to surround the player instead of direct chase")]
    public bool useSurroundBehavior = false;
    
    [Tooltip("Preferred distance from player when surrounding (only used if useSurroundBehavior is true)")]
    public float surroundDistance = 3f;

    [Tooltip("How aggressively the minion pursues the player (higher = more aggressive)")]
    [Range(0.5f, 2f)]
    public float aggressionMultiplier = 1.2f;

    [Header("Collision Damage")]
    [Tooltip("Base damage per second dealt when colliding with player")]
    public float collisionDamagePerSecond = 1f;
    
    [Tooltip("Cooldown between collision damage ticks (in seconds)")]
    public float collisionDamageCooldown = 1f;
    
    [Tooltip("Play attack animation when colliding with player")]
    public bool playAttackAnimOnCollision = true;
}
