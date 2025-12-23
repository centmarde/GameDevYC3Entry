using UnityEngine;

/// <summary>
/// Manages audio for wave events, particularly boss spawning
/// </summary>
[System.Serializable]
public class WaveAudioManager
{
    [Header("Audio Settings")]
    [Tooltip("Audio clip to play when a boss spawns")]
    [SerializeField] private AudioClip bossSpawnSound;
    [Tooltip("Volume for boss spawn sound")]
    [SerializeField] [Range(0f, 1f)] private float bossSpawnSoundVolume = 1f;
    
    private AudioSource audioSource;
    private GameObject parentObject;
    
    /// <summary>
    /// Initialize the audio manager
    /// </summary>
    public void Initialize(GameObject parent)
    {
        parentObject = parent;
        SetupAudioSource();
    }
    
    /// <summary>
    /// Setup AudioSource component
    /// </summary>
    private void SetupAudioSource()
    {
        if (parentObject == null) return;
        
        audioSource = parentObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = parentObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }
    
    /// <summary>
    /// Play boss spawn sound
    /// </summary>
    public void PlayBossSpawnSound()
    {
        if (audioSource != null && bossSpawnSound != null)
        {
            audioSource.PlayOneShot(bossSpawnSound, bossSpawnSoundVolume);
            Debug.Log($"[WaveAudioManager] Playing boss spawn audio: {bossSpawnSound.name}");
        }
        else
        {
            if (audioSource == null)
                Debug.LogWarning("[WaveAudioManager] AudioSource is null - cannot play boss spawn sound");
            if (bossSpawnSound == null)
                Debug.LogWarning("[WaveAudioManager] Boss spawn sound clip is null");
        }
    }
    
    /// <summary>
    /// Check if boss spawn sound is available
    /// </summary>
    public bool HasBossSpawnSound()
    {
        return bossSpawnSound != null && audioSource != null;
    }
    
    /// <summary>
    /// Set boss spawn sound clip
    /// </summary>
    public void SetBossSpawnSound(AudioClip clip)
    {
        bossSpawnSound = clip;
    }
    
    /// <summary>
    /// Set boss spawn sound volume
    /// </summary>
    public void SetBossSpawnVolume(float volume)
    {
        bossSpawnSoundVolume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// Play a one-shot audio clip with specified volume
    /// </summary>
    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
            Debug.Log($"[WaveAudioManager] Playing audio clip: {clip.name}");
        }
    }
    
    /// <summary>
    /// Stop all audio
    /// </summary>
    public void StopAllAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
    
    /// <summary>
    /// Reset audio manager
    /// </summary>
    public void Reset()
    {
        StopAllAudio();
        Debug.Log("[WaveAudioManager] Audio manager reset");
    }
    
    // Getters for inspector access
    public AudioClip GetBossSpawnSound() => bossSpawnSound;
    public float GetBossSpawnSoundVolume() => bossSpawnSoundVolume;
    public AudioSource GetAudioSource() => audioSource;
}