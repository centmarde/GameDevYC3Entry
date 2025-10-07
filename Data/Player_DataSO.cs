using UnityEngine;


[CreateAssetMenu(menuName = "Dagitab/Stats/Player Stats Data", fileName = "PlayerStatsData - ")]

public class Player_DataSO : ScriptableObject
{
    public float maxHealth = 100f;
    public float projectileSpeed = 10f;
    public float projectileDamage = 25f;
    public float meleeAttackRange = 1.5f;
    public float rangeAttackRange = 6f;
    public float deathDelay = 0.1f;
    public float moveSpeed = 5f;    
    public float turnSpeed = 1000f;
    public float currentSpeedMultiplier = 1.0f;
}
