#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Convenient Editor Window for fixing and validating Player prefab animators.
/// Access via: Tools > Animator Fix Window
/// </summary>
public class AnimatorFixWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private bool showDetailedInfo = false;
    
    [MenuItem("Tools/Animator Fix Window")]
    public static void ShowWindow()
    {
        AnimatorFixWindow window = GetWindow<AnimatorFixWindow>("Animator Fix");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Header
        GUILayout.Space(10);
        GUILayout.Label("Player Animator Fix Tool", EditorStyles.boldLabel);
        GUILayout.Label("Quick fix for player animation issues", EditorStyles.miniLabel);
        
        GUILayout.Space(20);
        
        // Main Fix Button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🔧 AUTO-FIX ALL PLAYER PREFABS", GUILayout.Height(40)))
        {
            FixAllPrefabs();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(10);
        
        // Validation Button
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("✅ VALIDATE PLAYER PREFABS", GUILayout.Height(30)))
        {
            ValidatePrefabs();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(20);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);
        
        // Additional Tools Section
        GUILayout.Label("Additional Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("📋 Show Prefab Hierarchy Details"))
        {
            ShowHierarchyDetails();
        }
        
        GUILayout.Space(5);
        
        if (Application.isPlaying)
        {
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("🔍 Check Spawned Player (Play Mode)"))
            {
                CheckSpawnedPlayer();
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Button("🔍 Check Spawned Player (Enter Play Mode First)");
            EditorGUI.EndDisabledGroup();
        }
        
        GUILayout.Space(20);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);
        
        // Status Section
        GUILayout.Label("Status & Info", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "This tool will:\n" +
            "• Find all Player prefabs\n" +
            "• Assign Animator Override fields\n" +
            "• Validate animator controllers\n" +
            "• Fix common animation issues",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        showDetailedInfo = EditorGUILayout.Foldout(showDetailedInfo, "Show Detailed Information");
        if (showDetailedInfo)
        {
            EditorGUILayout.HelpBox(
                "Common Issues Fixed:\n\n" +
                "1. Animator Override field not assigned\n" +
                "   → Auto-assigns child animator\n\n" +
                "2. RuntimeAnimatorController missing\n" +
                "   → Reports error for manual fix\n\n" +
                "3. Animator component disabled\n" +
                "   → Reports warning\n\n" +
                "4. Multiple animators in hierarchy\n" +
                "   → Shows all found animators",
                MessageType.None
            );
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("📖 Open Full Documentation"))
        {
            string guidePath = "Assets/Scripts/DevTools/ANIMATOR_FIX_GUIDE.md";
            string quickGuidePath = "Assets/ANIMATOR_FIX_INSTRUCTIONS.md";
            
            UnityEngine.Object guideAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(quickGuidePath);
            if (guideAsset != null)
            {
                Selection.activeObject = guideAsset;
                EditorGUIUtility.PingObject(guideAsset);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Documentation",
                    "Guide files:\n" +
                    "• ANIMATOR_FIX_INSTRUCTIONS.md (Quick Start)\n" +
                    "• Assets/Scripts/DevTools/ANIMATOR_FIX_GUIDE.md (Full Guide)",
                    "OK"
                );
            }
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void FixAllPrefabs()
    {
        Debug.Log("[AnimatorFixWindow] Starting auto-fix...");
        
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
                    if (FixSinglePrefab(prefab, path))
                    {
                        fixedCount++;
                    }
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog(
            "Auto-Fix Complete",
            $"Checked: {totalChecked} prefabs\n" +
            $"Fixed: {fixedCount} prefabs\n\n" +
            (fixedCount > 0 ? "✅ Prefabs have been updated!" : "ℹ️ All prefabs were already configured correctly."),
            "OK"
        );
        
        Debug.Log($"[AnimatorFixWindow] Complete! Fixed {fixedCount}/{totalChecked} prefabs");
    }
    
    private bool FixSinglePrefab(GameObject prefab, string path)
    {
        Player player = prefab.GetComponent<Player>();
        if (player == null) return false;
        
        // Check if already fixed
        if (player.AnimatorOverride != null)
        {
            return false;
        }
        
        // Find animator in children
        Animator childAnimator = prefab.GetComponentInChildren<Animator>(true);
        if (childAnimator == null)
        {
            Debug.LogError($"[AnimatorFixWindow] No Animator found in {prefab.name}!", prefab);
            return false;
        }
        
        // Assign using SerializedObject
        SerializedObject serializedPrefab = new SerializedObject(player);
        SerializedProperty animatorOverrideProperty = serializedPrefab.FindProperty("animatorOverride");
        
        if (animatorOverrideProperty != null)
        {
            animatorOverrideProperty.objectReferenceValue = childAnimator;
            serializedPrefab.ApplyModifiedProperties();
            PrefabUtility.SavePrefabAsset(prefab);
            
            string playerType = player is Player2 ? "Player2" : "Player1";
            Debug.Log($"[AnimatorFixWindow] ✅ Fixed {playerType}: {childAnimator.gameObject.name} → {player.gameObject.name}");
            return true;
        }
        
        return false;
    }
    
    private void ValidatePrefabs()
    {
        Debug.Log("[AnimatorFixWindow] Validating prefabs...");
        
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
                    string playerType = player is Player2 ? "Player2" : "Player1";
                    Animator animOverride = player.AnimatorOverride;
                    
                    if (animOverride != null)
                    {
                        if (animOverride.runtimeAnimatorController == null)
                        {
                            missingControllerCount++;
                            Debug.LogWarning($"[AnimatorFixWindow] ⚠️ {playerType} - Missing Controller", prefab);
                        }
                        else
                        {
                            validCount++;
                            Debug.Log($"[AnimatorFixWindow] ✅ {playerType} - Valid");
                        }
                    }
                    else
                    {
                        invalidCount++;
                        Debug.LogError($"[AnimatorFixWindow] ❌ {playerType} - Animator Override not assigned", prefab);
                    }
                }
            }
        }
        
        string message = $"Validation Results:\n\n" +
                        $"✅ Valid: {validCount}\n" +
                        $"❌ Invalid: {invalidCount}\n" +
                        $"⚠️ Missing Controller: {missingControllerCount}";
        
        if (invalidCount > 0)
        {
            bool shouldFix = EditorUtility.DisplayDialog(
                "Validation Results",
                message + "\n\nWould you like to fix the invalid prefabs?",
                "Fix Now",
                "Cancel"
            );
            
            if (shouldFix)
            {
                FixAllPrefabs();
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Validation Results", message, "OK");
        }
    }
    
    private void ShowHierarchyDetails()
    {
        Debug.Log("[AnimatorFixWindow] === Prefab Hierarchy Details ===");
        
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
                    string playerType = player is Player2 ? "Player2" : "Player1";
                    Debug.Log($"\n=== {playerType} Hierarchy ===");
                    
                    Animator[] allAnimators = prefab.GetComponentsInChildren<Animator>(true);
                    Debug.Log($"Total Animators: {allAnimators.Length}");
                    
                    foreach (Animator anim in allAnimators)
                    {
                        Debug.Log($"  - {anim.gameObject.name}");
                        Debug.Log($"    Controller: {(anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "NULL")}");
                        Debug.Log($"    Enabled: {anim.enabled}");
                    }
                }
            }
        }
        
        Debug.Log("\n=== End of Hierarchy Details ===");
        EditorUtility.DisplayDialog("Hierarchy Details", "Hierarchy information has been logged to Console.", "OK");
    }
    
    private void CheckSpawnedPlayer()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Not in Play Mode", "Please enter Play Mode first!", "OK");
            return;
        }
        
        Player[] players = FindObjectsOfType<Player>(true);
        
        if (players.Length == 0)
        {
            EditorUtility.DisplayDialog("No Player Found", "No spawned Player found in scene!", "OK");
            return;
        }
        
        Debug.Log($"[AnimatorFixWindow] === Checking {players.Length} spawned player(s) ===");
        
        foreach (Player player in players)
        {
            string playerType = player is Player2 ? "Player2" : "Player1";
            Debug.Log($"\n--- {playerType}: {player.gameObject.name} ---");
            
            if (player.anim != null)
            {
                Debug.Log($"✅ Animator: {player.anim.gameObject.name}");
                Debug.Log($"   Enabled: {player.anim.enabled}");
                Debug.Log($"   Controller: {(player.anim.runtimeAnimatorController != null ? player.anim.runtimeAnimatorController.name : "NULL")}");
            }
            else
            {
                Debug.LogError($"❌ Animator is NULL!", player);
            }
        }
        
        EditorUtility.DisplayDialog("Spawned Player Check", "Check results have been logged to Console.", "OK");
    }
}
#endif
