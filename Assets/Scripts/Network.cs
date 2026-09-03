using System.Runtime.CompilerServices;
using UnityEngine;

public class Network : MonoBehaviour
{

    public float gateWidth = 10f;
    public float trainStopDuration = 5f;

    private Station[] stations;
    private Vector3[] stationGatePositions;
    private Train train;
    private int trainStationIndex;
    private int trainDirection;
    private bool trainArrived;
    private float trainStopTimer;

    void Start()
    {
        stations = GetComponentsInChildren<Station>();
        InitializeStations();

        train = FindFirstObjectByType<Train>();
        InitializeTrain();
    }

    void Update()
    {
        UpdateTrain();
    }

    public void OnTrainArrive()
    {
        trainArrived = true;
        trainStopTimer = trainStopDuration;
    }

    private void InitializeStations()
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

        stationGatePositions = new Vector3[stations.Length];
        for (int i = 0; i < stations.Length; i++)
        {
            stationGatePositions[i] = stations[i].gatePosition;
        }

        train.Initialize(this, gateWidth, stationGatePositions);
        trainStationIndex = 0;
        trainDirection = 1;
        trainArrived = true;
        trainStopTimer = trainStopDuration;
    }

    private void UpdateTrain()
    {
        if (train == null || stationGatePositions == null || stationGatePositions.Length < 2)
        {
            return;
        }

        if (trainArrived)
        {
            trainStopTimer -= Time.deltaTime;
            if (trainStopTimer <= 0f)
            {
                AdvanceTrain();
            }
        }
    }


    private void AdvanceTrain()
    {
        if (train == null || stationGatePositions == null || stationGatePositions.Length < 2)
        {
            return;
        }

        int next = trainStationIndex + trainDirection;
        if (next < 0 || next >= stationGatePositions.Length)
        {
            trainDirection = -trainDirection;
            next = trainStationIndex + trainDirection;
        }

        trainStationIndex = next;
        train.GoToStation(trainStationIndex);
        trainArrived = false;
    }

}
