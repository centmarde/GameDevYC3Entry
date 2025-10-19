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
    [SerializeField] private PlayerAttack defaultAttack;


    public PlayerAttack currentAttack =>
        rangeAttackController != null && rangeAttackController.CurrentAttack != null
        ? rangeAttackController.CurrentAttack
        : defaultAttack;

    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private float faceTurnSpeed = 8f;
    private float aimSmoothSpeed = 8f;


    private bool canAttack = true;

    private void Awake()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Only rotate with movement if not attacking and not locked
        if (canAttack && !player.playerMovement.movementLocked)
        {
            FaceMoveDirection();
        }
    }


    public void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log($"[Player_Combat] OnFirePerformed called - Phase: {ctx.phase}, canAttack: {canAttack}");
        
        var atk = rangeAttackController?.CurrentAttack;
        bool isCharge = atk is Player_ChargedRangeAttack;
        
        Debug.Log($"[Player_Combat] Current attack: {atk?.GetType().Name ?? "NULL"}, isCharge: {isCharge}");

        switch (ctx.phase)
        {
            case InputActionPhase.Started:
            case InputActionPhase.Performed:  // Handle both Started and Performed
                Debug.Log($"[Player_Combat] Started/Performed phase - canAttack: {canAttack}");
                if (!canAttack)
                {
                    Debug.Log("[Player_Combat] Attack blocked - canAttack is false");
                    return;
                }

                canAttack = false;

                if (isCharge)
                {
                    Debug.Log("[Player_Combat] Starting charged attack state");
                    player.RequestStateChange(player.chargedAttackState);
                }
                else
                {
                    Debug.Log("[Player_Combat] Starting attack routine");
                    StartCoroutine(AttackRoutine());
                }
                break;

            case InputActionPhase.Canceled:
                Debug.Log("[Player_Combat] Canceled phase");
                if (isCharge)
                {
                    Debug.Log("[Player_Combat] Canceling charged attack");
                    player.RequestStateChange(player.idleState);
                    canAttack = true;
                }
                break;
        }
    }




    private IEnumerator AttackRoutine()
    {
        Debug.Log("[Player_Combat] AttackRoutine started");
        canAttack = false;

        cachedAimDirection = GetAimDirection();
        Debug.Log($"[Player_Combat] Aim direction: {cachedAimDirection}");

        FaceInstant(cachedAimDirection);

        var current = player.playerCombat.currentAttack;
        Debug.Log($"[Player_Combat] Current attack type: {current?.GetType().Name ?? "NULL"}");

        if (current is not Player_ChargedRangeAttack)
            player.playerMovement.movementLocked = true;

        // Tell the state to start the appropriate attack
        if (current is Player_ChargedRangeAttack)
        {
            Debug.Log("[Player_Combat] Changing to chargedAttackState");
            player.RequestStateChange(player.chargedAttackState);
        }
        else
        {
            Debug.Log($"[Player_Combat] Changing to rangeAttackState with aim: {cachedAimDirection}");
            player.rangeAttackState.SetCachedAim(cachedAimDirection);
            player.RequestStateChange(player.rangeAttackState);
        }

        yield return new WaitForSeconds(attackCooldown);
        Debug.Log("[Player_Combat] AttackRoutine cooldown finished");
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

    public void FaceSmooth(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, look, faceTurnSpeed * 360f * Time.deltaTime);
    }

}
