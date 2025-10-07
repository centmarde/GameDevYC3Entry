using UnityEngine;

public class Enemy_ReturnHomeState : EnemyState
{
    public Enemy_ReturnHomeState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        // Walk straight back to HOME
        enemy.movement.ReturnHome(Time.deltaTime);

        // When close enough to home, resume patrol
        if (enemy.movement.AtHome())
        {
            stateMachine.ChangeState(enemy.moveState);
            return;
        }
    }
}
