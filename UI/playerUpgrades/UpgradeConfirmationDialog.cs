using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Confirmation dialog that appears when player selects an upgrade
/// Allows player to confirm or cancel their choice
/// </summary>
public class UpgradeConfirmationDialog : MonoBehaviour
{
    [Header("Background Configuration")]
    [SerializeField] private Sprite backgroundSprite;
    [Tooltip("Optional background image for the dialog. If not set, uses solid color.")]
    
    [Header("Button Sprites")]
    [SerializeField] private Sprite confirmButtonSprite;
    [Tooltip("Optional sprite for the Confirm button. If not set, uses solid color.")]
    [SerializeField] private Sprite cancelButtonSprite;
    [Tooltip("Optional sprite for the Cancel button. If not set, uses solid color.")]
    
    private GameObject dialogPanel;
    private GameObject darkenBackground;
    private TextMeshProUGUI messageText;
    private Button confirmButton;
    private Button cancelButton;
    private CanvasGroup canvasGroup;
    private Image dialogPanelImage;
    
    public event Action OnConfirm;
    public event Action OnCancel;
    
    [SerializeField] private float fadeSpeed = 10f;
    private bool isVisible = false;
    
    /// <summary>
    /// Create the confirmation dialog UI
    /// </summary>
    public void Create(Transform parent, Color panelColor, Color buttonColor, Color buttonHighlight, Color textColor)
    {
        // Create darkened background
        CreateDarkenedBackground(parent);
        
        // Create dialog panel
        dialogPanel = new GameObject("ConfirmationDialogPanel");
        dialogPanel.transform.SetParent(parent, false);
        
        RectTransform panelRect = dialogPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(500, 250);
        
        dialogPanelImage = dialogPanel.AddComponent<Image>();
        
        // Apply background sprite if available, otherwise use solid color
        if (backgroundSprite != null)
        {
            dialogPanelImage.sprite = backgroundSprite;
            dialogPanelImage.type = Image.Type.Sliced; // Use sliced for better scaling
            dialogPanelImage.color = Color.white; // Keep full color for sprite
        }
        else
        {
            dialogPanelImage.color = new Color(panelColor.r, panelColor.g, panelColor.b, 0.98f);
        }
        
        // Add outline for better visibility
        Outline outline = dialogPanel.AddComponent<Outline>();
        outline.effectColor = new Color(0.4f, 0.7f, 1f, 0.8f);
        outline.effectDistance = new Vector2(3, -3);
        
        canvasGroup = dialogPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        
        // Ensure dialog is rendered in front of background
        darkenBackground.transform.SetAsFirstSibling();
        dialogPanel.transform.SetAsLastSibling();
        
        // Create message text
        CreateMessageText(dialogPanel.transform, textColor);
        
        // Create buttons
        CreateConfirmButton(dialogPanel.transform, buttonColor, buttonHighlight, textColor);
        CreateCancelButton(dialogPanel.transform, buttonColor, buttonHighlight, textColor);
        
        dialogPanel.SetActive(false);
        darkenBackground.SetActive(false);
    }
    
    /// <summary>
    /// Set the background sprite at runtime
    /// </summary>
    public void SetBackgroundSprite(Sprite sprite)
    {
        backgroundSprite = sprite;
        
        if (dialogPanelImage != null && sprite != null)
        {
            dialogPanelImage.sprite = sprite;
            dialogPanelImage.type = Image.Type.Sliced;
            dialogPanelImage.color = Color.white;
        }
    }
    
    /// <summary>
    /// Set the confirm button sprite at runtime
    /// </summary>
    public void SetConfirmButtonSprite(Sprite sprite)
    {
        confirmButtonSprite = sprite;
        
        if (confirmButton != null && sprite != null)
        {
            Image buttonImage = confirmButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = sprite;
                buttonImage.type = Image.Type.Sliced;
                buttonImage.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// Set the cancel button sprite at runtime
    /// </summary>
    public void SetCancelButtonSprite(Sprite sprite)
    {
        cancelButtonSprite = sprite;
        
        if (cancelButton != null && sprite != null)
        {
            Image buttonImage = cancelButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = sprite;
                buttonImage.type = Image.Type.Sliced;
                buttonImage.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// Create darkened background overlay
    /// </summary>
    private void CreateDarkenedBackground(Transform parent)
    {
        darkenBackground = new GameObject("DialogDarkenBackground");
        darkenBackground.transform.SetParent(parent, false);
        
        RectTransform bgRect = darkenBackground.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        // Semi-transparent black overlay
        Image bgImage = darkenBackground.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.7f);
        
        // Make it block raycasts to prevent clicking through
        bgImage.raycastTarget = true;
    }
    
    /// <summary>
    /// Create message text
    /// </summary>
    private void CreateMessageText(Transform parent, Color textColor)
    {
        GameObject textObj = new GameObject("MessageText");
        textObj.transform.SetParent(parent, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(0, 30);
        textRect.sizeDelta = new Vector2(450, 120);
        
        messageText = textObj.AddComponent<TextMeshProUGUI>();
        messageText.fontSize = 28;
        messageText.color = textColor;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.verticalAlignment = VerticalAlignmentOptions.Middle;
        messageText.enableWordWrapping = true;
        messageText.text = "Confirm this upgrade?";
    }
    
    /// <summary>
    /// Create confirm button
    /// </summary>
    private void CreateConfirmButton(Transform parent, Color buttonColor, Color buttonHighlight, Color textColor)
    {
        GameObject buttonObj = new GameObject("ConfirmButton");
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(-130, 25);
        buttonRect.sizeDelta = new Vector2(200, 60);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        
        // Apply sprite if available, otherwise use solid color
        if (confirmButtonSprite != null)
        {
            buttonImage.sprite = confirmButtonSprite;
            buttonImage.type = Image.Type.Sliced;
            buttonImage.color = Color.white;
        }
        else
        {
            buttonImage.color = new Color(0.2f, 0.7f, 0.3f, 1f); // Green tint
        }
        
        confirmButton = buttonObj.AddComponent<Button>();
        confirmButton.targetGraphic = buttonImage;
        
        ColorBlock colors = confirmButton.colors;
        colors.normalColor = new Color(0.2f, 0.7f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.9f, 0.4f, 1f);
        colors.pressedColor = new Color(0.1f, 0.5f, 0.2f, 1f);
        confirmButton.colors = colors;
        
        confirmButton.onClick.AddListener(HandleConfirm);
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "CONFIRM";
        text.fontSize = 24;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
    }
    
    /// <summary>
    /// Create cancel button
    /// </summary>
    private void CreateCancelButton(Transform parent, Color buttonColor, Color buttonHighlight, Color textColor)
    {
        GameObject buttonObj = new GameObject("CancelButton");
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(130, 25);
        buttonRect.sizeDelta = new Vector2(200, 60);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        
        // Apply sprite if available, otherwise use solid color
        if (cancelButtonSprite != null)
        {
            buttonImage.sprite = cancelButtonSprite;
            buttonImage.type = Image.Type.Sliced;
            buttonImage.color = Color.white;
        }
        else
        {
            buttonImage.color = new Color(0.7f, 0.2f, 0.2f, 1f); // Red tint
        }
        
        cancelButton = buttonObj.AddComponent<Button>();
        cancelButton.targetGraphic = buttonImage;
        
        ColorBlock colors = cancelButton.colors;
        colors.normalColor = new Color(0.7f, 0.2f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.9f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.5f, 0.1f, 0.1f, 1f);
        cancelButton.colors = colors;
        
        cancelButton.onClick.AddListener(HandleCancel);
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "CANCEL";
        text.fontSize = 24;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
    }
    
    /// <summary>
    /// Show the confirmation dialog
    /// </summary>
    public void Show(string message)
    {
        if (dialogPanel == null || messageText == null)
        {
            Debug.LogError("UpgradeConfirmationDialog: Dialog panel or message text is null!");
            return;
        }
        
        messageText.text = message;
        darkenBackground.SetActive(true);
        dialogPanel.SetActive(true);
        
        // Ensure proper ordering when shown
        darkenBackground.transform.SetAsLastSibling();
        dialogPanel.transform.SetAsLastSibling();
        
        isVisible = true;
        canvasGroup.alpha = 0f; // Start fade from 0
    }
    
    /// <summary>
    /// Hide the confirmation dialog
    /// </summary>
    public void Hide()
    {
        if (dialogPanel == null) return;
        
        isVisible = false;
        dialogPanel.SetActive(false);
        darkenBackground.SetActive(false);
    }
    
    /// <summary>
    /// Handle confirm button click
    /// </summary>
    private void HandleConfirm()
    {
        Hide();
        OnConfirm?.Invoke();
    }
    
    /// <summary>
    /// Handle cancel button click
    /// </summary>
    private void HandleCancel()
    {
        Hide();
        OnCancel?.Invoke();
    }
    
    /// <summary>
    /// Update fade animation
    /// </summary>
    private void Update()
    {
        if (canvasGroup == null) return;
        
        float targetAlpha = isVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
    }
}
