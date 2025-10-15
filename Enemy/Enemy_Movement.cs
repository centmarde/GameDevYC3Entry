using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{

    [SerializeField] private EnemyStatData_SO enemyStats;
    protected virtual EnemyStatData_SO Stats => enemyStats;

    private Rigidbody rb;
    private Transform player;

    private float chaseRadiusSqr, stopDistanceSqr, aggroRadiusSqr, homeStopDistanceSqr, leashRadiusSqr;

    // Time scale for slow motion effects
    private float timeScale = 1f;

    private Vector3 patrolDirXZ;
    private float patrolT;
    private float patrolIdleTimer;
    private int patrolDirSign = 1;
    private Vector3 origPos;

    private void Awake()
    {


        rb = GetComponent<Rigidbody>();

    }

    public void Init(EnemyStatData_SO stats, Vector3 home, Vector3 patrolDir, Transform playerRef)
    {
        enemyStats = stats;
        origPos = home;
        patrolDirXZ = new Vector3(patrolDir.x, 0f, patrolDir.z).normalized;
        player = playerRef;
        
        // precompute squares if stats are valid:
        if (Stats != null)
        {
            chaseRadiusSqr = Stats.chaseRadius * Stats.chaseRadius;
            stopDistanceSqr = Stats.stopDistance * Stats.stopDistance;
            aggroRadiusSqr = Stats.aggroRadius * Stats.aggroRadius;
            homeStopDistanceSqr = Stats.homeStopDistance * Stats.homeStopDistance;
            leashRadiusSqr = Stats.leashRadius * Stats.leashRadius;
        }
    }


    public bool PlayerWithinAggro() =>
      player != null && (player.position - transform.position).sqrMagnitude <= aggroRadiusSqr;

    public bool PlayerWithinChaseWindow() =>
        player != null && (player.position - transform.position).sqrMagnitude <= chaseRadiusSqr;

    public bool EnemyWithinLeash() =>
        (transform.position - origPos).sqrMagnitude <= leashRadiusSqr;

    public bool AtHome() =>
        (transform.position - origPos).sqrMagnitude <= homeStopDistanceSqr;

    // Set time scale for slow motion effects
    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Max(0f, scale); // Ensure it's never negative
    }

    public void LookAtPlayer()
    {
        if (!player) return;
        Vector3 to = player.position - transform.position; to.y = 0f;
        if (to.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(
                rb.rotation, targetRot, Stats.turnSpeed * Time.deltaTime * 60f * timeScale));
        }
    }

    public void MoveToPlayer()
    {
        if (!player) return;
        Vector3 to = player.position - transform.position; to.y = 0f;
        float distSqr = to.sqrMagnitude;

        if (distSqr <= chaseRadiusSqr && distSqr > stopDistanceSqr)
        {
            Vector3 step = to.normalized * Stats.chaseSpeed * Time.deltaTime * timeScale;
            rb.MovePosition(rb.position + step);
        }
    }


    public bool PatrolStep(float dt)
    {
        float scaledDt = dt * timeScale; // Apply time scale to delta time
        float dT = (Stats.patrolMoveSpeed / Mathf.Max(0.001f, Stats.patrolRadius)) * scaledDt * patrolDirSign;
        float nextT = patrolT + dT;

        bool hitEnd = false;
        if (nextT > 1f) { nextT = 1f; hitEnd = true; patrolDirSign = -1; }
        else if (nextT < -1f) { nextT = -1f; hitEnd = true; patrolDirSign = 1; }

        Vector3 targetPos = origPos + patrolDirXZ * (Stats.patrolRadius * nextT);
        Vector3 delta = targetPos - rb.position; delta.y = 0f;

        // clamp to max distance this frame
        float maxStep = Stats.patrolMoveSpeed * scaledDt;
        if (delta.sqrMagnitude <= maxStep * maxStep)
            rb.MovePosition(targetPos);
        else
            rb.MovePosition(rb.position + delta.normalized * maxStep);

        // rotate only if we actually stepped
        if (delta.sqrMagnitude > 1e-6f)
        {
            Quaternion face = Quaternion.LookRotation(delta.normalized, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, face, Stats.turnSpeed * scaledDt * 60f));
        }

        patrolT = nextT;
        return hitEnd;
    }


    public void StartPatrolIdleTimer() => patrolIdleTimer = Stats.patrolIdleDuration;

    public bool TickPatrolIdle(float dt)
    {
        patrolIdleTimer -= dt * timeScale; // Apply time scale to idle timer
        return patrolIdleTimer <= 0f;
    }

    public void ReturnHome(float dt)
    {
        float scaledDt = dt * timeScale; // Apply time scale to delta time
        Vector3 toHome = origPos - transform.position; toHome.y = 0f;
        float dist = toHome.magnitude;

        if (dist > 1e-4f)
        {
            Quaternion face = Quaternion.LookRotation(toHome / Mathf.Max(dist, 1e-4f), Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, face, Stats.turnSpeed * scaledDt * 60f));
        }

        float maxStep = Stats.moveSpeed * scaledDt;
        if (dist <= Mathf.Max(Stats.homeStopDistance, maxStep))
        {
            rb.MovePosition(origPos);       // snap to home
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        rb.MovePosition(rb.position + (toHome / dist) * maxStep);
    }






}
