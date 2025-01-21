using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

public class Sawmill : ConnectableBuilding
{
    public RecipeLoader recipeLoader;
    private Recipe currentRecipe;

    private Storage inputStorage = null;
    private Storage outputStorage = null;
    private bool isProcessing = false;

    public float transferTime = 1f;

    private void Start()
    {
        ConnectionPoints.Clear(); // Ensure the dictionary starts fresh for each instance

        isProcessing = false;
        currentRecipe = null;

        foreach (var point in GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("CableTerminal")))
        {
            ConnectionPoints[point] = true;
            Debug.Log($"Added connection point: {point.name} | {ConnectionPoints[point]}");
        }

        Debug.Log($"Total connection points: {ConnectionPoints.Count}");
    }

    private void Update()
    {
        Debug.Log($"Update called. isProcessing: {isProcessing}, inputStorage: {inputStorage}, outputStorage: {outputStorage}");
        if (!isProcessing && inputStorage != null && outputStorage != null)
        {
            TryStartProcessing();
            Debug.Log("Processing!");
        }
    }

    private void TryStartProcessing()
    {
        if (currentRecipe == null)
        {
            Debug.Log($"Trying to get recipe for input resource: {inputStorage.currentResourceType}");
            currentRecipe = recipeLoader.GetRecipeByInput(inputStorage.currentResourceType);
            if (currentRecipe == null)
            {
                Debug.LogWarning("No Valid Recipe For Input Resource");
                return;
            }

            Debug.Log($"Found recipe: {currentRecipe.inputResource} -> {currentRecipe.outputResource}");
        }

        if (inputStorage.currentStorage >= currentRecipe.inputAmount && outputStorage.currentStorage + currentRecipe.outputAmount <= outputStorage.storageCapacity)
        {
            StartCoroutine(ProcessResource());
        }
    }

    private IEnumerator ProcessResource()
    {
        isProcessing = true;

        while (true)
        {
            if (inputStorage.currentStorage >= currentRecipe.inputAmount && outputStorage.currentStorage + currentRecipe.outputAmount <= outputStorage.storageCapacity)
            {
                yield return new WaitForSeconds(currentRecipe.processingTime);

                inputStorage.RemoveResource(currentRecipe.inputAmount);
                outputStorage.AddResource(currentRecipe.outputAmount, (ResourceType)Enum.Parse(typeof(ResourceType), currentRecipe.outputResource));

                Debug.Log($"Processed {currentRecipe.inputAmount} {currentRecipe.inputResource} into {currentRecipe.outputAmount} {currentRecipe.outputResource}.");
                Debug.Log($"Waiting for {currentRecipe.processingTime} seconds");
            }
            else
            {
                isProcessing = false;
                yield break;
            }
        }
    }

    public void SetInputStorage(Storage storage) { inputStorage = storage; }
    public void SetOutputStorage(Storage storage) { outputStorage = storage; }

    public override bool CanConnectTo(ConnectableBuilding otherBuilding) => otherBuilding is Storage;
    
    public override void ConnectTo(ConnectableBuilding otherBuilding)
    {
        if (otherBuilding is Storage storage)
        {
            Transform availablePoint = GetAvailableConnectionPoint();

            if (availablePoint != null)
            {
                if (inputStorage == null) { SetInputStorage(storage); Debug.Log("Connected Input!"); }
                else if (inputStorage != null && outputStorage == null) { SetOutputStorage(storage); Debug.Log("Connected Output!"); }
                else { Debug.LogWarning("Both input and output storages are already connected!"); }

                ConnectedBuildings.Add(otherBuilding);
            }
        }
    }

    private void OnDestroy() { FindFirstObjectByType<ConnectionManager>().RemoveConnections(this); }
}