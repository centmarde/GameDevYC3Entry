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
        

        
        // Stat upgrade level getters
        public int GetDamageUpgradeLevel()
        {
            if (referenceManager.References.Player2Stats != null)
                return referenceManager.References.Player2Stats.damageUpgradeLevel;
            if (referenceManager.References.PlayerStats != null)
                return referenceManager.References.PlayerStats.damageUpgradeLevel;
            return 0;
        }
        
        public int GetMaxHealthUpgradeLevel()
        {
            if (referenceManager.References.Player2Stats != null)
                return referenceManager.References.Player2Stats.maxHealthUpgradeLevel;
            if (referenceManager.References.PlayerStats != null)
                return referenceManager.References.PlayerStats.maxHealthUpgradeLevel;
            return 0;
        }
        
        public int GetCriticalChanceUpgradeLevel()
        {
            if (referenceManager.References.Player2Stats != null)
                return referenceManager.References.Player2Stats.criticalChanceUpgradeLevel;
            if (referenceManager.References.PlayerStats != null)
                return referenceManager.References.PlayerStats.criticalChanceUpgradeLevel;
            return 0;
        }
        
        public int GetCriticalDamageUpgradeLevel()
        {
            if (referenceManager.References.Player2Stats != null)
                return referenceManager.References.Player2Stats.criticalDamageUpgradeLevel;
            if (referenceManager.References.PlayerStats != null)
                return referenceManager.References.PlayerStats.criticalDamageUpgradeLevel;
            return 0;
        }
        
        public int GetEvasionUpgradeLevel()
        {
            if (referenceManager.References.Player2Stats != null)
                return referenceManager.References.Player2Stats.evasionUpgradeLevel;
            if (referenceManager.References.PlayerStats != null)
                return referenceManager.References.PlayerStats.evasionUpgradeLevel;
            return 0;
        }
        
        public int GetBlinkDistanceUpgradeLevel()
        {
            if (referenceManager.References.Player2Stats != null)
                return referenceManager.References.Player2Stats.blinkDistanceUpgradeLevel;
            return 0;
        }
        

        
        public int GetStatUpgradeMaxLevel() => 10;
        
        // Skill stat getters
        public int GetCirclingProjectilesLevel() => GetSkillLevel(referenceManager.References.CirclingProjectilesSkills);
        public int GetCirclingProjectilesMaxLevel() => GetSkillMaxLevel(referenceManager.References.CirclingProjectilesSkills, 10);
        public int GetPushWaveLevel() => GetSkillLevel(referenceManager.References.PushWaveSkills);
        public int GetPushWaveMaxLevel() => GetSkillMaxLevel(referenceManager.References.PushWaveSkills, 10);


        
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
            // Defense now works as raw damage absorption, not percentage
            // Return the actual defense value for display purposes
            var refs = referenceManager.References;
            if (refs.PlayerStats != null)
                return refs.PlayerStats.defense;
            else if (refs.Player2Stats != null)
                return refs.Player2Stats.defense;
            return 0f;
        }
        
        public float GetDefenseAbsorptionChance()
        {
            // Defense always has 100% chance to absorb (up to defense value)
            var refs = referenceManager.References;
            if (refs.PlayerStats != null && refs.PlayerStats.defense > 0)
                return 100f;
            else if (refs.Player2Stats != null && refs.Player2Stats.defense > 0)
                return 100f;
            return 0f;
        }
        
        /// <summary>
        /// Get the raw defense value for damage absorption calculation
        /// </summary>
        public float GetDefenseValue()
        {
            var refs = referenceManager.References;
            if (refs.PlayerStats != null)
                return refs.PlayerStats.defense;
            else if (refs.Player2Stats != null)
                return refs.Player2Stats.defense;
            return 0f;
        }
        

        
        // Piccolo FireCracker getters
        public int GetPiccoloFireCrackerLevel()
        {
            var skills = referenceManager.References.PiccoloFireCrackerSkills;
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
        
        public int GetPiccoloFireCrackerMaxLevel()
        {
            var skills = referenceManager.References.PiccoloFireCrackerSkills;
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
        
        public float GetPiccoloFireCrackerDamage()
        {
            var skills = referenceManager.References.PiccoloFireCrackerSkills;
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
        
        public float GetPiccoloFireCrackerRadius()
        {
            var skills = referenceManager.References.PiccoloFireCrackerSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentAreaRadius;
                }
            }
            return 0f;
        }
        
        public float GetPiccoloFireCrackerExplosionTime()
        {
            var skills = referenceManager.References.PiccoloFireCrackerSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentExplosionTime;
                }
            }
            return 0f;
        }
        
        public int GetPiccoloFireCrackerBombCount()
        {
            var skills = referenceManager.References.PiccoloFireCrackerSkills;
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    if (skill != null && skill.IsObtained)
                        return skill.CurrentBombCount;
                }
            }
            return 0;
        }
    }
}
