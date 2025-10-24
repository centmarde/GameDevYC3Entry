using UnityEngine;
using static PlayerUpgradeData;

namespace PlayerUpgrades
{
    /// <summary>
    /// Configuration for all upgrade values
    /// </summary>
    [System.Serializable]
    public class PlayerUpgradeConfig
    {
        [Header("Base Upgrade Values")]
        public float damageUpgradeAmount = 5f;
        public float maxHealthUpgradeAmount = 50f;
        public float healAmount = 30f;
        public float criticalChanceUpgradeAmount = 5f;
        public float criticalDamageUpgradeAmount = 0.25f;
        public float evasionChanceUpgradeAmount = 3f;
        
        [Header("Player2 Specific Upgrades")]
        public float blinkDistanceUpgradeAmount = 1f;
        public float blinkCooldownReduction = 0.3f;
        public float dashCooldownReduction = 0.2f;
        public float blinkDashSpeedUpgrade = 3f;
        
        [Header("Circling Projectiles Skill Upgrades")]
        public float skillProjectileDamageUpgrade = 5f;
        public float skillRadiusUpgrade = 0.5f;
        public float skillSpeedUpgrade = 15f;
        
        [Header("Extra Hand Skill Upgrades")]
        public float extraHandDamagePerLevel = 2f;
        public float extraHandIntervalReductionPerLevel = 0.2f;
        public float extraHandRangePerLevel = 1f;
        
        [Header("Vampire Aura Skill Upgrades")]
        public float vampireAuraBaseLifestealPercentage = 5f; // Lifesteal percentage at level 1 (5% of damage dealt)
        public float vampireAuraLifestealPercentagePerLevel = 1f; // +1% per level
    }
}
