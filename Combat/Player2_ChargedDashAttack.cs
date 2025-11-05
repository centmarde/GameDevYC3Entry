using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player2))]
public class Player2_ChargedDashAttack : MonoBehaviour
{
    private Player2 player2;
    private Rigidbody rb;
    
    [Header("Charge Settings")]
    private float currentChargeTime;
    private bool isCharging;
    private bool isExecutingDash;
    
    [Header("Invulnerability Settings")]
    [Tooltip("Layer to switch player to during dash (should NOT be in enemy's damageableLayer)")]
    [SerializeField] private string invulnerableLayerName = "Ignore Raycast";
    private int originalLayer;
    private int invulnerableLayer;
    
    [Header("Dash Settings")]
    private Vector3 dashDirection;
    private float dashProgress;
    private Vector3 dashStartPosition;
    
    [Header("Damage Settings")]
    private float damageMultiplier = 1f;
    
    [Header("Projectile Settings")]
    [SerializeField] private ProjectileSlingshot dashProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    private ProjectileSlingshot activeDashProjectile;
    
    [Header("Dash Path Particle Effect")]
    [SerializeField] private GameObject dashPathParticlePrefab;
    [SerializeField] private LineRenderer dashLineRenderer;
    [SerializeField] private float pathEffectDuration = 0.8f;
    [SerializeField] private int pathParticleCount = 15;
    [SerializeField] private Color lineColor = new Color(1f, 0.5f, 0.2f, 1f); // Orange/red color
    [SerializeField] private float lineWidth = 0.3f;
    [SerializeField] private bool updateLineInRealtime = true;
    
    [Header("Cooldown")]
    private float cooldownTimer;
    
    public bool IsCharging => isCharging;
    public bool IsExecutingDash => isExecutingDash;
    public bool IsOnCooldown => cooldownTimer > 0f;
    public float ChargeProgress => Mathf.Clamp01(currentChargeTime / GetMaxChargeTime());
    public float CooldownProgress => GetDashCooldown() > 0f ? Mathf.Clamp01(1f - (cooldownTimer / GetDashCooldown())) : 1f;
    
    // Dynamic getters that read from Stats in real-time for upgrade support
    private float GetMaxChargeTime() => player2?.Stats?.chargedMaxChargeTime ?? 2f;
    private float GetDashDistance() => player2?.Stats?.blinkDistance ?? 5f;
    private float GetDashSpeed() => player2?.Stats?.blinkDashSpeed ?? 20f;
    private float GetBaseDamage() => player2?.Stats != null ? player2.Stats.projectileDamage * player2.Stats.dashDamageMultiplier : 20f;
    private float GetDashCooldown() => player2?.Stats?.dashAttackCooldown ?? 1.5f;
    
    private void Awake()
    {
        player2 = GetComponent<Player2>();
        rb = GetComponent<Rigidbody>();
        
        // If this is not Player2 or is Player1, disable this component
        if (player2 == null || !IsPlayer2Active())
        {
            Debug.Log($"[Player2_ChargedDashAttack] Disabling on {gameObject.name} - Not Player2 or Player2 not active");
            enabled = false;
            return;
        }
        
        // Cache the player's original layer and setup invulnerable layer
        originalLayer = gameObject.layer;
        invulnerableLayer = LayerMask.NameToLayer(invulnerableLayerName);
        
        if (invulnerableLayer == -1)
        {
            Debug.LogError($"[Player2_ChargedDashAttack] Layer '{invulnerableLayerName}' not found! Invulnerability will not work. Using 'Ignore Raycast' as fallback.", this);
            invulnerableLayer = LayerMask.NameToLayer("Ignore Raycast");
        }
        
        Debug.Log($"[Player2_ChargedDashAttack] Invulnerability setup: Original Layer={LayerMask.LayerToName(originalLayer)} ({originalLayer}), Invulnerable Layer={invulnerableLayerName} ({invulnerableLayer})");
        
        // Setup LineRenderer if not assigned
        if (dashLineRenderer == null)
        {
            // Create a child object for the line renderer
            GameObject lineObj = new GameObject("DashLineRenderer");
            lineObj.transform.SetParent(transform);
            lineObj.transform.localPosition = Vector3.zero;
            dashLineRenderer = lineObj.AddComponent<LineRenderer>();
            ConfigureLineRenderer();
        }
    }
    
    private void ConfigureLineRenderer()
    {
        if (dashLineRenderer == null) return;
        
        dashLineRenderer.enabled = false;
        dashLineRenderer.positionCount = 2;
        dashLineRenderer.startWidth = lineWidth;
        dashLineRenderer.endWidth = lineWidth;
        dashLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        dashLineRenderer.startColor = lineColor;
        dashLineRenderer.endColor = lineColor;
        dashLineRenderer.numCapVertices = 5;
        dashLineRenderer.numCornerVertices = 5;
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
    
    public void StartCharging(Vector3 direction)
    {
        if (IsOnCooldown || isExecutingDash) return;
        
        isCharging = true;
        currentChargeTime = 0f;
        dashDirection = direction.normalized;
        dashDirection.y = 0f;
        
        Debug.Log("[Player2_ChargedDashAttack] Started charging dash attack");
    }
    
    public void UpdateDashDirection(Vector3 direction)
    {
        if (!isCharging) return;
        
        // Continuously update dash direction based on mouse position
        dashDirection = direction.normalized;
        dashDirection.y = 0f;
    }
    
    public void TickCharge(float deltaTime)
    {
        if (!isCharging) return;
        
        float maxChargeTime = GetMaxChargeTime();
        currentChargeTime += deltaTime;
        if (currentChargeTime > maxChargeTime)
            currentChargeTime = maxChargeTime;
        
        // Calculate damage multiplier based on charge time
        float chargeRatio = currentChargeTime / GetMaxChargeTime();
        
        // Double damage when fully charged
        if (chargeRatio >= 1f)
        {
            damageMultiplier = player2.Stats.chargedMaxChargeMultiplier * 2f;
        }
        else
        {
            damageMultiplier = Mathf.Lerp(
                player2.Stats.chargedMinChargeMultiplier,
                player2.Stats.chargedMaxChargeMultiplier,
                chargeRatio
            );
        }
    }
    
    public void ReleaseCharge()
    {
        if (!isCharging) return;
        
        isCharging = false;
        
        // Execute the dash attack
        ExecuteDash();
    }
    
    public void CancelCharge()
    {
        isCharging = false;
        currentChargeTime = 0f;
        damageMultiplier = 1f;
    }
    
    private void ExecuteDash()
    {
        if (isExecutingDash) return;
        
        isExecutingDash = true;
        dashProgress = 0f;
        dashStartPosition = transform.position;
        
        // Start cooldown (get current cooldown value from Stats)
        cooldownTimer = GetDashCooldown();
        
        // Enable invulnerability - ignore collision with enemies
        EnableInvulnerability();
        
        // Spawn the dash projectile
        SpawnDashProjectile();
        
        // Calculate end position for particle effects (use current upgraded distance)
        float currentDashDistance = GetDashDistance();
        Vector3 dashEndPosition = dashStartPosition + dashDirection * currentDashDistance;
        
        // Create particle effect for the dash path
        CreateDashPathEffect(dashStartPosition, dashEndPosition);
        
        Debug.Log($"[Player2_ChargedDashAttack] Executing dash with {damageMultiplier}x damage multiplier");
        
        StartCoroutine(DashCoroutine());
    }
    
    private void SpawnDashProjectile()
    {
        if (dashProjectilePrefab == null)
        {
            Debug.LogWarning("[Player2_ChargedDashAttack] No dash projectile prefab assigned!");
            return;
        }
        
        // Get current base damage (recalculated for upgrades)
        float baseDamage = GetBaseDamage();
        
        // Calculate final damage
        float finalDamage = baseDamage * damageMultiplier;
        bool isCritical = player2.Stats.RollCriticalHit();
        finalDamage = player2.Stats.CalculateDamage(finalDamage, isCritical);
        
        // Spawn point (player position or custom spawn point)
        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        
        // Create the projectile
        activeDashProjectile = Instantiate(dashProjectilePrefab, spawnPos, Quaternion.identity);
        
        // The projectile moves with the player during dash, so minimal velocity
        Vector3 velocity = dashDirection * GetDashSpeed();
        activeDashProjectile.Launch(velocity, finalDamage, player2, isCritical);
        
        Debug.Log($"[Player2_ChargedDashAttack] Spawned dash projectile with {finalDamage} damage {(isCritical ? "(CRITICAL!)" : "")}");
    }
    
    private IEnumerator DashCoroutine()
    {
        // Get current upgraded values
        float dashDistance = GetDashDistance();
        float dashSpeed = GetDashSpeed();
        
        float elapsedTime = 0f;
        float dashDuration = dashDistance / dashSpeed;
        
        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / dashDuration);
            
            // Move the player
            Vector3 targetPosition = dashStartPosition + dashDirection * dashDistance * progress;
            rb.MovePosition(targetPosition);
            
            // Update projectile position to follow player
            if (activeDashProjectile != null)
            {
                activeDashProjectile.transform.position = transform.position;
            }
            
            // Update line renderer in realtime to show dash progress
            if (updateLineInRealtime && dashLineRenderer != null && dashLineRenderer.enabled)
            {
                dashLineRenderer.SetPosition(0, dashStartPosition);
                dashLineRenderer.SetPosition(1, transform.position);
            }
            
            yield return null;
        }
        
        // Ensure we reach exact end position
        Vector3 finalPosition = dashStartPosition + dashDirection * dashDistance;
        rb.MovePosition(finalPosition);
        
        // Destroy the dash projectile
        if (activeDashProjectile != null)
        {
            Destroy(activeDashProjectile.gameObject);
            activeDashProjectile = null;
        }
        
        // Disable invulnerability - restore collision with enemies
        DisableInvulnerability();
        
        isExecutingDash = false;
        damageMultiplier = 1f;
        
        Debug.Log("[Player2_ChargedDashAttack] Dash complete");
    }
    
    public void ForceStop()
    {
        if (isCharging)
        {
            CancelCharge();
        }
        
        if (isExecutingDash)
        {
            StopAllCoroutines();
            
            // Restore collision before ending dash
            DisableInvulnerability();
            
            isExecutingDash = false;
            damageMultiplier = 1f;
            
            // Destroy active projectile
            if (activeDashProjectile != null)
            {
                Destroy(activeDashProjectile.gameObject);
                activeDashProjectile = null;
            }
        }
    }
    
    private void CreateDashPathEffect(Vector3 startPos, Vector3 endPos)
    {
        // Create line renderer effect
        if (dashLineRenderer != null)
        {
            StartCoroutine(DashLineEffect(startPos, endPos));
        }
        
        // Create particle trail between positions
        if (dashPathParticlePrefab != null)
        {
            StartCoroutine(SpawnDashPathParticles(startPos, endPos));
        }
    }
    
    private IEnumerator DashLineEffect(Vector3 startPos, Vector3 endPos)
    {
        if (dashLineRenderer == null) yield break;
        
        // Enable and set line positions
        dashLineRenderer.enabled = true;
        dashLineRenderer.SetPosition(0, startPos);
        dashLineRenderer.SetPosition(1, endPos);
        
        // Wait for dash to complete (use current upgraded values)
        float dashDistance = GetDashDistance();
        float dashSpeed = GetDashSpeed();
        float dashDuration = dashDistance / dashSpeed;
        yield return new WaitForSeconds(dashDuration);
        
        // Fade out the line over time
        float elapsed = 0f;
        Color startColor = lineColor;
        
        while (elapsed < pathEffectDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / pathEffectDuration);
            
            Color fadeColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            dashLineRenderer.startColor = fadeColor;
            dashLineRenderer.endColor = fadeColor;
            
            yield return null;
        }
        
        // Disable line renderer
        dashLineRenderer.enabled = false;
        
        // Reset colors for next use
        dashLineRenderer.startColor = lineColor;
        dashLineRenderer.endColor = lineColor;
    }
    
    private IEnumerator SpawnDashPathParticles(Vector3 startPos, Vector3 endPos)
    {
        if (dashPathParticlePrefab == null) yield break;
        
        Vector3 direction = endPos - startPos;
        float distance = direction.magnitude;
        Vector3 normalizedDir = direction.normalized;
        
        // Spawn particles along the path with a delay for trail effect (use current values)
        float dashDistance = GetDashDistance();
        float dashSpeed = GetDashSpeed();
        float dashDuration = dashDistance / dashSpeed;
        float spawnDelay = dashDuration / pathParticleCount;
        
        for (int i = 0; i < pathParticleCount; i++)
        {
            float t = i / (float)(pathParticleCount - 1);
            Vector3 spawnPos = Vector3.Lerp(startPos, endPos, t);
            
            // Add slight random offset for more natural look
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.15f, 0.15f),
                Random.Range(-0.15f, 0.15f),
                Random.Range(-0.15f, 0.15f)
            );
            
            GameObject particle = Instantiate(dashPathParticlePrefab, spawnPos + randomOffset, Quaternion.identity);
            
            // Auto-destroy particle after duration
            Destroy(particle, pathEffectDuration);
            
            // Delay between spawns synced with dash movement
            yield return new WaitForSeconds(spawnDelay);
        }
    }
    
    /// <summary>
    /// Enable invulnerability by changing player layer to one enemies can't damage
    /// </summary>
    private void EnableInvulnerability()
    {
        // Change player's layer to invulnerable layer
        // This prevents Enemy_Minions_CollisionDamage from detecting the player
        // since it checks against its damageableLayer LayerMask
        gameObject.layer = invulnerableLayer;
        
        Debug.Log($"[Player2_ChargedDashAttack] Invulnerability ENABLED - changed layer from {LayerMask.LayerToName(originalLayer)} to {LayerMask.LayerToName(invulnerableLayer)}");
    }
    
    /// <summary>
    /// Disable invulnerability by restoring player's original layer
    /// </summary>
    private void DisableInvulnerability()
    {
        // Restore player's original layer
        gameObject.layer = originalLayer;
        
        Debug.Log($"[Player2_ChargedDashAttack] Invulnerability DISABLED - restored layer to {LayerMask.LayerToName(originalLayer)}");
    }
    
    /// <summary>
    /// Check if Player2 is the active character
    /// </summary>
    private bool IsPlayer2Active()
    {
        // Check CharacterSelectionManager
        if (CharacterSelectionManager.Instance != null)
        {
            return CharacterSelectionManager.Instance.SelectedCharacterIndex == 1;
        }
        
        // Fallback: check if gameObject is active and is Player2
        return gameObject.activeInHierarchy && player2 != null;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Only draw gizmos for active Player2
        if (!enabled || player2 == null || player2.Stats == null || !IsPlayer2Active() || !isExecutingDash)
            return;
        
        // Show the current hit detection sphere during dash
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, player2.Stats.dashAttackRadius);
    }
}
