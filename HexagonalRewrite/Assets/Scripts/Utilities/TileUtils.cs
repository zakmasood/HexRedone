using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileUtils : MonoBehaviour
{
    public Dictionary<int, TileData> tiles= new Dictionary<int, TileData>();

    public void AddTile(TileData tile)
    {
        tiles[tile.tileID] = tile;
    }

    public TileData GetTileDataByID(int id)
    {
        tiles.TryGetValue(id, out var tileData);
        return tileData;
    }

    private void DisplayTileInfo(TileData tileData)
    {
        Debug.Log($"Tile ID: {tileData.tileID}, Position: {tileData.tilePosition}, Resource: {tileData.resourceType}");
    }

    public float DistanceAtoB(TileData tileA, TileData tileB)
    {
        // Calculate the cube distance
        float dx = Mathf.Abs(tileA.x - tileB.x);
        float dy = Mathf.Abs(tileA.y - tileB.y);
        float dz = Mathf.Abs(tileA.z - tileB.z);

        Debug.Log($"Tile A: ({tileA.x}, {tileA.y}, {tileA.z})");
        Debug.Log($"Tile B: ({tileB.x}, {tileB.y}, {tileB.z})");
        Debug.Log($"dx: {dx}, dy: {dy}, dz: {dz}");

        return Mathf.Max(dx, dy, dz);
    }


    public TileData GetTileDataFromObject(GameObject tileObject)
    {
        foreach (var tile in tiles.Values)
        {
            if (tile.tileObject == tileObject)
            {
                return tile;
            }
        }

        return null; // No matching tile found
    }

    public void ChangeTileColor(TileData tile, Color color)
    {
        Renderer renderer = tile.tileObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    public Color GetColorForResource(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Wood:
                return new Color(0.55f, 0.27f, 0.07f); // Brown
            case ResourceType.Stone:
                return Color.gray;
            case ResourceType.None:
                return Color.clear; // Transparent for no resource
            default:
                return Color.white; // Default fallback
        }
    }
}
