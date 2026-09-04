using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Network : MonoBehaviour
{
    enum TrainState
    {
        Waiting,
        Moving,
    }


    public float gateWidth = 10f;
    public float trainStopDuration = 5f;

    private List<Station> stations;
    private Train train;
    private int trainStationIndex;
    private int trainDirection;
    private TrainState trainState;
    private float trainStopTimer;

    void Start()
    {
        stations = new List<Station>(GetComponentsInChildren<Station>());
        train = FindFirstObjectByType<Train>();

        InitializeStations();
        InitializeTrain();
    }

    void Update()
    {
        UpdateTrain();
    }

    public void OnTrainArrive()
    {
        trainStopTimer = trainStopDuration;
    }

    private void InitializeStations()
    {
        // hard check station number
        if (stations.Count != Utils.STATION_COUNT)
        {
            Debug.LogError($"Number of Station must be {Utils.STATION_COUNT}");
            return;
        }

        for (int id = 0; id < Utils.STATION_COUNT; id++)
        {
            stations[id].Initialize(id, gateWidth, train);
        }
    }

    private void InitializeTrain()
    {
        if (train == null)
        {
            Debug.LogError("Train not found in Network");
            return;
        }

        trainDirection = 1;
        train.Initialize(stations, gateWidth, trainDirection);
        trainStationIndex = 0;
        trainState = TrainState.Waiting;
        trainStopTimer = trainStopDuration;
    }

    private void UpdateTrain()
    {
        if (train == null)
        {
            return;
        }

        if (trainState == TrainState.Waiting)
        {
            trainStopTimer -= Time.deltaTime;
            if (trainStopTimer <= 0f && train.waitToken == 0)
            {
                trainState = TrainState.Moving;
                AdvanceTrain();
            }
        }
        else if (trainState == TrainState.Moving)
        {
            if (train.IsAtStation(trainStationIndex))
            {
                trainState = TrainState.Waiting;
                trainStopTimer = trainStopDuration;
            }
        }
    }


    private void AdvanceTrain()
    {
        if (train == null)
        {
            return;
        }

        int next = trainStationIndex + trainDirection;
        if (next < 0 || next >= stations.Count)
        {
            trainDirection = -trainDirection;
            next = trainStationIndex + trainDirection;
        }

        trainStationIndex = next;
        train.GoToStation(trainStationIndex);
    }
}
