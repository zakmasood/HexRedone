using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Scriptable Objects/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public GameObject buildingPrefab;
    public List<ResourceCost> resourceCosts;
    public int storageCapacity;

    public interface IBuilding
    {
        Transform[] GetCableTerminals(); // Retrieve all cable terminals for the building
        bool CanConnectTo(IBuilding other); // Check if this building can connect to another
        void ConnectTo(IBuilding other); // Logic for connecting to another building
    }

    public BuildingData(string buildingName, GameObject buildingPrefab)
    {
        this.buildingName = buildingName;
        this.buildingPrefab = buildingPrefab;
    }
}
