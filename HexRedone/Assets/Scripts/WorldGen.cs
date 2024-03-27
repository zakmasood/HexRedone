using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;
using Newtonsoft.Json;
using System.IO;

public class WorldGen : MonoBehaviour
{
    public GameObject baseTilePrefab;
    public int gridLength = 5;
    public int gridWidth = 5;
    public int tileWidth;
    public int tileHeight;
    public float heightOffset;
    public GameObject[,] tiles;
    public GameObject[,] resources;
    public GameObject[,] buildings;

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
    string filePath = "Assets/Data.json";

    [Header("Optimization Testing")]
    private float startTime;

    [System.Serializable]
    public class Tile
    {
        public Coordinates Coordinates { get; set; }
        public Resources Resources { get; set; }
        public Building Building { get; set; }
    }

    [System.Serializable]
    public class Coordinates
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    [System.Serializable]
    public class Resources
    {
        public int Oil { get; set; }
        public int Stone { get; set; }
        public int Wood { get; set; }
    }

    [System.Serializable]
    public class Building
    {
        public string Type { get; set; }
        public float Capacity { get; set; }
    }

    public static void AddNewTile(Tile newTile, string filePath, int tileNumber)
    {
        Dictionary<string, Tile> existingTiles;

        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);
            existingTiles = JsonConvert.DeserializeObject<Dictionary<string, Tile>>(jsonString);
        }
        else
        {
            existingTiles = new Dictionary<string, Tile>();
        }

        string tileKey = "Tile_" + tileNumber;
        existingTiles[tileKey] = newTile;

        string updatedJsonString = JsonConvert.SerializeObject(existingTiles, Formatting.Indented);
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
        tiles = new GameObject[gridLength, gridWidth];
        resources = new GameObject[gridLength, gridWidth];

        startTime = Time.realtimeSinceStartup;

        // Cache the rotation quaternion
        Quaternion rotation = Quaternion.Euler(0, 90, 0);

        // Use a single loop instead of nested loops
        int totalTiles = gridLength * gridWidth;
        for (int i = 0; i < totalTiles; i++)
        {
            int q = i % gridLength;
            int r = i / gridLength;

            double X, Z;
            CalculateTilePosition(q, r, out X, out Z);

            GameObject baseTile = Instantiate(baseTilePrefab, new Vector3((float)X, heightOffset, (float)Z), rotation);

            tiles[q, r] = baseTile;
            Tile newTile = new Tile
            {
                Coordinates = new Coordinates { X = (float)X, Y = heightOffset, Z = (float)Z },
                Resources = new Resources { Oil = 0, Stone = 0, Wood = 0 }
            };

            // AddNewTile(newTile, filePath, tileID);
        }

        float endTime = Time.realtimeSinceStartup;
        float elapsedTime = endTime - startTime;
        Debug.Log("World loaded in " + elapsedTime + " seconds");
    }
}