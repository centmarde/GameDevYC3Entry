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
        // Step 1: Configure camera
        if (configureCamera)
        {
            ConfigureIsoFollowCamera();
        }
        
        // Step 2: Setup pointer manager
        if (createPointerManager)
        {
            SetupPointerManager();
        }
        
        // Step 3: Setup settings UI
        if (createSettingsUI)
        {
            SetupSettingsUI();
        }
        
        // Step 4: Configure existing enemies
        ConfigureExistingEnemies();
        
        LogSetupInstructions();
    }
    
    /// <summary>
    /// Configure IsoCameraFollow - add it to the main camera if missing
    /// </summary>
    private void ConfigureIsoFollowCamera()
    {
        IsoCameraFollow existingCamera = FindObjectOfType<IsoCameraFollow>();
        
        if (existingCamera != null)
        {
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
            if (mainCamera.GetComponent<IsoCameraFollow>() == null)
            {
                mainCamera.gameObject.AddComponent<IsoCameraFollow>();
            }
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
        }
        else
        {
            // Update existing manager with new settings
            existingManager.SetMaxDistance(maxPointerDistance);
            existingManager.SetMinDistance(minPointerDistance);
            existingManager.SetShowOnlyOffscreen(showOnlyOffscreenEnemies);
            existingManager.SetUpdateFrequency(updateFrequency);
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
    }
    
    /// <summary>
    /// Log setup instructions for the user
    /// </summary>
    private void LogSetupInstructions()
    {
    }
    
    /// <summary>
    /// Validate the current setup
    /// </summary>
    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        
        bool allGood = true;
        
        // Check camera
        IsoCameraFollow isoCamera = FindObjectOfType<IsoCameraFollow>();
        Camera mainCamera = Camera.main;
        
        if (isoCamera != null)
        {
        }
        else
        {
            allGood = false;
        }
        
        if (mainCamera != null)
        {
        }
        else
        {
            allGood = false;
        }
        
        // Check pointer manager
        EnemyPointerManager pointerManager = FindObjectOfType<EnemyPointerManager>();
        if (pointerManager != null)
        {
        }
        else
        {
            allGood = false;
        }
        
        // Check settings UI
        EnemyPointerSettingsUI settingsUI = FindObjectOfType<EnemyPointerSettingsUI>();
        if (settingsUI != null)
        {
        }
        else
        {
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
        
    }
    
    /// <summary>
    /// Clean up and remove all enemy pointer system components
    /// </summary>
    [ContextMenu("Remove Enemy Pointer System")]
    public void RemoveEnemyPointerSystem()
    {
        
        int removedCount = 0;
        
        // Remove pointer manager
        EnemyPointerManager pointerManager = FindObjectOfType<EnemyPointerManager>();
        if (pointerManager != null)
        {
            DestroyImmediate(pointerManager.gameObject);
            removedCount++;
        }
        
        // Remove settings UI
        EnemyPointerSettingsUI settingsUI = FindObjectOfType<EnemyPointerSettingsUI>();
        if (settingsUI != null)
        {
            DestroyImmediate(settingsUI.gameObject);
            removedCount++;
        }
    }
}