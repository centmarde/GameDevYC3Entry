using UnityEngine;

public class Player_Movement : MonoBehaviour
{


    //Movement 
    [Header("Movement details")]
    public Vector2 moveInput { get; private set; }


    public bool movementLocked;

    private float moveSpeed;
    private float turnSpeed;
    private float currentSpeedMultiplier;

    
    private Vector3 moveVelocityXZ;
    private Quaternion lookRotation;
    private bool hasLookRotation;

    private float slowdownTimer;

    private Rigidbody rb;
    private Player player;


    private void Awake()
    {
        player = GetComponent<Player>();
        rb = player.GetComponent<Rigidbody>();

        moveSpeed = player.Stats.moveSpeed;
        turnSpeed = player.Stats.turnSpeed;
        currentSpeedMultiplier = player.Stats.currentSpeedMultiplier;
    }


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
        HandleSlowdown();
    }



    private void FixedUpdate()
    {
        if (!rb) return;

        if (movementLocked || player.CurrentState != player.moveState)
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
        moveVelocityXZ = Vector3.zero;
        hasLookRotation = false;
    }

    public Vector3 GetIsoDir()
    {
        Vector3 rawXZ = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 iso = Quaternion.Euler(0f, 45f, 0f) * rawXZ;

        return iso.sqrMagnitude > 0.0001f ? iso.normalized : Vector3.zero;
    }


    private void HandleSlowdown()
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
        Vector3 isoDir = GetIsoDir();
        Vector3 desiredVelocity = isoDir * moveSpeed * currentSpeedMultiplier;
        RequestMove(desiredVelocity);
        if (isoDir.sqrMagnitude > 0.0001f)
        {
            RequestLook(isoDir);
        }
    }

    public void ApplySlowdown(float duration, float slowAmount)
    {
        currentSpeedMultiplier = slowAmount;
        slowdownTimer = duration;
    }

}
