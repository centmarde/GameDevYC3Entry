using UnityEngine;

/// <summary>
/// Magnet pickable that attracts all experience orbs to the player when collected.
/// Follows the same pattern as health pickables with rotation, bobbing, and bounce animations.
/// </summary>
public class PlayerExperienceMagnet : MonoBehaviour
{
    [Header("Magnet Settings")]
    [Tooltip("Duration of the magnet effect in seconds")]
    [SerializeField] private float magnetDuration = 10f;
    
    [Tooltip("Radius to search for experience orbs")]
    [SerializeField] private float magnetRadius = 50f;
    
    [Tooltip("Player anchors to attach magnet to (supports Player and Player2). Leave empty to auto-find.")]
    [SerializeField] private Transform[] playerAnchors = new Transform[2];
    
    [Tooltip("How often to vacuum orbs (in seconds)")]
    [SerializeField] private float vacuumInterval = 0.5f;

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
    private bool isMagnetActive = false;
    private float nextVacuumTime = 0f;
    private Player attachedPlayer;
    private Transform activePlayerAnchor;

    private void Start()
    {
        startPosition = transform.position;
        bobbingTime = Random.Range(0f, Mathf.PI * 2f); // Random starting phase for bobbing
        
        //Debug.Log($"[PlayerExperienceMagnet] {gameObject.name} started at position {transform.position}");
        
        // Setup auto-despawn if enabled
        if (autoDespawn)
        {
            Invoke(nameof(DespawnPickable), despawnTime);
        }
    }

    private void Update()
    {
        // If magnet is active and attached to player, follow player
        if (isMagnetActive && activePlayerAnchor != null)
        {
            // Follow player position
            transform.position = activePlayerAnchor.position;
            
            // Continuously vacuum orbs at intervals
            if (Time.time >= nextVacuumTime)
            {
                VacuumExperienceOrbs(attachedPlayer);
                nextVacuumTime = Time.time + vacuumInterval;
            }
            
            return; // Skip other animations when active
        }
        
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

        // Mark as picked up
        isPickedUp = true;
        attachedPlayer = player;

        // Find the appropriate anchor for this player
        activePlayerAnchor = FindPlayerAnchor(player);
        
        // If no anchor found in array, use player's transform directly
        if (activePlayerAnchor == null)
        {
            activePlayerAnchor = player.transform;
        }

        // Activate continuous magnet effect
        isMagnetActive = true;
        
        // Initial vacuum
        VacuumExperienceOrbs(player);
        nextVacuumTime = Time.time + vacuumInterval;

        Debug.Log($"[PlayerExperienceMagnet] 🧲 PICKED UP: {gameObject.name} by {player.gameObject.name} - Continuous vacuum active for {magnetDuration}s!");

        // Play pickup effects
        PlayPickupEffects();

        // Hide visual mesh but keep GameObject active for duration
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        // Disable collider so it can't be picked up again
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Schedule destruction after duration
        Invoke(nameof(DeactivateMagnet), magnetDuration);
    }

    private Transform FindPlayerAnchor(Player player)
    {
        // Check if playerAnchors array has any entries
        if (playerAnchors == null || playerAnchors.Length == 0)
        {
            return null;
        }

        // Try to find matching anchor by checking if player is child of anchor
        foreach (Transform anchor in playerAnchors)
        {
            if (anchor == null) continue;
            
            // Check if this anchor is the player or contains the player
            if (anchor == player.transform || player.transform.IsChildOf(anchor) || anchor.IsChildOf(player.transform))
            {
                return anchor;
            }
            
            // Check if anchor has Player or Player2 component that matches
            Player anchorPlayer = anchor.GetComponent<Player>();
            if (anchorPlayer == player)
            {
                return anchor;
            }
        }

        // If no match found, try to use first non-null anchor
        foreach (Transform anchor in playerAnchors)
        {
            if (anchor != null) return anchor;
        }

        return null;
    }

    private void VacuumExperienceOrbs(Player player)
    {
        // Find all experience orbs in the scene
        ExperienceOrb[] allOrbs = FindObjectsOfType<ExperienceOrb>();
        
        int orbsAffected = 0;
        
        foreach (ExperienceOrb orb in allOrbs)
        {
            if (orb == null) continue;
            
            // Check if orb is within magnet radius
            float distance = Vector3.Distance(transform.position, orb.transform.position);
            
            if (distance <= magnetRadius)
            {
                // Force the orb to move towards the player by modifying its internal state
                // We'll use reflection or make the orb's fields accessible
                // For now, let's destroy the orb and give XP directly (instant collection)
                
                // Get the experience amount (you may need to make this field public or use reflection)
                // For simplicity, we'll just trigger collection by moving player to orb momentarily
                // or better yet, we can use a coroutine to animate them
                
                StartCoroutine(AttractOrbToPlayer(orb, player));
                orbsAffected++;
            }
        }
        
        Debug.Log($"[PlayerExperienceMagnet] 🧲 Magnet activated! Attracting {orbsAffected} experience orbs within {magnetRadius}m radius!");
    }

    private System.Collections.IEnumerator AttractOrbToPlayer(ExperienceOrb orb, Player player)
    {
        if (orb == null || player == null) yield break;
        
        // Get the orb's rigidbody and collider
        Rigidbody orbRb = orb.GetComponent<Rigidbody>();
        if (orbRb == null) yield break;
        
        Collider orbCollider = orb.GetComponent<Collider>();
        if (orbCollider != null)
        {
            // Make it a trigger to prevent physics collisions with player/enemies
            orbCollider.isTrigger = true;
        }
        
        // Disable gravity and constraints temporarily for instant attraction
        orbRb.useGravity = false;
        orbRb.constraints = RigidbodyConstraints.None;
        
        // Set high velocity towards player for instant vacuum effect
        float attractSpeed = 50f; // Very fast for instant vacuum feel
        Vector3 direction = (player.transform.position - orb.transform.position).normalized;
        orbRb.linearVelocity = direction * attractSpeed;
        
        // Add some rotation for visual effect
        orbRb.angularVelocity = new Vector3(
            Random.Range(-5f, 5f),
            Random.Range(-5f, 5f),
            Random.Range(-5f, 5f)
        );
        
        yield return null;
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
            //Debug.Log($"[PlayerExperienceMagnet] ⏱ DESPAWNED: {gameObject.name} auto-despawned after {despawnTime} seconds (not picked up)");
            Destroy(gameObject);
        }
    }

    private void DeactivateMagnet()
    {
        Debug.Log($"[PlayerExperienceMagnet] 🧲 Magnet effect ended after {magnetDuration}s");
        isMagnetActive = false;
        Destroy(gameObject);
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

        Debug.Log($"[PlayerExperienceMagnet] 🎯 {gameObject.name} is now interactable!");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw pickup radius in green
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
        
        // Draw magnet effect radius in cyan
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}
