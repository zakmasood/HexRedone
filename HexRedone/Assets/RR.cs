using UnityEngine;
using Random = UnityEngine.Random;
public class RR : MonoBehaviour
{
    public Quaternion DR;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        DR.y = Random.Range(1.0f, 360.0f);
        transform.localRotation = DR;     
    }
}
