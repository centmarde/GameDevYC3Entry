using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera camera2;

    [Header("Input Settings")]
    [SerializeField] private bool useKeyboard = true;

    private Camera currentActiveCamera;
    private bool isMainCameraActive = true;

    private void Start()
    {
        // Initialize camera references if not set
        if (mainCamera == null)
        {
            GameObject mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
            if (mainCamObj != null)
                mainCamera = mainCamObj.GetComponent<Camera>();
        }

        if (camera2 == null)
        {
            GameObject cam2Obj = GameObject.Find("Camera2");
            if (cam2Obj != null)
                camera2 = cam2Obj.GetComponent<Camera>();
        }

        // Validate cameras
        if (mainCamera == null)
        {
            Debug.LogError("CameraSwitcher: Main Camera is not assigned!");
            enabled = false;
            return;
        }

        if (camera2 == null)
        {
            Debug.LogError("CameraSwitcher: Camera2 is not assigned!");
            enabled = false;
            return;
        }

        // Set initial state - Main Camera active
        currentActiveCamera = mainCamera;
        mainCamera.enabled = true;
        camera2.enabled = false;

        // Ensure AudioListener is properly set
        SetupAudioListener(mainCamera);
    }

    private void Update()
    {
        if (useKeyboard && Keyboard.current != null)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                SwitchCamera();
            }
        }
    }

    /// <summary>
    /// Switches between Main Camera and Camera2
    /// </summary>
    public void SwitchCamera()
    {
        if (mainCamera == null || camera2 == null)
        {
            Debug.LogWarning("CameraSwitcher: Cannot switch - cameras not properly assigned.");
            return;
        }

        isMainCameraActive = !isMainCameraActive;

        if (isMainCameraActive)
        {
            ActivateCamera(mainCamera);
            DeactivateCamera(camera2);
            Debug.Log("Switched to Main Camera");
        }
        else
        {
            ActivateCamera(camera2);
            DeactivateCamera(mainCamera);
            Debug.Log("Switched to Camera2");
        }
    }

    /// <summary>
    /// Switches to Main Camera
    /// </summary>
    public void SwitchToMainCamera()
    {
        if (mainCamera == null || !isMainCameraActive) return;

        isMainCameraActive = true;
        ActivateCamera(mainCamera);
        DeactivateCamera(camera2);
        Debug.Log("Switched to Main Camera");
    }

    /// <summary>
    /// Switches to Camera2
    /// </summary>
    public void SwitchToCamera2()
    {
        if (camera2 == null || isMainCameraActive) return;

        isMainCameraActive = false;
        ActivateCamera(camera2);
        DeactivateCamera(mainCamera);
        Debug.Log("Switched to Camera2");
    }

    /// <summary>
    /// Activates a camera and sets up audio listener
    /// </summary>
    private void ActivateCamera(Camera cam)
    {
        cam.enabled = true;
        currentActiveCamera = cam;
        SetupAudioListener(cam);
    }

    /// <summary>
    /// Deactivates a camera and removes audio listener
    /// </summary>
    private void DeactivateCamera(Camera cam)
    {
        cam.enabled = false;
        RemoveAudioListener(cam);
    }

    /// <summary>
    /// Ensures only one AudioListener is active on the camera
    /// </summary>
    private void SetupAudioListener(Camera cam)
    {
        AudioListener listener = cam.GetComponent<AudioListener>();
        if (listener == null)
        {
            cam.gameObject.AddComponent<AudioListener>();
        }
        else
        {
            listener.enabled = true;
        }
    }

    /// <summary>
    /// Removes or disables AudioListener from the camera
    /// </summary>
    private void RemoveAudioListener(Camera cam)
    {
        AudioListener listener = cam.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = false;
        }
    }

    /// <summary>
    /// Gets the currently active camera
    /// </summary>
    public Camera GetActiveCamera()
    {
        return currentActiveCamera;
    }

    /// <summary>
    /// Checks if Main Camera is currently active
    /// </summary>
    public bool IsMainCameraActive()
    {
        return isMainCameraActive;
    }

    /// <summary>
    /// Enables or disables keyboard input for camera switching
    /// </summary>
    public void SetUseKeyboard(bool use)
    {
        useKeyboard = use;
    }
}
