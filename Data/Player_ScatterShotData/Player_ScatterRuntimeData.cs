
public class Player_ScatterRuntimeData 
{
    public float pelletDamage;
    public float pelletSpeed;
    public float pelletCount;
    public float spreadAngle;

    public Player_ScatterRuntimeData(Player_ScatterRangeAtkData_SO baseData)
    {
       pelletDamage = baseData.pelletDamage;
       pelletSpeed = baseData.pelletSpeed;
       pelletCount = baseData.pelletCount;
       spreadAngle = baseData.spreadAngle;
    }

    public void ApplyUpgrade(float dmgBonus, float spdBonus)
    {
        pelletDamage += dmgBonus;
        pelletSpeed += spdBonus;
    }


}
