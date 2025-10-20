using UnityEngine;

public class ResetPlayerData : MonoBehaviour
{
    [Header("Player Data Reference")]
    [Tooltip("Drag the Player_DataSO asset you want to reset")]
    public Player_DataSO playerData;
    
    [Header("Player2 Data Reference")]
    [Tooltip("Drag the Player2_DataSO asset you want to reset (for Player2)")]
    public Player2_DataSO player2Data;
    
    [Header("Skill Data Reference")]
    [Tooltip("Drag the PlayerSkill_DataSO asset you want to reset (optional)")]
    public PlayerSkill_DataSO skillData;
    
    [Header("Skill Component References")]
    [Tooltip("Array of circling projectile skills for Player1 (if not assigned, will auto-find)")]
    public PlayerSkill_CirclingProjectiles[] player1CirclingProjectilesSkills;
    
    [Tooltip("Array of circling projectile skills for Player2 (if not assigned, will auto-find)")]
    public PlayerSkill_CirclingProjectiles[] player2CirclingProjectilesSkills;
    
    [Header("Default Values")]
    [SerializeField] private float defaultMaxHealth = 100f;
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
    
    [Header("Player2 Specific Default Values")]
    [SerializeField] private float defaultDashDamageMultiplier = 2f;
    [SerializeField] private float defaultChargedMaxChargeTime = 2f;
    [SerializeField] private float defaultChargedMinChargeMultiplier = 1f;
    [SerializeField] private float defaultChargedMaxChargeMultiplier = 2.5f;
    [SerializeField] private float defaultDashAttackCooldown = 1.5f;
    [SerializeField] private float defaultDashAttackRadius = 1.5f;
    [SerializeField] private float defaultBlinkDistance = 5f;
    [SerializeField] private float defaultBlinkCooldown = 3f;
    [SerializeField] private float defaultBlinkDashSpeed = 50f;
    [SerializeField] private float defaultPlayer2TurnSpeed = 10f;
    
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
            return;
        }
        else if (!triggerCollider.isTrigger)
        {
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
    /// Resets the Player_DataSO or Player2_DataSO to default values
    /// </summary>
    public void ResetData()
    {
        if (playerData == null && player2Data == null)
        {
            if (showDebugMessages)
                Debug.LogWarning("[ResetPlayerData] No player data assigned!");
            return;
        }
        
        // Only reset if it hasn't been triggered before (prevents multiple resets)
        if (hasBeenTriggered)
        {
            if (showDebugMessages)
                Debug.Log("[ResetPlayerData] Already triggered, skipping reset");
            return;
        }
        
        // Reset Player1 data
        if (playerData != null)
        {
            playerData.maxHealth = defaultMaxHealth;
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
            
            if (showDebugMessages)
                Debug.Log("[ResetPlayerData] Player1 data reset to defaults");
            
            // Find and reset the actual Player1 entity in the scene
            Player player1Instance = FindObjectOfType<Player>();
            if (player1Instance != null && player1Instance.GetType() == typeof(Player)) // Make sure it's not Player2
            {
                Entity_Health health = player1Instance.GetComponent<Entity_Health>();
                if (health != null)
                {
                    health.SetMaxHealth(playerData.maxHealth);
                    if (showDebugMessages)
                        Debug.Log($"[ResetPlayerData] Player1 entity health reset to {playerData.maxHealth}");
                }
            }
        }
        
        // Reset Player2 data
        if (player2Data != null)
        {
            player2Data.maxHealth = defaultMaxHealth;
            player2Data.projectileDamage = defaultProjectileDamage;
            player2Data.meleeAttackRange = defaultMeleeAttackRange;
            player2Data.deathDelay = defaultDeathDelay;
            player2Data.moveSpeed = defaultMoveSpeed;
            player2Data.turnSpeed = defaultPlayer2TurnSpeed;
            player2Data.currentSpeedMultiplier = defaultCurrentSpeedMultiplier;
            player2Data.criticalChance = defaultCriticalChance;
            player2Data.criticalDamageMultiplier = defaultCriticalDamageMultiplier;
            player2Data.evasionChance = defaultEvasionChance;
            
            // Player2 specific stats
            player2Data.dashDamageMultiplier = defaultDashDamageMultiplier;
            player2Data.chargedMaxChargeTime = defaultChargedMaxChargeTime;
            player2Data.chargedMinChargeMultiplier = defaultChargedMinChargeMultiplier;
            player2Data.chargedMaxChargeMultiplier = defaultChargedMaxChargeMultiplier;
            player2Data.dashAttackCooldown = defaultDashAttackCooldown;
            player2Data.dashAttackRadius = defaultDashAttackRadius;
            player2Data.blinkDistance = defaultBlinkDistance;
            player2Data.blinkCooldown = defaultBlinkCooldown;
            player2Data.blinkDashSpeed = defaultBlinkDashSpeed;
            
            if (showDebugMessages)
                Debug.Log("[ResetPlayerData] Player2 data reset to defaults (including blink/dash settings)");
            
            // Find and reset the actual Player2 entity in the scene
            Player2 player2Instance = FindObjectOfType<Player2>();
            if (player2Instance != null)
            {
                Entity_Health health = player2Instance.GetComponent<Entity_Health>();
                if (health != null)
                {
                    health.SetMaxHealth(player2Data.maxHealth);
                    if (showDebugMessages)
                        Debug.Log($"[ResetPlayerData] Player2 entity health reset to {player2Data.maxHealth}");
                }
            }
        }
        
        // Reset skill data if assigned (only resets base stats, not isObtained)
        if (skillData != null)
        {
            skillData.defaultProjectileCount = defaultProjectileCount;
            skillData.projectileDamage = defaultSkillProjectileDamage;
            skillData.orbitRadius = defaultOrbitRadius;
            skillData.orbitSpeed = defaultOrbitSpeed;
        }
        
        // Reset Player1 circling projectiles skills
        if (playerData != null)
        {
            // Auto-find if array is empty or null
            if (player1CirclingProjectilesSkills == null || player1CirclingProjectilesSkills.Length == 0)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player1CirclingProjectilesSkills = playerObj.GetComponents<PlayerSkill_CirclingProjectiles>();
                }
            }
            
            if (player1CirclingProjectilesSkills != null && player1CirclingProjectilesSkills.Length > 0)
            {
                foreach (var skill in player1CirclingProjectilesSkills)
                {
                    if (skill != null)
                    {
                        skill.ResetSkill(); // This resets isObtained back to the Inspector value
                    }
                }
                
                if (showDebugMessages)
                    Debug.Log($"[ResetPlayerData] Reset {player1CirclingProjectilesSkills.Length} Player1 circling projectiles skills");
            }
        }
        
        // Reset Player2 circling projectiles skills
        if (player2Data != null)
        {
            // Auto-find if array is empty or null
            if (player2CirclingProjectilesSkills == null || player2CirclingProjectilesSkills.Length == 0)
            {
                Player2 player2Instance = FindObjectOfType<Player2>();
                if (player2Instance != null)
                {
                    player2CirclingProjectilesSkills = player2Instance.GetComponents<PlayerSkill_CirclingProjectiles>();
                }
            }
            
            if (player2CirclingProjectilesSkills != null && player2CirclingProjectilesSkills.Length > 0)
            {
                foreach (var skill in player2CirclingProjectilesSkills)
                {
                    if (skill != null)
                    {
                        skill.ResetSkill(); // This resets isObtained back to the Inspector value
                    }
                }
                
                if (showDebugMessages)
                    Debug.Log($"[ResetPlayerData] Reset {player2CirclingProjectilesSkills.Length} Player2 circling projectiles skills");
            }
        }
        
        // Make the collider solid after reset
        if (makeColliderSolidAfterReset && triggerCollider != null)
        {
            triggerCollider.isTrigger = false;
            hasBeenTriggered = true;
            
            if (showDebugMessages)
                Debug.Log("[ResetPlayerData] Collider set to solid (non-trigger)");
        }
        else
        {
            hasBeenTriggered = true;
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
