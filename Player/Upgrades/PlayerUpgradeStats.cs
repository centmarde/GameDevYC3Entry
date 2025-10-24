using UnityEngine;
using static PlayerUpgradeData;

namespace PlayerUpgrades
{
    /// <summary>
    /// Provides read-only access to current player stats and upgrade amounts
    /// </summary>
    public class PlayerUpgradeStats
    {
        private PlayerUpgradeConfig config;
        private PlayerReferenceManager referenceManager;
        
        public PlayerUpgradeStats(PlayerUpgradeConfig config, PlayerReferenceManager referenceManager)
        {
            this.config = config;
            this.referenceManager = referenceManager;
        }
        
        // Upgrade amount getters
        public float GetDamageUpgradeAmount() => config.damageUpgradeAmount;
        public float GetMaxHealthUpgradeAmount() => config.maxHealthUpgradeAmount;
        public float GetHealAmount() => config.healAmount;
        public float GetCriticalChanceUpgradeAmount() => config.criticalChanceUpgradeAmount;
        public float GetCriticalDamageUpgradeAmount() => config.criticalDamageUpgradeAmount;
        public float GetEvasionChanceUpgradeAmount() => config.evasionChanceUpgradeAmount;
        public float GetBlinkDistanceUpgradeAmount() => config.blinkDistanceUpgradeAmount;
        public float GetBlinkCooldownReduction() => config.blinkCooldownReduction;
        public float GetDashCooldownReduction() => config.dashCooldownReduction;
        public float GetBlinkDashSpeedUpgrade() => config.blinkDashSpeedUpgrade;
        
        // Current stat getters
        public float GetCurrentDamage()
        {
            var refs = referenceManager.References;
            return referenceManager.IsPlayer2Active() 
                ? (refs.Player2Stats != null ? refs.Player2Stats.projectileDamage : 0f)
                : (refs.PlayerStats != null ? refs.PlayerStats.projectileDamage : 0f);
        }
        
        public float GetCurrentHealth()
        {
            var refs = referenceManager.References;
            return referenceManager.IsPlayer2Active() 
                ? (refs.Player2Stats != null ? refs.Player2Stats.maxHealth : 0f)
                : (refs.PlayerStats != null ? refs.PlayerStats.maxHealth : 0f);
        }
        
        public float GetCurrentCriticalChance()
        {
            var refs = referenceManager.References;
            return referenceManager.IsPlayer2Active() 
                ? (refs.Player2Stats != null ? refs.Player2Stats.criticalChance : 0f)
                : (refs.PlayerStats != null ? refs.PlayerStats.criticalChance : 0f);
        }
        
        public float GetCurrentCriticalDamage()
        {
            var refs = referenceManager.References;
            return referenceManager.IsPlayer2Active() 
                ? (refs.Player2Stats != null ? refs.Player2Stats.criticalDamageMultiplier : 0f)
                : (refs.PlayerStats != null ? refs.PlayerStats.criticalDamageMultiplier : 0f);
        }
        
        public float GetCurrentEvasion()
        {
            var refs = referenceManager.References;
            return referenceManager.IsPlayer2Active() 
                ? (refs.Player2Stats != null ? refs.Player2Stats.evasionChance : 0f)
                : (refs.PlayerStats != null ? refs.PlayerStats.evasionChance : 0f);
        }
        
        public float GetCurrentBlinkDistance()
        {
            var refs = referenceManager.References;
            return referenceManager.IsPlayer2Active() && refs.Player2Stats != null 
                ? refs.Player2Stats.blinkDistance : 0f;
        }
        
        public float GetCurrentBlinkCooldown()
        {
            var refs = referenceManager.References;
            return referenceManager.IsPlayer2Active() && refs.Player2Stats != null 
                ? refs.Player2Stats.blinkCooldown : 0f;
        }
        
        public float GetCurrentDashCooldown()
        {
            var refs = referenceManager.References;
            return referenceManager.IsPlayer2Active() && refs.Player2Stats != null 
                ? refs.Player2Stats.dashAttackCooldown : 0f;
        }
        
        public float GetCurrentBlinkDashSpeed()
        {
            var refs = referenceManager.References;
            return referenceManager.IsPlayer2Active() && refs.Player2Stats != null 
                ? refs.Player2Stats.blinkDashSpeed : 0f;
        }
        
        // Skill stat getters
        public int GetCirclingProjectilesLevel() => GetSkillLevel(referenceManager.References.CirclingProjectilesSkills);
        public int GetCirclingProjectilesMaxLevel() => GetSkillMaxLevel(referenceManager.References.CirclingProjectilesSkills, 10);
        public int GetPushWaveLevel() => GetSkillLevel(referenceManager.References.PushWaveSkills);
        public int GetPushWaveMaxLevel() => GetSkillMaxLevel(referenceManager.References.PushWaveSkills, 10);
        public int GetDefenseLevel() => GetSkillLevel(referenceManager.References.DefenseSkills);
        public int GetDefenseMaxLevel() => GetSkillMaxLevel(referenceManager.References.DefenseSkills, 10);
        public int GetVampireAuraLevel() => GetSkillLevel(referenceManager.References.VampireAuraSkills);
        public int GetVampireAuraMaxLevel() => GetSkillMaxLevel(referenceManager.References.VampireAuraSkills, 10);
        
        public int GetExtraHandLevel()
        {
            var skills = referenceManager.References.ExtraHandSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null)
                        return skill.ExtraHandLevel;
                }
            }
            return 0;
        }
        
        public int GetExtraHandMaxLevel()
        {
            var skills = referenceManager.References.ExtraHandSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null)
                        return skill.MaxLevel;
                }
            }
            return 10;
        }
        
        private int GetSkillLevel(PlayerSkill_CirclingProjectiles[] skills)
        {
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null)
                        return skill.CurrentLevel;
                }
            }
            return 0;
        }
        
        private int GetSkillLevel(PlayerSkill_PushWave[] skills)
        {
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null)
                        return skill.CurrentLevel;
                }
            }
            return 0;
        }
        
        private int GetSkillLevel(PlayerSkill_Defense[] skills)
        {
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null)
                        return skill.CurrentLevel;
                }
            }
            return 0;
        }
        
        private int GetSkillLevel(PlayerSkill_VampireAura[] skills)
        {
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null)
                        return skill.CurrentLevel;
                }
            }
            return 0;
        }
        
        private int GetSkillMaxLevel<T>(T[] skills, int defaultMax) where T : class
        {
            if (skills != null && skills.Length > 0)
            {
                return defaultMax;
            }
            return defaultMax;
        }
        
        // Detailed skill stats
        public int GetCurrentProjectileCount()
        {
            var skills = referenceManager.References.CirclingProjectilesSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentProjectileCount;
                }
            }
            return 0;
        }
        
        public float GetCurrentProjectileDamage()
        {
            var skills = referenceManager.References.CirclingProjectilesSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentDamage;
                }
            }
            return 0f;
        }
        
        public float GetCurrentProjectileRadius()
        {
            var skills = referenceManager.References.CirclingProjectilesSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentRadius;
                }
            }
            return 0f;
        }
        
        public float GetCurrentProjectileSpeed()
        {
            var skills = referenceManager.References.CirclingProjectilesSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentSpeed;
                }
            }
            return 0f;
        }
        
        public float GetPushWaveRadius()
        {
            var skills = referenceManager.References.PushWaveSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentRadius;
                }
            }
            return 0f;
        }
        
        public float GetPushWaveForce()
        {
            var skills = referenceManager.References.PushWaveSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentForce;
                }
            }
            return 0f;
        }
        
        public float GetPushWaveDamage()
        {
            var skills = referenceManager.References.PushWaveSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentDamage;
                }
            }
            return 0f;
        }
        
        public float GetPushWaveInterval()
        {
            var skills = referenceManager.References.PushWaveSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentInterval;
                }
            }
            return 0f;
        }
        
        public float GetExtraHandDamage()
        {
            var skills = referenceManager.References.ExtraHandSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentDamage;
                }
            }
            return 0f;
        }
        
        public float GetExtraHandShootInterval()
        {
            var skills = referenceManager.References.ExtraHandSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentShootInterval;
                }
            }
            return 0f;
        }
        
        public float GetExtraHandRange()
        {
            var skills = referenceManager.References.ExtraHandSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentRange;
                }
            }
            return 0f;
        }
        
        public float GetDefenseAbsorptionPercent()
        {
            var skills = referenceManager.References.DefenseSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.DamageAbsorptionPercent;
                }
            }
            return 0f;
        }
        
        public float GetDefenseAbsorptionChance()
        {
            var skills = referenceManager.References.DefenseSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.AbsorptionChance;
                }
            }
            return 0f;
        }
        
        public float GetVampireAuraHealPercentage()
        {
            var skills = referenceManager.References.VampireAuraSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentHealPercentage;
                }
            }
            return 0f;
        }
    }
}
