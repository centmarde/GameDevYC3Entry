using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Inspector for PhotonGameManager with styled background
/// </summary>
[CustomEditor(typeof(PhotonGameManager))]
public class PhotonGameManagerEditor : Editor
{
    private Texture2D backgroundTexture;
    private Texture2D headerTexture;
    private GUIStyle headerStyle;
    private GUIStyle sectionStyle;
    private GUIStyle statusStyle;
    
    private SerializedProperty lobbyRoomName;
    private SerializedProperty maxPlayersInLobby;
    private SerializedProperty autoConnectOnStart;
    private SerializedProperty playerName;
    private SerializedProperty waveManager;
    private SerializedProperty statusText;
    private SerializedProperty playerCountText;
    private SerializedProperty showDebugLogs;

    private bool showLobbySettings = true;
    private bool showPlayerData = true;
    private bool showUIReferences = true;
    private bool showDebugSettings = true;
    private bool showRuntimeInfo = true;

    private void OnEnable()
    {
        // Initialize serialized properties
        lobbyRoomName = serializedObject.FindProperty("lobbyRoomName");
        maxPlayersInLobby = serializedObject.FindProperty("maxPlayersInLobby");
        autoConnectOnStart = serializedObject.FindProperty("autoConnectOnStart");
        playerName = serializedObject.FindProperty("playerName");
        waveManager = serializedObject.FindProperty("waveManager");
        statusText = serializedObject.FindProperty("statusText");
        playerCountText = serializedObject.FindProperty("playerCountText");
        showDebugLogs = serializedObject.FindProperty("showDebugLogs");

        CreateBackgroundTexture();
        CreateHeaderTexture();
        InitializeStyles();
    }

    private void CreateBackgroundTexture()
    {
        if (backgroundTexture == null)
        {
            backgroundTexture = new Texture2D(1, 2);
            backgroundTexture.SetPixel(0, 0, new Color(0.22f, 0.27f, 0.37f, 1f)); // Dark blue-gray
            backgroundTexture.SetPixel(0, 1, new Color(0.18f, 0.23f, 0.33f, 1f)); // Darker blue-gray
            backgroundTexture.Apply();
        }
    }

    private void CreateHeaderTexture()
    {
        if (headerTexture == null)
        {
            // Create a gradient header
            headerTexture = new Texture2D(1, 60);
            for (int i = 0; i < 60; i++)
            {
                float t = i / 60f;
                Color color = Color.Lerp(
                    new Color(0.3f, 0.4f, 0.6f, 1f), // Light blue
                    new Color(0.2f, 0.3f, 0.5f, 1f), // Dark blue
                    t
                );
                headerTexture.SetPixel(0, i, color);
            }
            headerTexture.Apply();
        }
    }

    private void InitializeStyles()
    {
        headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 16;
        headerStyle.normal.textColor = Color.white;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.padding = new RectOffset(10, 10, 15, 15);

        sectionStyle = new GUIStyle(EditorStyles.helpBox);
        sectionStyle.padding = new RectOffset(10, 10, 10, 10);

        statusStyle = new GUIStyle(EditorStyles.label);
        statusStyle.fontSize = 11;
        statusStyle.normal.textColor = new Color(0.7f, 0.9f, 1f);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        PhotonGameManager manager = (PhotonGameManager)target;

        // Draw header with background
        DrawHeader();

        // Runtime status
        if (Application.isPlaying)
        {
            DrawRuntimeStatus(manager);
        }

        EditorGUILayout.Space(5);

        // Draw sections
        DrawLobbySettings();
        DrawPlayerData();
        DrawUIReferences();
        DrawDebugSettings();

        EditorGUILayout.Space(10);

        // Action buttons
        DrawActionButtons(manager);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeader()
    {
        // Draw header background
        Rect headerRect = GUILayoutUtility.GetRect(0, 70, GUILayout.ExpandWidth(true));
        if (headerTexture != null)
        {
            GUI.DrawTexture(headerRect, headerTexture, ScaleMode.StretchToFill);
        }

        // Draw header content
        GUILayout.BeginArea(new Rect(headerRect.x, headerRect.y, headerRect.width, headerRect.height));
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        
        GUILayout.Label("🌐 PHOTON GAME MANAGER", headerStyle);
        
        GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel);
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        subtitleStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
        GUILayout.Label("Multiplayer Lobby & Leaderboard System", subtitleStyle);
        
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();

        GUILayout.Space(5);
    }

    private void DrawRuntimeStatus(PhotonGameManager manager)
    {
        EditorGUILayout.BeginVertical(sectionStyle);
        
        GUILayout.Label("🔴 LIVE STATUS", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Connection status
        bool isConnected = manager.IsConnectedToPhoton();
        DrawStatusRow("Connection:", isConnected ? "✅ Connected" : "❌ Disconnected", 
            isConnected ? Color.green : Color.red);

        // Lobby status
        bool inLobby = manager.IsInLobby();
        DrawStatusRow("Lobby:", inLobby ? $"✅ {manager.GetLobbyName()}" : "❌ Not in lobby", 
            inLobby ? Color.green : Color.red);

        if (inLobby)
        {
            DrawStatusRow("Players Online:", manager.GetPlayerCount().ToString(), Color.cyan);
        }

        // Player info
        DrawStatusRow("Player Name:", manager.GetPlayerName(), Color.yellow);
        DrawStatusRow("Current Wave:", manager.GetCurrentWave().ToString(), Color.white);
        DrawStatusRow("Highest Wave:", manager.GetHighestWave().ToString(), Color.magenta);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private void DrawStatusRow(string label, string value, Color valueColor)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(120));
        
        GUIStyle valueStyle = new GUIStyle(EditorStyles.label);
        valueStyle.normal.textColor = valueColor;
        valueStyle.fontStyle = FontStyle.Bold;
        GUILayout.Label(value, valueStyle);
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLobbySettings()
    {
        showLobbySettings = EditorGUILayout.BeginFoldoutHeaderGroup(showLobbySettings, "🏠 Lobby Settings");
        if (showLobbySettings)
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.PropertyField(lobbyRoomName, new GUIContent("Lobby Room Name"));
            EditorGUILayout.PropertyField(maxPlayersInLobby, new GUIContent("Max Players"));
            EditorGUILayout.PropertyField(autoConnectOnStart, new GUIContent("Auto Connect"));
            
            EditorGUILayout.HelpBox("Static lobby that all players join automatically.", MessageType.Info);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawPlayerData()
    {
        showPlayerData = EditorGUILayout.BeginFoldoutHeaderGroup(showPlayerData, "👤 Player Data");
        if (showPlayerData)
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.PropertyField(playerName, new GUIContent("Player Name"));
            EditorGUILayout.PropertyField(waveManager, new GUIContent("Wave Manager"));
            
            EditorGUILayout.HelpBox("Player name is set from LobbyController. WaveManager can auto-detect if not assigned.", MessageType.Info);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawUIReferences()
    {
        showUIReferences = EditorGUILayout.BeginFoldoutHeaderGroup(showUIReferences, "📺 UI References (Optional)");
        if (showUIReferences)
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.PropertyField(statusText, new GUIContent("Status Text"));
            EditorGUILayout.PropertyField(playerCountText, new GUIContent("Player Count Text"));
            
            EditorGUILayout.HelpBox("Optional: Assign UI text elements to display connection status.", MessageType.None);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawDebugSettings()
    {
        showDebugSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showDebugSettings, "🔧 Debug Settings");
        if (showDebugSettings)
        {
            EditorGUILayout.BeginVertical(sectionStyle);
            EditorGUILayout.PropertyField(showDebugLogs, new GUIContent("Show Debug Logs"));
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawActionButtons(PhotonGameManager manager)
    {
        EditorGUILayout.BeginVertical(sectionStyle);
        GUILayout.Label("Quick Actions", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        
        if (Application.isPlaying)
        {
            // Runtime buttons
            if (!manager.IsConnectedToPhoton())
            {
                if (GUILayout.Button("🔌 Connect to Photon", GUILayout.Height(30)))
                {
                    manager.ConnectToPhoton();
                }
            }
            else
            {
                if (GUILayout.Button("🔌 Disconnect", GUILayout.Height(30)))
                {
                    manager.DisconnectFromPhoton();
                }
            }

            if (GUILayout.Button("🔄 Reset Stats", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Reset Player Stats", 
                    "Are you sure you want to reset all player stats?", "Yes", "No"))
                {
                    manager.ResetPlayerStats();
                }
            }
        }
        else
        {
            // Editor buttons
            if (GUILayout.Button("📖 Open Documentation", GUILayout.Height(30)))
            {
                string path = System.IO.Path.Combine(Application.dataPath, 
                    "Scripts/Managers/PhotonGameManagerSetup.md");
                if (System.IO.File.Exists(path))
                {
                    System.Diagnostics.Process.Start(path);
                }
                else
                {
                    EditorUtility.DisplayDialog("File Not Found", 
                        "Documentation file not found at:\n" + path, "OK");
                }
            }

            if (GUILayout.Button("🔧 Open Setup Wizard", GUILayout.Height(30)))
            {
                PhotonGameManagerSetupWizard.ShowWindow();
            }
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}
