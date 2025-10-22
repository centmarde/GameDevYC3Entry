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
    [SerializeField] private GameObject[] players;
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
    private MonoBehaviour[][] playerScripts;
    private Rigidbody[] playerRigidbodies;
    private CharacterController[] playerCharControllers;
    private Animator[] playerAnimators;
    private GameObject triggeringPlayer; // Track which player triggered the cutscene
    private Player triggeringPlayerComponent; // Track triggering player's component for input control
    
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
        
        // Find players if not assigned
        if (players == null || players.Length == 0)
        {
            players = GameObject.FindGameObjectsWithTag("Player");
        }
        
        // Initialize arrays for player components
        if (players != null && players.Length > 0)
        {
            playerRigidbodies = new Rigidbody[players.Length];
            playerCharControllers = new CharacterController[players.Length];
            playerAnimators = new Animator[players.Length];
            playerScripts = new MonoBehaviour[players.Length][];
            
            // Get player components for disabling during cutscene
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                {
                    playerRigidbodies[i] = players[i].GetComponent<Rigidbody>();
                    playerCharControllers[i] = players[i].GetComponent<CharacterController>();
                    playerAnimators[i] = players[i].GetComponent<Animator>();
                    
                    // If animator not on player, check children
                    if (playerAnimators[i] == null)
                    {
                        playerAnimators[i] = players[i].GetComponentInChildren<Animator>();
                    }
                }
            }
        }
        
        //Debug.Log("IntroTrigger initialized. Camera: " + (mainCamera != null ? mainCamera.name : "NULL"));
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if player entered the trigger
        //Debug.Log("Trigger entered by: " + other.name);
        
        if (other.CompareTag("Player") && !hasTriggered && !isInCutscene)
        {
            //Debug.Log("Starting cutscene!");
            
            if (triggerOnce)
            {
                hasTriggered = true;
            }
            
            // Store which player triggered the cutscene
            triggeringPlayer = other.gameObject;
            triggeringPlayerComponent = other.GetComponent<Player>();
            
            if (triggeringPlayerComponent == null)
            {
                Debug.LogWarning("[IntroTrigger] No Player component found on triggering object!");
            }
            
            StartCoroutine(PlayCutscene());
        }
    }
    
    IEnumerator PlayCutscene()
    {
        isInCutscene = true;
        
        //Debug.Log("Cutscene started!");
        
        // Save original camera transform (both local and world)
        originalCameraParent = mainCamera.transform.parent;
        originalCameraLocalPosition = mainCamera.transform.localPosition;
        originalCameraLocalRotation = mainCamera.transform.localRotation;
        originalCameraWorldPosition = mainCamera.transform.position;
        originalCameraWorldRotation = mainCamera.transform.rotation;
        
        /* Debug.Log("Original camera parent: " + (originalCameraParent != null ? originalCameraParent.name : "NULL"));
        Debug.Log("Original camera position: " + originalCameraWorldPosition); */
        
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
           // Debug.Log("Using cutscene position object");
        }
        else
        {
            // Calculate position in front of triggering player
            Vector3 playerForward = triggeringPlayer.transform.forward;
            targetPosition = triggeringPlayer.transform.position + 
                           playerForward * distanceInFrontOfPlayer + 
                           Vector3.up * heightOffset;
            
            // Look at triggering player
            Vector3 lookAtTarget = triggeringPlayer.transform.position + Vector3.up * heightOffset;
            targetRotation = Quaternion.LookRotation(lookAtTarget - targetPosition);
            
            //Debug.Log("Calculated cutscene position: " + targetPosition);
        }
        
        // IMPORTANT: Detach camera from parent first!
        mainCamera.transform.SetParent(null);
        
       // Debug.Log("Camera detached, moving to cutscene position");
        
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
        
       // Debug.Log("Camera moved to: " + mainCamera.transform.position);
        
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
        
        //.Log("Cutscene complete, returning camera");
        
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
        
      //  Debug.Log("Camera returned to original position");
        
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
        if (players == null || players.Length == 0) return;
        
        Debug.Log("[IntroTrigger] Disabling player controls and input");
        
        // CRITICAL: Disable input for the triggering player (Player tag)
        if (triggeringPlayerComponent != null && triggeringPlayerComponent.input != null)
        {
            triggeringPlayerComponent.input.Disable();
            Debug.Log($"[IntroTrigger] Disabled input for {triggeringPlayer.name}");
        }
        
        // Disable controls for all players
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null) continue;
            
            // Set animation to idle state only (let animation continue playing)
            if (playerAnimators[i] != null && playerAnimators[i].isActiveAndEnabled)
            {
                // Set your specific animation parameters to idle state
                TrySetAnimatorParameter(playerAnimators[i], "idle", true);          // Set Idle to true
                TrySetAnimatorParameter(playerAnimators[i], "poseOrLook", 0f);      // Set to 0 for idle pose
                TrySetAnimatorParameter(playerAnimators[i], "move", false);         // Set to false for no movement
                
              //  Debug.Log("Player animation set to idle");
            }
            
            // Disable all MonoBehaviour scripts on player (except this one)
            playerScripts[i] = players[i].GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in playerScripts[i])
            {
                if (script != this && script.enabled)
                {
                    script.enabled = false;
                }
            }
            
            // Stop player movement
            if (playerRigidbodies[i] != null)
            {
                playerRigidbodies[i].linearVelocity = Vector3.zero;
                playerRigidbodies[i].angularVelocity = Vector3.zero;
                playerRigidbodies[i].isKinematic = true;
            }
            
            // Disable character controller if present
            if (playerCharControllers[i] != null)
            {
                playerCharControllers[i].enabled = false;
            }
        }
    }
    
    void TrySetAnimatorParameter(Animator animator, string paramName, float value)
    {
        if (animator != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName && param.type == AnimatorControllerParameterType.Float)
                {
                    animator.SetFloat(paramName, value);
                    return;
                }
            }
        }
    }
    
    void TrySetAnimatorParameter(Animator animator, string paramName, bool value)
    {
        if (animator != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(paramName, value);
                    return;
                }
            }
        }
    }
    
    void EnablePlayerControl()
    {
        if (players == null || players.Length == 0) return;
        
        Debug.Log("[IntroTrigger] Re-enabling player controls and input");
        
        // CRITICAL: Re-enable input for the triggering player (Player tag)
        if (triggeringPlayerComponent != null && triggeringPlayerComponent.input != null)
        {
            triggeringPlayerComponent.input.Enable();
            Debug.Log($"[IntroTrigger] Re-enabled input for {triggeringPlayer.name}");
        }
        
        // Re-enable controls for all players
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null) continue;
            
            // Keep animation in idle state when exiting cutscene
            if (playerAnimators[i] != null && playerAnimators[i].isActiveAndEnabled)
            {
                // Keep idle state
                TrySetAnimatorParameter(playerAnimators[i], "idle", false);         // Reset Idle trigger (set to false when cutscene ends)
                TrySetAnimatorParameter(playerAnimators[i], "poseOrLook", 0f);      // Idle pose
                TrySetAnimatorParameter(playerAnimators[i], "move", false);         // No movement
                
                Debug.Log("[IntroTrigger] Player animation restored to idle");
            }
            
            // Re-enable all MonoBehaviour scripts on player
            if (playerScripts[i] != null)
            {
                foreach (MonoBehaviour script in playerScripts[i])
                {
                    if (script != null)
                    {
                        script.enabled = true;
                    }
                }
            }
            
            // Re-enable player physics
            if (playerRigidbodies[i] != null)
            {
                playerRigidbodies[i].isKinematic = false;
            }
            
            // Re-enable character controller if present
            if (playerCharControllers[i] != null)
            {
                playerCharControllers[i].enabled = true;
            }
        }
    }
    
    public void SkipCutscene()
    {
        if (isInCutscene)
        {
           // Debug.Log("Skipping cutscene");
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
