using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ResourceCost
{
    public ResourceType resourceType;
    public int amount;
}

public class ResourceManager : MonoBehaviour
{
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    [SerializeField] private GameObject resourceUIPrefab; // Prefab for the UI element
    [SerializeField] private Transform UIParent; // Parent for the UI elements (e.g., a Vertical Layout Group)

    private Dictionary<ResourceType, Text> resourceUIElements = new Dictionary<ResourceType, Text>();

    private void Start()
    {
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            if (type != ResourceType.None)
            {
                resources[type] = 0;
                CreateResourceUIElement(type);
            }
        }
    }

    private void CreateResourceUIElement(ResourceType resourceType)
    {
        GameObject UIElement = Instantiate(resourceUIPrefab, UIParent); // Instantiate UI prefab
        Text textComponent = UIElement.GetComponentInChildren<Text>();

        if (textComponent != null)
        {
            textComponent.text = $"{resourceType}: 0";
            resourceUIElements[resourceType] = textComponent; // Store for updates
        }
    }

    public void UpdateResourceUI(ResourceType resourceType, int amount)
    {
        if (resourceUIElements.ContainsKey(resourceType))
        {
            resourceUIElements[resourceType].text = $"{resourceType}: {amount}";
        }
    }

    public void RefreshResourceUI()
    {
        foreach (var resourceType in resourceUIElements.Keys)
        {
            int currentAmount = GetResourceAmount(resourceType);
            resourceUIElements[resourceType].text = $"{resourceType}: {currentAmount}";
        }
    }


    public void AddResource(ResourceType resourceType, int amount)
    {
        if (resources.ContainsKey(resourceType))
        {
            resources[resourceType] += amount;
            UpdateResourceUI(resourceType, resources[resourceType]);
            // Debug.Log($"Added {amount} {resourceType}. Total: {resources[resourceType]}");
        }
        else
        {
            Debug.LogWarning($"Resource type {resourceType} not found!");
        }
    }

    public bool CanAfford(List<ResourceCost> costs)
    {
        foreach (var cost in costs)
        {
            if (!resources.ContainsKey(cost.resourceType) || resources[cost.resourceType] < cost.amount)
            {
                return false; // Not enough of a required resource
            }
        }
        return true; // All requirements met
    }

    public void DeductResources(List<ResourceCost> costs)
    {
        foreach (var cost in costs)
        {
            if (resources.ContainsKey(cost.resourceType))
            {
                resources[cost.resourceType] -= cost.amount;
            }
        }
    }

    public int GetResourceAmount(ResourceType resourceType)
    {
        return resources.ContainsKey(resourceType) ? resources[resourceType] : 0;
    }
}
