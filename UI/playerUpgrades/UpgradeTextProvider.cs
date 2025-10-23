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
                
            case PlayerUpgradeManager.UpgradeType.UpgradeCirclingProjectiles:
                int currentLevel = upgradeManager.GetCirclingProjectilesLevel();
                int maxLevel = upgradeManager.GetCirclingProjectilesMaxLevel();
                
                if (currentLevel == 0)
                {
                    // Not obtained yet - show as unlock
                    return $"CIRCLING PROJECTILES\n<size=24>Unlock Skill (Level 1/{maxLevel})</size>";
                }
                else if (currentLevel < maxLevel)
                {
                    // Show level upgrade
                    return $"CIRCLING PROJECTILES\n<size=24>Level {currentLevel} → {currentLevel + 1}</size>";
                }
                else
                {
                    // Max level reached (shouldn't appear but handle it)
                    return $"CIRCLING PROJECTILES\n<size=24>MAX LEVEL ({maxLevel})</size>";
                }
                
            case PlayerUpgradeManager.UpgradeType.UpgradePushWave:
                int pushLevel = upgradeManager.GetPushWaveLevel();
                int pushMaxLevel = upgradeManager.GetPushWaveMaxLevel();
                
                if (pushLevel == 0)
                {
                    // Not obtained yet - show as unlock
                    return $"FIREFLIES\n<size=24>Unlock Skill (Level 1/{pushMaxLevel})</size>";
                }
                else if (pushLevel < pushMaxLevel)
                {
                    // Show level upgrade
                    return $"FIREFLIES\n<size=24>Level {pushLevel} → {pushLevel + 1}</size>";
                }
                else
                {
                    // Max level reached (shouldn't appear but handle it)
                    return $"FIREFLIES\n<size=24>MAX LEVEL ({pushMaxLevel})</size>";
                }
                
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
                
            case PlayerUpgradeManager.UpgradeType.UpgradeCirclingProjectiles:
                return "<b><color=#C7CEEA>CIRCLING PROJECTILES</color></b>\n\nUnlock or upgrade this powerful passive skill! Each level increases:\n• <b>Projectile Count</b> (+1 per level)\n• <b>Damage</b> (+5 per level)\n• <b>Orbit Radius</b> (+0.5m per level)\n• <b>Orbit Speed</b> (+15°/s per level)\n\n<i>Max Level: 10 - A versatile skill that grows with you!</i>";
                
            case PlayerUpgradeManager.UpgradeType.UpgradePushWave:
                return "<b><color=#FFD700>FIREFLIES</color></b>\n\nUnlock or upgrade this magical automatic skill! Glowing fireflies orbit and damage enemies. Each level increases:\n• <b>Radius</b> (+0.2m per level)\n• <b>Push Force</b> (+1 per level)\n• <b>Damage</b> (+1 per level)\n• <b>Activation Speed</b> (4s → 2s at max level)\n\n<i>Max Level: 10 - Beautiful and deadly firefly swarm with lights!</i>";
                
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
