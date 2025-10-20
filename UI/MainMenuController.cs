using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main Menu Controller - Handles main menu UI interactions
/// 
/// QUICK SETUP DOCUMENTATION:
/// ==========================
/// 1. Create a Canvas in your MainMenu scene (GameObject > UI > Canvas)
/// 2. Add this script to the Canvas or create an empty GameObject and attach it
/// 3. Create UI elements:
///    - Image for background (UI > Image) - stretch to full screen
///    - Button for Start (UI > Button - TextMeshPro)
///    - Button for Quit (UI > Button - TextMeshPro)
/// 4. Assign references in the Inspector:
///    - Drag the Start button to the "Start Button" field
///    - Drag the Quit button to the "Quit Button" field
///    - (Optional) Drag background image to "Background Image" field
/// 5. Set your background image:
///    - Import your image into Unity (Assets/Graphics or Assets/UI folder)
///    - Set Texture Type to "Sprite (2D and UI)" in Inspector
///    - Drag the sprite to the Image component's "Source Image" field
/// 6. Add "MainBase" scene to Build Settings:
///    - File > Build Settings
///    - Add "MainBase" scene to "Scenes in Build" list
/// 7. Press Play to test!
/// 
/// STYLING TIPS:
/// - For background: Set Image component's Image Type to "Simple" and stretch anchors
/// - For buttons: Use TextMeshPro for better text quality
/// - Position buttons in center-bottom area of screen
/// - Add hover effects using Button's Transition settings
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Reference to the Start Game button")]
    [SerializeField] private Button startButton;
    
    [Tooltip("Reference to the Quit Game button")]
    [SerializeField] private Button quitButton;
    
    [Tooltip("Optional: Reference to background image for runtime customization")]
    [SerializeField] private Image backgroundImage;

    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load when Start is clicked")]
    [SerializeField] private string gameSceneName = "IntroScene"; // Changed to IntroScene for tutorial flow

    private void Start()
    {
        // Validate references
        if (startButton == null)
        {
            Debug.LogError("MainMenuController: Start Button is not assigned in the Inspector!");
        }
        else
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (quitButton == null)
        {
            Debug.LogError("MainMenuController: Quit Button is not assigned in the Inspector!");
        }
        else
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }

        // Optional: Ensure cursor is visible and unlocked in menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Called when the Start button is clicked
    /// Loads the main game scene with loading screen
    /// </summary>
    private void OnStartButtonClicked()
    {
        Debug.Log($"Loading scene: {gameSceneName}");
        
        // Use LoadingScreen if available, otherwise load normally
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.LoadScene(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    /// <summary>
    /// Called when the Quit button is clicked
    /// Exits the application
    /// </summary>
    private void OnQuitButtonClicked()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
        // If running in the Unity Editor, stop playing
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // If running as a build, quit the application
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Optional: Method to change background image at runtime
    /// </summary>
    /// <param name="newBackground">New sprite to use as background</param>
    public void SetBackgroundImage(Sprite newBackground)
    {
        if (backgroundImage != null && newBackground != null)
        {
            backgroundImage.sprite = newBackground;
        }
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }
    }
}
