using UnityEngine;
using PlayerUpgrades;
using static PlayerUpgradeData;

/// <summary>
/// Main coordinator for player upgrades after wave completion.
/// Integrates with WaveManager and updates Player_DataSO in real-time.
/// Delegates specific responsibilities to modular components.
/// </summary>
public class PlayerUpgradeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player_DataSO playerStats;
    [SerializeField] private Player2_DataSO player2Stats;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private PlayerUpgradeUI upgradeUI;
    
    [Header("Upgrade Configuration")]
    [SerializeField] private PlayerUpgradeConfig config = new PlayerUpgradeConfig();
    
    [Header("Auto-Setup")]
    [SerializeField] private bool autoFindReferences = true;
    
    // Modular components
    private PlayerReferenceManager referenceManager;
    private UpgradeProvider upgradeProvider;
    private UpgradeApplicator upgradeApplicator;
    private PlayerUpgradeStats statsProvider;
    
    private bool upgradePending = false;
    private UpgradeType[] currentUpgradeOptions = new UpgradeType[3];
    
    private void Awake()
    {
        InitializeComponents();
        
        if (autoFindReferences)
        {
            SetupReferences();
        }
    }
    
    private void Start()
    {
        // Setup references first
        if (autoFindReferences)
        {
            SetupReferences();
        }
        
        // Note: We no longer subscribe to wave events
        // PlayerUpgradeUI handles showing upgrades on level up directly
        
        // Hide upgrade UI at start
        if (upgradeUI != null)
        {
            upgradeUI.HideUpgradePanel();
        }
        
        // DON'T generate initial upgrades here - wait until player levels up
        // This avoids issues when players aren't spawned yet
        // Upgrades will be generated when ShowUpgradePanel is called
    }
    
    private void OnDestroy()
    {
        // Note: No need to unsubscribe since we don't subscribe to wave events anymore
    }
    
    /// <summary>
    /// Initialize modular components
    /// </summary>
    private void InitializeComponents()
    {
        referenceManager = new PlayerReferenceManager();
        upgradeProvider = new UpgradeProvider(referenceManager);
        upgradeApplicator = new UpgradeApplicator(config, referenceManager);
        statsProvider = new PlayerUpgradeStats(config, referenceManager);
    }
    
    /// <summary>
    /// Setup all references
    /// </summary>
    private void SetupReferences()
    {
        RefreshPlayerReferences();
        SetupWaveManager();
        SetupUpgradeUI();
    }
    
    /// <summary>
    /// Refresh player references
    /// </summary>
    private void RefreshPlayerReferences()
    {
        referenceManager.RefreshPlayerReferences();
        
        // Update local stat references
        var refs = referenceManager.References;
        if (playerStats == null) playerStats = refs.PlayerStats;
        if (player2Stats == null) player2Stats = refs.Player2Stats;
    }
    
    /// <summary>
    /// Setup wave manager reference
    /// </summary>
    private void SetupWaveManager()
    {
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
        }
    }
    
    /// <summary>
    /// Setup upgrade UI reference
    /// </summary>
    private void SetupUpgradeUI()
    {
        if (upgradeUI == null)
        {
            upgradeUI = FindObjectOfType<PlayerUpgradeUI>();
            
            if (upgradeUI == null)
            {
                GameObject uiObj = new GameObject("PlayerUpgradeUI");
                upgradeUI = uiObj.AddComponent<PlayerUpgradeUI>();
            }
        }
        
        if (upgradeUI != null)
        {
            upgradeUI.SetupWithManager(this);
        }
    }
    
    // Note: Wave event subscription removed - upgrades now trigger on level up via PlayerUpgradeUI
    
    /// <summary>
    /// Generate random upgrade options
    /// </summary>
    public void GenerateRandomUpgrades()
    {
        // Refresh references in case players were spawned after initialization
        RefreshPlayerReferences();
        
        currentUpgradeOptions = upgradeProvider.GenerateRandomUpgrades();
        Debug.Log($"[PlayerUpgradeManager] Generated {currentUpgradeOptions.Length} upgrade options: {string.Join(", ", currentUpgradeOptions)}");
    }
    
    // Note: PauseGameForUpgrade removed - PlayerUpgradeUI handles this directly on level up
    
    /// <summary>
    /// Apply the selected upgrade
    /// </summary>
    public void ApplyUpgrade(UpgradeType upgradeType)
    {
        // Refresh player references in case players were spawned after Awake
        RefreshPlayerReferences();
        
        // Apply the upgrade using the applicator FIRST
        upgradeApplicator.ApplyUpgrade(upgradeType);
        
        // Generate new random upgrades for next time AFTER applying (so level is updated)
        GenerateRandomUpgrades();
        
        // Resume game
        ResumeGameAfterUpgrade();
    }
    
    /// <summary>
    /// Resume game after upgrade
    /// </summary>
    private void ResumeGameAfterUpgrade()
    {
        if (upgradeUI != null)
        {
            upgradeUI.HideUpgradePanel();
        }
        
        upgradePending = false;
        
        // Note: Time.timeScale is handled by PlayerUpgradeUI
        // WaveManager automatically resumes when player levels up
    }
    
    #region Public Getters for UI
    
    public UpgradeType[] GetCurrentUpgradeOptions() => currentUpgradeOptions;
    
    // Delegate stat queries to stats provider
    public float GetDamageUpgradeAmount() => statsProvider.GetDamageUpgradeAmount();
    public float GetMaxHealthUpgradeAmount() => statsProvider.GetMaxHealthUpgradeAmount();
    public float GetCriticalChanceUpgradeAmount() => statsProvider.GetCriticalChanceUpgradeAmount();
    public float GetCriticalDamageUpgradeAmount() => statsProvider.GetCriticalDamageUpgradeAmount();
    public float GetEvasionChanceUpgradeAmount() => statsProvider.GetEvasionChanceUpgradeAmount();
    
    public float GetCurrentDamage() => statsProvider.GetCurrentDamage();
    public float GetCurrentHealth() => statsProvider.GetCurrentHealth();
    public float GetCurrentCriticalChance() => statsProvider.GetCurrentCriticalChance();
    public float GetCurrentCriticalDamage() => statsProvider.GetCurrentCriticalDamage();
    public float GetCurrentEvasion() => statsProvider.GetCurrentEvasion();
    
    // Player2 specific getters
    public float GetBlinkDistanceUpgradeAmount() => statsProvider.GetBlinkDistanceUpgradeAmount();
    public float GetCurrentBlinkDistance() => statsProvider.GetCurrentBlinkDistance();
    
    // Stat upgrade level getters
    public int GetDamageUpgradeLevel() => statsProvider.GetDamageUpgradeLevel();
    public int GetMaxHealthUpgradeLevel() => statsProvider.GetMaxHealthUpgradeLevel();
    public int GetCriticalChanceUpgradeLevel() => statsProvider.GetCriticalChanceUpgradeLevel();
    public int GetCriticalDamageUpgradeLevel() => statsProvider.GetCriticalDamageUpgradeLevel();
    public int GetEvasionUpgradeLevel() => statsProvider.GetEvasionUpgradeLevel();
    public int GetBlinkDistanceUpgradeLevel() => statsProvider.GetBlinkDistanceUpgradeLevel();

    public int GetStatUpgradeMaxLevel() => statsProvider.GetStatUpgradeMaxLevel();
    
    // Skill stat getters
    public int GetCirclingProjectilesLevel() => statsProvider.GetCirclingProjectilesLevel();
    public int GetCirclingProjectilesMaxLevel() => statsProvider.GetCirclingProjectilesMaxLevel();
    public int GetCurrentProjectileCount() => statsProvider.GetCurrentProjectileCount();
    public float GetCurrentProjectileDamage() => statsProvider.GetCurrentProjectileDamage();
    public float GetCurrentProjectileRadius() => statsProvider.GetCurrentProjectileRadius();
    public float GetCurrentProjectileSpeed() => statsProvider.GetCurrentProjectileSpeed();
    
    public int GetPushWaveLevel() => statsProvider.GetPushWaveLevel();
    public int GetPushWaveMaxLevel() => statsProvider.GetPushWaveMaxLevel();
    public float GetPushWaveRadius() => statsProvider.GetPushWaveRadius();
    public float GetPushWaveForce() => statsProvider.GetPushWaveForce();
    public float GetPushWaveDamage() => statsProvider.GetPushWaveDamage();
    public float GetPushWaveInterval() => statsProvider.GetPushWaveInterval();
    
    public int GetExtraHandLevel() => statsProvider.GetExtraHandLevel();
    public int GetExtraHandMaxLevel() => statsProvider.GetExtraHandMaxLevel();
    public float GetExtraHandDamage() => statsProvider.GetExtraHandDamage();
    public float GetExtraHandShootInterval() => statsProvider.GetExtraHandShootInterval();
    public float GetExtraHandRange() => statsProvider.GetExtraHandRange();
    
    public int GetSpearThrowLevel() => statsProvider.GetSpearThrowLevel();
    public int GetSpearThrowMaxLevel() => statsProvider.GetSpearThrowMaxLevel();
    
    public float GetDefenseAbsorptionPercent() => statsProvider.GetDefenseAbsorptionPercent();
    public float GetDefenseAbsorptionChance() => statsProvider.GetDefenseAbsorptionChance();
    
    public int GetPiccoloFireCrackerLevel() => statsProvider.GetPiccoloFireCrackerLevel();
    public int GetPiccoloFireCrackerMaxLevel() => statsProvider.GetPiccoloFireCrackerMaxLevel();
    public float GetPiccoloFireCrackerDamage() => statsProvider.GetPiccoloFireCrackerDamage();
    public float GetPiccoloFireCrackerRadius() => statsProvider.GetPiccoloFireCrackerRadius();
    public float GetPiccoloFireCrackerExplosionTime() => statsProvider.GetPiccoloFireCrackerExplosionTime();
    public int GetPiccoloFireCrackerBombCount() => statsProvider.GetPiccoloFireCrackerBombCount();
    
    #endregion
}
