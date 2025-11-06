using UnityEngine;
using TMPro;

/// <summary>
/// Provides visual feedback when an entity evades an attack
/// Works similar to CriticalHitIndicator for consistent UI integration
/// </summary>
public class EvasionFeedback : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private string evasionText = "EVADED!";
    [SerializeField] private Color evasionColor = new Color(0.2f, 0.8f, 1f); // Cyan color
    [SerializeField] private int fontSize = 36;
    
    [Header("Animation")]
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float fadeDelay = 0.5f;
    
    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;
    private float timer = 0f;
    private Vector3 startPosition;
    
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
        textMesh.text = evasionText;
        textMesh.color = evasionColor;
        textMesh.fontSize = fontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;
        
        startPosition = transform.position;
    }
    
    private void Update()
    {
        timer += Time.deltaTime;
        
        // Float upward
        transform.position = startPosition + Vector3.up * (floatSpeed * timer);
        
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
    /// Show an evasion indicator at the specified world position
    /// </summary>
    public static void ShowEvasion(Vector3 worldPosition)
    {
        // Check show interval to prevent spam
        if (Time.time - lastShowTime < showInterval)
        {
            return;
        }
        lastShowTime = Time.time;
        
        // Ensure we have a world canvas
        if (worldCanvas == null)
        {
            CreateWorldCanvas();
        }
        
        // Create indicator
        GameObject indicatorObj = new GameObject("EvasionIndicator");
        indicatorObj.transform.SetParent(worldCanvas.transform, false);
        
        // Position in world space
        indicatorObj.transform.position = worldPosition + Vector3.up * 0.5f;
        
        // Add component
        EvasionFeedback indicator = indicatorObj.AddComponent<EvasionFeedback>();
        
        // Add RectTransform for UI element
        RectTransform rectTransform = indicatorObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = indicatorObj.AddComponent<RectTransform>();
        }
        rectTransform.sizeDelta = new Vector2(200, 50);
    }
    
    /// <summary>
    /// Create a world-space canvas for indicators
    /// </summary>
    private static void CreateWorldCanvas()
    {
        GameObject canvasObj = new GameObject("EvasionCanvas");
        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100, 100);
        canvasRect.localScale = Vector3.one * 0.01f; // Scale down for world space
        
        // Ensure it's always visible
        worldCanvas.sortingOrder = 100;
    }
}
