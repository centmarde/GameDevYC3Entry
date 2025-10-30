using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

/// <summary>
/// Automated setup wizard for Photon Game Manager
/// Creates all necessary GameObjects, UI elements, and configurations
/// </summary>
public class PhotonGameManagerSetupWizard : EditorWindow
{
    private enum SetupStep
    {
        Welcome,
        CheckPhoton,
        CreateGameManager,
        CreateLobbyUI,
        CreateLeaderboardUI,
        Complete
    }

    private SetupStep currentStep = SetupStep.Welcome;
    private Vector2 scrollPosition;
    private Texture2D backgroundImage;
    private GUIStyle headerStyle;
    private GUIStyle buttonStyle;
    private GUIStyle textStyle;

    // Setup options
    private string lobbyRoomName = "GlobalLobby";
    private int maxPlayers = 100;
    private bool autoConnect = true;
    private bool createConnectionStatus = true;
    private bool createLeaderboardScene = true;
    private string leaderboardSceneName = "LeaderBoards";

    // Created objects
    private GameObject gameManagerObj;
    private GameObject connectionCheckerObj;

    [MenuItem("Tools/Photon Game Manager/Setup Wizard")]
    public static void ShowWindow()
    {
        PhotonGameManagerSetupWizard window = GetWindow<PhotonGameManagerSetupWizard>("Photon Setup Wizard");
        window.minSize = new Vector2(600, 500);
        window.Show();
    }

    private void OnEnable()
    {
        LoadBackgroundImage();
    }

    private void InitializeStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 24;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.padding = new RectOffset(10, 10, 10, 10);
        }

        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.padding = new RectOffset(20, 20, 10, 10);
        }

        if (textStyle == null)
        {
            textStyle = new GUIStyle(EditorStyles.label);
            textStyle.fontSize = 12;
            textStyle.wordWrap = true;
            textStyle.normal.textColor = Color.white;
        }
    }

    private void LoadBackgroundImage()
    {
        // Create a gradient background if no image is provided
        if (backgroundImage == null)
        {
            backgroundImage = new Texture2D(1, 2);
            backgroundImage.SetPixel(0, 0, new Color(0.2f, 0.25f, 0.35f)); // Dark blue
            backgroundImage.SetPixel(0, 1, new Color(0.15f, 0.2f, 0.3f)); // Darker blue
            backgroundImage.Apply();
        }
    }

    private void OnGUI()
    {
        // Initialize styles on first GUI call
        InitializeStyles();

        // Draw background
        if (backgroundImage != null)
        {
            GUI.DrawTexture(new Rect(0, 0, position.width, position.height), backgroundImage, ScaleMode.StretchToFill);
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Draw current step
        switch (currentStep)
        {
            case SetupStep.Welcome:
                DrawWelcomeStep();
                break;
            case SetupStep.CheckPhoton:
                DrawCheckPhotonStep();
                break;
            case SetupStep.CreateGameManager:
                DrawCreateGameManagerStep();
                break;
            case SetupStep.CreateLobbyUI:
                DrawCreateLobbyUIStep();
                break;
            case SetupStep.CreateLeaderboardUI:
                DrawCreateLeaderboardUIStep();
                break;
            case SetupStep.Complete:
                DrawCompleteStep();
                break;
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawWelcomeStep()
    {
        GUILayout.Space(20);
        
        if (headerStyle != null)
        {
            GUILayout.Label("🎮 Photon Game Manager Setup Wizard", headerStyle);
        }
        else
        {
            GUILayout.Label("🎮 Photon Game Manager Setup Wizard", EditorStyles.boldLabel);
        }
        
        GUILayout.Space(20);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Welcome to the Photon Game Manager Setup Wizard!");
        GUILayout.Space(10);
        EditorGUILayout.LabelField("This wizard will help you set up:");
        EditorGUILayout.LabelField("✅ PhotonGameManager (static lobby system)");
        EditorGUILayout.LabelField("✅ Connection status display");
        EditorGUILayout.LabelField("✅ Leaderboard UI system");
        EditorGUILayout.LabelField("✅ Integration with WaveManager");
        GUILayout.Space(10);
        EditorGUILayout.LabelField("⚠️ Make sure you have:");
        EditorGUILayout.LabelField("• Photon PUN2 installed");
        EditorGUILayout.LabelField("• Photon App ID configured");
        EditorGUILayout.LabelField("• TextMeshPro imported");
        EditorGUILayout.EndVertical();

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (buttonStyle != null)
        {
            if (GUILayout.Button("Get Started", buttonStyle, GUILayout.Width(200), GUILayout.Height(40)))
            {
                currentStep = SetupStep.CheckPhoton;
            }
        }
        else
        {
            if (GUILayout.Button("Get Started", GUILayout.Width(200), GUILayout.Height(40)))
            {
                currentStep = SetupStep.CheckPhoton;
            }
        }
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);
    }

    private void DrawCheckPhotonStep()
    {
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Step 1: Check Photon PUN2", headerStyle ?? EditorStyles.boldLabel);
        GUILayout.Space(20);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        bool photonInstalled = CheckPhotonInstalled();
        
        if (photonInstalled)
        {
            EditorGUILayout.LabelField("✅ Photon PUN2 is installed!");
            GUILayout.Space(10);
            
            bool appIdConfigured = CheckPhotonAppId();
            if (appIdConfigured)
            {
                EditorGUILayout.LabelField("✅ Photon App ID is configured!");
            }
            else
            {
                EditorGUILayout.LabelField("⚠️ Photon App ID not configured");
                EditorGUILayout.LabelField("Please configure your App ID:");
                if (GUILayout.Button("Open PUN Wizard"))
                {
                    EditorApplication.ExecuteMenuItem("Window/Photon Unity Networking/PUN Wizard");
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField("❌ Photon PUN2 not found!");
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Please install Photon PUN2:");
            if (GUILayout.Button("Open Asset Store"))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/network/pun-2-free-119922");
            }
        }
        
        EditorGUILayout.EndVertical();

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("← Back", GUILayout.Width(100)))
        {
            currentStep = SetupStep.Welcome;
        }
        GUILayout.FlexibleSpace();
        
        GUI.enabled = photonInstalled;
        if (GUILayout.Button("Next →", GUILayout.Width(100)))
        {
            currentStep = SetupStep.CreateGameManager;
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);
    }

    private void DrawCreateGameManagerStep()
    {
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Step 2: Configure Game Manager", headerStyle ?? EditorStyles.boldLabel);
        GUILayout.Space(20);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Configure your Photon settings:");
        GUILayout.Space(10);

        lobbyRoomName = EditorGUILayout.TextField("Lobby Room Name:", lobbyRoomName);
        maxPlayers = EditorGUILayout.IntSlider("Max Players:", maxPlayers, 10, 500);
        autoConnect = EditorGUILayout.Toggle("Auto Connect on Start:", autoConnect);
        createConnectionStatus = EditorGUILayout.Toggle("Create Connection Status UI:", createConnectionStatus);

        GUILayout.Space(10);

        if (GUILayout.Button("Create Game Manager", GUILayout.Height(30)))
        {
            CreateGameManager();
        }

        if (gameManagerObj != null)
        {
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("✅ Game Manager created successfully!", MessageType.Info);
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("← Back", GUILayout.Width(100)))
        {
            currentStep = SetupStep.CheckPhoton;
        }
        GUILayout.FlexibleSpace();
        
        GUI.enabled = gameManagerObj != null;
        if (GUILayout.Button("Next →", GUILayout.Width(100)))
        {
            currentStep = SetupStep.CreateLobbyUI;
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);
    }

    private void DrawCreateLobbyUIStep()
    {
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Step 3: Setup Connection Status", headerStyle ?? EditorStyles.boldLabel);
        GUILayout.Space(20);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        if (createConnectionStatus)
        {
            EditorGUILayout.LabelField("Creating connection status UI...");
            GUILayout.Space(10);

            if (GUILayout.Button("Create Connection Status UI", GUILayout.Height(30)))
            {
                CreateConnectionStatusUI();
            }

            if (connectionCheckerObj != null)
            {
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("✅ Connection Status UI created!", MessageType.Info);
            }
        }
        else
        {
            EditorGUILayout.LabelField("Connection Status UI creation skipped.");
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("← Back", GUILayout.Width(100)))
        {
            currentStep = SetupStep.CreateGameManager;
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Next →", GUILayout.Width(100)))
        {
            currentStep = SetupStep.CreateLeaderboardUI;
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);
    }

    private void DrawCreateLeaderboardUIStep()
    {
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Step 4: Setup Leaderboard", headerStyle ?? EditorStyles.boldLabel);
        GUILayout.Space(20);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        createLeaderboardScene = EditorGUILayout.Toggle("Create Leaderboard Scene:", createLeaderboardScene);
        
        if (createLeaderboardScene)
        {
            leaderboardSceneName = EditorGUILayout.TextField("Scene Name:", leaderboardSceneName);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Create Leaderboard Scene", GUILayout.Height(30)))
            {
                CreateLeaderboardScene();
            }
        }
        else
        {
            EditorGUILayout.LabelField("You can manually create the leaderboard UI later.");
        }

        EditorGUILayout.EndVertical();

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("← Back", GUILayout.Width(100)))
        {
            currentStep = SetupStep.CreateLobbyUI;
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Finish →", GUILayout.Width(100)))
        {
            currentStep = SetupStep.Complete;
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);
    }

    private void DrawCompleteStep()
    {
        GUILayout.Space(20);
        EditorGUILayout.LabelField("🎉 Setup Complete!", headerStyle ?? EditorStyles.boldLabel);
        GUILayout.Space(20);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Your Photon Game Manager is ready!");
        GUILayout.Space(10);
        EditorGUILayout.LabelField("What was created:");
        EditorGUILayout.LabelField("✅ PhotonGameManager GameObject");
        if (createConnectionStatus)
            EditorGUILayout.LabelField("✅ Connection Status UI");
        if (createLeaderboardScene)
            EditorGUILayout.LabelField("✅ Leaderboard Scene");
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Next steps:");
        EditorGUILayout.LabelField("1. Test the connection (Play mode)");
        EditorGUILayout.LabelField("2. Link WaveManager in Game scene");
        EditorGUILayout.LabelField("3. Add scenes to Build Settings");
        GUILayout.Space(10);
        EditorGUILayout.LabelField("📖 For detailed instructions, see:");
        EditorGUILayout.LabelField("Assets/Scripts/Managers/PhotonGameManagerSetup.md");
        EditorGUILayout.EndVertical();

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Open Documentation", GUILayout.Width(200)))
        {
            string path = Path.Combine(Application.dataPath, "Scripts/Managers/PhotonGameManagerSetup.md");
            if (File.Exists(path))
            {
                System.Diagnostics.Process.Start(path);
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close Wizard", GUILayout.Width(200)))
        {
            Close();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);
    }

    #region Helper Methods

    private bool CheckPhotonInstalled()
    {
        // Check if Photon namespace exists
        var assembly = System.AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.FullName.Contains("PhotonUnityNetworking"));
        return assembly != null;
    }

    private bool CheckPhotonAppId()
    {
        // Try to find PhotonServerSettings
        string[] guids = AssetDatabase.FindAssets("PhotonServerSettings t:ScriptableObject");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (settings != null)
            {
                var appIdField = settings.GetType().GetField("AppIdRealtime");
                if (appIdField != null)
                {
                    string appId = appIdField.GetValue(settings) as string;
                    return !string.IsNullOrEmpty(appId);
                }
            }
        }
        return false;
    }

    private void CreateGameManager()
    {
        // Check if already exists
        gameManagerObj = GameObject.Find("GameManager");
        if (gameManagerObj == null)
        {
            gameManagerObj = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(gameManagerObj, "Create Game Manager");
        }

        // Add PhotonGameManager component
        var manager = gameManagerObj.GetComponent<PhotonGameManager>();
        if (manager == null)
        {
            manager = gameManagerObj.AddComponent<PhotonGameManager>();
        }

        // Configure using reflection to set private fields
        var type = typeof(PhotonGameManager);
        
        SetPrivateField(manager, "lobbyRoomName", lobbyRoomName);
        SetPrivateField(manager, "maxPlayersInLobby", maxPlayers);
        SetPrivateField(manager, "autoConnectOnStart", autoConnect);
        SetPrivateField(manager, "showDebugLogs", true);

        EditorUtility.SetDirty(manager);
        Selection.activeGameObject = gameManagerObj;
        
        Debug.Log("[Setup Wizard] GameManager created successfully!");
    }

    private void CreateConnectionStatusUI()
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
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }

        // Create status text
        GameObject statusTextObj = new GameObject("ConnectionStatusText");
        statusTextObj.transform.SetParent(canvas.transform, false);
        Undo.RegisterCreatedObjectUndo(statusTextObj, "Create Status Text");

        TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Connecting...";
        statusText.fontSize = 18;
        statusText.alignment = TextAlignmentOptions.TopRight;
        statusText.color = Color.white;

        RectTransform rect = statusText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-20, -20);
        rect.sizeDelta = new Vector2(300, 50);

        // Create ConnectionChecker GameObject
        connectionCheckerObj = new GameObject("ConnectionChecker");
        Undo.RegisterCreatedObjectUndo(connectionCheckerObj, "Create Connection Checker");
        
        var checker = connectionCheckerObj.AddComponent<PhotonConnectionChecker>();
        SetPrivateField(checker, "statusText", statusText);
        SetPrivateField(checker, "autoConnectOnStart", false); // GameManager handles connection

        EditorUtility.SetDirty(checker);
        
        Debug.Log("[Setup Wizard] Connection Status UI created!");
    }

    private void CreateLeaderboardScene()
    {
        // Create or use existing scene
        bool createNewScene = EditorUtility.DisplayDialog(
            "Create Leaderboard Scene", 
            "Do you want to create a new scene for the leaderboard?\n\n" +
            "Yes = Create new scene\n" +
            "No = Add to current scene", 
            "Yes (New Scene)", 
            "No (Current Scene)");

        if (createNewScene)
        {
            // Create new scene
            var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, 
                UnityEditor.SceneManagement.NewSceneMode.Single);
        }

        // Create leaderboard UI
        CreateLeaderboardUI();

        if (createNewScene)
        {
            // Save scene
            string scenePath = $"Assets/Scenes/{leaderboardSceneName}.unity";
            
            // Ensure Scenes folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
            
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(), 
                scenePath);
            
            Debug.Log($"[Setup Wizard] Leaderboard scene created at: {scenePath}");
            EditorUtility.DisplayDialog("Scene Created", 
                $"Leaderboard scene created at:\n{scenePath}\n\n" +
                "Don't forget to add it to Build Settings!", 
                "OK");
        }
    }

    private void CreateLeaderboardUI()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }

        // Create LeaderboardPanel
        GameObject panelObj = new GameObject("LeaderboardPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        Undo.RegisterCreatedObjectUndo(panelObj, "Create Leaderboard Panel");

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f); // Dark semi-transparent

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Create Title
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        Undo.RegisterCreatedObjectUndo(titleObj, "Create Title");

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "WAVE LEADERBOARD";
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -50);
        titleRect.sizeDelta = new Vector2(-100, 80);

        // Create ScrollView
        GameObject scrollViewObj = new GameObject("ScrollView");
        scrollViewObj.transform.SetParent(panelObj.transform, false);
        Undo.RegisterCreatedObjectUndo(scrollViewObj, "Create ScrollView");

        RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.1f, 0.15f);
        scrollRect.anchorMax = new Vector2(0.9f, 0.85f);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;

        ScrollRect scrollComponent = scrollViewObj.AddComponent<ScrollRect>();
        scrollComponent.horizontal = false;
        scrollComponent.vertical = true;
        scrollComponent.movementType = ScrollRect.MovementType.Clamped;

        Image scrollImage = scrollViewObj.AddComponent<Image>();
        scrollImage.color = new Color(0.05f, 0.05f, 0.1f, 0.8f);

        // Create Viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        Undo.RegisterCreatedObjectUndo(viewportObj, "Create Viewport");

        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = Color.white;

        scrollComponent.viewport = viewportRect;

        // Create Content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        Undo.RegisterCreatedObjectUndo(contentObj, "Create Content");

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.spacing = 10;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        ContentSizeFitter sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollComponent.content = contentRect;

        // Create Scrollbar
        GameObject scrollbarObj = new GameObject("Scrollbar");
        scrollbarObj.transform.SetParent(scrollViewObj.transform, false);
        Undo.RegisterCreatedObjectUndo(scrollbarObj, "Create Scrollbar");

        RectTransform scrollbarRect = scrollbarObj.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = new Vector2(1, 1);
        scrollbarRect.pivot = new Vector2(1, 1);
        scrollbarRect.anchoredPosition = Vector2.zero;
        scrollbarRect.sizeDelta = new Vector2(20, 0);

        Image scrollbarImage = scrollbarObj.AddComponent<Image>();
        scrollbarImage.color = new Color(0.2f, 0.2f, 0.25f);

        Scrollbar scrollbarComponent = scrollbarObj.AddComponent<Scrollbar>();
        scrollbarComponent.direction = Scrollbar.Direction.BottomToTop;

        // Create Scrollbar Handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(scrollbarObj.transform, false);
        Undo.RegisterCreatedObjectUndo(handleObj, "Create Handle");

        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = Vector2.zero;
        handleRect.offsetMax = Vector2.zero;

        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = new Color(0.4f, 0.5f, 0.7f);

        scrollbarComponent.handleRect = handleRect;
        scrollbarComponent.targetGraphic = handleImage;
        scrollComponent.verticalScrollbar = scrollbarComponent;

        // Create No Players Text
        GameObject noPlayersObj = new GameObject("NoPlayersText");
        noPlayersObj.transform.SetParent(panelObj.transform, false);
        Undo.RegisterCreatedObjectUndo(noPlayersObj, "Create No Players Text");

        TextMeshProUGUI noPlayersText = noPlayersObj.AddComponent<TextMeshProUGUI>();
        noPlayersText.text = "No players online";
        noPlayersText.fontSize = 24;
        noPlayersText.alignment = TextAlignmentOptions.Center;
        noPlayersText.color = new Color(0.7f, 0.7f, 0.7f);

        RectTransform noPlayersRect = noPlayersObj.GetComponent<RectTransform>();
        noPlayersRect.anchorMin = new Vector2(0.2f, 0.4f);
        noPlayersRect.anchorMax = new Vector2(0.8f, 0.6f);
        noPlayersRect.offsetMin = Vector2.zero;
        noPlayersRect.offsetMax = Vector2.zero;

        // Create LeaderboardEntry Prefab
        GameObject entryPrefab = CreateLeaderboardEntryPrefab();

        // Create LeaderboardManager
        GameObject managerObj = new GameObject("LeaderboardManager");
        Undo.RegisterCreatedObjectUndo(managerObj, "Create Leaderboard Manager");

        WaveLeaderboardUI leaderboardUI = managerObj.AddComponent<WaveLeaderboardUI>();
        SetPrivateField(leaderboardUI, "leaderboardContainer", contentObj.transform);
        SetPrivateField(leaderboardUI, "leaderboardEntryPrefab", entryPrefab);
        SetPrivateField(leaderboardUI, "noPlayersText", noPlayersText);
        SetPrivateField(leaderboardUI, "titleText", titleText);

        EditorUtility.SetDirty(leaderboardUI);

        Selection.activeGameObject = panelObj;
        
        Debug.Log("[Setup Wizard] Leaderboard UI created successfully!");
        EditorUtility.DisplayDialog("Success", 
            "Leaderboard UI created!\n\n" +
            "Components created:\n" +
            "✅ ScrollView with Content\n" +
            "✅ Leaderboard Entry Prefab\n" +
            "✅ WaveLeaderboardUI Manager\n\n" +
            "The leaderboard is ready to use!", 
            "OK");
    }

    private GameObject CreateLeaderboardEntryPrefab()
    {
        // Create entry GameObject
        GameObject entryObj = new GameObject("LeaderboardEntry");

        // Add Image (background)
        Image bgImage = entryObj.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.2f, 0.3f, 0.9f);

        RectTransform entryRect = entryObj.GetComponent<RectTransform>();
        entryRect.sizeDelta = new Vector2(800, 80);

        // Add Horizontal Layout Group
        HorizontalLayoutGroup layoutGroup = entryObj.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.padding = new RectOffset(15, 15, 10, 10);
        layoutGroup.spacing = 20;
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        // Add Layout Element
        LayoutElement layoutElement = entryObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 80;
        layoutElement.preferredHeight = 80;

        // Create Rank Text
        GameObject rankObj = new GameObject("RankText");
        rankObj.transform.SetParent(entryObj.transform, false);

        TextMeshProUGUI rankText = rankObj.AddComponent<TextMeshProUGUI>();
        rankText.text = "#1";
        rankText.fontSize = 36;
        rankText.fontStyle = FontStyles.Bold;
        rankText.alignment = TextAlignmentOptions.Center;
        rankText.color = Color.white;

        RectTransform rankRect = rankObj.GetComponent<RectTransform>();
        rankRect.sizeDelta = new Vector2(80, 60);

        LayoutElement rankLayout = rankObj.AddComponent<LayoutElement>();
        rankLayout.preferredWidth = 80;

        // Create Name Text
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(entryObj.transform, false);

        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "PlayerName";
        nameText.fontSize = 32;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.color = Color.white;

        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(300, 60);

        LayoutElement nameLayout = nameObj.AddComponent<LayoutElement>();
        nameLayout.flexibleWidth = 1;

        // Create Current Wave Text
        GameObject currentObj = new GameObject("CurrentWaveText");
        currentObj.transform.SetParent(entryObj.transform, false);

        TextMeshProUGUI currentText = currentObj.AddComponent<TextMeshProUGUI>();
        currentText.text = "Current: Wave 5";
        currentText.fontSize = 24;
        currentText.alignment = TextAlignmentOptions.Left;
        currentText.color = new Color(0.7f, 0.7f, 0.7f);

        RectTransform currentRect = currentObj.GetComponent<RectTransform>();
        currentRect.sizeDelta = new Vector2(200, 60);

        LayoutElement currentLayout = currentObj.AddComponent<LayoutElement>();
        currentLayout.preferredWidth = 200;

        // Create Highest Wave Text
        GameObject highestObj = new GameObject("HighestWaveText");
        highestObj.transform.SetParent(entryObj.transform, false);

        TextMeshProUGUI highestText = highestObj.AddComponent<TextMeshProUGUI>();
        highestText.text = "Best: Wave 10";
        highestText.fontSize = 24;
        highestText.alignment = TextAlignmentOptions.Left;
        highestText.color = new Color(1f, 0.9f, 0.4f); // Gold

        RectTransform highestRect = highestObj.GetComponent<RectTransform>();
        highestRect.sizeDelta = new Vector2(200, 60);

        LayoutElement highestLayout = highestObj.AddComponent<LayoutElement>();
        highestLayout.preferredWidth = 200;

        // Save as prefab
        string prefabPath = "Assets/Prefabs/UI/LeaderboardEntryPrefab.prefab";
        
        // Ensure folders exist
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(entryObj, prefabPath);
        DestroyImmediate(entryObj);

        Debug.Log($"[Setup Wizard] Leaderboard entry prefab created at: {prefabPath}");
        
        return prefab;
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }

    #endregion
}
