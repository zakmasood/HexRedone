using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;
using CustomLogger;
using Logger = CustomLogger.Logger;
using System.Threading.Tasks;

/// @brief Provides extension methods for strings. 

public static class StringExtension
{
    /**
     * @brief Makes the string bold.
     * @param str The input string.
     * @return The bolded string.
     */
    public static string Bold(this string str) => "<b>" + str + "</b>";

    /**
     * @brief Colors the string with the specified color.
     * @param str The input string.
     * @param clr The color to apply.
     * @return The colored string.
     */
    public static string Color(this string str, string clr) => string.Format("<color={0}>{1}</color>", clr, str);

    /**
     * @brief Italicizes the string.
     * @param str The input string.
     * @return The italicized string.
     */
    public static string Italic(this string str) => "<i>" + str + "</i>";

    /**
     * @brief Sets the size of the string.
     * @param str The input string.
     * @param size The size to apply.
     * @return The resized string.
     */
    public static string Size(this string str, int size) => string.Format("<size={0}>{1}</size>", size, str);
}


/// @brief Represents the building data.
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

/// @brief Represents the list of buildings.
public class BuildingsList
{
    [JsonProperty("buildings")]
    public List<BuildingData> Buildings { get; set; }
}

/// @brief Controls the player actions and interactions in the game world.
public class PlayerController : MonoBehaviour
{
    /// The building type to build.
    public string buildingToBuild;

    /// The world generator instance.
    public WorldGen worldGen;

    /// The Factory Manager Instance.
    public FactoryManager factoryManager;

    /// The info text UI element.
    public Text infoText;

    /// The warning text UI element.
    public GameObject WarningText;

    /// The start position of the warning text.
    public Vector3 WTStartPos;

    /// The end position of the warning text.
    public Vector3 WTEndPos;

    /// The time duration for warning text animation.
    public float WTime;

    /// The container for labels.
    public Transform labelsContainer;

    /// The label prefab.
    public GameObject labelPrefab;

    /// The building placeholder.
    public GameObject buildingPlaceholder;

    private GameObject clickedTile;

    public Dictionary<string, int> buildingCounts = new Dictionary<string, int>();
    private Dictionary<string, Text> buildingTexts = new Dictionary<string, Text>();
    [Header("Matej´s Gamefeel things")]
    public GameObject effect;
    public Transform camTransform;
	
	// How long the object should shake for.
	private float shakeDuration = 0f;
	
	// Amplitude of the shake. A larger value shakes the camera harder.
	public float shakeAmount = 0.7f;
	public float decreaseFactor = 1.0f;
    public float shakeTime;
	
	//Vector3 originalPos;
    
    public Vector3 nextMove;
    public float AdjustSpeed;

    public void Awake()
    {
        //originalPos = camTransform.localPosition;
        
        nextMove = transform.position;
        Logger.Log(LogLevel.Error, Application.streamingAssetsPath);

        string pathToAssets = Application.dataPath + "/StreamingAssets";

        string[] assetFiles = Directory.GetFiles(pathToAssets);

        foreach (string filePath in assetFiles)
        {
            Debug.Log("Found asset: " + Path.GetFileName(filePath));
        }
    }

    /**
    * @brief Called every frame, handles player input.
    */
    private void Update()
    {
        HandleInput();
        //transform.position = nextMove;
        transform.position = Vector3.Lerp(transform.position, nextMove, Time.deltaTime * AdjustSpeed);
        if(Input.GetKey(KeyCode.W)) nextMove.x += 0.05f;
        if(Input.GetKey(KeyCode.A)) nextMove.z += 0.05f;
        if(Input.GetKey(KeyCode.S)) nextMove.x -= 0.05f; 
        if(Input.GetKey(KeyCode.D)) nextMove.z -= 0.05f; 

        if (shakeDuration > 0)
		{
			camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, nextMove + Random.insideUnitSphere * shakeAmount, Time.deltaTime);
			
			shakeDuration -= Time.deltaTime * decreaseFactor;
		}
		else
		{
			shakeDuration = 0f;
			//camTransform.localPosition = originalPos;
		}
    }

    /**
    * @brief Detects the clicked tile by the player.
    * @return The clicked tile game object.
    */
    private GameObject DetectClickedObject()
    {
        /*RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {*/
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100.0f))
        {
            GameObject clickedObject = hit.transform.gameObject;

            return clickedObject;
        }
        return null;
    }

    /**
    * @brief Handles player input for placing buildings.
    */
    public void HandleInput()
    {
        // Return early if the left mouse button is not clicked
        if (!Input.GetMouseButtonDown(0)) return;

        /*RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {*/
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100.0f))
        {
            GameObject clickedObject = hit.transform.gameObject;

            // Shift + left-click detected, get the associated TileData
            int factoryID = worldGen.ExtractTileID(clickedObject.name);
            TileData tileData = worldGen.GetTileData(factoryID);

            // Check and log based on the tag
            if (clickedObject.CompareTag("Factory"))
            {
                bool isFactoryTaskComplete = factoryManager.IsFactoryTaskComplete(factoryID);
                Logger.Log(LogLevel.Warning, isFactoryTaskComplete.ToString());

                // Check if either Shift key is pressed and the factory's task is complete
                if (isFactoryTaskComplete && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    if (tileData != null)
                    {
                        factoryManager.DeleteFactory(factoryID);

                        buildingCounts[buildingToBuild]--;

                        Destroy(clickedObject);

                        // Reset the resource type to the previous one
                        tileData.ResetToPreviousResourceType();
                        Logger.Log(LogLevel.Info, $"Resource type for Tile ID {factoryID} reset to {tileData.resourceType}");
                    }
                    // Update the building counts
                    if (buildingCounts.ContainsKey(buildingToBuild))
                    {
                        Logger.Log(LogLevel.Info, $"Factory at position {clickedObject.transform.position} has been deleted.");
                        Logger.Log(LogLevel.Success, "Deleted " + buildingToBuild + ". Total: " + buildingCounts[buildingToBuild].ToString());
                    }

                    return;
                }
                // Check if the factory's task is complete and Shift key is not pressed
                else if (isFactoryTaskComplete)
                {
                    Logger.Log(LogLevel.Info, $"Factory clicked at position {clickedObject.transform.position}");
                    Logger.Log(LogLevel.Info, "Restarting Factory!");

                    factoryManager.RestartFactory(factoryID);

                    return;
                }
            }
            else if (clickedObject.CompareTag("Tile"))
            {
                Logger.Log(LogLevel.Info, $"Tile clicked with ID: {clickedObject.name}");
            }

            // Extract tile ID from the clicked tile's name
            int tileID = worldGen.ExtractTileID(clickedObject.name);
            // Retrieve tile data using the tile ID
            TileData data = worldGen.GetTileData(tileID);

            if (data != null)
            {
                // Update info text with tile details
                infoText.text = $"Tile ID: {tileID}, X: {data.x}, Z: {data.z}, Element: {data.resourceType}";
            }
            else
            {
                // Log a warning if tile data is not found
                Logger.Log(LogLevel.Warning, $"TileData not found for tileID: {tileID}");
                return;
            }

            // Get a list of buildable buildings for the clicked tile
            List<string> buildableBuildings = GetBuildableBuildings(tileID);
            // Log a warning if no buildable buildings are available for this tile
            if (buildableBuildings.Count == 0)
            {
                Logger.Log(LogLevel.Warning, "No buildable buildings for this tile");
                return;
            }

            // Select the first buildable building from the list
            buildingToBuild = buildableBuildings.First();

            // Check if the selected building can be placed on the tile
            if (CanPlaceBuilding(tileID, buildingToBuild))
            {
                // Instantiate the building placeholder at the clicked tile's position
                InstantiateBuildingPlaceholder(clickedObject.transform.position, tileID);
                // Update the count of the built buildings
                UpdateBuildingCounts(buildingToBuild, 1);
                shakeDuration += shakeTime;
                Instantiate(effect, hit.transform.position, Quaternion.identity);
            }
        }
    }


    /**
    * @brief Gets the list of buildable buildings for a given tile.
    * @param tileID The ID of the tile.
    * @return A list of buildable building types.
    */
    private List<string> GetBuildableBuildings(int tileID)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "buildingTaxonomy.json");
        Logger.Log(LogLevel.Warning, "Filepath: " + path);

        if (!File.Exists(path))
        {
            Logger.Log(LogLevel.Error, "Json file not found at " + path);
            return new List<string>();
        }

        string json = File.ReadAllText(path);
        BuildingsList buildings = JsonConvert.DeserializeObject<BuildingsList>(json);

        if (buildings == null || buildings.Buildings == null || buildings.Buildings.Count == 0)
        {
            Logger.Log(LogLevel.Warning, "No building data is available from the JSON file.");
            return new List<string>();
        }

        TileData data = worldGen.GetTileData(tileID);

        if (data == null)
        {
            Logger.Log(LogLevel.Error, "TileData is null for tile ID " + tileID);
            return new List<string>();
        }

        string resourceType = data.resourceType;

        // Check if the resourceType is a building type
        if (buildings.Buildings.Any(b => b.BuildingType == resourceType))
        {
            Logger.Log(LogLevel.Warning, "Cannot place building. Tile already has a building of type: " + resourceType);
            WarningText.GetComponent<Text>().text = "Cannot Place Building On Tile! Building Already Exists!";
            LeanTween.moveLocal(WarningText, WTEndPos, WTime).setEaseInOutCubic();
            LeanTween.delayedCall(5f, () => { resetTweenedObjects(WTStartPos, WarningText); });
            return new List<string>();
        }
        else
        {
            ;
        }

        Logger.Log(LogLevel.Info, "Resource type on tile " + tileID + ": " + resourceType);

        List<string> buildingTypes = buildings.Buildings
            .Where(building => building.ItemNeeded.Contains(resourceType))
            .Select(building => building.BuildingType)
            .ToList();

        if (buildingTypes.Count == 0)
        {
            Logger.Log(LogLevel.Info, "No buildings can be built on this tile with element type " + resourceType);
            WarningText.GetComponent<Text>().text = "No buildings can be built on this tile!";
            LeanTween.moveLocal(WarningText, WTEndPos, WTime).setEaseInOutCubic();
            LeanTween.delayedCall(5f, () => { resetTweenedObjects(WTStartPos, WarningText); });
        }

        return buildingTypes;
    }

    /**
    * @brief Resets the position of tweened objects.
    * @param StartPos The start position.
    * @param gameObject The game object to reset.
    */
    public void resetTweenedObjects(Vector3 StartPos, GameObject gameObject)
    {
        LeanTween.moveLocal(gameObject, StartPos, 1f).setEaseInOutCubic();
    }

    /**
    * @brief Gets the name of the clicked tile.
    * @return The name of the clicked tile.
    */
    public string clickedTileData()
    {
        clickedTile = DetectClickedObject();
        return clickedTile.name;
    }

    /**
    * @brief Gets the list of buildings that can be built on a given tile.
    * @param tileID The ID of the tile.
    * @return A list of buildable building types.
    */


    /**
    * @brief Determines whether a building can be placed on a tile.
    * @param tileID The ID of the tile.
    * @param buildingType The building type to place.
    * @return A boolean indicating whether the building can be placed.
    */
    public bool CanPlaceBuilding(int tileID, string buildingType)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "buildingTaxonomy.json");
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
            Logger.Log(LogLevel.Error, "Cannot place building. Tile already has a building.");
            return false;
        }

        if (!itemsNeeded.All(item => resourceTypeList.Contains(item)))
        {
            Logger.Log(LogLevel.Error, "Cannot place building. Required item not in tile elements.");
            return false;
        }

        if (!itemsNeeded.Contains(data.resourceType))
        {
            Logger.Log(LogLevel.Error, $"Cannot place building. Tile does not contain required element for building type {buildingType}.");
            UpdateBuildingCounts(buildingToBuild, 0);
            return false;
        }

        if (data.x >= 0 && data.x < worldGen.tileData.GetLength(0) && data.z >= 0 && data.z < worldGen.tileData.GetLength(1))
        {
            worldGen.tileData[data.x, data.z].resourceType = data.resourceType;
            Logger.Log(LogLevel.Debug, "Placing tile");
            factoryManager.AddFactory(tileID, data.resourceType);
            worldGen.SetTileResourceType(tileID, buildingType);
        }

        Logger.Log(LogLevel.Success, "Can place building type " + buildingType + " on tile " + tileID + ".");
        return true;
    }

    /**
    * @brief Gets the list of resource types.
    * @return A list of resource types.
    */
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

    /**
    * @brief Instantiates a building placeholder at the specified position.
    * @param position The position to place the building placeholder.
    */
    private void InstantiateBuildingPlaceholder(Vector3 position, int tileID)
    {
        // Instantiate the building placeholder at the specified position
        GameObject placeholder = Instantiate(buildingPlaceholder, new Vector3(position.x, 1, position.z), Quaternion.identity);

        // Set the name of the placeholder to the tileID + "Factory"
        placeholder.name = $"Factory{tileID}";
    }

    /** 
    * @brief Updates the count of buildings of the specified type.
    * @param buildingToBuild The building type to update the count for.
    */
    private void UpdateBuildingCounts(string buildingToBuild, int amount)
    {
        if (buildingCounts.ContainsKey(buildingToBuild))
        {
            buildingCounts[buildingToBuild] += amount;
        }
        else
        {
            buildingCounts[buildingToBuild] = amount;
        }

        Logger.Log(LogLevel.Success, $"Built {buildingToBuild}. Total: {buildingCounts[buildingToBuild]}");

        if (buildingTexts.ContainsKey(buildingToBuild))
        {
            buildingTexts[buildingToBuild].text = $"{buildingToBuild}: {buildingCounts[buildingToBuild]}";
        }
        else
        {
            GameObject newLabel = Instantiate(labelPrefab, labelsContainer);
            Text newLabelText = newLabel.GetComponent<Text>();
            newLabelText.text = $"{buildingToBuild}: {buildingCounts[buildingToBuild]}";

            buildingTexts.Add(buildingToBuild, newLabelText);
        }

        // Debugging: Print all building counts
        DebugBuildingCounts();
    }

    public void FixedUpdate()
    {
        UpdateBuildingCounts(buildingToBuild, 0); // Update every lateFrame to prevent issues
    }

    private void DebugBuildingCounts()
    {
        Logger.Log(LogLevel.Debug, "Current Building Counts:");
        foreach (var building in buildingCounts)
        {
            Logger.Log(LogLevel.Debug, $"{building.Key}: {building.Value}");
        }
    }

    /**
    * @brief Saves the world data to a file.
    */
    private void SaveWorldData()
    {
        worldGen.SaveWorldData("tileData.json");
        Logger.Log(LogLevel.Success, "Tile data saved to " + Path.Combine(Application.dataPath, "tileData.json"));
    }

    /**
    * @brief Deletes the existing world grid.
    */
    private void DeleteGrid()
    {
        worldGen.DeleteGrid();
        Logger.Log(LogLevel.Debug, "Deleting World");
    }
}