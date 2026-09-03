using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;


public class PlatformEnd : MonoBehaviour
{

    [Header("References")]
    public GameObject pedestrianPrefab;
    public NavMeshSurface platformNavMesh;

    [Header("Destination Settings")]

    public int[] DestinationWeights = new int[]
    {
        1, // Station1A
        1, // Station1B
        1, // Station2A
        1, // Station2B
        1, // Station3A
        1, // Station3B
        1, // Station4A
        1, // Station4B
    };

    [Header("Spawn Settings")]
    public int maxPedestrians = 10;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 2f;
    public float navMeshSampleDistance = 5f;

    private PlatformEnd otherEnd;
    private Station station;

    private int pedestrianCount = 0;
    private List<Pedestrian> pedestrians;

    private Utils.PlatformEndId id;
    private List<Utils.PlatformEndId> destinationPollArray = new List<Utils.PlatformEndId>();


    public void Initialize(PlatformEnd otherEnd, Utils.PlatformEndId id, Station station)
    {
        this.otherEnd = otherEnd;
        this.id = id;
        this.station = station;

        pedestrians = new List<Pedestrian>();
        if (platformNavMesh != null && platformNavMesh.navMeshData == null)
        {
            platformNavMesh.BuildNavMesh();
        }
        PopulateDestinationPollArray();
        ApplyPlatformColor();

        StartCoroutine(SpawnPedestrianCoroutine());
    }

    private void ApplyPlatformColor()
    {
        Renderer platformRenderer = GetComponent<Renderer>();
        if (platformRenderer != null)
        {
            platformRenderer.material.color = Utils.GetPlatformColor(id);
        }
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
        Vector3 spawnPosition = GetRandomPosition(localPoint);
        Vector3 oppositePosition = otherEnd.transform.TransformPoint(localPoint);

        GameObject pedestrianObj = Instantiate(pedestrianPrefab, spawnPosition, Quaternion.identity);
        Pedestrian pedestrian = pedestrianObj.AddComponent<Pedestrian>();

        Utils.PlatformEndId destinationId = GetRandomDestinationId();

        if (Utils.IsSameStation(destinationId, id))
        {
            // passthrough
            pedestrian.Initialize(this, destinationId, oppositePosition);
        }
        else
        {
            // passenger
            WaitingQueue queue = station.ChooseQueue(spawnPosition);
            pedestrian.InitializePassenger(this, destinationId, queue);
        }

        pedestrianCount++;
        pedestrians.Add(pedestrian);
    }

    public Vector3 GetRandomPosition(Vector3? localPoint)
    {
        if (localPoint == null)
        {
            localPoint = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f));
        }
        return transform.TransformPoint(localPoint.Value);
    }


    public void OnPedestrianRemoved(Pedestrian pedestrian)
    {
        if (pedestrians.Contains(pedestrian))
        {
            pedestrians.Remove(pedestrian);
            pedestrianCount--;
        }
    }

    private void PopulateDestinationPollArray()
    {
        for (int i = 0; i < DestinationWeights.Length; i++)
        {
            Utils.PlatformEndId id = (Utils.PlatformEndId)i;
            if (id == this.id) continue;
            for (int j = 0; j < DestinationWeights[i]; j++)
            {
                destinationPollArray.Add(id);
            }
        }
    }

    private Utils.PlatformEndId GetRandomDestinationId()
    {
        int i = Random.Range(0, destinationPollArray.Count);
        return destinationPollArray[i];
    }
}
