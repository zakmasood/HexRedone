using CustomLogger;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Logger = CustomLogger.Logger;
using Random = UnityEngine.Random;

public class FactoryManager : MonoBehaviour
{
    // Store the list of factories
    public List<Factory> factories = new List<Factory>();

    public PlayerController playerController;
    public WorldGen worldGen;
    public TileData data;

    // Thread-safe queue for tasks to be executed on the main thread
    private static Queue<Action> mainThreadTasks = new Queue<System.Action>();
    private static readonly object mainThreadTasksLock = new object();

    private static FactoryManager instance;

    // Dictionary to store resource counters
    private static Dictionary<string, int> resourceCounters = new Dictionary<string, int>();

    // Add a new factory, now include a factory type parameter
    public void AddFactory(Vector3 position, string buildingType, string resourceType)
    {
        GameObject factoryObject = new GameObject(buildingType);
        factoryObject.transform.position = position;
        Factory factory = factoryObject.AddComponent<Factory>();
        factory.resourceType = resourceType; // Assign resource type to factory
        factories.Add(factory);

        LeanTween.delayedCall(1.5f, () => { factoryObject.AddComponent<SphereCollider>(); });

        Debug.Log($"Factory created at position: {position}, Total Factories: {factories.Count}");

        // Automatically run a repeating task to increment the corresponding resource counter
        factory.StartCounterIncrementTask();
    }

    // Remove a factory
    public void RemoveFactory(Factory factory)
    {
        if (factories.Contains(factory))
        {
            factory.StopAllTasks();
            factories.Remove(factory);
            Destroy(factory.gameObject);
            Debug.Log($"Factory removed. Total Factories: {factories.Count}");
        }
    }

    public void PrintAllFactories()
    {
        foreach (Factory factory in factories)
        {
            Debug.Log($"Factory Resource Type: {factory.resourceType}");
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void IncrementResourceCounter(string resourceType)
    {
        lock (resourceCounters)
        {
            if (resourceCounters.ContainsKey(resourceType))
            {
                resourceCounters[resourceType]++;
            }
            else
            {
                resourceCounters[resourceType] = 1;
            }
        }

        EnqueueMainThreadTask(() =>
        {
            Debug.Log($"Resource '{resourceType}' incremented, new value: {resourceCounters[resourceType]}");
            instance.PrintAllFactories();
        });
    }

    // Enqueue a task to be executed on the main thread
    public static void EnqueueMainThreadTask(System.Action action)
    {
        lock (mainThreadTasksLock)
        {
            mainThreadTasks.Enqueue(action);
        }
    }

    void Update()
    {
        // Process main thread tasks
        lock (mainThreadTasksLock)
        {
            while (mainThreadTasks.Count > 0)
            {
                var action = mainThreadTasks.Dequeue();
                Debug.Log("Executing main thread task.");
                action.Invoke();
            }
        }

        // For demonstration purposes, let's place a factory at a random position when the space key is pressed
        if (Input.GetMouseButtonDown(0))
        {
            data = worldGen.GetTileData(worldGen.ExtractTileID(playerController.clickedTileData()));
            Logger.Log(LogLevel.Warning, data.previousResourceType);
            LeanTween.delayedCall(0.1f, () => { AddFactory(new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)), playerController.buildingToBuild, data.resourceType); });
        }
    }
}

public class Factory : MonoBehaviour
{
    public string resourceType; // The type of resource this factory produces
    public TileData data;
    public WorldGen worldGen;
    public PlayerController playerController;
    private List<Thread> taskThreads = new List<Thread>();

    [Obsolete]
    public void Awake()
    {
        // If not set through the Inspector, find the component dynamically
        if (worldGen == null)
        {
            worldGen = FindObjectOfType<WorldGen>();
            if (worldGen == null)
            {
                Debug.LogError("WorldGen not found in the scene. Please assign it.");
            }
        }

        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController not found in the scene. Please assign it.");
            }
        }
    }

    public void StartCounterIncrementTask()
    {
        if (worldGen == null)
        {
            Debug.LogError("WorldGen is not initialized.");
            return;
        }

        if (playerController == null)
        {
            Debug.LogError("PlayerController is not initialized.");
            return;
        }

        var clickedTile = playerController.clickedTileData();
        if (clickedTile == null)
        {
            Debug.LogError("PlayerController.clickedTileData() returned null.");
            return;
        }

        var tileID = worldGen.ExtractTileID(clickedTile);
        if (tileID == null)
        {
            Debug.LogError("WorldGen.ExtractTileID() returned null or invalid ID.");
            return;
        }

        data = worldGen.GetTileData(tileID);
        if (data == null)
        {
            Debug.LogError("WorldGen.GetTileData() returned null.");
            return;
        }

        // Ensure previousResourceType is set
        if (string.IsNullOrEmpty(data.previousResourceType))
        {
            data.previousResourceType = data.resourceType;
        }

        Thread taskThread = new Thread(() =>
        {
            while (true)
            {
                // Wait for 1 second
                Thread.Sleep(1000);

                // Check for proper initialization
                if (!string.IsNullOrEmpty(data.previousResourceType))
                {
                    FactoryManager.IncrementResourceCounter(data.previousResourceType);
                }
            }
        });

        taskThreads.Add(taskThread);
        Debug.Log("Starting counter increment task thread for factory producing " + data.previousResourceType);
        taskThread.Start();
    }

    public void StopAllTasks()
    {
        lock (taskThreads)
        {
            foreach (var thread in taskThreads)
            {
                if (thread.IsAlive)
                {
                    Debug.Log("Aborting thread for factory");
                    thread.Abort(); // Aborting threads is not recommended, use with caution
                }
            }
            taskThreads.Clear();
        }
    }

    void OnDestroy()
    {
        Debug.Log("Factory is being destroyed.");
        StopAllTasks();
    }

    void OnMouseOver()
    {
        Debug.Log("Mouse over");
        if (Input.GetMouseButtonDown(1)) // Right-click
        {
            DestroyFactory();
        }
    }

    void DestroyFactory()
    {
        Debug.Log("Factory is being right-clicked and will be destroyed.");
        Destroy(gameObject);
    }
}
