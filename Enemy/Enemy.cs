using UnityEngine;

public class Enemy : Entity, ITargetable
{
    // ========= Stats / Targeting =========
    [SerializeField] private EnemyStatData_SO enemyStats;
    protected virtual EnemyStatData_SO Stats => enemyStats;
    public float AttackDamage => enemyStats != null ? enemyStats.attackDamage : 10f;
    public float AttackRadius => enemyStats != null ? enemyStats.attackRadius : 2f;
    public float AttackRange => enemyStats != null ? enemyStats.attackRange : 2f;
    public float AttackCooldown => enemyStats != null ? enemyStats.attackCooldown : 1f;


    public Enemy_Movement movement {  get; private set; }  

    public Enemy_Combat combat { get; private set; }


    [SerializeField] private Transform aimPoint;
    public Transform Transform => transform;
    public Vector3 AimPoint => aimPoint != null ? aimPoint.position : transform.position + Vector3.up * 1.2f;
    public bool IsAlive => health != null && health.IsAlive;

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

        // Check if enemyStats is assigned before using it
        if (Stats == null)
        {
            enabled = false; // Disable component to prevent further errors
            return;
        }

        if (health != null) health.SetMaxHealth(Stats.maxHealth);

        var pGO = GameObject.FindWithTag("Player");

        if (pGO != null) player = pGO.GetComponent<Player>();
        if (combat != null) combat.SetTarget(player != null ? player.transform : null);

        if (movement != null)
        {
            movement.Init(Stats, transform.position, transform.forward, player != null ? player.transform : null);
        }

        idleState = new Enemy_IdleState(this, stateMachine, "isIdle");
        moveState = new Enemy_MoveState(this, stateMachine, "isMoving");
        chaseState = new Enemy_ChaseState(this, stateMachine, "isChasing");
        returnHomeState = new Enemy_ReturnHomeState(this, stateMachine, "isChasing");
        meleeAttackState = new Enemy_MeleeAttackState(this, stateMachine, "isAttacking");
    }

    protected override void Start()
    {
        base.Start();
        if (Stats != null && stateMachine != null && idleState != null)
        {
            stateMachine.Initialize(idleState);
        }
    }


    public override void EntityDeath()
    {
        base.EntityDeath(); // stop motion, trigger anim if any

        // IMPORTANT: Notify wave manager about enemy death
        if (deathTracker != null)
        {
            deathTracker.NotifyDeath();
        }

        // finally, remove the object after the animation window
        float delay = Stats != null ? Stats.deathDelay : 2f;
        Destroy(gameObject, delay);
    }

    private void OnDrawGizmos()
    {
        if (Stats == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Stats.aggroRadius);
    }
}
