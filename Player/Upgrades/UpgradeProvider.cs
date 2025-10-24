using UnityEngine;
using System.Collections.Generic;
using static PlayerUpgradeData;

namespace PlayerUpgrades
{
    /// <summary>
    /// Generates random upgrade options for the player
    /// </summary>
    public class UpgradeProvider
    {
        private PlayerReferenceManager referenceManager;
        
        public UpgradeProvider(PlayerReferenceManager referenceManager)
        {
            this.referenceManager = referenceManager;
        }
        
        /// <summary>
        /// Generate 3 random unique upgrade options
        /// </summary>
        public UpgradeType[] GenerateRandomUpgrades()
        {
            var allUpgrades = new List<UpgradeType>();
            bool hasPlayer2 = referenceManager.IsPlayer2Active();
            
            if (hasPlayer2)
            {
                AddPlayer2Upgrades(allUpgrades);
            }
            else
            {
                AddPlayer1Upgrades(allUpgrades);
            }
            
            // Add skill upgrades if available
            AddSkillUpgrades(allUpgrades);
            
            return PickRandomUpgrades(allUpgrades);
        }
        
        private void AddPlayer1Upgrades(List<UpgradeType> allUpgrades)
        {
            allUpgrades.Add(UpgradeType.Damage);
            allUpgrades.Add(UpgradeType.MaxHealth);
            allUpgrades.Add(UpgradeType.Heal);
            allUpgrades.Add(UpgradeType.CriticalChance);
            allUpgrades.Add(UpgradeType.CriticalDamage);
            allUpgrades.Add(UpgradeType.Evasion);
        }
        
        private void AddPlayer2Upgrades(List<UpgradeType> allUpgrades)
        {
            allUpgrades.Add(UpgradeType.Damage);
            allUpgrades.Add(UpgradeType.MaxHealth);
            allUpgrades.Add(UpgradeType.Heal);
            allUpgrades.Add(UpgradeType.CriticalChance);
            allUpgrades.Add(UpgradeType.CriticalDamage);
            allUpgrades.Add(UpgradeType.Evasion);
            allUpgrades.Add(UpgradeType.UpgradeBlinkDistance);
            allUpgrades.Add(UpgradeType.ReduceBlinkCooldown);
            allUpgrades.Add(UpgradeType.ReduceDashCooldown);
            allUpgrades.Add(UpgradeType.UpgradeBlinkDashSpeed);
        }
        
        private void AddSkillUpgrades(List<UpgradeType> allUpgrades)
        {
            var refs = referenceManager.References;
            
            // Check Circling Projectiles
            if (refs.CirclingProjectilesSkills != null)
            {
                foreach (var skill in refs.CirclingProjectilesSkills)
                {
                    if (skill != null && skill.CurrentLevel < 10)
                    {
                        allUpgrades.Add(UpgradeType.UpgradeCirclingProjectiles);
                        break;
                    }
                }
            }
            
            // Check Push Wave
            if (refs.PushWaveSkills != null)
            {
                foreach (var skill in refs.PushWaveSkills)
                {
                    if (skill != null && skill.CurrentLevel < 10)
                    {
                        allUpgrades.Add(UpgradeType.UpgradePushWave);
                        break;
                    }
                }
            }
            
            // Check Extra Hand
            if (refs.ExtraHandSkills != null)
            {
                foreach (var skill in refs.ExtraHandSkills)
                {
                    if (skill != null && skill.ExtraHandLevel < 10)
                    {
                        allUpgrades.Add(UpgradeType.UpgradeExtraHand);
                        break;
                    }
                }
            }
            
            // Check Defense
            if (refs.DefenseSkills != null)
            {
                foreach (var skill in refs.DefenseSkills)
                {
                    if (skill != null && skill.CurrentLevel < 10)
                    {
                        allUpgrades.Add(UpgradeType.UpgradeDefense);
                        break;
                    }
                }
            }
        }
        
        private UpgradeType[] PickRandomUpgrades(List<UpgradeType> allUpgrades)
        {
            UpgradeType[] selectedUpgrades = new UpgradeType[3];
            
            for (int i = 0; i < 3 && allUpgrades.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, allUpgrades.Count);
                selectedUpgrades[i] = allUpgrades[randomIndex];
                allUpgrades.RemoveAt(randomIndex);
            }
            
            return selectedUpgrades;
        }
    }
}
