using UnityEngine;

/// <summary>
/// Manager script that ensures the damage number system is properly set up
/// Attach this to any GameObject in your scene (like GameManager) to auto-setup
/// </summary>
public class DamageNumberManager : MonoBehaviour
{
    [Header("Damage Number Settings")]
    [SerializeField] private bool autoSetupCanvas = true;
    [SerializeField] private Camera mainCamera;
    
    [Header("Appearance Customization")]
    [SerializeField] private Color normalDamageColor = Color.white;
    [SerializeField] private Color criticalDamageColor = new Color(1f, 0.3f, 0f, 1f);
    [SerializeField] private int normalFontSize = 24;
    [SerializeField] private int criticalFontSize = 32;
    
    [Header("Animation Customization")]
    [SerializeField] private float damageNumberLifetime = 1.2f;
    [SerializeField] private float floatSpeed = 1.5f;
    
    private static DamageNumberManager instance;
    private Canvas worldCanvas;
    
    public static DamageNumberManager Instance => instance;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (autoSetupCanvas)
            {
                SetupCanvas();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Ensure we have a camera reference
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("DamageNumberManager: No main camera found! Damage numbers may not display correctly.");
            }
        }
    }
    
    /// <summary>
    /// Setup the screen-space overlay canvas for damage numbers
    /// </summary>
    private void SetupCanvas()
    {
        // Look for existing damage number canvas
        GameObject existingCanvas = GameObject.Find("DamageNumberCanvas");
        if (existingCanvas != null)
        {
            worldCanvas = existingCanvas.GetComponent<Canvas>();
            if (worldCanvas != null)
            {
                Debug.Log("DamageNumberManager: Using existing DamageNumberCanvas");
                return;
            }
        }
        
        // Create new screen-space overlay canvas
        GameObject canvasObj = new GameObject("DamageNumberCanvas");
        canvasObj.transform.SetParent(transform);
        
        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // Changed to Overlay for proper display
        
        // Add CanvasScaler for consistent sizing
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster (required component)
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // High sorting order to appear above other UI
        worldCanvas.sortingOrder = 1000;
        
        Debug.Log("DamageNumberManager: Screen-space overlay canvas created successfully!");
    }
    
    /// <summary>
    /// Show a damage number at a world position
    /// This is a convenience method that can be called from anywhere
    /// </summary>
    public static void ShowDamage(float damage, Vector3 worldPosition, bool isCritical = false)
    {
        DamageNumberUI.ShowDamage(damage, worldPosition, isCritical);
    }
    
    /// <summary>
    /// Get the main camera used for world-to-screen conversion
    /// </summary>
    public Camera GetMainCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        return mainCamera;
    }
    
    /// <summary>
    /// Get current normal damage color
    /// </summary>
    public Color GetNormalDamageColor() => normalDamageColor;
    
    /// <summary>
    /// Get current critical damage color
    /// </summary>
    public Color GetCriticalDamageColor() => criticalDamageColor;
    
    /// <summary>
    /// Get normal font size
    /// </summary>
    public int GetNormalFontSize() => normalFontSize;
    
    /// <summary>
    /// Get critical font size
    /// </summary>
    public int GetCriticalFontSize() => criticalFontSize;
    
    #region Editor Helpers
    
#if UNITY_EDITOR
    [ContextMenu("Test Normal Damage")]
    private void TestNormalDamage()
    {
        Vector3 testPos = mainCamera != null ? mainCamera.transform.position + mainCamera.transform.forward * 5f : Vector3.zero;
        DamageNumberUI.ShowDamage(Random.Range(10f, 50f), testPos, false);
    }
    
    [ContextMenu("Test Critical Damage")]
    private void TestCriticalDamage()
    {
        Vector3 testPos = mainCamera != null ? mainCamera.transform.position + mainCamera.transform.forward * 5f : Vector3.zero;
        DamageNumberUI.ShowDamage(Random.Range(50f, 150f), testPos, true);
    }
    
    [ContextMenu("Test Multiple Damage Numbers")]
    private void TestMultipleDamageNumbers()
    {
        Vector3 centerPos = mainCamera != null ? mainCamera.transform.position + mainCamera.transform.forward * 5f : Vector3.zero;
        
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-2f, 2f),
                Random.Range(-1f, 1f),
                Random.Range(-2f, 2f)
            );
            
            bool isCrit = Random.value > 0.7f;
            float damage = isCrit ? Random.Range(50f, 150f) : Random.Range(10f, 50f);
            
            DamageNumberUI.ShowDamage(damage, centerPos + randomOffset, isCrit);
        }
    }
#endif
    
    #endregion
}
