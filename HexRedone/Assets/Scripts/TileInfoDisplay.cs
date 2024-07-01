/**
 * @file TileInfoDisplay.cs
 * @brief Displays information about clicked tiles in the game.
 *
 * This script is responsible for detecting mouse clicks on tiles and displaying their information.
 */

using UnityEngine;
using UnityEngine.UI;

/**
 * @class TileInfoDisplay
 * @brief Handles the display of tile information.
 *
 * This class interacts with the game's world generation system and the UI to show details about tiles when they are clicked.
 */
public class TileInfoDisplay : MonoBehaviour
{
    /// The world generation reference.
    public WorldGen worldGen;

    /// The UI text element where tile information will be displayed.
    public Text infoText;

    /// The main camera used for detecting clicks.
    public Camera mainCam;

    /**
     * @brief Detects the tile that was clicked by the player.
     * 
     * Casts a ray from the mouse position to detect if a tile was clicked.
     *
     * @return The GameObject of the clicked tile, or null if no tile was clicked.
     */
    GameObject DetectClickedTile()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            Debug.Log("You selected the " + hit.transform.name); // Ensure you picked the right object
            return hit.transform.gameObject;
        }
        return null;
    }

    /**
     * @brief Called once per frame.
     *
     * Checks for mouse clicks and updates the tile information display accordingly.
     */
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button clicked
        {
            GameObject clickedTile = DetectClickedTile();
            if (clickedTile != null)
            {
                int tileID = ExtractTileID(clickedTile.name);
                Debug.Log("TileID after extraction: " + tileID);
            }
        }
    }

    /**
     * @brief Extracts the tile ID from its name.
     *
     * Assumes the tile name is in the format "tile<ID>" and extracts the integer ID.
     *
     * @param tileName The name of the tile.
     * @return The extracted tile ID as an integer.
     */
    public int ExtractTileID(string tileName)
    {
        // Extract the ID from the tile's name (e.g., "tile62")
        return int.Parse(tileName.Replace("tile", ""));
    }
}