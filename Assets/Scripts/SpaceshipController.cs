using UnityEngine;
using FMODUnity; 
using FMOD.Studio; 

public class SpaceshipController : MonoBehaviour
{
    [Header("FMOD Settings")]
    public EventReference engineEvent; 
    
    [ParamRef] public string rpmParameterName = "Engine_RPM";
    [ParamRef] public string loadParameterName = "Engine_Load";

    [Header("Engine Physics")]
    [Range(0, 100)] public float idleRPM = 10f;
    [Range(0, 100)] public float maxRPM = 100f;
    public float timeToReachMax = 2.0f; 
    public float timeToDropToIdle = 3.5f; 

    [Header("Visual Polish (Tilt)")]
    public Transform shipModel;      
    public float maxTiltAngle = 8f;  
    public float tiltSpeed = 4f;     

    [Header("Camera Settings")]
    [Tooltip("How far the camera should be when we first switch to this ship.")]
    public float startingCameraDistance = 10f;

    [Header("Debug")]
    public bool showDebug = true;

    private EventInstance engineInstance;
    private float currentRPM;
    private float targetRPM;
    private float rpmVelocity; 
    private float currentLoad;
    private Quaternion initialRotation;

    void Awake()
    {
        // Store initial rotation once at the very beginning
        if (shipModel != null) initialRotation = shipModel.localRotation;
    }

    // CHANGED: Use OnEnable instead of Start so sound plays every time we select the ship
    void OnEnable()
    {
        currentRPM = idleRPM; // Reset RPM on start

        if (!engineEvent.IsNull)
        {
            engineInstance = RuntimeManager.CreateInstance(engineEvent);
            engineInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            engineInstance.start();
        }
        else
        {
            Debug.LogWarning($"FMOD Engine Event not assigned on {gameObject.name}!");
        }
    }

    // CHANGED: Use OnDisable instead of OnDestroy so sound stops when we switch ships
    void OnDisable()
    {
        if (engineInstance.isValid())
        {
            // STOP_MODE.IMMEDIATE prevents the sound from trailing off while the other ship starts
            engineInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            engineInstance.release();
        }
    }

    void Update()
    {
        //Input
        float throttleInput = Input.GetAxis("Vertical"); 
        targetRPM = Mathf.Lerp(idleRPM, maxRPM, Mathf.Clamp01(throttleInput));

        float activeSmoothTime = (targetRPM > currentRPM) ? timeToReachMax : timeToDropToIdle;
        float previousRPM = currentRPM;
        currentRPM = Mathf.SmoothDamp(currentRPM, targetRPM, ref rpmVelocity, activeSmoothTime);

        //Load Calculation
        float rawDelta = (currentRPM - previousRPM) / Time.deltaTime;
        float maxExpectedAcceleration = (maxRPM - idleRPM) / timeToReachMax;
        currentLoad = (maxExpectedAcceleration > 0) ? Mathf.Clamp01(rawDelta / maxExpectedAcceleration) : 0;

        //FMOD Params
        if (engineInstance.isValid())
        {
            engineInstance.setParameterByName(rpmParameterName, currentRPM);
            engineInstance.setParameterByName(loadParameterName, currentLoad);
            engineInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        }

        HandleTilt();
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
        Debug.Log($"<color=#00FFCC><b>[FMOD RPM]:</b> {currentRPM:F2}</color> | " +
                  $"<color=#FFCC00><b>[FMOD Load]:</b> {currentLoad:P0}</color>");
    }
}