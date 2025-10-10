using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Added for TextMeshPro support
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class IntroTrigger : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform cutscenePosition; // Position in front of player
    [SerializeField] private float cutsceneDuration = 5f;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private bool useVirtualCamera = false; // Use a separate cutscene camera
    
    [Header("Player Reference")]
    [SerializeField] private GameObject player;
    [SerializeField] private float distanceInFrontOfPlayer = 1f;
    [SerializeField] private float heightOffset = 1.5f;
    
    [Header("UI Settings")]
    [SerializeField] private Canvas dialogCanvas;
    [SerializeField] private TMP_Text dialogText; // Changed to TMP_Text for TextMeshPro support
    [SerializeField] private string[] dialogLines;
    [SerializeField] private float textDisplaySpeed = 0.05f;
    
    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnce = true;
    
    private Vector3 originalCameraLocalPosition;
    private Quaternion originalCameraLocalRotation;
    private Vector3 originalCameraWorldPosition;
    private Quaternion originalCameraWorldRotation;
    private Transform originalCameraParent;
    private bool hasTriggered = false;
    private bool isInCutscene = false;
    
    // For player control
    private MonoBehaviour[] playerScripts;
    private Rigidbody playerRigidbody;
    private CharacterController playerCharController;
    private Animator playerAnimator;
    
    // Store animation state
    private AnimatorStateInfo originalAnimState;
    private float originalAnimTime;
    
    void Start()
    {
        // Get main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Hide dialog canvas at start
        if (dialogCanvas != null)
        {
            dialogCanvas.gameObject.SetActive(false);
        }
        
        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        // Get player components for disabling during cutscene
        if (player != null)
        {
            playerRigidbody = player.GetComponent<Rigidbody>();
            playerCharController = player.GetComponent<CharacterController>();
            playerAnimator = player.GetComponent<Animator>();
            
            // If animator not on player, check children
            if (playerAnimator == null)
            {
                playerAnimator = player.GetComponentInChildren<Animator>();
            }
        }
        
        Debug.Log("IntroTrigger initialized. Camera: " + (mainCamera != null ? mainCamera.name : "NULL"));
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if player entered the trigger
        Debug.Log("Trigger entered by: " + other.name);
        
        if (other.CompareTag("Player") && !hasTriggered && !isInCutscene)
        {
            Debug.Log("Starting cutscene!");
            
            if (triggerOnce)
            {
                hasTriggered = true;
            }
            
            StartCoroutine(PlayCutscene());
        }
    }
    
    IEnumerator PlayCutscene()
    {
        isInCutscene = true;
        
        Debug.Log("Cutscene started!");
        
        // Save original camera transform (both local and world)
        originalCameraParent = mainCamera.transform.parent;
        originalCameraLocalPosition = mainCamera.transform.localPosition;
        originalCameraLocalRotation = mainCamera.transform.localRotation;
        originalCameraWorldPosition = mainCamera.transform.position;
        originalCameraWorldRotation = mainCamera.transform.rotation;
        
        Debug.Log("Original camera parent: " + (originalCameraParent != null ? originalCameraParent.name : "NULL"));
        Debug.Log("Original camera position: " + originalCameraWorldPosition);
        
        // Disable player controls
        DisablePlayerControl();
        
        // Calculate cutscene position in front of player
        Vector3 targetPosition;
        Quaternion targetRotation;
        
        if (cutscenePosition != null)
        {
            // Use predefined cutscene position
            targetPosition = cutscenePosition.position;
            targetRotation = cutscenePosition.rotation;
            Debug.Log("Using cutscene position object");
        }
        else
        {
            // Calculate position in front of player
            Vector3 playerForward = player.transform.forward;
            targetPosition = player.transform.position + 
                           playerForward * distanceInFrontOfPlayer + 
                           Vector3.up * heightOffset;
            
            // Look at player
            Vector3 lookAtTarget = player.transform.position + Vector3.up * heightOffset;
            targetRotation = Quaternion.LookRotation(lookAtTarget - targetPosition);
            
            Debug.Log("Calculated cutscene position: " + targetPosition);
        }
        
        // IMPORTANT: Detach camera from parent first!
        mainCamera.transform.SetParent(null);
        
        Debug.Log("Camera detached, moving to cutscene position");
        
        // Store start position for lerp
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;
        
        // Smoothly move camera to cutscene position
        float elapsed = 0f;
        float duration = 1f / transitionSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            mainCamera.transform.position = Vector3.Lerp(
                startPosition, 
                targetPosition, 
                t
            );
            mainCamera.transform.rotation = Quaternion.Slerp(
                startRotation, 
                targetRotation, 
                t
            );
            yield return null;
        }
        
        // Ensure final position is set
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;
        
        Debug.Log("Camera moved to: " + mainCamera.transform.position);
        
        // Show dialog canvas
        if (dialogCanvas != null)
        {
            dialogCanvas.gameObject.SetActive(true);
            
            // Display dialog lines
            if (dialogLines != null && dialogLines.Length > 0)
            {
                foreach (string line in dialogLines)
                {
                    yield return StartCoroutine(TypeText(line));
                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                // Wait for cutscene duration if no dialog
                yield return new WaitForSeconds(cutsceneDuration);
            }
            
            // Hide dialog canvas
            dialogCanvas.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(cutsceneDuration);
        }
        
        Debug.Log("Cutscene complete, returning camera");
        
        // Return camera to original position
        Vector3 currentPos = mainCamera.transform.position;
        Quaternion currentRot = mainCamera.transform.rotation;
        
        elapsed = 0f;
        duration = 1f / transitionSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            mainCamera.transform.position = Vector3.Lerp(
                currentPos, 
                originalCameraWorldPosition, 
                t
            );
            mainCamera.transform.rotation = Quaternion.Slerp(
                currentRot, 
                originalCameraWorldRotation, 
                t
            );
            yield return null;
        }
        
        // Reattach camera to original parent
        mainCamera.transform.SetParent(originalCameraParent);
        mainCamera.transform.localPosition = originalCameraLocalPosition;
        mainCamera.transform.localRotation = originalCameraLocalRotation;
        
        Debug.Log("Camera returned to original position");
        
        // Re-enable player controls
        EnablePlayerControl();
        
        isInCutscene = false;
    }
    
    IEnumerator TypeText(string text)
    {
        if (dialogText == null) yield break;
        
        dialogText.text = "";
        foreach (char letter in text.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(textDisplaySpeed);
        }
    }
    
    void DisablePlayerControl()
    {
        if (player == null) return;
        
        Debug.Log("Disabling player controls");
        
        // Set animation to idle state only (let animation continue playing)
        if (playerAnimator != null && playerAnimator.isActiveAndEnabled)
        {
            // Set your specific animation parameters to idle state
            TrySetAnimatorParameter("idle", true);          // Set Idle to true
            TrySetAnimatorParameter("poseOrLook", 0f);      // Set to 0 for idle pose
            TrySetAnimatorParameter("move", false);         // Set to false for no movement
            
            Debug.Log("Player animation set to idle");
        }
        
        // Disable all MonoBehaviour scripts on player (except this one)
        playerScripts = player.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in playerScripts)
        {
            if (script != this && script.enabled)
            {
                script.enabled = false;
            }
        }
        
        // Stop player movement
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true;
        }
        
        // Disable character controller if present
        if (playerCharController != null)
        {
            playerCharController.enabled = false;
        }
    }
    
    void TrySetAnimatorParameter(string paramName, float value)
    {
        if (playerAnimator != null)
        {
            foreach (AnimatorControllerParameter param in playerAnimator.parameters)
            {
                if (param.name == paramName && param.type == AnimatorControllerParameterType.Float)
                {
                    playerAnimator.SetFloat(paramName, value);
                    return;
                }
            }
        }
    }
    
    void TrySetAnimatorParameter(string paramName, bool value)
    {
        if (playerAnimator != null)
        {
            foreach (AnimatorControllerParameter param in playerAnimator.parameters)
            {
                if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
                {
                    playerAnimator.SetBool(paramName, value);
                    return;
                }
            }
        }
    }
    
    void EnablePlayerControl()
    {
        if (player == null) return;
        
        Debug.Log("Re-enabling player controls");
        
        // Keep animation in idle state when exiting cutscene
        if (playerAnimator != null && playerAnimator.isActiveAndEnabled)
        {
            // Keep idle state
            TrySetAnimatorParameter("idle", false);         // Reset Idle trigger (set to false when cutscene ends)
            TrySetAnimatorParameter("poseOrLook", 0f);      // Idle pose
            TrySetAnimatorParameter("move", false);         // No movement
            
            Debug.Log("Player animation restored to idle");
        }
        
        // Re-enable all MonoBehaviour scripts on player
        if (playerScripts != null)
        {
            foreach (MonoBehaviour script in playerScripts)
            {
                if (script != null)
                {
                    script.enabled = true;
                }
            }
        }
        
        // Re-enable player physics
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }
        
        // Re-enable character controller if present
        if (playerCharController != null)
        {
            playerCharController.enabled = true;
        }
    }
    
    public void SkipCutscene()
    {
        if (isInCutscene)
        {
            Debug.Log("Skipping cutscene");
            StopAllCoroutines();
            
            // Restore camera immediately
            mainCamera.transform.SetParent(originalCameraParent);
            mainCamera.transform.localPosition = originalCameraLocalPosition;
            mainCamera.transform.localRotation = originalCameraLocalRotation;
            
            // Hide dialog
            if (dialogCanvas != null)
            {
                dialogCanvas.gameObject.SetActive(false);
            }
            
            // Re-enable player
            EnablePlayerControl();
            
            isInCutscene = false;
        }
    }
    
    // Optional: Allow skipping with ESC key
    void Update()
    {
        if (isInCutscene)
        {
            #if ENABLE_INPUT_SYSTEM
            // New Input System
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SkipCutscene();
            }
            #else
            // Legacy Input Manager
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SkipCutscene();
            }
            #endif
        }
    }
}
