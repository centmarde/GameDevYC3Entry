using UnityEngine;

/// <summary>
/// Helper component to validate and ensure Animator component stays intact.
/// Attach this to your Player prefabs to help debug animator issues in builds.
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerAnimatorValidator : MonoBehaviour
{
    [Header("Validation Settings")]
    [SerializeField] private bool validateOnEnable = true;
    [SerializeField] private bool validateOnStart = true;
    [SerializeField] private bool continuousValidation = false;
    [SerializeField] private float validationInterval = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Player player;
    private Animator cachedAnimator;
    private float lastValidationTime;
    
    private void Awake()
    {
        player = GetComponent<Player>();
        CacheAnimator();
    }
    
    private void OnEnable()
    {
        if (validateOnEnable)
        {
            ValidateAnimator("OnEnable");
        }
    }
    
    private void Start()
    {
        if (validateOnStart)
        {
            ValidateAnimator("Start");
        }
    }
    
    private void Update()
    {
        if (continuousValidation && Time.time - lastValidationTime >= validationInterval)
        {
            ValidateAnimator("Update");
            lastValidationTime = Time.time;
        }
    }
    
    private void CacheAnimator()
    {
        if (cachedAnimator == null)
        {
            cachedAnimator = GetComponentInChildren<Animator>();
            if (showDebugLogs)
            {
                if (cachedAnimator != null)
                {
                    Debug.Log($"[PlayerAnimatorValidator] Cached Animator: {cachedAnimator.gameObject.name} on {gameObject.name}");
                }
                else
                {
                    Debug.LogError($"[PlayerAnimatorValidator] Failed to cache Animator on {gameObject.name}!", gameObject);
                }
            }
        }
    }
    
    private void ValidateAnimator(string context)
    {
        if (player == null)
        {
            Debug.LogError($"[PlayerAnimatorValidator] Player component is NULL! Context: {context}", gameObject);
            return;
        }
        
        if (player.anim == null)
        {
            Debug.LogError($"[PlayerAnimatorValidator] Player.anim is NULL! Context: {context}. Attempting recovery...", gameObject);
            
            // Call the public ReinitializeAnimator method to fix it
            player.ReinitializeAnimator();
            
            // Verify it worked
            if (player.anim != null)
            {
                Debug.Log($"[PlayerAnimatorValidator] ✅ Successfully recovered animator: {player.anim.gameObject.name}");
                cachedAnimator = player.anim; // Update cache
            }
            else
            {
                Debug.LogError($"[PlayerAnimatorValidator] ❌ FAILED to recover Animator! Context: {context}", gameObject);
                LogHierarchy();
            }
        }
        else if (showDebugLogs)
        {
            Debug.Log($"[PlayerAnimatorValidator] Animator OK in {context}: {player.anim.gameObject.name}");
        }
    }
    
    private void LogHierarchy()
    {
        Debug.Log($"[PlayerAnimatorValidator] GameObject Hierarchy for {gameObject.name}:");
        Debug.Log($"  - Active: {gameObject.activeInHierarchy}");
        Debug.Log($"  - ActiveSelf: {gameObject.activeSelf}");
        Debug.Log($"  - Children Count: {transform.childCount}");
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Animator childAnim = child.GetComponent<Animator>();
            Debug.Log($"  - Child {i}: {child.name} (Active: {child.gameObject.activeInHierarchy}, Has Animator: {childAnim != null})");
        }
        
        Animator[] allAnimators = GetComponentsInChildren<Animator>(true);
        Debug.Log($"  - Total Animators found (including inactive): {allAnimators.Length}");
        foreach (var anim in allAnimators)
        {
            Debug.Log($"    - Animator on: {anim.gameObject.name} (Active: {anim.gameObject.activeInHierarchy})");
        }
    }
    
    /// <summary>
    /// Call this method to manually trigger validation (useful for debugging)
    /// </summary>
    public void ManualValidation()
    {
        ValidateAnimator("ManualValidation");
    }
}
