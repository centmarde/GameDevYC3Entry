using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Auto-setup script for the ChooseMenu scene.
/// This script automatically creates the UI hierarchy when run in the Unity Editor.
/// Run this by adding it to a GameObject in the scene and clicking the "Setup UI" button in Inspector
/// or using the context menu (right-click on component → "Auto Setup Choose Menu UI").
/// </summary>
public class ChooseMenuAutoSetup : MonoBehaviour
{
    [Header("Instructions")]
    [TextArea(3, 6)]
    [SerializeField] private string instructions = "Click the button below or right-click this component and select 'Auto Setup Choose Menu UI' to automatically create the character selection UI.";

    [Header("Setup Button")]
    [SerializeField] private bool setupUI = false;

    private void OnValidate()
    {
        if (setupUI)
        {
            setupUI = false;
            AutoSetupUI();
        }
    }

    [ContextMenu("Auto Setup Choose Menu UI")]
    public void AutoSetupUI()
    {
        Debug.Log("[ChooseMenuAutoSetup] Starting auto setup...");

        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("[ChooseMenuAutoSetup] Created Canvas");
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        // Create Background Image
        GameObject backgroundGO = new GameObject("Background");
        backgroundGO.transform.SetParent(canvas.transform, false);
        RectTransform backgroundRT = backgroundGO.AddComponent<RectTransform>();
        backgroundRT.anchorMin = Vector2.zero;
        backgroundRT.anchorMax = Vector2.one;
        backgroundRT.sizeDelta = Vector2.zero;
        Image backgroundImage = backgroundGO.AddComponent<Image>();
        backgroundImage.color = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark background
        Debug.Log("[ChooseMenuAutoSetup] Created Background");

        // Create Title Text
        GameObject titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(canvas.transform, false);
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0, -100);
        titleRT.sizeDelta = new Vector2(800, 100);
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "Choose Your Character";
        titleText.fontSize = 60;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        Debug.Log("[ChooseMenuAutoSetup] Created Title Text");

        // Create Card Container
        GameObject containerGO = new GameObject("CardContainer");
        containerGO.transform.SetParent(canvas.transform, false);
        RectTransform containerRT = containerGO.AddComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(0.5f, 0.5f);
        containerRT.anchorMax = new Vector2(0.5f, 0.5f);
        containerRT.pivot = new Vector2(0.5f, 0.5f);
        containerRT.anchoredPosition = Vector2.zero;
        containerRT.sizeDelta = new Vector2(1400, 600);
        HorizontalLayoutGroup layoutGroup = containerGO.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 100;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        Debug.Log("[ChooseMenuAutoSetup] Created Card Container");

        // Create Player1 Card
        GameObject player1CardGO = CreateCharacterCard(containerGO.transform, "Player1Card", "Warrior", new Color(0.8f, 0.2f, 0.2f));
        Debug.Log("[ChooseMenuAutoSetup] Created Player1 Card");

        // Create Player2 Card
        GameObject player2CardGO = CreateCharacterCard(containerGO.transform, "Player2Card", "Assassin", new Color(0.2f, 0.2f, 0.8f));
        Debug.Log("[ChooseMenuAutoSetup] Created Player2 Card");

        // Create CharacterSelectionUI Manager GameObject
        GameObject uiManagerGO = new GameObject("CharacterSelectionUI");
        uiManagerGO.transform.SetParent(canvas.transform, false);
        CharacterSelectionUI selectionUI = uiManagerGO.AddComponent<CharacterSelectionUI>();

        // Assign references
        Button player1Button = player1CardGO.GetComponent<Button>();
        Button player2Button = player2CardGO.GetComponent<Button>();

        // Use reflection to assign private fields
        var player1ButtonField = typeof(CharacterSelectionUI).GetField("player1CardButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var player2ButtonField = typeof(CharacterSelectionUI).GetField("player2CardButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (player1ButtonField != null) player1ButtonField.SetValue(selectionUI, player1Button);
        if (player2ButtonField != null) player2ButtonField.SetValue(selectionUI, player2Button);

        Debug.Log("[ChooseMenuAutoSetup] Setup complete! Assign character images in the Inspector.");
        Debug.Log("[ChooseMenuAutoSetup] Look for 'Player1Card/CharacterImage' and 'Player2Card/CharacterImage' to assign your sprites.");
    }

    private GameObject CreateCharacterCard(Transform parent, string name, string characterName, Color accentColor)
    {
        // Card Root
        GameObject cardGO = new GameObject(name);
        cardGO.transform.SetParent(parent, false);
        RectTransform cardRT = cardGO.AddComponent<RectTransform>();
        cardRT.sizeDelta = new Vector2(500, 600);
        
        // Add Button Component
        Button button = cardGO.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        colors.selectedColor = accentColor;
        button.colors = colors;

        // Card Background
        Image cardBG = cardGO.AddComponent<Image>();
        cardBG.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        // Card Border
        GameObject borderGO = new GameObject("Border");
        borderGO.transform.SetParent(cardGO.transform, false);
        RectTransform borderRT = borderGO.AddComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.sizeDelta = new Vector2(-10, -10);
        Image borderImage = borderGO.AddComponent<Image>();
        borderImage.color = accentColor;
        Outline outline = borderGO.AddComponent<Outline>();
        outline.effectColor = accentColor;
        outline.effectDistance = new Vector2(3, -3);

        // Character Image Container
        GameObject imageContainerGO = new GameObject("ImageContainer");
        imageContainerGO.transform.SetParent(cardGO.transform, false);
        RectTransform imageContainerRT = imageContainerGO.AddComponent<RectTransform>();
        imageContainerRT.anchorMin = new Vector2(0.5f, 0.5f);
        imageContainerRT.anchorMax = new Vector2(0.5f, 0.5f);
        imageContainerRT.pivot = new Vector2(0.5f, 0.5f);
        imageContainerRT.anchoredPosition = new Vector2(0, 50);
        imageContainerRT.sizeDelta = new Vector2(400, 400);

        // Character Image (placeholder)
        GameObject characterImageGO = new GameObject("CharacterImage");
        characterImageGO.transform.SetParent(imageContainerGO.transform, false);
        RectTransform characterImageRT = characterImageGO.AddComponent<RectTransform>();
        characterImageRT.anchorMin = Vector2.zero;
        characterImageRT.anchorMax = Vector2.one;
        characterImageRT.sizeDelta = Vector2.zero;
        Image characterImage = characterImageGO.AddComponent<Image>();
        characterImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray placeholder
        // This is where you'll assign your character sprite

        // Character Name
        GameObject nameGO = new GameObject("CharacterName");
        nameGO.transform.SetParent(cardGO.transform, false);
        RectTransform nameRT = nameGO.AddComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0.5f, 0f);
        nameRT.anchorMax = new Vector2(0.5f, 0f);
        nameRT.pivot = new Vector2(0.5f, 0f);
        nameRT.anchoredPosition = new Vector2(0, 30);
        nameRT.sizeDelta = new Vector2(400, 80);
        TextMeshProUGUI nameText = nameGO.AddComponent<TextMeshProUGUI>();
        nameText.text = characterName;
        nameText.fontSize = 48;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = accentColor;
        nameText.fontStyle = FontStyles.Bold;

        // Highlight (optional selection indicator)
        GameObject highlightGO = new GameObject("Highlight");
        highlightGO.transform.SetParent(cardGO.transform, false);
        RectTransform highlightRT = highlightGO.AddComponent<RectTransform>();
        highlightRT.anchorMin = Vector2.zero;
        highlightRT.anchorMax = Vector2.one;
        highlightRT.sizeDelta = new Vector2(20, 20);
        Image highlightImage = highlightGO.AddComponent<Image>();
        highlightImage.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.3f);
        highlightGO.SetActive(false); // Hidden by default

        return cardGO;
    }
}
