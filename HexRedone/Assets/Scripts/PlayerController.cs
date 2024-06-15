using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

public static class StringExtension
{
    public static string Bold(this string str) => "<b>" + str + "</b>";
    public static string Color(this string str, string clr) => string.Format("<color={0}>{1}</color>", clr, str);
    public static string Italic(this string str) => "<i>" + str + "</i>";
    public static string Size(this string str, int size) => string.Format("<size={0}>{1}</size>", size, str);
}

public class Inventory
{
    public Dictionary<string, int> resources;

    public Inventory()
    {
        this.resources = new Dictionary<string, int>();
    }

    public void LoadInventory(string filePath)
    {
        // Clear existing data
        resources.Clear();

        // Open the file for reading with error checking
        try
        {
            StreamReader reader = new StreamReader(filePath);

            // Skip header row
            reader.ReadLine();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] data = line.Split(',');
                if (data.Length == 2)
                {
                    string resourceType = data[0];
                    int count = int.Parse(data[1]);
                    resources.Add(resourceType, count);
                }
                else
                {
                    Debug.LogError($"Invalid format in inventory file: {line}");
                }
            }

            reader.Close();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading inventory: {e.Message}");
        }
    }

    public void SaveInventory(string filePath)
    {
        try
        {
            Debug.Log($"Saving inventory to {filePath}");

            if (resources == null)
            {
                Debug.LogError("Resources dictionary is null.");
                return;
            }

            if (resources.Count == 0)
            {
                Debug.LogWarning("Resources dictionary is empty. No data to save.");
                return;
            }

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write header row
                Debug.Log("Writing header row: ResourceType,Count");
                writer.WriteLine("ResourceType,Count");

                // Write each resource and its count
                foreach (var resource in resources)
                {
                    if (resource.Key == null)
                    {
                        Debug.LogWarning("Encountered a null resource key. Skipping this entry.");
                        continue;
                    }

                    Debug.Log($"Writing resource: {resource.Key}, Count: {resource.Value}");
                    writer.WriteLine($"{resource.Key},{resource.Value}");
                }
            }

            Debug.Log("Inventory saved successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving inventory: {e.Message}");
        }
    }



}


public class BuildingData
{
    public string BuildingType { get; set; }
    public List<string> ItemNeeded { get; set; }
    public string icon { get; set; }
    public string color { get; set; }
    public string category { get; set; }
}

public class BuildingsList
{
    public List<BuildingData> Buildings { get; set; }
}

public class Factory
{
    public int tileId;
    private int productionCount;
    private bool shouldStop;
    public Factory(int tileId, string oreType)
    {
        this.tileId = tileId;
        OreType = oreType;
        productionCount = 0;
        Inventory = 0;
        IsRunning = false;
        shouldStop = false;
    }
    public string OreType { get; }
    public int Inventory { get; private set; }
    public bool IsRunning { get; private set; }

    public async Task StartProduction()
    {
        IsRunning = true;
        shouldStop = false;
        while (productionCount < 100 && !shouldStop)
        {
            await Task.Delay(1000); // Simulate production time
            productionCount++;
            Console.WriteLine($"Factory on tile {tileId} producing {OreType}: {productionCount}/100");
        }
        if (!shouldStop)
        {
            Inventory += productionCount;
            productionCount = 0;
            Console.WriteLine($"Factory on tile {tileId} transferred 100 {OreType} to inventory.");
        }
        IsRunning = false;
    }
    public void StopProduction()
    {
        shouldStop = true;
    }
}

public class PlayerController : MonoBehaviour
{
    public WorldGen worldGen;
    public ValidationEngine validator;
    public QuestController questController;

    public Text infoText;
    public GameObject labelPrefab;
    public Transform labelsContainer;
    public GameObject buildingPlaceholder;
    public GameObject clickedTile;

    public Dictionary<string, int> buildingCounts = new Dictionary<string, int>();
    private Dictionary<string, Text> buildingTexts = new Dictionary<string, Text>();
    private Dictionary<int, Factory> factories = new Dictionary<int, Factory>();
    private Dictionary<int, Task> factoryTasks = new Dictionary<int, Task>();

    public int count;

    Inventory inventory = new Inventory();

    GameObject DetectClickedTile()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            return hit.transform.gameObject;
        }
        return null;
    }

    public List<string> WhatCanIBuild(int tileID)
    {
        string path = Application.dataPath + "/buildingTaxonomy.json";

        if (!File.Exists(path))
        {
            Debug.LogError("Json file not found at " + path);
            return new List<string>();
        }

        string json = File.ReadAllText(path);
        BuildingsList buildings = JsonConvert.DeserializeObject<BuildingsList>(json);

        if (buildings == null || buildings.Buildings == null || buildings.Buildings.Count == 0)
        {
            Debug.LogWarning("No building data is available from the JSON file.");
            return new List<string>();
        }

        if (worldGen == null)
        {
            Debug.LogError("worldGen is not assigned in the inspector.");
            return new List<string>();
        }

        TileData data = worldGen.GetTileData(tileID);

        if (data == null)
        {
            Debug.LogError("TileData is null for tile ID " + tileID);
            return new List<string>();
        }

        string resourceType = data.resourceType;
        Debug.Log("Resource type on tile " + tileID + ": " + resourceType);

        List<string> buildingTypes = new List<string>();

        foreach (BuildingData building in buildings.Buildings)
        {
            if (building.ItemNeeded != null && building.ItemNeeded.Contains(resourceType))
            {
                buildingTypes.Add(building.BuildingType);
                Debug.Log("Building " + building.BuildingType + " can be built on this tile.");
            }
        }

        if (buildingTypes.Count == 0)
        {
            Debug.Log("No buildings can be built on this tile with element type " + resourceType);
        }

        return buildingTypes;
    }

    public void BuildFiveBuildingsQuest()
    {
        if (questController.hasDistinctValues(5, buildingCounts))
        {
            print("You win!!".Color("lime"));
        }
    }

    public bool CanPlaceBuilding(int tileID, string buildingType)
    {
        string path = Application.dataPath + "/buildingTaxonomy.json";
        string json = File.ReadAllText(path);

        BuildingsList buildings = JsonConvert.DeserializeObject<BuildingsList>(json);

        TileData data = worldGen.GetTileData(tileID);

        List<string> itemsNeeded = new List<string>();
        foreach (BuildingData building in buildings.Buildings)
        {
            if (building.BuildingType == buildingType)
            {
                itemsNeeded = building.ItemNeeded;
                break;
            }
        }

        // Use hardcoded resource types list instead of enum
        List<string> resourceTypeList = new List<string>
        {
            Elements.None,
            Elements.Tree,
            Elements.Water,
            Elements.Earth,
            Elements.CoalOre,
            Elements.IronOre,
            Elements.CopperOre,
            Elements.GoldOre,
            Elements.UraniumOre,
            Elements.Stone,
            Elements.Oil,
            Elements.NaturalGas
        };

        if (buildings.Buildings.Any(b => b.BuildingType == data.resourceType))
        {
            Debug.Log("Cannot place building. Tile already has a building.");
            return false;
        }

        foreach (string item in itemsNeeded)
        {
            if (!resourceTypeList.Contains(item))
            {
                Debug.Log("Cannot place building. Required item " + item + " not in tile elements.");
                return false;
            }
        }

        // Added check for correct resource exist on the tile
        if (!itemsNeeded.Contains(data.resourceType))
        {
            Debug.Log($"Cannot place building. Tile does not contain required element for building type {buildingType}.");
            return false;
        }

        if (data.x >= 0 && data.x < worldGen.tileData.GetLength(0) && data.z >= 0 && data.z < worldGen.tileData.GetLength(1))
        {
            worldGen.tileData[data.x, data.z].resourceType = data.resourceType;
            Debug.Log("Placing tile");
            worldGen.SetTileResourceType(tileID, buildingType);
        }
        Debug.Log("Can place building type " + buildingType + " on tile " + tileID + ".");
        return true;
    }

    public void CreateOrUpdateLabel(string buildingToBuild)
    {
        // Check if the building already has a label
        if (buildingTexts.ContainsKey(buildingToBuild))
        {
            // Update the text of the existing label
            buildingTexts[buildingToBuild].text = buildingToBuild + ": " + buildingCounts[buildingToBuild];
        }
        else
        {
            // Instantiate new label for this type of building
            GameObject newLabel = Instantiate(labelPrefab, new Vector2(labelsContainer.transform.position.x, labelsContainer.transform.position.y), Quaternion.identity, labelsContainer);
            Text newLabelText = newLabel.GetComponent<Text>();

            // Set the label's text
            newLabelText.text = buildingToBuild + ": " + buildingCounts[buildingToBuild];

            // Add the new label to the dictionary
            buildingTexts.Add(buildingToBuild, newLabelText);
        }
    }

    public void IncrementBuildingCount(string buildingToBuild)
    {
        if (!buildingCounts.ContainsKey(buildingToBuild))
        {
            buildingCounts[buildingToBuild] = 0;
        }

        buildingCounts[buildingToBuild]++;
        CreateOrUpdateLabel(buildingToBuild);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            worldGen.SaveWorldData("tileData.json");
            inventory.SaveInventory("inventory.csv");
            print("Tile data saved to " + Path.Combine(Application.dataPath, "tileData.json"));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            worldGen.DeleteGrid();
            Debug.Log("Deleting World");
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            //worldGen.LoadWorldData(Application.dataPath + "tiledata.json");
            foreach (KeyValuePair<string, int> item in buildingCounts)
            {
                Debug.Log("Key: " + item.Key + " Value: " + item.Value);
            }
        }

        if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
        {
            clickedTile = DetectClickedTile();
            if (clickedTile != null)
            {
                int tileID = worldGen.ExtractTileID(clickedTile.name);
                TileData data = worldGen.GetTileData(tileID);

                if (data != null)
                {
                    infoText.text = $"Tile ID: {tileID}, X: {data.x}, Z: {data.z}, Element: {data.resourceType}";
                }
                else
                {
                    Debug.Log("TileData not found for tileID: " + tileID);
                }
                List<string> buildableBuildings = WhatCanIBuild(tileID);

                if (buildableBuildings.Count == 0)
                {
                    Debug.Log("No buildable buildings for this tile");
                }

                string buildingToBuild = buildableBuildings[0];

                if (CanPlaceBuilding(tileID, buildingToBuild))
                {
                    Instantiate(buildingPlaceholder, new Vector3(clickedTile.transform.position.x, 3, clickedTile.transform.position.z), Quaternion.identity);
                    createFactory(tileID);

                    if (buildingCounts.ContainsKey(buildingToBuild))
                    {
                        buildingCounts[buildingToBuild]++;
                    }
                    else
                    {
                        buildingCounts[buildingToBuild] = 1;
                    }
                    Debug.Log("Built " + buildingToBuild + ". Total: " + buildingCounts[buildingToBuild]);

                    if (buildingTexts.ContainsKey(buildingToBuild))
                    {
                        buildingTexts[buildingToBuild].text = buildingToBuild + ": " + buildingCounts[buildingToBuild];
                    }
                    else
                    {
                        IncrementBuildingCount(buildingToBuild);
                    }
                }
            }
            BuildFiveBuildingsQuest();
        }
    }

    public void createFactory(int tileID)
    {
        if (factories.ContainsKey(tileID))
        {
            Debug.LogWarning($"Factory Already Exists On Tile {tileID}");
            return;
        }

        TileData data = worldGen.GetTileData(tileID);
        if (data == null)
        {
            Debug.LogWarning("Invalid tile ID!");
            return;
        }

        var factory = new Factory(tileID, data.resourceType);
        if (factories.TryAdd(tileID, factory))
        {
            StartFactoryProduction(factory);
            Debug.Log($"Factory created on tile {tileID} producing {data.resourceType}");
        }
    }

    private void StartFactoryProduction(Factory factory)
    {
        factoryTasks[factory.tileId] = Task.Run(async () => {
            while (true)
            {
                await factory.StartProduction();
                if (!factory.IsRunning) break;
            }
        });
    }

    public void StopFactoryProduction(int tileId)
    {
        if (factories.TryGetValue(tileId, out Factory factory))
        {
            factory.StopProduction();
            Debug.Log($"Stopping production for factory on tile {tileId}");
        }
    }

    public async Task RemoveFactory(int tileId)
    {
        if (factories.TryGetValue(tileId, out Factory factory))
        {
            factory.StopProduction();
            if (factoryTasks.TryGetValue(tileId, out Task task))
            {
                await task;
                factoryTasks.Remove(tileId);
            }
            factories.Remove(tileId);
            Debug.Log($"Factory on tile {tileId} has been removed.");
        }
    }

    public void CollectFactoryInventory(int tileId)
    {
        if (factories.TryGetValue(tileId, out Factory factory))
        {
            int inventory = factory.Inventory;
            Debug.Log($"Collected {inventory} {factory.OreType} from factory on tile {tileId}");
            // Transfer inventory to global resource management
        }
    }

    public void SaveBuildingCounts(string filePath)
    {
        try
        {
            string json = JsonConvert.SerializeObject(buildingCounts, Formatting.Indented);
            File.WriteAllText(filePath, json);
            Debug.Log($"Building counts saved to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving building counts: {e.Message}");
        }
    }

    public void LoadBuildingCounts(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                buildingCounts = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                Debug.Log($"Building counts loaded from: {filePath}");
            }
            else
            {
                Debug.LogWarning($"File not found: {filePath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading building counts: {e.Message}");
        }
    }

}
