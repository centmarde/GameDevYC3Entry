using UnityEngine;

public class Player_NormalRangeAttack : Player_RangeAttack
{
    // Runtime buffs
    private float damageBonus = 0f;
    private float speedBonus = 0f;

    // Override to use specific normal attack range from Player_DataSO
    public override float AttackRange => player.Stats.normalAttackRange;

    // ------------------------------------------
    protected override void Awake()
    {
        base.Awake();
    }

    // Called by WaveManager at the start of every run
    public void ResetRuntimeStats()
    {
        damageBonus = 0f;
        speedBonus = 0f;
    }

    //Called when the player receives a buff after a wave
    public void ApplyBuff(float dmgBonus, float spdBonus)
    {
        damageBonus += dmgBonus;
        speedBonus += spdBonus;
    }

    protected override void FireProjectile()
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
}
