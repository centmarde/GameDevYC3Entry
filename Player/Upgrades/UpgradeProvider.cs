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
            
            // Shuffle the list to ensure randomness
            ShuffleList(allUpgrades);
            
            Debug.Log($"[UpgradeProvider] Total available upgrades: {allUpgrades.Count} - {string.Join(", ", allUpgrades)}");
            
            return PickRandomUpgrades(allUpgrades);
        }
        
        private void AddPlayer1Upgrades(List<UpgradeType> allUpgrades)
        {
            var refs = referenceManager.References;
            if (refs.PlayerStats == null)
            {
                Debug.LogWarning("[UpgradeProvider] PlayerStats is null. Player may not be spawned yet.");
                return;
            }
            
            // Damage and MaxHealth have no level cap
            allUpgrades.Add(UpgradeType.Damage);
            allUpgrades.Add(UpgradeType.MaxHealth);
            if (refs.PlayerStats.criticalChanceUpgradeLevel < Player_DataSO.MaxUpgradeLevel)
                allUpgrades.Add(UpgradeType.CriticalChance);
            if (refs.PlayerStats.criticalDamageUpgradeLevel < Player_DataSO.MaxUpgradeLevel)
                allUpgrades.Add(UpgradeType.CriticalDamage);
            if (refs.PlayerStats.evasionUpgradeLevel < Player_DataSO.MaxUpgradeLevel)
                allUpgrades.Add(UpgradeType.Evasion);
        }
        
        private void AddPlayer2Upgrades(List<UpgradeType> allUpgrades)
        {
            var refs = referenceManager.References;
            if (refs.Player2Stats == null)
            {
                Debug.LogWarning("[UpgradeProvider] Player2Stats is null. Player2 may not be spawned yet.");
                return;
            }
            
            // Damage and MaxHealth have no level cap
            allUpgrades.Add(UpgradeType.Damage);
            allUpgrades.Add(UpgradeType.MaxHealth);
            if (refs.Player2Stats.criticalChanceUpgradeLevel < Player2_DataSO.MaxUpgradeLevel)
                allUpgrades.Add(UpgradeType.CriticalChance);
            if (refs.Player2Stats.criticalDamageUpgradeLevel < Player2_DataSO.MaxUpgradeLevel)
                allUpgrades.Add(UpgradeType.CriticalDamage);
            if (refs.Player2Stats.evasionUpgradeLevel < Player2_DataSO.MaxUpgradeLevel)
                allUpgrades.Add(UpgradeType.Evasion);
            if (refs.Player2Stats.blinkDistanceUpgradeLevel < Player2_DataSO.MaxUpgradeLevel)
                allUpgrades.Add(UpgradeType.UpgradeBlinkDistance);
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
            
            // Defense is now handled in AddPlayer1Upgrades/AddPlayer2Upgrades based on defenseUpgradeLevel
            // No longer needed here since it's a stat-based upgrade, not a skill
            
            // Check Spear Throw
            if (refs.SpearThrowSkills != null)
            {
                foreach (var skill in refs.SpearThrowSkills)
                {
                    if (skill != null && skill.CurrentLevel < 10)
                    {
                        allUpgrades.Add(UpgradeType.UpgradeSpearThrow);
                        break;
                    }
                }
            }
            
            // Check Piccolo FireCracker
            if (refs.PiccoloFireCrackerSkills != null)
            {
                foreach (var skill in refs.PiccoloFireCrackerSkills)
                {
                    if (skill != null && skill.CurrentLevel < 10)
                    {
                        allUpgrades.Add(UpgradeType.UpgradePiccoloFireCracker);
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Fisher-Yates shuffle algorithm for true randomization
        /// </summary>
        private void ShuffleList(List<UpgradeType> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                UpgradeType temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
        
        private UpgradeType[] PickRandomUpgrades(List<UpgradeType> allUpgrades)
        {
            UpgradeType[] selectedUpgrades = new UpgradeType[3];
            
            if (allUpgrades.Count == 0)
            {
                Debug.LogError("[UpgradeProvider] No upgrades available! This should not happen.");
                // Emergency fallback - should never reach here
                selectedUpgrades[0] = UpgradeType.Damage;
                selectedUpgrades[1] = UpgradeType.MaxHealth;
                selectedUpgrades[2] = UpgradeType.CriticalChance;
                return selectedUpgrades;
            }
            
            // Pick first 3 from already shuffled list
            // If less than 3 available, allow duplicates by cycling through the list
            for (int i = 0; i < 3; i++)
            {
                selectedUpgrades[i] = allUpgrades[i % allUpgrades.Count];
            }
            
            return selectedUpgrades;
        }
    }
}
