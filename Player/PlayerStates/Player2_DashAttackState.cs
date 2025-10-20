using UnityEngine;

public class Player2_DashAttackState : PlayerState
{
    private Player2 player2;
    private Vector3 cachedAimDirection;
    
    public Player2_DashAttackState(Player2 player, StateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName) 
    {
        this.player2 = player;
    }
    
    public override void Enter()
    {
        base.Enter();
        
        // Get the initial aim direction
        cachedAimDirection = player2.meleeCombat.GetAimDirection();
        
        // Allow free movement while charging with slowdown
        player2.playerMovement.movementLocked = false;
        player2.playerMovement.ApplySlowdown(999f, 0.6f); // Slow down to 60% speed while charging
        
        // Start charging
        player2.dashAttack.StartCharging(cachedAimDirection);
        
        Debug.Log("[Player2_DashAttackState] Started charging dash attack");
    }
    
    public override void Update()
    {
        base.Update();
        
        if (player2.dashAttack.IsCharging)
        {
            // Tick the charge
            player2.dashAttack.TickCharge(Time.deltaTime);
            
            // Continuously update dash direction based on mouse position
            Vector3 newAim = player2.meleeCombat.GetAimDirection();
            player2.dashAttack.UpdateDashDirection(newAim);
            
            // Face the aim direction smoothly while charging
            player2.meleeCombat.FaceSmooth(newAim);
            
            // Store the latest aim direction for dash execution
            cachedAimDirection = newAim;
            
            // Player can move freely while charging (movement speed is already slowed)
        }
        else if (player2.dashAttack.IsExecutingDash)
        {
            // Lock movement during dash execution
            player2.playerMovement.movementLocked = true;
            
            // Wait for dash to complete
            // The dash attack handles its own movement
        }
        else
        {
            // Dash is complete, return to appropriate state
            player2.playerMovement.movementLocked = false;
            
            if (player2.playerMovement.moveInput.sqrMagnitude > 0.001f)
                player2.RequestStateChange(player2.moveState);
            else
                player2.RequestStateChange(player2.idleState);
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // Release the charge when exiting (triggers dash)
        if (player2.dashAttack.IsCharging)
        {
            player2.dashAttack.ReleaseCharge();
        }
        
        // Reset movement
        player2.playerMovement.ApplySlowdown(0.1f, 1f); // Reset to normal speed
        player2.playerMovement.movementLocked = false;
        
        Debug.Log("[Player2_DashAttackState] Exited dash attack state");
    }
}
