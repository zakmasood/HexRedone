using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private RecipeLoader recipeLoader;
    [SerializeField] private BuildingData[] availableBuildings;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private TileUtils tileUtils;

    public bool deleteFlag;

    public BuildingData selectedBuilding;

    public void DeleteMode() { deleteFlag = true; }

    public void SelectBuilding(BuildingData building)
    {
        if (resourceManager.CanAfford(building.resourceCosts)) { selectedBuilding = building; }
        else { Debug.LogWarning($"Not enough resources for building: {building.buildingName}"); }
    }

    public void DeleteBuilding()
    {
        if (selectedBuilding != null) { Debug.Log("Cannot Delete! Building is not NULL"); return; } 

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            TileData tile = tileUtils.GetTileDataFromObject(hit.collider.gameObject);

            if (tile == null) { Debug.LogWarning("Invalid tile checked"); return; }
            if (tile.buildingData == null) { Debug.LogWarning("No building on tile!"); return; }

            if (tile.buildingData != null && deleteFlag == true)
            {
                Debug.Log("Deleting building!");

                deleteFlag = false;
                tile.buildingData = null; 

                ResourceType resourceType = tile.resourceType;
                Color resourceColor = tileUtils.GetColorForResource(resourceType); // DEBUG ONLY

                tileUtils.ChangeTileColor(tile, resourceColor); // DEBUG ONLY

                Drill drill = tile.tileObject.GetComponentInChildren<Drill>();
                if (drill != null) { drill.StopCollection(); }

                foreach (Transform child in tile.tileObject.transform) { Destroy(child.gameObject); }
            }
            else { Debug.LogError("Tile buildingData is NULL!"); }
        }
    }

    public void PlaceBuilding(BuildingData building)
    {
        if (selectedBuilding == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Retrieve the tile data from the clicked object
            TileData tile = tileUtils.GetTileDataFromObject(hit.collider.gameObject);

            // Invalid Tile Check
            if (tile == null) { Debug.LogWarning("Invalid tile clicked."); return; }

            // Ensure the tile does not already have a building
            if (tile.buildingData != null) { Debug.Log($"Tile already has a building: {tile.buildingData.buildingName}"); return; }

            // Deduct resources
            if (!resourceManager.CanAfford(selectedBuilding.resourceCosts)) { Debug.Log("Not enough resources to place the building."); return; }

            resourceManager.DeductResources(selectedBuilding.resourceCosts);
            resourceManager.RefreshResourceUI();

            // Place the building at the tile's position
            Vector3 buildingPosition = new Vector3(tile.tilePosition.x, 0.983f, tile.tilePosition.z);
            GameObject placedBuilding = Instantiate(selectedBuilding.buildingPrefab, buildingPosition, Quaternion.identity, tile.tileObject.transform);

            // Update the tile data to reflect the new building
            tile.buildingData = selectedBuilding;

            InitializeBuildingComponent(placedBuilding, tile);

            // Reset the selected building
            selectedBuilding = null;
        }
    }

    private void InitializeBuildingComponent(GameObject placedBuilding, TileData tile)
    {
        switch (selectedBuilding.name)
        {
            case "Drill":
                Drill drill = placedBuilding.AddComponent<Drill>();
                drill.tileData = tile;
                drill.resourceManager = resourceManager;
                break;
            case "Storage":
                Storage storage = placedBuilding.AddComponent<Storage>();
                storage.tileData = tile;
                storage.storageCapacity = selectedBuilding.storageCapacity;
                break;
            case "Sawmill":
                Sawmill sawmill = placedBuilding.AddComponent<Sawmill>();
                sawmill.recipeLoader = recipeLoader;
                break;
            default:
                Debug.LogWarning($"Unknown building type: {selectedBuilding.name}");
                break;
        }
    }
}