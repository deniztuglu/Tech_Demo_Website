using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Helper script to quickly set up telemetry display in your scene.
/// Add this to an empty GameObject and run CreateTelemetryDisplay() from the Inspector or code.
/// </summary>
public class TelemetrySetupHelper : MonoBehaviour
{
    [Header("Canvas Settings")]
    public Canvas targetCanvas;
    public bool findCanvasAutomatically = true;

    [Header("Position Settings")]
    public Vector2 position = new Vector2(-400, 300);
    public Vector2 size = new Vector2(250, 120);

    [Header("Style Settings")]
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    public Color textColor = Color.green;
    public int fontSize = 14;

    [ContextMenu("Create Telemetry Display")]
    public void CreateTelemetryDisplay()
    {
        // Find or use canvas
        if (findCanvasAutomatically && targetCanvas == null)
        {
            targetCanvas = FindAnyObjectByType<Canvas>();
        }

        if (targetCanvas == null)
        {
            Debug.LogError("No Canvas found! Please assign a target canvas.");
            return;
        }

        // Create main panel
        GameObject telemetryPanel = CreateTelemetryPanel();

        // Create text elements
        CreateTelemetryTexts(telemetryPanel);

        Debug.Log("Telemetry Display created successfully!");
    }

    private GameObject CreateTelemetryPanel()
    {
        GameObject panel = new GameObject("TelemetryDisplay");
        panel.transform.SetParent(targetCanvas.transform, false);

        // Add RectTransform
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1); // Top-right anchor
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        // Add background
        Image background = panel.AddComponent<Image>();
        background.color = backgroundColor;

        // Add telemetry script
        panel.AddComponent<TelemetryDisplay>();

        return panel;
    }

    private void CreateTelemetryTexts(GameObject parent)
    {
        TelemetryDisplay telemetryScript = parent.GetComponent<TelemetryDisplay>();

        // Create target RPM text
        GameObject targetRPMObj = CreateTextElement("TargetRPM", parent, new Vector2(0, -10));
        telemetryScript.targetRPMText = targetRPMObj.GetComponent<TextMeshProUGUI>();

        // Create current RPM text
        GameObject currentRPMObj = CreateTextElement("CurrentRPM", parent, new Vector2(0, -35));
        telemetryScript.currentRPMText = currentRPMObj.GetComponent<TextMeshProUGUI>();

        // Create load text
        GameObject loadObj = CreateTextElement("Load", parent, new Vector2(0, -60));
        telemetryScript.currentLoadText = loadObj.GetComponent<TextMeshProUGUI>();
    }

    private GameObject CreateTextElement(string name, GameObject parent, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(-20, 20); // Leave some padding

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = $"{name}: 0.0";
        text.color = textColor;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Left;

        return textObj;
    }
}