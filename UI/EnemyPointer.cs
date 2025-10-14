using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Individual enemy pointer UI element.
/// Handles the visual representation and behavior of a single enemy pointer.
/// </summary>
public class EnemyPointer : MonoBehaviour
{
    [Header("UI Components")]
    public Image pointerImage;
    public Text distanceText;
    public RectTransform rectTransform;
    
    [Header("Animation Settings")]
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.3f;
    
    [Header("Visual Effects")]
    public bool enableFadeBasedOnDistance = true;
    public float maxAlpha = 1f;
    public float minAlpha = 0.3f;
    public float maxFadeDistance = 50f;
    
    private GameObject targetEnemy;
    private float baseScale = 1f;
    private Color originalColor;
    private float pulseTimer;
    
    public GameObject TargetEnemy => targetEnemy;
    
    private void Awake()
    {
        SetupComponents();
    }
    
    private void Start()
    {
        if (pointerImage != null)
        {
            originalColor = pointerImage.color;
        }
        
        if (rectTransform != null)
        {
            baseScale = rectTransform.localScale.x;
        }
    }
    
    private void Update()
    {
        if (targetEnemy == null) return;
        
        UpdateAnimations();
    }
    
    private void SetupComponents()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        if (pointerImage == null)
        {
            pointerImage = GetComponent<Image>();
        }
        
        if (distanceText == null)
        {
            distanceText = GetComponentInChildren<Text>();
        }
        
        if (rectTransform == null)
        {
            Debug.LogError("EnemyPointer: No RectTransform found! This component requires a UI element.");
        }
    }
    
    public void SetTarget(GameObject enemy)
    {
        targetEnemy = enemy;
    }
    
    public void SetPosition(Vector2 position)
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = position;
        }
    }
    
    public void SetColor(Color color)
    {
        if (pointerImage != null)
        {
            originalColor = color;
            pointerImage.color = color;
        }
    }
    
    public void SetRotation(float angle)
    {
        if (rectTransform != null)
        {
            rectTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    public void SetScale(float scale)
    {
        baseScale = scale;
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * scale;
        }
    }
    
    public void SetDistance(float distance)
    {
        if (distanceText != null)
        {
            distanceText.text = $"{distance:F0}m";
            
            if (enableFadeBasedOnDistance && pointerImage != null)
            {
                float fadePercent = Mathf.Clamp01(distance / maxFadeDistance);
                float alpha = Mathf.Lerp(maxAlpha, minAlpha, fadePercent);
                
                Color color = originalColor;
                color.a = alpha;
                pointerImage.color = color;
                
                Color textColor = distanceText.color;
                textColor.a = alpha;
                distanceText.color = textColor;
            }
        }
    }
    
    private void UpdateAnimations()
    {
        if (rectTransform == null) return;
        
        if (enablePulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulseScale = 1f + Mathf.Sin(pulseTimer) * pulseIntensity;
            rectTransform.localScale = Vector3.one * (baseScale * pulseScale);
        }
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }
    
    public bool IsVisible()
    {
        return gameObject.activeInHierarchy;
    }
    
    public bool IsTargetValid()
    {
        return targetEnemy != null && targetEnemy.activeInHierarchy;
    }
    
    private void OnDestroy()
    {
        targetEnemy = null;
    }
}
