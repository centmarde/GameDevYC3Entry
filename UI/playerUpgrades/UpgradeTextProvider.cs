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
                
            case PlayerUpgradeData.UpgradeType.Heal:
                return $"HEAL\n<size=24>Restore to Full Health</size>";
                
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
                
            case PlayerUpgradeData.UpgradeType.UpgradeDefense:
                int defenseLevel = upgradeManager.GetDefenseLevel();
                int defenseMaxLevel = upgradeManager.GetDefenseMaxLevel();
                
                if (defenseLevel == 0)
                {
                    // Not obtained yet - show as unlock
                    return $"<color=#5DADE2>DEFENSE</color>\n<size=24>Unlock Skill (Level 1/{defenseMaxLevel})</size>";
                }
                else if (defenseLevel < defenseMaxLevel)
                {
                    // Show level upgrade
                    float currentAbsorption = upgradeManager.GetDefenseAbsorptionPercent();
                    // Calculate next level absorption (linear progression)
                    float increment = (80f - 20f) / 9f; // (max - min) / (levels - 1)
                    float nextAbsorption = Mathf.Min(currentAbsorption + increment, 80f);
                    return $"<color=#5DADE2>DEFENSE</color>\n<size=24>Level {defenseLevel} → {defenseLevel + 1}\n{currentAbsorption:F1}% → {nextAbsorption:F1}% Absorb</size>";
                }
                else
                {
                    // Max level reached (shouldn't appear but handle it)
                    return $"<color=#5DADE2>DEFENSE</color>\n<size=24>MAX LEVEL ({defenseMaxLevel})</size>";
                }
                
            case PlayerUpgradeData.UpgradeType.UpgradeVampireAura:
                int vampireLevel = upgradeManager.GetVampireAuraLevel();
                int vampireMaxLevel = upgradeManager.GetVampireAuraMaxLevel();
                
                if (vampireLevel == 0)
                {
                    // Not obtained yet - show as unlock
                    return $"<color=#CC0033>VAMPIRE AURA</color>\n<size=24>Unlock Skill (Level 1/{vampireMaxLevel})</size>";
                }
                else if (vampireLevel < vampireMaxLevel)
                {
                    // Show level upgrade with lifesteal percentage progression
                    float currentLifesteal = upgradeManager.GetVampireAuraHealPercentage();
                    float nextLifesteal = currentLifesteal + 1f; // +1% per level
                    return $"<color=#CC0033>VAMPIRE AURA</color>\n<size=24>Level {vampireLevel} → {vampireLevel + 1}\n{currentLifesteal:F0}% → {nextLifesteal:F0}% Lifesteal</size>";
                }
                else
                {
                    // Max level reached (shouldn't appear but handle it)
                    return $"<color=#CC0033>VAMPIRE AURA</color>\n<size=24>MAX LEVEL ({vampireMaxLevel})</size>";
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
                
            case PlayerUpgradeData.UpgradeType.ReduceBlinkCooldown:
                float currentBlinkCD = upgradeManager.GetCurrentBlinkCooldown();
                float blinkCDReduction = upgradeManager.GetBlinkCooldownReduction();
                float newBlinkCD = Mathf.Max(currentBlinkCD - blinkCDReduction, 0.5f);
                int blinkCDLevel = upgradeManager.GetBlinkCooldownUpgradeLevel();
                int maxUpgradeLevelBC = upgradeManager.GetStatUpgradeMaxLevel();
                return $"BLINK COOLDOWN (Lv{blinkCDLevel}/{maxUpgradeLevelBC})\n<size=24>{currentBlinkCD:F1}s → {newBlinkCD:F1}s</size>";
                
            case PlayerUpgradeData.UpgradeType.ReduceDashCooldown:
                float currentDashCD = upgradeManager.GetCurrentDashCooldown();
                float dashCDReduction = upgradeManager.GetDashCooldownReduction();
                float newDashCD = Mathf.Max(currentDashCD - dashCDReduction, 0.3f);
                int dashCDLevel = upgradeManager.GetDashCooldownUpgradeLevel();
                int maxUpgradeLevelDC = upgradeManager.GetStatUpgradeMaxLevel();
                return $"DASH COOLDOWN (Lv{dashCDLevel}/{maxUpgradeLevelDC})\n<size=24>{currentDashCD:F1}s → {newDashCD:F1}s</size>";
                
            case PlayerUpgradeData.UpgradeType.UpgradeBlinkDashSpeed:
                float currentSpeed2 = upgradeManager.GetCurrentBlinkDashSpeed();
                float speedUpgrade2 = upgradeManager.GetBlinkDashSpeedUpgrade();
                int dashSpeedLevel = upgradeManager.GetBlinkDashSpeedUpgradeLevel();
                int maxUpgradeLevelDS = upgradeManager.GetStatUpgradeMaxLevel();
                return $"DASH SPEED (Lv{dashSpeedLevel}/{maxUpgradeLevelDS})\n<size=24>{currentSpeed2:F0} → {currentSpeed2 + speedUpgrade2:F0}</size>";
                
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
                
            case PlayerUpgradeData.UpgradeType.Heal:
                return "<b><color=#95E1D3>HEAL</color></b>\n\nRestore all health to maximum instantly. Choose this when you're low on health.\n\n<i>Perfect for emergency recovery!</i>";
                
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
                
            case PlayerUpgradeData.UpgradeType.UpgradeDefense:
                return "<b><color=#5DADE2>ONION ARMOR</color></b>\n\nUnlock or upgrade this powerful defensive skill! Automatically absorbs incoming damage with 100% chance and reflects 100% of absorbed damage back to enemies. Each level increases:\n• <b>Absorption</b> (20% at Lv1 → 80% at Lv10)\n• <b>Reflection</b> (100% of absorbed damage)\n• <b>Chance</b> (Always 100%)\n\nDamage absorption scales linearly across all 10 levels. Reflected damage targets the attacker first, then nearest enemy within 5m radius.\n\n<i>Max Level: 10 - Turn enemy attacks into your weapon!</i>";
                
            case PlayerUpgradeData.UpgradeType.UpgradeVampireAura:
                return "<b><color=#CC0033>ANITOS BLESSING'S</color></b>\n\nUnlock or upgrade this lifesteal skill! Passively restores health whenever you deal damage to enemies. Each level increases:\n• <b>Lifesteal</b> (5% at Lv1 → 14% at Lv10)\n• <b>+1% per level</b>\n\nHeals for 5-14% of all damage you deal. Works with projectiles, dash attacks, and skills. Rewards aggressive playstyle with constant healing!\n\n<i>Max Level: 10 - Sustain yourself through combat!</i>";
                
            case PlayerUpgradeData.UpgradeType.UpgradePiccoloFireCracker:
                return "<b><color=#FF6600>PICCOLO FIRECRACKER</color></b>\n\nUnlock or upgrade this explosive bombardment skill! Throws explosive bombs to random areas that explode after a delay, damaging all nearby enemies. Each level increases:\n• <b>Damage</b> (+15% per level, 20 → 52 at Lv10)\n• <b>Explosion Radius</b> (+10% per level, 5m → 10m max)\n• <b>Explosion Speed</b> (-0.22s per level, 3s → 1s min)\n• <b>Bomb Count</b> (+1 every 3 levels, 2 → 5 max)\n\nAutomatically throws volleys of bombs every 5 seconds. Bombs arc through the air and create spectacular fiery explosions!\n\n<i>Max Level: 10 - Rain destruction from above!</i>";
                
            case PlayerUpgradeData.UpgradeType.UpgradeBlinkDistance:
                return "<b><color=#6BCF7F>BLINK RANGE</color></b>\n\nIncrease the maximum distance you can teleport with Blink or Dash. Better mobility and positioning control.\n\n<i>Escape danger or chase enemies more effectively!</i>";
                
            case PlayerUpgradeData.UpgradeType.ReduceBlinkCooldown:
                return "<b><color=#4A90E2>BLINK COOLDOWN</color></b>\n\nReduce the time between Blink uses. Use your mobility ability more frequently for better repositioning.\n\n<i>Minimum cooldown: 0.5 seconds</i>";
                
            case PlayerUpgradeData.UpgradeType.ReduceDashCooldown:
                return "<b><color=#E74C3C>DASH COOLDOWN</color></b>\n\nReduce the time between Dash attacks. Attack more frequently with your high-damage dash strike.\n\n<i>Minimum cooldown: 0.3 seconds</i>";
                
            case PlayerUpgradeData.UpgradeType.UpgradeBlinkDashSpeed:
                return "<b><color=#9B59B6>DASH SPEED</color></b>\n\nIncrease the movement speed during Blink and Dash. Move faster across the battlefield.\n\n<i>Harder for enemies to track and hit you!</i>";
                
            default:
                return "<b>UPGRADE</b>\n\nSelect this upgrade to enhance your character.";
        }
    }
}
