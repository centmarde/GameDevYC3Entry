using UnityEngine;

public class MonsterTrigger : MonoBehaviour
{
    [Header("Codex Reference")]
    public MonsterEntry monsterEntry;

    [Header("Trigger Settings")]
    public float triggerRadius = 15f;      // distance at which player unlocks entry
    private bool hasTriggered = false;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogWarning("[EnemyCodexTrigger] Player not found! Make sure your Player GameObject is tagged 'Player'.");
        }

        if (monsterEntry == null)
        {
            Debug.LogWarning($"[EnemyCodexTrigger] Missing CodexEntry on {gameObject.name}. Please assign one in the Inspector!");
        }
    }

    private void Update()
    {
        if (hasTriggered || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        //  Debug distance to see when it’s within range
        if (distance < triggerRadius + 1f) // log only when close
            Debug.Log($"[EnemyCodexTrigger] Distance to player: {distance:F1}");

        if (distance <= triggerRadius)
        {
            hasTriggered = true;

            // 🔥Call the Codex Manager to unlock the entry
            if (MonsterBookManager.Instance != null)
            {
                Debug.Log("[EnemyCodexTrigger] Calling CodexManager.Instance.UnlockEntry()");
                MonsterBookManager.Instance.UnlockEntry((monsterEntry));
            }
            else
            {
                Debug.LogError("[EnemyCodexTrigger] CodexManager.Instance is NULL! Make sure there is a CodexManager in the scene.");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Optional: Draw radius in Scene View for debugging
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
