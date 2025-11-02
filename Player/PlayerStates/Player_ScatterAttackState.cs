
using UnityEngine;

public class Player_ScatterAttackState : PlayerState
{
     private Vector3 cachedAim;

     public void SetCachedAim(Vector3 aim) => cachedAim = aim;

public Player_ScatterAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.playerMovement.StopMovement();
        player.playerMovement.movementLocked = true;

        var currentAttack = player.playerCombat.currentAttack;

        // Debug: Log which attack is being executed
        if (currentAttack != null)
        {
            // Debug.Log($"[
            //
            //
            // State] Executing: {currentAttack.GetType().Name} (Instance ID: {currentAttack.GetInstanceID()})");

            // Cache this attack as the active one (prevents scroll-switching mid-attack)
            var controller = player.GetComponent<Player_RangeAttackController>();
            if (controller != null)
            {
                var scatterRangeAttack = currentAttack as Player_ScatterRangeAttack;
                if (scatterRangeAttack != null)
                {
                    // Debug.Log($"[RangeAttackState] Setting active attack: {rangeAttack.GetType().Name} (Instance ID: {rangeAttack.GetInstanceID()})");
                    controller.SetActiveAttack(scatterRangeAttack);
                }
                else
                {
                    Debug.LogError($"[RangeAttackState] Failed to cast {currentAttack.GetType().Name} to Player_RangeAttack!");
                }
            }
        }

        currentAttack.ExecuteAttack(cachedAim);
    }

    public override void Update()
    {
        base.Update();

        // Return to idle when animation finishes
        var info = anim.GetCurrentAnimatorStateInfo(0);

        if (info.IsName("playerScatterAttack") && info.normalizedTime >= 1f)
        {
            player.RequestStateChange(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.playerMovement.movementLocked = false;
    }
}