using UnityEngine;

/// <summary>
/// Provides text content for upgrade buttons and tooltips
/// </summary>
public static class UpgradeTextProvider
{
    /// <summary>
    /// Get button text for an upgrade type
    /// </summary>
    public static string GetButtonText(PlayerUpgradeData.UpgradeType upgradeType, PlayerUpgradeManager upgradeManager)
    {
        switch (upgradeType)
        {
            case PlayerUpgradeData.UpgradeType.Damage:
                float currentDamage = upgradeManager.GetCurrentDamage();
                float damageUpgrade = upgradeManager.GetDamageUpgradeAmount();
                return $"DAMAGE\n<size=24>{currentDamage:F1} → {currentDamage + damageUpgrade:F1}</size>";
                
            case PlayerUpgradeData.UpgradeType.MaxHealth:
                float currentMaxHealth = upgradeManager.GetCurrentHealth();
                float maxHealthUpgrade = upgradeManager.GetMaxHealthUpgradeAmount();
                return $"MAX HEALTH\n<size=24>{currentMaxHealth:F0} → {currentMaxHealth + maxHealthUpgrade:F0}</size>";
                

                
            case PlayerUpgradeData.UpgradeType.CriticalChance:
                float currentCritChance = upgradeManager.GetCurrentCriticalChance();
                float critChanceUpgrade = upgradeManager.GetCriticalChanceUpgradeAmount();
                int critChanceLevel = upgradeManager.GetCriticalChanceUpgradeLevel();
                int maxUpgradeLevelCC = upgradeManager.GetStatUpgradeMaxLevel();
                return $"CRIT CHANCE (Lv{critChanceLevel}/{maxUpgradeLevelCC})\n<size=24>{currentCritChance:F1}% → {Mathf.Min(currentCritChance + critChanceUpgrade, 100f):F1}%</size>";
                
            case PlayerUpgradeData.UpgradeType.CriticalDamage:
                float currentCritDamage = upgradeManager.GetCurrentCriticalDamage();
                float critDamageUpgrade = upgradeManager.GetCriticalDamageUpgradeAmount();
                int critDamageLevel = upgradeManager.GetCriticalDamageUpgradeLevel();
                int maxUpgradeLevelCD = upgradeManager.GetStatUpgradeMaxLevel();
                return $"CRIT DAMAGE (Lv{critDamageLevel}/{maxUpgradeLevelCD})\n<size=24>{currentCritDamage:F2}x → {currentCritDamage + critDamageUpgrade:F2}x</size>";
                
            case PlayerUpgradeData.UpgradeType.Evasion:
                float currentEvasion = upgradeManager.GetCurrentEvasion();
                float evasionUpgrade = upgradeManager.GetEvasionChanceUpgradeAmount();
                int evasionLevel = upgradeManager.GetEvasionUpgradeLevel();
                int maxUpgradeLevelEv = upgradeManager.GetStatUpgradeMaxLevel();
                return $"EVASION (Lv{evasionLevel}/{maxUpgradeLevelEv})\n<size=24>{currentEvasion:F1}% → {Mathf.Min(currentEvasion + evasionUpgrade, 100f):F1}%</size>";
                
            case PlayerUpgradeData.UpgradeType.UpgradeCirclingProjectiles:
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
                
            case PlayerUpgradeData.UpgradeType.UpgradePushWave:
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
                
            case PlayerUpgradeData.UpgradeType.UpgradeExtraHand:
                int extraHandLevel = upgradeManager.GetExtraHandLevel();
                int extraHandMaxLevel = upgradeManager.GetExtraHandMaxLevel();
                
                if (extraHandLevel == 0)
                {
                    // Not obtained yet - show as unlock
                    return $"<color=#00FF00>EXTRA HAND</color>\n<size=24>Unlock Skill (Level 1/{extraHandMaxLevel})</size>";
                }
                else if (extraHandLevel < extraHandMaxLevel)
                {
                    // Show level upgrade
                    return $"<color=#00FF00>EXTRA HAND</color>\n<size=24>Level {extraHandLevel} → {extraHandLevel + 1}</size>";
                }
                else
                {
                    // Max level reached (shouldn't appear but handle it)
                    return $"<color=#00FF00>EXTRA HAND</color>\n<size=24>MAX LEVEL ({extraHandMaxLevel})</size>";
                }
                
            case PlayerUpgradeData.UpgradeType.UpgradeSpearThrow:
                int spearLevel = upgradeManager.GetSpearThrowLevel();
                int spearMaxLevel = upgradeManager.GetSpearThrowMaxLevel();
                
                if (spearLevel == 0)
                {
                    // Not obtained yet - show as unlock
                    return $"<color=#FFD700>SPEAR THROW</color>\n<size=24>Unlock Skill (Level 1/{spearMaxLevel})</size>";
                }
                else if (spearLevel < spearMaxLevel)
                {
                    // Show level upgrade
                    return $"<color=#FFD700>SPEAR THROW</color>\n<size=24>Level {spearLevel} → {spearLevel + 1}</size>";
                }
                else
                {
                    // Max level reached (shouldn't appear but handle it)
                    return $"<color=#FFD700>SPEAR THROW</color>\n<size=24>MAX LEVEL ({spearMaxLevel})</size>";
                }

                
            case PlayerUpgradeData.UpgradeType.UpgradePiccoloFireCracker:
                int piccoloLevel = upgradeManager.GetPiccoloFireCrackerLevel();
                int piccoloMaxLevel = upgradeManager.GetPiccoloFireCrackerMaxLevel();
                
                if (piccoloLevel == 0)
                {
                    // Not obtained yet - show as unlock
                    return $"<color=#FF6600>PICCOLO FIRECRACKER</color>\n<size=24>Unlock Skill (Level 1/{piccoloMaxLevel})</size>";
                }
                else if (piccoloLevel < piccoloMaxLevel)
                {
                    // Show level upgrade with damage and bomb count
                    float piccoloDamage = upgradeManager.GetPiccoloFireCrackerDamage();
                    int currentBombs = upgradeManager.GetPiccoloFireCrackerBombCount();
                    // Calculate next level stats
                    float nextPiccoloDamage = piccoloDamage * 1.15f; // 15% increase
                    int nextBombs = Mathf.Min(2 + Mathf.FloorToInt(piccoloLevel * 0.33f), 5);
                    return $"<color=#FF6600>PICCOLO FIRECRACKER</color>\n<size=24>Level {piccoloLevel} → {piccoloLevel + 1}\nDmg: {piccoloDamage:F0} → {nextPiccoloDamage:F0} | Bombs: {currentBombs} → {nextBombs}</size>";
                }
                else
                {
                    // Max level reached (shouldn't appear but handle it)
                    return $"<color=#FF6600>PICCOLO FIRECRACKER</color>\n<size=24>MAX LEVEL ({piccoloMaxLevel})</size>";
                }
                
            case PlayerUpgradeData.UpgradeType.UpgradeBlinkDistance:
                float currentBlinkDist = upgradeManager.GetCurrentBlinkDistance();
                float blinkDistUpgrade = upgradeManager.GetBlinkDistanceUpgradeAmount();
                int blinkDistLevel = upgradeManager.GetBlinkDistanceUpgradeLevel();
                int maxUpgradeLevelBD = upgradeManager.GetStatUpgradeMaxLevel();
                return $"BLINK RANGE (Lv{blinkDistLevel}/{maxUpgradeLevelBD})\n<size=24>{currentBlinkDist:F1}m → {currentBlinkDist + blinkDistUpgrade:F1}m</size>";
                

                

                

                
            default:
                return "UNKNOWN";
        }
    }
    
    /// <summary>
    /// Get tooltip text for an upgrade type
    /// </summary>
    public static string GetTooltipText(PlayerUpgradeData.UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case PlayerUpgradeData.UpgradeType.Damage:
                return "<b><color=#FF6B6B>DAMAGE</color></b>\n\nIncrease your base attack power. Applies to all attacks including projectiles and dash strikes.\n\n<i>Higher damage means faster enemy elimination.</i>";
                
            case PlayerUpgradeData.UpgradeType.MaxHealth:
                return "<b><color=#4ECDC4>MAX HEALTH</color></b>\n\nIncrease your maximum health pool. Your current health will remain the same percentage.\n\n<i>More health means better survivability in longer waves.</i>";
                
            case PlayerUpgradeData.UpgradeType.CriticalChance:
                return "<b><color=#FFD93D>CRITICAL CHANCE</color></b>\n\nIncrease the probability of landing critical hits. Critical hits deal bonus damage based on your Critical Damage multiplier.\n\n<i>Synergizes well with Critical Damage upgrades!</i>";
                
            case PlayerUpgradeData.UpgradeType.CriticalDamage:
                return "<b><color=#F38181>CRITICAL DAMAGE</color></b>\n\nIncrease the damage multiplier when you land a critical hit. Stacks multiplicatively with base damage.\n\n<i>Devastating when combined with high Crit Chance!</i>";
                
            case PlayerUpgradeData.UpgradeType.Evasion:
                return "<b><color=#A8E6CF>EVASION</color></b>\n\nIncrease your chance to completely avoid incoming damage. When you evade, you take zero damage from that attack.\n\n<i>Great defensive option for risky playstyles!</i>";
                
            case PlayerUpgradeData.UpgradeType.UpgradeCirclingProjectiles:
                return "<b><color=#C7CEEA>GRANDFATHERS AXE</color></b>\n\nUnlock or upgrade this powerful passive skill! Each level increases:\n• <b>Projectile Count</b> (+1 per level)\n• <b>Damage</b> (+5 per level)\n• <b>Orbit Radius</b> (+0.5m per level)\n• <b>Orbit Speed</b> (+15°/s per level)\n\n<i>Max Level: 10 - A versatile skill that grows with you!</i>";
                
            case PlayerUpgradeData.UpgradeType.UpgradePushWave:
                return "<b><color=#FFD700>LUNAR FIREFLIES</color></b>\n\nUnlock or upgrade this magical automatic skill! Glowing fireflies orbit and damage enemies. Each level increases:\n• <b>Radius</b> (+0.2m per level)\n• <b>Push Force</b> (+1 per level)\n• <b>Damage</b> (+1 per level)\n• <b>Activation Speed</b> (4s → 2s at max level)\n\n<i>Max Level: 10 - Beautiful and deadly firefly swarm with lights!</i>";
                
            case PlayerUpgradeData.UpgradeType.UpgradeExtraHand:
                return "<b><color=#00FF00>GRANDMOTHERS CANDY</color></b>\n\nUnlock or upgrade this auto-targeting skill! A Grand Mothers Candy shoots projectiles at nearby enemies. Each level increases:\n• <b>Damage</b> (+2 per level)\n• <b>Fire Rate</b> (-0.2s interval per level)\n• <b>Range</b> (+1m per level)\n• <b>Projectiles</b> (+1 on even levels 2,4,6,8,10)\n\n<i>Max Level: 10 - Never miss with this auto-aiming companion!</i>";
                
            case PlayerUpgradeData.UpgradeType.UpgradeSpearThrow:
                return "<b><color=#FFD700>SPEAR THROW</color></b>\n\nUnlock or upgrade this powerful ranged skill! Throws 3-5 spears in formation ahead of your aim direction. Each level increases:\n• <b>Damage</b> (+3 per level, 15 → 42 at Lv10)\n• <b>Projectile Speed</b> (+2 per level)\n• <b>Cooldown Reduction</b> (-0.2s per level, 4s → 2.2s)\n• <b>Spear Count</b> (+1 every 3 levels, 3 → 5 max)\n\nSpears inherit critical hits and player damage bonuses. Great for clearing groups of enemies in a straight line!\n\n<i>Max Level: 10 - Pierce through enemies with deadly precision!</i>";

                
            case PlayerUpgradeData.UpgradeType.UpgradePiccoloFireCracker:
                return "<b><color=#FF6600>PICCOLO FIRECRACKER</color></b>\n\nUnlock or upgrade this explosive bombardment skill! Throws explosive bombs to random areas that explode after a delay, damaging all nearby enemies. Each level increases:\n• <b>Damage</b> (+15% per level, 20 → 52 at Lv10)\n• <b>Explosion Radius</b> (+10% per level, 5m → 10m max)\n• <b>Explosion Speed</b> (-0.22s per level, 3s → 1s min)\n• <b>Bomb Count</b> (+1 every 3 levels, 2 → 5 max)\n\nAutomatically throws volleys of bombs every 5 seconds. Bombs arc through the air and create spectacular fiery explosions!\n\n<i>Max Level: 10 - Rain destruction from above!</i>";
                
            case PlayerUpgradeData.UpgradeType.UpgradeBlinkDistance:
                return "<b><color=#6BCF7F>BLINK RANGE</color></b>\n\nIncrease the maximum distance you can teleport with Blink or Dash. Better mobility and positioning control.\n\n<i>Escape danger or chase enemies more effectively!</i>";
                

                

                

                
            default:
                return "<b>UPGRADE</b>\n\nSelect this upgrade to enhance your character.";
        }
    }
    

}
