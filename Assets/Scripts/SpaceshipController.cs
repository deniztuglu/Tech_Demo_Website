using UnityEngine;
using AK.Wwise;

public class SpaceshipController : MonoBehaviour
{
    [Header("Wwise References")]
    public AK.Wwise.RTPC rpmRTPC;    
    public AK.Wwise.RTPC loadRTPC;   
    public AK.Wwise.Event engineEvent;

    [Header("Engine Physics")]
    [Range(0, 100)] public float idleRPM = 10f;
    [Range(0, 100)] public float maxRPM = 100f;
    
    [Tooltip("Time in seconds to reach Max RPM from Idle")]
    public float timeToReachMax = 2.0f; 
    
    [Tooltip("Time in seconds to drop back to Idle from Max")]
    public float timeToDropToIdle = 3.5f; 

    [Header("Visual Polish (Tilt)")]
    public Transform shipModel;      
    public float maxTiltAngle = 8f;  
    public float tiltSpeed = 4f;     

    [Header("Debug Settings")]
    public bool showDebug = true;

    private float currentRPM;
    private float targetRPM;
    private float rpmVelocity; 
    private float currentLoad;
    private Quaternion initialRotation;

    void Start()
    {
        currentRPM = idleRPM;
        if (engineEvent.IsValid()) engineEvent.Post(gameObject);
        if (shipModel != null) initialRotation = shipModel.localRotation;
    }

    void Update()
    {
        // 1. INPUT HANDLING
        float throttleInput = Input.GetAxis("Vertical"); 
        targetRPM = Mathf.Lerp(idleRPM, maxRPM, Mathf.Clamp01(throttleInput));

        // 2. RPM SIMULATION
        float activeSmoothTime = (targetRPM > currentRPM) ? timeToReachMax : timeToDropToIdle;
        float previousRPM = currentRPM;
        currentRPM = Mathf.SmoothDamp(currentRPM, targetRPM, ref rpmVelocity, activeSmoothTime);

        // 3. LOAD CALCULATION
        float rawDelta = (currentRPM - previousRPM) / Time.deltaTime;
        float maxExpectedAcceleration = (maxRPM - idleRPM) / timeToReachMax;
        
        // Prevent division by zero if timeToReachMax is set to 0
        if (maxExpectedAcceleration > 0)
            currentLoad = Mathf.Clamp01(rawDelta / maxExpectedAcceleration); 
        else
            currentLoad = 0;

        // 4. TILT & WWISE
        HandleTilt();
        rpmRTPC.SetGlobalValue(currentRPM);
        loadRTPC.SetGlobalValue(currentLoad);

        // 5. DEBUG LOGGING
        if (showDebug) LogEngineValues();
    }

    void HandleTilt()
    {
        if (shipModel == null) return;
        float dynamicTilt = -currentLoad * maxTiltAngle;
        Quaternion targetRotation = Quaternion.Euler(dynamicTilt, 0, 0) * initialRotation;
        shipModel.localRotation = Quaternion.Slerp(shipModel.localRotation, targetRotation, Time.deltaTime * tiltSpeed);
    }

    void LogEngineValues()
    {
        // Color coding: Cyan for RPM, Yellow for Load
        Debug.Log($"<color=cyan><b>[RPM]:</b> {currentRPM:F2}</color> | " +
                  $"<color=yellow><b>[Load]:</b> {currentLoad:P0}</color> | " +
                  $"<b>Target:</b> {targetRPM:F0} | " +
                  $"<b>Accel Time:</b> {timeToReachMax}s");
    }

    // Getters for the UI script
    public float GetRPM() => currentRPM;
    public float GetLoad() => currentLoad;
}
