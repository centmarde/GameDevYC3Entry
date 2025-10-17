using UnityEngine;

public class ResetPlayerData : MonoBehaviour
{
    [Header("Player Data Reference")]
    [Tooltip("Drag the Player_DataSO asset you want to reset")]
    public Player_DataSO playerData;
    
    [Header("Skill Data Reference")]
    [Tooltip("Drag the PlayerSkill_DataSO asset you want to reset (optional)")]
    public PlayerSkill_DataSO skillData;
    
    [Header("Skill Component Reference")]
    [Tooltip("If not assigned, will auto-find PlayerSkill_CirclingProjectiles on Player")]
    public PlayerSkill_CirclingProjectiles circlingProjectilesSkill;
    
    [Header("Default Values")]
    [SerializeField] private float defaultMaxHealth = 100f;
    [SerializeField] private float defaultProjectileSpeed = 10f;
    [SerializeField] private float defaultProjectileDamage = 25f;
    [SerializeField] private float defaultMeleeAttackRange = 1.5f;
    [SerializeField] private float defaultRangeAttackRange = 6f;
    [SerializeField] private float defaultDeathDelay = 0.1f;
    [SerializeField] private float defaultMoveSpeed = 5f;
    [SerializeField] private float defaultTurnSpeed = 1000f;
    [SerializeField] private float defaultCurrentSpeedMultiplier = 1.0f;
    [SerializeField] private float defaultCriticalChance = 5f;
    [SerializeField] private float defaultCriticalDamageMultiplier = 2f;
    [SerializeField] private float defaultEvasionChance = 1f;
    
    [Header("Skill Default Values")]
    [SerializeField] private bool defaultSkillIsObtained = false;
    [SerializeField] private int defaultProjectileCount = 2;
    [SerializeField] private float defaultSkillProjectileDamage = 2f;
    [SerializeField] private float defaultOrbitRadius = 2f;
    [SerializeField] private float defaultOrbitSpeed = 90f;
    
    [Header("Trigger Settings")]
    [SerializeField] private bool resetOnTriggerEnter = true;
    [SerializeField] private bool resetOnTriggerStay = false;
    [SerializeField] private bool resetOnTriggerExit = false;
    [SerializeField] private string targetTag = "Player"; // Only reset when this tag enters
    
    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;
    
    [Header("Collider Behavior")]
    [SerializeField] private bool makeColliderSolidAfterReset = true;
    [Tooltip("If true, the collider will become solid (non-trigger) after the first reset")]
    
    private Collider triggerCollider;
    private bool hasBeenTriggered = false;
    
    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        
        if (triggerCollider == null)
        {
            Debug.LogError("ResetPlayerData: No Collider component found on this GameObject!");
        }
        else if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning("ResetPlayerData: Collider is not set as trigger. Setting it to trigger mode.");
            triggerCollider.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (resetOnTriggerEnter && ShouldReset(other))
        {
            ResetData();
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (resetOnTriggerStay && ShouldReset(other))
        {
            ResetData();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (resetOnTriggerExit && ShouldReset(other))
        {
            ResetData();
        }
    }
    
    private bool ShouldReset(Collider other)
    {
        // If no tag is specified, reset for any collider
        if (string.IsNullOrEmpty(targetTag))
            return true;
            
        return other.CompareTag(targetTag);
    }
    
    /// <summary>
    /// Resets the Player_DataSO to default values
    /// </summary>
    public void ResetData()
    {
        if (playerData == null)
        {
            if (showDebugMessages)
                Debug.LogWarning("ResetPlayerData: No Player_DataSO assigned!");
            return;
        }
        
        // Only reset if it hasn't been triggered before (prevents multiple resets)
        if (hasBeenTriggered)
        {
            if (showDebugMessages)
                Debug.Log("ResetPlayerData: Already triggered. Collider is now solid.");
            return;
        }
        
        playerData.maxHealth = defaultMaxHealth;
        playerData.projectileSpeed = defaultProjectileSpeed;
        playerData.projectileDamage = defaultProjectileDamage;
        playerData.meleeAttackRange = defaultMeleeAttackRange;
        playerData.rangeAttackRange = defaultRangeAttackRange;
        playerData.deathDelay = defaultDeathDelay;
        playerData.moveSpeed = defaultMoveSpeed;
        playerData.turnSpeed = defaultTurnSpeed;
        playerData.currentSpeedMultiplier = defaultCurrentSpeedMultiplier;
        playerData.criticalChance = defaultCriticalChance;
        playerData.criticalDamageMultiplier = defaultCriticalDamageMultiplier;
        playerData.evasionChance = defaultEvasionChance;
        
        // Reset skill data if assigned (only resets base stats, not isObtained)
        if (skillData != null)
        {
            skillData.defaultProjectileCount = defaultProjectileCount;
            skillData.projectileDamage = defaultSkillProjectileDamage;
            skillData.orbitRadius = defaultOrbitRadius;
            skillData.orbitSpeed = defaultOrbitSpeed;
            
            if (showDebugMessages)
                Debug.Log($"ResetPlayerData: Skill '{skillData.skillName}' base stats reset - " +
                         $"Projectiles: {defaultProjectileCount}, Damage: {defaultSkillProjectileDamage}, " +
                         $"Radius: {defaultOrbitRadius}, Speed: {defaultOrbitSpeed}");
        }
        
        // Reset skill component if assigned or auto-find it
        if (circlingProjectilesSkill == null)
        {
            // Try to auto-find on player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                circlingProjectilesSkill = playerObj.GetComponent<PlayerSkill_CirclingProjectiles>();
            }
        }
        
        if (circlingProjectilesSkill != null)
        {
            circlingProjectilesSkill.ResetSkill(); // This resets isObtained back to the Inspector value
            
            if (showDebugMessages)
                Debug.Log($"ResetPlayerData: PlayerSkill_CirclingProjectiles component reset (isObtained reset to Inspector default: {defaultSkillIsObtained}).");
        }
        else if (showDebugMessages)
        {
            Debug.LogWarning("ResetPlayerData: Could not find PlayerSkill_CirclingProjectiles component to reset.");
        }
        
        if (showDebugMessages)
            Debug.Log($"ResetPlayerData: Player data has been reset to default values on {gameObject.name}");
        
        // Make the collider solid after reset
        if (makeColliderSolidAfterReset && triggerCollider != null)
        {
            triggerCollider.isTrigger = false;
            hasBeenTriggered = true;
            
            if (showDebugMessages)
                Debug.Log($"ResetPlayerData: Collider on {gameObject.name} is now solid. Player cannot pass through.");
        }
    }
    
    /// <summary>
    /// Manual reset method that can be called from other scripts or Unity Events
    /// </summary>
    public void ManualReset()
    {
        ResetData();
    }
}
