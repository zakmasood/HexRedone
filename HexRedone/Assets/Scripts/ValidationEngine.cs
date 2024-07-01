/**
 * @file ValidationEngine.cs
 * @brief Handles the validation of building placement within the game.
 *
 * This script is responsible for loading taxonomy and tile data, and validating if a building can be placed on a specific tile.
 */

using CustomLogger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Logger = CustomLogger.Logger;

/**
 * @class BuildingTaxonomy
 * @brief Represents a taxonomy for buildings, including their required items.
 *
 * This class stores information about the type of building and the resources needed for its construction.
 */
[System.Serializable]
public class BuildingTaxonomy
{
    /// The type of the building.
    public string buildingType;

    /// The list of items needed for the building.
    public List<string> itemNeeded;
}

/**
 * @class ValidationEngine
 * @brief The core validation engine for placing buildings on tiles.
 *
 * This class manages the relationships between buildings and tiles and validates if a building can be placed based on the available resources.
 */
public class ValidationEngine : MonoBehaviour
{
    /// The list of building taxonomies.
    public List<BuildingTaxonomy> buildingTaxonomies;

    /// The list of tiles in the game.
    public List<TileData> tiles;

    /**
     * @brief Loads the building taxonomy data from a JSON file.
     *
     * This method reads a JSON file containing building taxonomy data and stores it in the buildingTaxonomies list.
     */
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

    /**
     * @brief Loads the tile data from a JSON file.
     *
     * This method reads a JSON file containing tile data and stores it in the tiles list.
     */
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

    /**
     * @brief Called when the script instance is being loaded.
     *
     * Initializes the validation engine by loading the building taxonomy data.
     */
    public void Start()
    {
        LoadTaxonomy();
    }

    /**
     * @brief Validates whether a building can be placed on a specific tile.
     *
     * Checks if the required resources for a building are available on the given tile.
     *
     * @param buildingType The type of the building to place.
     * @param tileID The ID of the tile to place the building on.
     * @return True if the building can be placed, false otherwise.
     */
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