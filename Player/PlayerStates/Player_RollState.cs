// ⚠️ THIS FILE IS OBSOLETE - DELETE THIS FILE ⚠️
// The roll system has been removed from the game.
// This file is kept as an empty stub to prevent compilation errors until it can be safely deleted.
// See MOVEMENT_FIXES.md for details.

using UnityEngine;

public class Player_RollState : PlayerState
{
    public Player_RollState(Player player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        // Roll system removed - immediately exit to idle state
        player.RequestStateChange(player.idleState);
    }

    public override void Update()
    {
        base.Update();
        // This state should never be active - fail-safe exit
        player.RequestStateChange(player.idleState);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
