using UnityEngine;

public class Enemy_IdleState : EnemyState
{
    public Enemy_IdleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enemy.rb.linearVelocity = Vector3.zero;
        enemy.rb.angularVelocity = Vector3.zero;
    }

    public override void Update()
    {
        base.Update();

        // Immediately transition to chase if player exists
        if (enemy.movement.PlayerWithinAggro())
        {
            stateMachine.ChangeState(enemy.chaseState);
            return;
        }
    }
}
