using UnityEngine;

public abstract class Player_RangeAttack : PlayerAttack
{
    private Player_Combat playerCombat;

    [Header("Ranged Attack Settings")]
    [SerializeField] public Transform muzzle;
    [SerializeField] protected ProjectileSlingshot projectile;

    protected Vector3 cachedDirection;

    // Runtime buffs for all range attacks (accessible by child classes)
    protected float damageBonus = 0f;
    protected float speedBonus = 0f;

    public override float AttackRange => player.Stats.rangeAttackRange;

    public float muzzleHeightOffset => player.Stats.muzzleHeightOffset;
    public float muzzleForwardOffset => player.Stats.muzzleForwardOffset;
    public float DirectionEpsilon => player.Stats.DirectionEpsilon;

    protected override void Awake()
    {
        base.Awake();
        playerCombat = GetComponent<Player_Combat>();
    }

    public override void ExecuteAttack(Vector3 aimDirection)
    {
        if (!IsOffCooldown)
        {
            return;
        }

        // Safety check: use muzzle forward if aim direction is invalid
        if (aimDirection.sqrMagnitude < 0.0001f && muzzle != null)
        {
            cachedDirection = muzzle.forward;
        }
        else
        {
            cachedDirection = aimDirection.normalized;
        }
        ResetCooldown();

        if (player?.playerMovement != null)
        {
            player.playerMovement.movementLocked = true;
            player.playerMovement.StopMovement();
        }
    }

    // Called by WaveManager at the start of every run
    public virtual void ResetRuntimeStats()
    {
        damageBonus = 0f;
        speedBonus = 0f;
    }

    // Called when the player receives a buff after a wave
    public virtual void ApplyBuff(float dmgBonus, float spdBonus)
    {
        damageBonus += dmgBonus;
        speedBonus += spdBonus;
    }

    // Abstract method - must be implemented by child classes (Normal, Scatter, Charged)
    protected abstract void FireProjectile();

    private bool hasEndedThisAttack = false;

    public override void EndAttackInternal()
    {
        // Prevent multiple calls to EndAttack in the same frame
        if (hasEndedThisAttack)
        {
            Debug.LogWarning($"[{GetType().Name}] EndAttackInternal called multiple times! Ignoring duplicate call on instance {GetInstanceID()}");
            return;
        }

        hasEndedThisAttack = true;
        
        // Debug: Verify which instance is calling EndAttack
        Debug.Log($"[{GetType().Name}] EndAttackInternal called on instance {GetInstanceID()}");
        
        FireProjectile();
        if (player?.playerMovement != null)
            player.playerMovement.movementLocked = false;

        // Reset flag after a short delay to allow next attack
        StartCoroutine(ResetEndAttackFlag());
    }

    private System.Collections.IEnumerator ResetEndAttackFlag()
    {
        yield return new WaitForSeconds(0.1f);
        hasEndedThisAttack = false;
    }

    public override bool CanAttack(Transform target) => false;
    public override void BeginAttack(Transform target) { }
}
