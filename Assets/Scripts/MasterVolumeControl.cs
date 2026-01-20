using UnityEngine;
using UnityEngine.UI;
using FMODUnity; 
using FMOD.Studio; 

public class MasterVolumeControl : MonoBehaviour
{
    [Header("UI Component")]
    public Slider volumeSlider;

    [Header("FMOD Settings")]
    [Tooltip("Usually just 'bus:/' for the master bus")]
    public string busPath = "bus:/"; 

    private Bus masterBus;

    void Start()
    {
        // 1. Get the Bus
        masterBus = RuntimeManager.GetBus(busPath);

        // 2. Sync Slider to Current FMOD Volume
        // We have to reverse the math here so the slider visual matches the volume
        if (masterBus.isValid())
        {
            masterBus.getVolume(out float currentLinearVolume);
            // Inverse of x^2 is Sqrt(x)
            volumeSlider.value = Mathf.Sqrt(currentLinearVolume); 
        }

        // 3. Listen for changes
        volumeSlider.onValueChanged.AddListener(SetMasterVolume);
    }

    public void SetMasterVolume(float sliderValue)
    {
        // Logarithmic Volume Curve
        // We square the input (sliderValue * sliderValue)
        // Slider 0.5 (50%) -> FMOD 0.25 (25%) -> Sounds like "Half Volume"
        
        float logVolume = Mathf.Pow(sliderValue, 2f); 
        
        masterBus.setVolume(logVolume);
    }
}