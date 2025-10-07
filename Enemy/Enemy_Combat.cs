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
        if (!currentAttack)
            currentAttack = defaultAttack
                         ? defaultAttack
                         : GetComponent<EnemyAttack>() ?? GetComponentInChildren<EnemyAttack>();

        if (!currentAttack)
            Debug.LogError($"{name}: No EnemyAttack found/assigned. Add Enemy_MeleeAttack.", this);
    }

    public void SetTarget(Transform t) => Target = (t as Object) ? t : null;

    public bool TryAttack()
    {
        if (!Target || currentAttack == null) return false;
        bool can = currentAttack.CanAttack(Target);
        if (!can) return false;

        currentAttack.BeginAttack(Target);
        return true;
    }


    public void EndAttack() => currentAttack?.EndAttack();


    public void SetAttack(EnemyAttack attack)
    {
        if (!attack) return;
        currentAttack = attack;
    }

    // In Enemy_Combat.cs
    public bool PlayerWithinAttackRange()
    {
        if (Target == null) return false;

        float distanceToTarget = Vector3.Distance(transform.position, Target.position);
        return distanceToTarget <= enemy.AttackRange;
    }
}
