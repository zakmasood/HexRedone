using UnityEngine;
using TMPro;

public class Battery : MonoBehaviour
{
    public float batteryTime;
    public float bT;
    public TMP_Text batteryCounter;
    public GameObject eIcon;
    public GameObject resources;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bT = batteryTime;
    }

    // Update is called once per frame
    void Update()
    {
        bT -= Time.deltaTime;
        RaycastHit hit;
        batteryCounter.SetText(bT.ToString());
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100.0f))
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
            if(hit.transform.tag == "Base") 
            {
                bT = batteryTime;
            }
            }
            if(hit.transform.tag == "Base") 
            {
                eIcon.SetActive(true);
                resources.SetActive(true);
            }
            if(hit.transform.tag != "Base")
            {
                eIcon.SetActive(false);
                resources.SetActive(false);
            }
        }
        if(bT < 0)
        {
            Debug.Log("Ran Out Of Battery");
        }
    }
}
