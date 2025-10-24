using UnityEngine;
using System;

/// <summary>
/// Global static event broadcaster for damage dealt by the player
/// Allows skills like Vampire Aura to listen for damage events
/// </summary>
public static class DamageEventBroadcaster
{
    /// <summary>
    /// Event fired when the player deals damage to an enemy
    /// Parameters: damageAmount, hitPosition, damageSource
    /// </summary>
    public static event Action<float, Vector3, object> OnPlayerDamageDealt;
    
    /// <summary>
    /// Broadcast that the player has dealt damage to an enemy
    /// </summary>
    /// <param name="damageAmount">Amount of damage dealt</param>
    /// <param name="hitPosition">World position where the damage was dealt</param>
    /// <param name="damageSource">Source of the damage (Player, projectile, etc.)</param>
    public static void BroadcastPlayerDamage(float damageAmount, Vector3 hitPosition, object damageSource)
    {
        OnPlayerDamageDealt?.Invoke(damageAmount, hitPosition, damageSource);
    }
}
