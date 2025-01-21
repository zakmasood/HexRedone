using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ConnectableBuilding : MonoBehaviour
{
    public Dictionary<Transform, bool> ConnectionPoints { get; private set; } = new Dictionary<Transform, bool>();
    public HashSet<ConnectableBuilding> ConnectedBuildings { get; private set; } = new HashSet<ConnectableBuilding>();
    public ConnectableBuilding ConnectedTo { get; private set; }

    public event Action<ConnectableBuilding> OnConnected;
    public event Action<ConnectableBuilding> OnDisconnected;

    public int ConnectionCount => ConnectedBuildings.Count;
    public bool IsConnected => ConnectionCount > 0;

    public virtual Transform[] GetConnectionPoints() { return ConnectionPoints.Keys.ToArray(); }

    public virtual Transform GetAvailableConnectionPoint() { return ConnectionPoints.FirstOrDefault(kvp => kvp.Value).Key; }

    public virtual void ConnectTo(ConnectableBuilding otherBuilding)
    {
        if (CanConnectTo(otherBuilding))
        {
            Transform availablePoint = GetAvailableConnectionPoint();
            if (availablePoint != null)
            {
                MarkConnectionPointAsConnected(availablePoint);
                ConnectedTo = otherBuilding;
                ConnectedBuildings.Add(otherBuilding);
                OnConnected?.Invoke(otherBuilding);
            }
            else { Debug.LogWarning("No available connection points!"); }
        }
        else { Debug.LogWarning("Cannot connect to this building type."); }
    }

    public abstract bool CanConnectTo(ConnectableBuilding otherBuilding);

    public virtual bool IsConnectedTo(ConnectableBuilding otherBuilding) { return ConnectedBuildings.Contains(otherBuilding); }

    public virtual void MarkConnectionPointAsConnected(Transform point)
    {
        if (ConnectionPoints.ContainsKey(point)) { ConnectionPoints[point] = false; }
        else { Debug.LogWarning($"Connection point {point?.name} not found."); }
    }

    public virtual bool IsConnectionPointAvailable(Transform point)
    {
        return ConnectionPoints.ContainsKey(point) && ConnectionPoints[point];
    }

    public virtual void ResetConnectionPoints()
    {
        foreach (var point in ConnectionPoints.Keys.ToList()) { ConnectionPoints[point] = true; }
    }
}