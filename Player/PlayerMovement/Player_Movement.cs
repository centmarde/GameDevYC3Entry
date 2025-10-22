using UnityEngine;

public class Player_Movement : MonoBehaviour
{


    //Movement 
    [Header("Movement details")]
    public Vector2 moveInput { get; private set; }
    public Vector3 lastMoveDir { get; private set; }


    public bool movementLocked;

    private float baseMoveSpeed;
    private float turnSpeed;
    private float currentSpeedMultiplier;


    private Vector3 moveVelocityXZ;
    private Quaternion lookRotation;
    private bool hasLookRotation;
    private Vector3 lastNonZeroMoveDir; // Track last non-zero movement direction

    private float slowdownTimer;
    public bool isAiming;


    private Rigidbody rb;
    private Player player;


    private void Awake()
    {
        player = GetComponent<Player>();
        rb = player.GetComponent<Rigidbody>();

        baseMoveSpeed = player.Stats.moveSpeed;
        turnSpeed = player.Stats.turnSpeed;
        currentSpeedMultiplier = player.Stats.currentSpeedMultiplier;
        
        // Initialize lastMoveDir to face forward (down in isometric view)
        lastMoveDir = transform.forward;
        lastNonZeroMoveDir = transform.forward;
    }

    private float MoveSpeed => baseMoveSpeed * currentSpeedMultiplier;


    public void RequestMove(Vector3 velocityXZ)
    {
        moveVelocityXZ = new Vector3(velocityXZ.x, 0f, velocityXZ.z);
    }
    public void RequestLook(Vector3 worldDir)
    {
        if (worldDir.sqrMagnitude > 0.0001f)
        {
            lookRotation = Quaternion.LookRotation(worldDir.normalized, Vector3.up);
            hasLookRotation = true;
        }
        else
        {
            hasLookRotation = false;
        }
    }

    private void Update()
    {
        if (movementLocked) return;
        HandleMovement();
        UpdateSlowdown();
    }

    private void HandleMovement()
    {
        // Get current movement direction from input (real-time)
        Vector3 playerDirection = GetIsoDir();

        if (playerDirection.sqrMagnitude > 0.0001f)
        {
            // CRITICAL: Update both movement directions in real-time
            lastMoveDir = playerDirection;
            lastNonZeroMoveDir = playerDirection; // Store as last valid direction
            
            RequestMove(playerDirection * MoveSpeed);

            // Always update facing direction in real-time when moving
            if (!isAiming)
            {
                RequestLook(playerDirection);
            }
        }
        else
        {
            // Not moving - stop movement but maintain facing direction
            RequestMove(Vector3.zero);
            
            // Keep facing the last non-zero direction when not moving
            if (!isAiming && lastNonZeroMoveDir.sqrMagnitude > 0.0001f)
            {
                RequestLook(lastNonZeroMoveDir);
            }
        }
    }

    private void UpdateSlowdown()
    {
        if (slowdownTimer > 0f)
        {
            slowdownTimer -= Time.deltaTime;
            if (slowdownTimer <= 0f)
            {
                currentSpeedMultiplier = 1.0f;
                slowdownTimer = 0f;
            }
        }
    }



    private void FixedUpdate()
    {
        if (!rb) return;

        if (movementLocked)
        {
            StopMovement();
            RequestMove(Vector3.zero);
            return;
        }

        // Check if player is in a state that allows movement
        bool canMove = player.CurrentState == player.moveState || 
                       player.CurrentState == player.chargedAttackState ||
                       player.CurrentState == player.idleState;
        
        // For Player2, also allow movement during dash attack charging
        if (player is Player2 player2 && player2.dashAttackState != null)
        {
            canMove = canMove || player.CurrentState == player2.dashAttackState;
        }

        // CRITICAL: Always update rotation based on current look direction
        // This ensures the player always faces the direction of their last input
        if (hasLookRotation)
        {
            var next = Quaternion.RotateTowards(rb.rotation, lookRotation, turnSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(next);
            
            // Update transform.forward to match the rotation (for external systems)
            // This ensures other systems can read the correct facing direction
        }
        else if (lastNonZeroMoveDir.sqrMagnitude > 0.0001f)
        {
            // If no explicit look rotation, use last movement direction
            Quaternion targetRotation = Quaternion.LookRotation(lastNonZeroMoveDir, Vector3.up);
            var next = Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(next);
        }

        if (!canMove)
        {
            StopMovement();
            RequestMove(Vector3.zero);
            return;
        }

        rb.MovePosition(rb.position + moveVelocityXZ * Time.fixedDeltaTime);
    }

    public void SetMoveInput(Vector2 input)
    {
        // Store the input - HandleMovement will process it
        moveInput = input;
    }

    public void StopMovement()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void ClearMovementIntent()
    {
        moveInput = Vector2.zero;
        moveVelocityXZ = Vector3.zero;
        // Don't clear hasLookRotation or lastNonZeroMoveDir - keep the last facing direction
        // This ensures the player maintains their facing direction when movement is cleared
        // hasLookRotation = false;
    }

    public Vector3 GetIsoDir()
    {
        // Convert 2D input to 3D world direction
        Vector3 rawXZ = new Vector3(moveInput.x, 0f, moveInput.y);

        // Ignore tiny noise from keyboard/analog input
        if (rawXZ.sqrMagnitude < 0.02f)
            return Vector3.zero;

        // Apply isometric camera rotation (45 degrees)
        Vector3 iso = Quaternion.Euler(0f, 45f, 0f) * rawXZ;
        return iso.normalized;
    }
    
    /// <summary>
    /// Gets the player's current facing direction (real-time)
    /// This can be used by other systems (like enemies) to know where the player is facing
    /// </summary>
    public Vector3 GetFacingDirection()
    {
        // Return the current forward direction from transform
        // This is updated in real-time by FixedUpdate
        return transform.forward;
    }
    
    /// <summary>
    /// Gets the last non-zero movement direction
    /// Useful for systems that need to know the player's intended direction
    /// </summary>
    public Vector3 GetLastMoveDirection()
    {
        return lastNonZeroMoveDir;
    }



    public void ApplySlowdown(float duration, float slowAmount)
    {
        currentSpeedMultiplier = slowAmount;
        slowdownTimer = duration;
    }

}
