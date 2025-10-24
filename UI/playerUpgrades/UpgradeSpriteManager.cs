using UnityEngine;

/// <summary>
/// Manages sprite assignments for different upgrade types
/// </summary>
[System.Serializable]
public class UpgradeSpriteManager
{
    [Header("Upgrade Background Images")]
    [SerializeField] private Sprite damageUpgradeSprite;
    [SerializeField] private Sprite maxHealthUpgradeSprite;
    [SerializeField] private Sprite healUpgradeSprite;
    [SerializeField] private Sprite criticalChanceUpgradeSprite;
    [SerializeField] private Sprite criticalDamageUpgradeSprite;
    [SerializeField] private Sprite evasionUpgradeSprite;
    [SerializeField] private Sprite circlingProjectilesUpgradeSprite;
    [SerializeField] private Sprite pushWaveUpgradeSprite;
    [SerializeField] private Sprite extraHandUpgradeSprite;
    [SerializeField] private Sprite defenseUpgradeSprite;
    [SerializeField] private Sprite blinkDistanceUpgradeSprite;
    [SerializeField] private Sprite blinkCooldownUpgradeSprite;
    [SerializeField] private Sprite dashCooldownUpgradeSprite;
    [SerializeField] private Sprite blinkDashSpeedUpgradeSprite;
    [SerializeField] private Sprite defaultUpgradeSprite;
    
    /// <summary>
    /// Get the sprite for a specific upgrade type
    /// </summary>
    public Sprite GetSprite(PlayerUpgradeData.UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case PlayerUpgradeData.UpgradeType.Damage:
                return damageUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.MaxHealth:
                return maxHealthUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.Heal:
                return healUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.CriticalChance:
                return criticalChanceUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.CriticalDamage:
                return criticalDamageUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.Evasion:
                return evasionUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.UpgradeCirclingProjectiles:
                return circlingProjectilesUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.UpgradePushWave:
                return pushWaveUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.UpgradeExtraHand:
                return extraHandUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.UpgradeDefense:
                return defenseUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.UpgradeBlinkDistance:
                return blinkDistanceUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.ReduceBlinkCooldown:
                return blinkCooldownUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.ReduceDashCooldown:
                return dashCooldownUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeData.UpgradeType.UpgradeBlinkDashSpeed:
                return blinkDashSpeedUpgradeSprite ?? defaultUpgradeSprite;
            default:
                return defaultUpgradeSprite;
        }
    }
}
