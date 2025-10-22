using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Diagnostic tool to check animator setup on Player prefabs and spawned instances.
/// Focuses on prefab spawn approach used by PlayerSpawnManager.
/// Run from Editor menu: Tools > Animator Diagnostics
/// </summary>
public class AnimatorDiagnostics : MonoBehaviour
{
    [Header("Manual Prefab Check")]
    [SerializeField] private GameObject playerPrefabToCheck;
    [SerializeField] private GameObject player2PrefabToCheck;
    
    [Header("Runtime Check")]
    [SerializeField] private bool checkSpawnedPlayerOnStart = false;
    [SerializeField] private float delayBeforeCheck = 1f;
    
    private void Start()
    {
        if (checkSpawnedPlayerOnStart)
        {
            StartCoroutine(CheckSpawnedPlayerAfterDelay());
        }
    }
    
    private System.Collections.IEnumerator CheckSpawnedPlayerAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeCheck);
        CheckSpawnedPlayer();
    }
    
    [ContextMenu("Check Prefab References")]
    public void CheckPrefabReferences()
    {
        if (playerPrefabToCheck != null)
        {
            Debug.Log("=== Checking Player1 Prefab ===");
            CheckPrefab(playerPrefabToCheck, "Player1");
        }
        else
        {
            Debug.LogWarning("Player prefab not assigned!");
        }
        
        if (player2PrefabToCheck != null)
        {
            Debug.Log("=== Checking Player2 Prefab ===");
            CheckPrefab(player2PrefabToCheck, "Player2");
        }
        else
        {
            Debug.LogWarning("Player2 prefab not assigned!");
        }
    }
    
    [ContextMenu("Check Spawned Player")]
    public void CheckSpawnedPlayer()
    {
        Debug.Log("=== Checking Spawned Player Instance ===");
        
        // Find all players including those in DontDestroyOnLoad
        Player[] players = FindObjectsOfType<Player>(true);
        
        if (players.Length == 0)
        {
            Debug.LogWarning("No spawned Player found! Make sure to spawn a player first.");
            return;
        }
        
        if (players.Length > 1)
        {
            Debug.LogWarning($"Multiple Players found ({players.Length})! This may indicate duplicate spawning.");
        }
        
        foreach (Player player in players)
        {
            string playerType = player is Player2 ? "Player2 (Assassin)" : "Player1 (Ranger)";
            Debug.Log($"\n--- Checking {playerType}: {player.gameObject.name} ---");
            Debug.Log($"Scene: {player.gameObject.scene.name}");
            CheckPlayerInstance(player);
        }
    }
    
    private void CheckPrefab(GameObject prefab, string prefabName)
    {
        Entity entity = prefab.GetComponent<Entity>();
        Player player = prefab.GetComponent<Player>();
        
        if (entity == null)
        {
            Debug.LogError($"[FAIL] {prefabName}: No Entity component found!", prefab);
            return;
        }
        
        if (player == null)
        {
            Debug.LogError($"[FAIL] {prefabName}: No Player component found!", prefab);
            return;
        }
        
        // Check for Animator Override field using reflection
        System.Reflection.FieldInfo animOverrideField = typeof(Entity).GetField("animatorOverride", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (animOverrideField != null)
        {
            Animator animOverride = animOverrideField.GetValue(entity) as Animator;
            if (animOverride != null)
            {
                Debug.Log($"[OK] {prefabName}: Animator Override is assigned -> {animOverride.gameObject.name}");
            }
            else
            {
                Debug.LogError($"[CRITICAL] {prefabName}: Animator Override field is NOT assigned! This will cause build issues!", prefab);
            }
        }
        
        // Check for Animator in children
        Animator[] animators = prefab.GetComponentsInChildren<Animator>(true);
        Debug.Log($"[INFO] {prefabName}: Found {animators.Length} Animator(s) in hierarchy");
        
        foreach (Animator anim in animators)
        {
            Debug.Log($"  - Animator on: {anim.gameObject.name}");
            
            if (anim.runtimeAnimatorController == null)
            {
                Debug.LogWarning($"  [WARN] Animator Controller is NULL on {anim.gameObject.name}!", anim);
            }
            else
            {
                Debug.Log($"  [OK] Animator Controller: {anim.runtimeAnimatorController.name}");
            }
        }
        
        // Check for PlayerAnimatorValidator
        PlayerAnimatorValidator validator = prefab.GetComponent<PlayerAnimatorValidator>();
        if (validator != null)
        {
            Debug.Log($"[OK] {prefabName}: PlayerAnimatorValidator component is attached");
        }
        else
        {
            Debug.LogWarning($"[WARN] {prefabName}: PlayerAnimatorValidator component is NOT attached (optional but recommended)", prefab);
        }
    }
    
    private void CheckPlayerInstance(Player player)
    {
        GameObject go = player.gameObject;
        
        Debug.Log($"Active in Hierarchy: {go.activeInHierarchy}");
        Debug.Log($"Active Self: {go.activeSelf}");
        
        // Check animator via reflection (since it has protected setter)
        System.Reflection.PropertyInfo animProp = typeof(Entity).GetProperty("anim");
        if (animProp != null)
        {
            Animator anim = animProp.GetValue(player) as Animator;
            if (anim != null)
            {
                Debug.Log($"[OK] player.anim is assigned -> {anim.gameObject.name}");
                if (anim.runtimeAnimatorController != null)
                {
                    Debug.Log($"[OK] Animator Controller: {anim.runtimeAnimatorController.name}");
                }
                else
                {
                    Debug.LogWarning($"[WARN] Animator Controller is NULL!", anim);
                }
            }
            else
            {
                Debug.LogError($"[FAIL] player.anim is NULL!", player);
            }
        }
        
        // Check for Animator Override field
        System.Reflection.FieldInfo animOverrideField = typeof(Entity).GetField("animatorOverride",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (animOverrideField != null)
        {
            Animator animOverride = animOverrideField.GetValue(player) as Animator;
            if (animOverride != null)
            {
                Debug.Log($"[OK] Animator Override field is assigned -> {animOverride.gameObject.name}");
            }
            else
            {
                Debug.LogError($"[FAIL] Animator Override field is NULL!", player);
            }
        }
        
        // List all animators in hierarchy
        Animator[] animators = go.GetComponentsInChildren<Animator>(true);
        Debug.Log($"Total Animators in hierarchy: {animators.Length}");
        foreach (Animator anim in animators)
        {
            Debug.Log($"  - {anim.gameObject.name} (Active: {anim.gameObject.activeInHierarchy})");
        }
        
        // Check current state (state machine is protected, but we can check current state)
        if (player.CurrentState != null)
        {
            Debug.Log($"[OK] StateMachine is initialized");
            Debug.Log($"  Current State: {player.CurrentState.GetType().Name}");
        }
        else
        {
            Debug.LogWarning($"[WARN] Current State is NULL (StateMachine may not be initialized yet)");
        }
    }
    
#if UNITY_EDITOR
    [MenuItem("Tools/Animator Diagnostics/Check All Player Prefabs")]
    private static void CheckAllPlayerPrefabsMenuItem()
    {
        Debug.Log("=== Scanning for Player Prefabs ===");
        
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int checkedCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null)
            {
                Player player = prefab.GetComponent<Player>();
                if (player != null)
                {
                    checkedCount++;
                    string prefabName = player is Player2 ? "Player2" : "Player1";
                    Debug.Log($"\n=== Checking {prefabName} at {path} ===");
                    
                    AnimatorDiagnostics diagnostics = new GameObject("TempDiagnostics").AddComponent<AnimatorDiagnostics>();
                    diagnostics.CheckPrefab(prefab, prefabName);
                    DestroyImmediate(diagnostics.gameObject);
                }
            }
        }
        
        Debug.Log($"\n=== Scan Complete: {checkedCount} player prefabs checked ===");
    }
    
    [MenuItem("Tools/Animator Diagnostics/Check Spawned Player")]
    private static void CheckSpawnedPlayerMenuItem()
    {
        if (!UnityEngine.Application.isPlaying)
        {
            Debug.LogWarning("[WARN] This check requires Play Mode! Please enter Play Mode and spawn a player first.");
            return;
        }
        
        AnimatorDiagnostics diagnostics = new GameObject("TempDiagnostics").AddComponent<AnimatorDiagnostics>();
        diagnostics.CheckSpawnedPlayer();
        DestroyImmediate(diagnostics.gameObject);
    }
#endif
}
