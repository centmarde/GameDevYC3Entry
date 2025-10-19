public class Player_NormalRuntimeData
{
    public float normalAttackDamage;
    public float normalAttackSpeed;
    public float normalAttackRange;

    public Player_NormalRuntimeData(Player_NormalRangeAtkData_SO baseData, Player_DataSO playerStats)
    {
        // Use player stats projectile damage as base
        normalAttackDamage = playerStats.projectileDamage;
        normalAttackSpeed = baseData.normalAttackSpeed;
        normalAttackRange = baseData.normalAttackRange;
    }

    public void ApplyUpgrade(float dmgBonus, float spdBonus)
    {
        normalAttackDamage += dmgBonus;
        normalAttackSpeed += spdBonus;
    }
}
