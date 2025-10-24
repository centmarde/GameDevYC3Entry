using UnityEngine;
using TMPro;

/// <summary>
/// Displays floating damage numbers when entities take damage
/// Automatically handles regular damage and critical hits with different styling
/// </summary>
public class DamageNumberUI : MonoBehaviour
{
    [Header("Normal Damage Appearance")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private int normalFontSize = 24;
    
    [Header("Critical Damage Appearance")]
    [SerializeField] private Color criticalColor = new Color(1f, 0.3f, 0f, 1f); // Orange-red
    [SerializeField] private int criticalFontSize = 32;
    [SerializeField] private string criticalPrefix = "CRITICAL! ";
    
    [Header("Animation Settings")]
    [SerializeField] private float lifetime = 1.2f;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float fadeDelay = 0.3f;
    [SerializeField] private float randomSpreadX = 0.3f;
    [SerializeField] private float initialYOffset = 0.5f;
    
    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;
    private float timer = 0f;
    private Vector3 startPosition;
    private Vector3 velocity;
    
    private static Canvas worldCanvas;
    
    private void Awake()
    {
        // Setup components
        textMesh = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (textMesh == null)
        {
            textMesh = gameObject.AddComponent<TextMeshProUGUI>();
        }
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;
        
        // Add slight outline for better visibility
        textMesh.outlineWidth = 0.2f;
        textMesh.outlineColor = new Color(0, 0, 0, 0.8f);
    }
    
    private void Update()
    {
        timer += Time.deltaTime;
        
        // Float upward with slight random movement
        transform.position = startPosition + velocity * timer;
        
        // Fade out after delay
        if (timer > fadeDelay)
        {
            float fadeProgress = (timer - fadeDelay) / (lifetime - fadeDelay);
            canvasGroup.alpha = 1f - fadeProgress;
        }
        
        // Destroy after lifetime
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initialize the damage number with damage value and critical status
    /// </summary>
    public void Initialize(float damage, bool isCritical, Vector3 worldPosition)
    {
        // Setup text content
        string damageText = Mathf.CeilToInt(damage).ToString();
        if (isCritical)
        {
            textMesh.text = criticalPrefix + damageText;
            textMesh.color = criticalColor;
            textMesh.fontSize = criticalFontSize;
        }
        else
        {
            textMesh.text = damageText;
            textMesh.color = normalColor;
            textMesh.fontSize = normalFontSize;
        }
        
        // Convert world position to screen position
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("No main camera found for damage numbers!");
            Destroy(gameObject);
            return;
        }
        
        // Add offset to world position
        Vector3 offsetWorldPos = worldPosition + Vector3.up * initialYOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(offsetWorldPos);
        
        // Add random horizontal spread in screen space
        float randomX = Random.Range(-randomSpreadX * 50f, randomSpreadX * 50f);
        screenPos.x += randomX;
        
        startPosition = screenPos;
        transform.position = startPosition;
        
        // Setup velocity for floating animation (in screen space)
        velocity = Vector3.up * floatSpeed * 30f; // Multiply for screen space
    }
    
    /// <summary>
    /// Initialize as a heal number (green color, + prefix)
    /// </summary>
    public void InitializeHeal(float healAmount, Vector3 worldPosition)
    {
        // Setup text content with + prefix and green color
        string healText = "+" + Mathf.CeilToInt(healAmount).ToString();
        textMesh.text = healText;
        textMesh.color = new Color(0.2f, 1f, 0.2f, 1f); // Bright green
        textMesh.fontSize = normalFontSize;
        
        // Convert world position to screen position
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("No main camera found for heal numbers!");
            Destroy(gameObject);
            return;
        }
        
        // Add offset to world position
        Vector3 offsetWorldPos = worldPosition + Vector3.up * initialYOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(offsetWorldPos);
        
        // Add random horizontal spread in screen space
        float randomX = Random.Range(-randomSpreadX * 50f, randomSpreadX * 50f);
        screenPos.x += randomX;
        
        startPosition = screenPos;
        transform.position = startPosition;
        
        // Setup velocity for floating animation (in screen space)
        velocity = Vector3.up * floatSpeed * 30f; // Multiply for screen space
    }
    
    /// <summary>
    /// Show a damage number at the specified world position
    /// </summary>
    public static void ShowDamage(float damage, Vector3 worldPosition, bool isCritical = false)
    {
        // Ensure we have a world canvas
        if (worldCanvas == null)
        {
            CreateWorldCanvas();
        }
        
        // Create damage number object
        GameObject damageObj = new GameObject("DamageNumber");
        damageObj.transform.SetParent(worldCanvas.transform, false);
        
        // Add RectTransform for UI element
        RectTransform rectTransform = damageObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 60);
        
        // Add and initialize component
        DamageNumberUI damageNumber = damageObj.AddComponent<DamageNumberUI>();
        damageNumber.Initialize(damage, isCritical, worldPosition);
    }
    
    /// <summary>
    /// Show a heal number at the specified world position (green text)
    /// </summary>
    public static void ShowHeal(float healAmount, Vector3 worldPosition)
    {
        // Ensure we have a world canvas
        if (worldCanvas == null)
        {
            CreateWorldCanvas();
        }
        
        // Create heal number object
        GameObject healObj = new GameObject("HealNumber");
        healObj.transform.SetParent(worldCanvas.transform, false);
        
        // Add RectTransform for UI element
        RectTransform rectTransform = healObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 60);
        
        // Add and initialize component with custom color
        DamageNumberUI healNumber = healObj.AddComponent<DamageNumberUI>();
        healNumber.InitializeHeal(healAmount, worldPosition);
    }
    
    /// <summary>
    /// Create a screen-space canvas for damage numbers if it doesn't exist
    /// </summary>
    private static void CreateWorldCanvas()
    {
        // Look for existing damage number canvas
        GameObject existingCanvas = GameObject.Find("DamageNumberCanvas");
        if (existingCanvas != null)
        {
            worldCanvas = existingCanvas.GetComponent<Canvas>();
            if (worldCanvas != null)
            {
                Debug.Log("Using existing DamageNumberCanvas");
                return;
            }
        }
        
        // Create new screen-space overlay canvas
        GameObject canvasObj = new GameObject("DamageNumberCanvas");
        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Add CanvasScaler for consistent sizing
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster (required for canvas)
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Ensure it's always visible
        worldCanvas.sortingOrder = 1000;
        
        // Make it persistent (optional)
        Object.DontDestroyOnLoad(canvasObj);
        
        Debug.Log("✓ DamageNumberCanvas created successfully! Damage numbers will now display.");
    }
    
    /// <summary>
    /// Set custom colors for damage numbers
    /// </summary>
    public void SetColors(Color normal, Color critical)
    {
        normalColor = normal;
        criticalColor = critical;
    }
    
    /// <summary>
    /// Set custom font sizes
    /// </summary>
    public void SetFontSizes(int normal, int critical)
    {
        normalFontSize = normal;
        criticalFontSize = critical;
    }
}
