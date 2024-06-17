using CustomLogger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Logger = CustomLogger.Logger;

[System.Serializable]
public class BuildingTaxonomy
{
    public string buildingType;
    public List<string> itemNeeded;
}

public class ValidationEngine : MonoBehaviour
{
    public List<BuildingTaxonomy> buildingTaxonomies;
    public List<TileData> tiles;

    public void LoadTaxonomy()
    {
        // Assuming JSON file contains data that matches the structure of BuildingTaxonomy 
        string filePath = Path.Combine(Application.dataPath, "buildingTaxonomy.json");
        Logger.Log(LogLevel.Info, filePath);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Logger.Log(LogLevel.Info, json);
            buildingTaxonomies = JsonUtility.FromJson<List<BuildingTaxonomy>>(json);
            Logger.Log(LogLevel.Info, buildingTaxonomies.ToString());
        }
        else
        {
            Debug.LogError("Couldn't load taxonomy file");
        }
    }

    public void LoadTiles()
    {
        // Assuming JSON file contains data that matches the structure of TileData
        string filePath = Path.Combine(Application.dataPath, "tiles.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            tiles = JsonUtility.FromJson<List<TileData>>(json);
        }
        else
        {
            Debug.LogError("Couldn't load tiles file");
        }
    }

    public void Start()
    {
        LoadTaxonomy();
    }


    public bool CanPlaceBuilding(string buildingType, int tileID)
    {
        BuildingTaxonomy building = buildingTaxonomies.Find(b => b.buildingType == buildingType);
        TileData tile = tiles.Find(t => t.TileID == tileID);

        if (building == null)
        {
            Debug.LogError("Building type not found");
            return false;
        }

        if (tile == null)
        {
            Debug.LogError("Tile not found");
            return false;
        }

        // Check if the resource needed for the building is available in the tile
        if (building.itemNeeded.Contains(tile.resourceType.ToString()))
        {
            return true;
        }
        else
        {
            Debug.Log("Required resource for building type " + buildingType + " not found on tile " + tileID);
            return false;
        }
    }
}