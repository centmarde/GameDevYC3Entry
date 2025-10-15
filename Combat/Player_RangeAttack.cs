using UnityEngine;

public class Player_RangeAttack : PlayerAttack
{
    private Player_Combat playerCombat;
    [Header("Ranged Attack")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private ProjectileSlingshot projectile;

    private Vector3 cachedDirection;

    public override float AttackRange => player != null && player.Stats != null ? player.Stats.rangeAttackRange : 10f;


    protected override void Awake()
    {
        base.Awake();
        playerCombat = GetComponent<Player_Combat>();
    }

    public override void ExecuteAttack(Vector3 aimDirection)
    {
        if (!IsOffCooldown) return;
        if (player == null || player.Stats == null)
        {
            Debug.LogWarning($"{name}: Cannot execute attack - player or stats is null");
            return;
        }

        cachedDirection = aimDirection.normalized;
        ResetCooldown();

        // Lock movement
        if (player.playerMovement != null)
        {
            player.playerMovement.movementLocked = true;
            player.playerMovement.StopMovement();
        }
    }

    public override void EndAttack()
    {
        FireProjectile();
        if (player != null && player.playerMovement != null)
            player.playerMovement.movementLocked = false;
    }

    private void FireProjectile()
    {
        if (projectile == null || muzzle == null)
        {
            Debug.LogWarning($"{name}: Cannot fire projectile - projectile prefab or muzzle is null");
            return;
        }

        if (player == null || player.Stats == null)
        {
            Debug.LogWarning($"{name}: Cannot fire projectile - player or stats is null");
            return;
        }

        Vector3 dir = cachedDirection.sqrMagnitude > 0.0001f
            ? cachedDirection
            : muzzle.forward;

        Vector3 spawnPos = muzzle.position + dir.normalized * 0.25f;
        spawnPos.y -= 0.3f; 
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        ProjectileSlingshot p = Instantiate(projectile, spawnPos, rot);
        if (p == null)
        {
            Debug.LogError($"{name}: Failed to instantiate projectile");
            return;
        }

        p.transform.SetParent(null, true);

        // Ignore collision with player
        var projCol = p.GetComponent<Collider>();
        if (projCol != null && player != null)
        {
            foreach (var c in player.GetComponentsInChildren<Collider>())
            {
                if (c != null)
                    Physics.IgnoreCollision(projCol, c, true);
            }
        }

        // Roll for critical hit
        bool isCritical = player.Stats.RollCriticalHit();
        float finalDamage = player.Stats.CalculateDamage(isCritical);
        
        // CRITICAL: Launch projectile with damage - this is what damages enemies!
        p.Launch(dir.normalized * player.Stats.projectileSpeed, finalDamage, this, isCritical);
    }

    public override bool CanAttack(Transform target) => false;
    public override void BeginAttack(Transform target) { }
}
