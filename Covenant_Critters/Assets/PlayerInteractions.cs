using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 1.5f;
    public LayerMask characterLayer;
    public KeyCode interactionKey = KeyCode.X;
    
    [Header("Camera Settings")]
    public Camera mainCamera;
    public Camera secondaryCamera;
    private bool useMainCamera = true;
    
    [Header("Debug Visualization")]
    public bool showDebugVisuals = true;
    public Color debugInteractionColor = Color.red;
    public float debugVisualDuration = 0.5f;
    
    private Rigidbody2D rb;
    private Vector2 lastMoveDirection;
    private Vector2 currentInteractionPoint;
    private bool isInteracting = false;
    private float debugTimer = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Check for camera setup issues
        ValidateCameraSetup();
        
        // Initialize camera states
        SetupCameras();
    }
    
    void ValidateCameraSetup()
    {
        if (mainCamera == null || secondaryCamera == null)
        {
            Debug.LogError("One or both cameras are not assigned!");
            return;
        }
        
        // Check if cameras are identical
        if (Vector3.Distance(mainCamera.transform.position, secondaryCamera.transform.position) < 0.01f &&
            Quaternion.Angle(mainCamera.transform.rotation, secondaryCamera.transform.rotation) < 0.01f)
        {
            Debug.LogWarning("Main and secondary cameras are in the same position and rotation!");
        }
        
        // Check for multiple audio listeners
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        if (listeners.Length > 1)
        {
            Debug.LogWarning("Multiple AudioListener components found in the scene. This can cause issues with camera switching.");
        }
    }
    
    void SetupCameras()
    {
        if (mainCamera != null && secondaryCamera != null)
        {
            // Enable main camera, disable secondary
            mainCamera.enabled = true;
            secondaryCamera.enabled = false;
            useMainCamera = true;
            
            Debug.Log("Initial camera setup complete - using Main Camera.");
        }
    }
    
    void Update()
    {
        // Update direction based on movement
        Vector2 currentVelocity = rb.linearVelocity;
        if (currentVelocity.magnitude > 0.1f)
        {
            lastMoveDirection = currentVelocity.normalized;
        }
        
        // Check for interaction key press
        if (Input.GetKeyDown(interactionKey))
        {
            TryInteract();
        }
        
        // For testing purposes - add a dedicated camera toggle key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleCamera();
            Debug.Log("Camera toggled with Tab key");
        }
        
        // Update debug timer
        if (isInteracting)
        {
            debugTimer -= Time.deltaTime;
            if (debugTimer <= 0)
            {
                isInteracting = false;
            }
        }
    }
    
    void TryInteract()
    {
        // Calculate interaction point
        Vector2 checkPosition = (Vector2)transform.position + lastMoveDirection * interactionDistance;
        currentInteractionPoint = checkPosition;
        
        // Debug visualization
        if (showDebugVisuals)
        {
            isInteracting = true;
            debugTimer = debugVisualDuration;
        }
        
        // Check for objects to interact with
        Collider2D hitObject = Physics2D.OverlapCircle(checkPosition, 0.2f, characterLayer);
        
        if (hitObject != null)
        {
            Debug.Log("Interacting with object: " + hitObject.name);
            
            // Toggle camera when interacting
            ToggleCamera();
        }
        else
        {
            Debug.Log("No interactive object nearby");
        }
    }
    
    void ToggleCamera()
    {
        if (mainCamera == null || secondaryCamera == null) return;
        
        // Toggle between cameras
        if (useMainCamera)
        {
            // Switch to secondary camera
            secondaryCamera.enabled = true;
            mainCamera.enabled = false;
        }
        else
        {
            // Switch to main camera
            mainCamera.enabled = true;
            secondaryCamera.enabled = false;
        }
        
        // Flip the flag
        useMainCamera = !useMainCamera;
        
        // Debug info
        Camera currentCamera = useMainCamera ? mainCamera : secondaryCamera;
        Debug.Log("Switched to: " + currentCamera.name + 
                  " | Position: " + currentCamera.transform.position +
                  " | Enabled: " + currentCamera.enabled);
    }
    
    void OnDrawGizmos()
    {
        if (Application.isPlaying && showDebugVisuals && isInteracting)
        {
            Gizmos.color = debugInteractionColor;
            Gizmos.DrawWireSphere(currentInteractionPoint, 0.2f);
        }
    }
}