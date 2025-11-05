using UnityEngine;
using System;

/// <summary>
/// Manages player experience, levels, and the experience UI.
/// Singleton pattern for easy access from anywhere.
/// </summary>
public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Header("Experience Settings")]
    [Tooltip("Starting level for the player")]
    [SerializeField] private int startingLevel = 1;
    
    [Tooltip("Base experience required to reach level 2")]
    [SerializeField] private int baseExperienceRequired = 100;
    
    [Tooltip("Multiplier applied to experience requirement per level (exponential scaling)")]
    [SerializeField] private float experienceScaling = 1.5f;

    [Header("Current State")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExperience = 0;
    [SerializeField] private int experienceToNextLevel = 100;

    // Events
    public event Action<int> OnExperienceGained;
    public event Action<int> OnLevelUp;
    public event Action<int, int, int> OnExperienceChanged; // current, required, level

    private ExperienceUI experienceUI;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize
        currentLevel = startingLevel;
        currentExperience = 0;
        CalculateExperienceToNextLevel();

        Debug.Log($"[ExperienceManager] Initialized at Level {currentLevel}, need {experienceToNextLevel} XP to level up");
    }

    private void Start()
    {
        // Find or create the UI
        experienceUI = FindObjectOfType<ExperienceUI>();
        if (experienceUI == null)
        {
            Debug.LogWarning("[ExperienceManager] No ExperienceUI found in scene!");
        }
        else
        {
            // Update UI with initial values (start at 0 XP)
            Debug.Log($"[ExperienceManager] Initializing UI - Level: {currentLevel}, XP: {currentExperience}/{experienceToNextLevel}");
            UpdateUI();
        }
    }

    /// <summary>
    /// Add experience points to the player
    /// </summary>
    public void AddExperience(int amount)
    {
        if (amount <= 0) return;

        currentExperience += amount;
        OnExperienceGained?.Invoke(amount);

        Debug.Log($"[ExperienceManager] Gained {amount} XP. Current: {currentExperience}/{experienceToNextLevel}");

        // Check for level up
        while (currentExperience >= experienceToNextLevel)
        {
            LevelUp();
        }

        UpdateUI();
    }

    /// <summary>
    /// Level up the player
    /// </summary>
    private void LevelUp()
    {
        currentLevel++;
        currentExperience -= experienceToNextLevel;

        Debug.Log($"[ExperienceManager] LEVEL UP! Now level {currentLevel}");

        OnLevelUp?.Invoke(currentLevel);

        // Calculate new experience requirement
        CalculateExperienceToNextLevel();

        // Show level up effect/UI (you can expand this)
        ShowLevelUpEffect();
    }

    /// <summary>
    /// Calculate experience required for next level
    /// </summary>
    private void CalculateExperienceToNextLevel()
    {
        // Exponential scaling: each level requires more XP
        experienceToNextLevel = Mathf.RoundToInt(baseExperienceRequired * Mathf.Pow(experienceScaling, currentLevel - 1));
        Debug.Log($"[ExperienceManager] Level {currentLevel} -> {currentLevel + 1} requires {experienceToNextLevel} XP");
    }

    /// <summary>
    /// Update the UI with current experience values
    /// </summary>
    private void UpdateUI()
    {
        OnExperienceChanged?.Invoke(currentExperience, experienceToNextLevel, currentLevel);

        if (experienceUI != null)
        {
            experienceUI.UpdateExperienceBar(currentExperience, experienceToNextLevel, currentLevel);
        }
    }

    /// <summary>
    /// Show level up visual effect
    /// </summary>
    private void ShowLevelUpEffect()
    {
        if (experienceUI != null)
        {
            experienceUI.ShowLevelUpEffect();
        }

        // You can add more effects here (particles, sound, etc.)
    }

    /// <summary>
    /// Get current level
    /// </summary>
    public int GetCurrentLevel() => currentLevel;

    /// <summary>
    /// Get current experience
    /// </summary>
    public int GetCurrentExperience() => currentExperience;

    /// <summary>
    /// Get experience required for next level
    /// </summary>
    public int GetExperienceToNextLevel() => experienceToNextLevel;

    /// <summary>
    /// Get experience progress as a percentage (0-1)
    /// </summary>
    public float GetExperienceProgress()
    {
        return (float)currentExperience / experienceToNextLevel;
    }

    /// <summary>
    /// Reset experience and level (useful for new game)
    /// </summary>
    public void ResetExperience()
    {
        currentLevel = startingLevel;
        currentExperience = 0;
        CalculateExperienceToNextLevel();
        UpdateUI();
        Debug.Log("[ExperienceManager] Experience reset to starting values");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
