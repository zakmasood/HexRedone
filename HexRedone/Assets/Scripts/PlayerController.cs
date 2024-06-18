﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;

public static class StringExtension
{
    public static string Bold(this string str) => "<b>" + str + "</b>";
    public static string Color(this string str, string clr) => string.Format("<color={0}>{1}</color>", clr, str);
    public static string Italic(this string str) => "<i>" + str + "</i>";
    public static string Size(this string str, int size) => string.Format("<size={0}>{1}</size>", size, str);
}

public class BuildingData
{
    [JsonProperty("building-type")]
    public string BuildingType { get; set; }

    [JsonProperty("item-needed")]
    public List<string> ItemNeeded { get; set; }

    [JsonProperty("icon")]
    public string Icon { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; }
}

public class BuildingsList
{
    [JsonProperty("buildings")]
    public List<BuildingData> Buildings { get; set; }
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

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
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

                List<string> buildableBuildings = GetBuildableBuildings(tileID);

                if (buildableBuildings.Count == 0)
                {
                    Debug.Log("No buildable buildings for this tile");
                }

                string buildingToBuild = buildableBuildings[0];

                if (CanPlaceBuilding(tileID, buildingToBuild))
                {
                    InstantiateBuildingPlaceholder(clickedTile.transform.position);
                    UpdateBuildingCounts(buildingToBuild);
                }
            }
        }
    }

    private List<string> GetBuildableBuildings(int tileID)
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

        TileData data = worldGen.GetTileData(tileID);

        if (data == null)
        {
            Debug.LogError("TileData is null for tile ID " + tileID);
            return new List<string>();
        }

        string resourceType = data.resourceType;
        Debug.Log("Resource type on tile " + tileID + ": " + resourceType);

        List<string> buildingTypes = buildings.Buildings
            .Where(building => building.ItemNeeded.Contains(resourceType))
            .Select(building => building.BuildingType)
            .ToList();

        if (buildingTypes.Count == 0)
        {
            Debug.Log("No buildings can be built on this tile with element type " + resourceType);
        }

        return buildingTypes;
    }

    private GameObject DetectClickedTile()
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
            if (building.ItemNeeded.Contains(resourceType))
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
        if (questController.hasDistinctValues(2, buildingCounts))
        {
            print("You win!!".Color("lime"));
        }
    }

    private bool CanPlaceBuilding(int tileID, string buildingType)
    {
        string path = Application.dataPath + "/buildingTaxonomy.json";
        string json = File.ReadAllText(path);

        BuildingsList buildings = JsonConvert.DeserializeObject<BuildingsList>(json);

        TileData data = worldGen.GetTileData(tileID);

        List<string> itemsNeeded = buildings.Buildings
            .Where(building => building.BuildingType == buildingType)
            .SelectMany(building => building.ItemNeeded)
            .ToList();

        List<string> resourceTypeList = GetResourceTypeList();

        if (buildings.Buildings.Any(b => b.BuildingType == data.resourceType))
        {
            Debug.Log("Cannot place building. Tile already has a building.");
            return false;
        }

        if (!itemsNeeded.All(item => resourceTypeList.Contains(item)))
        {
            Debug.Log("Cannot place building. Required item not in tile elements.");
            return false;
        }

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

    private List<string> GetResourceTypeList()
    {
        return new List<string>
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
    }

    private void InstantiateBuildingPlaceholder(Vector3 position)
    {
        Instantiate(buildingPlaceholder, new Vector3(position.x, 3, position.z), Quaternion.identity);
    }

    private void UpdateBuildingCounts(string buildingToBuild)
    {
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
            GameObject newLabel = Instantiate(labelPrefab, new Vector2(labelsContainer.transform.position.x, labelsContainer.transform.position.y), Quaternion.identity, labelsContainer);
            Text newLabelText = newLabel.GetComponent<Text>();

            newLabelText.text = buildingToBuild + ": " + buildingCounts[buildingToBuild];

            buildingTexts.Add(buildingToBuild, newLabelText);
        }
    }

    private void SaveWorldData()
    {
        worldGen.SaveWorldData("tileData.json");
        Debug.Log("Tile data saved to " + Path.Combine(Application.dataPath, "tileData.json"));
    }

    private void DeleteGrid()
    {
        worldGen.DeleteGrid();
        Debug.Log("Deleting World");
    }
}