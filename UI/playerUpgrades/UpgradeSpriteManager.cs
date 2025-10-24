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
    [SerializeField] private Sprite blinkDistanceUpgradeSprite;
    [SerializeField] private Sprite blinkCooldownUpgradeSprite;
    [SerializeField] private Sprite dashCooldownUpgradeSprite;
    [SerializeField] private Sprite blinkDashSpeedUpgradeSprite;
    [SerializeField] private Sprite defaultUpgradeSprite;
    
    /// <summary>
    /// Get the sprite for a specific upgrade type
    /// </summary>
    public Sprite GetSprite(PlayerUpgradeManager.UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case PlayerUpgradeManager.UpgradeType.Damage:
                return damageUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.MaxHealth:
                return maxHealthUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.Heal:
                return healUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.CriticalChance:
                return criticalChanceUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.CriticalDamage:
                return criticalDamageUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.Evasion:
                return evasionUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.UpgradeCirclingProjectiles:
                return circlingProjectilesUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.UpgradePushWave:
                return pushWaveUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.UpgradeExtraHand:
                return extraHandUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.UpgradeBlinkDistance:
                return blinkDistanceUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.ReduceBlinkCooldown:
                return blinkCooldownUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.ReduceDashCooldown:
                return dashCooldownUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.UpgradeBlinkDashSpeed:
                return blinkDashSpeedUpgradeSprite ?? defaultUpgradeSprite;
            default:
                return defaultUpgradeSprite;
        }
    }
}
