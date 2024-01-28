using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillController : MonoBehaviour
{
    public Vector3 drillPoint;
    public GameObject camera;
    public GameObject[,] tiles;
    public int Xtilepos;
    public int Ztilepos;

    // TODO:
    /*
     * Functions needed:
     * TileUnderDrillPoint (Checks for what tile is under the drill point)
     * [CORO] Extract (Runs an extraction cycle based on the stats returned by TileUnderDrillPoint)
     * [CORO] CoolDown (Transfer resources if pipeline is hooked up, if not, store resources and wait)
     */


    // On drill placement
    void Start()
    {
        TileUnderDrillPoint();
    }

    public void TileUnderDrillPoint()
    {
        tiles = camera.GetComponent<WorldGen>().tiles;
        Xtilepos = camera.GetComponent<Raycasting>().Xtilepos;
        Ztilepos = camera.GetComponent<Raycasting>().Ztilepos;
        Debug.Log("Tile Under Drill is: " + tiles[Xtilepos, Ztilepos]);
    }
}
