using UnityEngine;

public class Player_AttackAnimEvents : MonoBehaviour
{
    public Player_RangeAttack rangedAttack;

    public void EndAttack()
    {
        rangedAttack.EndAttack();
    }
}
