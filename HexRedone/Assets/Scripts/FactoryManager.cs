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
    private string materialType;
    private static readonly object lockObj = new object();
    private int factoryId;
    public static bool debugMode = false; // Static flag for debug mode

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

    // Set debug mode
    public static void SetDebugMode(bool isDebug)
    {
        debugMode = isDebug;
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
                // Logger.Log(LogLevel.Info, $"Factory {factoryId} producing {materialType}: {producedMaterials}");

                if (producedMaterials >= maxMaterials)
                {
                    isRunning = false;
                    isTaskComplete = true;

                    Logger.Log(LogLevel.Success, $"Task complete. Factory {factoryId} produced {producedMaterials} {materialType} and set isComplete to {isTaskComplete}");

                    // Add produced materials to global resource counter
                    producedMaterials = 0; // Reset produced materials for the next task
                }
            }
            ResourceManager.Instance.AddResources(materialType, 1);
            await Task.Delay(debugMode ? 50 : 1000); // 50ms in debug mode, 1000ms otherwise
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
            Logger.Log(LogLevel.Debug, $"Factory {factoryId} isTaskComplete status: {isTaskComplete}");
            return isTaskComplete;
        }
    }

    // Reset the task completion status
    public void ResetTaskComplete()
    {
        lock (lockObj)
        {
            isTaskComplete = false;
            Logger.Log(LogLevel.Debug, $"Factory {factoryId} task completion status reset to {isTaskComplete}");
        }
    }
}


public class FactoryManager : MonoBehaviour
{
    private List<Factory> factories = new List<Factory>();
    private int[] factoryIds = { };
    public bool isDebug;

    void OnApplicationQuit()
    {
        foreach (var factory in factories)
        {
            factory.StopTask();
        }
    }

    private void Start()
    {
        Factory.SetDebugMode(isDebug);
    }

    // Reset the task completion status for a specific factory by ID
    public void ResetFactoryTaskComplete(int factoryId)
    {
        foreach (var factory in factories)
        {
            if (factory.GetFactoryId() == factoryId)
            {
                factory.ResetTaskComplete();
                Debug.Log($"Task completion status reset for factory with ID: {factoryId}");
                return;
            }
        }
        Debug.LogWarning($"Factory with ID {factoryId} not found. Cannot reset task completion status.");
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
                bool isComplete = factory.IsTaskComplete();
                return isComplete;
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

    // Stop all factories
    public void StopAllFactories()
    {
        foreach (var factory in factories)
        {
            factory.StopTask();
        }
    }

    // Delete a specific factory by ID
    public void DeleteFactory(int factoryId)
    {
        for (int i = 0; i < factories.Count; i++)
        {
            if (factories[i].GetFactoryId() == factoryId)
            {
                // Stop the factory's task
                factories[i].StopTask();

                // Remove the factory from the list
                factories.RemoveAt(i);

                Debug.Log($"Factory with ID {factoryId} has been deleted.");
                return;
            }
        }
        Debug.LogWarning($"Factory with ID {factoryId} not found. Cannot delete.");
    }
}
