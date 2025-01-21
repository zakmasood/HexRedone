using UnityEngine;
using System.Collections.Generic;

public enum ResourceType
{
    None,
    Stone,
    Wood,
    Plank
}

[System.Serializable]
public class TileData
{
    public int tileID { get; set; }
    public GameObject tileObject { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public Vector3 tilePosition { get; set; }
    public ResourceType resourceType;
    public BuildingData buildingData { get; set; } // Null if no building is placed

    public TileData(int tileID, Vector3 tilePosition, float x, float y, float z, GameObject tileObject)
    {
        this.tileID = tileID;
        this.tilePosition = tilePosition;
        this.x = x;
        this.y = y;
        this.z = z;
        this.tileObject = tileObject;
        this.buildingData = null; // Default to no building
    }
}

public static class NoiseGenerator
{
    public static float GeneratePerlinNoise(Vector3 position, float scale = 1.0f, int octaves = 4, float persistence = 0.5f, float lacunarity = 2.0f, Vector3 offset = default(Vector3))
    {
        if (scale <= 0)
            scale = 0.0001f;

        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (position.x + offset.x) / scale * frequency;
            float sampleY = (position.y + offset.y) / scale * frequency;
            float sampleZ = (position.z + offset.z) / scale * frequency;

            // 2D Perlin Noise, combining two planes to simulate 3D-like noise
            float perlinValueXY = Mathf.PerlinNoise(sampleX, sampleY);
            float perlinValueXZ = Mathf.PerlinNoise(sampleX, sampleZ);
            float perlinValueYZ = Mathf.PerlinNoise(sampleY, sampleZ);
            float combinedPerlinValue = (perlinValueXY + perlinValueXZ + perlinValueYZ) / 3;

            noiseHeight += combinedPerlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return Mathf.Clamp01(noiseHeight); // Ensure value is between 0 and 1
    }
}

public class WorldGen : MonoBehaviour
{
    public GameObject hexPrefab;

    public HexUtils hexUtils;
    public TileUtils tileUtils;

    public Color resourceColor1;
    public Color resourceColor2;

    public float noiseScale = 10.0f;
    public int noiseOctaves = 4;
    public float noisePersistence = 0.5f;
    public float noiseLacunarity = 2.0f;

    public int rings;

    private int nextTileID = 0;

    void Start()
    {
        GenerateHexGrid();
    }

    public void GenerateHexGrid()
    {
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        Vector3Int startPos = new Vector3Int(0, 0, 0);
        queue.Enqueue(startPos);

        while (queue.Count > 0)
        {
            Vector3Int currentPos = queue.Dequeue();

            if (!visited.Contains(currentPos))
            {
                visited.Add(currentPos);
                CreateHexTile(currentPos);

                foreach (Vector3Int direction in hexUtils.directions)
                {
                    Vector3Int neighbor = currentPos + direction;
                    if (!visited.Contains(neighbor) && GetRing(neighbor) <= rings)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }

    public void CreateHexTile(Vector3Int position) // Position arg is CUBE COORDS
    {
        Vector3 worldPos = hexUtils.CubeToWorld(position); // UNITY 3D COORDS

        int tileID = nextTileID++;

        GameObject hexObject = Instantiate(hexPrefab, worldPos, Quaternion.identity);

        hexObject.name = "Tile_" + tileID;

        TileData tileData = new TileData(tileID, worldPos, position.x, position.y, position.z, hexObject);
        // Debug.Log($"Tile Cube Coordinates | X {position.x}, Y {position.y}, Z {position.z}"); // DEBUG ONLY 
        tileData.tilePosition = new Vector3(worldPos.x, worldPos.y, worldPos.z);

        // Customize your noise settings
        
        Vector3 noiseOffset = new Vector3(100, 100, 100); // Optional offset to avoid tiling

        // Generate resource value using customized Perlin noise
        float noiseValue = NoiseGenerator.GeneratePerlinNoise(worldPos, noiseScale, noiseOctaves, noisePersistence, noiseLacunarity, noiseOffset);

        // Assign resources based on noise value
        if (noiseValue > 0.7f)
        {
            // Assign resource type 1
            tileData.resourceType = ResourceType.Stone;
            tileUtils.ChangeTileColor(tileData, Color.gray);
        }
        else if (noiseValue > 0.4f)
        {
            // Assign resource type 2
            tileData.resourceType = ResourceType.Wood;
            tileUtils.ChangeTileColor(tileData, resourceColor2);
        }
        else
        {
            // Default tile without resource
            tileData.resourceType = ResourceType.None;
            tileUtils.ChangeTileColor(tileData, Color.cyan);
        }

        tileUtils.tiles[tileID] = tileData;
    }

    public void RegenerateHexGrid()
    {
        // Clear the existing grid
        foreach (var tile in tileUtils.tiles.Values)
        {
            Destroy(tile.tileObject);
        }

        tileUtils.tiles.Clear();
        nextTileID = 0;

        // Generate a new grid
        GenerateHexGrid();
    }

    int GetRing(Vector3Int pos)
    {
        return (Mathf.Abs(pos.x) + Mathf.Abs(pos.y) + Mathf.Abs(pos.z)) / 2;
    }
}