using UnityEngine;

public class Player_OpenChestState : PlayerState
{

    public Player_OpenChestState(Player player, StateMachine sm, string animBoolName)
        : base(player, sm, animBoolName) { }

    public override void Enter()
    {
        base.Enter();

        player.pendingInteractable?.Interact(player);

        player.playerMovement.StopMovement();
        player.playerMovement.ClearMovementIntent();
    }

    public override void Update()
    {
        base.Update();

        var info = player.anim.GetCurrentAnimatorStateInfo(0);
        if (!player.anim.IsInTransition(0) && info.IsName("playerOpeningChest") && info.normalizedTime >= 1f)
        {
           player.RequestStateChange(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.playerMovement.ClearMovementIntent();

        player.pendingInteractable = null;
        player.pendingProfile = null;
    }
}
