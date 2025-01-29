using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class Storage : ConnectableBuilding
{
    [Header("Storage Settings")]
    public int currentStorage = 0;
    public int storageCapacity = 100; // Maximum capacity of the storage
    public int transferRate = 1; // Amount of resources transferred per cycle
    public float transferSpeed = 1f;

    public ResourceType allowedResourceType;
    public ResourceType currentResourceType;

    public TileData tileData;

    // Reference to the next storage in the cascade
    private Storage connectedStorage;
    private Storage sourceStorage; // Track the source to prevent backflow

    private void Start()
    {
        ConnectionPoints.Clear(); // Ensure the dictionary starts fresh for each instance

        allowedResourceType = ResourceType.None; // Set on instantiation

        foreach (var point in GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("CableTerminal")))
        {
            ConnectionPoints[point] = true;
            Debug.Log($"Added connection point: {point.name} | {ConnectionPoints[point]}");
        }

        Debug.Log($"Total connection points: {ConnectionPoints.Count}");
    }

    private void OnMouseEnter()
    {
        // Get the position slightly above the building
        Vector3 textPosition = transform.position + Vector3.up * 2f;

        if (currentResourceType != null)
        {
            FloatingTextManager.Instance.ShowText(textPosition, $"{currentResourceType.ToString()}: {currentStorage} | {storageCapacity}", 0.5f);
        }
        else
        {
            FloatingTextManager.Instance.ShowText(textPosition, $"Empty: {currentStorage} | {storageCapacity}", 0.5f);
        }
    }

    private void OnMouseExit() { FloatingTextManager.Instance.HideText(0.5f); }

    public void AddResource(int amount, ResourceType resourceType, Storage source = null)
    {
        // Set the allowed resource type if it is not set
        if (allowedResourceType == ResourceType.None) { allowedResourceType = resourceType; }

        // Check if the resource type matches the allowed resource type
        if (resourceType != allowedResourceType) { Debug.LogWarning($"Storage only accepts {allowedResourceType}!"); return; }

        // Check if there is enough space in the storage
        if (currentStorage + amount > storageCapacity) { Debug.LogWarning("Not enough space in storage!"); return; }

        // Add the resource to the storage
        currentStorage += amount;
        currentResourceType = resourceType; // Update the current resource type
        sourceStorage = source; // Track the source storage
    }

    public void RemoveResource(int amount) { currentStorage = Mathf.Clamp(currentStorage - amount, 0, storageCapacity); }

    private IEnumerator CascadeResources()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // Transfer every second

            if (connectedStorage != null && currentStorage > 0)
            {   
                // Prevent backflow to the source storage
                if (connectedStorage == sourceStorage) { Debug.LogWarning($"Skipping backflow to source storage: {sourceStorage.name}"); continue; }

                ResourceType resourceTypeToTransfer = currentResourceType;
                int transferAmount = Mathf.Min(transferRate, currentStorage); // Transfer only available amount

                RemoveResource(transferAmount);
                connectedStorage.AddResource(transferAmount, resourceTypeToTransfer); // Pass this storage as the source

                Debug.Log($"Transferred {transferAmount} resources to {connectedStorage.name}");
            }
        }
    }

    public override void ConnectTo(ConnectableBuilding otherBuilding)
    {
        if (otherBuilding is Storage otherStorage)
        {
            connectedStorage = otherStorage;
            StartCoroutine(CascadeResources());
            ConnectedBuildings.Add(otherBuilding);
            otherStorage.allowedResourceType = sourceStorage.allowedResourceType;
        }
        // else { Debug.LogWarning("Attempted to connect Storage to a non-storage building."); }
    }

    public override bool CanConnectTo(ConnectableBuilding otherBuilding)
    {
        if (otherBuilding is Storage storage && storage.allowedResourceType == currentResourceType)
        {
            return true;
        }

        return false;
    }

    private void OnDestroy() { FindFirstObjectByType<ConnectionManager>().RemoveConnections(this); }
}