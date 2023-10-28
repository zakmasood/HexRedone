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

    public int tileWidth;
    public int tileHeight;

    public GameObject[,] tiles; // Declare a 2D array to store the tiles (BASE)
    public GameObject[,] resources; // 2D array to store the resource tiles (RESOURCES)

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


    [Header("Perlin Noise Generator")]
    public double pHeight;
    public double pWidth;
    public float scaleFactor;
    public int seed;
    public int octaveAmount;
    public float lacuAmount;
    public float persAmount;

    public void Spriglet(GameObject prefab, double X, double height, double Z)
    {
        GameObject instance = Instantiate(prefab, new Vector3((float)X, (float)height, (float)Z), Quaternion.identity);
        instance.transform.Rotate(-90, 90, 0); // Fix 90 degree bug
        instance.transform.position = new Vector3((float)X + Random.Range(-tileWidth, tileWidth), (float)height, (float)Z + Random.Range(-tileHeight, tileHeight));
    }

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


    void Start()
    {
        float[,] noiseMap = GenerateNoiseMap(gridWidth, gridLength, scaleFactor, seed, octaveAmount, persAmount, lacuAmount);
        tiles = new GameObject[gridLength, gridWidth];
        resources = new GameObject[gridLength, gridWidth];

        for (int q = 0; q < gridLength; q++)
        {
            for (int r = 0; r < gridWidth; r++)
            {
                // Base tile generation

                double tileW = tileWidth * 1.732;
                double tileH = tileHeight * 1.5;

                double X = tileH * q;
                double Z = tileW * (r + 0.5 * (q % 2));

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

                /*// Tree generation
                for (int j = 0; j < treeAmount; j++)
                {
                    Spriglet(treePrefab, X, height, Z);
                }*/

                // Grass generation

                for (int j = 0; j < grassAmount; j++)
                {
                    Spriglet(grassPrefab, X, height + 5.2f, Z);
                }

                tiles[q, r] = baseTile;

                /*if (Random.Range(0, 4) == 2)
                {
                    GameObject stoneTile = Instantiate(stoneTilePrefab, new Vector3((float)X, baseTile.transform.position.y + 1f, (float)Z), Quaternion.identity);
                    stoneTile.transform.Rotate(-90, 30, -105); // Fix 90 degree bug

                    int stoneTileID = q * gridWidth + r;
                    stoneTile.name = "StoneTile_" + stoneTileID;

                    resources[q, r] = stoneTile;

                    // Now we remove the grass at that tile

                    //tiles[q, r].transform.Find("GrassOne").gameObject.SetActive(false);
                    //tiles[q, r].transform.Find("GrassTwo").gameObject.SetActive(false);
                    //tiles[q, r].transform.Find("GrassThree").gameObject.SetActive(false);
                }*/
            }
        }
    }
}
