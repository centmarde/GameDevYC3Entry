using UnityEngine;

[CreateAssetMenu(menuName = "Dagitab/Stats/Player Scattershot Atk Stats Data", fileName = "Player_ScattershotAtkData- ")]

public class Player_ScatterRangeAtkData_SO : ScriptableObject
{


    public int pelletCount = 5;
    public float spreadAngle = 30f;
    public float pelletDamage = 5f;
    public float pelletSpeed = 30f;


    public float projectileLifetime = 0.25f; 

}
