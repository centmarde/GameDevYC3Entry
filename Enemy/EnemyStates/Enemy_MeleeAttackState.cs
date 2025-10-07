
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

        enemy.movement?.LookAtPlayer();
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
