
public class Player_ChargedRuntimeData
{
    public float chargedAttackDamage;
    public float chargedAttackSpeed;
    public float maxChargeTime;
    public float minChargeMultiplier;
    public float maxChargeMultiplier;

    public Player_ChargedRuntimeData(Player_ChargedRangeAtkData_SO baseData)
    {
        chargedAttackDamage = baseData.chargedAttackDamage;
        chargedAttackSpeed = baseData.chargedAttackSpeed;
        maxChargeTime   =   baseData.maxChargeTime;
        minChargeMultiplier = baseData.minChargeMultiplier;
        maxChargeMultiplier = baseData.maxChargeMultiplier;

    }

    public void ApplyUpgrade(float dmgBonus, float spdBonus)
    {
        chargedAttackDamage += dmgBonus;
        chargedAttackSpeed += spdBonus;
    }
}
