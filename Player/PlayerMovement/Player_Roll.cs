using UnityEngine;
using System.Collections;

public class Player_Roll : MonoBehaviour
{
    [Header("Roll Settings")]
    public float rollSpeed = 5f;
    public float rollDuration = 1f;

    private Rigidbody rb;
    private Player player;

    [Header("Invulnerability Settings")]
    public bool grantInvulnerability = true;
    public float invulnStartTime = 0.1f;
    public float invulnDuration = 0.6f;
    private bool invulnerable;

    [Header("Cooldown Settings")]
    public float rollCooldown = 3f;
    private bool onCooldown = false;

    private Vector3 rollDir;
    private bool rollingActive;
    private float rollTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
    }

    public void BeginRoll(Vector3 direction)
    {
        rollDir = direction;
        rollDir.y = 0f;
        rollDir.Normalize();

        rollTimer = rollDuration;
        rollingActive = true;

        if (grantInvulnerability)
            StartCoroutine(HandleInvulnerability());

        // start cooldown timer
        StartCoroutine(RollCooldownRoutine());
    }

    public void EndRoll()
    {
        rollingActive = false;
    }

    public bool IsRolling => rollingActive;
    public bool IsInvulnerable => invulnerable;
    public bool IsOnCooldown => onCooldown;

    public void Tick()
    {
        if (!rollingActive) return;

        rollTimer -= Time.deltaTime;

        Vector3 newPos = rb.position + rollDir * rollSpeed * Time.deltaTime;
        rb.MovePosition(newPos);

        if (rollTimer <= 0f)
            EndRoll();
    }

    private IEnumerator HandleInvulnerability()
    {
        // wait before granting invuln
        yield return new WaitForSeconds(invulnStartTime);

        invulnerable = true;
        yield return new WaitForSeconds(invulnDuration);
        invulnerable = false;
    }

    private IEnumerator RollCooldownRoutine()
    {
        onCooldown = true;
        yield return new WaitForSeconds(rollCooldown);
        onCooldown = false;
    }
}
