using UnityEngine;

public class PedestrianController : MonoBehaviour
{
    private CrowdSpawner spawner;

    public void Initialize(CrowdSpawner spawner)
    {
        this.spawner = spawner;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("destroyPedestrain", 5f);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void destroyPedestrain()
    {
        spawner.OnPedestrianRemoved(gameObject);
        Destroy(gameObject);
    }

}
