using UnityEngine;

public class Player_MoveState : PlayerState
{
    public Player_MoveState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    // Use movement component's MoveSpeed which includes multipliers (powerups, slowdowns, etc.)
    private float playerMoveSpeed => player.playerMovement.MoveSpeed;
    private Vector3 playerDirection;

    public override void Update()
    {
        base.Update();

        playerDirection = player.playerMovement.GetIsoDir();

        if (playerDirection.sqrMagnitude <= 0f)
        {
            player.playerMovement.RequestMove(Vector3.zero);
            player.RequestStateChange(player.idleState);
            return;
        }

        player.playerMovement.RequestMove(playerDirection * playerMoveSpeed); 
        player.playerMovement.RequestLook(playerDirection);                    
    }
}
