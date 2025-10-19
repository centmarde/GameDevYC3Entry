using UnityEngine;

public class Player_RangeAttack : PlayerAttack
{
    private Player_Combat playerCombat;

    [Header("Ranged Attack Settings")]
    [SerializeField] public Transform muzzle;
    [SerializeField] protected ProjectileSlingshot projectile;

    protected Vector3 cachedDirection;

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

        cachedDirection = aimDirection.normalized;
        ResetCooldown();

        if (player?.playerMovement != null)
        {
            player.playerMovement.movementLocked = true;
            player.playerMovement.StopMovement();
        }
    }

    public override void EndAttack()
    {
        FireProjectile();
        if (player?.playerMovement != null)
            player.playerMovement.movementLocked = false;
    }

    protected virtual void FireProjectile()
    {
        if (projectile == null || muzzle == null) return;


    }

    public override bool CanAttack(Transform target) => false;
    public override void BeginAttack(Transform target) { }
}
