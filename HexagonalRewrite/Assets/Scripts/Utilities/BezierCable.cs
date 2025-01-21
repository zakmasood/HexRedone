using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BezierCable : MonoBehaviour
{
    public Transform startPoint;     // Start of the cable
    public Transform endPoint;       // End of the cable
    public float controlHeight = 2f; // Height offset for the control point
    public int resolution = 20;      // Number of points along the curve

    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = resolution + 1; // Set number of points
    }

    private void Update()
    {
        if (startPoint == null || endPoint == null) return;

        Vector3 p0 = startPoint.position;
        Vector3 p2 = endPoint.position;
        Vector3 p1 = (p0 + p2) / 2 + Vector3.up * controlHeight; // Control point above midpoint

        DrawBezierCurve(p0, p1, p2);
    }

    private void DrawBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution; // Normalize t to [0, 1]
            Vector3 point = CalculateBezierPoint(t, p0, p1, p2);
            lineRenderer.SetPosition(i, point); // Set point in LineRenderer
        }
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        return uu * p0 + 2 * u * t * p1 + tt * p2;
    }
}
