using UnityEngine;
using static PlayerUpgradeData;

namespace PlayerUpgrades
{
    /// <summary>
    /// Applies specific upgrades to player stats and skills
    /// </summary>
    public class UpgradeApplicator
    {
        private PlayerUpgradeConfig config;
        private PlayerReferenceManager referenceManager;
        
        public UpgradeApplicator(PlayerUpgradeConfig config, PlayerReferenceManager referenceManager)
        {
            this.config = config;
            this.referenceManager = referenceManager;
        }
        
        /// <summary>
        /// Apply the selected upgrade
        /// </summary>
        public void ApplyUpgrade(UpgradeType upgradeType)
        {
            bool hasPlayer2 = referenceManager.IsPlayer2Active();
            var refs = referenceManager.References;
            
            Debug.Log($"[UpgradeApplicator] Applying upgrade: {upgradeType}, hasPlayer2: {hasPlayer2}");
            
            if (hasPlayer2 && refs.Player2Stats == null)
            {
                Debug.LogWarning("[UpgradeApplicator] Player2 is active but player2Stats is null!");
                return;
            }
            else if (!hasPlayer2 && refs.PlayerStats == null)
            {
                Debug.LogWarning("[UpgradeApplicator] Player1 is active but playerStats is null!");
                return;
            }
            
            switch (upgradeType)
            {
                case UpgradeType.Damage:
                    ApplyDamageUpgrade(hasPlayer2, refs);
                    break;
                case UpgradeType.MaxHealth:
                    ApplyMaxHealthUpgrade(hasPlayer2, refs);
                    break;
                case UpgradeType.Heal:
                    ApplyHealUpgrade(hasPlayer2, refs);
                    break;
                case UpgradeType.CriticalChance:
                    ApplyCriticalChanceUpgrade(hasPlayer2, refs);
                    break;
                case UpgradeType.CriticalDamage:
                    ApplyCriticalDamageUpgrade(hasPlayer2, refs);
                    break;
                case UpgradeType.Evasion:
                    ApplyEvasionUpgrade(hasPlayer2, refs);
                    break;
                case UpgradeType.UpgradeCirclingProjectiles:
                    ApplyCirclingProjectilesUpgrade(refs);
                    break;
                case UpgradeType.UpgradePushWave:
                    ApplyPushWaveUpgrade(refs);
                    break;
                case UpgradeType.UpgradeExtraHand:
                    ApplyExtraHandUpgrade(refs);
                    break;
                case UpgradeType.UpgradeDefense:
                    ApplyDefenseUpgrade(refs);
                    break;
                case UpgradeType.UpgradeBlinkDistance:
                    ApplyBlinkDistanceUpgrade(hasPlayer2, refs);
                    break;
                case UpgradeType.ReduceBlinkCooldown:
                    ApplyBlinkCooldownUpgrade(hasPlayer2, refs);
                    break;
                case UpgradeType.ReduceDashCooldown:
                    ApplyDashCooldownUpgrade(hasPlayer2, refs);
                    break;
                case UpgradeType.UpgradeBlinkDashSpeed:
                    ApplyBlinkDashSpeedUpgrade(hasPlayer2, refs);
                    break;
            }
        }
        
        private void ApplyDamageUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2)
            {
                refs.Player2Stats.projectileDamage += config.damageUpgradeAmount;
                Debug.Log($"[UpgradeApplicator] Player2 Damage upgraded to {refs.Player2Stats.projectileDamage}");
            }
            else
            {
                refs.PlayerStats.projectileDamage += config.damageUpgradeAmount;
            }
        }
        
        private void ApplyMaxHealthUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2)
            {
                refs.Player2Stats.maxHealth += config.maxHealthUpgradeAmount;
            }
            else
            {
                refs.PlayerStats.maxHealth += config.maxHealthUpgradeAmount;
            }
            
            Player[] allPlayers = hasPlayer2 ? refs.Player2s : refs.Players;
            if (allPlayers != null)
            {
                foreach (Player p in allPlayers)
                {
                    if (p != null)
                    {
                        var health = p.GetComponent<Entity_Health>();
                        if (health != null)
                        {
                            health.IncreaseMaxHealth(config.maxHealthUpgradeAmount, false);
                        }
                    }
                }
            }
        }
        
        private void ApplyHealUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            Debug.Log($"[UpgradeApplicator] Heal upgrade selected. hasPlayer2: {hasPlayer2}");
            
            if (hasPlayer2)
            {
                HealPlayers(refs.Player2s, "Player2");
            }
            else
            {
                HealPlayers(refs.Players, "Player");
            }
        }
        
        private void HealPlayers(Player[] players, string playerType)
        {
            Debug.Log($"[UpgradeApplicator] Healing {playerType} instances. Count: {(players != null ? players.Length : 0)}");
            
            if (players != null && players.Length > 0)
            {
                foreach (Player p in players)
                {
                    if (p != null && p.gameObject.activeInHierarchy)
                    {
                        var health = p.GetComponent<Entity_Health>();
                        if (health != null)
                        {
                            float currentHP = health.CurrentHealth;
                            float maxHP = health.MaxHealth;
                            health.Heal(maxHP);
                            Debug.Log($"[UpgradeApplicator] Healed {p.name}: {currentHP} -> {health.CurrentHealth} (Max: {maxHP})");
                        }
                        else
                        {
                            Debug.LogWarning($"[UpgradeApplicator] {playerType} {p.name} has no Entity_Health component!");
                        }
                    }
                    else if (p != null)
                    {
                        Debug.LogWarning($"[UpgradeApplicator] {playerType} {p.name} is not active in hierarchy!");
                    }
                }
            }
            else
            {
                Debug.LogError($"[UpgradeApplicator] No {playerType} instances found to heal!");
            }
        }
        
        private void ApplyCriticalChanceUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2)
            {
                refs.Player2Stats.criticalChance += config.criticalChanceUpgradeAmount;
                refs.Player2Stats.criticalChance = Mathf.Min(refs.Player2Stats.criticalChance, 100f);
            }
            else
            {
                refs.PlayerStats.criticalChance += config.criticalChanceUpgradeAmount;
                refs.PlayerStats.criticalChance = Mathf.Min(refs.PlayerStats.criticalChance, 100f);
            }
        }
        
        private void ApplyCriticalDamageUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2)
            {
                refs.Player2Stats.criticalDamageMultiplier += config.criticalDamageUpgradeAmount;
            }
            else
            {
                refs.PlayerStats.criticalDamageMultiplier += config.criticalDamageUpgradeAmount;
            }
        }
        
        private void ApplyEvasionUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2)
            {
                refs.Player2Stats.evasionChance += config.evasionChanceUpgradeAmount;
                refs.Player2Stats.evasionChance = Mathf.Min(refs.Player2Stats.evasionChance, 100f);
            }
            else
            {
                refs.PlayerStats.evasionChance += config.evasionChanceUpgradeAmount;
                refs.PlayerStats.evasionChance = Mathf.Min(refs.PlayerStats.evasionChance, 100f);
            }
        }
        
        private void ApplyCirclingProjectilesUpgrade(PlayerReferences refs)
        {
            if (refs.CirclingProjectilesSkills != null)
            {
                foreach (var skill in refs.CirclingProjectilesSkills)
                {
                    if (skill != null)
                    {
                        if (!skill.IsObtained)
                        {
                            skill.ObtainSkill();
                            Debug.Log($"[UpgradeApplicator] Obtained Circling Projectiles skill! Level: {skill.CurrentLevel}");
                        }
                        else if (skill.CurrentLevel < 10)
                        {
                            skill.UpgradeLevel();
                            Debug.Log($"[UpgradeApplicator] Upgraded Circling Projectiles to Level {skill.CurrentLevel}");
                        }
                    }
                }
            }
        }
        
        private void ApplyPushWaveUpgrade(PlayerReferences refs)
        {
            if (refs.PushWaveSkills != null)
            {
                foreach (var skill in refs.PushWaveSkills)
                {
                    if (skill != null)
                    {
                        if (!skill.IsObtained)
                        {
                            skill.ObtainSkill();
                            Debug.Log($"[UpgradeApplicator] Obtained Push Wave skill! Level: {skill.CurrentLevel}");
                        }
                        else if (skill.CurrentLevel < 10)
                        {
                            skill.UpgradeLevel();
                            Debug.Log($"[UpgradeApplicator] Upgraded Push Wave to Level {skill.CurrentLevel}");
                        }
                    }
                }
            }
        }
        
        private void ApplyExtraHandUpgrade(PlayerReferences refs)
        {
            if (refs.ExtraHandSkills != null)
            {
                foreach (var skill in refs.ExtraHandSkills)
                {
                    if (skill != null)
                    {
                        if (!skill.IsObtained)
                        {
                            skill.ObtainSkill();
                            Debug.Log($"[UpgradeApplicator] Obtained Extra Hand skill! Level: {skill.ExtraHandLevel}");
                        }
                        else if (skill.ExtraHandLevel < 10)
                        {
                            skill.UpgradeLevel(config.extraHandDamagePerLevel, 
                                             config.extraHandIntervalReductionPerLevel, 
                                             config.extraHandRangePerLevel);
                            Debug.Log($"[UpgradeApplicator] Upgraded Extra Hand to Level {skill.ExtraHandLevel}");
                        }
                    }
                }
            }
        }
        
        private void ApplyDefenseUpgrade(PlayerReferences refs)
        {
            if (refs.DefenseSkills != null)
            {
                foreach (var skill in refs.DefenseSkills)
                {
                    if (skill != null)
                    {
                        if (!skill.IsObtained)
                        {
                            skill.ObtainSkill();
                            Debug.Log($"[UpgradeApplicator] Obtained Defense skill! Level: {skill.CurrentLevel}, Absorption: {skill.DamageAbsorptionPercent:F1}%");
                        }
                        else if (skill.CurrentLevel < 10)
                        {
                            skill.UpgradeSkill();
                            Debug.Log($"[UpgradeApplicator] Upgraded Defense to Level {skill.CurrentLevel}, Absorption: {skill.DamageAbsorptionPercent:F1}%");
                        }
                    }
                }
            }
        }
        
        private void ApplyBlinkDistanceUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2 && refs.Player2Stats != null)
            {
                refs.Player2Stats.blinkDistance += config.blinkDistanceUpgradeAmount;
                Debug.Log($"[UpgradeApplicator] Blink/Dash distance upgraded to {refs.Player2Stats.blinkDistance}");
            }
        }
        
        private void ApplyBlinkCooldownUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2 && refs.Player2Stats != null)
            {
                refs.Player2Stats.blinkCooldown -= config.blinkCooldownReduction;
                refs.Player2Stats.blinkCooldown = Mathf.Max(refs.Player2Stats.blinkCooldown, 0.5f);
                Debug.Log($"[UpgradeApplicator] Blink cooldown reduced to {refs.Player2Stats.blinkCooldown}s");
            }
        }
        
        private void ApplyDashCooldownUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2 && refs.Player2Stats != null)
            {
                refs.Player2Stats.dashAttackCooldown -= config.dashCooldownReduction;
                refs.Player2Stats.dashAttackCooldown = Mathf.Max(refs.Player2Stats.dashAttackCooldown, 0.3f);
                Debug.Log($"[UpgradeApplicator] Dash cooldown reduced to {refs.Player2Stats.dashAttackCooldown}s");
            }
        }
        
        private void ApplyBlinkDashSpeedUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2 && refs.Player2Stats != null)
            {
                refs.Player2Stats.blinkDashSpeed += config.blinkDashSpeedUpgrade;
                Debug.Log($"[UpgradeApplicator] Blink/Dash speed upgraded to {refs.Player2Stats.blinkDashSpeed}");
            }
        }
    }
}
