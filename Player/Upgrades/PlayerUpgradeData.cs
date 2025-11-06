using UnityEngine;

/// <summary>
/// Core data structures for the player upgrade system
/// </summary>
public static class PlayerUpgradeData
{
    /// <summary>
    /// Defines all available upgrade types for players
    /// </summary>
    public enum UpgradeType
    {
        Damage,
        MaxHealth,
        CriticalChance,
        CriticalDamage,
        Evasion,
        UpgradeCirclingProjectiles,
        UpgradePushWave,
        UpgradeExtraHand,
        UpgradePiccoloFireCracker,
        // Player2 specific upgrades
        UpgradeBlinkDistance
    }
    
    /// <summary>
    /// Container for player references
    /// </summary>
    public class PlayerReferences
    {
        public Player[] Players;
        public Player2[] Player2s;
        public PlayerSkill_CirclingProjectiles[] CirclingProjectilesSkills;
        public PlayerSkill_PushWave[] PushWaveSkills;
        public PlayerSkill_ExtraHand[] ExtraHandSkills;
        public PlayerSkill_PiccoloFireCracker[] PiccoloFireCrackerSkills;
        public Player_DataSO PlayerStats;
        public Player2_DataSO Player2Stats;
    }
}
