using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Helper script to create a graphical telemetry chart similar to performance monitoring tools.
/// Creates a real-time graph showing RPM and Load data over time.
/// </summary>
public class GraphicalTelemetrySetup : MonoBehaviour
{
    [Header("Canvas Settings")]
    public Canvas targetCanvas;
    public bool findCanvasAutomatically = true;

    [Header("Chart Settings")]
    public Vector2 chartPosition = new Vector2(-20, -20);
    public Vector2 chartSize = new Vector2(400, 250);

    [Header("Style Settings")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    public Color textColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color accentColor = new Color(0.2f, 1f, 0.3f, 1f);

    [ContextMenu("Create Graphical Telemetry")]
    public void CreateGraphicalTelemetry()
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

        // Create main chart panel
        GameObject chartPanel = CreateChartPanel();

        // Create chart components
        CreateChartComponents(chartPanel);

        Debug.Log("Graphical Telemetry Chart created successfully!");
    }

    private GameObject CreateChartPanel()
    {
        GameObject panel = new GameObject("TelemetryChart");
        panel.transform.SetParent(targetCanvas.transform, false);

        // Add RectTransform - positioned at bottom-right
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0); // Bottom-right anchor
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.anchoredPosition = chartPosition;
        rect.sizeDelta = chartSize;

        // Add background with border
        Image background = panel.AddComponent<Image>();
        background.color = backgroundColor;

        // Add subtle border effect
        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = accentColor;
        outline.effectDistance = new Vector2(1, 1);

        return panel;
    }

    private void CreateChartComponents(GameObject parent)
    {
        // Create title
        GameObject titleObj = CreateTextElement("Title", parent,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
            new Vector2(10, -10), new Vector2(-20, 25));
        var titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "TELEMETRY";
        titleText.fontSize = 16;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = accentColor;
        titleText.alignment = TextAlignmentOptions.TopLeft;

        // Create chart area (main drawing area)
        GameObject chartArea = new GameObject("ChartArea");
        chartArea.transform.SetParent(parent.transform, false);

        RectTransform chartRect = chartArea.AddComponent<RectTransform>();
        chartRect.anchorMin = new Vector2(0, 0);
        chartRect.anchorMax = new Vector2(1, 1);
        chartRect.pivot = new Vector2(0, 0);
        chartRect.offsetMin = new Vector2(50, 40); // Leave space for labels
        chartRect.offsetMax = new Vector2(-20, -40); // Leave space for title

        // Add chart background
        Image chartBg = chartArea.AddComponent<Image>();
        chartBg.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);

        // Create data labels
        GameObject rpmLabel = CreateTextElement("RPMLabel", parent,
            new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 0),
            new Vector2(10, 10), new Vector2(150, 20));
        var rpmText = rpmLabel.GetComponent<TextMeshProUGUI>();
        rpmText.fontSize = 12;
        rpmText.color = new Color(0.2f, 1f, 0.3f, 1f);
        rpmText.alignment = TextAlignmentOptions.BottomLeft;

        GameObject loadLabel = CreateTextElement("LoadLabel", parent,
            new Vector2(0.5f, 0), new Vector2(1f, 0), new Vector2(0, 0),
            new Vector2(10, 10), new Vector2(-10, 20));
        var loadText = loadLabel.GetComponent<TextMeshProUGUI>();
        loadText.fontSize = 12;
        loadText.color = new Color(1f, 0.8f, 0.2f, 1f);
        loadText.alignment = TextAlignmentOptions.BottomRight;

        GameObject timeLabel = CreateTextElement("TimeLabel", parent,
            new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-10, 10), new Vector2(80, 20));
        var timeText = timeLabel.GetComponent<TextMeshProUGUI>();
        timeText.fontSize = 10;
        timeText.color = textColor * 0.7f;
        timeText.alignment = TextAlignmentOptions.BottomRight;

        // Add the main telemetry chart script
        TelemetryChart chartScript = parent.AddComponent<TelemetryChart>();
        chartScript.chartArea = chartRect;
        chartScript.titleText = titleText;
        chartScript.rpmLabel = rpmText;
        chartScript.loadLabel = loadText;
        chartScript.timeLabel = timeText;

        // Configure chart appearance
        chartScript.rpmLineColor = new Color(0.2f, 1f, 0.3f, 1f);
        chartScript.loadLineColor = new Color(1f, 0.8f, 0.2f, 1f);
        chartScript.gridColor = new Color(0.4f, 0.4f, 0.4f, 0.2f);
        chartScript.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.8f);
        chartScript.lineWidth = 2f;
        chartScript.maxDataPoints = 200;
        chartScript.timeSpan = 30f;
        chartScript.updateInterval = 0.05f; // Smooth updates
    }

    private GameObject CreateTextElement(string name, GameObject parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = name;
        text.color = textColor;
        text.fontSize = 12;
        text.fontStyle = FontStyles.Normal;

        return textObj;
    }

    [ContextMenu("Remove Existing Telemetry")]
    public void RemoveExistingTelemetry()
    {
        // Find and remove existing telemetry displays
        TelemetryChart[] existing = FindObjectsByType<TelemetryChart>(FindObjectsSortMode.None);
        for (int i = existing.Length - 1; i >= 0; i--)
        {
            if (existing[i] != null)
                DestroyImmediate(existing[i].gameObject);
        }

        Debug.Log("Removed existing telemetry displays.");
    }
}