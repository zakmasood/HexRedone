using System.Collections;
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
    public LayerMask tileLayer;

    public GameObject placementMarkerPrefab;
    private GameObject placementMarker; // Instance

    public void DeleteMode() { deleteFlag = true; }

    public void SelectBuilding(BuildingData building)
    {
        selectedBuilding = building; 
        StartPlacementMarkerLoop();
    }

    private void StartPlacementMarkerLoop()
    {
        if (placementMarker == null) 
        { 
            placementMarker = Instantiate(placementMarkerPrefab); 
            placementMarker.SetActive(false);
        }

        StartCoroutine(PlacementLoop());
    }

    private IEnumerator PlacementLoop()
    {
        while (selectedBuilding != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tileLayer))
            {
                TileData tile = tileUtils.GetTileDataFromObject(hit.collider.gameObject);
                if (tile != null)
                {
                    placementMarker.SetActive(true);
                    placementMarker.transform.position = new Vector3(tile.tilePosition.x, 0.983f, tile.tilePosition.z);

                    if (IsValidPlacement(tile) && resourceManager.CanAfford(selectedBuilding.resourceCosts)) { SetMarkerColor(Color.green); }
                    else { SetMarkerColor(Color.red); }

                    if (Input.GetMouseButtonDown(0) && IsValidPlacement(tile) && resourceManager.CanAfford(selectedBuilding.resourceCosts)) // Left-click to place
                    {
                        PlaceBuilding(selectedBuilding);
                        yield break; // Exit the loop after placing the building
                    }
                }
            }
            else { placementMarker.SetActive(false); }
            yield return null;
        }
        // Cleanup if placement loop ends
        if (placementMarker != null) Destroy(placementMarker);
    }

    private bool IsValidPlacement(TileData tile) { return tile.buildingData == null; } 

    private void SetMarkerColor(Color color)
    {
        Renderer renderer = placementMarker.GetComponent<Renderer>();
        if (renderer != null) { renderer.material.color = color; }
    }

    public void DeleteBuilding()
    {
        if (selectedBuilding != null) { Debug.Log("Cannot Delete! Building is not NULL"); return; } 

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, tileLayer))
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

        if (Physics.Raycast(ray, out RaycastHit hit, tileLayer))
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
                InitializeDrill(placedBuilding, tile);
                break;
            case "Storage":
                InitializeStorage(placedBuilding, tile);
                break;
            case "Sawmill":
                InitializeSawmill(placedBuilding);
                break;
            default:
                Debug.LogWarning($"Unknown building type: {selectedBuilding.name}");
                break;
        }
    }

    private void InitializeDrill(GameObject placedBuilding, TileData tile)
    {
        Drill drill = placedBuilding.AddComponent<Drill>();
        drill.tileData = tile;
        drill.resourceManager = resourceManager;
    }

    private void InitializeStorage(GameObject placedBuilding, TileData tile)
    {
        Storage storage = placedBuilding.AddComponent<Storage>();
        storage.tileData = tile;
        storage.storageCapacity = selectedBuilding.storageCapacity;
    }

    private void InitializeSawmill(GameObject placedBuilding)
    {
        Sawmill sawmill = placedBuilding.AddComponent<Sawmill>();
        sawmill.recipeLoader = recipeLoader;
    }
}