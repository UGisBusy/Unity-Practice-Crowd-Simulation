using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class Train : MonoBehaviour
{
    [Header("References")]
    public Transform floor;
    public NavMeshSurface navMeshSurface;

    [Header("Dimensions")]
    public float length = 40f;
    public float width = 6f;
    public float floorHeight = 0.15f;

    [Header("Gate")]
    [Tooltip("Width of the boarding gate gap in the middle of the train that seats must leave clear.")]
    public float gateWidth = 3f;
    [Tooltip("Seconds between the gate toggling open/closed.")]
    public float gateOpenInterval = 10f;
    [Tooltip("The station platform's gate point this train bridges to when docked. Assign when placing the train next to a platform.")]
    public Transform platformGatePoint;
    [Tooltip("Fallback bridge length used when platformGatePoint isn't assigned.")]
    public float defaultBridgeDistance = 1.5f;

    [Header("Seats")]
    public int seatCountEachSide = 20;
    public float seatSize = 0.4f;
    [Tooltip("Distance of each seat row from the side wall.")]
    public float seatRowInset = 0.8f;
    [Tooltip("Margin kept clear at both ends of the train.")]
    public float endMargin = 0.8f;

    void Awake()
    {
        ResizeFloor();
        SpawnSeats();
        SpawnGate();

        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
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
        float rowZ = width / 2f - seatRowInset;

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

        float halfLength = length / 2f - endMargin;
        float halfGate = gateWidth / 2f;

        float availableLength = length - 2f * endMargin - gateWidth;
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
