using UnityEngine;

public class Player_ChargedRangeAttack : Player_RangeAttack
{

    [Header("Base Data")]
    [SerializeField] private Player_ChargedRangeAtkData_SO baseData;
        
    private Player_ChargedRuntimeData runtimeData;
    private IsoCameraFollow camFollow;

    private float chargeTimer;
    private bool isCharging;

    protected override void Awake()
    {
        base.Awake();
        camFollow = FindFirstObjectByType<IsoCameraFollow>();

        // create fresh runtime data from base SO
        runtimeData = new Player_ChargedRuntimeData(baseData);
    }

    // called by WaveManager at the start of every new run
    public void ResetRuntimeStats()
    {
        runtimeData = new Player_ChargedRuntimeData(baseData);
    }

    // called by WaveManager when granting buffs/upgrades
    public void ApplyBuff(float dmgBonus, float spdBonus)
    {
        runtimeData.ApplyUpgrade(dmgBonus, spdBonus);
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
        chargeTimer = Mathf.Min(chargeTimer,runtimeData.maxChargeTime);

        Vector3 liveAim = player.playerCombat.GetAimDirection().normalized;
        player.playerCombat.FaceSmooth(liveAim);

        if (camFollow != null)
        {
            float chargePercent = Mathf.Clamp01(chargeTimer / runtimeData.maxChargeTime);
            camFollow.SetZoom(chargePercent);
        }
    }

    public override void EndAttack()
    {
        if (!isCharging) return;

        Vector3 releaseDirection = player.playerCombat.GetAimDirection().normalized;

        float chargePercent = Mathf.Clamp01(chargeTimer / runtimeData.maxChargeTime);
        float multiplier = Mathf.Lerp(runtimeData.minChargeMultiplier, runtimeData.maxChargeMultiplier, chargePercent);

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

        // use runtime stats for final values
        float finalDamage = runtimeData.chargedAttackDamage * multiplier;
        float finalSpeed = runtimeData.chargedAttackSpeed;

        p.Launch(direction * finalSpeed, finalDamage, this);
    }
}
