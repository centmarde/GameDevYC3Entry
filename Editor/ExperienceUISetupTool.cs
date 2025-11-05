using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Unity Editor tool to automatically set up the Experience UI system.
/// Access via: Tools > Experience System > Setup Experience UI
/// </summary>
public class ExperienceUISetupTool : EditorWindow
{
    private Color fillColor = new Color(0.3f, 0.8f, 1f); // Cyan
    private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    private int startingLevel = 1;
    private int baseExperienceRequired = 100;
    private float experienceScaling = 1.5f;

    [MenuItem("Tools/Experience System/Setup Experience UI")]
    public static void ShowWindow()
    {
        ExperienceUISetupTool window = GetWindow<ExperienceUISetupTool>("Experience UI Setup");
        window.minSize = new Vector2(400, 400);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Experience System Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool will automatically create:\n" +
            "• ExperienceManager GameObject\n" +
            "• Canvas with proper settings\n" +
            "• ExperienceUI Panel ready for customization\n" +
            "• XP Bar with background and fill\n" +
            "• Level and Experience text labels\n" +
            "• All references properly assigned\n" +
            "• Easy to modify in Inspector after creation",
            MessageType.Info
        );

        EditorGUILayout.Space();

        // UI Customization
        GUILayout.Label("UI Customization", EditorStyles.boldLabel);
        fillColor = EditorGUILayout.ColorField("XP Bar Color", fillColor);
        backgroundColor = EditorGUILayout.ColorField("Background Color", backgroundColor);

        EditorGUILayout.Space();

        // Manager Settings
        GUILayout.Label("Experience Settings", EditorStyles.boldLabel);
        startingLevel = EditorGUILayout.IntField("Starting Level", startingLevel);
        baseExperienceRequired = EditorGUILayout.IntField("Base XP Required", baseExperienceRequired);
        experienceScaling = EditorGUILayout.Slider("XP Scaling", experienceScaling, 1.1f, 3f);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Setup Button
        if (GUILayout.Button("Setup Experience System", GUILayout.Height(40)))
        {
            SetupExperienceSystem();
        }

        EditorGUILayout.Space();

        // Info
        EditorGUILayout.HelpBox(
            "After setup:\n" +
            "1. Check Hierarchy: ExperienceManager, ExperienceCanvas, ExperienceUI_Panel\n" +
            "2. Customize UI: Resize panel, change colors, adjust padding\n" +
            "3. Panel has Image component - easy to add sprites/materials\n" +
            "4. All UI elements are visible and editable in Inspector\n" +
            "5. Create experience orb prefab and assign to enemies",
            MessageType.None
        );
    }

    private void SetupExperienceSystem()
    {
        // Check if already exists
        if (FindObjectOfType<ExperienceManager>() != null)
        {
            if (!EditorUtility.DisplayDialog(
                "Experience System Exists",
                "An ExperienceManager already exists in the scene. Do you want to replace it?",
                "Replace", "Cancel"))
            {
                return;
            }

            DestroyImmediate(FindObjectOfType<ExperienceManager>().gameObject);
        }

        if (FindObjectOfType<ExperienceUI>() != null)
        {
            DestroyImmediate(FindObjectOfType<ExperienceUI>().gameObject);
        }

        // Create Experience Manager
        GameObject managerObj = new GameObject("ExperienceManager");
        ExperienceManager manager = managerObj.AddComponent<ExperienceManager>();
        
        // Use reflection to set private fields
        var managerType = typeof(ExperienceManager);
        var startingLevelField = managerType.GetField("startingLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var baseExpField = managerType.GetField("baseExperienceRequired", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var scalingField = managerType.GetField("experienceScaling", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (startingLevelField != null) startingLevelField.SetValue(manager, startingLevel);
        if (baseExpField != null) baseExpField.SetValue(manager, baseExperienceRequired);
        if (scalingField != null) scalingField.SetValue(manager, experienceScaling);

        Undo.RegisterCreatedObjectUndo(managerObj, "Create Experience Manager");

        // Create or find Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject canvasObj;

        if (canvas == null)
        {
            canvasObj = new GameObject("ExperienceCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }
        else
        {
            canvasObj = canvas.gameObject;
        }

        // Create Experience UI Panel (with Image component for easy styling)
        GameObject uiObj = new GameObject("ExperienceUI_Panel");
        uiObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform panelRect = uiObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(0.5f, 1);
        panelRect.anchoredPosition = new Vector2(0, 0);
        panelRect.sizeDelta = new Vector2(0, 40); // 40px height for the panel
        
        // Add Image component to panel for background (can be customized)
        Image panelImage = uiObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black
        
        // Add ExperienceUI component to the panel
        ExperienceUI expUI = uiObj.AddComponent<ExperienceUI>();
        Undo.RegisterCreatedObjectUndo(uiObj, "Create Experience UI Panel");

        // Create Bar Container inside the panel
        GameObject barContainer = new GameObject("ExperienceBarContainer");
        barContainer.transform.SetParent(uiObj.transform, false);

        RectTransform containerRect = barContainer.AddComponent<RectTransform>();
        // Stretch inside the panel with some padding
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.offsetMin = new Vector2(10, 5); // Left and bottom padding
        containerRect.offsetMax = new Vector2(-10, -5); // Right and top padding

        // Create Background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(barContainer.transform, false);

        RectTransform bgRect = backgroundObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = backgroundObj.AddComponent<Image>();
        bgImage.color = backgroundColor;

        // Create Fill Bar
        GameObject fillObj = new GameObject("FillBar");
        fillObj.transform.SetParent(backgroundObj.transform, false);

        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        // Stretch to fill the background area with padding
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.offsetMin = new Vector2(2, 2); // 2px padding
        fillRect.offsetMax = new Vector2(-2, -2);

        Image fillBar = fillObj.AddComponent<Image>();
        fillBar.color = fillColor;
        fillBar.type = Image.Type.Filled;
        fillBar.fillMethod = Image.FillMethod.Horizontal;
        fillBar.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillBar.fillAmount = 0f; // Start at 0%
        
        Debug.Log("[ExperienceUISetupTool] Created fill bar with fillAmount: 0");

        // Create Level Text
        GameObject levelTextObj = new GameObject("LevelText");
        levelTextObj.transform.SetParent(barContainer.transform, false);

        RectTransform levelRect = levelTextObj.AddComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0, 0.5f);
        levelRect.anchorMax = new Vector2(0, 0.5f);
        levelRect.pivot = new Vector2(0, 0.5f);
        levelRect.anchoredPosition = new Vector2(10, 0);
        levelRect.sizeDelta = new Vector2(100, 30);

        TextMeshProUGUI levelText = levelTextObj.AddComponent<TextMeshProUGUI>();
        levelText.text = $"Level {startingLevel}";
        levelText.fontSize = 18;
        levelText.fontStyle = FontStyles.Bold;
        levelText.color = Color.white;
        levelText.alignment = TextAlignmentOptions.Left;

        // Create Experience Text
        GameObject expTextObj = new GameObject("ExperienceText");
        expTextObj.transform.SetParent(barContainer.transform, false);

        RectTransform expRect = expTextObj.AddComponent<RectTransform>();
        expRect.anchorMin = new Vector2(0.5f, 0.5f);
        expRect.anchorMax = new Vector2(0.5f, 0.5f);
        expRect.pivot = new Vector2(0.5f, 0.5f);
        expRect.anchoredPosition = Vector2.zero;
        expRect.sizeDelta = new Vector2(200, 30);

        TextMeshProUGUI experienceText = expTextObj.AddComponent<TextMeshProUGUI>();
        experienceText.text = $"0 / {baseExperienceRequired}";
        experienceText.fontSize = 16;
        experienceText.color = Color.white;
        experienceText.alignment = TextAlignmentOptions.Center;

        // Assign references to ExperienceUI using reflection
        var uiType = typeof(ExperienceUI);
        var fillBarField = uiType.GetField("fillBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var levelTextField = uiType.GetField("levelText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var expTextField = uiType.GetField("experienceText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fillColorField = uiType.GetField("fillColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (fillBarField != null) fillBarField.SetValue(expUI, fillBar);
        if (levelTextField != null) levelTextField.SetValue(expUI, levelText);
        if (expTextField != null) expTextField.SetValue(expUI, experienceText);
        if (fillColorField != null) fillColorField.SetValue(expUI, fillColor);

        // Verify fill bar is working correctly
        if (fillBar != null)
        {
            Debug.Log($"[ExperienceUISetupTool] Fill bar verification - Type: {fillBar.type}, Method: {fillBar.fillMethod}, Amount: {fillBar.fillAmount}");
        }

        // Mark dirty for save
        EditorUtility.SetDirty(managerObj);
        EditorUtility.SetDirty(uiObj);
        EditorUtility.SetDirty(canvasObj);

        // Select the manager
        Selection.activeGameObject = managerObj;

        EditorUtility.DisplayDialog(
            "Success!",
            "Experience System has been set up successfully!\n\n" +
            "Created:\n" +
            "✓ ExperienceManager (configured)\n" +
            "✓ Canvas with proper scaling\n" +
            "✓ ExperienceUI_Panel (customizable)\n" +
            "✓ XP Bar with fill and text\n\n" +
            "Next steps:\n" +
            "1. Customize the UI in Inspector (colors, size, sprites)\n" +
            "2. Create an experience orb prefab\n" +
            "3. Assign orb to enemy prefabs\n" +
            "4. Test in Play mode!",
            "OK"
        );

        Debug.Log("[ExperienceUISetupTool] Experience System created successfully!");
    }
}
