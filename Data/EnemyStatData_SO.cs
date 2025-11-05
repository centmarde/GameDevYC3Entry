using UnityEngine;

[CreateAssetMenu(menuName = "Dagitab/Stats/Enemy Stats Data", fileName = "EnemyStatsData - ")]

public class EnemyStatData_SO : ScriptableObject
{
    [Header("Core")]
    public float maxHealth = 50f;
    public float moveSpeed = 3f;

    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackCooldown = 0.4f;

    [Header("Death")]
    public float deathDelay = 0.75f;
}
