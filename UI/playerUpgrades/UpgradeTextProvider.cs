using UnityEngine;

/// <summary>
/// Provides text content for upgrade buttons and tooltips
/// </summary>
public static class UpgradeTextProvider
{
    /// <summary>
    /// Get button text for an upgrade type
    /// </summary>
    public static string GetButtonText(PlayerUpgradeManager.UpgradeType upgradeType, PlayerUpgradeManager upgradeManager)
    {
        switch (upgradeType)
        {
            case PlayerUpgradeManager.UpgradeType.Damage:
                float currentDamage = upgradeManager.GetCurrentDamage();
                float damageUpgrade = upgradeManager.GetDamageUpgradeAmount();
                return $"DAMAGE\n<size=24>{currentDamage:F1} → {currentDamage + damageUpgrade:F1}</size>";
                
            case PlayerUpgradeManager.UpgradeType.MaxHealth:
                float currentMaxHealth = upgradeManager.GetCurrentHealth();
                float maxHealthUpgrade = upgradeManager.GetMaxHealthUpgradeAmount();
                return $"MAX HEALTH\n<size=24>{currentMaxHealth:F0} → {currentMaxHealth + maxHealthUpgrade:F0}</size>";
                
            case PlayerUpgradeManager.UpgradeType.Heal:
                return $"HEAL\n<size=24>Restore to Full Health</size>";
                
            case PlayerUpgradeManager.UpgradeType.CriticalChance:
                float currentCritChance = upgradeManager.GetCurrentCriticalChance();
                float critChanceUpgrade = upgradeManager.GetCriticalChanceUpgradeAmount();
                return $"CRIT CHANCE\n<size=24>{currentCritChance:F1}% → {Mathf.Min(currentCritChance + critChanceUpgrade, 100f):F1}%</size>";
                
            case PlayerUpgradeManager.UpgradeType.CriticalDamage:
                float currentCritDamage = upgradeManager.GetCurrentCriticalDamage();
                float critDamageUpgrade = upgradeManager.GetCriticalDamageUpgradeAmount();
                return $"CRIT DAMAGE\n<size=24>{currentCritDamage:F2}x → {currentCritDamage + critDamageUpgrade:F2}x</size>";
                
            case PlayerUpgradeManager.UpgradeType.Evasion:
                float currentEvasion = upgradeManager.GetCurrentEvasion();
                float evasionUpgrade = upgradeManager.GetEvasionChanceUpgradeAmount();
                return $"EVASION\n<size=24>{currentEvasion:F1}% → {Mathf.Min(currentEvasion + evasionUpgrade, 100f):F1}%</size>";
                
            case PlayerUpgradeManager.UpgradeType.UnlockCirclingProjectiles:
                return $"UNLOCK SKILL\n<size=24>Circling Projectiles</size>";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileCount:
                if (upgradeManager.GetCurrentProjectileCount() > 0)
                {
                    int currentCount = upgradeManager.GetCurrentProjectileCount();
                    return $"ADD PROJECTILE\n<size=24>{currentCount} → {currentCount + 1}</size>";
                }
                return "ERROR";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileDamage:
                if (upgradeManager.GetCurrentProjectileDamage() > 0)
                {
                    float currentSkillDmg = upgradeManager.GetCurrentProjectileDamage();
                    float skillDmgUpgrade = upgradeManager.GetSkillDamageUpgradeAmount();
                    return $"PROJECTILE DMG\n<size=24>{currentSkillDmg:F1} → {currentSkillDmg + skillDmgUpgrade:F1}</size>";
                }
                return "ERROR";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileRadius:
                if (upgradeManager.GetCurrentProjectileRadius() > 0)
                {
                    float currentRadius = upgradeManager.GetCurrentProjectileRadius();
                    float radiusUpgrade = upgradeManager.GetSkillRadiusUpgradeAmount();
                    return $"ORBIT RADIUS\n<size=24>{currentRadius:F1}m → {currentRadius + radiusUpgrade:F1}m</size>";
                }
                return "ERROR";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileSpeed:
                if (upgradeManager.GetCurrentProjectileSpeed() > 0)
                {
                    float currentSpeed = upgradeManager.GetCurrentProjectileSpeed();
                    float speedUpgrade = upgradeManager.GetSkillSpeedUpgradeAmount();
                    return $"ORBIT SPEED\n<size=24>{currentSpeed:F0}° → {currentSpeed + speedUpgrade:F0}°/s</size>";
                }
                return "ERROR";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeBlinkDistance:
                float currentBlinkDist = upgradeManager.GetCurrentBlinkDistance();
                float blinkDistUpgrade = upgradeManager.GetBlinkDistanceUpgradeAmount();
                return $"BLINK RANGE\n<size=24>{currentBlinkDist:F1}m → {currentBlinkDist + blinkDistUpgrade:F1}m</size>";
                
            case PlayerUpgradeManager.UpgradeType.ReduceBlinkCooldown:
                float currentBlinkCD = upgradeManager.GetCurrentBlinkCooldown();
                float blinkCDReduction = upgradeManager.GetBlinkCooldownReduction();
                float newBlinkCD = Mathf.Max(currentBlinkCD - blinkCDReduction, 0.5f);
                return $"BLINK COOLDOWN\n<size=24>{currentBlinkCD:F1}s → {newBlinkCD:F1}s</size>";
                
            case PlayerUpgradeManager.UpgradeType.ReduceDashCooldown:
                float currentDashCD = upgradeManager.GetCurrentDashCooldown();
                float dashCDReduction = upgradeManager.GetDashCooldownReduction();
                float newDashCD = Mathf.Max(currentDashCD - dashCDReduction, 0.3f);
                return $"DASH COOLDOWN\n<size=24>{currentDashCD:F1}s → {newDashCD:F1}s</size>";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeBlinkDashSpeed:
                float currentSpeed2 = upgradeManager.GetCurrentBlinkDashSpeed();
                float speedUpgrade2 = upgradeManager.GetBlinkDashSpeedUpgrade();
                return $"DASH SPEED\n<size=24>{currentSpeed2:F0} → {currentSpeed2 + speedUpgrade2:F0}</size>";
                
            default:
                return "UNKNOWN";
        }
    }
    
    /// <summary>
    /// Get tooltip text for an upgrade type
    /// </summary>
    public static string GetTooltipText(PlayerUpgradeManager.UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case PlayerUpgradeManager.UpgradeType.Damage:
                return "<b><color=#FF6B6B>DAMAGE</color></b>\n\nIncrease your base attack power. Applies to all attacks including projectiles and dash strikes.\n\n<i>Higher damage means faster enemy elimination.</i>";
                
            case PlayerUpgradeManager.UpgradeType.MaxHealth:
                return "<b><color=#4ECDC4>MAX HEALTH</color></b>\n\nIncrease your maximum health pool. Your current health will remain the same percentage.\n\n<i>More health means better survivability in longer waves.</i>";
                
            case PlayerUpgradeManager.UpgradeType.Heal:
                return "<b><color=#95E1D3>HEAL</color></b>\n\nRestore all health to maximum instantly. Choose this when you're low on health.\n\n<i>Perfect for emergency recovery!</i>";
                
            case PlayerUpgradeManager.UpgradeType.CriticalChance:
                return "<b><color=#FFD93D>CRITICAL CHANCE</color></b>\n\nIncrease the probability of landing critical hits. Critical hits deal bonus damage based on your Critical Damage multiplier.\n\n<i>Synergizes well with Critical Damage upgrades!</i>";
                
            case PlayerUpgradeManager.UpgradeType.CriticalDamage:
                return "<b><color=#F38181>CRITICAL DAMAGE</color></b>\n\nIncrease the damage multiplier when you land a critical hit. Stacks multiplicatively with base damage.\n\n<i>Devastating when combined with high Crit Chance!</i>";
                
            case PlayerUpgradeManager.UpgradeType.Evasion:
                return "<b><color=#A8E6CF>EVASION</color></b>\n\nIncrease your chance to completely avoid incoming damage. When you evade, you take zero damage from that attack.\n\n<i>Great defensive option for risky playstyles!</i>";
                
            case PlayerUpgradeManager.UpgradeType.UnlockCirclingProjectiles:
                return "<b><color=#C7CEEA>UNLOCK: CIRCLING PROJECTILES</color></b>\n\nUnlock a powerful passive skill! Projectiles will orbit around you, automatically damaging nearby enemies.\n\n<i>Great for close-range defense and crowd control!</i>";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileCount:
                return "<b><color=#FFDAC1>ADD PROJECTILE</color></b>\n\nAdd one more circling projectile to your orbit. More projectiles mean more damage output and better coverage.\n\n<i>Maximum: 8 projectiles</i>";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileDamage:
                return "<b><color=#FF8B94>PROJECTILE DAMAGE</color></b>\n\nIncrease the damage dealt by your circling projectiles. Independent from your base weapon damage.\n\n<i>Scale your passive damage output!</i>";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileRadius:
                return "<b><color=#B4A7D6>ORBIT RADIUS</color></b>\n\nIncrease the distance your projectiles orbit from you. Larger radius means wider coverage area.\n\n<i>Hit enemies before they get too close!</i>";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeProjectileSpeed:
                return "<b><color=#FFE66D>ORBIT SPEED</color></b>\n\nIncrease how fast your projectiles rotate around you. Faster rotation means more hits per second.\n\n<i>Higher DPS against clustered enemies!</i>";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeBlinkDistance:
                return "<b><color=#6BCF7F>BLINK RANGE</color></b>\n\nIncrease the maximum distance you can teleport with Blink or Dash. Better mobility and positioning control.\n\n<i>Escape danger or chase enemies more effectively!</i>";
                
            case PlayerUpgradeManager.UpgradeType.ReduceBlinkCooldown:
                return "<b><color=#4A90E2>BLINK COOLDOWN</color></b>\n\nReduce the time between Blink uses. Use your mobility ability more frequently for better repositioning.\n\n<i>Minimum cooldown: 0.5 seconds</i>";
                
            case PlayerUpgradeManager.UpgradeType.ReduceDashCooldown:
                return "<b><color=#E74C3C>DASH COOLDOWN</color></b>\n\nReduce the time between Dash attacks. Attack more frequently with your high-damage dash strike.\n\n<i>Minimum cooldown: 0.3 seconds</i>";
                
            case PlayerUpgradeManager.UpgradeType.UpgradeBlinkDashSpeed:
                return "<b><color=#9B59B6>DASH SPEED</color></b>\n\nIncrease the movement speed during Blink and Dash. Move faster across the battlefield.\n\n<i>Harder for enemies to track and hit you!</i>";
                
            default:
                return "<b>UPGRADE</b>\n\nSelect this upgrade to enhance your character.";
        }
    }
}
