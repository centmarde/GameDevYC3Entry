
public class Player_ScatterRuntimeData 
{
    public float pelletDamage;
    public float pelletSpeed;
    public float scatterAttackRange;
    public float pelletCount;
    public float spreadAngle;

    public Player_ScatterRuntimeData(Player_ScatterRangeAtkData_SO baseData, Player_DataSO playerStats)
    {
       // Use player stats projectile damage as base (divided by pellet count for balance)
       pelletDamage = playerStats.projectileDamage / baseData.pelletCount;
       pelletSpeed = baseData.pelletSpeed;
       scatterAttackRange = baseData.scatterAttackRange;
       pelletCount = baseData.pelletCount;
       spreadAngle = baseData.spreadAngle;
    }

    public void ApplyUpgrade(float dmgBonus, float spdBonus)
    {
        pelletDamage += dmgBonus;
        pelletSpeed += spdBonus;
    }


}
