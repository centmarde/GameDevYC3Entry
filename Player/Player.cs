using UnityEngine;
using UnityEngine.UI;

public class Player : Entity
{

    [SerializeField] private Player_DataSO playerStats;
    public Player_DataSO Stats => playerStats;

    private Entity_Health health;
    public PlayerInputSet input { get; protected set; }
    public PlayerSkill_Manager skillManager { get; private set; }
    public Player_Movement playerMovement { get; private set; }

    public Player_Combat playerCombat { get; private set; }
    public Player_RangeAttackController rangeAttackController { get; private set; }


    //State Variables
    public Player_MoveState moveState { get; private set; }
    public Player_IdleState idleState { get; private set; }
    public Player_RangeAttackState rangeAttackState { get; private set; }

    public Player_ScatterAttackState scatterAttackState { get; private set; }

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
    [SerializeField] private float introFadeDuration = 3f;
    [SerializeField] private AnimationCurve introMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private bool hasPlayedIntro = false;
    private bool isPlayingIntro = false;
    private GameObject fadeOverlay;
    private CanvasGroup fadeCanvasGroup;


    protected override void Awake()
    {
        Debug.Log($"[Player] Awake called for {gameObject.name}");
        base.Awake();
        
        // Double-check animator after base.Awake()
        if (anim == null)
        {
            Debug.LogError($"[Player] Animator is NULL after base.Awake()! GameObject: {gameObject.name}", gameObject);
        }

        if (rb != null)
        {
            rb.useGravity = false;
        }


        skillManager = GetComponent<PlayerSkill_Manager>();
        playerMovement = GetComponent<Player_Movement>();
        playerCombat = GetComponent<Player_Combat>();
        rangeAttackController = GetComponent<Player_RangeAttackController>();

        input = new PlayerInputSet();
        moveState = new Player_MoveState(this, stateMachine, "move");
        idleState = new Player_IdleState(this, stateMachine, "idle");
        rangeAttackState = new Player_RangeAttackState(this, stateMachine, "rangeAttack");
        chargedAttackState = new Player_ChargedAttackState(this, stateMachine, "isCharging");
        scatterAttackState = new Player_ScatterAttackState(this, stateMachine, "scatterAttack");
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
            Debug.LogWarning($"[Player] Start called but GameObject is not active: {gameObject.name}");
            return;
        }
        
        // Final animator check before initializing state machine
        if (anim == null)
        {
            Debug.LogError($"[Player] CRITICAL: Animator is still NULL in Start! Cannot initialize properly. GameObject: {gameObject.name}", gameObject);
            // Last desperate attempt
            ReinitializeAnimator();
            if (anim != null)
            {
                Debug.Log($"[Player] Emergency animator recovery successful!");
            }
            else
            {
                Debug.LogError($"[Player] FATAL: Cannot recover animator! Check prefab structure!", gameObject);
                LogAnimatorDiagnostics();
            }
        }
        else
        {
            // Verify animator is working
            if (!anim.enabled)
            {
                Debug.LogWarning($"[Player] Animator was disabled! Enabling now.", gameObject);
                anim.enabled = true;
            }
            
            if (anim.runtimeAnimatorController == null)
            {
                Debug.LogError($"[Player] Animator has NO RuntimeAnimatorController! Animations cannot play!", gameObject);
                LogAnimatorDiagnostics();
            }
        }
        
        Debug.Log($"[Player] Initializing state machine for {gameObject.name} with animator: {(anim != null ? anim.gameObject.name : "NULL")}");
        stateMachine.Initialize(idleState);

        // Force PhotonGameManager to connect to WaveManager on this player
        if (PhotonGameManager.Instance != null)
        {
            PhotonGameManager.Instance.ForceConnectToWaveManager();
            Debug.Log("[Player] Forced PhotonGameManager to connect to WaveManager on player spawn");
        }

        // Play entrance intro animation on first mount
        if (!hasPlayedIntro)
        {
            SetupFadeUI();
            StartCoroutine(PlayEntranceIntro());
        }
    }

    private void OnEnable()
    {
        // Don't enable input if this player is being destroyed or inactive
        if (!gameObject.activeInHierarchy || !this || this == null)
        {
            return;
        }

        // CRITICAL: Re-validate animator at OnEnable (can become null between Awake and OnEnable)
        if (anim == null)
        {
            Debug.LogWarning($"[Player] Animator is NULL in OnEnable! Attempting recovery for {gameObject.name}");
            if (animatorOverride != null)
            {
                anim = animatorOverride;
            }
            else
            {
                anim = GetComponentInChildren<Animator>(true);
            }

            if (anim == null)
            {
                Debug.LogError($"[Player] FAILED to recover animator in OnEnable for {gameObject.name}!", gameObject);
            }
            else
            {
                Debug.Log($"[Player] Successfully recovered animator in OnEnable: {anim.gameObject.name}");
            }
        }

        if (input == null)
        {
            input = new PlayerInputSet();
        }

        input.Enable();

        if (playerCombat != null)
        {
            input.Player.Attack.performed += playerCombat.OnFirePerformed;
            input.Player.Attack.canceled += playerCombat.OnFirePerformed;
        }

        if (rangeAttackController != null)
        {
            input.Player.SwitchAttackType.performed += rangeAttackController.OnScroll;
        }

        // Movement Input
        if (playerMovement != null)
        {
            input.Player.Movement.performed += ctx => playerMovement.SetMoveInput(ctx.ReadValue<Vector2>());
            input.Player.Movement.canceled += ctx => playerMovement.SetMoveInput(Vector2.zero);
        }

        input.Player.MonsterDex.performed += ctx => ToggleMonsterBook();

    }

    private void OnDisable()
    {
        if (input == null)
            return;

        input.Disable();

        if (playerCombat != null)
        {
            input.Player.Attack.performed -= playerCombat.OnFirePerformed;
            input.Player.Attack.canceled -= playerCombat.OnFirePerformed;
        }

        if (rangeAttackController != null)
        {
            input.Player.SwitchAttackType.performed -= rangeAttackController.OnScroll;
        }

        input.Player.MonsterDex.performed -= ctx => ToggleMonsterBook();

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

    public override void EntityDeath()
    {
        // Push runtime wave data to Photon Cloud (only happens on death)
        if (PhotonGameManager.Instance != null)
        {
            PhotonGameManager.Instance.SaveCurrentWaveToLeaderboard();
            Debug.Log("[Player] Wave progress pushed to Photon Cloud upon death.");
        }
        else
        {
            Debug.LogWarning("[Player] PhotonGameManager instance not found. Wave progress not saved upon death.");
        }

        // Show game over panel immediately
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverPanel();
            Debug.Log("[Player] Game Over panel displayed.");
        }
        else
        {
            Debug.LogError("[Player] UIManager.Instance is null! Cannot show Game Over panel.");
        }
        
        // Delay destruction to allow UI to fully appear
        float delay = Mathf.Max(0.5f, Stats.deathDelay);
        Invoke(nameof(DestroyPlayerAfterDelay), delay);
    }

    private void DestroyPlayerAfterDelay()
    {
        Debug.Log("[Player] Destroying player GameObject after death.");
        Destroy(gameObject);
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

        // Keep player in idle state (no movement)
        stateMachine.ChangeState(idleState);

        float elapsedTime = 0f;

        // Fade in from black without moving
        while (elapsedTime < introFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Fade in overlay (fade out from black)
            if (fadeCanvasGroup != null)
            {
                float fadeProgress = Mathf.Clamp01(elapsedTime / introFadeDuration);
                fadeCanvasGroup.alpha = 1f - fadeProgress; // Fade from 1 to 0
            }

            yield return null;
        }
        
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
    
    /// <summary>
    /// Logs detailed animator diagnostics for debugging
    /// </summary>
    private void LogAnimatorDiagnostics()
    {
        Debug.Log($"=== Animator Diagnostics for {gameObject.name} ===");
        Debug.Log($"  Active in Hierarchy: {gameObject.activeInHierarchy}");
        Debug.Log($"  Active Self: {gameObject.activeSelf}");
        
        Animator[] allAnimators = GetComponentsInChildren<Animator>(true);
        Debug.Log($"  Total Animators found (including inactive): {allAnimators.Length}");
        
        for (int i = 0; i < allAnimators.Length; i++)
        {
            Animator a = allAnimators[i];
            Debug.Log($"    [{i}] Animator on: {a.gameObject.name}");
            Debug.Log($"        - Active: {a.gameObject.activeInHierarchy}");
            Debug.Log($"        - Enabled: {a.enabled}");
            Debug.Log($"        - Controller: {(a.runtimeAnimatorController != null ? a.runtimeAnimatorController.name : "NULL")}");
        }
    }

    private void ToggleMonsterBook()
    {
        if (MonsterDexUI.Instance == null)
            return;

        // Prevent opening the Dex if a discovery popup is showing
        if (MonsterBookUI.Instance != null && MonsterBookUI.Instance.rootPanel.activeSelf)
            return;

        bool isActive = MonsterDexUI.Instance.rootPanel.activeSelf;

        if (isActive)
        {
            MonsterDexUI.Instance.Close();
            Time.timeScale = 1f;
            input.Enable();
        }
        else
        {
            MonsterDexUI.Instance.Open();
            Time.timeScale = 0f;
        }
    }
}
