using UnityEngine;

public class Player : Entity
{

    [SerializeField] private Player_DataSO playerStats;
    public Player_DataSO Stats => playerStats;

    private Entity_Health health;
    public PlayerInputSet input { get; private set; }
    public PlayerSkill_Manager skillManager { get; private set; }
    public Player_Movement playerMovement { get; private set; }

    public Player_Roll playerRoll { get; private set; }
    public Player_Combat playerCombat { get; private set; }
    public Player_RangeAttackController rangeAttackController { get; private set; }


    //State Variables
    public Player_MoveState moveState { get; private set; }
    public Player_IdleState idleState { get; private set; }
    public Player_OpenChestState openChestState { get; private set; }
    public Player_RangeAttackState rangeAttackState { get; private set; }
    

    public Player_HurtState hurtState { get; private set; }

    public Player_RollState rollState { get; private set; }

    public Player_ChargedAttackState chargedAttackState { get; private set; }

    public Player_DeathState deathState { get; private set; }


    public EntityState CurrentState => stateMachine.currentState;

    //Attack stats
    public float RangeAttackRange => playerStats.rangeAttackRange;


    [Header("Interact")]
    public float interactRadius = 0.5f;
    public IInteractable pendingInteractable;
    public InteractionProfile pendingProfile;
    public bool interactPressed;


    protected override void Awake()
    {
        base.Awake();

        rb.useGravity = false;


        skillManager = GetComponent<PlayerSkill_Manager>();
        playerMovement = GetComponent<Player_Movement>();
        playerCombat = GetComponent<Player_Combat>();
        rangeAttackController = GetComponent<Player_RangeAttackController>();
        playerRoll = GetComponent<Player_Roll>();

        input = new PlayerInputSet();
        moveState = new Player_MoveState(this, stateMachine, "move");
        idleState = new Player_IdleState(this, stateMachine, "idle");
        openChestState = new Player_OpenChestState(this, stateMachine, "isOpeningChest");
        rangeAttackState = new Player_RangeAttackState(this, stateMachine, "rangeAttack");
        hurtState = new Player_HurtState(this, stateMachine, "hurt");
        rollState = new Player_RollState(this, stateMachine, "isRolling");
        chargedAttackState = new Player_ChargedAttackState(this, stateMachine, "isCharging");
        deathState = new Player_DeathState(this, stateMachine, "isDead");

        health = GetComponent<Entity_Health>();

        if (health && Stats != null)
        {
            health.SetMaxHealth(Stats.maxHealth);
            // Set up evasion system
            health.SetEvasionCheck(() => Stats.RollEvasion());
        }


    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);

    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Attack.performed += playerCombat.OnFirePerformed;
        input.Player.Attack.canceled += playerCombat.OnFirePerformed;
        input.Player.Attack.performed += playerCombat.OnFirePerformed;

        input.Player.SwitchAttackType.performed += rangeAttackController.OnScroll;
        input.Player.Roll.performed += ctx => TryStartRoll();




        // Movement Input
        input.Player.Movement.performed += ctx => playerMovement.SetMoveInput(ctx.ReadValue<Vector2>());
        input.Player.Movement.canceled += ctx => playerMovement.SetMoveInput(Vector2.zero);



    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Attack.performed -= playerCombat.OnFirePerformed;
        input.Player.Attack.canceled -= playerCombat.OnFirePerformed;
        input.Player.Attack.performed -= playerCombat.OnFirePerformed;

        input.Player.SwitchAttackType.performed -= rangeAttackController.OnScroll;
        input.Player.Roll.performed -= ctx => TryStartRoll();


    }

    public void RequestStateChange(PlayerState newState)
    {
        stateMachine.ChangeState(newState);

    }


    public bool TryFindInteractable(float radius, out IInteractable nearestInteractable, out InteractionProfile nearestProfile)
    {

        nearestInteractable = null;
        nearestProfile = null;


        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, radius);

        float closestDistanceSoFar = float.MaxValue;
        Vector3 playerPosition = transform.position;

        foreach (Collider nearbyCollider in nearbyColliders)
        {

            //skip self player collider 
            if (nearbyCollider.transform == transform || nearbyCollider.transform.IsChildOf(transform))
                continue;

            //

            if (!nearbyCollider.TryGetComponent(out IInteractable possibleInteractable))
                continue;

            if (possibleInteractable is Object_Chest chest && chest.isHidden)
                continue;

            Vector3 closestPointOnCollider = nearbyCollider.ClosestPoint(playerPosition);
            float distanceToPlayer = (closestPointOnCollider - playerPosition).magnitude;

            if (distanceToPlayer < closestDistanceSoFar)
            {
                closestDistanceSoFar = distanceToPlayer;
                nearestInteractable = possibleInteractable;
            }
        }

        if (nearestInteractable != null)
        {
            nearestProfile = nearestInteractable.GetProfile();
            return true;
        }

        return false;
    }

    private void TryStartRoll()
    {
        if (stateMachine.currentState == rollState ||
            stateMachine.currentState == hurtState ||
            stateMachine.currentState == rangeAttackState)
            return;

        if (playerRoll == null) return;
        if (playerRoll.IsOnCooldown)
            return;

        stateMachine.ChangeState(rollState);

    }

     public override void EntityDeath()
    {
        Destroy(gameObject, Stats.deathDelay);


        Debug.Log("You died.");
    }

    private void OnDrawGizmosSelected()
    {
        float iRadius = 0.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, iRadius);



        // optional: show forward direction (handy in isometric)
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * iRadius);
    }


}
