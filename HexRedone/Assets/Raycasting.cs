using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Raycasting : MonoBehaviour
{
    public GameObject camera;
    public GameObject building;

    public Text data;

    public GameObject mining; // Particle effect

    GameObject leftTile;
    GameObject rightTile;

    GameObject topLeftTile;
    GameObject topRightTile;

    GameObject bottomLeftTile;
    GameObject bottomRightTile;

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObject = hit.collider.gameObject;
                data.text = clickedObject.name.ToString();
            }
        }

        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedTile = hit.collider.gameObject;

                if (clickedTile.CompareTag("Tile"))
                {
                    WorldGen worldGen = camera.GetComponent<WorldGen>();

                    // Get neighbors of tile

                    for (int i = 0; i < worldGen.gridLength; i++)
                    {
                        for (int j = 0; j < worldGen.gridWidth; j++)
                        {
                            if (worldGen.tiles[i, j].name == hit.collider.gameObject.name)
                            {
                                GameObject centerTile = worldGen.tiles[i, j].gameObject;

                                if (i % 2 == 1)
                                {
                                    // Debug.LogWarning("ODD ROW");

                                    leftTile = worldGen.tiles[i, j + 1].gameObject;
                                    rightTile = worldGen.tiles[i, j - 1].gameObject;

                                    topLeftTile = worldGen.tiles[i + 1, j + 1].gameObject;
                                    topRightTile = worldGen.tiles[i + 1, j].gameObject;

                                    bottomLeftTile = worldGen.tiles[i - 1, j + 1].gameObject;
                                    bottomRightTile = worldGen.tiles[i - 1, j].gameObject;
                                }

                                else
                                {
                                    // Debug.LogWarning("EVEN ROW");

                                    leftTile = worldGen.tiles[i, j + 1].gameObject;
                                    rightTile = worldGen.tiles[i, j - 1].gameObject;

                                    topLeftTile = worldGen.tiles[i + 1, j].gameObject;
                                    topRightTile = worldGen.tiles[i + 1, j - 1].gameObject;

                                    bottomLeftTile = worldGen.tiles[i - 1, j].gameObject;
                                    bottomRightTile = worldGen.tiles[i - 1, j - 1].gameObject;
                                }
                            }
                        }
                    }
                    // Building Placement

                    for (int i = 0; i < worldGen.gridLength; i++)
                    {
                        for (int j = 0; j < worldGen.gridWidth; j++)
                        {
                            if (worldGen.tiles[i, j].name == hit.collider.gameObject.name)
                            {
                                // String to string comparasion, highly inefficient but you find a better method matej.
                                Debug.Log("Found tile with name: " + hit.collider.gameObject.name + " | At Pos: " + i.ToString() + " " + j.ToString());

                                // Building placement 
                                GameObject newBuilding = Instantiate(building, new Vector3((float)hit.collider.gameObject.transform.position.x, (float)hit.collider.gameObject.transform.position.y + 5.05f, (float)hit.collider.gameObject.transform.position.z), Quaternion.identity);
                                newBuilding.transform.Rotate(0, 30, 0); // Fix 90 degree bug
                                worldGen.tiles[i, j].transform.Find("HB").GetComponent<TMP_Text>().text = "YES";
                            }
                        }
                    }
                }
            }
        }
    }
}
