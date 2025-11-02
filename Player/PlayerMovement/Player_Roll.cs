using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class Player_Roll : MonoBehaviour
{
    [Header("Roll Settings")]
    public float rollDistance = 6f;
    public float rollDuration = 0.8f;
    private float rollSpeed;

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
    private float cooldownRemaining = 0f;

    private Vector3 rollDir;
    private bool rollingActive;
    private float rollTimer;

    // 🔔 Events for UI / VFX
    public event System.Action OnRollStarted;
    public event System.Action OnRollReady;

    // Public read-only properties
    public bool IsRolling => rollingActive;
    public bool IsInvulnerable => invulnerable;
    public bool IsOnCooldown => onCooldown;
    public float CooldownRemaining => cooldownRemaining;
    public float CooldownDuration => rollCooldown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
        invuln = GetComponent<Player_Invulnerability>();
    }

    private void Update()
    {
        // Handle cooldown ticking (unscaled so UI keeps animating when paused)
        if (onCooldown)
        {
            cooldownRemaining -= Time.unscaledDeltaTime;
            if (cooldownRemaining <= 0f)
            {
                cooldownRemaining = 0f;
                onCooldown = false;
                OnRollReady?.Invoke(); // tell UI cooldown is done
            }
        }
    }

    public void BeginRoll(Vector3 direction)
    {
        rollDir = direction.normalized;

        // Calculate roll speed
        rollSpeed = rollDistance / rollDuration;

        rollTimer = rollDuration;
        rollingActive = true;

        // Start invulnerability window
        if (grantInvulnerability && invuln != null)
            StartCoroutine(HandleInvulnerability());

        // Start cooldown
        onCooldown = true;
        cooldownRemaining = rollCooldown;
        OnRollStarted?.Invoke(); // tell UI to start cooldown
    }

    public void EndRoll()
    {
        rollingActive = false;
    }

    public void Tick()
    {
        if (!rollingActive) return;

        rollTimer -= Time.deltaTime;

        // Move smoothly during roll
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
}
