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

    [Header("Goal Settings")]

    public int[] GoalWeights = new int[]
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
    private List<Utils.PlatformEndId> goalArray = new List<Utils.PlatformEndId>();


    public void Initilize(PlatformEnd otherEnd, Utils.PlatformEndId id, Station station)
    {
        this.otherEnd = otherEnd;
        this.id = id;
        this.station = station;

        pedestrians = new List<Pedestrian>();
        if (platformNavMesh != null && platformNavMesh.navMeshData == null)
        {
            platformNavMesh.BuildNavMesh();
        }
        PopulateGoalArray();

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

        GameObject pedestrianObj = Instantiate(pedestrianPrefab, spawnPosition, Quaternion.identity);
        Pedestrian pedestrian = pedestrianObj.AddComponent<Pedestrian>();

        Utils.PlatformEndId goalId = GetRandomGoalId();

        if (Utils.IsSameStation(goalId, id))
        {
            // passthrough
            pedestrian.Initialize(this, oppositePosition);
        }
        else
        {
            // passenger
            WaitingQueue queue = station.ChooseQueue(spawnPosition);
            pedestrian.InitializeWithQueue(this, queue, oppositePosition);
        }

        pedestrianCount++;
        pedestrians.Add(pedestrian);
    }


    public void OnPedestrianRemoved(Pedestrian pedestrian)
    {
        if (pedestrians.Contains(pedestrian))
        {
            pedestrians.Remove(pedestrian);
            pedestrianCount--;
        }
    }

    private void PopulateGoalArray()
    {
        for (int i = 0; i < GoalWeights.Length; i++)
        {
            Utils.PlatformEndId id = (Utils.PlatformEndId)i;
            if (id == this.id) continue;
            for (int j = 0; j < GoalWeights[i]; j++)
            {
                goalArray.Add(id);
            }
        }
    }

    private Utils.PlatformEndId GetRandomGoalId()
    {
        int i = Random.Range(0, goalArray.Count);
        return goalArray[i];
    }
}
