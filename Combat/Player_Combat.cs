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

    [Header("Attack Settings")]
    [SerializeField] private PlayerAttack defaultAttack;
    public PlayerAttack currentAttack { get; private set; }

    [SerializeField] private float attackCooldown = 0.25f;
    [SerializeField] private float faceTurnSpeed = 8f;

    private bool canAttack = true;

    private void Awake()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        currentAttack = defaultAttack;
    }

    private void Update()
    {
        if (canAttack && !player.playerMovement.movementLocked)
        {
            FaceMoveDirection();
        }
    }


    public void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && canAttack)
            StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;

        player.playerMovement.movementLocked = true;

        cachedAimDirection = GetAimDirection();

        FaceInstant(cachedAimDirection);

        player.rangeAttackState.SetCachedAim(cachedAimDirection);
        player.RequestStateChange(player.rangeAttackState);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }


    public Vector3 GetAimDirection(Transform origin = null)
    {
        if (!mainCamera) mainCamera = Camera.main;

        Vector3 originPos = origin ? origin.position : transform.position;
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        Plane plane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 dir = (hitPoint - originPos).normalized;
            dir.y = 0f;
            return dir;
        }

        return transform.forward;
    }

    private void FaceInstant(Vector3 dir)
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
}
