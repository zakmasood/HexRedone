using UnityEngine;

public class WorldPrinter : MonoBehaviour
{
    public WorldGen worldGen;

    public void PrintTileData(int x, int y)
    {
        if (worldGen.tileData != null && x >= 0 && x < worldGen.gridLength && y >= 0 && y < worldGen.gridWidth)
        {
            TileData tile = worldGen.tileData[x, y];
            Debug.Log(string.Format("Tile at ({0}, {1})", tile.x, tile.z));
        }
        else
        {
            Debug.Log("Tile at specified position does not exist.");
        }
    }
}
