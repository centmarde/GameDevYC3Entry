using UnityEngine;

public class Player_RollState : PlayerState
{
    private Player_Roll rollComponent;

    public Player_RollState(Player player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();

        if (rollComponent == null)
            rollComponent = player.GetComponent<Player_Roll>();

        player.input.Player.Disable();


        Vector3 dir = player.transform.forward;
        rollComponent.BeginRoll(dir);

        // No need to reference animEvents.rollState anymore
    }


    public override void Update()
    {
        base.Update();

        player.playerRoll.Tick();

        if (!player.playerRoll.IsRolling)
        {
            player.playerMovement.movementLocked = false;

            if (player.playerMovement.moveInput.sqrMagnitude > 0.001f)
                player.RequestStateChange(player.moveState);
            else
                player.RequestStateChange(player.idleState);
        }
    }


    public override void Exit()
    {
        base.Exit();
        player.input.Player.Enable();

        rollComponent.EndRoll();
        player.rb.linearVelocity = Vector3.zero;
        
        // Clear the look rotation to prevent face direction from getting stuck
        player.playerMovement.ClearMovementIntent();
    }
}
