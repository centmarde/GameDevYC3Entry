using UnityEngine;

/// <summary>
/// ScriptableObject configuration for damage number audio and visual settings
/// Create this asset via: Assets > Create > Damage Number Settings
/// </summary>
[CreateAssetMenu(fileName = "DamageNumberSettings", menuName = "UI/Damage Number Settings", order = 1)]
public class DamageNumberSettings : ScriptableObject
{
    [Header("Sound Effects")]
    [Tooltip("Array of sounds for normal damage (plays randomly)")]
    public AudioClip[] normalDamageSounds;
    
    [Tooltip("Array of sounds for critical damage (plays randomly)")]
    public AudioClip[] criticalDamageSounds;
    
    [Tooltip("Array of sounds for healing (plays randomly)")]
    public AudioClip[] healSounds;
    
    [Tooltip("Volume for all damage number sounds (0-1)")]
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;
    
    /// <summary>
    /// Get a random normal damage sound from the array
    /// </summary>
    public AudioClip GetRandomNormalDamageSound()
    {
        if (normalDamageSounds == null || normalDamageSounds.Length == 0) return null;
        return normalDamageSounds[Random.Range(0, normalDamageSounds.Length)];
    }
    
    /// <summary>
    /// Get a random critical damage sound from the array
    /// </summary>
    public AudioClip GetRandomCriticalDamageSound()
    {
        if (criticalDamageSounds == null || criticalDamageSounds.Length == 0) return null;
        return criticalDamageSounds[Random.Range(0, criticalDamageSounds.Length)];
    }
    
    /// <summary>
    /// Get a random heal sound from the array
    /// </summary>
    public AudioClip GetRandomHealSound()
    {
        if (healSounds == null || healSounds.Length == 0) return null;
        return healSounds[Random.Range(0, healSounds.Length)];
    }
    
    [Header("Visual Settings (Optional Overrides)")]
    [Tooltip("Default color for normal damage numbers")]
    public Color normalColor = Color.white;
    
    [Tooltip("Default color for critical damage numbers")]
    public Color criticalColor = new Color(1f, 0.3f, 0f, 1f);
    
    [Tooltip("Default color for heal numbers")]
    public Color healColor = new Color(0.2f, 1f, 0.2f, 1f);
    
    private static DamageNumberSettings instance;
    
    /// <summary>
    /// Get the singleton instance of DamageNumberSettings
    /// Automatically loads from Resources folder
    /// </summary>
    public static DamageNumberSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<DamageNumberSettings>("DamageNumberSettings");
                
                if (instance == null)
                {
                    Debug.LogWarning("[DamageNumberSettings] No settings found in Resources folder! " +
                                   "Create one via: Assets > Create > UI > Damage Number Settings, " +
                                   "then move it to a Resources folder.");
                }
            }
            return instance;
        }
    }
}
