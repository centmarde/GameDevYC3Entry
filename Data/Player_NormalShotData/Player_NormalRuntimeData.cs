public class Player_NormalRuntimeData
{
    public float normalAttackDamage;
    public float normalAttackSpeed;

    public Player_NormalRuntimeData(Player_NormalRangeAtkData_SO baseData)
    {
        normalAttackDamage = baseData.normalAttackDamage;
        normalAttackSpeed = baseData.normalAttackSpeed;
    }

    public void ApplyUpgrade(float dmgBonus, float spdBonus)
    {
        normalAttackDamage += dmgBonus;
        normalAttackSpeed += spdBonus;
    }
}
