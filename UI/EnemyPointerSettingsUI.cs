using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Auto-built settings UI for the Enemy Pointer system.
/// Allows runtime configuration of pointer behavior.
/// </summary>
public class EnemyPointerSettingsUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private bool autoCreateUI = true;
    [SerializeField] private Key toggleKey = Key.P;
    [SerializeField] private string settingsPanelName = "EnemyPointerSettings";
    
    [Header("Panel Appearance")]
    [SerializeField] private Vector2 panelSize = new Vector2(300, 400);
    [SerializeField] private Vector2 panelPosition = new Vector2(50, 50);
    [SerializeField] private Color panelBackgroundColor = new Color(0, 0, 0, 0.8f);
    
    private Canvas settingsCanvas;
    private GameObject settingsPanel;
    private EnemyPointerManager pointerManager;
    
    private Slider maxDistanceSlider;
    private Slider minDistanceSlider;
    private Toggle showOffscreenOnlyToggle;
    private Slider updateFrequencySlider;
    private Button refreshButton;
    private Text statusText;
    
    private bool isUIVisible = false;
    
    private void Awake()
    {
        pointerManager = FindObjectOfType<EnemyPointerManager>();
        
        if (pointerManager == null)
        {
            return;
        }
        
        if (autoCreateUI)
        {
            CreateSettingsUI();
        }
    }
    
    private void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            ToggleUI();
        }
        
        if (statusText != null && pointerManager != null && isUIVisible)
        {
            UpdateStatusText();
        }
    }
    
    private void CreateSettingsUI()
    {
        settingsCanvas = FindOrCreateCanvas();
        CreateMainPanel();
        CreateUIControls();
    }
    
    private Canvas FindOrCreateCanvas()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("SettingsCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        return canvas;
    }
    
    private void CreateMainPanel()
    {
        settingsPanel = new GameObject(settingsPanelName);
        settingsPanel.transform.SetParent(settingsCanvas.transform, false);
        
        RectTransform panelRect = settingsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = panelPosition;
        panelRect.sizeDelta = panelSize;
        
        Image panelImage = settingsPanel.AddComponent<Image>();
        panelImage.color = panelBackgroundColor;
        
        CreateTitle();
    }
    
    private void CreateTitle()
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(settingsPanel.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(0, 30);
        
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Enemy Pointer Settings";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 16;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
    }
    
    private void CreateUIControls()
    {
        float yOffset = -50;
        float spacing = 40;
        
        maxDistanceSlider = CreateSlider("Max Distance", 10f, 200f, 100f, yOffset);
        yOffset -= spacing;
        
        minDistanceSlider = CreateSlider("Min Distance", 0f, 50f, 10f, yOffset);
        yOffset -= spacing;
        
        updateFrequencySlider = CreateSlider("Update Freq", 0.01f, 1f, 0.1f, yOffset);
        yOffset -= spacing;
        
        showOffscreenOnlyToggle = CreateToggle("Offscreen Only", true, yOffset);
        yOffset -= spacing;
        
        refreshButton = CreateButton("Force Refresh", yOffset, OnRefreshClicked);
        yOffset -= spacing;
        
        statusText = CreateStatusText(yOffset);
        
        SetupEventListeners();
    }
    
    private Slider CreateSlider(string label, float minValue, float maxValue, float defaultValue, float yPos)
    {
        GameObject container = new GameObject($"{label}Container");
        container.transform.SetParent(settingsPanel.transform, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(10, yPos);
        containerRect.sizeDelta = new Vector2(-20, 30);
        
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);
        
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.5f, 1);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 12;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;
        
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(container.transform, false);
        
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0);
        sliderRect.anchorMax = new Vector2(1, 1);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = defaultValue;
        
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform, false);
        
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(sliderObj.transform, false);
        
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 20);
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        
        slider.targetGraphic = handleImage;
        slider.handleRect = handleRect;
        
        return slider;
    }
    
    private Toggle CreateToggle(string label, bool defaultValue, float yPos)
    {
        GameObject container = new GameObject($"{label}Container");
        container.transform.SetParent(settingsPanel.transform, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(10, yPos);
        containerRect.sizeDelta = new Vector2(-20, 30);
        
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(container.transform, false);
        
        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(0, 0);
        toggleRect.anchorMax = new Vector2(0, 1);
        toggleRect.sizeDelta = new Vector2(20, 0);
        
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = defaultValue;
        
        GameObject background = new GameObject("Background");
        background.transform.SetParent(toggleObj.transform, false);
        
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(toggleObj.transform, false);
        
        RectTransform checkRect = checkmark.AddComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.sizeDelta = Vector2.zero;
        
        Image checkImage = checkmark.AddComponent<Image>();
        checkImage.color = Color.green;
        
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);
        
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(30, 0);
        labelRect.offsetMax = Vector2.zero;
        
        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 12;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;
        
        return toggle;
    }
    
    private Button CreateButton(string label, float yPos, System.Action onClick)
    {
        GameObject buttonObj = new GameObject($"{label}Button");
        buttonObj.transform.SetParent(settingsPanel.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 1);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.pivot = new Vector2(0.5f, 1);
        buttonRect.anchoredPosition = new Vector2(0, yPos);
        buttonRect.sizeDelta = new Vector2(-20, 30);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.4f, 0.8f, 0.8f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(() => onClick?.Invoke());
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = label;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 14;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        
        return button;
    }
    
    private Text CreateStatusText(float yPos)
    {
        GameObject textObj = new GameObject("StatusText");
        textObj.transform.SetParent(settingsPanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 1);
        textRect.anchoredPosition = new Vector2(0, yPos);
        textRect.sizeDelta = new Vector2(-20, 60);
        
        Text text = textObj.AddComponent<Text>();
        text.text = "Status: Ready";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 11;
        text.alignment = TextAnchor.UpperLeft;
        text.color = Color.yellow;
        
        return text;
    }
    
    private void SetupEventListeners()
    {
        if (pointerManager == null) return;
        
        if (maxDistanceSlider != null)
        {
            maxDistanceSlider.onValueChanged.AddListener(value => pointerManager.SetMaxDistance(value));
        }
        
        if (minDistanceSlider != null)
        {
            minDistanceSlider.onValueChanged.AddListener(value => pointerManager.SetMinDistance(value));
        }
        
        if (updateFrequencySlider != null)
        {
            updateFrequencySlider.onValueChanged.AddListener(value => pointerManager.SetUpdateFrequency(value));
        }
        
        if (showOffscreenOnlyToggle != null)
        {
            showOffscreenOnlyToggle.onValueChanged.AddListener(value => pointerManager.SetShowOnlyOffscreen(value));
        }
    }
    
    private void OnRefreshClicked()
    {
        if (pointerManager != null)
        {
            pointerManager.ForceRefresh();
        }
    }
    
    private void UpdateStatusText()
    {
        if (statusText == null || pointerManager == null) return;
        
        int trackedCount = pointerManager.GetTrackedEnemyCount();
        int activeCount = pointerManager.GetActivePointerCount();
        
        statusText.text = $"Status: Active\n" +
                         $"Tracked Enemies: {trackedCount}\n" +
                         $"Active Pointers: {activeCount}";
    }
    
    public void ToggleUI()
    {
        if (settingsPanel == null) return;
        
        isUIVisible = !isUIVisible;
        settingsPanel.SetActive(isUIVisible);
    }
    
    public void ShowUI()
    {
        if (settingsPanel != null)
        {
            isUIVisible = true;
            settingsPanel.SetActive(true);
        }
    }
    
    public void HideUI()
    {
        if (settingsPanel != null)
        {
            isUIVisible = false;
            settingsPanel.SetActive(false);
        }
    }
}
