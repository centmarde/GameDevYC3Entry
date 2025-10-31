using UnityEngine;

/// <summary>
/// Debug script to test the wave progress system
/// Attach to any GameObject to test wave tracking and death scenarios
/// </summary>
public class WaveProgressTester : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private bool enableTestKeys = true;
    
    [Header("Debug Info")]
    [SerializeField] private int currentWaveDisplay = 0;
    [SerializeField] private int highestWaveDisplay = 0;
    [SerializeField] private bool photonConnected = false;

    private void Update()
    {
        if (!enableTestKeys) return;

        // Update display info
        if (PhotonGameManager.Instance != null)
        {
            currentWaveDisplay = PhotonGameManager.Instance.GetCurrentWave();
            highestWaveDisplay = PhotonGameManager.Instance.GetHighestWave();
            photonConnected = PhotonGameManager.Instance.IsConnectedToPhoton();
        }

        // Test keys
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestWaveProgression();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            TestPlayerDeath();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            TestPhotonConnection();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            TestWaveManagerConnection();
        }
    }

    /// <summary>
    /// Test wave progression (T key)
    /// </summary>
    private void TestWaveProgression()
    {
        if (PhotonGameManager.Instance == null)
        {
            Debug.LogError("[WaveProgressTester] PhotonGameManager not found!");
            return;
        }

        Debug.Log($"[WaveProgressTester] Before - Current: {currentWaveDisplay}, Highest: {highestWaveDisplay}");
        
        // Simulate wave progression by manually triggering wave events
        if (PhotonGameManager.Instance != null)
        {
            // This simulates what happens during normal gameplay
            int newWave = currentWaveDisplay + 1;
            Debug.Log($"[WaveProgressTester] Simulating wave {newWave} start...");
            
            // Note: This would normally be called by WaveManager events
            // For testing, we can't directly access the private method, so we'll just log
            Debug.Log($"[WaveProgressTester] Wave {newWave} would be stored locally (not pushed to Photon)");
        }

        Debug.Log($"[WaveProgressTester] After - Current: {currentWaveDisplay}, Highest: {highestWaveDisplay}");
    }

    /// <summary>
    /// Test player death scenario (Y key)
    /// </summary>
    private void TestPlayerDeath()
    {
        if (PhotonGameManager.Instance == null)
        {
            Debug.LogError("[WaveProgressTester] PhotonGameManager not found!");
            return;
        }

        Debug.Log($"[WaveProgressTester] TESTING PLAYER DEATH - This should push to Photon Cloud");
        Debug.Log($"[WaveProgressTester] Before Death - Current: {currentWaveDisplay}, Highest: {highestWaveDisplay}, Photon: {photonConnected}");
        
        // This is what Player.EntityDeath() calls
        PhotonGameManager.Instance.SaveCurrentWaveToLeaderboard();
        
        Debug.Log($"[WaveProgressTester] Death simulation complete - Check console for Photon Cloud push logs");
    }

    /// <summary>
    /// Test Photon connection status (U key)
    /// </summary>
    private void TestPhotonConnection()
    {
        if (PhotonGameManager.Instance == null)
        {
            Debug.LogError("[WaveProgressTester] PhotonGameManager not found!");
            return;
        }

        Debug.Log($"[WaveProgressTester] Photon Connection Status:");
        Debug.Log($"  - Connected: {PhotonGameManager.Instance.IsConnectedToPhoton()}");
        Debug.Log($"  - In Lobby: {PhotonGameManager.Instance.IsInLobby()}");
        Debug.Log($"  - Player Count: {PhotonGameManager.Instance.GetPlayerCount()}");
        Debug.Log($"  - Player Name: {PhotonGameManager.Instance.GetPlayerName()}");
        Debug.Log($"  - Current Wave: {PhotonGameManager.Instance.GetCurrentWave()}");
        Debug.Log($"  - Highest Wave: {PhotonGameManager.Instance.GetHighestWave()}");
    }

    /// <summary>
    /// Test WaveManager connection (I key)
    /// </summary>
    private void TestWaveManagerConnection()
    {
        if (PhotonGameManager.Instance == null)
        {
            Debug.LogError("[WaveProgressTester] PhotonGameManager not found!");
            return;
        }

        Debug.Log("[WaveProgressTester] Running WaveManager connection debug...");
        PhotonGameManager.Instance.DebugWaveManagerConnection();
    }

    private void OnGUI()
    {
        if (!enableTestKeys) return;

        // Show test controls in top-left corner
        GUI.Box(new Rect(10, 10, 300, 120), "Wave Progress Tester");
        
        GUI.Label(new Rect(20, 35, 280, 20), $"Current Wave: {currentWaveDisplay}");
        GUI.Label(new Rect(20, 55, 280, 20), $"Highest Wave: {highestWaveDisplay}");
        GUI.Label(new Rect(20, 75, 280, 20), $"Photon Connected: {photonConnected}");
        
        GUI.Label(new Rect(20, 95, 280, 20), "T = Test Wave Progression");
        GUI.Label(new Rect(20, 110, 280, 20), "Y = Test Player Death (Push to Cloud)");
        GUI.Label(new Rect(20, 125, 280, 20), "U = Test Photon Status");
        GUI.Label(new Rect(150, 125, 280, 20), "I = Debug WaveManager Connection");
    }
}