using UnityEngine;

[RequireComponent(typeof(Player))]
public abstract class PlayerAttack : MonoBehaviour
{
    protected Player player;

    [SerializeField] protected float attackCooldown = 0.45f;
    private float attackCd; // internal timer

    public abstract float AttackRange { get; }

    protected virtual void Awake()
    {
        player = GetComponent<Player>();
    }

    protected void Update()
    {
        if (attackCd > 0f)
        {
            attackCd -= Time.deltaTime;
            if (attackCd < 0f) attackCd = 0f;
        }
    }

    // --- Cooldown helpers ---
    protected bool IsOffCooldown => attackCd <= 0f;
    protected void ResetCooldown() => attackCd = attackCooldown;
    protected float CooldownRemaining => attackCd;

    // --- Old compatibility (still usable for melee) ---
    public abstract bool CanAttack(Transform target);
    public abstract void BeginAttack(Transform target);
    
    // Internal method called by animation event handler - not directly by Unity
    public abstract void EndAttackInternal();

    // --- NEW: Universal free-aim attack entry point ---
    public virtual void ExecuteAttack(Vector3 aimDirection)
    {
        // Default: do nothing (override in subclasses)
    }
}
