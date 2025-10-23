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
    [SerializeField] private Sprite unlockSkillUpgradeSprite;
    [SerializeField] private Sprite projectileCountUpgradeSprite;
    [SerializeField] private Sprite projectileDamageUpgradeSprite;
    [SerializeField] private Sprite projectileRadiusUpgradeSprite;
    [SerializeField] private Sprite projectileSpeedUpgradeSprite;
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
            case PlayerUpgradeManager.UpgradeType.UnlockCirclingProjectiles:
                return unlockSkillUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileCount:
                return projectileCountUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileDamage:
                return projectileDamageUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileRadius:
                return projectileRadiusUpgradeSprite ?? defaultUpgradeSprite;
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileSpeed:
                return projectileSpeedUpgradeSprite ?? defaultUpgradeSprite;
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
