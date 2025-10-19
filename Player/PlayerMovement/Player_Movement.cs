using UnityEngine;

public class Player_Movement : MonoBehaviour
{


    //Movement 
    [Header("Movement details")]
    public Vector2 moveInput { get; private set; }
    public Vector2 lastMoveDir { get; private set; }


    public bool movementLocked;

    private float baseMoveSpeed;
    private float turnSpeed;
    private float currentSpeedMultiplier;


    private Vector3 moveVelocityXZ;
    private Quaternion lookRotation;
    private bool hasLookRotation;

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
        Vector3 playerDirection = GetIsoDir();

        if (playerDirection.sqrMagnitude > 0.0001f)
        {
            lastMoveDir = playerDirection;
            RequestMove(playerDirection * MoveSpeed);

            // Only rotate with movement if not aiming
            if (!isAiming)
                RequestLook(playerDirection);
        }
        else
        {
            RequestMove(Vector3.zero);
            if (!isAiming && lastMoveDir.sqrMagnitude > 0.0001f)
                RequestLook(lastMoveDir);
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


        if (player.CurrentState != player.moveState && player.CurrentState != player.chargedAttackState)
        {
            StopMovement();
            RequestMove(Vector3.zero);
            return;
        }


        rb.MovePosition(rb.position + moveVelocityXZ * Time.fixedDeltaTime);

        if (hasLookRotation)
        {
            var next = Quaternion.RotateTowards(rb.rotation, lookRotation, turnSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(next);
        }
    }

    public void SetMoveInput(Vector2 input)
    {
        // Now that the variable is in this script, you can set its value
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
        hasLookRotation = false;
    }

    public Vector3 GetIsoDir()
    {
        Vector3 rawXZ = new Vector3(moveInput.x, 0f, moveInput.y);

        // Ignore tiny noise from keyboard/analog input
        if (rawXZ.sqrMagnitude < 0.02f)
            return Vector3.zero;

        Vector3 iso = Quaternion.Euler(0f, 45f, 0f) * rawXZ;
        return iso.normalized;
    }



    public void ApplySlowdown(float duration, float slowAmount)
    {
        currentSpeedMultiplier = slowAmount;
        slowdownTimer = duration;
    }

}
