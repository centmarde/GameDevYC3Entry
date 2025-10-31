using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Plays an audio source only when specific scenes are loaded.
/// The audio will automatically play/stop based on the active scene.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SceneSpecificAudioSource : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("List of scene names where this audio should play")]
    [SerializeField] private string[] allowedScenes;

    [Header("Audio Settings")]
    [Tooltip("Should the audio loop when playing?")]
    [SerializeField] private bool loopAudio = true;

    [Tooltip("Volume of the audio (0-1)")]
    [SerializeField] [Range(0f, 1f)] private float volume = 1f;

    [Tooltip("Should audio start playing immediately when entering a valid scene?")]
    [SerializeField] private bool playOnSceneLoad = true;

    [Tooltip("Fade in duration in seconds (0 = no fade)")]
    [SerializeField] private float fadeInDuration = 0f;

    [Tooltip("Fade out duration in seconds (0 = no fade)")]
    [SerializeField] private float fadeOutDuration = 0f;

    private AudioSource audioSource;
    private bool isValidScene = false;
    private float targetVolume;
    private float fadeTimer = 0f;
    private bool isFading = false;
    
    private static SceneSpecificAudioSource instance;

    private void Awake()
    {
        // Singleton pattern - only allow one instance
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"[SceneSpecificAudioSource] Duplicate instance found on {gameObject.name}. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = loopAudio;
        audioSource.playOnAwake = false;
        targetVolume = volume;

        // Make this object persist across scenes
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void Start()
    {
        // Check current scene on start
        CheckCurrentScene();
    }

    private void Update()
    {
        // Handle fading
        if (isFading)
        {
            fadeTimer += Time.deltaTime;
            float fadeDuration = isValidScene ? fadeInDuration : fadeOutDuration;

            if (fadeDuration > 0)
            {
                float t = Mathf.Clamp01(fadeTimer / fadeDuration);
                audioSource.volume = isValidScene ? Mathf.Lerp(0f, targetVolume, t) : Mathf.Lerp(targetVolume, 0f, t);

                if (t >= 1f)
                {
                    isFading = false;
                    if (!isValidScene && audioSource.isPlaying)
                    {
                        audioSource.Stop();
                    }
                }
            }
            else
            {
                isFading = false;
                audioSource.volume = isValidScene ? targetVolume : 0f;
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckCurrentScene();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // Optionally check if we need to stop audio when a scene unloads
        CheckCurrentScene();
    }

    private void CheckCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        bool shouldPlay = IsSceneAllowed(currentSceneName);
        bool wasValidScene = isValidScene;

        isValidScene = shouldPlay;

        // Only play if transitioning from invalid to valid scene
        // Don't restart if already playing in another allowed scene
        if (isValidScene && !wasValidScene)
        {
            PlayAudio();
        }
        // Only stop if transitioning from valid to invalid scene
        else if (!isValidScene && wasValidScene)
        {
            StopAudio();
        }
        // If both scenes are valid, just continue playing without restarting
    }

    private bool IsSceneAllowed(string sceneName)
    {
        if (allowedScenes == null || allowedScenes.Length == 0)
        {
            Debug.LogWarning($"[SceneSpecificAudioSource] No scenes specified in allowedScenes array on {gameObject.name}");
            return false;
        }

        foreach (string allowedScene in allowedScenes)
        {
            if (allowedScene.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void PlayAudio()
    {
        if (!playOnSceneLoad) return;

        if (fadeInDuration > 0)
        {
            audioSource.volume = 0f;
            fadeTimer = 0f;
            isFading = true;
        }
        else
        {
            audioSource.volume = targetVolume;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        Debug.Log($"[SceneSpecificAudioSource] Playing audio on {SceneManager.GetActiveScene().name}");
    }

    private void StopAudio()
    {
        if (fadeOutDuration > 0)
        {
            fadeTimer = 0f;
            isFading = true;
        }
        else
        {
            audioSource.Stop();
            audioSource.volume = 0f;
        }

        Debug.Log($"[SceneSpecificAudioSource] Stopping audio on {SceneManager.GetActiveScene().name}");
    }

    /// <summary>
    /// Manually play the audio if in a valid scene
    /// </summary>
    public void ManualPlay()
    {
        if (isValidScene)
        {
            PlayAudio();
        }
    }

    /// <summary>
    /// Manually stop the audio
    /// </summary>
    public void ManualStop()
    {
        StopAudio();
    }

    /// <summary>
    /// Check if the current scene is in the allowed list
    /// </summary>
    public bool IsCurrentSceneAllowed()
    {
        return isValidScene;
    }

    /// <summary>
    /// Add a scene to the allowed scenes list at runtime
    /// </summary>
    public void AddAllowedScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        System.Array.Resize(ref allowedScenes, allowedScenes.Length + 1);
        allowedScenes[allowedScenes.Length - 1] = sceneName;
        CheckCurrentScene();
    }

    /// <summary>
    /// Remove a scene from the allowed scenes list at runtime
    /// </summary>
    public void RemoveAllowedScene(string sceneName)
    {
        if (allowedScenes == null || allowedScenes.Length == 0) return;

        int index = System.Array.FindIndex(allowedScenes, s => s.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            string[] newArray = new string[allowedScenes.Length - 1];
            int newIndex = 0;
            for (int i = 0; i < allowedScenes.Length; i++)
            {
                if (i != index)
                {
                    newArray[newIndex++] = allowedScenes[i];
                }
            }
            allowedScenes = newArray;
            CheckCurrentScene();
        }
    }

    /// <summary>
    /// Update the volume at runtime
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        targetVolume = volume;
        if (!isFading && isValidScene)
        {
            audioSource.volume = targetVolume;
        }
    }
}
