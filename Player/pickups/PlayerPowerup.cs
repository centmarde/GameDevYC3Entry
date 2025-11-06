using UnityEngine;

/// <summary>
/// Powerup item that grants temporary invulnerability and speed boost to the player.
/// Effects stack when multiple powerups are collected.
/// </summary>
public class PlayerPowerup : MonoBehaviour
{
    [Header("Powerup Effects")]
    [Tooltip("Duration of the powerup effect in seconds")]
    [SerializeField] private float effectDuration = 7f;
    
    [Tooltip("Movement speed multiplier (2.0 = 200% speed)")]
    [SerializeField] private float speedMultiplier = 2f;
    
    [Tooltip("Grant invulnerability during effect")]
    [SerializeField] private bool grantInvulnerability = true;

    [Header("Visual Feedback")]
    [Tooltip("Particle effect to play when picked up (optional)")]
    [SerializeField] private GameObject pickupEffectPrefab;
    
    [Tooltip("Sound to play when picked up (optional)")]
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Rotation Animation")]
    [Tooltip("Should the powerup rotate?")]
    [SerializeField] private bool shouldRotate = true;
    
    [Tooltip("Rotation speed in degrees per second")]
    [SerializeField] private float rotationSpeed = 120f;
    
    [Header("Bobbing Animation")]
    [Tooltip("Should the powerup bob up and down?")]
    [SerializeField] private bool shouldBob = true;
    
    [Tooltip("Bobbing height in units")]
    [SerializeField] private float bobbingHeight = 0.4f;
    
    [Tooltip("Bobbing speed")]
    [SerializeField] private float bobbingSpeed = 2.5f;
    
    private Vector3 startPosition;
    private float bobbingTime;

    [Header("Pickup Settings")]
    [Tooltip("Radius within which the player can pick up this item")]
    [SerializeField] private float pickupRadius = 2f;
    
    [Tooltip("Should this item automatically despawn if not picked up?")]
    [SerializeField] private bool autoDespawn = true;
    
    [Tooltip("Time in seconds before auto-despawn (if enabled)")]
    [SerializeField] private float despawnTime = 30f;

    [Header("Bounce Animation (From Container)")]
    [Tooltip("Height of bounce animation")]
    [SerializeField] private float bounceHeight = 2f;
    
    [Tooltip("Duration of bounce animation")]
    [SerializeField] private float bounceDuration = 0.8f;

    private bool isPickedUp = false;
    private bool canBePickedUp = true;
    private bool isPlayingBounceAnimation = false;

    private void Start()
    {
        startPosition = transform.position;
        bobbingTime = Random.Range(0f, Mathf.PI * 2f);
        
        //Debug.Log($"[PlayerPowerup] {gameObject.name} spawned - Duration: {effectDuration}s, Speed: {speedMultiplier}x");
        
        if (autoDespawn)
        {
            Invoke(nameof(DespawnPowerup), despawnTime);
        }
    }

    private void Update()
    {
        if (isPickedUp) return;

        // Rotation animation
        if (shouldRotate)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        // Bobbing animation
        if (shouldBob)
        {
            bobbingTime += Time.deltaTime * bobbingSpeed;
            float newY = startPosition.y + Mathf.Sin(bobbingTime) * bobbingHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        // Check for nearby player
        CheckForPlayer();
    }

    private void CheckForPlayer()
    {
        // Don't allow pickup if not ready
        if (!canBePickedUp) return;
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRadius);
        
        foreach (Collider col in hitColliders)
        {
            Player player = col.GetComponent<Player>();
            if (player != null)
            {
                PickupByPlayer(player);
                return;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp || !canBePickedUp) return;

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            PickupByPlayer(player);
        }
    }

    private void PickupByPlayer(Player player)
    {
        if (isPickedUp) return;

        // Get or add the PowerupManager component
        PowerupManager powerupManager = player.GetComponent<PowerupManager>();
        if (powerupManager == null)
        {
            powerupManager = player.gameObject.AddComponent<PowerupManager>();
        }

        // Apply the powerup effect (stacks if already active)
        powerupManager.ApplyPowerup(effectDuration, speedMultiplier, grantInvulnerability);
        
        //Debug.Log($"[PlayerPowerup] Player picked up powerup! Duration: {effectDuration}s, Speed: {speedMultiplier}x");

        isPickedUp = true;

        // Play pickup effects
        PlayPickupEffects();

        // Destroy the powerup
        Destroy(gameObject);
    }

    private void PlayPickupEffects()
    {
        if (pickupEffectPrefab != null)
        {
            GameObject effect = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }

    private void DespawnPowerup()
    {
        if (!isPickedUp)
        {
            //Debug.Log($"[PlayerPowerup] {gameObject.name} despawned after {despawnTime} seconds");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Play bounce animation when spawned from container
    /// Makes the powerup non-interactable for the specified delay
    /// </summary>
    public void PlayBounceAnimation(float interactionDelay)
    {
        if (!isPlayingBounceAnimation)
        {
            StartCoroutine(BounceAnimationCoroutine(interactionDelay));
        }
    }

    private System.Collections.IEnumerator BounceAnimationCoroutine(float interactionDelay)
    {
        isPlayingBounceAnimation = true;
        canBePickedUp = false;

        Vector3 startPos = transform.position;
        Vector3 peakPos = startPos + Vector3.up * bounceHeight;
        float elapsed = 0f;

        // Bounce up
        while (elapsed < bounceDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (bounceDuration / 2f);
            
            // Ease out quad for smooth bounce
            float easeProgress = 1f - (1f - progress) * (1f - progress);
            transform.position = Vector3.Lerp(startPos, peakPos, easeProgress);
            
            // Add extra spin during bounce
            transform.Rotate(Vector3.up, rotationSpeed * 2f * Time.deltaTime);
            
            yield return null;
        }

        elapsed = 0f;
        
        // Bounce down
        while (elapsed < bounceDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (bounceDuration / 2f);
            
            // Ease in quad for smooth landing
            float easeProgress = progress * progress;
            transform.position = Vector3.Lerp(peakPos, startPos, easeProgress);
            
            // Continue spinning
            transform.Rotate(Vector3.up, rotationSpeed * 2f * Time.deltaTime);
            
            yield return null;
        }

        // Ensure we land exactly at start position
        transform.position = startPos;
        startPosition = startPos; // Update start position for bobbing

        // Wait for interaction delay
        float remainingDelay = interactionDelay - bounceDuration;
        if (remainingDelay > 0f)
        {
            yield return new WaitForSeconds(remainingDelay);
        }

        // Enable pickup
        canBePickedUp = true;
        isPlayingBounceAnimation = false;

        Debug.Log($"[PlayerPowerup] 🎯 {gameObject.name} is now interactable!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
