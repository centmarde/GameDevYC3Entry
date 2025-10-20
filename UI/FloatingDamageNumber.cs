using UnityEngine;
using TMPro;

/// <summary>
/// Individual floating damage number instance
/// This script goes on the prefab that shows the actual damage text
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class FloatingDamageNumber : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    
    private float lifetime = 1.2f;
    private float fadeStartTime = 0.3f;
    private float moveSpeed = 50f;
    private float timer = 0f;
    
    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    private void Update()
    {
        timer += Time.deltaTime;
        
        // Move upward
        rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;
        
        // Fade out
        if (timer > fadeStartTime)
        {
            float fadeProgress = (timer - fadeStartTime) / (lifetime - fadeStartTime);
            canvasGroup.alpha = 1f - fadeProgress;
        }
        
        // Destroy when done
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Setup this damage number with damage value and styling
    /// </summary>
    public void Setup(float damage, bool isCritical, Vector3 screenPosition)
    {
        if (isCritical)
        {
            textMesh.text = $"CRIT! {Mathf.CeilToInt(damage)}";
            textMesh.color = new Color(1f, 0.3f, 0f); // Orange
            textMesh.fontSize = 36;
        }
        else
        {
            textMesh.text = Mathf.CeilToInt(damage).ToString();
            textMesh.color = Color.white;
            textMesh.fontSize = 28;
        }
        
        // Set position
        rectTransform.position = screenPosition;
        
        // Add random horizontal offset
        float randomX = Random.Range(-20f, 20f);
        rectTransform.anchoredPosition += Vector2.right * randomX;
    }
}
