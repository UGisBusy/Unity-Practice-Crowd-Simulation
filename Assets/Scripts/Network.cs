using System.Runtime.CompilerServices;
using UnityEngine;

public class Network : MonoBehaviour
{

    private Station[] stations;

    void Start()
    {
        stations = GetComponentsInChildren<Station>();
        InitialzeStatoins();
    }

    void Update()
    {

    }

    private void InitialzeStatoins()
    {
        // hard check station number
        if (stations.Length != Utils.STATION_COUNT)
        {
            Debug.LogError($"Number of Station must be {Utils.STATION_COUNT}");
            return;
        }

        for (int id = 0; id < Utils.STATION_COUNT; id++)
        {
            stations[id].Initialize(id);
        }
    }
}
