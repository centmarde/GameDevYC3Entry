using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple setup script to automatically configure the Enemy Pointer System.
/// Just add this to any GameObject in your scene and it will set everything up.
/// </summary>
public class EnemyPointerSystemSetup : MonoBehaviour
{
    [Header("Auto Setup Options")]
    [SerializeField] private bool setupOnAwake = true;
    [SerializeField] private bool createPointerManager = true;
    [SerializeField] private bool createSettingsUI = true;
    [SerializeField] private bool configureCamera = true;
    
    [Header("Pointer Settings")]
    [SerializeField] private float maxPointerDistance = 100f;
    [SerializeField] private float minPointerDistance = 10f;
    [SerializeField] private bool showOnlyOffscreenEnemies = true;
    [SerializeField] private float updateFrequency = 0.1f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color pointerColor = Color.red;
    [SerializeField] private Vector2 pointerSize = new Vector2(30f, 30f);
    
    private void Awake()
    {
        if (setupOnAwake)
        {
            SetupEnemyPointerSystem();
        }
    }
    
    /// <summary>
    /// Manually trigger the setup process
    /// </summary>
    [ContextMenu("Setup Enemy Pointer System")]
    public void SetupEnemyPointerSystem()
    {
        Debug.Log("=== ENEMY POINTER SYSTEM SETUP STARTING ===");
        
        int stepNumber = 1;
        
        // Step 1: Configure camera
        if (configureCamera)
        {
            Debug.Log($"Step {stepNumber++}: Configuring camera...");
            ConfigureIsoFollowCamera();
        }
        
        // Step 2: Setup pointer manager
        if (createPointerManager)
        {
            Debug.Log($"Step {stepNumber++}: Setting up pointer manager...");
            SetupPointerManager();
        }
        
        // Step 3: Setup settings UI
        if (createSettingsUI)
        {
            Debug.Log($"Step {stepNumber++}: Setting up settings UI...");
            SetupSettingsUI();
        }
        
        // Step 4: Configure existing enemies
        Debug.Log($"Step {stepNumber++}: Configuring existing enemies...");
        ConfigureExistingEnemies();
        
        Debug.Log("=== SETUP COMPLETE ===");
        LogSetupInstructions();
    }
    
    /// <summary>
    /// Configure IsoFollowCamera - add it to the main camera if missing
    /// </summary>
    private void ConfigureIsoFollowCamera()
    {
        IsoFollowCamera existingCamera = FindObjectOfType<IsoFollowCamera>();
        
        if (existingCamera != null)
        {
            Debug.Log($"  ✓ IsoFollowCamera found on '{existingCamera.gameObject.name}'");
            return;
        }
        
        // Find main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera != null)
        {
            // Check if it already has the component (shouldn't happen, but safety check)
            if (mainCamera.GetComponent<IsoFollowCamera>() == null)
            {
                mainCamera.gameObject.AddComponent<IsoFollowCamera>();
                Debug.Log($"  ✓ Added IsoFollowCamera to '{mainCamera.gameObject.name}'");
            }
        }
        else
        {
            Debug.LogWarning("  ✗ No camera found in scene!");
        }
    }
    
    /// <summary>
    /// Setup the pointer manager with configured settings
    /// </summary>
    private void SetupPointerManager()
    {
        EnemyPointerManager existingManager = FindObjectOfType<EnemyPointerManager>();
        
        if (existingManager == null)
        {
            // Create new pointer manager GameObject
            GameObject managerObj = new GameObject("EnemyPointerManager");
            EnemyPointerManager manager = managerObj.AddComponent<EnemyPointerManager>();
            
            // Apply configuration
            manager.SetMaxDistance(maxPointerDistance);
            manager.SetMinDistance(minPointerDistance);
            manager.SetShowOnlyOffscreen(showOnlyOffscreenEnemies);
            manager.SetUpdateFrequency(updateFrequency);
            
            Debug.Log("  ✓ Created EnemyPointerManager");
            Debug.Log($"    - Max Distance: {maxPointerDistance}m");
            Debug.Log($"    - Min Distance: {minPointerDistance}m");
            Debug.Log($"    - Offscreen Only: {showOnlyOffscreenEnemies}");
        }
        else
        {
            // Update existing manager with new settings
            existingManager.SetMaxDistance(maxPointerDistance);
            existingManager.SetMinDistance(minPointerDistance);
            existingManager.SetShowOnlyOffscreen(showOnlyOffscreenEnemies);
            existingManager.SetUpdateFrequency(updateFrequency);
            
            Debug.Log("  ✓ Updated existing EnemyPointerManager");
        }
    }
    
    /// <summary>
    /// Setup the settings UI
    /// </summary>
    private void SetupSettingsUI()
    {
        EnemyPointerSettingsUI existingUI = FindObjectOfType<EnemyPointerSettingsUI>();
        
        if (existingUI == null)
        {
            // Create new settings UI GameObject
            GameObject uiObj = new GameObject("EnemyPointerSettingsUI");
            uiObj.AddComponent<EnemyPointerSettingsUI>();
            
            Debug.Log("  ✓ Created EnemyPointerSettingsUI");
            Debug.Log("    - Press [P] to toggle settings");
        }
        else
        {
            Debug.Log("  ✓ Found existing EnemyPointerSettingsUI");
        }
    }
    
    /// <summary>
    /// Configure existing enemies in the scene
    /// </summary>
    private void ConfigureExistingEnemies()
    {
        int configuredCount = 0;
        int totalEnemies = 0;
        bool usedTagSearch = true;
        
        // Try finding enemies by tag first
        try
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            totalEnemies = enemies.Length;
            
            foreach (GameObject enemy in enemies)
            {
                if (enemy.GetComponent<EnemyDeathTracker>() == null)
                {
                    enemy.AddComponent<EnemyDeathTracker>();
                    configuredCount++;
                }
            }
        }
        catch (UnityException)
        {
            // "Enemy" tag doesn't exist - try finding by name
            usedTagSearch = false;
            Debug.Log("  ℹ 'Enemy' tag not found, searching by name...");
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("enemy"))
                {
                    totalEnemies++;
                    if (obj.GetComponent<EnemyDeathTracker>() == null)
                    {
                        obj.AddComponent<EnemyDeathTracker>();
                        configuredCount++;
                    }
                }
            }
        }
        
        // Log results
        if (totalEnemies > 0)
        {
            string searchMethod = usedTagSearch ? "by tag" : "by name";
            Debug.Log($"  ✓ Found {totalEnemies} enemies {searchMethod}");
            if (configuredCount > 0)
            {
                Debug.Log($"    - Added EnemyDeathTracker to {configuredCount} enemies");
            }
        }
        else
        {
            Debug.Log("  ℹ No enemies in scene (will track when spawned)");
        }
    }
    
    /// <summary>
    /// Log setup instructions for the user
    /// </summary>
    private void LogSetupInstructions()
    {
        Debug.Log("\n" + "=".PadRight(50, '='));
        Debug.Log("ENEMY POINTER SYSTEM - READY!");
        Debug.Log("=".PadRight(50, '='));
        Debug.Log("📍 Press [P] to open settings");
        Debug.Log("🎯 Pointers show direction to off-screen enemies");
        Debug.Log("📏 Distance text shows how far enemies are");
        Debug.Log("🔄 New enemies are tracked automatically");
        Debug.Log("✨ System is fully operational!");
        Debug.Log("=".PadRight(50, '=') + "\n");
    }
    
    /// <summary>
    /// Validate the current setup
    /// </summary>
    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        Debug.Log("\n" + "=".PadRight(50, '='));
        Debug.Log("VALIDATING ENEMY POINTER SYSTEM");
        Debug.Log("=".PadRight(50, '=') + "\n");
        
        bool allGood = true;
        
        // Check camera
        IsoFollowCamera isoCamera = FindObjectOfType<IsoFollowCamera>();
        Camera mainCamera = Camera.main;
        
        if (isoCamera != null)
        {
            Debug.Log($"✓ IsoFollowCamera: Found on '{isoCamera.gameObject.name}'");
        }
        else
        {
            Debug.LogWarning("✗ IsoFollowCamera: NOT FOUND");
            allGood = false;
        }
        
        if (mainCamera != null)
        {
            Debug.Log($"✓ Main Camera: '{mainCamera.gameObject.name}'");
        }
        else
        {
            Debug.LogWarning("✗ Main Camera: NOT FOUND");
            allGood = false;
        }
        
        // Check pointer manager
        EnemyPointerManager pointerManager = FindObjectOfType<EnemyPointerManager>();
        if (pointerManager != null)
        {
            int trackedEnemies = pointerManager.GetTrackedEnemyCount();
            int activePointers = pointerManager.GetActivePointerCount();
            Debug.Log($"✓ Pointer Manager: Active");
            Debug.Log($"  - Tracking: {trackedEnemies} enemies");
            Debug.Log($"  - Showing: {activePointers} pointers");
        }
        else
        {
            Debug.LogWarning("✗ Pointer Manager: NOT FOUND");
            allGood = false;
        }
        
        // Check settings UI
        EnemyPointerSettingsUI settingsUI = FindObjectOfType<EnemyPointerSettingsUI>();
        if (settingsUI != null)
        {
            Debug.Log("✓ Settings UI: Ready (Press P to toggle)");
        }
        else
        {
            Debug.LogWarning("✗ Settings UI: NOT FOUND");
            allGood = false;
        }
        
        // Check enemies
        int totalEnemies = 0;
        int enemiesWithTracker = 0;
        bool usedTag = true;
        
        try
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            totalEnemies = enemies.Length;
            
            foreach (GameObject enemy in enemies)
            {
                if (enemy.GetComponent<EnemyDeathTracker>() != null)
                {
                    enemiesWithTracker++;
                }
            }
        }
        catch (UnityException)
        {
            usedTag = false;
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("enemy"))
                {
                    totalEnemies++;
                    if (obj.GetComponent<EnemyDeathTracker>() != null)
                    {
                        enemiesWithTracker++;
                    }
                }
            }
        }
        
        string searchMethod = usedTag ? "by tag" : "by name";
        if (totalEnemies > 0)
        {
            Debug.Log($"✓ Enemies: Found {totalEnemies} {searchMethod}");
            Debug.Log($"  - {enemiesWithTracker} have EnemyDeathTracker");
        }
        else
        {
            Debug.Log($"ℹ Enemies: None in scene (will track on spawn)");
        }
        
        // Final status
        Debug.Log("\n" + "=".PadRight(50, '='));
        if (allGood)
        {
            Debug.Log("STATUS: ✓ ALL SYSTEMS OPERATIONAL");
        }
        else
        {
            Debug.LogWarning("STATUS: ⚠ SOME ISSUES DETECTED");
        }
        Debug.Log("=".PadRight(50, '=') + "\n");
    }
    
    /// <summary>
    /// Clean up and remove all enemy pointer system components
    /// </summary>
    [ContextMenu("Remove Enemy Pointer System")]
    public void RemoveEnemyPointerSystem()
    {
        Debug.Log("\n" + "=".PadRight(50, '='));
        Debug.Log("REMOVING ENEMY POINTER SYSTEM");
        Debug.Log("=".PadRight(50, '=') + "\n");
        
        int removedCount = 0;
        
        // Remove pointer manager
        EnemyPointerManager pointerManager = FindObjectOfType<EnemyPointerManager>();
        if (pointerManager != null)
        {
            DestroyImmediate(pointerManager.gameObject);
            Debug.Log("✓ Removed EnemyPointerManager");
            removedCount++;
        }
        
        // Remove settings UI
        EnemyPointerSettingsUI settingsUI = FindObjectOfType<EnemyPointerSettingsUI>();
        if (settingsUI != null)
        {
            DestroyImmediate(settingsUI.gameObject);
            Debug.Log("✓ Removed EnemyPointerSettingsUI");
            removedCount++;
        }
        
        // Note: IsoFollowCamera is kept as it's part of the core camera system
        Debug.Log("ℹ IsoFollowCamera kept (core camera component)");
        
        Debug.Log("\n" + "=".PadRight(50, '='));
        Debug.Log($"REMOVAL COMPLETE - {removedCount} components removed");
        Debug.Log("=".PadRight(50, '=') + "\n");
    }
}