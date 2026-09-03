using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class Train : MonoBehaviour
{
    [Header("References")]
    public Transform floor;
    public NavMeshSurface navMeshSurface;

    [Header("Dimensions")]
    public float length = 80f;
    public float width = 20f;
    public float floorHeight = 0.15f;

    [Header("Gate")]
    public float gateOpenInterval = 10f;
    public Transform platformGatePoint;
    public float defaultBridgeDistance = 1.5f;

    [Header("Seats")]
    public int seatCountEachSide = 20;
    public float seatSize = 1f;
    public float margin = 1f;

    [Header("Movement")]
    public float moveSpeed = 20f;

    private float gateWidth;
    private Network network;
    private Vector3[] routePositions;
    private Vector3 currentTarget;
    private bool isMoving;

    public void Initialize(Network network, float gateWidth, Vector3[] stationPositions)
    {
        this.gateWidth = gateWidth;
        this.network = network;

        ResizeFloor();
        SpawnSeats();
        SpawnGate();

        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }

        if (stationPositions == null || stationPositions.Length == 0)
        {
            return;
        }
        Vector3 anchorOffset = transform.position - stationPositions[0];
        routePositions = new Vector3[stationPositions.Length];
        for (int i = 0; i < stationPositions.Length; i++)
        {
            routePositions[i] = stationPositions[i] + anchorOffset;
        }

        currentTarget = transform.position;
        isMoving = false;
    }

    public void GoToStation(int stationIndex)
    {
        if (routePositions == null || stationIndex < 0 || stationIndex >= routePositions.Length)
        {
            return;
        }

        currentTarget = routePositions[stationIndex];
        isMoving = true;
    }

    void Update()
    {
        if (!isMoving)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
        if (transform.position == currentTarget)
        {
            isMoving = false;
            network.OnTrainArrive();
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
        seat.AddComponent<TrainSeat>();
    }

    private void SpawnGate()
    {
        // Threshold sits on the train's near edge, facing the platform (-Z, toward the gate).
        GameObject gate = new GameObject("Gate");
        gate.transform.SetParent(transform, false);
        gate.transform.localPosition = new Vector3(0f, floorHeight, -width / 2f);

        NavMeshLink link = gate.AddComponent<NavMeshLink>();
        link.enabled = false;
        link.startPoint = Vector3.zero;
        link.endPoint = platformGatePoint != null
            ? gate.transform.InverseTransformPoint(platformGatePoint.position)
            : new Vector3(0f, 0f, -defaultBridgeDistance);
        link.width = gateWidth;
        link.bidirectional = true;

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = "GateMarker";
        marker.transform.SetParent(gate.transform, false);
        marker.transform.localScale = new Vector3(gateWidth, 0.05f, 0.4f);
        Destroy(marker.GetComponent<BoxCollider>());

        Gate trainGate = gate.AddComponent<Gate>();
        trainGate.cycleInterval = gateOpenInterval;
    }
}
