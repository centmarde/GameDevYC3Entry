using UnityEngine;

public class Enemy_MoveState : EnemyState
{
    public Enemy_MoveState(Enemy enemy, StateMachine stateMachine, string animBoolName)
        : base(enemy, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();
        // optional: setup move anim params, reset any local timers
    }

    public override void Update()
    {
        base.Update();

        // This state is no longer used, but kept for compatibility
        // Immediately transition to chase if player exists
        if (enemy.movement.PlayerWithinAggro())
        {
            stateMachine.ChangeState(enemy.chaseState);
            return;
        }
    }
}
