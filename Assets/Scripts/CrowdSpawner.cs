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

    [Header("Spawn Settings")]
    public int maxPedestrians = 10;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 2f;
    public float navMeshSampleDistance = 5f;

    private int pedestrianCount = 0;
    private List<GameObject> pedestrians;

    void Start()
    {
        pedestrians = new List<GameObject>();

        if (platformNavMesh != null && platformNavMesh.navMeshData == null)
        {
            platformNavMesh.BuildNavMesh();
        }

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
        Vector3 destinationPosition = otherEnd.transform.TransformPoint(localPoint);

        GameObject pedestrian = Instantiate(pedestrianPrefab, spawnPosition, Quaternion.identity);
        PedestrianController controller = pedestrian.AddComponent<PedestrianController>();
        controller.Initialize(this, otherEnd, destinationPosition);

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
}
