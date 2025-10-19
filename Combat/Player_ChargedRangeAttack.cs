using UnityEngine;

public class Player_ChargedRangeAttack : Player_RangeAttack
{
    // Runtime buffs
    private float damageBonus = 0f;
    private float speedBonus = 0f;
    
    private IsoCameraFollow camFollow;
    private float chargeTimer;
    private bool isCharging;

    // Override to use specific charged attack range from Player_DataSO
    public override float AttackRange => player.Stats.chargedAttackRange;

    protected override void Awake()
    {
        base.Awake();
        camFollow = FindFirstObjectByType<IsoCameraFollow>();
    }

    // called by WaveManager at the start of every new run
    public void ResetRuntimeStats()
    {
        damageBonus = 0f;
        speedBonus = 0f;
    }

    // called by WaveManager when granting buffs/upgrades
    public void ApplyBuff(float dmgBonus, float spdBonus)
    {
        damageBonus += dmgBonus;
        speedBonus += spdBonus;
    }

    public override void ExecuteAttack(Vector3 aimDirection)
    {
        if (!IsOffCooldown) return;

        ResetCooldown();
        isCharging = true;
        chargeTimer = 0f;
    }

    public void TickCharge(float deltaTime)
    {
        if (!isCharging) return;

        chargeTimer += deltaTime;
        chargeTimer = Mathf.Min(chargeTimer, player.Stats.chargedMaxChargeTime);

        Vector3 liveAim = player.playerCombat.GetAimDirection().normalized;
        player.playerCombat.FaceSmooth(liveAim);

        if (camFollow != null)
        {
            float chargePercent = Mathf.Clamp01(chargeTimer / player.Stats.chargedMaxChargeTime);
            camFollow.SetZoom(chargePercent);
        }
    }

    public override void EndAttack()
    {
        if (!isCharging) return;

        Vector3 releaseDirection = player.playerCombat.GetAimDirection().normalized;

        float chargePercent = Mathf.Clamp01(chargeTimer / player.Stats.chargedMaxChargeTime);
        float multiplier = Mathf.Lerp(player.Stats.chargedMinChargeMultiplier, player.Stats.chargedMaxChargeMultiplier, chargePercent);

        FireChargedProjectile(releaseDirection, multiplier);

        player.playerMovement.isAiming = false;
        if (player.playerMovement != null)
            player.playerMovement.movementLocked = false;

        if (camFollow != null) camFollow.ResetZoom();
        isCharging = false;
    }

    private void FireChargedProjectile(Vector3 direction, float multiplier)
    {
        if (projectile == null || muzzle == null) return;

        // Safety check: use muzzle forward if direction is invalid
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = muzzle.forward;
        }
        direction.Normalize();

        Vector3 spawnPos = muzzle.position + direction * muzzleForwardOffset;
        spawnPos.y -= muzzleHeightOffset;
        Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);

        ProjectileSlingshot p = Instantiate(projectile, spawnPos, rot);
        p.transform.SetParent(null, true);

        var projCol = p.GetComponent<Collider>();
        if (projCol && player)
        {
            foreach (var c in player.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(projCol, c, true);
        }

        // Calculate final damage and speed with buffs from Player_DataSO
        float baseDamage = player.Stats.projectileDamage + damageBonus;
        float finalDamage = baseDamage * multiplier;
        float finalSpeed = player.Stats.chargedAttackSpeed + speedBonus;

        p.Launch(direction * finalSpeed, finalDamage, this);
    }
}
