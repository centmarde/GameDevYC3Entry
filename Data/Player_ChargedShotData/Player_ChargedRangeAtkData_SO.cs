using UnityEngine;

[CreateAssetMenu(menuName = "Dagitab/Stats/Player Charged Atk Stats Data", fileName = "Player_ChargedshotAtkData- ")]

public class Player_ChargedRangeAtkData_SO : ScriptableObject
{


    public float chargedAttackDamage = 10f;
    public float chargedAttackSpeed = 50f;
    public float maxChargeTime = 2f;
    public float minChargeMultiplier = 1f;
    public float maxChargeMultiplier = 2.5f;

}
