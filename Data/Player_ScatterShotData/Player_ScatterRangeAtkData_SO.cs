using UnityEngine;

[CreateAssetMenu(menuName = "Dagitab/Stats/Player Scattershot Atk Stats Data", fileName = "Player_ScattershotAtkData- ")]

public class Player_ScatterRangeAtkData_SO : ScriptableObject
{
    [Header("Scatter Settings")]
    public int pelletCount = 5;
    public float spreadAngle = 30f;
    
    [Header("Attack Stats")]
    [Tooltip("Damage is taken from Player_DataSO.projectileDamage / pelletCount")]
    public float pelletSpeed = 30f;
    public float scatterAttackRange = 8f; // Shorter range for scatter shot

    [Header("Projectile Settings")]
    public float projectileLifetime = 0.25f; 

}
