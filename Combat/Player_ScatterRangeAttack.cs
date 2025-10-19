using UnityEngine;

public class Player_ScatterRangeAttack : Player_RangeAttack
{
    // Runtime buffs
    private float damageBonus = 0f;
    private float speedBonus = 0f;

    // Override to use scatter-specific range from Player_DataSO
    public override float AttackRange => player.Stats.scatterAttackRange;

    protected override void Awake()
    {
        base.Awake();
    }

    public void ResetRuntimeStats()
    {
        damageBonus = 0f;
        speedBonus = 0f;
    }

    public void ApplyBuff(float dmgBonus, float speedBonus)
    {
        damageBonus += dmgBonus;
        speedBonus += speedBonus;
    }

    protected override void FireProjectile()
    {
        if (projectile == null || muzzle == null) return;

        // Safety check: use muzzle forward if cached direction is invalid
        Vector3 baseDir = cachedDirection.sqrMagnitude > DirectionEpsilon 
            ? cachedDirection 
            : muzzle.forward;
        baseDir.Normalize();

        // Calculate final damage and speed with buffs from Player_DataSO
        // Scatter damage is base projectile damage divided by pellet count
        float pelletDamage = (player.Stats.projectileDamage + damageBonus) / player.Stats.scatterPelletCount;
        float pelletSpeed = player.Stats.scatterPelletSpeed + speedBonus;

        for (int i = 0; i < player.Stats.scatterPelletCount; i++)
        {
            // Slight random Y-axis rotation to simulate spread
            Quaternion spreadRot = Quaternion.Euler(0f, Random.Range(-player.Stats.scatterSpreadAngle, player.Stats.scatterSpreadAngle), 0f);
            Vector3 dir = spreadRot * baseDir;

            Vector3 spawnPos = muzzle.position + dir.normalized * muzzleForwardOffset;
            spawnPos.y -= muzzleHeightOffset;

            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            ProjectileSlingshot p = Instantiate(projectile, spawnPos, rot);
            p.transform.SetParent(null, true);

            var projCol = p.GetComponent<Collider>();
            if (projCol && player)
            {
                foreach (var c in player.GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(projCol, c, true);
            }

            p.Launch(dir.normalized * pelletSpeed, pelletDamage, this);
        }
    }
}
