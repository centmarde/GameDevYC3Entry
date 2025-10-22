using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor tool to automatically fix Player prefabs by assigning the Animator Override field.
/// This ensures animations work correctly after prefab instantiation.
/// Usage: Tools > Player Prefab Fixer > Fix All Player Prefabs
/// </summary>
public class PlayerPrefabFixer : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Player Prefab Fixer/Fix All Player Prefabs")]
    private static void FixAllPlayerPrefabs()
    {
        Debug.Log("[PlayerPrefabFixer] Starting to fix all Player prefabs...");
        
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        int fixedCount = 0;
        int totalChecked = 0;
        
        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                Player player = prefab.GetComponent<Player>();
                if (player != null)
                {
                    totalChecked++;
                    string playerType = player is Player2 ? "Player2 (Assassin)" : "Player1 (Ranger)";
                    Debug.Log($"\n[PlayerPrefabFixer] Checking {playerType} at {path}");
                    
                    if (FixPrefab(prefab, path))
                    {
                        fixedCount++;
                        Debug.Log($"[PlayerPrefabFixer] ✅ Fixed {playerType}");
                    }
                    else
                    {
                        Debug.Log($"[PlayerPrefabFixer] ℹ️ {playerType} already correctly configured");
                    }
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"\n[PlayerPrefabFixer] Complete! Checked {totalChecked} player prefabs, fixed {fixedCount}");
        
        if (fixedCount > 0)
        {
            EditorUtility.DisplayDialog(
                "Player Prefab Fixer", 
                $"Successfully fixed {fixedCount} out of {totalChecked} player prefabs!\n\nThe Animator Override field has been assigned for proper animation playback.", 
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Player Prefab Fixer", 
                $"All {totalChecked} player prefabs are already correctly configured!\n\nNo fixes were needed.", 
                "OK"
            );
        }
    }
    
    [MenuItem("Tools/Player Prefab Fixer/Validate Player Prefabs")]
    private static void ValidateAllPlayerPrefabs()
    {
        Debug.Log("[PlayerPrefabFixer] Validating all Player prefabs...");
        
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        int validCount = 0;
        int invalidCount = 0;
        int missingControllerCount = 0;
        
        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                Player player = prefab.GetComponent<Player>();
                if (player != null)
                {
                    string playerType = player is Player2 ? "Player2 (Assassin)" : "Player1 (Ranger)";
                    
                    // Check if animatorOverride is assigned
                    Animator animOverride = player.AnimatorOverride;
                    Animator childAnimator = prefab.GetComponentInChildren<Animator>(true);
                    
                    if (animOverride != null)
                    {
                        // Check if animator has controller
                        if (animOverride.runtimeAnimatorController == null)
                        {
                            missingControllerCount++;
                            Debug.LogError($"[PlayerPrefabFixer] ⚠️ {playerType} - Animator Override assigned BUT RuntimeAnimatorController is NULL!\n  Path: {path}", prefab);
                        }
                        else if (!animOverride.enabled)
                        {
                            Debug.LogWarning($"[PlayerPrefabFixer] ⚠️ {playerType} - Animator is disabled! This may cause issues.\n  Path: {path}", prefab);
                            validCount++;
                        }
                        else
                        {
                            validCount++;
                            Debug.Log($"[PlayerPrefabFixer] ✅ {playerType} is valid - Animator Override: {animOverride.gameObject.name}, Controller: {animOverride.runtimeAnimatorController.name}");
                        }
                    }
                    else if (childAnimator != null)
                    {
                        invalidCount++;
                        Debug.LogWarning($"[PlayerPrefabFixer] ⚠️ {playerType} NEEDS FIX - Animator Override is NULL but child animator exists: {childAnimator.gameObject.name}\n  Path: {path}", prefab);
                    }
                    else
                    {
                        invalidCount++;
                        Debug.LogError($"[PlayerPrefabFixer] ❌ {playerType} CRITICAL - No animator found anywhere!\n  Path: {path}", prefab);
                    }
                }
            }
        }
        
        Debug.Log($"\n[PlayerPrefabFixer] Validation complete! Valid: {validCount}, Invalid: {invalidCount}, Missing Controller: {missingControllerCount}");
        
        if (invalidCount > 0)
        {
            bool fix = EditorUtility.DisplayDialog(
                "Player Prefab Validation", 
                $"Found {invalidCount} prefab(s) that need fixing!\n\nWould you like to fix them now?", 
                "Fix Now", 
                "Cancel"
            );
            
            if (fix)
            {
                FixAllPlayerPrefabs();
            }
        }
        else if (missingControllerCount > 0)
        {
            EditorUtility.DisplayDialog(
                "Player Prefab Validation", 
                $"All prefabs have Animator Override assigned, but {missingControllerCount} are missing RuntimeAnimatorController!\n\nPlease assign the Animator Controller manually in the Inspector.", 
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Player Prefab Validation", 
                $"All {validCount} player prefabs are correctly configured! ✅", 
                "OK"
            );
        }
    }
    
    private static bool FixPrefab(GameObject prefab, string path)
    {
        Player player = prefab.GetComponent<Player>();
        if (player == null) return false;
        
        // Check if already fixed
        if (player.AnimatorOverride != null)
        {
            return false; // Already fixed
        }
        
        // Find animator in children
        Animator childAnimator = prefab.GetComponentInChildren<Animator>(true);
        if (childAnimator == null)
        {
            Debug.LogError($"[PlayerPrefabFixer] Cannot fix {prefab.name} - No Animator found in hierarchy!", prefab);
            return false;
        }
        
        // Use SerializedObject to assign the field
        SerializedObject serializedPrefab = new SerializedObject(player);
        SerializedProperty animatorOverrideProperty = serializedPrefab.FindProperty("animatorOverride");
        
        if (animatorOverrideProperty != null)
        {
            animatorOverrideProperty.objectReferenceValue = childAnimator;
            serializedPrefab.ApplyModifiedProperties();
            
            // Save the prefab
            PrefabUtility.SavePrefabAsset(prefab);
            
            Debug.Log($"[PlayerPrefabFixer] Assigned Animator Override: {childAnimator.gameObject.name} -> {player.gameObject.name}");
            return true;
        }
        else
        {
            Debug.LogError($"[PlayerPrefabFixer] Could not find 'animatorOverride' property on {prefab.name}!");
            return false;
        }
    }
    
    [MenuItem("Tools/Player Prefab Fixer/Show Animator Hierarchy")]
    private static void ShowAnimatorHierarchy()
    {
        Debug.Log("[PlayerPrefabFixer] Scanning for Player prefabs and their animator structure...");
        
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        
        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                Player player = prefab.GetComponent<Player>();
                if (player != null)
                {
                    string playerType = player is Player2 ? "Player2 (Assassin)" : "Player1 (Ranger)";
                    Debug.Log($"\n=== {playerType} Hierarchy ===");
                    Debug.Log($"Path: {path}");
                    Debug.Log($"Animator Override Field: {(player.AnimatorOverride != null ? player.AnimatorOverride.gameObject.name : "NULL")}");
                    
                    // Show all animators in hierarchy
                    Animator[] allAnimators = prefab.GetComponentsInChildren<Animator>(true);
                    Debug.Log($"Total Animators found in hierarchy: {allAnimators.Length}");
                    
                    foreach (Animator anim in allAnimators)
                    {
                        string animPath = GetGameObjectPath(anim.transform);
                        Debug.Log($"  - {anim.gameObject.name} (Active: {anim.gameObject.activeInHierarchy})");
                        Debug.Log($"    Full path: {animPath}");
                        Debug.Log($"    Controller: {(anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "NULL")}");
                    }
                }
            }
        }
    }
    
    private static string GetGameObjectPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
#endif
}
