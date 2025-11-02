using UnityEngine;

[CreateAssetMenu(menuName = "Dagitab/Stats/Enemy Stats Data", fileName = "EnemyStatsData - ")]

public class EnemyStatData_SO : ScriptableObject
{
    [Header("Core")]
    public float maxHealth = 50f;
    public float moveSpeed = 3f;
    public float chaseSpeed = 5f;
    public float turnSpeed = 6f;

    [Header("Ranges")]
    public float stopDistance = 2f;
    public float chaseRadius = 8f;
    public float aggroRadius = 5f;
    public float leashRadius = 10f;
    public float homeStopDistance = 0.15f;
    [Tooltip("Maximum distance from player before enemy respawns at spawn point. Set to 0 to disable.")]
    public float maxDistanceFromPlayer = 50f;

    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 0.4f;
    public float attackRadius = 2f;

    [Header("Patrol")]
    public float patrolRadius = 5f;            
    public float patrolMoveSpeed = 1f;        
    public Vector3 patrolDirection = new Vector3(1, 0, 0); 
    public float patrolIdleDuration = 3f;

    [Header("Death")]
    public float deathDelay = 0.75f;


}
