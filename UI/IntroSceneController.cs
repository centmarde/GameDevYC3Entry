using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Intro Scene Controller - Displays a sequence of tutorial/intro slides with images, text, and audio
/// 
/// FEATURES:
/// - Dynamic slide system (add as many slides as you want)
/// - Each slide has: Image, Text, and Audio
/// - Navigation: Next, Previous, Skip (current), Skip All
/// - Auto-advance option
/// - Progress indicator
/// - Fade transitions
/// - Keyboard support (Arrow keys, Space, Escape)
/// 
/// QUICK SETUP:
/// 1. Create Canvas with UI elements (see documentation)
/// 2. Attach this script to an empty GameObject
/// 3. Create IntroSlide data in Inspector (minimum 12, can add more)
/// 4. Assign UI references
/// 5. Set target scene name
/// 6. Test!
/// </summary>
[System.Serializable]
public class IntroSlide
{
    [Tooltip("The image to display for this slide")]
    public Sprite slideImage;
    
    [Tooltip("The text content for this slide")]
    [TextArea(3, 10)]
    public string slideText;
    
    [Tooltip("Optional audio clip to play for this slide")]
    public AudioClip slideAudio;
    
    [Tooltip("How long to display this slide (if auto-advance is enabled)")]
    public float displayDuration = 5f;
}

public class IntroSceneController : MonoBehaviour
{
    [Header("Slide Data")]
    [Tooltip("Array of intro slides - add as many as you need (minimum 12 recommended)")]
    [SerializeField] private List<IntroSlide> slides = new List<IntroSlide>();

    [Header("UI References")]
    [SerializeField] private Image slideImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button skipAllButton;
    [SerializeField] private Slider progressSlider; // Optional progress bar

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup slideCanvasGroup;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("Background music audio source (separate from narration)")]
    [SerializeField] private AudioSource backgroundMusicSource;
    
    [Tooltip("Background music clip to play during intro")]
    [SerializeField] private AudioClip backgroundMusicClip;

    [Header("Scene Settings")]
    [Tooltip("Scene to load after intro completes")]
    [SerializeField] private string targetSceneName = "MainBase";
    
    [Tooltip("Auto-advance to next slide after duration")]
    [SerializeField] private bool autoAdvance = true;
    
    [Tooltip("Default time before auto-advancing (if slide doesn't specify)")]
    [SerializeField] private float defaultAutoAdvanceTime = 5f;
    
    [Header("Animation Settings")]
    [Tooltip("Enable dramatic zoom/tilt animation for slide images")]
    [SerializeField] private bool enableImageAnimation = true;
    
    [Tooltip("Zoom intensity (1.0 = no zoom, 1.2 = 20% zoom)")]
    [SerializeField] private float zoomIntensity = 1.15f;
    
    [Tooltip("Tilt/rotation intensity in degrees")]
    [SerializeField] private float tiltIntensity = 2f;
    
    [Tooltip("Animation duration in seconds")]
    [SerializeField] private float animationDuration = 5f;

    private int currentSlideIndex = 0;
    private bool isTransitioning = false;
    private Coroutine autoAdvanceCoroutine;

    private void Start()
    {
        // Validate setup
        ValidateSetup();

        // Setup button listeners
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
        
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipButtonClicked);
        
        if (skipAllButton != null)
            skipAllButton.onClick.AddListener(OnSkipAllButtonClicked);

        // Start background music
        StartBackgroundMusic();

        // Show first slide
        currentSlideIndex = 0;
        ShowSlide(currentSlideIndex, false);
    }

    private void Update()
    {
        // Keyboard shortcuts using new Input System
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                OnNextButtonClicked();
            }
            else if (keyboard.escapeKey.wasPressedThisFrame)
            {
                OnSkipAllButtonClicked();
            }
            else if (keyboard.nKey.wasPressedThisFrame)
            {
                OnSkipButtonClicked();
            }
        }
    }

    /// <summary>
    /// Display a specific slide with optional fade transition
    /// </summary>
    private void ShowSlide(int index, bool useFade = true)
    {
        if (isTransitioning) return;
        if (index < 0 || index >= slides.Count) return;

        currentSlideIndex = index;

        if (useFade && slideCanvasGroup != null)
        {
            StartCoroutine(ShowSlideWithFade(index));
        }
        else
        {
            DisplaySlideContent(index);
        }
    }

    /// <summary>
    /// Fade transition between slides
    /// </summary>
    private IEnumerator ShowSlideWithFade(int index)
    {
        isTransitioning = true;

        // Stop any existing auto-advance
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }

        // Fade out
        if (slideCanvasGroup != null)
        {
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                slideCanvasGroup.alpha = 1f - (timer / fadeDuration);
                yield return null;
            }
            slideCanvasGroup.alpha = 0f;
        }

        // Reset image transform before updating content
        if (slideImage != null)
        {
            RectTransform imageRect = slideImage.GetComponent<RectTransform>();
            if (imageRect != null)
            {
                imageRect.localScale = Vector3.one;
                imageRect.localRotation = Quaternion.identity;
            }
        }

        // Update content
        DisplaySlideContent(index);

        // Start image animation BEFORE fade-in so it's visible immediately
        if (enableImageAnimation && slideImage != null)
        {
            StartCoroutine(AnimateSlideImage());
        }

        // Fade in
        if (slideCanvasGroup != null)
        {
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                slideCanvasGroup.alpha = timer / fadeDuration;
                yield return null;
            }
            slideCanvasGroup.alpha = 1f;
        }

        isTransitioning = false;

        // Start auto-advance if enabled
        if (autoAdvance)
        {
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine());
        }
    }

    /// <summary>
    /// Update UI with slide content
    /// </summary>
    private void DisplaySlideContent(int index)
    {
        IntroSlide slide = slides[index];

        // Update image
        if (slideImage != null && slide.slideImage != null)
        {
            slideImage.sprite = slide.slideImage;
            slideImage.enabled = true;
        }
        else if (slideImage != null)
        {
            slideImage.enabled = false;
        }

        // Play audio
        if (audioSource != null && slide.slideAudio != null)
        {
            audioSource.Stop();
            audioSource.clip = slide.slideAudio;
            audioSource.Play();
        }

        // Update progress
        UpdateProgress();

        // Update button states
        UpdateButtonStates();

        // Start animation for first slide (when useFade is false)
        if (enableImageAnimation && slideImage != null && !isTransitioning)
        {
            StartCoroutine(AnimateSlideImage());
        }

        // Start auto-advance for first slide
        if (autoAdvance && !isTransitioning)
        {
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceCoroutine());
        }
    }

    /// <summary>
    /// Auto-advance to next slide after duration
    /// </summary>
    private IEnumerator AutoAdvanceCoroutine()
    {
        float duration = slides[currentSlideIndex].displayDuration;
        if (duration <= 0)
        {
            duration = defaultAutoAdvanceTime;
        }
        
        yield return new WaitForSeconds(duration);

        // Check if we're still on the same slide (user might have navigated)
        if (autoAdvance && !isTransitioning)
        {
            if (currentSlideIndex < slides.Count - 1)
            {
                ShowSlide(currentSlideIndex + 1);
            }
            else
            {
                // Last slide reached, load target scene
                OnSkipAllButtonClicked();
            }
        }
    }
    
    /// <summary>
    /// Dramatic zoom and tilt animation for slide images
    /// </summary>
    private IEnumerator AnimateSlideImage()
    {
        if (slideImage == null) yield break;
        
        RectTransform rectTransform = slideImage.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        // Reset to default
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * zoomIntensity;
        
        // Random direction for tilt (left or right)
        float startRotation = 0f;
        float endRotation = Random.Range(0, 2) == 0 ? -tiltIntensity : tiltIntensity;
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Smooth easing
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // Apply zoom
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, smoothT);
            
            // Apply tilt
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startRotation, endRotation, smoothT));
            
            yield return null;
        }
        
        // Ensure final values are set
        rectTransform.localScale = endScale;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, endRotation);
    }
    
    /// <summary>
    /// Start background music
    /// </summary>
    private void StartBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicClip != null)
        {
            backgroundMusicSource.clip = backgroundMusicClip;
            backgroundMusicSource.loop = true;
            backgroundMusicSource.Play();
        }
    }
    
    /// <summary>
    /// Stop background music
    /// </summary>
    private void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
        }
    }

    /// <summary>
    /// Update progress indicators
    /// </summary>
    private void UpdateProgress()
    {
        if (progressSlider != null)
        {
            progressSlider.maxValue = slides.Count - 1;
            progressSlider.value = currentSlideIndex;
        }
    }

    /// <summary>
    /// Enable/disable navigation buttons based on current position
    /// </summary>
    private void UpdateButtonStates()
    {
        if (nextButton != null)
        {
            nextButton.interactable = currentSlideIndex < slides.Count - 1;
        }
    }

    /// <summary>
    /// Go to next slide
    /// </summary>
    private void OnNextButtonClicked()
    {
        if (currentSlideIndex < slides.Count - 1)
        {
            if (autoAdvanceCoroutine != null)
                StopCoroutine(autoAdvanceCoroutine);
            
            ShowSlide(currentSlideIndex + 1);
        }
        else
        {
            // Last slide - go to target scene
            OnSkipAllButtonClicked();
        }
    }

    /// <summary>
    /// Skip current slide (go to next)
    /// </summary>
    private void OnSkipButtonClicked()
    {
        OnNextButtonClicked();
    }

    /// <summary>
    /// Skip all slides and go to target scene
    /// </summary>
    private void OnSkipAllButtonClicked()
    {
        Debug.Log($"Skipping intro, loading scene: {targetSceneName}");
        
        // Stop all audio
        if (audioSource != null)
            audioSource.Stop();
        
        StopBackgroundMusic();
        
        SceneManager.LoadScene(targetSceneName);
    }

    /// <summary>
    /// Validate setup and log warnings
    /// </summary>
    private void ValidateSetup()
    {
        if (slides == null || slides.Count == 0)
        {
            Debug.LogError("IntroSceneController: No slides configured! Add slides in the Inspector.");
        }
        else if (slides.Count < 12)
        {
            Debug.LogWarning($"IntroSceneController: Only {slides.Count} slides configured. Consider adding more (recommended minimum: 12)");
        }

        if (slideImage == null)
            Debug.LogError("IntroSceneController: Slide Image is not assigned!");
        
        if (audioSource == null)
            Debug.LogWarning("IntroSceneController: Audio Source is not assigned. Narration audio will not play.");
        
        if (backgroundMusicSource == null)
            Debug.LogWarning("IntroSceneController: Background Music Source is not assigned. Background music will not play.");
        
        if (backgroundMusicClip == null)
            Debug.LogWarning("IntroSceneController: Background Music Clip is not assigned.");
        
        if (string.IsNullOrEmpty(targetSceneName))
            Debug.LogError("IntroSceneController: Target Scene Name is empty!");
        
        if (autoAdvance)
            Debug.Log($"IntroSceneController: Auto-advance enabled. Slides will advance every {defaultAutoAdvanceTime} seconds (or custom duration per slide).");
    }

    /// <summary>
    /// Public method to jump to specific slide (for external controls)
    /// </summary>
    public void JumpToSlide(int index)
    {
        if (index >= 0 && index < slides.Count)
        {
            if (autoAdvanceCoroutine != null)
                StopCoroutine(autoAdvanceCoroutine);
            
            ShowSlide(index);
        }
    }

    /// <summary>
    /// Get total number of slides
    /// </summary>
    public int GetTotalSlides() => slides.Count;

    /// <summary>
    /// Get current slide index
    /// </summary>
    public int GetCurrentSlideIndex() => currentSlideIndex;

    private void OnDestroy()
    {
        // Clean up listeners
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
        
        if (skipButton != null)
            skipButton.onClick.RemoveListener(OnSkipButtonClicked);
        
        if (skipAllButton != null)
            skipAllButton.onClick.RemoveListener(OnSkipAllButtonClicked);

        if (autoAdvanceCoroutine != null)
            StopCoroutine(autoAdvanceCoroutine);
        
        // Stop background music
        StopBackgroundMusic();
    }
}
