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
                case UpgradeType.UpgradeVampireAura:
                    ApplyVampireAuraUpgrade(refs);
                    break;
                case UpgradeType.UpgradePiccoloFireCracker:
                    ApplyPiccoloFireCrackerUpgrade(refs);
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
                refs.Player2Stats.damageUpgradeLevel++;
                Debug.Log($"[UpgradeApplicator] Player2 Damage upgraded to {refs.Player2Stats.projectileDamage} (Level {refs.Player2Stats.damageUpgradeLevel})");
            }
            else
            {
                refs.PlayerStats.projectileDamage += config.damageUpgradeAmount;
                refs.PlayerStats.damageUpgradeLevel++;
                Debug.Log($"[UpgradeApplicator] Player Damage upgraded to {refs.PlayerStats.projectileDamage} (Level {refs.PlayerStats.damageUpgradeLevel})");
            }
        }
        
        private void ApplyMaxHealthUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2)
            {
                refs.Player2Stats.maxHealth += config.maxHealthUpgradeAmount;
                refs.Player2Stats.maxHealthUpgradeLevel++;
                Debug.Log($"[UpgradeApplicator] Player2 MaxHealth upgraded to {refs.Player2Stats.maxHealth} (Level {refs.Player2Stats.maxHealthUpgradeLevel})");
            }
            else
            {
                refs.PlayerStats.maxHealth += config.maxHealthUpgradeAmount;
                refs.PlayerStats.maxHealthUpgradeLevel++;
                Debug.Log($"[UpgradeApplicator] Player MaxHealth upgraded to {refs.PlayerStats.maxHealth} (Level {refs.PlayerStats.maxHealthUpgradeLevel})");
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
                if (refs.Player2Stats.criticalChanceUpgradeLevel < Player2_DataSO.MaxUpgradeLevel)
                {
                    refs.Player2Stats.criticalChance += config.criticalChanceUpgradeAmount;
                    refs.Player2Stats.criticalChance = Mathf.Min(refs.Player2Stats.criticalChance, 100f);
                    refs.Player2Stats.criticalChanceUpgradeLevel++;
                    Debug.Log($"[UpgradeApplicator] Player2 Critical Chance upgraded to {refs.Player2Stats.criticalChance}% (Level {refs.Player2Stats.criticalChanceUpgradeLevel}/{Player2_DataSO.MaxUpgradeLevel})");
                }
            }
            else
            {
                if (refs.PlayerStats.criticalChanceUpgradeLevel < Player_DataSO.MaxUpgradeLevel)
                {
                    refs.PlayerStats.criticalChance += config.criticalChanceUpgradeAmount;
                    refs.PlayerStats.criticalChance = Mathf.Min(refs.PlayerStats.criticalChance, 100f);
                    refs.PlayerStats.criticalChanceUpgradeLevel++;
                    Debug.Log($"[UpgradeApplicator] Player Critical Chance upgraded to {refs.PlayerStats.criticalChance}% (Level {refs.PlayerStats.criticalChanceUpgradeLevel}/{Player_DataSO.MaxUpgradeLevel})");
                }
            }
        }
        
        private void ApplyCriticalDamageUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2)
            {
                if (refs.Player2Stats.criticalDamageUpgradeLevel < Player2_DataSO.MaxUpgradeLevel)
                {
                    refs.Player2Stats.criticalDamageMultiplier += config.criticalDamageUpgradeAmount;
                    refs.Player2Stats.criticalDamageUpgradeLevel++;
                    Debug.Log($"[UpgradeApplicator] Player2 Critical Damage upgraded to {refs.Player2Stats.criticalDamageMultiplier}x (Level {refs.Player2Stats.criticalDamageUpgradeLevel}/{Player2_DataSO.MaxUpgradeLevel})");
                }
            }
            else
            {
                if (refs.PlayerStats.criticalDamageUpgradeLevel < Player_DataSO.MaxUpgradeLevel)
                {
                    refs.PlayerStats.criticalDamageMultiplier += config.criticalDamageUpgradeAmount;
                    refs.PlayerStats.criticalDamageUpgradeLevel++;
                    Debug.Log($"[UpgradeApplicator] Player Critical Damage upgraded to {refs.PlayerStats.criticalDamageMultiplier}x (Level {refs.PlayerStats.criticalDamageUpgradeLevel}/{Player_DataSO.MaxUpgradeLevel})");
                }
            }
        }
        
        private void ApplyEvasionUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2)
            {
                if (refs.Player2Stats.evasionUpgradeLevel < Player2_DataSO.MaxUpgradeLevel)
                {
                    refs.Player2Stats.evasionChance += config.evasionChanceUpgradeAmount;
                    refs.Player2Stats.evasionChance = Mathf.Min(refs.Player2Stats.evasionChance, 100f);
                    refs.Player2Stats.evasionUpgradeLevel++;
                    Debug.Log($"[UpgradeApplicator] Player2 Evasion upgraded to {refs.Player2Stats.evasionChance}% (Level {refs.Player2Stats.evasionUpgradeLevel}/{Player2_DataSO.MaxUpgradeLevel})");
                }
            }
            else
            {
                if (refs.PlayerStats.evasionUpgradeLevel < Player_DataSO.MaxUpgradeLevel)
                {
                    refs.PlayerStats.evasionChance += config.evasionChanceUpgradeAmount;
                    refs.PlayerStats.evasionChance = Mathf.Min(refs.PlayerStats.evasionChance, 100f);
                    refs.PlayerStats.evasionUpgradeLevel++;
                    Debug.Log($"[UpgradeApplicator] Player Evasion upgraded to {refs.PlayerStats.evasionChance}% (Level {refs.PlayerStats.evasionUpgradeLevel}/{Player_DataSO.MaxUpgradeLevel})");
                }
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
        
        private void ApplyVampireAuraUpgrade(PlayerReferences refs)
        {
            if (refs.VampireAuraSkills != null)
            {
                foreach (var skill in refs.VampireAuraSkills)
                {
                    if (skill != null)
                    {
                        if (!skill.IsObtained)
                        {
                            skill.ObtainSkill();
                            Debug.Log($"[UpgradeApplicator] Obtained Vampire Aura skill! Level: {skill.CurrentLevel}, Lifesteal: {skill.CurrentHealPercentage}%");
                        }
                        else if (skill.CurrentLevel < skill.MaxLevel)
                        {
                            skill.UpgradeSkill();
                            Debug.Log($"[UpgradeApplicator] Upgraded Vampire Aura to Level {skill.CurrentLevel}, Lifesteal: {skill.CurrentHealPercentage}%");
                        }
                    }
                }
            }
        }
        
        private void ApplyPiccoloFireCrackerUpgrade(PlayerReferences refs)
        {
            if (refs.PiccoloFireCrackerSkills != null)
            {
                foreach (var skill in refs.PiccoloFireCrackerSkills)
                {
                    if (skill != null)
                    {
                        if (!skill.IsObtained)
                        {
                            skill.ObtainSkill();
                            Debug.Log($"[UpgradeApplicator] Obtained Piccolo FireCracker skill! Level: {skill.CurrentLevel}, Damage: {skill.CurrentDamage:F1}, Bombs: {skill.CurrentBombCount}");
                        }
                        else if (skill.CurrentLevel < skill.MaxLevel)
                        {
                            skill.UpgradeSkill();
                            Debug.Log($"[UpgradeApplicator] Upgraded Piccolo FireCracker to Level {skill.CurrentLevel}, Damage: {skill.CurrentDamage:F1}, Bombs: {skill.CurrentBombCount}");
                        }
                    }
                }
            }
        }
        
        private void ApplyBlinkDistanceUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2 && refs.Player2Stats != null)
            {
                if (refs.Player2Stats.blinkDistanceUpgradeLevel < Player2_DataSO.MaxUpgradeLevel)
                {
                    refs.Player2Stats.blinkDistance += config.blinkDistanceUpgradeAmount;
                    refs.Player2Stats.blinkDistanceUpgradeLevel++;
                    Debug.Log($"[UpgradeApplicator] Blink/Dash distance upgraded to {refs.Player2Stats.blinkDistance} (Level {refs.Player2Stats.blinkDistanceUpgradeLevel}/{Player2_DataSO.MaxUpgradeLevel})");
                }
            }
        }
        
        private void ApplyBlinkCooldownUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2 && refs.Player2Stats != null)
            {
                if (refs.Player2Stats.blinkCooldownUpgradeLevel < Player2_DataSO.MaxUpgradeLevel)
                {
                    refs.Player2Stats.blinkCooldown -= config.blinkCooldownReduction;
                    refs.Player2Stats.blinkCooldown = Mathf.Max(refs.Player2Stats.blinkCooldown, 0.5f);
                    refs.Player2Stats.blinkCooldownUpgradeLevel++;
                    Debug.Log($"[UpgradeApplicator] Blink cooldown reduced to {refs.Player2Stats.blinkCooldown}s (Level {refs.Player2Stats.blinkCooldownUpgradeLevel}/{Player2_DataSO.MaxUpgradeLevel})");
                }
            }
        }
        
        private void ApplyDashCooldownUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2 && refs.Player2Stats != null)
            {
                if (refs.Player2Stats.dashCooldownUpgradeLevel < Player2_DataSO.MaxUpgradeLevel)
                {
                    refs.Player2Stats.dashAttackCooldown -= config.dashCooldownReduction;
                    refs.Player2Stats.dashAttackCooldown = Mathf.Max(refs.Player2Stats.dashAttackCooldown, 0.3f);
                    refs.Player2Stats.dashCooldownUpgradeLevel++;
                    Debug.Log($"[UpgradeApplicator] Dash cooldown reduced to {refs.Player2Stats.dashAttackCooldown}s (Level {refs.Player2Stats.dashCooldownUpgradeLevel}/{Player2_DataSO.MaxUpgradeLevel})");
                }
            }
        }
        
        private void ApplyBlinkDashSpeedUpgrade(bool hasPlayer2, PlayerReferences refs)
        {
            if (hasPlayer2 && refs.Player2Stats != null)
            {
                if (refs.Player2Stats.blinkDashSpeedUpgradeLevel < Player2_DataSO.MaxUpgradeLevel)
                {
                    refs.Player2Stats.blinkDashSpeed += config.blinkDashSpeedUpgrade;
                    refs.Player2Stats.blinkDashSpeedUpgradeLevel++;
                    Debug.Log($"[UpgradeApplicator] Blink/Dash speed upgraded to {refs.Player2Stats.blinkDashSpeed} (Level {refs.Player2Stats.blinkDashSpeedUpgradeLevel}/{Player2_DataSO.MaxUpgradeLevel})");
                }
            }
        }
    }
}
