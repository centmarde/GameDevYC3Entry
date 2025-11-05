
public class Enemy_MeleeAttackState : EnemyState
{
    private Enemy_MeleeAttack melee;

    public Enemy_MeleeAttackState(Enemy enemy, StateMachine stateMachine, string animBoolName)
        : base(enemy, stateMachine, animBoolName) { }

    public override void Enter()
    {
        base.Enter();

        melee = enemy.combat.CurrentAttack as Enemy_MeleeAttack;

        enemy.combat.TryAttack();
    }

    public override void Update()
    {
        base.Update();

        // Keep moving toward player while attacking
        enemy.movement?.LookAtPlayer();
        enemy.movement?.MoveToPlayer();

        // Only try to transition AFTER the attack is marked Finished
        if (melee != null && melee.Finished)
        {
            if (enemy.combat.PlayerWithinAttackRange())
            {
                // Re-enter the attack state
                stateMachine.ChangeState(enemy.meleeAttackState);
            }
            else
            {
                // Otherwise go back to chase
                stateMachine.ChangeState(enemy.chaseState);
            }

            return;
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (melee != null)
        {
            melee.Finished = false;
        }
    }
}
