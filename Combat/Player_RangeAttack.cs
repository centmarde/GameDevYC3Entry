using UnityEngine;

public class Player_RangeAttack : PlayerAttack
{
    private Player_Combat playerCombat;
    [Header("Ranged Attack")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private ProjectileSlingshot projectile;

    private Vector3 cachedDirection;

    public override float AttackRange => player.Stats.rangeAttackRange;


    protected override void Awake()
    {
        base.Awake();
        playerCombat = GetComponent<Player_Combat>();
    }

    public override void ExecuteAttack(Vector3 aimDirection)
    {
        if (!IsOffCooldown) return;

        cachedDirection = aimDirection.normalized;
        ResetCooldown();

        // Lock movement
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

    private void FireProjectile()
    {
        if (projectile == null || muzzle == null) return;

        Vector3 dir = cachedDirection.sqrMagnitude > 0.0001f
            ? cachedDirection
            : muzzle.forward;

        Vector3 spawnPos = muzzle.position + dir.normalized * 0.25f;
        spawnPos.y -= 0.3f; 
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        ProjectileSlingshot p = Instantiate(projectile, spawnPos, rot);
        p.transform.SetParent(null, true);

        // Ignore collision with player
        var projCol = p.GetComponent<Collider>();
        if (projCol && player)
        {
            foreach (var c in player.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(projCol, c, true);
        }

        p.Launch(dir.normalized * player.Stats.projectileSpeed, player.Stats.projectileDamage, this);
    }

    public override bool CanAttack(Transform target) => false;
    public override void BeginAttack(Transform target) { }
}
