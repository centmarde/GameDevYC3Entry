using UnityEngine;
using UnityEngine.UI;

public class Player_ChargedRangeAttack : Player_RangeAttack
{
    private IsoCameraFollow camFollow;
    private float chargeTimer;
    private bool isCharging;
    private bool isFullyCharged;

    [Header("Charge Indicator UI")]
    [SerializeField] private GameObject chargeIndicatorPrefab;
    private GameObject chargeIndicatorInstance;
    private Image chargeFillImage;
    private Canvas chargeCanvas;

    // Override to use specific charged attack range from Player_DataSO
    public override float AttackRange => player.Stats.chargedAttackRange;

    protected override void Awake()
    {
        base.Awake();
        camFollow = FindFirstObjectByType<IsoCameraFollow>();
        CreateChargeIndicator();
    }

    private void CreateChargeIndicator()
    {
        // Create canvas for charge indicator
        GameObject canvasObj = new GameObject("ChargeIndicatorCanvas");
        canvasObj.transform.SetParent(transform, false);
        chargeCanvas = canvasObj.AddComponent<Canvas>();
        chargeCanvas.renderMode = RenderMode.WorldSpace;
        chargeCanvas.gameObject.SetActive(false);

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2f, 0.3f);
        canvasRect.localPosition = new Vector3(0f, 2.5f, 0f); // Above player

        // Create fill image only (no background)
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvasObj.transform, false);
        chargeFillImage = fillObj.AddComponent<Image>();
        chargeFillImage.color = new Color(1f, 0.8f, 0f, 1f); // Yellow/gold
        chargeFillImage.type = Image.Type.Filled;
        chargeFillImage.fillMethod = Image.FillMethod.Horizontal;
        chargeFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        chargeFillImage.fillAmount = 0f;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
    }

    public override void ExecuteAttack(Vector3 aimDirection)
    {
        if (!IsOffCooldown) return;

        ResetCooldown();
        isCharging = true;
        isFullyCharged = false;
        chargeTimer = 0f;

        // Show charge indicator
        if (chargeCanvas != null)
        {
            chargeCanvas.gameObject.SetActive(true);
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

        // Update charge indicator
        if (chargeFillImage != null)
        {
            chargeFillImage.fillAmount = chargePercent;
            
            // Change color when fully charged
            if (chargePercent >= 1f)
            {
                if (!isFullyCharged)
                {
                    isFullyCharged = true;
                    chargeFillImage.color = new Color(0f, 1f, 0f, 1f); // Green when fully charged
                }
            }
            else
            {
                // Yellow/gold while charging
                chargeFillImage.color = Color.Lerp(new Color(1f, 0.3f, 0f, 1f), new Color(1f, 0.8f, 0f, 1f), chargePercent);
            }
        }

        if (camFollow != null)
        {
            camFollow.SetZoom(chargePercent);
        }
    }

    protected override void FireProjectile()
    {
        // Charged attack doesn't use the standard FireProjectile
        // It uses FireChargedProjectile with charge multiplier instead
        // Debug.LogWarning("[ChargedRangeAttack] FireProjectile called directly - use FireChargedProjectile instead!");
    }

    public override void EndAttackInternal()
    {
        if (!isCharging) return;

        // Hide charge indicator
        if (chargeCanvas != null)
        {
            chargeCanvas.gameObject.SetActive(false);
        }

        // Only fire if fully charged
        if (isFullyCharged)
        {
            Vector3 releaseDirection = player.playerCombat.GetAimDirection().normalized;
            float chargePercent = Mathf.Clamp01(chargeTimer / player.Stats.chargedMaxChargeTime);
            float multiplier = Mathf.Lerp(player.Stats.chargedMinChargeMultiplier, player.Stats.chargedMaxChargeMultiplier, chargePercent);

            FireChargedProjectile(releaseDirection, multiplier);
        }

        player.playerMovement.isAiming = false;
        if (player.playerMovement != null)
            player.playerMovement.movementLocked = false;

        if (camFollow != null) camFollow.ResetZoom();
        isCharging = false;
        isFullyCharged = false;
    }

    private void FireChargedProjectile(Vector3 direction, float multiplier)
    {
        // Debug.Log($"[ChargedRangeAttack] FireChargedProjectile called with multiplier {multiplier:F2}");
        
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

        // Roll for critical hit
        bool isCritical = player.Stats.RollCriticalHit();
        
        // Calculate final damage and speed with buffs from Player_DataSO
        float baseDamage = player.Stats.projectileDamage + damageBonus;
        float chargedDamage = baseDamage * multiplier;
        float finalDamage = isCritical ? chargedDamage * player.Stats.criticalDamageMultiplier : chargedDamage;
        float finalSpeed = player.Stats.chargedAttackSpeed + speedBonus;

        p.Launch(direction * finalSpeed, finalDamage, this, isCritical);
    }
}
