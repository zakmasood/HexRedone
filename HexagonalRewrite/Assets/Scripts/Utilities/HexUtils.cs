using System.Collections.Generic;
using UnityEngine;

public class HexUtils : MonoBehaviour
{
    public float hexSize = 1.732f;
    public WorldGen worldGen;
    public TileUtils tileUtils;

    public Vector3Int[] directions = {
           new Vector3Int(1, -1, 0),
           new Vector3Int(1, 0, -1),
           new Vector3Int(0, 1, -1),
           new Vector3Int(-1, 1, 0),
           new Vector3Int(-1, 0, 1),
           new Vector3Int(0, -1, 1)
     };

    public Vector3 CubeToWorld(Vector3Int cubePos)
    {
        float x = hexSize * (cubePos.x + (cubePos.z / 2f));
        float z = hexSize * (Mathf.Sqrt(3) / 2f * cubePos.z);
        return new Vector3(x, 0, z);
    }

    public Vector3 CubeRound(Vector3 cube)
    {
        int rx = Mathf.RoundToInt(cube.x);
        int ry = Mathf.RoundToInt(cube.y);
        int rz = Mathf.RoundToInt(cube.z);

        float xDiff = Mathf.Abs(rx - cube.x);
        float yDiff = Mathf.Abs(ry - cube.y);
        float zDiff = Mathf.Abs(rz - cube.z);

        if (xDiff > yDiff && xDiff > zDiff)
        {
            rx = -ry - rz;
        }
        else if (yDiff > zDiff)
        {
            ry = -rx - rz;
        }
        else
        {
            rz = -rx - ry;
        }

        return new Vector3(rx, ry, rz);
    }

    public List<Vector3Int> GetLine(TileData a, TileData b)
    {
        List<Vector3Int> result = new List<Vector3Int>();

        Vector3 start = new Vector3(a.x, a.y, a.z);
        Vector3 end = new Vector3(b.x, b.y, b.z);

        int N = Mathf.Max(Mathf.Abs((int)(start.x - end.x)), Mathf.Abs((int)(start.y - end.y)), Mathf.Abs((int)(start.z - end.z)));

        for (int i = 0; i <= N; i++)
        {
            float t = 1.0f * i / N;
            Vector3 cube = CubeRound(Vector3.Lerp(start, end, t));
            result.Add(new Vector3Int((int)cube.x, (int)cube.y, (int)cube.z));
        }

        return result;
    }

    public List<TileData> GetNeighbors(TileData tile)
    {
        List<TileData> neighbors = new List<TileData>();
        foreach (Vector3Int direction in directions)
        {
            Vector3 neighborPos = new Vector3(tile.x + direction.x, tile.y + direction.y, tile.z + direction.z);

            foreach (var kvp in tileUtils.tiles)
            {
                TileData neighborTile = kvp.Value;
                if (neighborTile.x == neighborPos.x && neighborTile.y == neighborPos.y && neighborTile.z == neighborPos.z)
                {
                    neighbors.Add(neighborTile);
                    break;
                }
            }
        }
        return neighbors;
    }
}
