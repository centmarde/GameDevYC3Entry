using UnityEngine;

[RequireComponent(typeof(Enemy))]
public abstract class EnemyAttack : MonoBehaviour
{
    protected Enemy enemy;
    float attackCd;

    protected virtual void Awake() => enemy = GetComponent<Enemy>();
    protected void Update() { if (attackCd > 0f) attackCd = Mathf.Max(0, attackCd - Time.deltaTime); }

    public bool IsOffCooldown => attackCd <= 0f;
    protected void ResetCooldown() => attackCd = enemy.AttackCooldown;
    protected float CooldownRemaining => attackCd;

    public abstract bool CanAttack(Transform target);
    public abstract void BeginAttack(Transform target);  // play anim, lock movement if needed
    public abstract void EndAttack();                    // usually called after anim
}
