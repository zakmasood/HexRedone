using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Drill : ConnectableBuilding
{
    public TileData tileData; // The tile this drill is placed on
    public ResourceManager resourceManager;
    private Storage connectedStorage;
    private DrillAnimator drillAnimator;

    public float collectionInterval = 1f; // Time in seconds between collections
    private bool isCollecting = false;

    private void Start()
    {
        ConnectionPoints.Clear(); // Ensure the dictionary starts fresh for each instance

        drillAnimator = GetComponent<DrillAnimator>();

        // Ensure tileData and resourceManager are assigned
        if (tileData == null || resourceManager == null) { Debug.LogError("Drill is missing required references!"); return; }
        if (tileData.resourceType == ResourceType.None) { Debug.Log("No resources to collect on this tile!"); return; }
        if (drillAnimator == null) { Debug.LogError("DrillAnimator component not found on this GameObject!"); return; }

        foreach (var point in GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("CableTerminal")))
        {
            ConnectionPoints[point] = true;
            Debug.Log($"Added connection point: {point.name} | {ConnectionPoints[point]}");
        }
        Debug.Log($"Total connection points: {ConnectionPoints.Count}");
    }

    // If the drill is not collecting -> Set bool to true -> Start Coroutine
    public void StartCollection() { if (!isCollecting) { isCollecting = true; StartCoroutine(CollectResources()); } }
    // Ste bool to false -> StopCoroutine -> Stop animator
    public void StopCollection() { isCollecting = false; StopCoroutine(CollectResources()); drillAnimator.StopDrilling(); }

    private IEnumerator CollectResources()
    {
        while (isCollecting)
        {
            if (connectedStorage != null)
            {
                if (connectedStorage.currentStorage < connectedStorage.storageCapacity)
                {
                    // If the drill is not lowered -> Lower drill -> Wait for animation to end
                    if (!drillAnimator.IsLowered) { drillAnimator.LowerDrill(); yield return new WaitForSeconds(drillAnimator.LoweringDuration); }

                    connectedStorage.AddResource(1, tileData.resourceType);
                    // Debug.Log($"Tile resouce under drill is | {tileData.resourceType}");
                    drillAnimator.StartDrilling();
                }
                // Debug -> Stop Collecting -> Stop Animation Play -> Pause, no further action
                else { Debug.LogWarning("Connected storage is full!"); StopCollection(); drillAnimator.StopDrilling(); yield return null; }
            }
            // Debug -> Stop Collecting -> Stop Animation Play -> Pause, no further action
            else { Debug.LogWarning("Drill has no connected storage!"); StopCollection(); drillAnimator.StopDrilling(); yield return null; }

            yield return new WaitForSeconds(collectionInterval);
        }
    }

    // Write / use a function from ConnectableBuilding to run the StartCollection function
    public override void ConnectTo(ConnectableBuilding otherBuilding)
    {
        if (otherBuilding is Storage storage)
        {
            connectedStorage = storage;
            StartCollection();
        }
    }

    public override bool CanConnectTo(ConnectableBuilding otherBuilding)
    {
        return otherBuilding is Sawmill or Storage;
    }

    private void OnDestroy() { FindFirstObjectByType<ConnectionManager>().RemoveConnections(this); }
}