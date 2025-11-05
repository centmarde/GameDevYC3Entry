using UnityEngine;

public class Enemy_ReturnHomeState : EnemyState
{
    public Enemy_ReturnHomeState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        // This state is no longer used, but kept for compatibility
        // Immediately transition to chase instead
        if (enemy.movement.PlayerWithinAggro())
        {
            stateMachine.ChangeState(enemy.chaseState);
            return;
        }
    }
}
