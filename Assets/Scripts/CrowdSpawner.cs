using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrowdSpawner : MonoBehaviour
{

    public GameObject SamplePedestrian;

    [Header("Spawn Settings")]
    public int maxPedestrians = 10;
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 2f;


    private int pedestrianCount = 0;
    private List<GameObject> pedestrians;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pedestrians = new List<GameObject>();
        StartCoroutine(SpawnPedestrianCoroutine());
    }

    // Update is called once per frame
    void Update()
    {

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
        GameObject pedestrian = Instantiate(SamplePedestrian, transform.position, transform.rotation);
        PedestrianController controller = pedestrian.AddComponent<PedestrianController>();
        controller.Initialize(this);

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
