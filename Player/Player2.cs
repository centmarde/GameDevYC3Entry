using UnityEngine;
using UnityEngine.UI;

public class Player2 : Player
{
    [SerializeField] private Player2_DataSO player2Stats;
    public new Player2_DataSO Stats => player2Stats;
    
    public Player2_MeleeCombat meleeCombat { get; private set; }
    public Player2_ChargedDashAttack dashAttack { get; private set; }
    public Player2_ChargeUI chargeUI { get; private set; }
    public Player2_BlinkSkill blinkSkill { get; private set; }
    
    // State Variables specific to Player2
    public Player2_DashAttackState dashAttackState { get; private set; }
    public Player2_BlinkState blinkState { get; private set; }
    
    protected override void Awake()
    {
        // Call base Awake first to initialize Player components
        base.Awake();
        
        // Now initialize Player2-specific components
        meleeCombat = GetComponent<Player2_MeleeCombat>();
        dashAttack = GetComponent<Player2_ChargedDashAttack>();
        chargeUI = GetComponent<Player2_ChargeUI>();
        blinkSkill = GetComponent<Player2_BlinkSkill>();
        
        // Auto-add ChargeUI if not present
        if (chargeUI == null)
        {
            chargeUI = gameObject.AddComponent<Player2_ChargeUI>();
            Debug.Log("[Player2] Auto-added Player2_ChargeUI component");
        }
        
        // Auto-add BlinkSkill if not present
        if (blinkSkill == null)
        {
            blinkSkill = gameObject.AddComponent<Player2_BlinkSkill>();
            Debug.Log("[Player2] Auto-added Player2_BlinkSkill component");
        }
        
        // Create Player2-specific states
        dashAttackState = new Player2_DashAttackState(this, stateMachine, "isCharging");
        blinkState = new Player2_BlinkState(this, stateMachine, "isOpeningChest");
        
        // Override health settings with Player2 stats if available
        var health = GetComponent<Entity_Health>();
        if (health && player2Stats != null)
        {
            health.SetMaxHealth(player2Stats.maxHealth);
            health.SetEvasionCheck(() => player2Stats.RollEvasion());
        }
    }
    
    protected override void Start()
    {
        // Call base to properly initialize state machine
        base.Start();
    }
    
    private void OnEnable()
    {
        // Enable input system
        input.Enable();
        
        // Unsubscribe base Player combat events
        if (playerCombat != null)
        {
            input.Player.Attack.performed -= playerCombat.OnFirePerformed;
            input.Player.Attack.canceled -= playerCombat.OnFirePerformed;
        }
        
        // Unsubscribe rangeAttackController if it exists
        if (rangeAttackController != null)
        {
            input.Player.SwitchAttackType.performed -= rangeAttackController.OnScroll;
        }
        
        // Subscribe to Player2 melee combat events
        if (meleeCombat != null)
        {
            input.Player.Attack.performed += meleeCombat.OnAttackPerformed;
            input.Player.Attack.canceled += meleeCombat.OnAttackPerformed;
        }
        
        // Subscribe to Roll
        input.Player.Roll.performed += ctx => TryStartRoll_Player2();
        
        // Subscribe to Blink (Space key - you'll need to add this to the Input Actions in Unity Editor)
        // For now, we'll use the Search key (C) as a placeholder for testing
        input.Player.Search.performed += ctx => TryStartBlink();
        
        // Subscribe to Movement Input
        input.Player.Movement.performed += ctx => playerMovement.SetMoveInput(ctx.ReadValue<Vector2>());
        input.Player.Movement.canceled += ctx => playerMovement.SetMoveInput(Vector2.zero);
    }
    
    private void OnDisable()
    {
        // Disable input system
        input.Disable();
        
        // Unsubscribe Player2 melee combat events
        if (meleeCombat != null)
        {
            input.Player.Attack.performed -= meleeCombat.OnAttackPerformed;
            input.Player.Attack.canceled -= meleeCombat.OnAttackPerformed;
        }
        
        // Unsubscribe roll
        input.Player.Roll.performed -= ctx => TryStartRoll_Player2();
        
        // Unsubscribe blink
        input.Player.Search.performed -= ctx => TryStartBlink();
    }
    
    private void TryStartRoll_Player2()
    {
        if (stateMachine.currentState == rollState ||
            stateMachine.currentState == hurtState ||
            stateMachine.currentState == dashAttackState)
            return;
        
        if (playerRoll == null) return;
        if (playerRoll.IsOnCooldown)
            return;
        
        stateMachine.ChangeState(rollState);
    }
    
    private void TryStartBlink()
    {
        // Cannot blink during certain states
        if (stateMachine.currentState == hurtState ||
            stateMachine.currentState == dashAttackState ||
            stateMachine.currentState == rollState ||
            stateMachine.currentState == blinkState)
            return;
        
        if (blinkSkill == null) return;
        if (blinkSkill.IsOnCooldown)
        {
            Debug.Log($"[Player2] Blink is on cooldown!");
            return;
        }
        
        stateMachine.ChangeState(blinkState);
    }
    
    public override void EntityDeath()
    {
        if (player2Stats != null)
        {
            Destroy(gameObject, player2Stats.deathDelay);
        }
        else
        {
            Destroy(gameObject, 0.1f);
        }
        Debug.Log("Player2 died.");
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw interaction radius
        float iRadius = 0.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, iRadius);
        
        // Show forward direction
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * iRadius);
        
        // Show dash attack radius if stats are available
        if (player2Stats != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, player2Stats.dashAttackRadius);
            
            // Show dash distance (now uses same distance as blink)
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, transform.forward * player2Stats.blinkDistance);
        }
    }
}
