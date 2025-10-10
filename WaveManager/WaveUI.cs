using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject announcementPanel;
    [SerializeField] private TextMeshProUGUI waveText;
    
    [Header("Animation Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("Appearance Settings")]
    [SerializeField] private string waveTextFormat = "WAVE {0}";
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 48;
    
    private CanvasGroup canvasGroup;
    private float animationTimer = 0f;
    private bool isAnimating = false;
    
    private void Awake()
    {
        SetupUI();
    }
    
    private void Start()
    {
        // Hide announcement at start
        if (announcementPanel != null)
        {
            announcementPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (isAnimating)
        {
            AnimateAnnouncement();
        }
    }
    
    /// <summary>
    /// Setup UI components programmatically if needed
    /// </summary>
    private void SetupUI()
    {
        // If no panel assigned, create one
        if (announcementPanel == null)
        {
            CreateAnnouncementPanel();
        }
        
        // Get or add CanvasGroup for fade effects
        canvasGroup = announcementPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = announcementPanel.AddComponent<CanvasGroup>();
        }
        
        // Setup text component
        if (waveText != null)
        {
            waveText.color = textColor;
            waveText.fontSize = fontSize;
            waveText.alignment = TextAlignmentOptions.Center;
        }
    }
    
    /// <summary>
    /// Create announcement panel programmatically
    /// </summary>
    private void CreateAnnouncementPanel()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create panel
        announcementPanel = new GameObject("WaveAnnouncementPanel");
        announcementPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = announcementPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0, -50);
        panelRect.sizeDelta = new Vector2(400, 100);
        
        // Add background (optional, semi-transparent)
        Image bgImage = announcementPanel.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);
        
        // Create text object
        GameObject textObj = new GameObject("WaveText");
        textObj.transform.SetParent(announcementPanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        waveText = textObj.AddComponent<TextMeshProUGUI>();
        waveText.color = textColor;
        waveText.fontSize = fontSize;
        waveText.alignment = TextAlignmentOptions.Center;
        waveText.fontStyle = FontStyles.Bold;
    }
    
    /// <summary>
    /// Show wave announcement
    /// </summary>
    public void ShowWaveAnnouncement(int waveNumber)
    {
        if (announcementPanel == null || waveText == null) return;
        
        // Set wave text
        waveText.text = string.Format(waveTextFormat, waveNumber);
        
        // Show panel and start animation
        announcementPanel.SetActive(true);
        canvasGroup.alpha = 0f;
        animationTimer = 0f;
        isAnimating = true;
    }
    
    /// <summary>
    /// Animate the announcement (fade in, hold, fade out)
    /// </summary>
    private void AnimateAnnouncement()
    {
        animationTimer += Time.deltaTime;
        
        // Fade in
        if (animationTimer < fadeInDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, animationTimer / fadeInDuration);
        }
        // Hold
        else if (animationTimer < fadeInDuration + displayDuration)
        {
            canvasGroup.alpha = 1f;
        }
        // Fade out
        else if (animationTimer < fadeInDuration + displayDuration + fadeOutDuration)
        {
            float fadeOutTime = animationTimer - (fadeInDuration + displayDuration);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeOutTime / fadeOutDuration);
        }
        // Hide
        else
        {
            canvasGroup.alpha = 0f;
            announcementPanel.SetActive(false);
            isAnimating = false;
        }
    }
    
    /// <summary>
    /// Manually hide the announcement
    /// </summary>
    public void HideAnnouncement()
    {
        if (announcementPanel != null)
        {
            announcementPanel.SetActive(false);
        }
        isAnimating = false;
    }
}
