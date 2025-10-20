using UnityEngine;
using TMPro;

/// <summary>
/// [DEPRECATED] Old critical hit indicator system
/// Use DamageNumberUI.ShowDamage() instead - it handles both normal and critical hits
/// This file is kept for backward compatibility but should not be used in new code
/// </summary>
[System.Obsolete("Use DamageNumberUI.ShowDamage() instead", false)]
public class CriticalHitIndicator : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private string criticalText = "CRITICAL!";
    [SerializeField] private Color criticalColor = new Color(1f, 0.3f, 0f, 1f); // Orange-red
    [SerializeField] private int fontSize = 36;
    
    [Header("Animation")]
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float fadeDelay = 0.5f;
    
    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;
    private float timer = 0f;
    private Vector3 startPosition;
    
    private static GameObject indicatorPrefab;
    private static Canvas worldCanvas;
    
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
        textMesh.text = criticalText;
        textMesh.color = criticalColor;
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
    /// Show a critical hit indicator at the specified world position
    /// </summary>
    public static void ShowCritical(Vector3 worldPosition)
    {
        // Ensure we have a world canvas
        if (worldCanvas == null)
        {
            CreateWorldCanvas();
        }
        
        // Create indicator
        GameObject indicatorObj = new GameObject("CriticalHitIndicator");
        indicatorObj.transform.SetParent(worldCanvas.transform, false);
        
        // Position in world space
        indicatorObj.transform.position = worldPosition + Vector3.up * 0.5f;
        
        // Add component
        CriticalHitIndicator indicator = indicatorObj.AddComponent<CriticalHitIndicator>();
        
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
        GameObject canvasObj = new GameObject("CriticalHitCanvas");
        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100, 100);
        canvasRect.localScale = Vector3.one * 0.01f; // Scale down for world space
        
        // Ensure it's always visible
        worldCanvas.sortingOrder = 100;
    }
}
