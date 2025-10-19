
public class Player_ChargedRuntimeData
{
    public float chargedAttackDamage;
    public float chargedAttackSpeed;
    public float chargedAttackRange;
    public float maxChargeTime;
    public float minChargeMultiplier;
    public float maxChargeMultiplier;

    public Player_ChargedRuntimeData(Player_ChargedRangeAtkData_SO baseData, Player_DataSO playerStats)
    {
        // Use player stats projectile damage as base
        chargedAttackDamage = playerStats.projectileDamage;
        chargedAttackSpeed = baseData.chargedAttackSpeed;
        chargedAttackRange = baseData.chargedAttackRange;
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
