using UnityEngine;

public class Player_ChargedAttackState : PlayerState
{

    private Vector3 cachedAim;

    public Player_ChargedAttackState(Player player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();


        cachedAim = player.playerCombat.GetAimDirection();
        player.playerCombat.FaceInstant(cachedAim);

        player.playerMovement.ApplySlowdown(999f, 0.2f);


        // Start the charge logic
        var currentAttack = player.playerCombat.currentAttack as Player_ChargedRangeAttack;
        if (currentAttack != null)
        {
            currentAttack.ExecuteAttack(player.playerCombat.GetAimDirection());
        }

        Debug.Log("[ChargeState] Started charging...");
    }

    public override void Update()
    {
        base.Update();

        if (player.playerCombat.currentAttack is Player_ChargedRangeAttack chargedAttack)
        {
            chargedAttack.TickCharge(Time.deltaTime);

            // Always face the mouse aim � even while moving
            Vector3 newAim = player.playerCombat.GetAimDirection();
            player.playerCombat.FaceSmooth(newAim);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.playerMovement.ApplySlowdown(0.1f, 1f); // Reset to normal speed



        var currentAttack = player.playerCombat.currentAttack as Player_ChargedRangeAttack;
        if (currentAttack != null)
        {
            currentAttack.EndAttackInternal();
        }

        Debug.Log("[ChargeState] Released charge.");
    }
}
