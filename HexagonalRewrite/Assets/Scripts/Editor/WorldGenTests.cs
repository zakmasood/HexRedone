using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

public class WorldGenTests
{
    private WorldGen worldGen;
    private HexUtils mockHexUtils;
    private MockTileUtils mockTileUtils;

    [SetUp]
    public void Setup()
    {
        var gameObject = new GameObject();
        worldGen = gameObject.AddComponent<WorldGen>();

        mockHexUtils = gameObject.AddComponent<MockHexUtils>();
        mockTileUtils = new MockTileUtils(); // Changed to direct instantiation

        worldGen.hexUtils = mockHexUtils;
        worldGen.tileUtils = mockTileUtils;

        worldGen.hexPrefab = new GameObject(); // Mock prefab for tiles
    }

    [Test]
    public void GenerateHexGrid_CreatesCorrectTiles()
    {
        worldGen.rings = 2;
        worldGen.GenerateHexGrid();

        int expectedTileCount = 19; // 1 center tile + 6 ring 1 tiles + 12 ring 2 tiles
        Assert.AreEqual(expectedTileCount, mockTileUtils.tiles.Count);
    }

    [Test]
    public void CreateHexTile_AssignsCorrectResourceType()
    {
        Vector3Int testPosition = new Vector3Int(0, 0, 0);

        worldGen.CreateHexTile(testPosition);

        var tile = mockTileUtils.tiles[0];

        Assert.IsNotNull(tile);
        Assert.IsTrue(tile.resourceType == ResourceType.Stone ||
                      tile.resourceType == ResourceType.Wood ||
                      tile.resourceType == ResourceType.None);
    }

    [Test]
    public void NoiseGenerator_ProducesConsistentValues()
    {
        Vector3 testPosition = new Vector3(10, 20, 30);
        float noise1 = NoiseGenerator.GeneratePerlinNoise(testPosition, 10.0f, 4, 0.5f, 2.0f);
        float noise2 = NoiseGenerator.GeneratePerlinNoise(testPosition, 10.0f, 4, 0.5f, 2.0f);

        Assert.AreEqual(noise1, noise2, "Noise values should be consistent for the same input.");
    }
}

public class MockHexUtils : HexUtils
{
    public new List<Vector3Int> directions = new List<Vector3Int>
    {
        new Vector3Int(1, -1, 0),
        new Vector3Int(-1, 1, 0),
        new Vector3Int(1, 0, -1),
        new Vector3Int(-1, 0, 1),
        new Vector3Int(0, 1, -1),
        new Vector3Int(0, -1, 1)
    };

    public new Vector3 CubeToWorld(Vector3Int cubeCoords)
    {
        return new Vector3(cubeCoords.x, 0, cubeCoords.z); // Simplified for testing
    }
}

public class MockTileUtils : TileUtils
{
    public new Dictionary<int, TileData> tiles = new Dictionary<int, TileData>();
}
