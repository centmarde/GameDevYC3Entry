using UnityEngine;

public class Player_AttackAnimEvents : MonoBehaviour
{
     public void EndAttack()
    {
        // Try to find the range attack controller in the player hierarchy
        var controller = GetComponentInParent<Player_RangeAttackController>();

        if (controller != null && controller.CurrentAttack != null)
        {
            controller.CurrentAttack.EndAttack();
        }
        else
        {
            Debug.LogWarning("Player_AttackAnimEvents: No current ranged attack found when ending attack!");
        }
    }
}
