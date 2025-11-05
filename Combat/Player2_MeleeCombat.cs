using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Player2))]
public class Player2_MeleeCombat : MonoBehaviour
{
    private Player2 player2;
    private Rigidbody rb;
    private Camera mainCamera;
    
    private Vector3 cachedAimDirection;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask groundMask;
    
    private Vector3 lastStableAimDir;
    
    [Header("Attack Settings")]
    [SerializeField] private float faceTurnSpeed = 8f;
    private float aimSmoothSpeed = 8f;
    
    private bool canAttack = true;
    private bool isCharging = false;
    
    private void Awake()
    {
        player2 = GetComponent<Player2>();
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }
    
    private void Update()
    {
        // Only rotate with movement if not in combat states and not locked
        bool isInCombatState = player2.CurrentState == player2.dashAttackState ||
                               player2.CurrentState == player2.blinkState;
        
        if (!isInCombatState && !player2.playerMovement.movementLocked)
        {
            FaceLastKeyboardDirection();
        }
    }
    
    private void FaceLastKeyboardDirection()
    {
        // Get the last non-zero movement direction from Player_Movement
        Vector3 lastMoveDir = player2.playerMovement.GetLastMoveDirection();
        
        if (lastMoveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(lastMoveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, look, faceTurnSpeed * 360f * Time.deltaTime);
        }
    }
    
    public void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        switch (ctx.phase)
        {
            case InputActionPhase.Started:
            case InputActionPhase.Performed:
                if (!canAttack || player2.dashAttack.IsOnCooldown)
                {
                    return;
                }
                
                canAttack = false;
                isCharging = true;
                
                // Start charging the dash attack
                // The state will handle facing direction during charge
                player2.RequestStateChange(player2.dashAttackState);
                break;
                
            case InputActionPhase.Canceled:
                if (isCharging)
                {
                    // Release happens automatically when exiting the state
                    isCharging = false;
                    canAttack = true;
                }
                break;
        }
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
        Vector3 moveDir = player2.playerMovement.GetIsoDir();
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
