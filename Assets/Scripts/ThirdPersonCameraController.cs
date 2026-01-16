using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;

    [Header("Orbit Settings")]
    [SerializeField] private float lookSpeedX = 15f; // Adjusted for new logic
    [SerializeField] private float lookSpeedY = 15f; // Adjusted for new logic
    [SerializeField] private bool invertY = false;

    [Header("Orbit Limits (Clamping)")]
    [Tooltip("Lowest angle (Ground). 0 is horizon, -90 is looking up.")]
    [SerializeField] private float minVerticalAngle = 0f; 
    [Tooltip("Highest angle. 90 is looking straight down.")]
    [SerializeField] private float maxVerticalAngle = 85f;

    [Header("Coasting / Physics")]
    [Tooltip("How quickly the camera stops after releasing (Lower = longer drift)")]
    [SerializeField] private float friction = 5f; 
    [Tooltip("Smooths out mouse jitter. Helps momentum feel better.")]
    [SerializeField] private float inputSmoothing = 15f; 

    private PlayerControls controls;
    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private Vector2 scrollDelta;
    
    private float targetZoom;
    private float currentZoom;
    private bool isDragging = false;

    // Physics variables
    private Vector2 currentVelocity; // The actual speed moving the camera
    private Vector2 targetVelocity;  // The speed the mouse is trying to reach

    void Start()
    {
        controls = new PlayerControls();
        controls.Enable();
        controls.CameraControls.MouseZoom.performed += HandleMouseScroll;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cam = GetComponent<CinemachineCamera>();
        orbital = cam.GetComponent<CinemachineOrbitalFollow>();

        targetZoom = currentZoom = orbital.Radius;
    }

    private void HandleMouseScroll(InputAction.CallbackContext context)
    {
        scrollDelta = context.ReadValue<Vector2>();
    }

    void Update()
    {
        HandleZoom();
        HandleOrbit();
    }

    private void HandleZoom()
    {
        if (scrollDelta.y != 0 && orbital != null)
        {
            targetZoom = Mathf.Clamp(orbital.Radius - scrollDelta.y * zoomSpeed, minDistance, maxDistance);
            scrollDelta = Vector2.zero;
        }
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);
        orbital.Radius = currentZoom;
    }

    private void HandleOrbit()
    {
        bool rightMouseDown = Mouse.current.rightButton.isPressed;

        if (rightMouseDown)
        {
            if (!isDragging)
            {
                isDragging = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                
                // Clear lingering momentum when we grab the camera again
                currentVelocity = Vector2.zero; 
            }

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            // Calculate target velocity based on input
            float targetX = mouseDelta.x * lookSpeedX;
            float targetY = mouseDelta.y * lookSpeedY * (invertY ? 1 : -1);

            targetVelocity = new Vector2(targetX, targetY);
            
            // Smoothly interpolate current velocity towards the mouse movement (Input Smoothing)
            // This prevents "0 velocity" frames from killing momentum instantly
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, Time.deltaTime * inputSmoothing);
        }
        else
        {
            if (isDragging)
            {
                isDragging = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Apply Friction: Decay velocity towards zero when not dragging
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, Time.deltaTime * friction);
        }

        // 1. Apply Horizontal Movement
        if (Mathf.Abs(currentVelocity.x) > 0.01f)
        {
            orbital.HorizontalAxis.Value += currentVelocity.x * Time.deltaTime;
        }

        // 2. Apply Vertical Movement with CLAMPING
        if (Mathf.Abs(currentVelocity.y) > 0.01f)
        {
            orbital.VerticalAxis.Value += currentVelocity.y * Time.deltaTime;
        }
        
        // 3. Hard Clamp: Force the value to stay within bounds
        // We do this last to ensure momentum doesn't push it past limits
        orbital.VerticalAxis.Value = Mathf.Clamp(orbital.VerticalAxis.Value, minVerticalAngle, maxVerticalAngle);
    }

    private void OnDisable()
    {
        if (controls != null) controls.Disable();
    }
}