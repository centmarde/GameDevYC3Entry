using UnityEngine;
using UnityEngine.UI;

public class Player_ChargedRangeAttack : Player_RangeAttack
{
    private IsoCameraFollow camFollow;
    private float chargeTimer;
    private bool isCharging;
    private bool isFullyCharged;

    [Header("Charge Indicator (Manual Prefab Reference)")]
    [SerializeField] private Image chargeFillImage; // ← assign the Fill image in Inspector
    [SerializeField] private GameObject chargeIndicatorUI; // ← assign the ChargeIndicatorUI GameObject (parent)

    [Header("Charge Colors")]
    [SerializeField] private Color startColor = new Color(0f, 0.3f, 0f, 1f); // dark green
    [SerializeField] private Color endColor = new Color(1f, 0.5f, 0f, 1f);   // orange

    public override float AttackRange => player.Stats.chargedAttackRange;

    protected override void Awake()
    {
        base.Awake();
        camFollow = FindFirstObjectByType<IsoCameraFollow>();

        if (chargeIndicatorUI != null)
            chargeIndicatorUI.SetActive(false);
    }

    public override void ExecuteAttack(Vector3 aimDirection)
    {
        if (!IsOffCooldown) return;

        ResetCooldown();
        isCharging = true;
        isFullyCharged = false;
        chargeTimer = 0f;

        if (chargeIndicatorUI != null)
        {
            chargeIndicatorUI.SetActive(true);
        }

        if (chargeFillImage != null)
        {
            chargeFillImage.fillAmount = 0f;
            chargeFillImage.color = startColor;
        }
    }

    public void TickCharge(float deltaTime)
    {
        if (!isCharging) return;

        chargeTimer += deltaTime;
        chargeTimer = Mathf.Min(chargeTimer, player.Stats.chargedMaxChargeTime);

        Vector3 liveAim = player.playerCombat.GetAimDirection().normalized;
        player.playerCombat.FaceSmooth(liveAim);

        float chargePercent = Mathf.Clamp01(chargeTimer / player.Stats.chargedMaxChargeTime);

        // Update fill + color
        if (chargeFillImage != null)
        {
            chargeFillImage.fillAmount = chargePercent;

            // Dark green → orange as it fills
            chargeFillImage.color = Color.Lerp(startColor, endColor, chargePercent);

            // Optional: mark fully charged
            if (chargePercent >= 1f && !isFullyCharged)
                isFullyCharged = true;
        }

        if (camFollow != null)
            camFollow.SetZoom(chargePercent);
    }

    protected override void FireProjectile() { }

    public override void EndAttackInternal()
    {
        if (!isCharging) return;

        if (chargeIndicatorUI != null)
            chargeIndicatorUI.SetActive(false);

        if (isFullyCharged)
        {
            Vector3 releaseDirection = player.playerCombat.GetAimDirection().normalized;
            float chargePercent = Mathf.Clamp01(chargeTimer / player.Stats.chargedMaxChargeTime);
            float multiplier = Mathf.Lerp(
                player.Stats.chargedMinChargeMultiplier,
                player.Stats.chargedMaxChargeMultiplier,
                chargePercent
            );

            FireChargedProjectile(releaseDirection, multiplier);
        }

        player.playerMovement.isAiming = false;
        if (player.playerMovement != null)
            player.playerMovement.movementLocked = false;

        if (camFollow != null)
            camFollow.ResetZoom();

        isCharging = false;
        isFullyCharged = false;
    }

    private void FireChargedProjectile(Vector3 direction, float multiplier)
    {
        if (projectile == null || muzzle == null) return;

        if (direction.sqrMagnitude < 0.0001f)
            direction = muzzle.forward;
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

        bool isCritical = player.Stats.RollCriticalHit();

        float baseDamage = player.Stats.projectileDamage + damageBonus;
        float chargedDamage = baseDamage * multiplier;
        float finalDamage = isCritical
            ? chargedDamage * player.Stats.criticalDamageMultiplier
            : chargedDamage;
        float finalSpeed = player.Stats.chargedAttackSpeed + speedBonus;

        ProjectileTracer tracer = p.gameObject.AddComponent<ProjectileTracer>();
        float chargePercent = chargeTimer / player.Stats.chargedMaxChargeTime;
        tracer.SetChargingState(chargePercent);
        if (isCritical)
            tracer.SetCriticalHit();

        p.Launch(direction * finalSpeed, finalDamage, this, isCritical);
    }
}
