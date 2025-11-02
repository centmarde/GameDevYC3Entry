using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Player))]
public class Player_Combat : MonoBehaviour
{
    private Player player;
    private Rigidbody rb;
    private Camera mainCamera;

    private Vector3 cachedAimDirection;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private Player_RangeAttackController rangeAttackController;

    private Vector3 lastStableAimDir;



    [Header("Attack Settings")]
    private PlayerAttack defaultAttack;

    public PlayerAttack currentAttack =>
        rangeAttackController != null && rangeAttackController.CurrentAttack != null
        ? rangeAttackController.CurrentAttack
        : defaultAttack;

    [SerializeField] private float attackCooldown = 0.25f;

    [Header("Attack Sound Settings")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] [Range(0f, 1f)] private float attackSoundVolume = 0.7f;
    private AudioSource audioSource;
    [SerializeField] private float faceTurnSpeed = 8f;
    private float aimSmoothSpeed = 8f;


    private bool canAttack = true;

    private void Awake()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        // Auto-detect default attack (Player_NormalShotAttack)
        if (defaultAttack == null)
        {
            // Try to find the normal shot attack first
            defaultAttack = GetComponent<Player_NormalShotAttack>();
            
            // Fallback to any Player_RangeAttack if normal shot not found
            if (defaultAttack == null)
            {
                defaultAttack = GetComponent<Player_RangeAttack>();
            }
            
            if (defaultAttack == null)
            {
                Debug.LogWarning("[Player_Combat] No default PlayerAttack found! Please attach a Player_NormalShotAttack component.");
            }
        }

        // Setup AudioSource for attack sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound for player attacks
    }

    private void Update()
    {
        // Only rotate with movement if not attacking and not locked
        // Priority: keyboard movement direction over mouse attack direction
        if (canAttack && !player.playerMovement.movementLocked)
        {
            FaceLastKeyboardDirection();
        }
    }


    public void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        var atk = rangeAttackController?.CurrentAttack;
        bool isCharge = atk is Player_ChargedRangeAttack;

        switch (ctx.phase)
        {
            case InputActionPhase.Started:
            case InputActionPhase.Performed:  // Handle both Started and Performed
                if (!canAttack)
                {
                    return;
                }

                canAttack = false;

                if (isCharge)
                {
                    player.RequestStateChange(player.chargedAttackState);
                }
                else
                {
                    StartCoroutine(AttackRoutine());
                }
                break;

            case InputActionPhase.Canceled:
                if (isCharge)
                {
                    player.RequestStateChange(player.idleState);
                    canAttack = true;
                }
                break;
        }
    }




    private IEnumerator AttackRoutine()
    {
        canAttack = false;

        // Play attack sound at the start of the attack
        PlayAttackSound();

        // Get attack direction from mouse
        cachedAimDirection = GetAimDirection();

        // Face attack direction instantly during attack
        FaceInstant(cachedAimDirection);

        var current = player.playerCombat.currentAttack;

        if (current is not Player_ChargedRangeAttack)
            player.playerMovement.movementLocked = true;

        // Tell the state machine which attack state to use
        if (current is Player_ChargedRangeAttack)
        {
            player.RequestStateChange(player.chargedAttackState);
        }
        else if (current is Player_ScatterRangeAttack) //
        {
            player.scatterAttackState.SetCachedAim(cachedAimDirection);
            player.RequestStateChange(player.scatterAttackState);
        }
        else // Default normal ranged attack
        {
            player.rangeAttackState.SetCachedAim(cachedAimDirection);
            player.RequestStateChange(player.rangeAttackState);
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }


    private bool TryGetAimPoint(out Vector3 hitPoint, out Transform hitTarget)
    {
        if (!mainCamera) mainCamera = Camera.main;
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // Priority: check for enemy hit first
        if (Physics.Raycast(ray, out RaycastHit enemyHit, 500f, enemyMask))
        {
            hitPoint = enemyHit.point;
            hitTarget = enemyHit.transform;
            return true;
        }

        // Fallback: ground hit (for free aim)
        if (Physics.Raycast(ray, out RaycastHit groundHit, 500f, groundMask))
        {
            hitPoint = groundHit.point;
            hitTarget = null;
            return true;
        }

        hitPoint = Vector3.zero;
        hitTarget = null;
        return false;
    }


    public Vector3 GetAimDirection(Transform origin = null)
    {
        if (!mainCamera) mainCamera = Camera.main;

        Vector3 originPos = origin ? origin.position : transform.position;
        Vector3 hitPoint;
        Transform hitTarget;

        if (TryGetAimPoint(out hitPoint, out hitTarget))
        {
            Vector3 dir;

            if (hitTarget != null)
            {
                dir = (hitTarget.position - originPos).normalized;
                dir.y = 0f;
            }
            else
            {
                dir = (hitPoint - originPos).normalized;
                dir.y = 0f;
            }

            // Stabilize: only update if direction changes meaningfully
            if (lastStableAimDir == Vector3.zero ||
                Vector3.Angle(lastStableAimDir, dir) > 0.5f)
            {
                lastStableAimDir = dir;
            }

            // Optional smoothing
            lastStableAimDir = Vector3.Lerp(lastStableAimDir, dir, aimSmoothSpeed * Time.deltaTime).normalized;

            return lastStableAimDir;
        }

        // Fallback: use plane hit or last stable dir
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        float projectileHeight = originPos.y;
        Plane projectilePlane = new Plane(Vector3.up, new Vector3(0f, projectileHeight, 0f));

        if (projectilePlane.Raycast(ray, out float enter))
        {
            Vector3 fallbackHit = ray.GetPoint(enter);
            Vector3 dir = (fallbackHit - originPos).normalized;
            dir.y = 0f;
            lastStableAimDir = dir;
            return lastStableAimDir;
        }

        return lastStableAimDir != Vector3.zero ? lastStableAimDir : transform.forward;
    }




    public void FaceInstant(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
        if (rb) rb.MoveRotation(look);
        else transform.rotation = look;
    }

    private void FaceMoveDirection()
    {
        Vector3 moveDir = player.playerMovement.GetIsoDir();
        if (moveDir.sqrMagnitude < 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(moveDir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, look, faceTurnSpeed * 360f * Time.deltaTime);
    }
    
    /// <summary>
    /// Face the direction of the last keyboard input (not mouse attack direction)
    /// This ensures the player faces their last movement direction when idle
    /// </summary>
    private void FaceLastKeyboardDirection()
    {
        // Get the last keyboard movement direction from Player_Movement
        Vector3 lastKeyboardDir = player.playerMovement.GetLastMoveDirection();
        
        // Only rotate if we have a valid direction
        if (lastKeyboardDir.sqrMagnitude < 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(lastKeyboardDir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, look, faceTurnSpeed * 360f * Time.deltaTime);
    }

    public void FaceSmooth(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, look, faceTurnSpeed * 360f * Time.deltaTime);
    }

    /// <summary>
    /// Plays the attack sound effect at the start of the attack
    /// </summary>
    private void PlayAttackSound()
    {
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound, attackSoundVolume);
        }
    }

}
