using UnityEngine;

public class Player_HurtState : PlayerState
{

    public Player_HurtState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.playerMovement.ClearMovementIntent();


    }

    public override void Update()
    {
        base.Update();


        var info = anim.GetCurrentAnimatorStateInfo(0);

        if (info.IsName("playerHurt") && info.normalizedTime >= 0.95f)
        {
            if (player.playerMovement.moveInput.sqrMagnitude > 0.01f)
                player.RequestStateChange(player.moveState);
            else
                player.RequestStateChange(player.idleState);
        }

    }


    public override void Exit()
    {
        base.Exit();
        player.playerMovement.ClearMovementIntent();




    }

}

