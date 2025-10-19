using UnityEngine;

[CreateAssetMenu(menuName = "Dagitab/Stats/Player Normal Atk Stats Data", fileName = "Player_NormalAtkData- ")]
public class Player_NormalRangeAtkData_SO : ScriptableObject
{
    [Header("Normal Attack Stats")]
    public float normalAttackDamage = 10f;
    public float normalAttackSpeed = 50f;

    public float normalAttackRange = 15f;

}
