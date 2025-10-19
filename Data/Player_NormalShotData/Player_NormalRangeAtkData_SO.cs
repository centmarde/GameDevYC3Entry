using UnityEngine;

[CreateAssetMenu(menuName = "Dagitab/Stats/Player Normal Atk Stats Data", fileName = "Player_NormalAtkData- ")]
public class Player_NormalRangeAtkData_SO : ScriptableObject
{
    [Header("Normal Attack Stats")]
    [Tooltip("Damage is taken from Player_DataSO.projectileDamage")]
    public float normalAttackSpeed = 50f;
    public float normalAttackRange = 15f;

}
