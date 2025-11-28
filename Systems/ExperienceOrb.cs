using UnityEngine;

/// <summary>
/// Experience orb that drops from enemies and can be collected by players.
/// Automatically moves towards the nearest player when within pickup range.
/// </summary>
public class ExperienceOrb : MonoBehaviour
{
    [Header("Experience Settings")]
    [Tooltip("Amount of experience this orb gives when collected")]
    [SerializeField] private int experienceAmount = 10;

    [Header("Pickup Settings")]
    [Tooltip("Distance at which the orb starts moving towards the player")]
    [SerializeField] private float pickupRange = 5f;
    
    [Tooltip("Speed at which the orb moves towards the player")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Tooltip("Acceleration applied when moving towards player")]
    [SerializeField] private float acceleration = 1f;
    
    [Tooltip("Time in seconds before the orb can be picked up")]
    [SerializeField] private float pickupDelay = 3f;
    
    [Tooltip("Minimum height above ground to maintain")]
    [SerializeField] private float minHeightAboveGround = 0.5f;

    [Header("Visuals")]
    [Tooltip("Particle effect to play when collected")]
    [SerializeField] private GameObject collectEffect;
    
    [Header("Audio")]
    [Tooltip("Audio clips to play when collected (picks one randomly)")]
    [SerializeField] private AudioClip[] collectSounds;
    
    [Tooltip("Volume of the collection sound (0 to 1)")]
    [Range(0f, 1f)]
    [SerializeField] private float collectVolume = 1f;

    [Header("Optimization")]
    [Tooltip("Time in seconds after which the orb will auto-destroy if not collected")]
    [SerializeField] private float autoDestroyTime = 120f; // 2 minutes

    private Transform targetPlayer;
    private Rigidbody rb;
    private float currentSpeed = 0f;
    private bool isBeingCollected = false;
    private float spawnTime;
    private bool canBePickedUp = false;
    private bool hasLanded = false;
    private float groundY;
    private Collider orbCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        orbCollider = GetComponent<Collider>();
        
        // CRITICAL: Ensure collider is initially NOT a trigger for physics bouncing
        if (orbCollider != null)
        {
            orbCollider.isTrigger = false;
        }
        
        // Ensure rigidbody settings for proper physics
        if (rb != null)
        {
            rb.useGravity = true;
            // CRITICAL: Use ContinuousSpeculative for trigger collisions (prevents tunneling)
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            // Increase solver iterations for better collision detection
            rb.solverIterations = 10;
            rb.solverVelocityIterations = 10;
            // Allow free rotation for ragdoll-like tumbling
            rb.constraints = RigidbodyConstraints.None;
            
            // Add random angular velocity for spinning effect (smoother)
            rb.angularVelocity = new Vector3(
                Random.Range(-2f, 2f),
                Random.Range(-2f, 2f),
                Random.Range(-2f, 2f)
            );
        }
        
        // Record spawn time for pickup delay
        spawnTime = Time.time;
        
        // Start auto-destroy timer for optimization
        StartCoroutine(AutoDestroyTimer());
    }

    private void Update()
    {
        if (isBeingCollected) return;

        // Check if orb has landed on ground
        if (!hasLanded && rb != null)
        {
            // Check if velocity is near zero (has landed)
            if (rb.linearVelocity.magnitude < 0.1f && Time.time > spawnTime + 0.3f)
            {
                hasLanded = true;
                groundY = transform.position.y;
                
                // Raycast to find actual ground level
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
                {
                    groundY = hit.point.y + minHeightAboveGround;
                    transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
                }
                
                // Lock Y position after landing to prevent sinking
                // But keep rotation free for continued spinning effect
                rb.constraints = RigidbodyConstraints.FreezePositionY;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero; // Stop all movement
                
                // Reduce angular velocity after landing but don't stop it completely
                rb.angularVelocity *= 0.3f;
            }
        }

        // Prevent sinking below ground level
        if (hasLanded && transform.position.y < groundY - 0.1f)
        {
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        }

        // Check if enough time has passed to allow pickup
        if (!canBePickedUp)
        {
            if (Time.time >= spawnTime + pickupDelay)
            {
                canBePickedUp = true;
                
                // Unfreeze Y position to allow movement towards player
                // Keep rotation free for spinning during flight to player
                if (rb != null && hasLanded)
                {
                    rb.constraints = RigidbodyConstraints.None;
                    
                    // Add more angular velocity when starting to move to player
                    rb.angularVelocity = new Vector3(
                        Random.Range(-1.5f, 1.5f),
                        Random.Range(-1.5f, 1.5f),
                        Random.Range(-1.5f, 1.5f)
                    );
                }
            }
            else
            {
                // Still in delay period, don't move towards player yet
                return;
            }
        }

        // Find nearest player
        FindNearestPlayer();

        // Move towards player if within range
        if (targetPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, targetPlayer.position);
            
            if (distance <= pickupRange)
            {
                MoveTowardsPlayer();
            }
        }
    }

    private void FindNearestPlayer()
    {
        // Try to find Player by tag
        GameObject player1 = GameObject.FindWithTag("Player");
        GameObject player2 = null;

        // Try to find Player2 by name patterns
        player2 = GameObject.Find("Player2") ?? GameObject.Find("Player 2");

        // If not found, search for Player components
        if (player1 == null && player2 == null)
        {
            Player[] players = FindObjectsOfType<Player>();
            if (players.Length > 0)
            {
                player1 = players[0].gameObject;
                if (players.Length > 1)
                {
                    player2 = players[1].gameObject;
                }
            }
        }

        // Find the closest player
        Transform closest = null;
        float closestDistance = float.MaxValue;

        if (player1 != null)
        {
            float dist = Vector3.Distance(transform.position, player1.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = player1.transform;
            }
        }

        if (player2 != null)
        {
            float dist = Vector3.Distance(transform.position, player2.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = player2.transform;
            }
        }

        targetPlayer = closest;
    }

    private void MoveTowardsPlayer()
    {
        if (targetPlayer == null || rb == null) return;

        // Calculate distance first
        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
        
        // AGGRESSIVE COLLECTION: Larger radius for instant collection
        if (distanceToPlayer < 1.5f)
        {
            Player player = targetPlayer.GetComponent<Player>();
            if (player != null)
            {
                Entity_Health health = player.GetComponent<Entity_Health>();
                if (health != null && health.IsAlive)
                {
                    CollectOrb(player);
                    return;
                }
            }
        }

        // Make collider a trigger when moving to player to avoid pushing
        if (orbCollider != null && !orbCollider.isTrigger)
        {
            orbCollider.isTrigger = true;
        }

        // Calculate direction to player
        Vector3 direction = (targetPlayer.position - transform.position).normalized;

        // Use kinematic movement when very close for precise control
        if (distanceToPlayer < 2f)
        {
            // Direct position interpolation for final approach
            float step = 8f * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, step);
            return;
        }

        // Accelerate towards player (capped to prevent tunneling through triggers)
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, moveSpeed);
        
        // Clamp max speed when getting closer to prevent overshooting
        if (distanceToPlayer < 3f)
        {
            currentSpeed = Mathf.Min(currentSpeed, distanceToPlayer * 2f);
        }

        // Move using rigidbody, maintaining minimum height
        Vector3 velocity = direction * currentSpeed;
        
        // Prevent moving downward below minimum height
        if (hasLanded && transform.position.y < groundY + minHeightAboveGround && velocity.y < 0)
        {
            velocity.y = 0;
        }
        
        rb.linearVelocity = velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBeingCollected) return;
        
        // Don't allow pickup during delay period
        if (!canBePickedUp) return;

        // Check if collided with a player
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            // Check if player is alive by checking their health component
            Entity_Health health = player.GetComponent<Entity_Health>();
            if (health != null && health.IsAlive)
            {
                CollectOrb(player);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isBeingCollected) return;
        
        // Don't allow pickup during delay period
        if (!canBePickedUp) return;

        // Check if staying in contact with a player
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            // Check if player is alive by checking their health component
            Entity_Health health = player.GetComponent<Entity_Health>();
            if (health != null && health.IsAlive)
            {
                CollectOrb(player);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isBeingCollected) return;
        
        // Don't allow pickup during delay period
        if (!canBePickedUp) return;

        // Check if collided with a player (when orb is solid, not trigger)
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            // Check if player is alive by checking their health component
            Entity_Health health = player.GetComponent<Entity_Health>();
            if (health != null && health.IsAlive)
            {
                CollectOrb(player);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isBeingCollected) return;
        
        // Don't allow pickup during delay period
        if (!canBePickedUp) return;

        // Check if staying in contact with a player (when orb is solid, not trigger)
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            // Check if player is alive by checking their health component
            Entity_Health health = player.GetComponent<Entity_Health>();
            if (health != null && health.IsAlive)
            {
                CollectOrb(player);
            }
        }
    }

    private void CollectOrb(Player player)
    {
        isBeingCollected = true;

        // Give experience to the player
        ExperienceManager.Instance?.AddExperience(experienceAmount);

        // Play particle effect
        if (collectEffect != null)
        {
            GameObject effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // Play random audio clip
        if (collectSounds != null && collectSounds.Length > 0)
        {
            // Filter out null entries
            AudioClip[] validClips = System.Array.FindAll(collectSounds, clip => clip != null);
            
            if (validClips.Length > 0)
            {
                // Pick a random sound from the array
                int randomIndex = Random.Range(0, validClips.Length);
                AudioClip selectedClip = validClips[randomIndex];
                
                // Play the sound at the orb's position with specified volume
                AudioSource.PlayClipAtPoint(selectedClip, transform.position, collectVolume);
            }
        }

        // Destroy the orb
        Destroy(gameObject);
    }

    /// <summary>
    /// Auto-destroy the orb after specified time for optimization purposes
    /// </summary>
    private System.Collections.IEnumerator AutoDestroyTimer()
    {
        yield return new WaitForSeconds(autoDestroyTime);
        
        // Only destroy if not already being collected
        if (!isBeingCollected && gameObject != null)
        {
            Debug.Log($"[ExperienceOrb] Auto-destroying uncollected orb after {autoDestroyTime} seconds for optimization");
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw pickup range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
