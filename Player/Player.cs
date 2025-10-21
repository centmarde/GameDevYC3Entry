using UnityEngine;
using UnityEngine.UI;

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

    [Header("Entrance Intro")]
    [SerializeField] private float introMoveDistance = 15f;
    [SerializeField] private float introMoveDuration = 2f;
    [SerializeField] private float introFadeDuration = 1f;
    [SerializeField] private AnimationCurve introMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private bool hasPlayedIntro = false;
    private bool isPlayingIntro = false;
    private GameObject fadeOverlay;
    private CanvasGroup fadeCanvasGroup;


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
        
        // Check if this player instance is active and not being destroyed
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        
        stateMachine.Initialize(idleState);

        // Play entrance intro animation on first mount
        if (!hasPlayedIntro)
        {
            SetupFadeUI();
            StartCoroutine(PlayEntranceIntro());
        }
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

    private void SetupFadeUI()
    {
        // Don't setup UI if this player is being destroyed or inactive
        if (!gameObject.activeInHierarchy || !this || this == null)
        {
            return;
        }
        
        // Find or create canvas for fade overlay
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Render on top
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        // Create fade overlay
        fadeOverlay = new GameObject("FadeOverlay");
        fadeOverlay.transform.SetParent(canvas.transform, false);
        
        // Setup RectTransform to cover full screen
        RectTransform rectTransform = fadeOverlay.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // Add Image component for black overlay
        Image image = fadeOverlay.AddComponent<Image>();
        image.color = Color.black;
        
        // Add CanvasGroup for fade control
        fadeCanvasGroup = fadeOverlay.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 1f; // Start fully black
        fadeCanvasGroup.blocksRaycasts = false; // Don't block input
    }

    private System.Collections.IEnumerator PlayEntranceIntro()
    {
        // Don't play intro if this player is being destroyed or inactive
        if (!gameObject.activeInHierarchy || !this || this == null)
        {
            yield break;
        }
        
        isPlayingIntro = true;
        hasPlayedIntro = true;

        // Disable player input during intro
        input.Disable();

        // Store the original spawn position
        Vector3 originalPosition = transform.position;
        
        // Move player back by introMoveDistance to start the entrance
        Vector3 startPosition = originalPosition - transform.forward * introMoveDistance;
        transform.position = startPosition;

        // End position is the original spawn position
        Vector3 endPosition = originalPosition;

        // Change to move state for walk animation
        stateMachine.ChangeState(moveState);
        
        // Simulate forward movement input for animation
        playerMovement.SetMoveInput(Vector2.up);

        float elapsedTime = 0f;

        // Animate the movement forward while fading in
        while (elapsedTime < introMoveDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / introMoveDuration);
            float curveValue = introMoveCurve.Evaluate(normalizedTime);

            // Lerp position using the animation curve
            transform.position = Vector3.Lerp(startPosition, endPosition, curveValue);
            
            // Fade in overlay (fade out from black)
            if (fadeCanvasGroup != null)
            {
                float fadeProgress = Mathf.Clamp01(elapsedTime / introFadeDuration);
                fadeCanvasGroup.alpha = 1f - fadeProgress; // Fade from 1 to 0
            }

            yield return null;
        }

        // Ensure we reach the exact end position
        transform.position = endPosition;
        
        // Ensure fade is complete
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
        }
        
        // Clean up fade overlay
        if (fadeOverlay != null)
        {
            Destroy(fadeOverlay);
        }

        // Stop movement input and return to idle
        playerMovement.SetMoveInput(Vector2.zero);
        stateMachine.ChangeState(idleState);

        // Re-enable player input
        input.Enable();
        isPlayingIntro = false;
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
