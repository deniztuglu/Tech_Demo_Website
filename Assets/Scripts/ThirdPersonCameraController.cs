using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private Texture2D draggableCursor;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;

    [Header("Orbit Settings")]
    [SerializeField] private float lookSpeedX = 15f;
    [SerializeField] private float lookSpeedY = 15f;
    [SerializeField] private bool invertY = false;

    [Header("Orbit Limits")]
    [SerializeField] private float minVerticalAngle = 0f;
    [SerializeField] private float maxVerticalAngle = 85f;

    [Header("Physics")]
    [SerializeField] private float friction = 5f;
    [SerializeField] private float inputSmoothing = 15f;

    private PlayerControls controls;
    private CinemachineCamera cam;
    private CinemachineOrbitalFollow orbital;
    private Vector2 scrollDelta;
    
    private float targetZoom;
    private float currentZoom;
    private bool isDragging = false;
    private Vector2 currentVelocity;
    private Vector2 targetVelocity;
    
    // We add this flag so the Manager can freeze input during a fade
    public bool IsInputLocked { get; set; } = false;

    void Start()
    {
        controls = new PlayerControls();
        controls.Enable();
        controls.CameraControls.MouseZoom.performed += ctx => scrollDelta = ctx.ReadValue<Vector2>();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cam = GetComponent<CinemachineCamera>();
        orbital = cam.GetComponent<CinemachineOrbitalFollow>();

        targetZoom = currentZoom = orbital.Radius;
    }

    void Update()
    {
        if (IsInputLocked) return; // Stop processing if locked by manager

        HandleZoom();
        HandleOrbit();
        HandleCursorState();
    }

    // Helper to snap camera settings if we switch objects
    public void ResetCameraState()
    {
        currentVelocity = Vector2.zero;
        targetZoom = currentZoom = orbital.Radius;
    }

    private void HandleCursorState()
    {
        if (isDragging) return;

        bool isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (isOverUI) Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        else Cursor.SetCursor(draggableCursor, cursorHotspot, CursorMode.Auto);
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
        
        // Prevent click-through on UI
        if (rightMouseDown && !isDragging)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        }

        if (rightMouseDown)
        {
            if (!isDragging)
            {
                isDragging = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                currentVelocity = Vector2.zero;
            }

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            targetVelocity = new Vector2(mouseDelta.x * lookSpeedX, mouseDelta.y * lookSpeedY * (invertY ? 1 : -1));
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
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, Time.deltaTime * friction);
        }

        if (Mathf.Abs(currentVelocity.x) > 0.01f) orbital.HorizontalAxis.Value += currentVelocity.x * Time.deltaTime;
        if (Mathf.Abs(currentVelocity.y) > 0.01f) orbital.VerticalAxis.Value += currentVelocity.y * Time.deltaTime;
        
        orbital.VerticalAxis.Value = Mathf.Clamp(orbital.VerticalAxis.Value, minVerticalAngle, maxVerticalAngle);
    }

    private void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        if (controls != null) controls.Disable();
    }
}