using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;
using Newtonsoft.Json;
using System.IO;

public class WorldGen : MonoBehaviour
{
    /*  Possiblity to use a custom scripted tool to generate tile prefabs?
     *  EG: Add prefab model, add offset, add tag, and it would autogenerate the data in an array 🤔
     */

    public GameObject baseTilePrefab;

    public int gridLength = 5;
    public int gridWidth = 5;

    public int tileWidth;
    public int tileHeight;

    public float heightOffset;

    public GameObject[,] tiles; // Declare a 2D array to store the tiles (BASE)
    public GameObject[,] resources; // 2D array to store the resource tiles (RESOURCES)
    public GameObject[,] buildings;

    /*  With Resources array, no need to declare multiple, since we can use tag
     *  comparisons to determine what resources are at the given location.
     *  This also means we save on runtime performance by not using multiple array calls
     *  and declarations.
     */

    [Header("Spriglets")]
    public GameObject grassPrefab;
    public float grassAmount;

    [Header("Resources")]
    public GameObject treePrefab;
    public float treeAmount;
    public GameObject stonePrefab;
    public int stoneChance;

    [Header("Perlin Noise Generator")]
    public double pHeight;
    public double pWidth;
    public float scaleFactor;
    public int seed;
    public int octaveAmount;
    public float lacuAmount;
    public float persAmount;

    [Header("TileData")]
    string filePath = "Assets/Data.json"; // Specify the path to Data

    [Header("Optimization Testing")]
    private float startTime;

    public class Tile
    {
        public Coordinates Coordinates { get; set; }
        public Resources Resources { get; set; }
        public Building Building { get; set; }
    }

    public class Coordinates
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class Resources
    {
        public int Oil { get; set; }
        public int Stone { get; set; }
        public int Wood { get; set; }
    }

    public class Building
    {
        public string Type { get; set; }
        public float Capacity { get; set; }
    }

    public void Spriglet(GameObject prefab, double X, double height, double Z)
    {
        GameObject instance = Instantiate(prefab, new Vector3((float)X, (float)height, (float)Z), Quaternion.identity);
        instance.transform.Rotate(-90, 90, 0); // Fix 90 degree bug
        instance.transform.position = new Vector3((float)X + Random.Range(-tileWidth, tileWidth), (float)height, (float)Z + Random.Range(-tileHeight, tileHeight));
    }

    // Noise map generation and World creation

    public float[,] GenerateNoiseMap(int width, int height, float scale, int seed, int octaves, float persistence, float lacunarity)
    {
        float[,] noiseMap = new float[width, height];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }

    public static void AddNewTile(Tile newTile, string filePath, int tileNumber)
    {
        // Notes ( Needs Following Edgecase ):
        // If file does not have any data but exists, then error will occur

        Dictionary<string, Tile> existingTiles; // Define dict for existing tiles

        if (File.Exists(filePath)) // Check if the file exists
        {
            string jsonString = File.ReadAllText(filePath); // Read existing data

            // Deserialize existing data into a dictionary
            existingTiles = JsonConvert.DeserializeObject<Dictionary<string, Tile>>(jsonString);
        }
        else // Edgecase for if file does not exist
        {
            existingTiles = new Dictionary<string, Tile>(); // If the file doesn't exist, create a new dictionary
        }

        // Add the new tile to the dictionary with its corresponding tileID
        string tileKey = "Tile_" + tileNumber;
        existingTiles[tileKey] = newTile; // Add to dict with its data

        // Serialize the updated dictionary of tiles back to a JSON string
        string updatedJsonString = JsonConvert.SerializeObject(existingTiles, Formatting.Indented);

        // Write the updated JSON string back to the file
        File.WriteAllText(filePath, updatedJsonString);
    }

    void CalculateTilePosition(int q, int r, out double X, out double Z)
    {
        double tileW = tileWidth * 1.732;
        Z = tileW * (r + 0.5 * (q % 2));

        double tileH = tileHeight * 1.5;
        X = tileH * q;
    }

    void Start()
    {
        //float[,] noiseMap = GenerateNoiseMap(gridWidth, gridLength, scaleFactor, seed, octaveAmount, persAmount, lacuAmount);
        tiles = new GameObject[gridLength, gridWidth];
        resources = new GameObject[gridLength, gridWidth];

        // Optimization Testing
        // Record the start time when the script starts
        startTime = Time.realtimeSinceStartup;

        // Instantiation of tiles ( Put all instantiate - needed objects here ) ( Needs more optimization )

        for (int q = 0; q < gridLength; q++)
        {
            for (int r = 0; r < gridWidth; r++)
            {
                // Grab perlin point

                //float perlinValue = noiseMap[q, r];
                //float height = perlinValue * 7 + 5;

                double X, Z;
                CalculateTilePosition(q, r, out X, out Z);

                /*if (Random.Range(0, 5) == stoneChance)
                {
                    // Instantiate stone tile
                    GameObject stoneTile = Instantiate(stonePrefab, new Vector3((float)X, heightOffset + 1, (float)Z), Quaternion.identity);
                    stoneTile.transform.Rotate(-90, -15, 0); // Fix 90 degree bug
                    // TODO:
                    // Define purity
                    // Serialize to file
                    Debug.Log("Instantiate stone tile");
                }*/

                GameObject baseTile = Instantiate(baseTilePrefab, new Vector3((float)X, heightOffset, (float)Z), Quaternion.identity);

                Transform tileTransform = baseTile.transform;
                Transform coordsTransform = tileTransform.Find("Coords");

                tileTransform.Rotate(0, 90, 0); // Fix 90 degree bug

                coordsTransform.GetComponent<TMP_Text>().text = q.ToString() + ", " + r.ToString();

                int tileID = q * gridWidth + r;
                baseTile.name = "Tile_" + tileID;

                baseTile.transform.Find("TileID").GetComponent<TMP_Text>().text = baseTile.name;

                tiles[q, r] = baseTile;

                Tile newTile = new Tile
                {
                    Coordinates = new Coordinates { X = (float)X, Y = heightOffset, Z = (float)Z },
                    Resources = new Resources { Oil = 0, Stone = 0, Wood = 0 }
                };

                AddNewTile(newTile, filePath, tileID);
            }
        }

        // Record the end time after world has been generated
        float endTime = Time.realtimeSinceStartup;

        // Calculate and log elapsed time
        float elapsedTime = endTime - startTime;
        Debug.Log("World loaded in " + elapsedTime + " seconds");
    }
}
