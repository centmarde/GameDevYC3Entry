using UnityEngine;

public class Player_RollAnimEvents : MonoBehaviour
{
    private Player_Roll rollComponent;

    private void Awake()
    {
        // Find the roll component in the parent (usually the Player)
        rollComponent = GetComponentInParent<Player_Roll>();
    }

    // Called by Animation Event at the start of the roll animation
    public void OnRollStart()
    {
        if (rollComponent != null)
        {
            // Use the player's current forward direction
            Vector3 dir = rollComponent.transform.forward;
            rollComponent.BeginRoll(dir);
        }
    }

    // Called by Animation Event at the end of the roll animation
    public void OnRollEnd()
    {
        if (rollComponent != null)
        {
            rollComponent.EndRoll();
        }
    }
}
