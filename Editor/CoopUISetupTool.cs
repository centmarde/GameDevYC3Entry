using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

/// <summary>
/// Unity Editor tool to automatically create Coop UI in the scene
/// Access via: Tools > Setup > Create Coop UI
/// </summary>
public class CoopUISetupTool : EditorWindow
{
    private bool createPhotonView = true;
    private bool autoAssignReferences = true;
    
    [MenuItem("Tools/Setup/Create Coop UI")]
    public static void ShowWindow()
    {
        CoopUISetupTool window = GetWindow<CoopUISetupTool>("Coop UI Setup");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Coop UI Setup Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This tool will create a complete Coop UI with all necessary components:\n\n" +
            "• Canvas with CoopController\n" +
            "• Player Name and Room Name Input Fields\n" +
            "• Create Room, Join Room, Ready, Start Game, and Back Buttons\n" +
            "• Status, Players List, Room Info, and Ready Status Text\n" +
            "• PhotonView component for networking\n" +
            "• All references automatically assigned",
            MessageType.Info
        );

        GUILayout.Space(10);

        createPhotonView = EditorGUILayout.Toggle("Create PhotonView Component", createPhotonView);
        autoAssignReferences = EditorGUILayout.Toggle("Auto-Assign References", autoAssignReferences);

        GUILayout.Space(20);

        if (GUILayout.Button("Create Coop UI", GUILayout.Height(40)))
        {
            CreateCoopUI();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Remove Existing Coop UI", GUILayout.Height(30)))
        {
            RemoveExistingCoopUI();
        }
    }

    private void CreateCoopUI()
    {
        // Check if Coop UI already exists
        CoopController existingController = FindObjectOfType<CoopController>();
        if (existingController != null)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Coop UI Already Exists",
                "A CoopController already exists in the scene. Do you want to replace it?",
                "Yes, Replace",
                "Cancel"
            );

            if (!overwrite)
            {
                return;
            }

            DestroyImmediate(existingController.gameObject);
        }

        // Create Canvas
        GameObject canvasObj = new GameObject("CoopCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // Add CoopController
        CoopController controller = canvasObj.AddComponent<CoopController>();

        // Add PhotonView if requested
        if (createPhotonView)
        {
            PhotonView photonView = canvasObj.AddComponent<PhotonView>();
            photonView.ObservedComponents = new System.Collections.Generic.List<Component> { controller };
            photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
        }

        // Create main container
        GameObject container = CreateUIObject("CoopUIContainer", canvasObj.transform);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = new Vector2(50, 50);
        containerRect.offsetMax = new Vector2(-50, -50);

        // Create UI Elements
        TMP_InputField playerNameInput = CreateInputField(container.transform, "PlayerNameInput", "Enter your name...", 
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(10, -10), new Vector2(300, 50));

        TMP_InputField roomNameInput = CreateInputField(container.transform, "RoomNameInput", "Room name...", 
            new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -10), new Vector2(300, 50));

        TextMeshProUGUI statusText = CreateTextLabel(container.transform, "StatusText", "Status: Initializing...", 
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -10), new Vector2(300, 50), TextAlignmentOptions.Right);

        Button createRoomButton = CreateButton(container.transform, "CreateRoomButton", "CREATE ROOM", 
            new Vector2(0.25f, 0.6f), new Vector2(0.25f, 0.6f), Vector2.zero, new Vector2(200, 50), new Color(0.2f, 0.6f, 0.8f));

        Button joinRoomButton = CreateButton(container.transform, "JoinRoomButton", "JOIN ROOM", 
            new Vector2(0.75f, 0.6f), new Vector2(0.75f, 0.6f), Vector2.zero, new Vector2(200, 50), new Color(0.2f, 0.6f, 0.8f));

        Button readyButton = CreateButton(container.transform, "ReadyButton", "READY", 
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200, 50), new Color(0.8f, 0.8f, 0.2f));
        readyButton.gameObject.SetActive(false);

        Button startGameButton = CreateButton(container.transform, "StartGameButton", "START GAME", 
            new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(250, 60), new Color(0.2f, 0.8f, 0.2f));
        startGameButton.gameObject.SetActive(false);

        Button backButton = CreateButton(container.transform, "BackButton", "BACK", 
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(10, 10), new Vector2(150, 50), new Color(0.6f, 0.2f, 0.2f));

        TextMeshProUGUI playersListText = CreateTextLabel(container.transform, "PlayersListText", "Players:\nWaiting to join room...", 
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(250, 300), TextAlignmentOptions.TopLeft);
        playersListText.fontSize = 18;

        TextMeshProUGUI roomInfoText = CreateTextLabel(container.transform, "RoomInfoText", "", 
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(250, 200), TextAlignmentOptions.TopRight);
        roomInfoText.fontSize = 18;

        TextMeshProUGUI readyStatusText = CreateTextLabel(container.transform, "ReadyStatusText", "", 
            new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero, new Vector2(300, 40), TextAlignmentOptions.Center);
        readyStatusText.fontSize = 20;
        readyStatusText.fontStyle = FontStyles.Bold;
        readyStatusText.color = new Color(0.2f, 0.8f, 0.2f);

        // Auto-assign references if requested
        if (autoAssignReferences)
        {
            SerializedObject serializedController = new SerializedObject(controller);
            
            serializedController.FindProperty("playerNameInput").objectReferenceValue = playerNameInput;
            serializedController.FindProperty("roomNameInput").objectReferenceValue = roomNameInput;
            serializedController.FindProperty("createRoomButton").objectReferenceValue = createRoomButton;
            serializedController.FindProperty("joinRoomButton").objectReferenceValue = joinRoomButton;
            serializedController.FindProperty("readyButton").objectReferenceValue = readyButton;
            serializedController.FindProperty("startGameButton").objectReferenceValue = startGameButton;
            serializedController.FindProperty("backButton").objectReferenceValue = backButton;
            serializedController.FindProperty("statusText").objectReferenceValue = statusText;
            serializedController.FindProperty("playersListText").objectReferenceValue = playersListText;
            serializedController.FindProperty("roomInfoText").objectReferenceValue = roomInfoText;
            serializedController.FindProperty("readyStatusText").objectReferenceValue = readyStatusText;
            
            serializedController.ApplyModifiedProperties();
        }

        // Select the created canvas
        Selection.activeGameObject = canvasObj;

        EditorUtility.DisplayDialog(
            "Coop UI Created Successfully!",
            "The Coop UI has been created in your scene.\n\n" +
            "Next steps:\n" +
            "1. Ensure PhotonView is properly configured\n" +
            "2. Set up your Photon App ID in PhotonServerSettings\n" +
            "3. Add the 'Coop' scene to Build Settings\n" +
            "4. Test the UI in Play mode!",
            "OK"
        );

        Debug.Log("[CoopUISetupTool] Coop UI created successfully!");
    }

    private void RemoveExistingCoopUI()
    {
        CoopController controller = FindObjectOfType<CoopController>();
        
        if (controller == null)
        {
            EditorUtility.DisplayDialog("No Coop UI Found", "No CoopController found in the scene.", "OK");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog(
            "Remove Coop UI",
            "Are you sure you want to remove the existing Coop UI?",
            "Yes, Remove",
            "Cancel"
        );

        if (confirm)
        {
            DestroyImmediate(controller.gameObject);
            Debug.Log("[CoopUISetupTool] Coop UI removed from scene.");
        }
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        return obj;
    }

    private TMP_InputField CreateInputField(Transform parent, string name, string placeholder,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        GameObject inputObj = CreateUIObject(name, parent);
        RectTransform rect = inputObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x, anchorMax.y);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        Image bg = inputObj.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        TMP_InputField input = inputObj.AddComponent<TMP_InputField>();

        // Create text area
        GameObject textArea = CreateUIObject("Text Area", inputObj.transform);
        RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 5);
        textAreaRect.offsetMax = new Vector2(-10, -5);

        // Create placeholder
        GameObject placeholderObj = CreateUIObject("Placeholder", textArea.transform);
        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;

        TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 16;
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
        placeholderText.alignment = TextAlignmentOptions.Left;

        // Create text
        GameObject textObj = CreateUIObject("Text", textArea.transform);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;

        input.textViewport = textAreaRect;
        input.textComponent = text;
        input.placeholder = placeholderText;

        return input;
    }

    private Button CreateButton(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color color)
    {
        GameObject buttonObj = CreateUIObject(name, parent);
        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        Image bg = buttonObj.AddComponent<Image>();
        bg.color = color;

        Button button = buttonObj.AddComponent<Button>();
        
        // Set button color transitions
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.2f;
        colors.pressedColor = color * 0.8f;
        colors.selectedColor = color * 1.1f;
        button.colors = colors;

        // Create button text
        GameObject textObj = CreateUIObject("Text", buttonObj.transform);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = 18;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;

        return button;
    }

    private TextMeshProUGUI CreateTextLabel(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, TextAlignmentOptions alignment)
    {
        GameObject textObj = CreateUIObject(name, parent);
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(anchorMin.x, anchorMax.y);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 16;
        textComponent.color = Color.white;
        textComponent.alignment = alignment;

        return textComponent;
    }
}
