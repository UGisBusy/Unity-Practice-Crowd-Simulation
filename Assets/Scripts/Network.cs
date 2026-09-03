using System.Runtime.CompilerServices;
using UnityEngine;

public class Network : MonoBehaviour
{

    public float gateWidth = 10f;
    private Station[] stations;

    private Train train;

    void Start()
    {
        stations = GetComponentsInChildren<Station>();
        InitialzeStatoins();

        train = GetComponentInChildren<Train>();
        InitializeTrain();
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
            stations[id].Initialize(id, gateWidth);
        }
    }

    private void InitializeTrain()
    {
        if (train == null)
        {
            Debug.LogError("Train not found in Network");
            return;
        }

        train.Initialize(gateWidth);
    }
}
