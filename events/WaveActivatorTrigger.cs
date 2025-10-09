using UnityEngine;

/// <summary>
/// Attach this to a GameObject with a Collider (set as Trigger).
/// When the player collides with it, the wave system will be activated.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WaveActivatorTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Auto-find WaveManager if not assigned")]
    [SerializeField] private WaveManager waveManager;
    
    [Header("Trigger Behavior")]
    [SerializeField] private bool activateOnce = true;
    [Tooltip("Destroy this trigger object after activation")]
    [SerializeField] private bool destroyAfterActivation = false;
    [SerializeField] private float destroyDelay = 0.5f;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showDebugMessage = true;
    [SerializeField] private GameObject visualEffect; // Optional particle effect or visual
    
    private bool hasActivated = false;
    
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
            // Check if already activated (if activateOnce is true)
            if (activateOnce && hasActivated)
            {
                return;
            }
            
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
        
        hasActivated = true;
        
        if (showDebugMessage)
        {
            Debug.Log($"WaveActivatorTrigger: Player entered trigger '{gameObject.name}' - Activating waves!");
        }
        
        // Activate the wave system
        waveManager.ActivateWaves();
        
        // Show visual effect if assigned
        if (visualEffect != null)
        {
            visualEffect.SetActive(true);
        }
        
        // Destroy this trigger if configured
        if (destroyAfterActivation)
        {
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
