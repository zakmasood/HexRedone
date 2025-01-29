using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Drill : ConnectableBuilding
{
    public TileData tileData; // The tile this drill is placed on
    public ResourceManager resourceManager; // Global resource manager
    private Storage connectedStorage; // The storage this drill is connected to
    private DrillAnimator drillAnimator; // Animator for the drill

    public float collectionInterval = 1f; // Time in seconds between collections
    private bool isCollecting = false;

    [SerializeField]
    private TMP_Text floatingText;

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
        // Debug.Log($"Total connection points: {ConnectionPoints.Count}");
    }

    /* # -----| Hover Handling |----- # */

    private void OnMouseEnter()
    {
        // Get the position slightly above the building
        Vector3 textPosition = transform.position + Vector3.up * 3.5f;

        FloatingTextManager.Instance.ShowText(textPosition, tileData.resourceType.ToString(), 0.5f);
        Debug.Log("Hovering over drill.");
    }

    private void OnMouseExit() { FloatingTextManager.Instance.HideText(0.5f); }


    /* # -----| Collection Handling |----- # */

    // If the drill is not collecting -> Set bool to true -> Start Coroutine
    public void StartCollection() { if (!isCollecting) { isCollecting = true; StartCoroutine(CollectResources()); } }
    // Ste bool to false -> StopCoroutine -> Stop animator
    public void StopCollection() { isCollecting = false; StopCoroutine(CollectResources()); drillAnimator.StopDrilling(); }

    private IEnumerator CollectResources()
    {
        while (isCollecting)
        {
            if (connectedStorage != null && (connectedStorage.currentStorage < connectedStorage.storageCapacity))
            {
                // If the drill is not lowered -> Lower drill -> Wait for animation to end
                if (!drillAnimator.IsLowered) { drillAnimator.LowerDrill(); yield return new WaitForSeconds(drillAnimator.LoweringDuration); }

                connectedStorage.AddResource(1, tileData.resourceType);
                // Debug.Log($"Tile resouce under drill is | {tileData.resourceType}");
                drillAnimator.StartDrilling();
            }
            // Debug -> Stop Collecting -> Stop Animation Play -> Pause, no further action
            else { Debug.LogWarning("Connected storage is full!"); StopCollection(); drillAnimator.StopDrilling(); yield return null; }

            // Wait for the collection interval
            yield return new WaitForSeconds(collectionInterval);
        }
    }

    // Override the ConnectTo method from ConnectableBuilding
    public override void ConnectTo(ConnectableBuilding otherBuilding)
    {
        if (otherBuilding is Storage storage)
        {
            connectedStorage = storage;
            StartCollection();
        }
    }

    // Return building type that can be connected to
    public override bool CanConnectTo(ConnectableBuilding otherBuilding)
    {
        return otherBuilding is Sawmill or Storage;
    }

    // Remove connections when the drill is destroyed
    private void OnDestroy() { FindFirstObjectByType<ConnectionManager>().RemoveConnections(this); }
}