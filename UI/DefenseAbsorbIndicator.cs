using UnityEngine;
using TMPro;

/// <summary>
/// Visual indicator for damage absorption by the Defense skill.
/// Displays floating absorbed damage numbers similar to DamageNumberUI.
/// Auto-setup with programmatic creation using screen-space overlay.
/// </summary>
public class DefenseAbsorbIndicator : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private Color absorbColor = new Color(0.2f, 0.8f, 1f, 1f); // Cyan/blue for shields
    [SerializeField] private int fontSize = 26;
    
    [Header("Animation Settings")]
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float fadeDelay = 0.4f;
    [SerializeField] private float randomSpreadX = 0.4f;
    [SerializeField] private float initialYOffset = 0.8f;
    
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
        
        // Add outline for better visibility
        textMesh.outlineWidth = 0.2f;
        textMesh.outlineColor = new Color(0, 0, 0, 0.9f);
    }
    
    private void Update()
    {
        timer += Time.deltaTime;
        
        // Float upward with velocity
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
    /// Initialize the absorption indicator with absorbed damage value
    /// </summary>
    public void Initialize(float absorbedDamage, Vector3 worldPosition)
    {
        // Setup text content without emoji
        textMesh.text = $"-{Mathf.CeilToInt(absorbedDamage)}";
        textMesh.color = absorbColor;
        textMesh.fontSize = fontSize;
        
        // Convert world position to screen position
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("No main camera found for defense absorb indicator!");
            Destroy(gameObject);
            return;
        }
        
        // Add offset to world position (slightly higher than damage numbers)
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
    /// Show an absorption indicator at the specified world position
    /// </summary>
    /// <param name="worldPosition">Position in world space to show the indicator</param>
    /// <param name="absorbedAmount">Amount of damage absorbed</param>
    public static void ShowAbsorption(Vector3 worldPosition, float absorbedAmount)
    {
        // Ensure we have a world canvas
        if (worldCanvas == null)
        {
            CreateWorldCanvas();
        }
        
        // Create absorption indicator object
        GameObject indicatorObj = new GameObject("DefenseAbsorbIndicator");
        indicatorObj.transform.SetParent(worldCanvas.transform, false);
        
        // Add RectTransform for UI element
        RectTransform rectTransform = indicatorObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 60);
        
        // Add and initialize component
        DefenseAbsorbIndicator indicator = indicatorObj.AddComponent<DefenseAbsorbIndicator>();
        indicator.Initialize(absorbedAmount, worldPosition);
    }
    
    /// <summary>
    /// Create a screen-space canvas for absorption indicators if it doesn't exist
    /// </summary>
    private static void CreateWorldCanvas()
    {
        // Look for existing defense absorb canvas
        GameObject existingCanvas = GameObject.Find("DefenseAbsorbCanvas");
        if (existingCanvas != null)
        {
            worldCanvas = existingCanvas.GetComponent<Canvas>();
            if (worldCanvas != null)
            {
                Debug.Log("Using existing DefenseAbsorbCanvas");
                return;
            }
        }
        
        // Create new screen-space overlay canvas
        GameObject canvasObj = new GameObject("DefenseAbsorbCanvas");
        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Add CanvasScaler for consistent sizing
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster (required for canvas)
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Ensure it's visible and above damage numbers
        worldCanvas.sortingOrder = 1005; // Above DamageNumberCanvas (1000)
        
        // Make it persistent (optional)
        Object.DontDestroyOnLoad(canvasObj);
        
        Debug.Log("✓ DefenseAbsorbCanvas created successfully! Defense absorption indicators will now display.");
    }
    
    /// <summary>
    /// Set custom color for absorption indicators
    /// </summary>
    public void SetColor(Color color)
    {
        absorbColor = color;
        if (textMesh != null)
        {
            textMesh.color = color;
        }
    }
}
