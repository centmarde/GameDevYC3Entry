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
        SubscribeToEvents();
        
        // Hide upgrade UI at start
        if (upgradeUI != null)
        {
            upgradeUI.HideUpgradePanel();
        }
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
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
    
    /// <summary>
    /// Subscribe to wave events
    /// </summary>
    private void SubscribeToEvents()
    {
        if (waveManager != null)
        {
            waveManager.OnAllEnemiesCleared.AddListener(OnWaveCleared);
        }
    }
    
    /// <summary>
    /// Unsubscribe from wave events
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (waveManager != null)
        {
            waveManager.OnAllEnemiesCleared.RemoveListener(OnWaveCleared);
        }
    }
    
    /// <summary>
    /// Called when a wave is cleared
    /// </summary>
    private void OnWaveCleared(int waveNumber)
    {
        GenerateRandomUpgrades();
        PauseGameForUpgrade();
    }
    
    /// <summary>
    /// Generate random upgrade options
    /// </summary>
    private void GenerateRandomUpgrades()
    {
        currentUpgradeOptions = upgradeProvider.GenerateRandomUpgrades();
    }
    
    /// <summary>
    /// Pause game and show upgrade UI
    /// </summary>
    private void PauseGameForUpgrade()
    {
        if (waveManager != null)
        {
            waveManager.PauseWaves();
        }
        
        upgradePending = true;
        if (upgradeUI != null)
        {
            upgradeUI.ShowUpgradePanel();
        }
        
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Apply the selected upgrade
    /// </summary>
    public void ApplyUpgrade(UpgradeType upgradeType)
    {
        // Refresh player references in case players were spawned after Awake
        RefreshPlayerReferences();
        
        // Apply the upgrade using the applicator
        upgradeApplicator.ApplyUpgrade(upgradeType);
        
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
        if (waveManager != null)
        {
            waveManager.ResumeWaves();
        }
        
        Time.timeScale = 1f;
    }
    
    #region Public Getters for UI
    
    public UpgradeType[] GetCurrentUpgradeOptions() => currentUpgradeOptions;
    
    // Delegate stat queries to stats provider
    public float GetDamageUpgradeAmount() => statsProvider.GetDamageUpgradeAmount();
    public float GetMaxHealthUpgradeAmount() => statsProvider.GetMaxHealthUpgradeAmount();
    public float GetHealAmount() => statsProvider.GetHealAmount();
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
    public float GetBlinkCooldownReduction() => statsProvider.GetBlinkCooldownReduction();
    public float GetDashCooldownReduction() => statsProvider.GetDashCooldownReduction();
    public float GetBlinkDashSpeedUpgrade() => statsProvider.GetBlinkDashSpeedUpgrade();
    
    public float GetCurrentBlinkDistance() => statsProvider.GetCurrentBlinkDistance();
    public float GetCurrentBlinkCooldown() => statsProvider.GetCurrentBlinkCooldown();
    public float GetCurrentDashCooldown() => statsProvider.GetCurrentDashCooldown();
    public float GetCurrentBlinkDashSpeed() => statsProvider.GetCurrentBlinkDashSpeed();
    
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
    
    #endregion
}
