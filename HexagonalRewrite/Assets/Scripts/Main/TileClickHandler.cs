using UnityEngine;
using System.Collections.Generic;

public class TileClickHandler : MonoBehaviour
{
    [SerializeField] private TileUtils tileUtils; // Reference to TileManager
    [SerializeField] private ResourceManager resourceManager; // Reference to ResourceManager;
    [SerializeField] private BuildingManager buildingManager; // Reference to ResourceManager;

    public LayerMask tileLayer;

    private List<TileData> selectedTiles = new List<TileData>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, tileLayer))
            {
                GameObject clickedObject = hit.collider.gameObject;
                TileData tileData = tileUtils.GetTileDataFromObject(clickedObject);

                if (tileData != null)
                {
                    if (!Input.GetKey(KeyCode.LeftShift)) { HandleLeftClick(tileData); }
                    else { HandleShiftLeftClick(tileData, clickedObject); }
                }
                else { Debug.LogWarning("Tile does not exist"); }
            }
        }
    }

    private void HandleLeftClick(TileData tileData)
    {
        // Place building
        if (buildingManager.selectedBuilding != null) { buildingManager.PlaceBuilding(buildingManager.selectedBuilding); }
        // Delete building
        if (buildingManager.selectedBuilding == null && buildingManager.deleteFlag) { buildingManager.DeleteBuilding(); }
        // Collect resource on click
        if (tileData.resourceType != ResourceType.None) { resourceManager.AddResource(tileData.resourceType, 1); }

        else { Debug.LogWarning("Tile has no resource type"); }
    }

    private void HandleShiftLeftClick(TileData tileData, GameObject clickedObject)
    {
        if (tileData.buildingData != null)
        {
            MonoBehaviour building = clickedObject.GetComponentInChildren<MonoBehaviour>();

            if (building != null)
            {
                FindAnyObjectByType<ConnectionManager>().SelectBuilding(building);
                Debug.Log($"Selected building: {tileData.buildingData.buildingName} on tile {tileData.tileID}");
            }
        }
        else { Debug.LogWarning("No building found on the selected tile."); }
    }

    private void SelectTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            TileData tile = tileUtils.GetTileDataFromObject(hit.collider.gameObject);

            if (tile != null && !selectedTiles.Contains(tile)) { selectedTiles.Add(tile); tileUtils.ChangeTileColor(tile, Color.green); }
        }
    }

    private void CalculateDistance()
    {
        if (selectedTiles.Count == 2)
        {
            float distance = tileUtils.DistanceAtoB(selectedTiles[0], selectedTiles[1]);
            Debug.Log($"Distance between Tile {selectedTiles[0].tileID} and Tile {selectedTiles[1].tileID}: {distance}");
        }
    }

    private void ResetSelection() { selectedTiles.Clear(); Debug.Log("Selection reset."); }
}