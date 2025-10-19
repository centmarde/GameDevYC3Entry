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
        Debug.Log("[Player_RangeAttackState] Enter called");
        Debug.Log($"[Player_RangeAttackState] Cached aim: {cachedAim}");
        Debug.Log($"[Player_RangeAttackState] Current attack: {player.playerCombat.currentAttack?.GetType().Name ?? "NULL"}");
        
        player.playerMovement.StopMovement();
        player.playerMovement.movementLocked = true;

        player.playerCombat.currentAttack.ExecuteAttack(cachedAim);
        Debug.Log("[Player_RangeAttackState] ExecuteAttack called");
    }

    public override void Update()
    {
        base.Update();

        // Return to idle when animation finishes
        var info = anim.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[Player_RangeAttackState] Update - Current anim state: {info.shortNameHash}, normalizedTime: {info.normalizedTime}");
        Debug.Log($"[Player_RangeAttackState] Is 'playerRangeAttack'? {info.IsName("playerRangeAttack")}");
        
        if (info.IsName("playerRangeAttack") && info.normalizedTime >= 1f)
        {
            Debug.Log("[Player_RangeAttackState] Animation finished, returning to idle");
            player.RequestStateChange(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.playerMovement.movementLocked = false;
    }
}
