using UnityEngine;

public class Player_AttackAnimEvents : MonoBehaviour
{
     public void EndAttack()
    {
        Debug.Log("[AttackAnimEvents] EndAttack animation event triggered!");
        
        // Try to find the range attack controller in the player hierarchy
        var controller = GetComponentInParent<Player_RangeAttackController>();

        if (controller != null)
        {
            var activeAttack = controller.ActiveAttack;
            
            if (activeAttack != null)
            {
                // Use ActiveAttack instead of CurrentAttack to ensure we fire the attack that was started,
                // not the one currently selected (in case player scrolled mid-attack)
                Debug.Log($"[AttackAnimEvents] Calling EndAttackInternal on: {activeAttack.GetType().Name} (Instance ID: {activeAttack.GetInstanceID()})");
                activeAttack.EndAttackInternal();
                controller.ClearActiveAttack();
            }
            else
            {
                Debug.LogWarning("Player_AttackAnimEvents: ActiveAttack is null!");
            }
        }
        else
        {
            Debug.LogWarning("Player_AttackAnimEvents: No Player_RangeAttackController found in parent!");
        }
    }
}
