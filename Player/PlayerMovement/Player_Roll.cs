using UnityEngine;
using System.Collections;

public class Player_Roll : MonoBehaviour
{
    [Header("Roll Settings")]
    public float rollDistance = 6f;   // ✅ Total distance to cover
    public float rollDuration = 0.8f; // ✅ Matches animation length
    private float rollSpeed;          // Computed automatically

    private Rigidbody rb;
    private Player player;
    private Player_Invulnerability invuln;

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
        invuln = GetComponent<Player_Invulnerability>();
    }

    public void BeginRoll(Vector3 direction)
    {
        rollDir = direction.normalized;

        // 🔹 Compute speed based on distance and duration
        rollSpeed = rollDistance / rollDuration;

        rollTimer = rollDuration;
        rollingActive = true;

        if (grantInvulnerability && invuln != null)
            StartCoroutine(HandleInvulnerability());

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

        // 🔹 Move the player smoothly over time
        Vector3 newPos = rb.position + rollDir * rollSpeed * Time.deltaTime;
        rb.MovePosition(newPos);

        if (rollTimer <= 0f)
            EndRoll();
    }

    private IEnumerator HandleInvulnerability()
    {
        yield return new WaitForSeconds(invulnStartTime);

        if (invuln != null)
            invuln.SetTemporaryInvulnerability(invulnDuration);
    }

    private IEnumerator RollCooldownRoutine()
    {
        onCooldown = true;
        yield return new WaitForSeconds(rollCooldown);
        onCooldown = false;
    }
}
