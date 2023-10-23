using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

public class WorldGen : MonoBehaviour
{
    /*  Possiblity to use a custom scripted tool to generate tile prefabs?
     *  EG: Add prefab model, add offset, add tag, and it would autogenerate the data in an array 🤔
     */

    public GameObject baseTilePrefab;
    public GameObject stoneTilePrefab;

    public int gridLength = 5;
    public int gridWidth = 5;

    public double horizontalOffset;
    public double verticalOffset;

    public GameObject[,] tiles; // Declare a 2D array to store the tiles (BASE)
    public GameObject[,] resources; // 2D array to store the resource tiles (RESOURCES)

    /*  With Resources array, no need to declare multiple, since we can use tag
     *  comparisons to determine what resources are at the given location.
     *  This also means we save on runtime performance by not using multiple array calls
     *  and declarations.
     */

    [Header("Perlin Noise Generator")]
    public double pHeight;
    public double pWidth;

    public Vector3 spriglet(float maxX, float maxZ, float height) // Randomizes pos
    {
        return new Vector3(maxX + Random.Range(-0.5f, 0.5f), height + 1.2f, maxZ + Random.Range(-0.5f, 0.5f));
    }

    public float[,] GenerateNoiseMap(int width, int height, float scale)
    {
        float[,] noiseMap = new float[width, height];

        // Loop through each point in the grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Calculate sample coordinates based on scale and position
                float sampleX = (float)x / width * scale;
                float sampleY = (float)y / height * scale;

                // Generate Perlin noise value at the sample coordinates
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);

                // Store the noise value in the noise map
                noiseMap[x, y] = perlinValue;
            }
        }

        return noiseMap;
    }

    void Start()
    {
        float[,] noiseMap = GenerateNoiseMap(gridWidth, gridLength, 5f);
        tiles = new GameObject[gridLength, gridWidth];
        resources = new GameObject[gridLength, gridWidth];

        for (int q = 0; q < gridLength; q++)
        {
            for (int r = 0; r < gridWidth; r++)
            {
                double X = 1.5 * q;
                double Z = 1.732 * (r + 0.5 * (q % 2));

                // Grab perlin point
                float perlinValue = noiseMap[q, r];
                float height = perlinValue * 7 + 5;

                GameObject baseTile = Instantiate(baseTilePrefab, new Vector3((float)X, height, (float)Z), Quaternion.identity);

                baseTile.transform.Find("Coords").GetComponent<TMP_Text>().text = q.ToString() + ", " + r.ToString();
                baseTile.transform.Rotate(0, 90, 0); // Fix 90 degree bug

                int tileID = q * gridWidth + r;
                baseTile.name = "Tile_" + tileID;

                baseTile.transform.Find("TileID").GetComponent<TMP_Text>().text = baseTile.name;

                float baseTileX = baseTile.transform.position.x;
                float baseTileZ = baseTile.transform.position.z;

                // Grass position randomization
                baseTile.transform.Find("GrassOne").gameObject.transform.position = new Vector3(baseTileX + Random.Range(-0.5f, 0.5f), height + 1.2f , baseTileZ + Random.Range(-0.5f, 0.5f));
                baseTile.transform.Find("GrassTwo").gameObject.transform.position = new Vector3(baseTileX + Random.Range(-0.5f, 0.5f), height + 1.2f , baseTileZ + Random.Range(-0.5f, 0.5f));
                baseTile.transform.Find("GrassThree").gameObject.transform.position = new Vector3(baseTileX + Random.Range(-0.5f, 0.5f), height + 1.2f , baseTileZ + Random.Range(-0.5f, 0.5f));

                tiles[q, r] = baseTile;

                if (Random.Range(0, 4) == 2)
                {
                    GameObject stoneTile = Instantiate(stoneTilePrefab, new Vector3((float)X, baseTile.transform.position.y + 1f, (float)Z), Quaternion.identity);
                    stoneTile.transform.Rotate(-90, 30, -105); // Fix 90 degree bug

                    int stoneTileID = q * gridWidth + r;
                    stoneTile.name = "StoneTile_" + stoneTileID;

                    resources[q, r] = stoneTile;

                    // Now we remove the grass at that tile

                    tiles[q, r].transform.Find("GrassOne").gameObject.SetActive(false);
                    tiles[q, r].transform.Find("GrassTwo").gameObject.SetActive(false);
                    tiles[q, r].transform.Find("GrassThree").gameObject.SetActive(false);
                }
            }
        }
    }
}
