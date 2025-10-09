using UnityEngine;

/// <summary>
/// Attach this to a GameObject with a Collider (set as Trigger).
/// When the player collides with it, the wave system will be paused or resumed.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WavePauseTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Auto-find WaveManager if not assigned")]
    [SerializeField] private WaveManager waveManager;
    
    [Header("Pause Behavior")]
    [SerializeField] private PauseBehavior pauseBehavior = PauseBehavior.Toggle;
    [Tooltip("Should the trigger work only once or multiple times")]
    [SerializeField] private bool canTriggerMultipleTimes = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showDebugMessage = true;
    [SerializeField] private GameObject pausedVisual; // Show when paused
    [SerializeField] private GameObject resumedVisual; // Show when resumed
    
    public enum PauseBehavior
    {
        Toggle,          // Each collision toggles pause/resume
        PauseOnly,       // Only pauses waves
        ResumeOnly,      // Only resumes waves
        PauseOnEnter,    // Pause when entering, resume when exiting
    }
    
    private bool hasTriggered = false;
    private bool playerInside = false;
    
    private void Awake()
    {
        // Auto-find WaveManager if not assigned
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
            
            if (waveManager == null)
            {
                Debug.LogError("WavePauseTrigger: No WaveManager found in scene!");
            }
        }
        
        // Ensure this object has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("WavePauseTrigger: Collider is not set as Trigger! Setting it now.");
            col.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if player collided
        if (other.CompareTag(playerTag))
        {
            // Check if can trigger multiple times
            if (!canTriggerMultipleTimes && hasTriggered)
            {
                return;
            }
            
            playerInside = true;
            hasTriggered = true;
            
            HandlePause();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if player left
        if (other.CompareTag(playerTag))
        {
            playerInside = false;
            
            // Handle PauseOnEnter behavior
            if (pauseBehavior == PauseBehavior.PauseOnEnter)
            {
                ResumeWaves();
            }
        }
    }
    
    /// <summary>
    /// Handle pause logic based on behavior setting
    /// </summary>
    private void HandlePause()
    {
        if (waveManager == null) return;
        
        switch (pauseBehavior)
        {
            case PauseBehavior.Toggle:
                if (waveManager.AreWavesPaused())
                {
                    ResumeWaves();
                }
                else
                {
                    PauseWaves();
                }
                break;
                
            case PauseBehavior.PauseOnly:
                PauseWaves();
                break;
                
            case PauseBehavior.ResumeOnly:
                ResumeWaves();
                break;
                
            case PauseBehavior.PauseOnEnter:
                PauseWaves();
                break;
        }
    }
    
    /// <summary>
    /// Pause the wave system
    /// </summary>
    private void PauseWaves()
    {
        if (waveManager == null) return;
        
        if (showDebugMessage)
        {
            Debug.Log($"WavePauseTrigger: Player triggered '{gameObject.name}' - PAUSING waves!");
        }
        
        waveManager.PauseWaves();
        
        // Update visuals
        if (pausedVisual != null)
            pausedVisual.SetActive(true);
        if (resumedVisual != null)
            resumedVisual.SetActive(false);
    }
    
    /// <summary>
    /// Resume the wave system
    /// </summary>
    private void ResumeWaves()
    {
        if (waveManager == null) return;
        
        if (showDebugMessage)
        {
            Debug.Log($"WavePauseTrigger: Player triggered '{gameObject.name}' - RESUMING waves!");
        }
        
        waveManager.ResumeWaves();
        
        // Update visuals
        if (pausedVisual != null)
            pausedVisual.SetActive(false);
        if (resumedVisual != null)
            resumedVisual.SetActive(true);
    }
    
    /// <summary>
    /// Manually pause (can be called from buttons, events, etc.)
    /// </summary>
    public void ManualPause()
    {
        PauseWaves();
    }
    
    /// <summary>
    /// Manually resume (can be called from buttons, events, etc.)
    /// </summary>
    public void ManualResume()
    {
        ResumeWaves();
    }
    
    /// <summary>
    /// Manually toggle (can be called from buttons, events, etc.)
    /// </summary>
    public void ManualToggle()
    {
        if (waveManager != null)
        {
            waveManager.TogglePause();
        }
    }
    
    private void OnDrawGizmos()
    {
        // Visualize the trigger area in editor
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange transparent
            
            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            }
        }
        
        // Draw label
        #if UNITY_EDITOR
        string label = pauseBehavior == PauseBehavior.PauseOnly ? "WAVE PAUSE" :
                       pauseBehavior == PauseBehavior.ResumeOnly ? "WAVE RESUME" :
                       "WAVE PAUSE/RESUME";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
        #endif
    }
}
