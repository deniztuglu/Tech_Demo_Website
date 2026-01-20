using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TelemetryDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI targetRPMText;
    public TextMeshProUGUI currentRPMText;
    public TextMeshProUGUI currentLoadText;

    [Header("Display Settings")]
    public bool showTelemetry = true;
    public string rpmFormat = "F1";
    public string loadFormat = "P0";

    [Header("Visual Style")]
    public Color normalColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;
    public float warningThreshold = 80f;
    public float criticalThreshold = 95f;

    private SpaceshipController currentShip;

    void Start()
    {
        // Find the active spaceship controller
        FindCurrentSpaceship();
    }

    void Update()
    {
        if (!showTelemetry)
        {
            gameObject.SetActive(false);
            return;
        }

        // Find current ship if we don't have one or if it's disabled
        if (currentShip == null || !currentShip.gameObject.activeInHierarchy)
        {
            FindCurrentSpaceship();
        }

        if (currentShip != null)
        {
            UpdateTelemetryDisplay();
        }
    }

    private void FindCurrentSpaceship()
    {
        // Look for the currently active spaceship
        SpaceshipController[] ships = FindObjectsByType<SpaceshipController>(FindObjectsSortMode.None);
        foreach (var ship in ships)
        {
            if (ship.gameObject.activeInHierarchy)
            {
                currentShip = ship;
                break;
            }
        }
    }

    private void UpdateTelemetryDisplay()
    {
        if (currentShip == null) return;

        // Get values from the spaceship controller
        float targetRPM = currentShip.GetTargetRPM();
        float currentRPM = currentShip.GetCurrentRPM();
        float currentLoad = currentShip.GetCurrentLoad();

        // Update text displays
        if (targetRPMText != null)
        {
            targetRPMText.text = $"Target RPM: {targetRPM.ToString(rpmFormat)}";
            targetRPMText.color = GetRPMColor(targetRPM);
        }

        if (currentRPMText != null)
        {
            currentRPMText.text = $"Current RPM: {currentRPM.ToString(rpmFormat)}";
            currentRPMText.color = GetRPMColor(currentRPM);
        }

        if (currentLoadText != null)
        {
            currentLoadText.text = $"Load: {currentLoad.ToString(loadFormat)}";
            currentLoadText.color = GetLoadColor(currentLoad);
        }
    }

    private Color GetRPMColor(float rpm)
    {
        float percentage = rpm / currentShip.maxRPM * 100f;

        if (percentage >= criticalThreshold)
            return criticalColor;
        else if (percentage >= warningThreshold)
            return warningColor;
        else
            return normalColor;
    }

    private Color GetLoadColor(float load)
    {
        // Load is already a 0-1 value
        float percentage = load * 100f;

        if (percentage >= criticalThreshold)
            return criticalColor;
        else if (percentage >= warningThreshold)
            return warningColor;
        else
            return normalColor;
    }

    public void ToggleTelemetry()
    {
        showTelemetry = !showTelemetry;
        gameObject.SetActive(showTelemetry);
    }
}