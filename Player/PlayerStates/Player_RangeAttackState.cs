using UnityEngine;

public class Player_RangeAttackState : PlayerState
{
    private Vector3 cachedAim;

    public void SetCachedAim(Vector3 aim) => cachedAim = aim;

    public Player_RangeAttackState(Player player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        
        player.playerMovement.StopMovement();
        player.playerMovement.movementLocked = true;

        player.playerCombat.currentAttack.ExecuteAttack(cachedAim);
    }

    public override void Update()
    {
        base.Update();

        // Return to idle when animation finishes
        var info = anim.GetCurrentAnimatorStateInfo(0);
        
        if (info.IsName("playerRangeAttack") && info.normalizedTime >= 1f)
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
