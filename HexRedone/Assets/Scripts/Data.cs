using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data : MonoBehaviour
{
    public bool Wood;
    public bool Stone;
    public bool Oil;

    public void Update()
    {
        Wood = false;
        Stone = false;
        Oil = false;
        Wood = true;
        Stone = true;
        Oil = true;

        Debug.Log("Rahhhhhhhhhh!");
    }
}   
