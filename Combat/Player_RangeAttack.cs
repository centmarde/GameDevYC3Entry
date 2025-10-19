using UnityEngine;

public class Player_RangeAttack : PlayerAttack
{
    private Player_Combat playerCombat;

    [Header("Ranged Attack Settings")]
    [SerializeField] public Transform muzzle;
    [SerializeField] protected ProjectileSlingshot projectile;

    protected Vector3 cachedDirection;

    // Runtime buffs for default normal attack behavior
    private float damageBonus = 0f;
    private float speedBonus = 0f;

    public override float AttackRange => player.Stats.normalAttackRange;

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

    protected virtual void FireProjectile()
    {
        if (projectile == null || muzzle == null) return;

        // Get aim direction with fallback
        Vector3 dir = cachedDirection.sqrMagnitude > DirectionEpsilon
           ? cachedDirection
           : muzzle.forward;
        
        // Safety check: ensure direction is valid
        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = muzzle.forward;
        }
        dir.Normalize();

        // Calculate spawn position
        Vector3 spawnPos = muzzle.position + dir * muzzleForwardOffset;
        spawnPos.y += muzzleHeightOffset;

        // Create rotation facing the direction
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        // Instantiate SINGLE projectile for normal shot
        ProjectileSlingshot p = Instantiate(projectile, spawnPos, rot);
        p.transform.SetParent(null, true);

        // Ignore collision with player
        var projCol = p.GetComponent<Collider>();
        if (projCol && player)
        {
            foreach (var c in player.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(projCol, c, true);
        }

        // Calculate final damage and speed with buffs from Player_DataSO
        float finalDamage = player.Stats.projectileDamage + damageBonus;
        float finalSpeed = player.Stats.normalAttackSpeed + speedBonus;

        // Launch single projectile with normal attack stats
        p.Launch(
            dir * finalSpeed,
            finalDamage,
            this
        );
    }

    public override void EndAttack()
    {
        FireProjectile();
        if (player?.playerMovement != null)
            player.playerMovement.movementLocked = false;
    }

    public override bool CanAttack(Transform target) => false;
    public override void BeginAttack(Transform target) { }
}
