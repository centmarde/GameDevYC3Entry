public class Enemy_ChaseState : EnemyState
{


    public Enemy_ChaseState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
      
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // 1. Check if the player is in range to attack.
        bool isInAttackRange = enemy.combat.CurrentAttack.CanAttack(enemy.combat.Target);

        // 2. Transition to the attack state if the enemy is ready.
        if (isInAttackRange)
        {
            stateMachine.ChangeState(enemy.meleeAttackState);
            return;
        }

        // 3. If not in attack range, continue to chase.
        enemy.movement.LookAtPlayer();
        enemy.movement.MoveToPlayer();

        // 4. Check if the player is too far away to chase.
        if (!enemy.movement.EnemyWithinLeash() || !enemy.movement.PlayerWithinChaseWindow())
        {
            stateMachine.ChangeState(enemy.returnHomeState);
            return;
        }
    }

}
