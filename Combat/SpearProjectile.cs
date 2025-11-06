using UnityEngine;

/// <summary>
/// Simple spear projectile component that works with ProjectileSlingshot
/// This can be added to spear prefabs to customize their appearance and behavior
/// </summary>
public class SpearProjectile : MonoBehaviour
{
    [Header("Spear Visual Settings")]
    [SerializeField] private float rotationSpeed = 360f; // Degrees per second
    [SerializeField] private bool rotateWhileFlying = true;
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    
    [Header("Trail Effect")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private bool autoSetupTrail = true;
    [SerializeField] private Color trailColor = Color.yellow;
    [SerializeField] private float trailWidth = 0.1f;
    [SerializeField] private float trailTime = 0.5f;
    
    [Header("Boomerang Behavior")]
    [SerializeField] private float maxRange = 8f; // Maximum distance before returning
    [SerializeField] private float returnSpeed = 15f; // Speed when returning to player
    [SerializeField] private float returnAcceleration = 2f; // How quickly it accelerates back
    
    private Rigidbody rb;
    private Transform player;
    private Vector3 startPosition;
    private bool isReturning = false;
    private float distanceTraveled = 0f;
    private Vector3 lastPosition;
    private bool lifetimeDisabled = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        lastPosition = transform.position;
        
        // Find player reference
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
        }
        
        // Auto-setup trail if enabled and none exists
        if (autoSetupTrail && trailRenderer == null)
        {
            SetupTrailRenderer();
        }
    }
    
    private void Update()
    {
        // Rotate the spear while flying
        if (rotateWhileFlying && rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
        }
        
        // Handle boomerang behavior
        HandleBoomerangBehavior();
    }
    
    private void HandleBoomerangBehavior()
    {
        if (rb == null || player == null) return;
        
        // Update distance traveled
        distanceTraveled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;
        
        // Check if should start returning
        if (!isReturning && distanceTraveled >= maxRange)
        {
            StartReturning();
        }
        
        // Handle return behavior
        if (isReturning)
        {
            ReturnToPlayer();
        }
    }
    
    private void StartReturning()
    {
        isReturning = true;
        
        // Disable lifetime destruction so spear only gets destroyed when reaching player
        DisableLifetimeDestruction();
        
        // Smoothly reverse direction to return to player
        if (rb != null && player != null)
        {
            // Calculate direction back to player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            
            // Gradually change velocity direction
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, directionToPlayer * (rb.linearVelocity.magnitude * 0.8f), 0.5f);
        }
    }
    
    private void ReturnToPlayer()
    {
        if (player == null || rb == null) return;
        
        // Calculate direction to player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        
        // Apply stronger return force to make it fly back more aggressively
        Vector3 returnForce = directionToPlayer * returnSpeed * returnAcceleration * 1.5f;
        rb.AddForce(returnForce, ForceMode.Acceleration);
        
        // Also directly adjust velocity to ensure it's moving toward player
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 desiredVelocity = directionToPlayer * returnSpeed;
        rb.linearVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, Time.deltaTime * 2f);
        
        // Limit maximum return speed
        if (rb.linearVelocity.magnitude > returnSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * returnSpeed;
        }
        
        // Check if close enough to player to destroy
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < 1f)
        {
            // Spear reached player, destroy it
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Disable the lifetime destruction so spear only gets destroyed when returning to player
    /// </summary>
    public void DisableLifetimeDestruction()
    {
        if (!lifetimeDisabled)
        {
            lifetimeDisabled = true;
            // Cancel any scheduled destruction
            CancelInvoke();
        }
    }
    
    /// <summary>
    /// Setup a default trail renderer for the spear
    /// </summary>
    private void SetupTrailRenderer()
    {
        trailRenderer = gameObject.AddComponent<TrailRenderer>();
        
        if (trailRenderer != null)
        {
            trailRenderer.material = CreateTrailMaterial();
            
            // Set trail color using gradient
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(trailColor, 0.0f), new GradientColorKey(trailColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trailRenderer.colorGradient = gradient;
            
            trailRenderer.startWidth = trailWidth;
            trailRenderer.endWidth = 0f;
            trailRenderer.time = trailTime;
            trailRenderer.autodestruct = false;
        }
    }
    
    /// <summary>
    /// Create a simple material for the trail
    /// </summary>
    private Material CreateTrailMaterial()
    {
        // Create a simple unlit material for the trail
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = trailColor;
        return mat;
    }
    
    /// <summary>
    /// Called when spear hits something (can be used for custom effects)
    /// </summary>
    public void OnSpearHit()
    {
        // Stop rotation on hit
        rotateWhileFlying = false;
        
        // Fade out trail
        if (trailRenderer != null)
        {
            trailRenderer.time = 0.1f;
        }
    }
}