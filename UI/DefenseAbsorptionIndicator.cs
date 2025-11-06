using UnityEngine;
using TMPro;

/// <summary>
/// Visual indicator for defense absorption - shows when damage is absorbed by defense
/// Similar to CriticalHitIndicator but for defense mechanics
/// </summary>
public class DefenseAbsorptionIndicator : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private Color absorptionColor = new Color(0.2f, 0.8f, 1f, 1f); // Cyan blue
    [SerializeField] private int fontSize = 32;
    [SerializeField] private string prefix = "ABSORBED ";
    
    [Header("Animation")]
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float fadeDelay = 0.8f;
    [SerializeField] private float scaleAnimation = 1.2f;
    
    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;
    private float timer = 0f;
    private Vector3 startPosition;
    private Vector3 initialScale;
    
    private static Canvas worldCanvas;
    private static float lastShowTime = -999f;
    private static float showInterval = 0.5f; // Minimum time between indicators
    
    private void Awake()
    {
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
        
        // Setup text appearance
        textMesh.color = absorptionColor;
        textMesh.fontSize = fontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;
        
        startPosition = transform.position;
        initialScale = transform.localScale;
    }
    
    private void Update()
    {
        timer += Time.deltaTime;
        
        // Float upward and slightly to the side
        Vector3 sideMovement = Vector3.right * Mathf.Sin(timer * 2f) * 0.3f;
        transform.position = startPosition + Vector3.up * (floatSpeed * timer) + sideMovement;
        
        // Scale animation - grows then shrinks
        float scaleProgress = timer / (lifetime * 0.3f);
        if (scaleProgress <= 1f)
        {
            float scale = Mathf.Lerp(1f, scaleAnimation, scaleProgress);
            transform.localScale = initialScale * scale;
        }
        else
        {
            float shrinkProgress = (timer - lifetime * 0.3f) / (lifetime * 0.7f);
            float scale = Mathf.Lerp(scaleAnimation, 0.8f, shrinkProgress);
            transform.localScale = initialScale * scale;
        }
        
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
    /// Show a defense absorption indicator at the specified world position
    /// </summary>
    /// <param name="worldPosition">World position where indicator should appear</param>
    /// <param name="absorbedDamage">Amount of damage absorbed</param>
    public static void ShowAbsorption(Vector3 worldPosition, float absorbedDamage)
    {
        try
        {
            // Check show interval to prevent spam
            if (Time.time - lastShowTime < showInterval)
            {
                return;
            }
            lastShowTime = Time.time;
            
            Debug.Log($"[DefenseAbsorptionIndicator] ShowAbsorption called with {absorbedDamage} damage at {worldPosition}");
            
            // Don't show indicator for very small absorption amounts
            if (absorbedDamage < 0.1f)
            {
                Debug.Log($"[DefenseAbsorptionIndicator] Skipping indicator - damage too small: {absorbedDamage}");
                return;
            }
            
            // Ensure we have a world canvas
            if (worldCanvas == null)
            {
                Debug.Log("[DefenseAbsorptionIndicator] Creating world canvas...");
                CreateWorldCanvas();
            }
            
            if (worldCanvas == null)
            {
                Debug.LogError("[DefenseAbsorptionIndicator] Failed to create or find world canvas!");
                return;
            }
        
        // Create indicator
        GameObject indicatorObj = new GameObject("DefenseAbsorptionIndicator");
        indicatorObj.transform.SetParent(worldCanvas.transform, false);
        
        // Position in world space (slightly offset from hit point)
        indicatorObj.transform.position = worldPosition + Vector3.up * 0.8f + Vector3.right * 0.3f;
        
        // Add component
        DefenseAbsorptionIndicator indicator = indicatorObj.AddComponent<DefenseAbsorptionIndicator>();
        
        // Set the text based on absorbed damage
        string displayText;
        if (absorbedDamage >= 999f)
        {
            displayText = indicator.prefix + "999+";
        }
        else if (absorbedDamage >= 10f)
        {
            displayText = indicator.prefix + Mathf.RoundToInt(absorbedDamage).ToString();
        }
        else
        {
            displayText = indicator.prefix + absorbedDamage.ToString("F1");
        }
        
        indicator.textMesh.text = displayText;
        
        // Add RectTransform for UI element
        RectTransform rectTransform = indicatorObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = indicatorObj.AddComponent<RectTransform>();
        }
        rectTransform.sizeDelta = new Vector2(250, 60);
        
        Debug.Log($"[DefenseAbsorptionIndicator] Successfully created absorption indicator: {displayText} at {worldPosition}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DefenseAbsorptionIndicator] Error creating absorption indicator: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
    }
    
    /// <summary>
    /// Show absorption indicator with custom text (for complete absorption)
    /// </summary>
    /// <param name="worldPosition">World position where indicator should appear</param>
    /// <param name="customText">Custom text to display</param>
    public static void ShowFullAbsorption(Vector3 worldPosition, string customText = "ABSORBED!")
    {
        try
        {
            // Check show interval to prevent spam
            if (Time.time - lastShowTime < showInterval)
            {
                return;
            }
            lastShowTime = Time.time;
            
            Debug.Log($"[DefenseAbsorptionIndicator] ShowFullAbsorption called with text: {customText} at {worldPosition}");
            
            // Ensure we have a world canvas
            if (worldCanvas == null)
            {
                Debug.Log("[DefenseAbsorptionIndicator] Creating world canvas for full absorption...");
                CreateWorldCanvas();
            }
            
            if (worldCanvas == null)
            {
                Debug.LogError("[DefenseAbsorptionIndicator] Failed to create or find world canvas for full absorption!");
                return;
            }
        
        // Create indicator
        GameObject indicatorObj = new GameObject("DefenseAbsorptionIndicator_Full");
        indicatorObj.transform.SetParent(worldCanvas.transform, false);
        
        // Position in world space
        indicatorObj.transform.position = worldPosition + Vector3.up * 0.8f + Vector3.right * 0.3f;
        
        // Add component
        DefenseAbsorptionIndicator indicator = indicatorObj.AddComponent<DefenseAbsorptionIndicator>();
        
        // Customize for full absorption
        indicator.absorptionColor = new Color(0f, 1f, 0.3f, 1f); // Bright green for full block
        indicator.fontSize = 36;
        indicator.scaleAnimation = 1.5f;
        indicator.lifetime = 2.5f;
        
        indicator.textMesh.text = customText;
        indicator.textMesh.color = indicator.absorptionColor;
        indicator.textMesh.fontSize = indicator.fontSize;
        
        // Add RectTransform for UI element
        RectTransform rectTransform = indicatorObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = indicatorObj.AddComponent<RectTransform>();
        }
        rectTransform.sizeDelta = new Vector2(300, 70);
        
        Debug.Log($"[DefenseAbsorptionIndicator] Successfully created full absorption indicator: {customText} at {worldPosition}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DefenseAbsorptionIndicator] Error creating full absorption indicator: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
    }
    
    /// <summary>
    /// Create a world-space canvas for indicators (shared with other indicators)
    /// </summary>
    private static void CreateWorldCanvas()
    {
        try
        {
            // Always create a dedicated canvas for defense indicators to avoid conflicts
            GameObject canvasObj = new GameObject("DefenseAbsorptionCanvas");
            worldCanvas = canvasObj.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            
            // Add CanvasScaler for better scaling
            var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;
            
            RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(100, 100);
            canvasRect.localScale = Vector3.one * 0.01f; // Scale down for world space
            
            // Ensure it's always visible
            worldCanvas.sortingOrder = 101; // Higher than critical hit indicator
            
            Debug.Log("[DefenseAbsorptionIndicator] Created dedicated world canvas for defense indicators");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DefenseAbsorptionIndicator] Failed to create world canvas: {ex.Message}");
        }
    }
}