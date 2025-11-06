using UnityEngine;

/// <summary>
/// Pickable item that heals the player when collected.
/// Attach this script to pickup objects scattered on the map.
/// Visual representation is set by the spawner from an array of prefabs.
/// </summary>
public class PlayerPickable : MonoBehaviour
{
    [Header("Heal Settings")]
    [Tooltip("Possible heal percentages that can spawn (e.g., 0.2 for 20%, 0.5 for 50%)")]
    [SerializeField] private float[] possibleHealPercentages = new float[] { 0.2f, 0.5f, 0.8f, 1.0f };
    
    [Tooltip("The actual heal percentage for this instance (randomly selected on spawn)")]
    private float healPercentage;

    [Header("Visual Feedback")]
    [Tooltip("Particle effect to play when picked up (optional)")]
    [SerializeField] private GameObject pickupEffectPrefab;
    
    [Tooltip("Sound to play when picked up (optional)")]
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Rotation Animation")]
    [Tooltip("Should the pickable rotate?")]
    [SerializeField] private bool shouldRotate = true;
    
    [Tooltip("Rotation speed in degrees per second")]
    [SerializeField] private float rotationSpeed = 90f;
    
    [Header("Bobbing Animation")]
    [Tooltip("Should the pickable bob up and down?")]
    [SerializeField] private bool shouldBob = true;
    
    [Tooltip("Bobbing height in units")]
    [SerializeField] private float bobbingHeight = 0.3f;
    
    [Tooltip("Bobbing speed")]
    [SerializeField] private float bobbingSpeed = 2f;
    
    private Vector3 startPosition;
    private float bobbingTime;

    [Header("Pickup Settings")]
    [Tooltip("Radius within which the player can pick up this item")]
    [SerializeField] private float pickupRadius = 1.5f;
    
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

    private void Awake()
    {
        // Randomly select a heal percentage from the available options
        if (possibleHealPercentages.Length > 0)
        {
            healPercentage = possibleHealPercentages[Random.Range(0, possibleHealPercentages.Length)];
            //Debug.Log($"[PlayerPickable] 🎲 {gameObject.name} spawned with {healPercentage * 100}% heal value (Awake)");
        }
        else
        {
            //Debug.LogWarning($"[PlayerPickable] {gameObject.name} has no heal percentages defined! Defaulting to 50%");
            healPercentage = 0.5f;
        }
        
        // Ensure this GameObject is on a visible layer
        //Debug.Log($"[PlayerPickable] {gameObject.name} is on layer: {LayerMask.LayerToName(gameObject.layer)}");
    }

    private void Start()
    {
        startPosition = transform.position;
        bobbingTime = Random.Range(0f, Mathf.PI * 2f); // Random starting phase for bobbing
        
        //Debug.Log($"[PlayerPickable] {gameObject.name} started at position {transform.position} with scale {transform.localScale}");
        
        // Setup auto-despawn if enabled
        if (autoDespawn)
        {
            Invoke(nameof(DespawnPickable), despawnTime);
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
            // Check if it's a Player or Player2
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

        // Check if the colliding object is a Player
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            PickupByPlayer(player);
        }
    }

    private void PickupByPlayer(Player player)
    {
        if (isPickedUp) return;

        // Get the player's health component
        Entity_Health playerHealth = player.GetComponent<Entity_Health>();
        
        if (playerHealth == null)
        {
            //Debug.LogWarning($"[PlayerPickable] Player {player.name} has no Entity_Health component!");
            return;
        }

        // Check if player needs healing
        if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
        {
            //Debug.Log($"[PlayerPickable] Player is already at full health. Pickup ignored.");
            return; // Don't pick up if already at full health
        }

        // Calculate heal amount based on max health
        float healAmount = playerHealth.MaxHealth * healPercentage;
        
        // Heal the player
        playerHealth.Heal(healAmount);
        
        //Debug.Log($"[PlayerPickable] 💊 PICKED UP: {gameObject.name} - Player healed {healPercentage * 100}% ({healAmount} HP)");

        // Mark as picked up
        isPickedUp = true;

        // Play pickup effects
        PlayPickupEffects();

        // Destroy the pickable
        Destroy(gameObject);
    }

    private void PlayPickupEffects()
    {
        // Spawn particle effect
        if (pickupEffectPrefab != null)
        {
            GameObject effect = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Clean up after 2 seconds
        }

        // Play sound effect
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }

    private void DespawnPickable()
    {
        if (!isPickedUp)
        {
            //Debug.Log($"[PlayerPickable] ⏱ DESPAWNED: {gameObject.name} auto-despawned after {despawnTime} seconds (not picked up)");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Force set the heal percentage (useful for spawners that want specific values)
    /// </summary>
    public void SetHealPercentage(float percentage)
    {
        healPercentage = Mathf.Clamp01(percentage);
        //Debug.Log($"[PlayerPickable] Heal percentage manually set to {healPercentage * 100}%");
    }

    /// <summary>
    /// Get the current heal percentage of this pickable
    /// </summary>
    public float GetHealPercentage()
    {
        return healPercentage;
    }

    /// <summary>
    /// Play bounce animation when spawned from container
    /// Makes the pickable non-interactable for the specified delay
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

        Debug.Log($"[PlayerPickable] 🎯 {gameObject.name} is now interactable!");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw pickup radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
