using UnityEngine;

/// <summary>
/// Debug tool for testing the evasion system
/// Attach to player or any entity with Entity_Health
/// </summary>
public class EvasionTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private Entity_Health targetHealth;
    [SerializeField] private float testDamage = 10f;
    [SerializeField] private KeyCode testKey = KeyCode.E;
    
    [Header("Statistics")]
    [SerializeField] private int totalTests = 0;
    [SerializeField] private int successfulEvasions = 0;
    [SerializeField] private float evasionRate = 0f;
    
    private void Awake()
    {
        if (targetHealth == null)
            targetHealth = GetComponent<Entity_Health>();
    }
    
    private void OnEnable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnEvaded += OnEvaded;
            targetHealth.OnDamaged += OnDamaged;
        }
    }
    
    private void OnDisable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnEvaded -= OnEvaded;
            targetHealth.OnDamaged -= OnDamaged;
        }
    }
    
    private void Update()
    {
        // Test evasion on key press
        if (Input.GetKeyDown(testKey))
        {
            TestEvasion();
        }
        
        // Reset stats on R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetStats();
        }
    }
    
    private void TestEvasion()
    {
        if (targetHealth == null)
        {
            Debug.LogError("EvasionTester: No Entity_Health component found!");
            return;
        }
        
        totalTests++;
        Vector3 hitPoint = transform.position;
        Vector3 hitNormal = Vector3.up;
        
        targetHealth.TakeDamage(testDamage, hitPoint, hitNormal, this);
    }
    
    private void OnEvaded(float damage, Vector3 hitPoint, Vector3 hitNormal, object source)
    {
        if (source == this) // Only count our test attacks
        {
            successfulEvasions++;
            UpdateEvasionRate();
            
            Debug.Log($"<color=cyan>EVADED!</color> ({successfulEvasions}/{totalTests}) - {evasionRate:F1}% success rate");
        }
    }
    
    private void OnDamaged(float damage, Vector3 hitPoint, Vector3 hitNormal, object source)
    {
        if (source == this) // Only count our test attacks
        {
            UpdateEvasionRate();
            
            Debug.Log($"<color=red>HIT!</color> {damage} damage taken. ({successfulEvasions}/{totalTests}) - {evasionRate:F1}% success rate");
        }
    }
    
    private void UpdateEvasionRate()
    {
        if (totalTests > 0)
            evasionRate = (successfulEvasions / (float)totalTests) * 100f;
    }
    
    private void ResetStats()
    {
        totalTests = 0;
        successfulEvasions = 0;
        evasionRate = 0f;
        
        Debug.Log("Evasion test stats reset!");
    }
    
    private void OnGUI()
    {
        // Display stats in top-left corner
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label($"<b>Evasion Tester</b>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 16 });
        GUILayout.Space(5);
        
        if (targetHealth != null && TryGetComponent<Player>(out var player))
        {
            GUILayout.Label($"Evasion Chance: {player.Stats.evasionChance:F1}%");
        }
        
        GUILayout.Label($"Total Tests: {totalTests}");
        GUILayout.Label($"Evasions: {successfulEvasions}");
        GUILayout.Label($"Success Rate: {evasionRate:F1}%");
        
        GUILayout.Space(5);
        GUILayout.Label($"Press [{testKey}] to test");
        GUILayout.Label("Press [R] to reset stats");
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    #if UNITY_EDITOR
    [ContextMenu("Run 100 Tests")]
    private void RunBatchTest()
    {
        StartCoroutine(BatchTestRoutine(100));
    }
    
    [ContextMenu("Run 1000 Tests")]
    private void RunLargeBatchTest()
    {
        StartCoroutine(BatchTestRoutine(1000));
    }
    
    private System.Collections.IEnumerator BatchTestRoutine(int count)
    {
        ResetStats();
        Debug.Log($"Starting batch test of {count} attacks...");
        
        for (int i = 0; i < count; i++)
        {
            TestEvasion();
            
            // Small delay to prevent lag
            if (i % 100 == 0)
                yield return null;
        }
        
        Debug.Log($"Batch test complete! Evasion rate: {evasionRate:F2}%");
        
        if (TryGetComponent<Player>(out var player))
        {
            float expectedRate = player.Stats.evasionChance;
            float difference = Mathf.Abs(evasionRate - expectedRate);
            
            if (difference < 5f)
                Debug.Log($"<color=green>Results are within expected range (±5%)</color>");
            else
                Debug.Log($"<color=yellow>Results differ from expected by {difference:F1}%</color>");
        }
    }
    #endif
}
