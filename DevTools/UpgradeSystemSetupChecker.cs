using UnityEngine;

/// <summary>
/// Development tool to verify the upgrade system is properly configured
/// Add this to any GameObject and check the console for setup status
/// </summary>
public class UpgradeSystemSetupChecker : MonoBehaviour
{
    [Header("Auto-check on Start")]
    [SerializeField] private bool checkOnStart = true;
    
    [ContextMenu("Check Setup")]
    public void CheckSetup()
    {
        Debug.Log("=== UPGRADE SYSTEM SETUP CHECK ===");
        
        bool allGood = true;
        
        // Check Player
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            Debug.Log("✓ Player found");
            
            if (player.Stats != null)
            {
                Debug.Log($"✓ Player_DataSO assigned: {player.Stats.name}");
                Debug.Log($"  - Base Damage: {player.Stats.projectileDamage}");
                Debug.Log($"  - Max Health: {player.Stats.maxHealth}");
                Debug.Log($"  - Move Speed: {player.Stats.moveSpeed}");
                Debug.Log($"  - Critical Chance: {player.Stats.criticalChance}%");
                Debug.Log($"  - Critical Multiplier: {player.Stats.criticalDamageMultiplier}x");
            }
            else
            {
                Debug.LogError("✗ Player_DataSO not assigned to Player!");
                allGood = false;
            }
        }
        else
        {
            Debug.LogError("✗ Player not found in scene!");
            allGood = false;
        }
        
        // Check WaveManager
        WaveManager waveManager = FindObjectOfType<WaveManager>();
        if (waveManager != null)
        {
            Debug.Log("✓ WaveManager found");
            Debug.Log($"  - Current Wave: {waveManager.GetCurrentWave()}");
            Debug.Log($"  - Waves Activated: {waveManager.AreWavesActivated()}");
        }
        else
        {
            Debug.LogError("✗ WaveManager not found in scene!");
            allGood = false;
        }
        
        // Check PlayerUpgradeManager
        PlayerUpgradeManager upgradeManager = FindObjectOfType<PlayerUpgradeManager>();
        if (upgradeManager != null)
        {
            Debug.Log("✓ PlayerUpgradeManager found");
            Debug.Log($"  - Damage Upgrade: +{upgradeManager.GetDamageUpgradeAmount()}");
            Debug.Log($"  - Health Upgrade: +{upgradeManager.GetHealthUpgradeAmount()}");
            Debug.Log($"  - Speed Upgrade: +{upgradeManager.GetSpeedUpgradeAmount()}");
        }
        else
        {
            Debug.LogWarning("⚠ PlayerUpgradeManager not found - Add it to scene!");
            Debug.LogWarning("  → Create empty GameObject, add PlayerUpgradeManager component");
            allGood = false;
        }
        
        // Check PlayerUpgradeUI
        PlayerUpgradeUI upgradeUI = FindObjectOfType<PlayerUpgradeUI>();
        if (upgradeUI != null)
        {
            Debug.Log("✓ PlayerUpgradeUI found (or will be auto-created)");
        }
        else
        {
            Debug.Log("→ PlayerUpgradeUI will be auto-created by PlayerUpgradeManager");
        }
        
        // Check Player_RangeAttack
        Player_RangeAttack rangeAttack = FindObjectOfType<Player_RangeAttack>();
        if (rangeAttack != null)
        {
            Debug.Log("✓ Player_RangeAttack found (critical hits enabled)");
        }
        else
        {
            Debug.LogWarning("⚠ Player_RangeAttack not found - Critical hits may not work");
        }
        
        // Summary
        Debug.Log("=== SETUP CHECK COMPLETE ===");
        if (allGood)
        {
            Debug.Log("✓✓✓ ALL SYSTEMS READY! ✓✓✓");
            Debug.Log("Start the game, complete a wave, and test upgrades!");
        }
        else
        {
            Debug.LogWarning("⚠ SOME COMPONENTS MISSING - See errors above");
        }
    }
    
    private void Start()
    {
        if (checkOnStart)
        {
            Invoke(nameof(CheckSetup), 0.5f);
        }
    }
    
    [ContextMenu("Test Critical Hit Roll")]
    public void TestCriticalRoll()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null && player.Stats != null)
        {
            Debug.Log("=== CRITICAL HIT TEST ===");
            int crits = 0;
            int total = 100;
            
            for (int i = 0; i < total; i++)
            {
                if (player.Stats.RollCriticalHit())
                {
                    crits++;
                }
            }
            
            float actualRate = (crits / (float)total) * 100f;
            float expectedRate = player.Stats.criticalChance;
            
            Debug.Log($"Critical hits: {crits}/{total} ({actualRate:F1}%)");
            Debug.Log($"Expected rate: {expectedRate:F1}%");
            Debug.Log($"Difference: {Mathf.Abs(actualRate - expectedRate):F1}%");
        }
        else
        {
            Debug.LogError("Cannot test - Player or Player_DataSO not found!");
        }
    }
    
    [ContextMenu("Show Test Critical Indicator")]
    public void TestCriticalIndicator()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            Vector3 pos = player.transform.position + Vector3.forward * 2f + Vector3.up;
            CriticalHitIndicator.ShowCritical(pos);
            Debug.Log("Critical indicator shown at player position");
        }
        else
        {
            Debug.LogError("Cannot test - Player not found!");
        }
    }
}
