using CustomLogger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Logger = CustomLogger.Logger;

public class Factory
{
    private int producedMaterials;
    private const int maxMaterials = 100;
    private bool isRunning;
    private bool isTaskComplete;
    private string materialType; // Changed to string for material type
    private static readonly object lockObj = new object();
    private int factoryId;

    public Factory(int id, string type)
    {
        producedMaterials = 0;
        isRunning = false;
        factoryId = id;
        isTaskComplete = false;
        materialType = type;
    }

    public int GetFactoryId()
    {
        return factoryId;
    }

    // Restart the production task for the factory with the given ID
    public void RestartTask(int id)
    {
        lock (lockObj)
        {
            if (factoryId == id)
            {
                if (!isRunning)
                {
                    producedMaterials = 0;
                    isTaskComplete = false;
                    StartTask();
                    Logger.Log(LogLevel.Debug, $"Restarted task for factory {factoryId}.");
                }
                else
                {
                    Logger.Log(LogLevel.Error, $"Factory {factoryId} is currently running. Cannot restart.");
                }
            }
        }
    }

    // Start the production task if it is not already running
    public void StartTask()
    {
        lock (lockObj)
        {
            if (isRunning)
            {
                Debug.Log($"Factory {factoryId} is already running a task.");
                return;
            }

            isRunning = true;
            isTaskComplete = false;
            Task.Run(() => RunTask());
        }
    }

    // Run the production task
    private async Task RunTask()
    {
        while (isRunning && producedMaterials < maxMaterials)
        {
            lock (lockObj)
            {
                producedMaterials++;
                Logger.Log(LogLevel.Info, $"Factory {factoryId} producing {materialType}: {producedMaterials}");

                if (producedMaterials >= maxMaterials)
                {
                    Logger.Log(LogLevel.Success, $"Task complete. Factory {factoryId} produced {producedMaterials} {materialType}.");
                    isRunning = false;
                    isTaskComplete = true;

                    // Add produced materials to global resource counter
                    producedMaterials = 0; // Reset produced materials for the next task
                }
            }
            ResourceManager.Instance.AddResources(materialType, 1);
            await Task.Delay(1000); // Simulate work by delaying for 1000 milliseconds
        }
    }

    // Stop the production task
    public void StopTask()
    {
        lock (lockObj)
        {
            isRunning = false;
            Logger.Log(LogLevel.Warning, $"Task Aborted. Factory {factoryId} is now idle.");
        }
    }

    // Check if the task is complete
    public bool IsTaskComplete()
    {
        lock (lockObj)
        {
            return isTaskComplete;
        }
    }
}


public class FactoryManager : MonoBehaviour
{
    private List<Factory> factories = new List<Factory>();
    private int[] factoryIds = { };

    void OnApplicationQuit()
    {
        foreach (var factory in factories)
        {
            factory.StopTask();
        }
    }

    public void AddFactory(int factoryID, string resource)
    {
        // Instantiate the factory with a unique ID and add to the list
        Factory newFactory = new Factory(factoryID, resource);
        factories.Add(newFactory);

        // Start the factory's task
        newFactory.StartTask();

        Debug.Log("Added new factory with ID: " + newFactory.GetFactoryId());
    }

    // Check if a specific factory has completed its task by factory ID
    public bool IsFactoryTaskComplete(int factoryId)
    {
        foreach (var factory in factories)
        {
            if (factory.GetFactoryId() == factoryId)
            {
                return factory.IsTaskComplete();
            }
        }
        Debug.LogWarning($"Factory with ID {factoryId} not found.");
        return false;
    }

    // Restart a specific factory by ID
    public void RestartFactory(int factoryId)
    {
        foreach (var factory in factories)
        {
            if (factory.GetFactoryId() == factoryId)
            {
                factory.RestartTask(factoryId);
                return;
            }
        }
        Debug.LogWarning($"Factory with ID {factoryId} not found. Cannot restart.");
    }
}
