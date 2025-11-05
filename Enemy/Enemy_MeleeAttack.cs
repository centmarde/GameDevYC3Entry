using UnityEngine;
using System.Collections;

public class Enemy_MeleeAttack : EnemyAttack
{

    [SerializeField] LayerMask damageableMask;
    [SerializeField] private float playerSlowDuration = 1.0f;
    [SerializeField] private float playerSlowAmount = 0.2f; // 20% of the original speed

    readonly Collider[] hits = new Collider[6];

    // flag the state can read
    public bool Finished;

    public override bool CanAttack(Transform target)
    {
        if (!IsOffCooldown || !target || enemy == null) return false;
        Vector3 to = target.position - enemy.transform.position; to.y = 0f;
        // Use default attack range of 2.0f since we removed it from ScriptableObject
        float attackRange = enemy.AttackRange > 0 ? enemy.AttackRange : 2.0f;
        return to.sqrMagnitude <= attackRange * attackRange;
    }

    public override void BeginAttack(Transform target)
    {
        Finished = false;

    }

    public override void EndAttack()
    {
        ResetCooldown();

    }

    // Anim event (hit frame)
    public void AnimEvent_DealDamage()
    {
        // Use default values since attackRange/attackRadius removed from ScriptableObject
        float attackRange = enemy.AttackRange > 0 ? enemy.AttackRange : 2.0f;
        float attackRadius = enemy.AttackRadius > 0 ? enemy.AttackRadius : 2.0f;
        
        Vector3 center = enemy.AimPoint + enemy.transform.forward * (attackRange * 0.5f);
        int count = Physics.OverlapSphereNonAlloc(center, attackRadius, hits, damageableMask, QueryTriggerInteraction.Ignore);

        // Calculate final damage (base + any bonuses from wave scaling)
        float finalDamage = enemy.AttackDamage;
        EnemyStatModifier statModifier = GetComponent<EnemyStatModifier>();
        if (statModifier != null)
        {
            finalDamage = statModifier.GetModifiedDamage(finalDamage);
        }

        for (int i = 0; i < count; i++)
        {
            var rb = hits[i].attachedRigidbody;
            var health = rb ? rb.GetComponent<Entity_Health>() : hits[i].GetComponentInParent<Entity_Health>();
            var playerMovement = rb ? rb.GetComponent<Player_Movement>() : hits[i].GetComponentInParent<Player_Movement>();
            if (health == null || !health.IsAlive) continue;

            Vector3 hitPoint = hits[i].ClosestPoint(center);
            Vector3 toPoint = hitPoint - hits[i].bounds.center;
            Vector3 hitNormal = toPoint.sqrMagnitude > 1e-6f ? toPoint.normalized : -enemy.transform.forward;

            if (health.TakeDamage(finalDamage, hitPoint, hitNormal, enemy))
            {
                // Apply the slowdown to the player
                if (playerMovement != null)
                {
                    //playerMovement.ApplySlowdown(playerSlowDuration, playerSlowAmount); removed players dont want slow
                }
                break;
            }
        }
    }

    // Anim event near the end of the clip
    public void AnimEvent_AttackEnd()
    {
        Finished = true;
        Debug.Log("attack end");
        EndAttack(); // cooldown only
    }




}
