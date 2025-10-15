using UnityEngine;
using System;

/// <summary>
/// Manages player stat upgrades after each wave completion
/// Integrates with WaveManager and updates Player_DataSO in real-time
/// </summary>
public class PlayerUpgradeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player_DataSO playerStats;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private PlayerUpgradeUI upgradeUI;
    
    [Header("Upgrade Values")]
    [SerializeField] private float damageUpgradeAmount = 5f;
    [SerializeField] private float healthUpgradeAmount = 20f;
    [SerializeField] private float speedUpgradeAmount = 0.5f;
    
    [Header("Auto-Setup")]
    [SerializeField] private bool autoFindReferences = true;
    
    private Player player;
    private bool upgradePending = false;
    
    public enum UpgradeType
    {
        Damage,
        Health,
        Speed
    }
    
    private void Awake()
    {
        if (autoFindReferences)
        {
            SetupReferences();
        }
    }
    
    private void Start()
    {
        // Subscribe to wave completion event
        if (waveManager != null)
        {
            waveManager.OnAllEnemiesCleared.AddListener(OnWaveCleared);
        }
        else
        {
            Debug.LogWarning("PlayerUpgradeManager: WaveManager not assigned!");
        }
        
        // Hide upgrade UI at start
        if (upgradeUI != null)
        {
            upgradeUI.HideUpgradePanel();
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from wave events
        if (waveManager != null)
        {
            waveManager.OnAllEnemiesCleared.RemoveListener(OnWaveCleared);
        }
    }
    
    /// <summary>
    /// Auto-setup references if not assigned
    /// </summary>
    private void SetupReferences()
    {
        // Find player
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }
        
        // Get player stats from player
        if (playerStats == null && player != null)
        {
            playerStats = player.Stats;
        }
        
        // Find wave manager
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
        }
        
        // Find or create upgrade UI
        if (upgradeUI == null)
        {
            upgradeUI = FindObjectOfType<PlayerUpgradeUI>();
            
            if (upgradeUI == null)
            {
                // Create upgrade UI
                GameObject uiObj = new GameObject("PlayerUpgradeUI");
                upgradeUI = uiObj.AddComponent<PlayerUpgradeUI>();
            }
        }
        
        // Setup UI with this manager
        if (upgradeUI != null)
        {
            upgradeUI.SetupWithManager(this);
        }
    }
    
    /// <summary>
    /// Called when a wave is cleared
    /// </summary>
    private void OnWaveCleared(int waveNumber)
    {
        Debug.Log($"PlayerUpgradeManager: Wave {waveNumber} cleared! Showing upgrade options...");
        
        // Pause wave progression
        if (waveManager != null)
        {
            waveManager.PauseWaves();
        }
        
        // Show upgrade UI
        upgradePending = true;
        if (upgradeUI != null)
        {
            upgradeUI.ShowUpgradePanel();
        }
        
        // Pause game
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Apply the selected upgrade
    /// </summary>
    public void ApplyUpgrade(UpgradeType upgradeType)
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerUpgradeManager: Cannot apply upgrade - playerStats is null!");
            return;
        }
        
        switch (upgradeType)
        {
            case UpgradeType.Damage:
                playerStats.projectileDamage += damageUpgradeAmount;
                Debug.Log($"Damage upgraded! New damage: {playerStats.projectileDamage}");
                break;
                
            case UpgradeType.Health:
                playerStats.maxHealth += healthUpgradeAmount;
                // Also increase player's max health and heal to full
                if (player != null)
                {
                    var health = player.GetComponent<Entity_Health>();
                    if (health != null)
                    {
                        health.IncreaseMaxHealth(healthUpgradeAmount, true); // Increase max and heal to full
                    }
                }
                Debug.Log($"Max Health upgraded! New max health: {playerStats.maxHealth}");
                break;
                
            case UpgradeType.Speed:
                playerStats.moveSpeed += speedUpgradeAmount;
                Debug.Log($"Move Speed upgraded! New speed: {playerStats.moveSpeed}");
                break;
        }
        
        // Hide UI
        if (upgradeUI != null)
        {
            upgradeUI.HideUpgradePanel();
        }
        
        // Resume wave progression
        upgradePending = false;
        if (waveManager != null)
        {
            waveManager.ResumeWaves();
        }
        
        // Unpause game
        Time.timeScale = 1f;
    }
    
    // Public getters for upgrade amounts (for UI display)
    public float GetDamageUpgradeAmount() => damageUpgradeAmount;
    public float GetHealthUpgradeAmount() => healthUpgradeAmount;
    public float GetSpeedUpgradeAmount() => speedUpgradeAmount;
    
    public float GetCurrentDamage() => playerStats != null ? playerStats.projectileDamage : 0f;
    public float GetCurrentHealth() => playerStats != null ? playerStats.maxHealth : 0f;
    public float GetCurrentSpeed() => playerStats != null ? playerStats.moveSpeed : 0f;
}
