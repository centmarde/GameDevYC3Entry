using UnityEngine;

public class Enemy : Entity, ITargetable
{
    // ========= Stats / Targeting =========
    [SerializeField] private EnemyStatData_SO enemyStats;
    protected virtual EnemyStatData_SO Stats => enemyStats;
    public float AttackDamage => enemyStats.attackDamage;
    public float AttackRadius => enemyStats.attackRadius;
    public float AttackRange => enemyStats.attackRange;
    public float AttackCooldown => enemyStats.attackCooldown;


    public Enemy_Movement movement {  get; private set; }  

    public Enemy_Combat combat { get; private set; }


    [SerializeField] private Transform aimPoint;
    public Transform Transform => transform;
    public Vector3 AimPoint => aimPoint ? aimPoint.position : transform.position + Vector3.up * 1.2f;
    public bool IsAlive => GetComponent<Entity_Health>()?.IsAlive ?? true;

    private Entity_Health health;
    private EnemyDeathTracker deathTracker;

    private Player player;



    public Enemy_MoveState moveState { get; private set; }
    public Enemy_ChaseState chaseState { get; private set; }
    public Enemy_ReturnHomeState returnHomeState { get; private set; }
    public Enemy_IdleState idleState { get; private set; }
    public Enemy_MeleeAttackState meleeAttackState { get; private set; }




    protected override void Awake()
    {
        base.Awake();

        combat = GetComponent<Enemy_Combat>();
        movement = GetComponent<Enemy_Movement>();
        health = GetComponent<Entity_Health>();
        deathTracker = GetComponent<EnemyDeathTracker>();

        if (health) health.SetMaxHealth(Stats.maxHealth);

        var pGO = GameObject.FindWithTag("Player");

        if (pGO != null) player = pGO.GetComponent<Player>();
        combat?.SetTarget(player ? player.transform : null);



        movement.Init(Stats, transform.position, transform.forward, player ? player.transform : null);

        idleState = new Enemy_IdleState(this, stateMachine, "isIdle");
        moveState = new Enemy_MoveState(this, stateMachine, "isMoving");
        chaseState = new Enemy_ChaseState(this, stateMachine, "isChasing");
        returnHomeState = new Enemy_ReturnHomeState(this, stateMachine, "isChasing");
        meleeAttackState = new Enemy_MeleeAttackState(this, stateMachine, "isAttacking");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }


    public override void EntityDeath()
    {
        base.EntityDeath(); // stop motion, trigger anim if any

        // IMPORTANT: Notify wave manager about enemy death
        if (deathTracker != null)
        {
            deathTracker.NotifyDeath();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no EnemyDeathTracker component! Wave system may not track this enemy properly.");
        }

        // finally, remove the object after the animation window
        Destroy(gameObject, Stats.deathDelay);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Stats.aggroRadius);
    }
}
