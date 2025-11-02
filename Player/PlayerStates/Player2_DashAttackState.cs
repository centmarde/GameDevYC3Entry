using UnityEngine;

public class Player2_DashAttackState : PlayerState
{
    private Player2 player2;
    private Vector3 cachedAimDirection;

    public Player2_DashAttackState(Player2 player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName)
    {
        this.player2 = player;
    }

    public override void Enter()
    {
        base.Enter();

        // Get initial aim direction
        cachedAimDirection = player2.meleeCombat.GetAimDirection();

        // Allow free movement with slowdown while charging
        player2.playerMovement.movementLocked = false;
        player2.playerMovement.ApplySlowdown(999f, 0.6f);

        // Start charging dash
        player2.dashAttack.StartCharging(cachedAimDirection);

        // 🟡 UI: show stance ring
        if (player2.meleeIndicator != null)
            player2.meleeIndicator.EnterDrawState();

        Debug.Log("[Player2_DashAttackState] Started charging dash attack");
    }

    public override void Update()
    {
        base.Update();

        // While charging
        if (player2.dashAttack.IsCharging)
        {
            player2.dashAttack.TickCharge(Time.deltaTime);

            Vector3 newAim = player2.meleeCombat.GetAimDirection();
            player2.dashAttack.UpdateDashDirection(newAim);
            player2.meleeCombat.FaceSmooth(newAim);
            cachedAimDirection = newAim;
        }
        // When dash executes
        else if (player2.dashAttack.IsExecutingDash)
        {
            player2.playerMovement.movementLocked = true;

            // 💥 UI: flash when the blink/dash actually happens
            if (player2.meleeIndicator != null)
                player2.meleeIndicator.ExecuteSlash();
        }
        // When done
        else
        {
            player2.playerMovement.movementLocked = false;

            if (player2.playerMovement.moveInput.sqrMagnitude > 0.001f)
                player2.RequestStateChange(player2.moveState);
            else
                player2.RequestStateChange(player2.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        // Release charge if still holding
        if (player2.dashAttack.IsCharging)
            player2.dashAttack.ReleaseCharge();

        // Reset movement
        player2.playerMovement.ApplySlowdown(0.1f, 1f);
        player2.playerMovement.movementLocked = false;

        // ⚪ UI: ensure the ring fades out fully on exit
        if (player2.meleeIndicator != null)
            player2.meleeIndicator.ExecuteSlash();

        Debug.Log("[Player2_DashAttackState] Exited dash attack state");
    }
}
