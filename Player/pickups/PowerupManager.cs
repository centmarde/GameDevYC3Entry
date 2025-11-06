using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages powerup effects on the player, including stacking multiple powerups.
/// Handles invulnerability and speed boost effects.
/// </summary>
public class PowerupManager : MonoBehaviour
{
    [Header("Active Powerup Info")]
    [SerializeField] private int activePowerupCount = 0;
    [SerializeField] private float totalSpeedMultiplier = 1f;
    [SerializeField] private bool isInvulnerable = false;
    
    [Header("Visual Feedback")]
    [Tooltip("Material to apply when invulnerable (optional)")]
    [SerializeField] private Material invulnerableMaterial;
    
    [Tooltip("Particle effect while powerup is active (optional)")]
    [SerializeField] private GameObject powerupParticleEffect;
    
    private Player player;
    private Entity_Health playerHealth;
    private Player_DataSO playerData;
    private Player2_DataSO player2Data;
    private Player_Movement playerMovement;
    private List<PowerupEffect> activePowerups = new List<PowerupEffect>();
    private Renderer[] playerRenderers;
    private Material[] originalMaterials;
    private GameObject activeParticleEffect;
    private float baseSpeedMultiplier = 1f;

    private class PowerupEffect
    {
        public float endTime;
        public float speedMultiplier;
        public bool grantsInvulnerability;
        
        public PowerupEffect(float duration, float speed, bool invuln)
        {
            endTime = Time.time + duration;
            speedMultiplier = speed;
            grantsInvulnerability = invuln;
        }
        
        public bool IsExpired()
        {
            return Time.time >= endTime;
        }
        
        public float GetRemainingTime()
        {
            return Mathf.Max(0f, endTime - Time.time);
        }
    }

    private void Awake()
    {
        player = GetComponent<Player>();
        playerHealth = GetComponent<Entity_Health>();
        playerMovement = GetComponent<Player_Movement>();
        
        // Get player data (try Player first, then Player2)
        Player2 player2 = GetComponent<Player2>();
        
        if (player2 != null && player2.Stats != null)
        {
            // Player2
            player2Data = player2.Stats;
            baseSpeedMultiplier = player2Data.currentSpeedMultiplier;
        }
        else if (player != null && player.Stats != null)
        {
            // Player (base)
            playerData = player.Stats;
            baseSpeedMultiplier = playerData.currentSpeedMultiplier;
        }
        
        // Cache renderers for visual effects
        playerRenderers = GetComponentsInChildren<Renderer>();
    }

    private void Update()
    {
        // Update active powerups and remove expired ones
        for (int i = activePowerups.Count - 1; i >= 0; i--)
        {
            if (activePowerups[i].IsExpired())
            {
                //Debug.Log($"[PowerupManager] Powerup expired. Remaining: {activePowerups.Count - 1}");
                activePowerups.RemoveAt(i);
            }
        }
        
        // Update active count and effects
        activePowerupCount = activePowerups.Count;
        
        if (activePowerupCount > 0)
        {
            UpdatePowerupEffects();
        }
        else
        {
            // No active powerups, reset to normal
            if (isInvulnerable || totalSpeedMultiplier != 1f)
            {
                DeactivateEffects();
            }
        }
    }

    /// <summary>
    /// Apply a powerup effect. Stacks with existing powerups.
    /// </summary>
    public void ApplyPowerup(float duration, float speedMultiplier, bool grantInvulnerability)
    {
        // Add new powerup to the list
        PowerupEffect newPowerup = new PowerupEffect(duration, speedMultiplier, grantInvulnerability);
        activePowerups.Add(newPowerup);
        
        //Debug.Log($"[PowerupManager] Powerup applied! Total active: {activePowerups.Count}, Duration: {duration}s");
        
        // Immediately update effects
        UpdatePowerupEffects();
        
        // Create visual feedback if this is the first powerup
        if (activePowerups.Count == 1 && powerupParticleEffect != null)
        {
            activeParticleEffect = Instantiate(powerupParticleEffect, transform);
        }
    }

    private void UpdatePowerupEffects()
    {
        // Calculate total speed multiplier (stacking)
        totalSpeedMultiplier = baseSpeedMultiplier;
        bool hasInvulnerability = false;
        
        foreach (PowerupEffect powerup in activePowerups)
        {
            // Stack speed multipliers additively (can be changed to multiplicative)
            totalSpeedMultiplier += (powerup.speedMultiplier - 1f);
            
            if (powerup.grantsInvulnerability)
            {
                hasInvulnerability = true;
            }
        }
        
        // Apply speed boost to player data SO (for other systems to read)
        if (playerData != null)
        {
            playerData.currentSpeedMultiplier = totalSpeedMultiplier;
        }
        else if (player2Data != null)
        {
            player2Data.currentSpeedMultiplier = totalSpeedMultiplier;
        }
        
        // Apply speed boost to movement component (for immediate effect)
        if (playerMovement != null)
        {
            playerMovement.SetSpeedMultiplier(totalSpeedMultiplier);
        }
        
        // Apply invulnerability
        if (hasInvulnerability && !isInvulnerable)
        {
            ActivateInvulnerability();
        }
        else if (!hasInvulnerability && isInvulnerable)
        {
            DeactivateInvulnerability();
        }
        
        isInvulnerable = hasInvulnerability;
    }

    private void ActivateInvulnerability()
    {
        if (playerHealth != null)
        {
            playerHealth.SetInvulnerable(true);
        }
        
        // Apply visual effect
        if (invulnerableMaterial != null && playerRenderers.Length > 0)
        {
            // Store original materials if not already stored
            if (originalMaterials == null)
            {
                originalMaterials = new Material[playerRenderers.Length];
                for (int i = 0; i < playerRenderers.Length; i++)
                {
                    if (playerRenderers[i] != null)
                    {
                        originalMaterials[i] = playerRenderers[i].material;
                    }
                }
            }
            
            // Apply invulnerable material
            foreach (Renderer renderer in playerRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = invulnerableMaterial;
                }
            }
        }
        
        //Debug.Log("[PowerupManager] Invulnerability activated!");
    }

    private void DeactivateInvulnerability()
    {
        if (playerHealth != null)
        {
            playerHealth.SetInvulnerable(false);
        }
        
        // Restore original materials
        if (originalMaterials != null && playerRenderers.Length > 0)
        {
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                if (playerRenderers[i] != null && originalMaterials[i] != null)
                {
                    playerRenderers[i].material = originalMaterials[i];
                }
            }
        }
        
        //Debug.Log("[PowerupManager] Invulnerability deactivated");
    }

    private void DeactivateEffects()
    {
        // Reset speed in SO
        if (playerData != null)
        {
            playerData.currentSpeedMultiplier = baseSpeedMultiplier;
        }
        else if (player2Data != null)
        {
            player2Data.currentSpeedMultiplier = baseSpeedMultiplier;
        }
        
        // Reset speed in movement component
        if (playerMovement != null)
        {
            playerMovement.SetSpeedMultiplier(baseSpeedMultiplier);
        }
        
        totalSpeedMultiplier = 1f;
        
        // Remove invulnerability
        if (isInvulnerable)
        {
            DeactivateInvulnerability();
        }
        
        isInvulnerable = false;
        
        // Remove particle effect
        if (activeParticleEffect != null)
        {
            Destroy(activeParticleEffect);
            activeParticleEffect = null;
        }
        
        //Debug.Log("[PowerupManager] All powerup effects deactivated");
    }

    /// <summary>
    /// Get the remaining time of the longest active powerup
    /// </summary>
    public float GetRemainingTime()
    {
        float maxTime = 0f;
        foreach (PowerupEffect powerup in activePowerups)
        {
            float remaining = powerup.GetRemainingTime();
            if (remaining > maxTime)
            {
                maxTime = remaining;
            }
        }
        return maxTime;
    }

    /// <summary>
    /// Check if the player currently has any active powerups
    /// </summary>
    public bool HasActivePowerup()
    {
        return activePowerupCount > 0;
    }

    /// <summary>
    /// Get the number of currently active stacked powerups
    /// </summary>
    public int GetActivePowerupCount()
    {
        return activePowerupCount;
    }

    /// <summary>
    /// Check if the player is currently invulnerable from powerups
    /// </summary>
    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

    /// <summary>
    /// Get the current total speed multiplier
    /// </summary>
    public float GetSpeedMultiplier()
    {
        return totalSpeedMultiplier;
    }

    private void OnDestroy()
    {
        // Clean up when player is destroyed
        DeactivateEffects();
    }
}
