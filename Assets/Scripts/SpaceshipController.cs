using UnityEngine;
using FMODUnity; // Required for EventReference
using FMOD.Studio; 

public class SpaceshipController : MonoBehaviour
{
    [Header("FMOD Settings")]
    // This creates the [Browse] button in the Inspector
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

    [Header("Debug")]
    public bool showDebug = true;

    private EventInstance engineInstance;
    private float currentRPM;
    private float targetRPM;
    private float rpmVelocity; 
    private float currentLoad;
    private Quaternion initialRotation;

    void Start()
    {
        currentRPM = idleRPM;

        // 1. START FMOD INSTANCE
        // We check if the event is assigned before trying to play it
        if (!engineEvent.IsNull)
        {
            engineInstance = RuntimeManager.CreateInstance(engineEvent);
            engineInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            engineInstance.start();
        }
        else
        {
            Debug.LogWarning("FMOD Engine Event not assigned in the Inspector!");
        }

        if (shipModel != null) initialRotation = shipModel.localRotation;
    }

    void Update()
    {
        // 2. INPUT & PHYSICS LOGIC
        float throttleInput = Input.GetAxis("Vertical"); 
        targetRPM = Mathf.Lerp(idleRPM, maxRPM, Mathf.Clamp01(throttleInput));

        float activeSmoothTime = (targetRPM > currentRPM) ? timeToReachMax : timeToDropToIdle;
        float previousRPM = currentRPM;
        currentRPM = Mathf.SmoothDamp(currentRPM, targetRPM, ref rpmVelocity, activeSmoothTime);

        // 3. LOAD CALCULATION
        float rawDelta = (currentRPM - previousRPM) / Time.deltaTime;
        float maxExpectedAcceleration = (maxRPM - idleRPM) / timeToReachMax;
        currentLoad = (maxExpectedAcceleration > 0) ? Mathf.Clamp01(rawDelta / maxExpectedAcceleration) : 0;

        // 4. UPDATE FMOD PARAMETERS
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

    void OnDestroy()
    {
        if (engineInstance.isValid())
        {
            engineInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            engineInstance.release();
        }
    }

    void LogEngineValues()
    {
        Debug.Log($"<color=#00FFCC><b>[FMOD RPM]:</b> {currentRPM:F2}</color> | " +
                  $"<color=#FFCC00><b>[FMOD Load]:</b> {currentLoad:P0}</color>");
    }

    public float GetRPM() => currentRPM;
    public float GetLoad() => currentLoad;
}