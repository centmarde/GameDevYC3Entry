using UnityEngine;

public class Player2_BlinkState : PlayerState
{
    private Player2 player2;
    private float stateTimer;
    private const float BLINK_CHARGE_DURATION = 0.1f; // 1 second charge-up time
    private const float BLINK_RECOVERY_DURATION = 0.1f; // Brief recovery after blink
    private bool hasExecutedBlink;
    
    public Player2_BlinkState(Player2 player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) 
    {
        this.player2 = player;
    }
    
    public override void Enter()
    {
        base.Enter();
        
        stateTimer = 0f;
        hasExecutedBlink = false;
        
        // Lock movement during blink charge-up
        player2.playerMovement.movementLocked = true;
        
        // Check if blink is available
        if (player2.blinkSkill != null && player2.blinkSkill.IsOnCooldown)
        {
            // If blink is on cooldown, immediately exit
            Debug.Log("[Player2_BlinkState] Blink is on cooldown, exiting");
            ExitToAppropriateState();
            return;
        }
        
        Debug.Log("[Player2_BlinkState] Started charging blink (1 second)");
    }
    
    public override void Update()
    {
        base.Update();
        
        stateTimer += Time.deltaTime;
        
        // Charging phase (0 to 1 second)
        if (!hasExecutedBlink && stateTimer >= BLINK_CHARGE_DURATION)
        {
            // Execute the blink after charge-up
            if (player2.blinkSkill != null)
            {
                bool success = player2.blinkSkill.TryBlink();
                
                if (!success)
                {
                    // If blink failed, exit immediately
                    ExitToAppropriateState();
                    return;
                }
                
                hasExecutedBlink = true;
                Debug.Log("[Player2_BlinkState] Blink executed!");
            }
        }
        
        // Recovery phase (brief pause after blink)
        if (hasExecutedBlink && stateTimer >= BLINK_CHARGE_DURATION + BLINK_RECOVERY_DURATION)
        {
            ExitToAppropriateState();
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // Unlock movement
        player2.playerMovement.movementLocked = false;
        
        Debug.Log("[Player2_BlinkState] Exited blink state");
    }
    
    private void ExitToAppropriateState()
    {
        // Return to appropriate state based on input
        if (player2.playerMovement.moveInput.sqrMagnitude > 0.001f)
            player2.RequestStateChange(player2.moveState);
        else
            player2.RequestStateChange(player2.idleState);
    }
}