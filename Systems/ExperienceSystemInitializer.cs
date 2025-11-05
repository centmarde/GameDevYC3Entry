using UnityEngine;

/// <summary>
/// Automatically initializes the experience system when the scene loads.
/// Add this to any GameObject in your scene (or it will auto-create itself).
/// </summary>
public class ExperienceSystemInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        // Check if ExperienceManager exists
        if (ExperienceManager.Instance == null)
        {
            GameObject managerObj = new GameObject("ExperienceManager");
            managerObj.AddComponent<ExperienceManager>();
            Debug.Log("[ExperienceSystemInitializer] Created ExperienceManager");
        }

        // Check if ExperienceUI exists
        if (FindObjectOfType<ExperienceUI>() == null)
        {
            // Create UI GameObject
            GameObject uiObj = new GameObject("ExperienceUI");
            uiObj.AddComponent<ExperienceUI>();
            Debug.Log("[ExperienceSystemInitializer] Created ExperienceUI - it will auto-setup the UI");
        }

        Debug.Log("[ExperienceSystemInitializer] Experience system initialized!");
    }
}
