using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Piccolo FireCracker skill - throws explosive projectiles to random areas
/// that explode after a duration, damaging nearby enemies
/// Level 1-10: Progressive scaling of damage, radius, explosion timing, and bomb count
/// Independent skill with no ScriptableObject dependency
/// </summary>
public class PlayerSkill_PiccoloFireCracker : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private GameObject bombPrefab; // Optional - will auto-create if null
    [SerializeField] private Transform bombSpawnPoint; // Optional spawn point
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Auto-Created Bomb Settings")]
    [SerializeField] private bool autoCreateBomb = true;
    [SerializeField] private float bombSize = 0.5f;
    [SerializeField] private Color bombColor = new Color(1f, 0.3f, 0f, 1f); // Orange/red for firecracker
    
    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionEffectPrefab; // Optional explosion VFX prefab
    
    [Header("Arc Trace Effect")]
    [SerializeField] private bool enableArcTrace = true;
    [SerializeField] private Color traceColor = new Color(1f, 0.5f, 0f, 0.8f);
    [SerializeField] private float traceWidth = 0.1f;
    [SerializeField] private float traceDuration = 0.5f;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private AudioClip[] throwSounds; // Multiple throw sounds for variety
    [SerializeField] [Range(0f, 1f)] private float throwVolume = 0.7f;
    [SerializeField] [Range(0.8f, 1.2f)] private float throwPitchMin = 0.9f;
    [SerializeField] [Range(0.8f, 1.2f)] private float throwPitchMax = 1.1f;
    private AudioSource audioSource;
    
    [Header("Base Skill Settings")]
    [SerializeField] private float baseDamage = 20f;
    [SerializeField] private float baseAreaRadius = 5f;
    [SerializeField] private float baseExplosionTime = 3f;
    [SerializeField] private int baseBombCount = 2;
    [SerializeField] private float throwInterval = 5f; // Time between volleys
    
    [Header("Per-Level Upgrades (Percentage-based)")]
    [SerializeField] private float damagePercentPerLevel = 15f; // 15% damage increase per level
    [SerializeField] private float radiusPercentPerLevel = 10f; // 10% radius increase per level
    [SerializeField] private float explosionTimeReductionPerLevel = 0.22f; // Reduces explosion time by 0.22s per level
    [SerializeField] private float bombCountIncreasePerLevel = 0.33f; // +1 bomb every 3 levels (0.33 * 3 = 1)
    
    [Header("Max Limits")]
    [SerializeField] private float maxAreaRadius = 10f;
    [SerializeField] private float minExplosionTime = 1f;
    [SerializeField] private int maxBombCount = 5;
    
    [Header("Throwing Settings")]
    [SerializeField] private float throwRange = 15f; // How far bombs are thrown
    [SerializeField] private float throwArcHeight = 3f; // Arc height for projectile
    
    [Header("Visual Settings")]
    [SerializeField] private bool showDebugSphere = true;
    
    // State
    private bool isActive = false;
    [Header("Runtime State")]
    [SerializeField] private bool isObtained = false;
    private bool wasObtainedLastFrame = false;
    
    // Current stats (modified by upgrades)
    private float currentDamage;
    private float currentAreaRadius;
    private float currentExplosionTime;
    private int currentBombCount;
    
    // Level tracking (1-10)
    private int currentLevel = 0;
    private const int MAX_LEVEL = 10;
    
    // Throwing state
    private Coroutine throwingCoroutine;
    
    // Player reference
    private Player player;
    
    // Public accessors
    public bool IsObtained => isObtained;
    public int CurrentLevel => currentLevel;
    public int MaxLevel => MAX_LEVEL;
    public float CurrentDamage => currentDamage;
    public float CurrentAreaRadius => currentAreaRadius;
    public float CurrentExplosionTime => currentExplosionTime;
    public int CurrentBombCount => currentBombCount;
    
    private void Awake()
    {
        player = GetComponentInParent<Player>();
        
        if (player == null)
        {
            Debug.LogWarning("[PiccoloFireCracker] Player reference is null! Make sure this script is attached to a child of a Player GameObject.");
        }
        
        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.5f; // Slightly 3D for spatial awareness
        }
        
        InitializeStats();
    }
    
    private void Start()
    {
        if (isObtained)
        {
            currentLevel = 1;
            ApplyLevelStats();
            ActivateSkill();
        }
        wasObtainedLastFrame = isObtained;
    }
    
    private void Update()
    {
        // Check if isObtained was toggled in Inspector during Play Mode
        if (isObtained != wasObtainedLastFrame)
        {
            if (isObtained)
            {
                currentLevel = 1;
                ApplyLevelStats();
                ActivateSkill();
            }
            else
            {
                DeactivateSkill();
            }
            wasObtainedLastFrame = isObtained;
        }
    }
    
    /// <summary>
    /// Initialize skill stats to base values
    /// </summary>
    private void InitializeStats()
    {
        currentDamage = baseDamage;
        currentAreaRadius = baseAreaRadius;
        currentExplosionTime = baseExplosionTime;
        currentBombCount = baseBombCount;
    }
    
    /// <summary>
    /// Obtain the skill (sets to Level 1)
    /// </summary>
    public void ObtainSkill()
    {
        if (isObtained)
        {
            return;
        }
        
        isObtained = true;
        currentLevel = 1;
        ApplyLevelStats();
        ActivateSkill();
        
        Debug.Log($"[PiccoloFireCracker] Skill obtained at Level 1! Damage: {currentDamage}, Radius: {currentAreaRadius}, Bombs: {currentBombCount}");
    }
    
    /// <summary>
    /// Upgrade the skill to the next level
    /// </summary>
    public void UpgradeSkill()
    {
        if (!isObtained)
        {
            ObtainSkill();
            return;
        }
        
        if (currentLevel >= MAX_LEVEL)
        {
            Debug.LogWarning($"[PiccoloFireCracker] Already at MAX level ({MAX_LEVEL})");
            return;
        }
        
        currentLevel++;
        ApplyLevelStats();
        
        Debug.Log($"[PiccoloFireCracker] Upgraded to Level {currentLevel}! Damage: {currentDamage}, Radius: {currentAreaRadius}, Bombs: {currentBombCount}");
    }
    
    /// <summary>
    /// Apply stats based on current level using percentage-based scaling
    /// </summary>
    private void ApplyLevelStats()
    {
        // Damage: percentage-based increase
        currentDamage = baseDamage * (1f + (currentLevel - 1) * (damagePercentPerLevel / 100f));
        
        // Area Radius: percentage-based increase with max cap
        currentAreaRadius = Mathf.Min(
            baseAreaRadius * (1f + (currentLevel - 1) * (radiusPercentPerLevel / 100f)),
            maxAreaRadius
        );
        
        // Explosion Time: linear reduction with min cap
        currentExplosionTime = Mathf.Max(
            baseExplosionTime - (currentLevel - 1) * explosionTimeReductionPerLevel,
            minExplosionTime
        );
        
        // Bomb Count: increases every 3 levels with max cap
        currentBombCount = Mathf.Min(
            baseBombCount + Mathf.FloorToInt((currentLevel - 1) * bombCountIncreasePerLevel),
            maxBombCount
        );
        
        Debug.Log($"[PiccoloFireCracker] Level {currentLevel} - Damage: {currentDamage:F1}, Radius: {currentAreaRadius:F1}m, Time: {currentExplosionTime:F1}s, Bombs: {currentBombCount}");
    }
    
    /// <summary>
    /// Activate the skill
    /// </summary>
    public void ActivateSkill()
    {
        if (isActive) return;
        
        if (!isObtained)
        {
            return;
        }
        
        isActive = true;
        
        // Start throwing coroutine
        if (throwingCoroutine != null)
        {
            StopCoroutine(throwingCoroutine);
        }
        throwingCoroutine = StartCoroutine(ThrowBombsCoroutine());
        
        Debug.Log($"[PiccoloFireCracker] Skill activated!");
    }
    
    /// <summary>
    /// Deactivate the skill
    /// </summary>
    public void DeactivateSkill()
    {
        if (!isActive) return;
        
        isActive = false;
        
        if (throwingCoroutine != null)
        {
            StopCoroutine(throwingCoroutine);
            throwingCoroutine = null;
        }
    }
    
    /// <summary>
    /// Coroutine that throws bombs at intervals
    /// </summary>
    private IEnumerator ThrowBombsCoroutine()
    {
        while (isActive)
        {
            // Wait for throw interval
            yield return new WaitForSeconds(throwInterval);
            
            // Throw volley of bombs
            ThrowBombVolley();
        }
    }
    
    /// <summary>
    /// Throw a volley of bombs to random locations
    /// </summary>
    private void ThrowBombVolley()
    {
        if (player == null) return;
        
        Vector3 playerPos = player.transform.position;
        
        for (int i = 0; i < currentBombCount; i++)
        {
            // Generate random position within throw range
            Vector3 randomOffset = new Vector3(
                Random.Range(-throwRange, throwRange),
                0f,
                Random.Range(-throwRange, throwRange)
            );
            
            Vector3 targetPosition = playerPos + randomOffset;
            
            // Play throw sound
            PlayThrowSound();
            
            // Create and throw bomb
            StartCoroutine(ThrowBomb(playerPos + Vector3.up * 1.5f, targetPosition));
            
            // Small delay between throws for visual effect
            if (i < currentBombCount - 1)
            {
                // Add a small delay to stagger bomb throws
            }
        }
        
        Debug.Log($"[PiccoloFireCracker] Threw {currentBombCount} bombs!");
    }
    
    /// <summary>
    /// Throw a single bomb with arc trajectory
    /// </summary>
    private IEnumerator ThrowBomb(Vector3 startPos, Vector3 targetPos)
    {
        GameObject bombObj = CreateBomb(startPos);
        
        if (bombObj == null)
        {
            Debug.LogWarning("[PiccoloFireCracker] Failed to create bomb!");
            yield break;
        }
        
        // Create arc trace effect
        LineRenderer traceRenderer = null;
        if (enableArcTrace)
        {
            GameObject traceObj = new GameObject("BombTrace");
            traceRenderer = traceObj.AddComponent<LineRenderer>();
            traceRenderer.material = new Material(Shader.Find("Sprites/Default"));
            traceRenderer.startColor = traceColor;
            traceRenderer.endColor = new Color(traceColor.r, traceColor.g, traceColor.b, 0f);
            traceRenderer.startWidth = traceWidth;
            traceRenderer.endWidth = traceWidth * 0.5f;
            traceRenderer.positionCount = 20;
            traceRenderer.useWorldSpace = true;
            
            // Start fade coroutine
            StartCoroutine(FadeOutTrace(traceRenderer, traceDuration));
        }
        
        // Animate bomb trajectory
        float travelTime = 0.8f; // Time to reach target
        float elapsedTime = 0f;
        Vector3 previousPos = startPos;
        int traceIndex = 0;
        
        while (elapsedTime < travelTime)
        {
            if (bombObj == null) yield break;
            
            float t = elapsedTime / travelTime;
            
            // Lerp position with arc
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            
            // Add arc height using sine curve
            currentPos.y += Mathf.Sin(t * Mathf.PI) * throwArcHeight;
            
            bombObj.transform.position = currentPos;
            
            // Update trace renderer
            if (traceRenderer != null && traceIndex < traceRenderer.positionCount)
            {
                traceRenderer.SetPosition(traceIndex, currentPos);
                traceIndex++;
            }
            
            previousPos = currentPos;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Fill remaining trace positions
        if (traceRenderer != null)
        {
            for (int i = traceIndex; i < traceRenderer.positionCount; i++)
            {
                traceRenderer.SetPosition(i, targetPos);
            }
        }
        
        // Ensure bomb reaches target
        if (bombObj != null)
        {
            bombObj.transform.position = targetPos;
            
            // Initialize bomb explosion
            PiccoloFireCrackerBomb bomb = bombObj.GetComponent<PiccoloFireCrackerBomb>();
            if (bomb != null)
            {
                bomb.Initialize(currentExplosionTime, currentDamage, currentAreaRadius, player, enemyLayer, explosionEffectPrefab);
            }
        }
    }
    
    /// <summary>
    /// Fade out and destroy trace line renderer
    /// </summary>
    private IEnumerator FadeOutTrace(LineRenderer trace, float duration)
    {
        if (trace == null) yield break;
        
        float elapsed = 0f;
        Color startColor = trace.startColor;
        Color endColor = trace.endColor;
        
        while (elapsed < duration)
        {
            if (trace == null) yield break;
            
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration);
            
            trace.startColor = new Color(startColor.r, startColor.g, startColor.b, startColor.a * alpha);
            trace.endColor = new Color(endColor.r, endColor.g, endColor.b, endColor.a * alpha);
            
            yield return null;
        }
        
        if (trace != null)
        {
            Destroy(trace.gameObject);
        }
    }
    
    /// <summary>
    /// Create a bomb - either from prefab or auto-generate one
    /// </summary>
    private GameObject CreateBomb(Vector3 position)
    {
        GameObject bombObj;
        
        // Use prefab if assigned
        if (bombPrefab != null)
        {
            bombObj = Instantiate(bombPrefab, position, Quaternion.identity);
            return bombObj;
        }
        
        // Auto-create bomb if no prefab
        if (!autoCreateBomb)
        {
            Debug.LogWarning("[PiccoloFireCracker] No bomb prefab assigned and auto-create is disabled!");
            return null;
        }
        
        // Create base bomb object
        bombObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bombObj.name = "PiccoloFireCracker_Bomb";
        bombObj.transform.position = position;
        bombObj.transform.localScale = Vector3.one * bombSize;
        
        // Remove default collider (we'll add our own trigger)
        Destroy(bombObj.GetComponent<Collider>());
        
        // Make it glow
        Renderer renderer = bombObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetColor("_Color", bombColor);
            mat.SetColor("_EmissionColor", bombColor * 3f);
            mat.EnableKeyword("_EMISSION");
            renderer.material = mat;
        }
        
        // Add trigger collider for explosion detection
        SphereCollider col = bombObj.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.5f;
        
        // Add bomb script
        PiccoloFireCrackerBomb bomb = bombObj.AddComponent<PiccoloFireCrackerBomb>();
        
        return bombObj;
    }
    
    /// <summary>
    /// Reset the skill to its original state
    /// </summary>
    public void ResetSkill()
    {
        isObtained = false;
        currentLevel = 0;
        DeactivateSkill();
        InitializeStats();
    }
    
    private void OnDisable()
    {
        DeactivateSkill();
    }
    
    /// <summary>
    /// Play throw sound with pitch variation
    /// </summary>
    private void PlayThrowSound()
    {
        if (audioSource == null) return;
        
        AudioClip clipToPlay = null;
        
        // Use array if available, otherwise use single clip
        if (throwSounds != null && throwSounds.Length > 0)
        {
            clipToPlay = throwSounds[Random.Range(0, throwSounds.Length)];
        }
        else if (throwSound != null)
        {
            clipToPlay = throwSound;
        }
        
        if (clipToPlay != null)
        {
            // Random pitch for variety
            float randomPitch = Random.Range(throwPitchMin, throwPitchMax);
            audioSource.pitch = randomPitch;
            audioSource.PlayOneShot(clipToPlay, throwVolume);
        }
    }
    
    private void OnDestroy()
    {
        DeactivateSkill();
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (showDebugSphere && player != null && isObtained)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.transform.position, throwRange);
        }
    }
}
