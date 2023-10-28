using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Raycasting : MonoBehaviour
{
    public GameObject camera;
    public GameObject building;

    public GameObject mining; // Particle effect

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedTile = hit.collider.gameObject;

                if (clickedTile.CompareTag("Tile")) // Make sure to set the tag on your tiles in the Unity Editor
                {
                    WorldGen worldGen = camera.GetComponent<WorldGen>();

                    // Get neighbors of tile

                    for (int i = 0; i < worldGen.gridLength; i++)
                    {
                        for (int j = 0; j < worldGen.gridWidth; j++)
                        {
                            if (worldGen.tiles[i, j].name == hit.collider.gameObject.name)
                            {
                                // Leveling for multitile buildings
                                /* TODO:
                                 * First grab the height of the clicked tile
                                 * Then set the height of the neighboring tiles to the height of the center tile
                                 */

                                Debug.Log(worldGen.tiles[i, j].name);

                                GameObject centerTile = worldGen.tiles[i, j].gameObject;

                                Debug.Log(centerTile.name);

                                if (i % 2 == 1)
                                {
                                    // Debug.LogWarning("ODD ROW");

                                    GameObject leftTile = worldGen.tiles[i, j + 1].gameObject;
                                    GameObject rightTile = worldGen.tiles[i, j - 1].gameObject;

                                    GameObject topLeftTile = worldGen.tiles[i + 1, j + 1].gameObject; // Top Right Odd
                                    GameObject topRightTile = worldGen.tiles[i + 1, j].gameObject;

                                    GameObject bottomLeftTile = worldGen.tiles[i - 1, j + 1].gameObject;
                                    GameObject bottomRightTile = worldGen.tiles[i - 1, j].gameObject;

                                    topRightTile.transform.position = new Vector3(topRightTile.transform.position.x, centerTile.transform.position.y, topRightTile.transform.position.z);
                                    topLeftTile.transform.position = new Vector3(topLeftTile.transform.position.x, centerTile.transform.position.y, topLeftTile.transform.position.z);

                                    bottomLeftTile.transform.position = new Vector3(bottomLeftTile.transform.position.x, centerTile.transform.position.y, bottomLeftTile.transform.position.z);
                                    bottomRightTile.transform.position = new Vector3(bottomRightTile.transform.position.x, centerTile.transform.position.y, bottomRightTile.transform.position.z);

                                    leftTile.transform.position = new Vector3(leftTile.transform.position.x, centerTile.transform.position.y, leftTile.transform.position.z);
                                    rightTile.transform.position = new Vector3(rightTile.transform.position.x, centerTile.transform.position.y, rightTile.transform.position.z);
                                }
                                else
                                {
                                    // Debug.LogWarning("EVEN ROW");

                                    GameObject leftTile = worldGen.tiles[i, j + 1].gameObject;
                                    GameObject rightTile = worldGen.tiles[i, j - 1].gameObject;

                                    GameObject topLeftTile = worldGen.tiles[i + 1, j].gameObject;
                                    GameObject topRightTile = worldGen.tiles[i + 1, j - 1].gameObject;

                                    GameObject bottomLeftTile = worldGen.tiles[i - 1, j].gameObject;
                                    GameObject bottomRightTile = worldGen.tiles[i - 1, j - 1].gameObject;

                                    topRightTile.transform.position = new Vector3(topRightTile.transform.position.x, centerTile.transform.position.y, topRightTile.transform.position.z);
                                    topLeftTile.transform.position = new Vector3(topLeftTile.transform.position.x, centerTile.transform.position.y, topLeftTile.transform.position.z);

                                    bottomLeftTile.transform.position = new Vector3(bottomLeftTile.transform.position.x, centerTile.transform.position.y, bottomLeftTile.transform.position.z);
                                    bottomRightTile.transform.position = new Vector3(bottomRightTile.transform.position.x, centerTile.transform.position.y, bottomRightTile.transform.position.z);

                                    leftTile.transform.position = new Vector3(leftTile.transform.position.x, centerTile.transform.position.y, leftTile.transform.position.z);
                                    rightTile.transform.position = new Vector3(rightTile.transform.position.x, centerTile.transform.position.y, rightTile.transform.position.z);
                                }
                            }
                        }
                    }
                    // Building Placement

                    /*for (int i = 0; i < worldGen.gridLength; i++)
                    {
                        for (int j = 0; j < worldGen.gridWidth; j++)
                        {
                            if (worldGen.tiles[i, j].name == hit.collider.gameObject.name)
                            {
                                // String to string comparasion, highly inefficient but you find a better method matej.

                                Debug.Log("Found tile with name: " + hit.collider.gameObject.name + " | At Pos: " + i.ToString() + " " + j.ToString());
                                // Building placement 
                                GameObject newBuilding = Instantiate(building, new Vector3((float)hit.collider.gameObject.transform.position.x, (float)hit.collider.gameObject.transform.position.y + 0.28f, (float)hit.collider.gameObject.transform.position.z), Quaternion.identity);
                                newBuilding.transform.Rotate(0, 30, 0); // Fix 90 degree bug
                                worldGen.tiles[i, j].transform.Find("HB").GetComponent<TMP_Text>().text = "YES";
                            }
                        }
                    }*/ 
                }
                else if (clickedTile.CompareTag("StoneTile"))
                {
                    Debug.Log("Stone Clicked!");
                    GameObject pSystem = Instantiate(mining, new Vector3(clickedTile.transform.position.x, clickedTile.transform.position.y, clickedTile.transform.position.z), Quaternion.identity);
                    pSystem.transform.Rotate(-90, 0, 0);
                    pSystem.GetComponent<ParticleSystem>().Play();
                }
            }
        }
    }
}
