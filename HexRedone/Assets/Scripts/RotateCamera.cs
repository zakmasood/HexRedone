using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{   
    public float Rotation;
    public float lerpTime;
    private float FinalRotation = 60;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, FinalRotation, 0), lerpTime * Time.deltaTime);
        if(Input.GetKeyDown(KeyCode.Q))   
        {
            FinalRotation -= Rotation;
        }
        if(Input.GetKeyDown(KeyCode.E))   
        {
            FinalRotation += Rotation;
        }
    }
}
