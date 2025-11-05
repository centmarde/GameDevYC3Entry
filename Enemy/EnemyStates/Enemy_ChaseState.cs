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

        // Check if this is a minion (collision-based damage)
        bool isMinion = enemy is Enemy_Minions;
        
        if (isMinion)
        {
            // Minions don't use attack states - just chase continuously
            // They deal damage through collision
            if (enemy.movement != null)
            {
                enemy.movement.LookAtPlayer();
                enemy.movement.MoveToPlayer();
            }
            return;
        }

        // Null safety checks for regular enemies
        if (enemy.combat == null || enemy.combat.CurrentAttack == null || enemy.combat.Target == null)
        {
            // Continue chasing if combat isn't ready yet
            if (enemy.movement != null)
            {
                enemy.movement.LookAtPlayer();
                enemy.movement.MoveToPlayer();
            }
            return;
        }

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

        // 4. Always chase player - no return home check
    }

}
