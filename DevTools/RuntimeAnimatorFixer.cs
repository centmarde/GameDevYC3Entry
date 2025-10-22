using UnityEngine;

/// <summary>
/// Runtime tool to automatically detect and fix animator issues on spawned Player instances.
/// This component can be attached to Player prefabs or added at runtime.
/// It will continuously monitor and fix any animator-related issues.
/// </summary>
[RequireComponent(typeof(Player))]
public class RuntimeAnimatorFixer : MonoBehaviour
{
    [Header("Auto-Fix Settings")]
    [SerializeField] private bool enableAutoFix = true;
    [SerializeField] private float checkInterval = 0.5f; // Check every 0.5 seconds
    
    [Header("Debug Settings")]
    [SerializeField] private bool verboseLogging = true;
    
    private Player player;
    private float nextCheckTime;
    private int fixAttempts = 0;
    private const int MaxFixAttempts = 5;
    
    private void Awake()
    {
        player = GetComponent<Player>();
        
        if (player == null)
        {
            Debug.LogError("[RuntimeAnimatorFixer] No Player component found!", gameObject);
            enabled = false;
            return;
        }
        
        // Immediate check on spawn
        CheckAndFixAnimator();
    }
    
    private void Start()
    {
        // Check again after Start (in case Awake had issues)
        CheckAndFixAnimator();
    }
    
    private void Update()
    {
        if (!enableAutoFix) return;
        
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            CheckAndFixAnimator();
        }
    }
    
    private void CheckAndFixAnimator()
    {
        if (player == null) return;
        
        bool hasIssue = false;
        
        // Check 1: Is animator reference null?
        if (player.anim == null)
        {
            hasIssue = true;
            if (verboseLogging)
            {
                Debug.LogWarning($"[RuntimeAnimatorFixer] Animator is NULL on {gameObject.name}! Attempting fix...", gameObject);
            }
            
            if (fixAttempts < MaxFixAttempts)
            {
                player.ReinitializeAnimator();
                fixAttempts++;
                
                if (player.anim != null)
                {
                    Debug.Log($"[RuntimeAnimatorFixer] ✅ Successfully fixed NULL animator (Attempt {fixAttempts})", gameObject);
                    fixAttempts = 0; // Reset counter on success
                }
                else
                {
                    Debug.LogError($"[RuntimeAnimatorFixer] ❌ Failed to fix animator (Attempt {fixAttempts}/{MaxFixAttempts})", gameObject);
                }
            }
            else
            {
                Debug.LogError($"[RuntimeAnimatorFixer] Max fix attempts reached! Cannot recover animator.", gameObject);
            }
        }
        
        // Check 2: Is animator component disabled?
        if (player.anim != null && !player.anim.enabled)
        {
            hasIssue = true;
            if (verboseLogging)
            {
                Debug.LogWarning($"[RuntimeAnimatorFixer] Animator component is DISABLED! Enabling it...", gameObject);
            }
            player.anim.enabled = true;
        }
        
        // Check 3: Is RuntimeAnimatorController missing?
        if (player.anim != null && player.anim.runtimeAnimatorController == null)
        {
            hasIssue = true;
            Debug.LogError($"[RuntimeAnimatorFixer] ❌ RuntimeAnimatorController is NULL! Cannot auto-fix this - please assign controller in prefab!", player.anim);
            
            // Try to log what animators are available
            Animator[] allAnimators = GetComponentsInChildren<Animator>(true);
            Debug.Log($"[RuntimeAnimatorFixer] Found {allAnimators.Length} Animator(s) in hierarchy:");
            foreach (var anim in allAnimators)
            {
                Debug.Log($"  - {anim.gameObject.name}: Controller={(anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "NULL")}");
            }
        }
        
        // Check 4: Is animator GameObject inactive?
        if (player.anim != null && !player.anim.gameObject.activeInHierarchy)
        {
            hasIssue = true;
            Debug.LogWarning($"[RuntimeAnimatorFixer] Animator GameObject is INACTIVE! Activating it...", gameObject);
            player.anim.gameObject.SetActive(true);
        }
        
        // Log success if no issues found
        if (!hasIssue && verboseLogging && fixAttempts == 0 && Time.time < 2f) // Only log once at startup
        {
            Debug.Log($"[RuntimeAnimatorFixer] ✅ Animator validation passed for {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Force an immediate check (useful for debugging)
    /// </summary>
    [ContextMenu("Force Check Animator")]
    public void ForceCheck()
    {
        Debug.Log($"[RuntimeAnimatorFixer] === Forcing animator check for {gameObject.name} ===");
        CheckAndFixAnimator();
    }
}
