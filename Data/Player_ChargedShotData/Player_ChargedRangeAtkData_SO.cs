using UnityEngine;

[CreateAssetMenu(menuName = "Dagitab/Stats/Player Charged Atk Stats Data", fileName = "Player_ChargedshotAtkData- ")]

public class Player_ChargedRangeAtkData_SO : ScriptableObject
{
    [Header("Attack Stats")]
    [Tooltip("Damage is taken from Player_DataSO.projectileDamage")]
    public float chargedAttackSpeed = 50f;
    public float chargedAttackRange = 25f;
    
    [Header("Charge Settings")]
    public float maxChargeTime = 2f;
    public float minChargeMultiplier = 1f;
    public float maxChargeMultiplier = 2.5f;

}
