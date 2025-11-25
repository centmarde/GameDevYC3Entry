using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GroqLLMUISetup : MonoBehaviour
{
    [Header("Click the button below to generate UI")]
    [SerializeField] private bool setupComplete = false;
    
    [ContextMenu("Setup UI Automatically")]
    public void SetupUI()
    {
        // Create Canvas if needed
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("GroqLLM_Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            transform.SetParent(canvasObj.transform);
        }
        
        // Ensure EventSystem exists
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("EventSystem created for UI interaction");
        }
        
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        
        // Set panel size
        rectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        rectTransform.anchorMax = new Vector2(0.9f, 0.9f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Add background
        Image bgImage = GetComponent<Image>();
        if (bgImage == null)
        {
            bgImage = gameObject.AddComponent<Image>();
        }
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        // Create Title
        GameObject titleObj = CreateTextObject("Title", "Groq LLM Chat", 24, TextAlignmentOptions.Center);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.SetParent(transform);
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(-20, 40);
        
        // Create ScrollView for output
        GameObject scrollViewObj = new GameObject("ScrollView");
        scrollViewObj.transform.SetParent(transform);
        RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0.15f);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.pivot = new Vector2(0.5f, 1);
        scrollRect.anchoredPosition = new Vector2(0, -60);
        scrollRect.sizeDelta = new Vector2(-20, -70);
        
        Image scrollBg = scrollViewObj.AddComponent<Image>();
        scrollBg.color = new Color(0.05f, 0.05f, 0.05f, 1f);
        
        ScrollRect scroll = scrollViewObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        
        // Create Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform);
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        
        Image viewportMask = viewportObj.AddComponent<Image>();
        viewportMask.color = Color.clear;
        Mask mask = viewportObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        scroll.viewport = viewportRect;
        
        // Create Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 500);
        
        ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.padding = new RectOffset(10, 10, 10, 10);
        
        scroll.content = contentRect;
        
        // Create Output Text
        GameObject outputTextObj = CreateTextObject("OutputText", "", 16, TextAlignmentOptions.TopLeft);
        outputTextObj.transform.SetParent(contentObj.transform);
        RectTransform outputRect = outputTextObj.GetComponent<RectTransform>();
        outputRect.anchorMin = new Vector2(0, 1);
        outputRect.anchorMax = new Vector2(1, 1);
        outputRect.pivot = new Vector2(0.5f, 1);
        
        TextMeshProUGUI outputText = outputTextObj.GetComponent<TextMeshProUGUI>();
        outputText.enableWordWrapping = true;
        
        LayoutElement outputLayout = outputTextObj.AddComponent<LayoutElement>();
        outputLayout.preferredHeight = -1;
        outputLayout.flexibleHeight = 1;
        
        // Create Input Field
        GameObject inputFieldObj = new GameObject("InputField");
        inputFieldObj.transform.SetParent(transform);
        RectTransform inputRect = inputFieldObj.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0, 0);
        inputRect.anchorMax = new Vector2(0.75f, 0.15f);
        inputRect.pivot = new Vector2(0, 0);
        inputRect.anchoredPosition = new Vector2(10, 10);
        inputRect.sizeDelta = new Vector2(-15, -20);
        
        Image inputBg = inputFieldObj.AddComponent<Image>();
        inputBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        TMP_InputField inputField = inputFieldObj.AddComponent<TMP_InputField>();
        
        // Create Text Area
        GameObject textAreaObj = new GameObject("Text Area");
        textAreaObj.transform.SetParent(inputFieldObj.transform);
        RectTransform textAreaRect = textAreaObj.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.sizeDelta = new Vector2(-20, -10);
        textAreaRect.anchoredPosition = Vector2.zero;
        
        RectMask2D textMask = textAreaObj.AddComponent<RectMask2D>();
        
        // Create Placeholder
        GameObject placeholderObj = CreateTextObject("Placeholder", "Enter your message...", 14, TextAlignmentOptions.Left);
        placeholderObj.transform.SetParent(textAreaObj.transform);
        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;
        placeholderRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI placeholderText = placeholderObj.GetComponent<TextMeshProUGUI>();
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        placeholderText.fontStyle = FontStyles.Italic;
        
        // Create Input Text
        GameObject inputTextObj = CreateTextObject("Text", "", 14, TextAlignmentOptions.Left);
        inputTextObj.transform.SetParent(textAreaObj.transform);
        RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.sizeDelta = Vector2.zero;
        inputTextRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI inputText = inputTextObj.GetComponent<TextMeshProUGUI>();
        inputText.color = Color.white;
        
        inputField.textViewport = textAreaRect;
        inputField.textComponent = inputText;
        inputField.placeholder = placeholderText;
        inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
        
        // Create Send Button
        GameObject buttonObj = CreateButton("SendButton", "Send");
        buttonObj.transform.SetParent(transform);
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.76f, 0);
        buttonRect.anchorMax = new Vector2(1, 0.15f);
        buttonRect.pivot = new Vector2(1, 0);
        buttonRect.anchoredPosition = new Vector2(-10, 10);
        buttonRect.sizeDelta = new Vector2(-5, -20);
        
        // Setup GroqLLMIntegration component
        GroqLLMIntegration groqScript = GetComponent<GroqLLMIntegration>();
        if (groqScript == null)
        {
            groqScript = gameObject.AddComponent<GroqLLMIntegration>();
        }
        
#if UNITY_EDITOR
        // Assign references via reflection since they're private serialized fields
        SerializedObject so = new SerializedObject(groqScript);
        so.FindProperty("inputField").objectReferenceValue = inputField;
        so.FindProperty("outputText").objectReferenceValue = outputText;
        so.FindProperty("sendButton").objectReferenceValue = buttonObj.GetComponent<Button>();
        so.FindProperty("scrollRect").objectReferenceValue = scroll;
        so.ApplyModifiedProperties();
        
        // Mark setup as complete
        SerializedObject setupSO = new SerializedObject(this);
        setupSO.FindProperty("setupComplete").boolValue = true;
        setupSO.ApplyModifiedProperties();
#endif
        
        Debug.Log("Groq LLM UI Setup Complete! EventSystem and Canvas are ready. You can now interact with the UI.");
    }
    
    private GameObject CreateTextObject(string name, string text, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name);
        TextMeshProUGUI textComponent = obj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = Color.white;
        return obj;
    }
    
    private GameObject CreateButton(string name, string buttonText)
    {
        GameObject buttonObj = new GameObject(name);
        Image buttonImg = buttonObj.AddComponent<Image>();
        buttonImg.color = new Color(0.2f, 0.6f, 0.2f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 0.3f, 1f);
        colors.pressedColor = new Color(0.15f, 0.5f, 0.15f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        button.colors = colors;
        
        GameObject textObj = CreateTextObject("Text", buttonText, 16, TextAlignmentOptions.Center);
        textObj.transform.SetParent(buttonObj.transform);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        return buttonObj;
    }
}
