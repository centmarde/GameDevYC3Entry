using UnityEngine;

/// <summary>
/// Attach this to a GameObject with a Collider (set as Trigger).
/// When the player collides with it, the wave system will be activated and resumed if paused.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WaveActivatorTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Auto-find WaveManager if not assigned")]
    [SerializeField] private WaveManager waveManager;
    
    [Header("Trigger Behavior")]
    [Tooltip("Destroy this trigger object after activation")]
    [SerializeField] private bool destroyAfterActivation = false;
    [SerializeField] private float destroyDelay = 0.5f;
    
    [Header("Area Clear")]
    [Tooltip("If true, destroy the attached object when area is cleared")]
    [SerializeField] private bool areaClear = false;
    [Tooltip("The object to destroy when area is cleared (if not assigned, uses this GameObject)")]
    [SerializeField] private GameObject objectToDestroy;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showDebugMessage = true;
    [SerializeField] private GameObject visualEffect; // Optional particle effect or visual
    
    private void Awake()
    {
        // Auto-find WaveManager if not assigned
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<WaveManager>();
            
            if (waveManager == null)
            {
                Debug.LogError("WaveActivatorTrigger: No WaveManager found in scene!");
            }
        }
        
        // Ensure this object has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("WaveActivatorTrigger: Collider is not set as Trigger! Setting it now.");
            col.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if player collided
        if (other.CompareTag(playerTag))
        {
            ActivateWaves();
        }
    }
    
    /// <summary>
    /// Activate the wave system
    /// </summary>
    private void ActivateWaves()
    {
        if (waveManager == null)
        {
            Debug.LogError("WaveActivatorTrigger: Cannot activate waves - WaveManager is null!");
            return;
        }
        
        if (showDebugMessage)
        {
            Debug.Log($"WaveActivatorTrigger: Player entered trigger '{gameObject.name}' - Activating/Resuming waves!");
        }
        
        // Activate the wave system
        waveManager.ActivateWaves();
        
        // If waves are paused, automatically resume them
        if (waveManager.AreWavesPaused())
        {
            waveManager.ResumeWaves();
            
            if (showDebugMessage)
            {
                Debug.Log($"WaveActivatorTrigger: Waves were paused - automatically resuming!");
            }
        }
        
        // Show visual effect if assigned
        if (visualEffect != null)
        {
            visualEffect.SetActive(true);
        }
        
        // Area Clear: Destroy attached object if enabled
        if (areaClear)
        {
            GameObject targetToDestroy = objectToDestroy != null ? objectToDestroy : gameObject;
            
            if (showDebugMessage)
            {
                Debug.Log($"WaveActivatorTrigger: Area cleared! Destroying '{targetToDestroy.name}' in {destroyDelay} seconds...");
            }
            
            Destroy(targetToDestroy, destroyDelay);
            return; // Exit early since object will be destroyed
        }
        
        // Destroy only this specific trigger GameObject if configured
        // This will not affect other objects that may have this script attached
        if (destroyAfterActivation)
        {
            if (showDebugMessage)
            {
                Debug.Log($"WaveActivatorTrigger: Destroying trigger '{gameObject.name}' in {destroyDelay} seconds...");
            }
            Destroy(gameObject, destroyDelay);
        }
    }
    
    /// <summary>
    /// Manually activate (can be called from buttons, events, etc.)
    /// </summary>
    public void ManualActivate()
    {
        ActivateWaves();
    }
    
    private void OnDrawGizmos()
    {
        // Visualize the trigger area in editor
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Green transparent
            
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
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, "WAVE ACTIVATOR");
        #endif
    }
}
