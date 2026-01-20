using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class TelemetryChart : MonoBehaviour
{
    [Header("Chart Settings")]
    public int maxDataPoints = 1000; // Allow more data points
    public float timeSpan = 20f; // 20 seconds of data
    public float updateInterval = 0.2f; // Update every 200ms

    [Header("Performance Settings")]
    public int maxRenderedSegments = 200; // Limit rendered segments for performance
    public bool adaptiveQuality = true; // Automatically adjust quality based on container size
    public float minSegmentLength = 2f; // Skip segments shorter than this

    [Header("Chart Appearance")]
    public Color targetLineColor = new Color(0.3f, 1f, 0.4f, 1f); // Bright green
    public Color rpmLineColor = new Color(0.3f, 1f, 0.4f, 1f); // Bright green
    public Color loadLineColor = new Color(1f, 0.9f, 0.3f, 1f); // Bright yellow/orange
    public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.15f); // Subtle grid
    public Color backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.9f);
    public float lineWidth = 3f; // Slightly thicker lines

    [Header("Chart Layout")]
    public RectTransform chartArea;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI rpmLabel;
    public TextMeshProUGUI loadLabel;
    public TextMeshProUGUI timeLabel;

    private List<DataPoint> dataPoints = new List<DataPoint>();
    private SpaceshipController currentShip;
    private float nextUpdateTime;
    private Image chartBackground;
    private List<GameObject> chartLines = new List<GameObject>();
    private Camera uiCamera;
    private Vector2 lastChartSize;
    private int currentRenderQuality = 1;

    [System.Serializable]
    private struct DataPoint
    {
        public float time;
        public float rpm;
        public float load;
        public float targetRPM;
    }

    void Start()
    {
        FindCurrentSpaceship();
        SetupChart();
        uiCamera = Camera.main; // Fallback to main camera if UI camera not found
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.worldCamera != null)
            uiCamera = canvas.worldCamera;
    }

    void Update()
    {
        // Find current ship if we don't have one
        if (currentShip == null || !currentShip.gameObject.activeInHierarchy)
        {
            FindCurrentSpaceship();
        }

        if (currentShip != null && Time.time >= nextUpdateTime)
        {
            AddDataPoint();
            CheckContainerResize();
            UpdateChart();
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    private void FindCurrentSpaceship()
    {
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

    private void SetupChart()
    {
        if (chartArea == null) return;

        // Setup background
        if (chartBackground == null)
        {
            chartBackground = chartArea.GetComponent<Image>();
            if (chartBackground == null)
                chartBackground = chartArea.gameObject.AddComponent<Image>();
        }
        chartBackground.color = backgroundColor;

        // Setup title
        if (titleText != null)
            titleText.text = "TELEMETRY";
    }

    private void AddDataPoint()
    {
        if (currentShip == null) return;

        DataPoint newPoint = new DataPoint
        {
            time = Time.time,
            rpm = currentShip.GetCurrentRPM(),
            load = currentShip.GetCurrentLoad() * 100f, // Convert to percentage
            targetRPM = currentShip.GetTargetRPM()
        };

        dataPoints.Add(newPoint);

        // Remove old data points
        float cutoffTime = Time.time - timeSpan;
        dataPoints.RemoveAll(point => point.time < cutoffTime);

        // Limit data points
        if (dataPoints.Count > maxDataPoints)
        {
            dataPoints.RemoveAt(0);
        }
    }

    private void UpdateChart()
    {
        if (dataPoints.Count < 2 || chartArea == null) return;

        // Clear existing lines
        ClearChart();

        // Update labels
        UpdateLabels();

        // Draw lines
        DrawRPMLine();
        DrawLoadLine();
        DrawGrid();
    }

    private void ClearChart()
    {
        foreach (var line in chartLines)
        {
            if (line != null)
                DestroyImmediate(line);
        }
        chartLines.Clear();
    }

    private void UpdateLabels()
    {
        if (dataPoints.Count == 0) return;

        var latest = dataPoints.Last();

        if (rpmLabel != null)
            rpmLabel.text = $"RPM: {latest.rpm:F1} / {latest.targetRPM:F1}";

        if (loadLabel != null)
            loadLabel.text = $"LOAD: {latest.load:F1}%";

        if (timeLabel != null)
            timeLabel.text = $"Time: {timeSpan}s";
    }

    private void DrawRPMLine()
    {
        var rpmPoints = dataPoints.Select(d => new Vector2(
            GetTimePosition(d.time),
            GetRPMPosition(d.rpm)
        )).ToArray();

        DrawLine(rpmPoints, rpmLineColor, "RPM_Line");

        // Draw target RPM as dashed line
        var targetPoints = dataPoints.Select(d => new Vector2(
            GetTimePosition(d.time),
            GetRPMPosition(d.targetRPM)
        )).ToArray();

        DrawLine(targetPoints, targetLineColor * 0.7f, "Target_RPM_Line");
    }

    private void DrawLoadLine()
    {
        var loadPoints = dataPoints.Select(d => new Vector2(
            GetTimePosition(d.time),
            GetLoadPosition(d.load)
        )).ToArray();

        DrawLine(loadPoints, loadLineColor, "Load_Line");
    }

    private void DrawGrid()
    {
        Rect rect = chartArea.rect;

        // Vertical grid lines (time)
        for (int i = 0; i <= 6; i++)
        {
            float x = rect.width * i / 6f;
            Vector2[] gridLine = {
                new Vector2(x, 0),
                new Vector2(x, rect.height)
            };
            DrawLine(gridLine, gridColor, $"VGrid_{i}");
        }

        // Horizontal grid lines (values)
        for (int i = 0; i <= 4; i++)
        {
            float y = rect.height * i / 4f;
            Vector2[] gridLine = {
                new Vector2(0, y),
                new Vector2(rect.width, y)
            };
            DrawLine(gridLine, gridColor, $"HGrid_{i}");
        }
    }

    private void DrawLine(Vector2[] points, Color color, string name)
    {
        if (points.Length < 2) return;

        // Calculate adaptive step size based on container size and performance settings
        int totalSegments = points.Length - 1;
        int targetSegments = adaptiveQuality ?
            Mathf.Min(maxRenderedSegments / currentRenderQuality, totalSegments) :
            Mathf.Min(maxRenderedSegments, totalSegments);

        int step = Mathf.Max(1, totalSegments / targetSegments);

        for (int i = 0; i < points.Length - step; i += step)
        {
            int nextIndex = Mathf.Min(i + step, points.Length - 1);
            Vector2 p1 = points[i];
            Vector2 p2 = points[nextIndex];

            // Skip very short segments for performance
            float segmentLength = Vector2.Distance(p1, p2);
            if (segmentLength < minSegmentLength) continue;

            GameObject lineObj = new GameObject($"{name}_Seg_{i}");
            lineObj.transform.SetParent(chartArea, false);

            RectTransform lineRect = lineObj.AddComponent<RectTransform>();

            // Calculate line properties
            Vector2 direction = p2 - p1;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Set line properties - responsive to container size
            float responsiveLineWidth = lineWidth * GetContainerScale();
            lineRect.anchorMin = new Vector2(0, 0);
            lineRect.anchorMax = new Vector2(0, 0);
            lineRect.sizeDelta = new Vector2(distance, responsiveLineWidth);
            lineRect.anchoredPosition = p1 + direction * 0.5f;
            lineRect.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Add Image component with solid color
            Image lineImage = lineObj.AddComponent<Image>();
            lineImage.color = color;
            lineImage.raycastTarget = false;

            chartLines.Add(lineObj);
        }
    }

    private float GetTimePosition(float time)
    {
        if (dataPoints.Count == 0) return 0;

        // Use current time as reference for scrolling effect
        float currentTime = Time.time;
        float startTime = currentTime - timeSpan;

        // Normalize time position from right to left (newest on right)
        float normalizedTime = (time - startTime) / timeSpan;
        return chartArea.rect.width * normalizedTime;
    }

    private float GetRPMPosition(float rpm)
    {
        if (currentShip == null) return 0;
        return chartArea.rect.height * rpm / currentShip.maxRPM;
    }

    private float GetLoadPosition(float loadPercent)
    {
        return chartArea.rect.height * loadPercent / 100f;
    }

    private void CheckContainerResize()
    {
        if (chartArea == null) return;

        Vector2 currentSize = chartArea.rect.size;
        if (currentSize != lastChartSize)
        {
            lastChartSize = currentSize;
            UpdateRenderQuality();
        }
    }

    private void UpdateRenderQuality()
    {
        if (!adaptiveQuality) return;

        // Adjust render quality based on container size
        float containerArea = lastChartSize.x * lastChartSize.y;
        if (containerArea > 200000) // Large container
            currentRenderQuality = 1; // High quality
        else if (containerArea > 80000) // Medium container
            currentRenderQuality = 2; // Medium quality
        else // Small container
            currentRenderQuality = 3; // Lower quality for performance
    }

    private float GetContainerScale()
    {
        if (chartArea == null) return 1f;

        // Scale line width based on container size
        float baseSize = 400f; // Reference size
        float currentSize = Mathf.Min(chartArea.rect.width, chartArea.rect.height);
        return Mathf.Clamp(currentSize / baseSize, 0.5f, 2f);
    }
}