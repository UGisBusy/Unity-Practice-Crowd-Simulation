using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;

public class CrowdSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject pedestrianPrefab;
    public NavMeshSurface platformNavMesh;
    public CrowdSpawner otherEnd;
    public Transform gatePoint;

    [Header("Destination ID Settings")]
    public List<int> DestinationWeights = new List<int> { 1, 1 };

    // use id to identify crowdSpawner for now
    // 0: opposite
    // 1: wait at the gate in a line for 5 sec before go to opposite end


    [Header("Spawn Settings")]
    public int maxPedestrians = 10;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 2f;
    public float navMeshSampleDistance = 5f;

    [Header("Queue Settings")]
    public float queueSlotSpacing = 1f;

    private int pedestrianCount = 0;
    private List<GameObject> pedestrians;

    private List<int> destinationArray = new List<int>();

    // Which queue slots (indices along the line, back from the gate) are currently occupied.
    private readonly List<int> occupiedQueueSlots = new List<int>();

    void Start()
    {
        pedestrians = new List<GameObject>();
        if (platformNavMesh != null && platformNavMesh.navMeshData == null)
        {
            platformNavMesh.BuildNavMesh();
        }
        PopulateDestinationArray();

        StartCoroutine(SpawnPedestrianCoroutine());
    }

    private IEnumerator SpawnPedestrianCoroutine()
    {
        while (true)
        {
            if (pedestrianCount < maxPedestrians)
            {
                float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
                yield return new WaitForSeconds(spawnInterval);
                SpawnPedestrian();
            }
            else
            {
                yield return new WaitForSeconds(1);
            }
        }
    }

    private void SpawnPedestrian()
    {
        if (otherEnd == null)
        {
            return;
        }

        Vector3 localPoint = new(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f));
        Vector3 spawnPosition = transform.TransformPoint(localPoint);
        Vector3 oppositePosition = otherEnd.transform.TransformPoint(localPoint);

        GameObject pedestrian = Instantiate(pedestrianPrefab, spawnPosition, Quaternion.identity);
        PedestrianController controller = pedestrian.AddComponent<PedestrianController>();

        int destId = GetRandomDestinationId();

        if (destId == 0)
        {
            controller.Initialize(this, otherEnd, oppositePosition);
        }
        else
        {
            // wait at the gate in a line for 5 sec before go to opposite end
            int slot = AcquireQueueSlot();
            Vector3 queueSlotPosition = GetQueueSlotPosition(slot);
            controller.InitializeWithQueue(this, otherEnd, queueSlotPosition, oppositePosition, slot);
        }

        pedestrianCount++;
        pedestrians.Add(pedestrian);
    }

    public void OnPedestrianRemoved(GameObject pedestrian)
    {
        if (pedestrians.Contains(pedestrian))
        {
            pedestrians.Remove(pedestrian);
            pedestrianCount--;
        }
    }

    // Hands out the frontmost free slot in the line so it stays gapless as pedestrians pass through.
    public int AcquireQueueSlot()
    {
        int slot = 0;
        while (occupiedQueueSlots.Contains(slot))
        {
            slot++;
        }
        occupiedQueueSlots.Add(slot);
        return slot;
    }

    public void ReleaseQueueSlot(int slot)
    {
        occupiedQueueSlots.Remove(slot);
    }

    // Slot 0 is closest to the gate; the line extends back toward this spawner's own end,
    // staying at the gate's height so every slot lands on the walkable surface.
    public Vector3 GetQueueSlotPosition(int slot)
    {
        Vector3 gatePosition = gatePoint.position;
        Vector3 originFlat = new Vector3(transform.position.x, gatePosition.y, transform.position.z);
        Vector3 lineDirection = (originFlat - gatePosition).normalized;
        return gatePosition + lineDirection * (slot + 1) * queueSlotSpacing;
    }

    private void PopulateDestinationArray()
    {
        for (int i = 0; i < DestinationWeights.Count; i++)
        {
            // if (i == DestinationId) continue;
            for (int w = 0; w < DestinationWeights[i]; w++)
            {
                destinationArray.Add(i);
            }
        }
    }

    private int GetRandomDestinationId()
    {
        int i = Random.Range(0, destinationArray.Count);
        return destinationArray[i];
    }
}
