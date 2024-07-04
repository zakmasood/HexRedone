using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    private Dictionary<string, int> resourceCounters = new Dictionary<string, int>();
    private Dictionary<string, Text> resourceTexts = new Dictionary<string, Text>();

    public Text noneText;
    public Text treeText;
    public Text waterText;
    public Text earthText;
    public Text coalText;
    public Text ironText;
    public Text copperText;
    public Text goldText;
    public Text uraniumText;
    public Text stoneText;
    public Text oilText;
    public Text gasText;

    void Awake()
    {
        Instance = this;
        InitializeResourceCounters();
    }

    void Start()
    {
        // Initialize text fields with initial resource counts
        UpdateResourceTexts();
    }

    void Update()
    {
        // Example: Update text fields every frame (you might want to optimize this)
        UpdateResourceTexts();
    }

    private void UpdateResourceTexts()
    {
        noneText.text = $"None: {GetResourceCount(Elements.None)}";
        treeText.text = $"Tree: {GetResourceCount(Elements.Tree)}";
        waterText.text = $"Water: {GetResourceCount(Elements.Water)}";
        earthText.text = $"Earth: {GetResourceCount(Elements.Earth)}";
        coalText.text = $"Coal Ore: {GetResourceCount(Elements.CoalOre)}";
        ironText.text = $"Iron Ore: {GetResourceCount(Elements.IronOre)}";
        copperText.text = $"Copper Ore: {GetResourceCount(Elements.CopperOre)}";
        goldText.text = $"Gold Ore: {GetResourceCount(Elements.GoldOre)}";
        uraniumText.text = $"Uranium Ore: {GetResourceCount(Elements.UraniumOre)}";
        stoneText.text = $"Stone: {GetResourceCount(Elements.Stone)}";
        oilText.text = $"Oil: {GetResourceCount(Elements.Oil)}";
        gasText.text = $"Natural Gas: {GetResourceCount(Elements.NaturalGas)}";
    }

    private void InitializeResourceCounters()
    {
        // Initialize resource counters for each resource type
        resourceCounters[Elements.None] = 0;
        resourceCounters[Elements.Tree] = 0;
        resourceCounters[Elements.Water] = 0;
        resourceCounters[Elements.Earth] = 0;
        resourceCounters[Elements.CoalOre] = 0;
        resourceCounters[Elements.IronOre] = 0;
        resourceCounters[Elements.CopperOre] = 0;
        resourceCounters[Elements.GoldOre] = 0;
        resourceCounters[Elements.UraniumOre] = 0;
        resourceCounters[Elements.Stone] = 0;
        resourceCounters[Elements.Oil] = 0;
        resourceCounters[Elements.NaturalGas] = 0;
    }

    public void AddResources(string type, int amount)
    {
        if (resourceCounters.ContainsKey(type))
        {
            resourceCounters[type] += amount;
            Debug.Log($"Added {amount} {type} to global resources. Total {type}: {resourceCounters[type]}");
        }
        else
        {
            Debug.LogError($"Unknown resource type {type}");
        }
    }

    public int GetResourceCount(string type)
    {
        if (resourceCounters.ContainsKey(type))
        {
            return resourceCounters[type];
        }
        else
        {
            Debug.LogError($"Unknown resource type {type}");
            return 0;
        }
    }
}
