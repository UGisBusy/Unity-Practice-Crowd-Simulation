using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using Unity.AppUI.UI;

public class Train : MonoBehaviour
{
    public event Action<Station> OnArrivedAtStation;

    [Header("References")]
    public Transform floor;
    public NavMeshSurface navMeshSurface;

    [Header("Dimensions")]
    public float length = 80f;
    public float width = 20f;
    public float floorHeight = 0.15f;

    [Header("Seats")]
    public int seatCountEachSide = 20;
    public float seatSize = 1f;
    public float margin = 1f;

    [Header("Movement")]
    public float moveSpeed = 20f;

    public float gateDistance = 2f;


    public bool isArrived;

    private float gateWidth;
    private List<Station> stations;
    private Vector3 anchorOffset;
    private Vector3 currentTarget;
    private int pendingStationIndex;
    private NavMeshLink gateLink;
    private List<TrainSeat> seats = new List<TrainSeat>();
    private Renderer gateMarkerRenderer;

    public int waitToken;
    public int direction;

    public int CurrentStationIndex { get; private set; }

    public void Initialize(List<Station> stations, float gateWidth, int direction)
    {
        this.stations = stations;
        this.gateWidth = gateWidth;
        this.direction = direction;

        waitToken = 0;

        ResizeFloor();
        SpawnSeats();
        SpawnGate();

        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }

        if (stations == null || stations.Count == 0)
        {
            return;
        }
        anchorOffset = transform.position - stations[0].gatePosition;
        currentTarget = transform.position;
        isArrived = true;
        CurrentStationIndex = 0;
        SetGateOpen(true);
    }

    public void GoToStation(int stationIndex)
    {
        if (stations == null || stationIndex < 0 || stationIndex >= stations.Count)
        {
            return;
        }

        currentTarget = stations[stationIndex].gatePosition + anchorOffset;
        pendingStationIndex = stationIndex;
        isArrived = false;
        SetGateOpen(false);
    }

    public bool IsAtStation(int stationId)
    {
        return isArrived && CurrentStationIndex == stationId;
    }

    public TrainSeat ReserveSeat()
    {
        foreach (TrainSeat seat in seats)
        {
            if (!seat.IsOccupied)
            {
                seat.IsOccupied = true;
                return seat;
            }
        }

        return null;
    }

    void Update()
    {
        if (isArrived)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
        if (transform.position == currentTarget)
        {
            isArrived = true;
            CurrentStationIndex = pendingStationIndex;
            SetGateOpen(true);

            if (CurrentStationIndex == Utils.STATION_COUNT - 1)
            {
                direction = -1;
            }
            else if (CurrentStationIndex == 0)
            {
                direction = 1;
            }

            OnArrivedAtStation?.Invoke(stations[CurrentStationIndex]);
        }
    }

    private void ResizeFloor()
    {
        if (floor == null)
        {
            return;
        }

        // Default Unity Plane mesh is 10x10 units.
        floor.localScale = new Vector3(length / 10f, floor.localScale.y, width / 10f);
    }

    private void SpawnSeats()
    {
        Transform seatsParent = new GameObject("Seats").transform;
        seatsParent.SetParent(transform, false);

        List<float> slots = GetSeatSlotPositions();
        float rowZ = width / 2f - margin;

        int seatIndex = 0;
        for (int row = 0; row < 2; row++)
        {
            float z = row == 0 ? -rowZ : rowZ;
            foreach (float x in slots)
            {
                CreateSeat(seatsParent, x, z, seatIndex);
                seatIndex++;
            }
        }
    }

    private List<float> GetSeatSlotPositions()
    {
        List<float> slots = new List<float>();

        float halfLength = length / 2f - margin;
        float halfGate = gateWidth / 2f;

        float availableLength = length - 2f * margin - gateWidth;
        float spacing = availableLength / (seatCountEachSide - 1);

        for (int i = 0; i < seatCountEachSide; i++)
        {
            float x = -halfLength + i * spacing;
            if (x > -halfGate)
            {
                x += gateWidth;
            }
            slots.Add(x);
        }

        return slots;
    }

    private void CreateSeat(Transform parent, float x, float z, int index)
    {
        GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat.name = $"Seat_{index}";
        seat.transform.SetParent(parent, false);
        seat.transform.localPosition = new Vector3(x, floorHeight + seatSize / 2f, z);
        seat.transform.localScale = Vector3.one * seatSize;
        seats.Add(seat.AddComponent<TrainSeat>());
    }

    private void SpawnGate()
    {
        // Threshold sits on the train's near edge, facing the platform (-Z, toward the gate).
        GameObject gate = new GameObject("Gate");
        gate.transform.SetParent(transform, false);
        gate.transform.localPosition = new Vector3(0f, floorHeight, -gateWidth / 2 + 0.7f);

        gateLink = gate.AddComponent<NavMeshLink>();
        gateLink.enabled = false;
        gateLink.startPoint = new Vector3(0f, 0f, 0f);
        gateLink.endPoint = new Vector3(0f, 0f, -gateDistance);
        gateLink.width = gateWidth;
        gateLink.bidirectional = true;


        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "GateMarker";
        marker.transform.SetParent(gateLink.transform, false);
        marker.transform.localScale = new Vector3(gateWidth, 0.05f, gateDistance);
        marker.transform.localPosition = new Vector3(0f, 0f, -gateDistance / 2f);
        Destroy(marker.GetComponent<BoxCollider>());
        gateMarkerRenderer = marker.GetComponent<Renderer>();
        gateMarkerRenderer.enabled = false;
    }

    private void SetGateOpen(bool open)
    {
        if (gateLink != null)
        {
            gateLink.enabled = open;
        }

        if (gateMarkerRenderer != null)
        {
            gateMarkerRenderer.enabled = open;
        }
    }
}
