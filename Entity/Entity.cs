using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public abstract class Entity : MonoBehaviour 
{
    [Header("Entity Components")]
    [SerializeField] protected Animator animatorOverride;
    
    public Animator anim { get; protected set; }
    public Rigidbody rb { get; private set; }
    
    // Public property to access animatorOverride (for editor tools)
    public Animator AnimatorOverride => animatorOverride;

    protected StateMachine stateMachine;


    protected virtual void Awake()
    {
        // Priority 1: Use serialized animator if assigned in Inspector
        if (animatorOverride != null)
        {
            anim = animatorOverride;
            Debug.Log($"[Entity] Using assigned Animator Override on {gameObject.name}: {anim.gameObject.name}", gameObject);
        }
        // Priority 2: Find in children (fallback)
        else
        {
            anim = GetComponentInChildren<Animator>();
            if (anim != null)
            {
                // Cache it to animatorOverride for future reference
                animatorOverride = anim;
                Debug.LogWarning($"[Entity] Animator Override was not assigned! Auto-cached: {anim.gameObject.name}. IMPORTANT: Please assign it in Inspector for builds!", gameObject);
            }
        }
        
        rb = GetComponent<Rigidbody>();
        stateMachine = new StateMachine();
        
        if (rb != null)
        {
            rb.useGravity = false;
        }
        
        // Validate animator was found
        if (anim == null)
        {
            Debug.LogError($"[Entity] CRITICAL: No Animator found on {gameObject.name} or its children! Animations will not work!", gameObject);
        }
        else
        {
            Debug.Log($"[Entity] Animator successfully initialized on {gameObject.name}: {anim.gameObject.name}", gameObject);
            ValidateAnimatorSetup();
        }
    }


    protected virtual void Start()
    {
        // Re-validate animator in Start (in case Awake failed)
        if (anim == null)
        {
            Debug.LogWarning($"[Entity] Animator was null in Start! Attempting to re-fetch for {gameObject.name}");
            ReinitializeAnimator();
        }
    }
    
    /// <summary>
    /// Public method to force re-initialization of the animator.
    /// Can be called by external systems if animator becomes null.
    /// </summary>
    public void ReinitializeAnimator()
    {
        if (animatorOverride != null)
        {
            anim = animatorOverride;
            Debug.Log($"[Entity] Reinitialized animator from override for {gameObject.name}");
        }
        else
        {
            anim = GetComponentInChildren<Animator>(true); // Include inactive
            if (anim != null)
            {
                animatorOverride = anim; // Cache it
                Debug.Log($"[Entity] Reinitialized animator by finding in children for {gameObject.name}");
            }
        }
        
        if (anim == null)
        {
            Debug.LogError($"[Entity] FAILED to reinitialize Animator for {gameObject.name}! Check prefab structure.", gameObject);
        }
        else
        {
            Debug.Log($"[Entity] Successfully reinitialized Animator for {gameObject.name}: {anim.gameObject.name}");
            ValidateAnimatorSetup();
        }
    }
    
    /// <summary>
    /// Validates that the animator is properly configured and enabled.
    /// </summary>
    private void ValidateAnimatorSetup()
    {
        if (anim == null) return;
        
        // Check if animator component is enabled
        if (!anim.enabled)
        {
            Debug.LogWarning($"[Entity] Animator component was disabled! Enabling it for {gameObject.name}", anim);
            anim.enabled = true;
        }
        
        // Check if animator has a controller
        if (anim.runtimeAnimatorController == null)
        {
            Debug.LogError($"[Entity] Animator is missing RuntimeAnimatorController on {anim.gameObject.name}! Animations will not play!", anim);
        }
        else
        {
            Debug.Log($"[Entity] Animator validation passed. Controller: {anim.runtimeAnimatorController.name}");
        }
        
        // Check if animator GameObject is active
        if (!anim.gameObject.activeInHierarchy)
        {
            Debug.LogError($"[Entity] Animator GameObject is not active in hierarchy: {anim.gameObject.name}", anim);
        }
    }

    protected virtual void Update()
    {
        // Safety check: Ensure animator is still valid
        if (anim == null && animatorOverride != null)
        {
            Debug.LogWarning($"[Entity] Animator became null during runtime! Re-initializing for {gameObject.name}");
            anim = animatorOverride;
        }
        
        stateMachine.UpdateActiveState();
    }


    public virtual void EntityDeath()
    {
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        
    }




}
