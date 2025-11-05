using UnityEngine;
using Object = UnityEngine.Object;

[RequireComponent(typeof(Enemy))]
public class Enemy_Combat : MonoBehaviour
{
    [SerializeField] private EnemyAttack defaultAttack;      // optional convenience
    [SerializeField] private EnemyAttack currentAttack;      // assign or auto-wire
    public EnemyAttack CurrentAttack => currentAttack;

    public Transform Target { get; private set; }
    private Enemy enemy;

    void Awake()
    {
        enemy = GetComponent<Enemy>();

        // Auto-wire if nothing assigned in the Inspector
        if (currentAttack == null)
            currentAttack = defaultAttack != null
                         ? defaultAttack
                         : GetComponent<EnemyAttack>() ?? GetComponentInChildren<EnemyAttack>();

        if (currentAttack == null)
            Debug.LogError($"{name}: No EnemyAttack found/assigned. Add Enemy_MeleeAttack.", this);
    }

    public void SetTarget(Transform t) => Target = (t as Object) ? t : null;

    public bool TryAttack()
    {
        if (Target == null || currentAttack == null) return false;
        bool can = currentAttack.CanAttack(Target);
        if (!can) return false;

        currentAttack.BeginAttack(Target);
        return true;
    }


    public void EndAttack() => currentAttack?.EndAttack();


    public void SetAttack(EnemyAttack attack)
    {
        if (attack == null) return;
        currentAttack = attack;
    }

    // In Enemy_Combat.cs
    public bool PlayerWithinAttackRange()
    {
        if (Target == null) return false;

        float distanceToTarget = Vector3.Distance(transform.position, Target.position);
        // Use default attack range of 2.0f since we removed it from ScriptableObject
        float attackRange = enemy.AttackRange > 0 ? enemy.AttackRange : 2.0f;
        return distanceToTarget <= attackRange;
    }
}
