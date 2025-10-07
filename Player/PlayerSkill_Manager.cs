using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkill_Manager : MonoBehaviour
{
    [Header("Slow Motion Settings")]
    [SerializeField] private float slowMotionTimeScale = 0.3f; // How slow enemies become (0.3 = 30% speed)
    [SerializeField] private float slowMotionDuration = 3f;     // How long the effect lasts
    [SerializeField] private float slowMotionCooldown = 5f;     // Cooldown time before can use again
    [SerializeField] private Key slowMotionKey = Key.X;         // Key to activate slow motion

    private bool isSlowMotionActive = false;
    private float slowMotionTimer = 0f;
    private float cooldownTimer = 0f;
    private List<Enemy> affectedEnemies = new List<Enemy>();

    private void Update()
    {
        // Update cooldown timer
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Check for slow motion activation using new Input System
        if (Keyboard.current != null && Keyboard.current[slowMotionKey].wasPressedThisFrame 
            && !isSlowMotionActive && cooldownTimer <= 0f)
        {
            ActivateSlowMotion();
        }

        // Update slow motion timer
        if (isSlowMotionActive)
        {
            slowMotionTimer -= Time.deltaTime;
            if (slowMotionTimer <= 0f)
            {
                DeactivateSlowMotion();
            }
        }
    }

    private void ActivateSlowMotion()
    {
        isSlowMotionActive = true;
        slowMotionTimer = slowMotionDuration;

        // Find all enemies in the scene
        affectedEnemies.Clear();
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null && enemy.IsAlive)
            {
                affectedEnemies.Add(enemy);
                ApplySlowMotionToEnemy(enemy, slowMotionTimeScale);
            }
        }

        Debug.Log($"Slow Motion Activated! {affectedEnemies.Count} enemies affected.");
    }

    private void DeactivateSlowMotion()
    {
        isSlowMotionActive = false;

        // Restore normal speed to all affected enemies
        foreach (Enemy enemy in affectedEnemies)
        {
            if (enemy != null)
            {
                ApplySlowMotionToEnemy(enemy, 1f);
            }
        }

        affectedEnemies.Clear();
        
        // Start cooldown
        cooldownTimer = slowMotionCooldown;
        
        Debug.Log("Slow Motion Deactivated!");
    }

    private void ApplySlowMotionToEnemy(Enemy enemy, float timeScale)
    {
        // Affect the animator speed
        if (enemy.anim != null)
        {
            enemy.anim.speed = timeScale;
        }

        // Store the time scale for movement calculations
        // The enemy movement scripts will need to use this
        Enemy_Movement movement = enemy.movement;
        if (movement != null)
        {
            // We'll add a time scale property to Enemy_Movement
            movement.SetTimeScale(timeScale);
        }
    }

    // Public method to check if slow motion is active
    public bool IsSlowMotionActive() => isSlowMotionActive;

    // Public method to get current time scale for enemies
    public float GetEnemyTimeScale() => isSlowMotionActive ? slowMotionTimeScale : 1f;

    // Public methods for UI
    public bool IsOnCooldown() => cooldownTimer > 0f && !isSlowMotionActive;
    
    public float GetRemainingCooldown() => Mathf.Max(0f, cooldownTimer);
    
    public float GetCooldownPercent() => slowMotionCooldown > 0 ? (cooldownTimer / slowMotionCooldown) : 0f;
    
    public float GetRemainingDurationPercent() => isSlowMotionActive && slowMotionDuration > 0 
        ? (slowMotionTimer / slowMotionDuration) : 0f;
    
    public string GetSlowMotionKeyName() => slowMotionKey.ToString();

    private void OnDestroy()
    {
        // Clean up - restore all enemies to normal speed
        if (isSlowMotionActive)
        {
            DeactivateSlowMotion();
        }
    }
}
