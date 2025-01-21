using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ConnectionManager : MonoBehaviour
{
    private List<ConnectableBuilding> selectedBuildings = new List<ConnectableBuilding>();
    private Dictionary<ConnectableBuilding, List<GameObject>> connectionLines = new Dictionary<ConnectableBuilding, List<GameObject>>();

    public void SelectBuilding(MonoBehaviour building)
    {
        // Try to resolve the correct component if not passed directly
        ConnectableBuilding connectableBuilding = building.GetComponent<ConnectableBuilding>();

        if (connectableBuilding != null)
        {
            if (!selectedBuildings.Contains(connectableBuilding)) { selectedBuildings.Add(connectableBuilding); }
            if (selectedBuildings.Count == 2) { ConnectBuildings(); }
        }
        else { Debug.LogWarning($"{building.name} does not contain a connectable building component."); }
    }

    private void ConnectBuildings()
    {
        ConnectableBuilding first = selectedBuildings[0];
        Debug.Log($"First building: {first.name}");
        ConnectableBuilding second = selectedBuildings[1];
        Debug.Log($"Second building: {second.name}");

        if (first == null || second == null) { Debug.LogError("Selected Buildings are not connectable."); return; }

        if (first.IsConnectedTo(second) || second.IsConnectedTo(first))
        {
            Debug.LogWarning($"{first.GetType().Name} and {second.GetType().Name} are already connected.");
            selectedBuildings.Clear();
            return;
        }

        Transform closestFirstTerminal = null;
        Transform closestSecondTerminal = null;
        float shortestDistance = float.MaxValue;

        // Find the closest available connection points on both buildings
        foreach (Transform firstTerminal in first.GetConnectionPoints())
        {
            if (!first.IsConnectionPointAvailable(firstTerminal)) continue;

            foreach (Transform secondTerminal in second.GetConnectionPoints())
            {
                if (!second.IsConnectionPointAvailable(secondTerminal)) continue;

                float distance = Vector2.Distance(firstTerminal.position, secondTerminal.position);
                if (distance < shortestDistance) { shortestDistance = distance; closestFirstTerminal = firstTerminal; closestSecondTerminal = secondTerminal; }
            }
        }

        // Ensure valid connection points were found
        if (closestFirstTerminal != null && closestSecondTerminal != null)
        {
            first.MarkConnectionPointAsConnected(closestFirstTerminal);
            second.MarkConnectionPointAsConnected(closestSecondTerminal);

            // Visualize the connection with a line
            MakeLine(first, closestFirstTerminal, second, closestSecondTerminal);

            // Connect the buildings logically
            first.ConnectTo(second);
            second.ConnectTo(first);

            Debug.Log($"Connected {first.GetType().Name} to {second.GetType().Name}.");
        }
        else { Debug.LogError("No valid terminals found for connection."); }
        // Debug the terminals / connections
        Debug.Log($"Closest terminals: {closestFirstTerminal.name} and {closestSecondTerminal.name}");
        selectedBuildings.Clear();
    }

    public void RemoveConnections(ConnectableBuilding building)
    {
        if (connectionLines.ContainsKey(building))
        {
            foreach (var line in connectionLines[building]) { Destroy(line); }

            connectionLines.Remove(building);
            Debug.Log($"All connections for {building.GetType().Name} have been removed.");
        }

        building.ResetConnectionPoints();
    }

    private void MakeLine(ConnectableBuilding firstBuilding, Transform start, ConnectableBuilding secondBuilding, Transform end)
    {
        // Validate inputs
        if (start == null || end == null) { Debug.LogError("MakeLine: Start or End transform is null."); return; }

        // Create a new GameObject to hold the LineRenderer
        GameObject lineObject = new GameObject("BezierLine");

        // Add LineRenderer component to the GameObject
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        // Configure LineRenderer properties
        lineRenderer.startWidth = 0.05f; // Thickness of the line
        lineRenderer.endWidth = 0.05f;   // Thickness of the line
        lineRenderer.material = new Material(Shader.Find("Unlit/Color")); // Use unlit shader for color
        lineRenderer.material.color = Color.black; // Set line color
        lineRenderer.useWorldSpace = true; // Ensure positions are in world space

        // Calculate sag amount based on distance
        Vector3 startPos = start.position;
        Vector3 endPos = end.position;
        float distance = Vector3.Distance(startPos, endPos);
        float sag = Mathf.Clamp(distance * 0.05f, 0.1f, 5f); // Adjust sag factor as needed

        // Define control points for the Bézier curve
        Vector3 midPoint = (startPos + endPos) / 2; // Midpoint between start and end
        midPoint.y -= sag; // Apply sag downward on the Y-axis

        // Generate Bézier points
        int curveResolution = 20; // Number of segments in the curve
        Vector3[] curvePoints = new Vector3[curveResolution];
        for (int i = 0; i < curveResolution; i++)
        {
            float t = i / (float)(curveResolution - 1); // Normalized time (0 to 1)
            curvePoints[i] = CalculateBezierPoint(t, startPos, midPoint, endPos);
        }

        // Apply points to LineRenderer
        lineRenderer.positionCount = curvePoints.Length;
        lineRenderer.SetPositions(curvePoints);

        if (!connectionLines.ContainsKey(firstBuilding)) { connectionLines[firstBuilding] = new List<GameObject>(); }
        if (!connectionLines.ContainsKey(secondBuilding)) { connectionLines[secondBuilding] = new List<GameObject>(); }

        connectionLines[firstBuilding].Add(lineObject);
        connectionLines[secondBuilding].Add(lineObject);

        // Log for debugging
        // Debug.Log($"Bezier line created between {start.name} and {end.name} with sag: {sag}");
    }

    // Bézier curve calculation
    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        return (uu * p0) + (2 * u * t * p1) + (tt * p2); // Quadratic Bézier formula
    }
}
