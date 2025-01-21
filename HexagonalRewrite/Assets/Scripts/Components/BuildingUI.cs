using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    [SerializeField] private BuildingManager buildingManager;
    [SerializeField] private BuildingData building;

    public void OnSelectBuilding()
    {
        buildingManager.SelectBuilding(building);
    }
}
