using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player2))]
public class Player2_BlinkSkill : MonoBehaviour
{
    private Player2 player2;
    private Rigidbody rb;
    
    [Header("Blink Settings")]
    private float cooldownTimer;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject blinkStartVFX;
    [SerializeField] private GameObject blinkEndVFX;
    [SerializeField] private TrailRenderer blinkTrail;
    
    [Header("Blink Path Particle Effect")]
    [SerializeField] private GameObject blinkPathParticlePrefab;
    [SerializeField] private LineRenderer blinkLineRenderer;
    [SerializeField] private float pathEffectDuration = 0.5f;
    [SerializeField] private int pathParticleCount = 10;
    [SerializeField] private Color lineColor = new Color(0.5f, 0.8f, 1f, 1f); // Cyan/blue color
    [SerializeField] private float lineWidth = 0.2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip blinkSound;
    private AudioSource audioSource;
    
    public bool IsOnCooldown => cooldownTimer > 0f;
    public float CooldownProgress => GetBlinkCooldown() > 0f ? Mathf.Clamp01(1f - (cooldownTimer / GetBlinkCooldown())) : 1f;
    
    // Dynamic getters that read from Stats in real-time
    private float GetBlinkDistance() => player2?.Stats?.blinkDistance ?? 5f;
    private float GetBlinkCooldown() => player2?.Stats?.blinkCooldown ?? 3f;
    
    private void Awake()
    {
        player2 = GetComponent<Player2>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Setup LineRenderer if not assigned
        if (blinkLineRenderer == null)
        {
            blinkLineRenderer = gameObject.AddComponent<LineRenderer>();
            ConfigureLineRenderer();
        }
    }
    
    private void ConfigureLineRenderer()
    {
        if (blinkLineRenderer == null) return;
        
        blinkLineRenderer.enabled = false;
        blinkLineRenderer.positionCount = 2;
        blinkLineRenderer.startWidth = lineWidth;
        blinkLineRenderer.endWidth = lineWidth;
        blinkLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        blinkLineRenderer.startColor = lineColor;
        blinkLineRenderer.endColor = lineColor;
        blinkLineRenderer.numCapVertices = 5;
        blinkLineRenderer.numCornerVertices = 5;
    }
    
    private void Start()
    {
        // No need to cache values anymore - we read them dynamically from Stats
        // This allows upgrades to take effect in real-time
    }
    
    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f) cooldownTimer = 0f;
        }
    }
    
    /// <summary>
    /// Attempts to execute the blink skill.
    /// Note: This should only be called after the charge-up period (handled by Player2_BlinkState)
    /// </summary>
    /// <returns>True if blink was successfully executed, false otherwise</returns>
    public bool TryBlink()
    {
        if (IsOnCooldown)
        {
            Debug.Log($"[Player2_BlinkSkill] Blink is on cooldown! {cooldownTimer:F1}s remaining");
            return false;
        }
        
        ExecuteBlink();
        return true;
    }
    
    private void ExecuteBlink()
    {
        // Get current blink distance from Stats (allows real-time upgrades)
        float currentBlinkDistance = GetBlinkDistance();
        
        // Get the player's forward direction
        Vector3 blinkDirection = transform.forward.normalized;
        blinkDirection.y = 0f; // Keep blink on horizontal plane
        
        // Calculate target position
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + (blinkDirection * currentBlinkDistance);
        
        // Optional: Raycast to prevent blinking through walls
        if (Physics.Raycast(startPosition, blinkDirection, out RaycastHit hit, currentBlinkDistance, LayerMask.GetMask("Default", "Environment")))
        {
            // If we hit something, blink to just before the obstacle
            targetPosition = hit.point - (blinkDirection * 0.3f);
            Debug.Log($"[Player2_BlinkSkill] Obstacle detected, adjusting blink distance");
        }
        
        // Visual effect at start position
        if (blinkStartVFX != null)
        {
            Instantiate(blinkStartVFX, startPosition, Quaternion.identity);
        }
        
        // Play sound
        if (blinkSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(blinkSound);
        }
        
        // Create particle effect between start and end positions BEFORE teleporting
        CreateBlinkPathEffect(startPosition, targetPosition);
        
        // Teleport the player
        if (rb != null)
        {
            rb.MovePosition(targetPosition);
        }
        else
        {
            transform.position = targetPosition;
        }
        
        // Visual effect at end position
        if (blinkEndVFX != null)
        {
            Instantiate(blinkEndVFX, targetPosition, Quaternion.identity);
        }
        
        // Enable trail effect briefly
        if (blinkTrail != null)
        {
            StartCoroutine(BlinkTrailEffect());
        }
        
        // Start cooldown (get current cooldown value from Stats)
        cooldownTimer = GetBlinkCooldown();
        
        Debug.Log($"[Player2_BlinkSkill] Blinked {Vector3.Distance(startPosition, targetPosition):F1} units forward");
    }
    
    private void CreateBlinkPathEffect(Vector3 startPos, Vector3 endPos)
    {
        // Create line renderer effect
        if (blinkLineRenderer != null)
        {
            StartCoroutine(BlinkLineEffect(startPos, endPos));
        }
        
        // Create particle trail between positions
        if (blinkPathParticlePrefab != null)
        {
            StartCoroutine(SpawnBlinkPathParticles(startPos, endPos));
        }
    }
    
    private IEnumerator BlinkLineEffect(Vector3 startPos, Vector3 endPos)
    {
        if (blinkLineRenderer == null) yield break;
        
        // Enable and set line positions
        blinkLineRenderer.enabled = true;
        blinkLineRenderer.SetPosition(0, startPos);
        blinkLineRenderer.SetPosition(1, endPos);
        
        // Fade out the line over time
        float elapsed = 0f;
        Color startColor = lineColor;
        
        while (elapsed < pathEffectDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / pathEffectDuration);
            
            Color fadeColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            blinkLineRenderer.startColor = fadeColor;
            blinkLineRenderer.endColor = fadeColor;
            
            yield return null;
        }
        
        // Disable line renderer
        blinkLineRenderer.enabled = false;
    }
    
    private IEnumerator SpawnBlinkPathParticles(Vector3 startPos, Vector3 endPos)
    {
        if (blinkPathParticlePrefab == null) yield break;
        
        Vector3 direction = endPos - startPos;
        float distance = direction.magnitude;
        Vector3 normalizedDir = direction.normalized;
        
        // Spawn particles along the path
        for (int i = 0; i < pathParticleCount; i++)
        {
            float t = i / (float)(pathParticleCount - 1);
            Vector3 spawnPos = Vector3.Lerp(startPos, endPos, t);
            
            // Add slight random offset for more natural look
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f)
            );
            
            GameObject particle = Instantiate(blinkPathParticlePrefab, spawnPos + randomOffset, Quaternion.identity);
            
            // Auto-destroy particle after duration
            Destroy(particle, pathEffectDuration);
            
            // Small delay between spawns for trail effect
            yield return new WaitForSeconds(0.02f);
        }
    }
    
    private IEnumerator BlinkTrailEffect()
    {
        if (blinkTrail != null)
        {
            blinkTrail.enabled = true;
            blinkTrail.Clear();
            yield return new WaitForSeconds(0.3f);
            blinkTrail.enabled = false;
        }
    }
    
    /// <summary>
    /// Force reset the cooldown (for debugging or power-ups)
    /// </summary>
    public void ResetCooldown()
    {
        cooldownTimer = 0f;
        Debug.Log("[Player2_BlinkSkill] Cooldown reset");
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw blink distance preview (shows current upgraded distance)
        if (player2 != null && player2.Stats != null)
        {
            Gizmos.color = Color.cyan;
            float currentDistance = GetBlinkDistance();
            Vector3 blinkEnd = transform.position + (transform.forward * currentDistance);
            Gizmos.DrawLine(transform.position, blinkEnd);
            Gizmos.DrawWireSphere(blinkEnd, 0.5f);
            
            // Draw distance text
            UnityEditor.Handles.Label(blinkEnd, $"Blink: {currentDistance:F1}m");
        }
    }
}