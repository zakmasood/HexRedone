using UnityEngine;
using System.Collections.Generic;

public class TileClickHandler : MonoBehaviour
{
    [SerializeField] private TileUtils tileUtils; // Reference to TileManager
    [SerializeField] private ResourceManager resourceManager; // Reference to ResourceManager;
    [SerializeField] private BuildingManager buildingManager; // Reference to ResourceManager;

    private List<TileData> selectedTiles = new List<TileData>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift)) // Left mouse button click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clickedObject = hit.collider.gameObject;
                TileData tileData = tileUtils.GetTileDataFromObject(clickedObject);

                if (tileData != null)
                {
                    if (buildingManager.selectedBuilding != null) { buildingManager.PlaceBuilding(buildingManager.selectedBuilding); }

                    if (buildingManager.selectedBuilding == null && buildingManager.deleteFlag == true)
                    {
                        buildingManager.DeleteBuilding();
                        Debug.Log("Building is being deleted");
                    }

                    if (tileData.resourceType != ResourceType.None) { resourceManager.AddResource(tileData.resourceType, 1); }
                    else { Debug.LogWarning("Tile has no resource type"); }
                }
                else { Debug.LogWarning("Tile does not exist"); }
            }
        }

        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clickedObject = hit.collider.gameObject;
                TileData tileData = tileUtils.GetTileDataFromObject(clickedObject);

                if (tileData != null && tileData.buildingData != null)
                {
                    MonoBehaviour building = clickedObject.GetComponentInChildren<MonoBehaviour>();

                    if (building != null)
                    {
                        FindAnyObjectByType<ConnectionManager>().SelectBuilding(building);
                        Debug.Log($"Selected building: {tileData.buildingData.buildingName} on tile {tileData.tileID}");
                    }
                    else { Debug.LogWarning("No building component found on the selected tile."); }
                }
                else { Debug.LogWarning("No building found on the selected tile."); }
            }
        }
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