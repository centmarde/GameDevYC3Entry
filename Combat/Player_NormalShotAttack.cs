using UnityEngine;

/// <summary>
/// Normal single-shot attack - fires one projectile at a time
/// This is the default range attack for the player
/// </summary>
public class Player_NormalShotAttack : Player_RangeAttack
{
    // Override to use specific normal attack range from Player_DataSO
    public override float AttackRange => player.Stats.normalAttackRange;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void FireProjectile()
    {
        // Debug.Log($"[NormalShotAttack] FireProjectile called on Instance {GetInstanceID()} - firing SINGLE projectile");
        
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

        // Roll for critical hit
        bool isCritical = player.Stats.RollCriticalHit();
        
        // Calculate final damage and speed with buffs from Player_DataSO
        float baseDamage = player.Stats.projectileDamage + damageBonus;
        float finalDamage = isCritical ? baseDamage * player.Stats.criticalDamageMultiplier : baseDamage;
        float finalSpeed = player.Stats.normalAttackSpeed + speedBonus;

        // Add visual tracer effect (like ChargeUI but for projectiles)
        ProjectileTracer tracer = p.gameObject.AddComponent<ProjectileTracer>();
        if (isCritical)
        {
            tracer.SetCriticalHit();
        }

        // Launch single projectile with normal attack stats
        p.Launch(
            dir * finalSpeed,
            finalDamage,
            this,
            isCritical
        );
    }
}
