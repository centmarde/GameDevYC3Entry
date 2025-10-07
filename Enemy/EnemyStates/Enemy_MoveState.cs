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

        // Aggro check: if player close to HOME ring → chase
        if (enemy.movement.PlayerWithinAggro())
        {
            stateMachine.ChangeState(enemy.chaseState);
            return;
        }

        // Patrol step; returns true when an end (-1 or +1) is reached
        bool hitEnd = enemy.movement.PatrolStep(Time.deltaTime);
        if (hitEnd)
        {
            // If you want 4s idle pauses at ends:
            stateMachine.ChangeState(enemy.idleState);
            return;
        }
    }
}
