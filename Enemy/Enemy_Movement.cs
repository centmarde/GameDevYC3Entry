using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{

    [SerializeField] private EnemyStatData_SO enemyStats;
    protected virtual EnemyStatData_SO Stats => enemyStats;

    private Rigidbody rb;
    private Transform player;

    // Time scale for slow motion effects
    private float timeScale = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Init(EnemyStatData_SO stats, Vector3 home, Vector3 patrolDir, Transform playerRef)
    {
        enemyStats = stats;
        player = playerRef;
        
        // Auto-find player if not provided
        if (player == null)
        {
            GameObject pGO = GameObject.FindWithTag("Player");
            if (pGO == null)
            {
                pGO = GameObject.Find("Player1") ?? GameObject.Find("Player2") ?? GameObject.Find("Player");
            }
            if (pGO == null)
            {
                Player[] players = FindObjectsOfType<Player>();
                if (players.Length > 0) pGO = players[0].gameObject;
            }
            if (pGO != null) player = pGO.transform;
        }
    }
    
    /// <summary>
    /// Update player reference (useful for runtime player finding)
    /// </summary>
    public void UpdatePlayerReference(Transform newPlayer)
    {
        player = newPlayer;
    }


    public bool PlayerWithinAggro()
    {
        // Always return true to make enemies always chase (no aggro radius limit)
        return player != null;
    }

    // Set time scale for slow motion effects
    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Max(0f, scale); // Ensure it's never negative
    }
    
    // Get player reference
    public Transform GetPlayer() => player;

    public void LookAtPlayer()
    {
        if (!player) return;
        Vector3 to = player.position - transform.position; to.y = 0f;
        if (to.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(to.normalized, Vector3.up);
            // Use a default turn speed of 360 degrees per second
            rb.MoveRotation(Quaternion.RotateTowards(
                rb.rotation, targetRot, 360f * Time.deltaTime * timeScale));
        }
    }

    public void MoveToPlayer()
    {
        if (!player) return;
        Vector3 to = player.position - transform.position; to.y = 0f;
        float distance = to.magnitude;

        // Stop moving when within attack range (use 1.5f default for collision-based combat)
        float effectiveAttackRange = 1.5f;
        if (distance > effectiveAttackRange)
        {
            Vector3 step = to.normalized * Stats.moveSpeed * Time.deltaTime * timeScale;
            rb.MovePosition(rb.position + step);
        }
        else
        {
            // Stop movement when in attack range
            rb.linearVelocity = Vector3.zero;
        }
    }
}
