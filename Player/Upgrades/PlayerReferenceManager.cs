using UnityEngine;
using System.Collections.Generic;
using static PlayerUpgradeData;

namespace PlayerUpgrades
{
    /// <summary>
    /// Manages references to all player-related components
    /// </summary>
    public class PlayerReferenceManager
    {
        private PlayerReferences references = new PlayerReferences();
        
        public PlayerReferences References => references;
        
        /// <summary>
        /// Refresh all player references
        /// </summary>
        public void RefreshPlayerReferences()
        {
            // Find all Player2 instances (including inactive)
            references.Player2s = Object.FindObjectsOfType<Player2>(true);
            Debug.Log($"[PlayerReferenceManager] Found {references.Player2s.Length} Player2 instances");
            
            // Find all Player instances (including inactive)
            Player[] allPlayers = Object.FindObjectsOfType<Player>(true);
            
            // Filter out Player2 instances to get only base Player
            List<Player> player1List = new List<Player>();
            foreach (Player p in allPlayers)
            {
                if (!(p is Player2))
                {
                    player1List.Add(p);
                }
            }
            references.Players = player1List.ToArray();
            Debug.Log($"[PlayerReferenceManager] Found {references.Players.Length} Player instances");
            
            // Find skills on all players
            FindSkills(allPlayers);
            
            // Warn if Player2 is active but has no skills attached
            ValidatePlayer2Skills();
            
            // Get stats from first available player
            GetPlayerStats();
        }
        
        private void FindSkills(Player[] allPlayers)
        {
            List<PlayerSkill_CirclingProjectiles> circlingSkillsList = new List<PlayerSkill_CirclingProjectiles>();
            List<PlayerSkill_PushWave> pushWaveSkillsList = new List<PlayerSkill_PushWave>();
            List<PlayerSkill_ExtraHand> extraHandSkillsList = new List<PlayerSkill_ExtraHand>();
            List<PlayerSkill_PiccoloFireCracker> piccoloFireCrackerSkillsList = new List<PlayerSkill_PiccoloFireCracker>();
            
            foreach (Player p in allPlayers)
            {
                if (p != null)
                {
                    var circlingSkill = p.GetComponent<PlayerSkill_CirclingProjectiles>();
                    if (circlingSkill != null)
                        circlingSkillsList.Add(circlingSkill);
                    
                    var pushWaveSkill = p.GetComponent<PlayerSkill_PushWave>();
                    if (pushWaveSkill != null)
                        pushWaveSkillsList.Add(pushWaveSkill);
                    
                    var extraHandSkill = p.GetComponent<PlayerSkill_ExtraHand>();
                    if (extraHandSkill != null)
                        extraHandSkillsList.Add(extraHandSkill);
                    
                    var piccoloFireCrackerSkill = p.GetComponent<PlayerSkill_PiccoloFireCracker>();
                    if (piccoloFireCrackerSkill != null)
                        piccoloFireCrackerSkillsList.Add(piccoloFireCrackerSkill);
                }
            }
            
            references.CirclingProjectilesSkills = circlingSkillsList.ToArray();
            references.PushWaveSkills = pushWaveSkillsList.ToArray();
            references.ExtraHandSkills = extraHandSkillsList.ToArray();
            references.PiccoloFireCrackerSkills = piccoloFireCrackerSkillsList.ToArray();
            
            Debug.Log($"[PlayerReferenceManager] Found {references.CirclingProjectilesSkills.Length} CirclingProjectiles skills, " +
                     $"{references.PushWaveSkills.Length} PushWave skills, {references.ExtraHandSkills.Length} ExtraHand skills, " +
                     $"and {references.PiccoloFireCrackerSkills.Length} PiccoloFireCracker skills");
        }
        
        private void ValidatePlayer2Skills()
        {
            if (references.Player2s != null && references.Player2s.Length > 0)
            {
                foreach (var p2 in references.Player2s)
                {
                    if (p2 != null && p2.gameObject.activeInHierarchy)
                    {
                        if (references.CirclingProjectilesSkills.Length == 0 && 
                            references.PushWaveSkills.Length == 0 && 
                            references.ExtraHandSkills.Length == 0)
                        {
                            Debug.LogWarning($"[PlayerReferenceManager] Player2 '{p2.gameObject.name}' has NO skill components attached! " +
                                           $"Add PlayerSkill_ExtraHand, PlayerSkill_CirclingProjectiles, and PlayerSkill_PushWave components.", p2.gameObject);
                        }
                    }
                }
            }
        }
        
        private void GetPlayerStats()
        {
            if (references.Player2s != null && references.Player2s.Length > 0)
            {
                if (references.Player2Stats == null)
                {
                    references.Player2Stats = references.Player2s[0].Stats;
                }
            }
            else if (references.Players != null && references.Players.Length > 0)
            {
                if (references.PlayerStats == null)
                {
                    references.PlayerStats = references.Players[0].Stats;
                }
            }
        }
        
        /// <summary>
        /// Check if Player2 is the active character
        /// </summary>
        public bool IsPlayer2Active()
        {
            // Method 1: Check CharacterSelectionManager
            if (CharacterSelectionManager.Instance != null)
            {
                int selectedIndex = CharacterSelectionManager.Instance.SelectedCharacterIndex;
                if (selectedIndex == 1)
                    return true;
                if (selectedIndex == 0)
                    return false;
            }
            
            // Method 2: Check if Player2 instances exist and are active
            if (references.Player2s != null && references.Player2s.Length > 0)
            {
                foreach (var p2 in references.Player2s)
                {
                    if (p2 != null && p2.gameObject.activeInHierarchy)
                        return true;
                }
            }
            
            // Method 3: Check by finding active player type in scene
            Player2 activePlayer2 = Object.FindObjectOfType<Player2>();
            if (activePlayer2 != null && activePlayer2.gameObject.activeInHierarchy)
                return true;
            
            // Default to Player1
            return false;
        }
    }
}
