using UnityEngine;
using UnityEngine.UI;

public class TileInfoDisplay : MonoBehaviour
{
    public WorldGen worldGen;
    public Text infoText;
    public Camera mainCam;

    GameObject DetectClickedTile()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            Debug.Log("You selected the " + hit.transform.name); // ensure you picked right object
            return hit.transform.gameObject;
        }
        return null;
    }

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

    public int ExtractTileID(string tileName)
    {
        // Extract the ID from the tile's name (e.g., "tile62")
        return int.Parse(tileName.Replace("tile", ""));
    }
}
