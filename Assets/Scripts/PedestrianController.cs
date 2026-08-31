using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PedestrianController : MonoBehaviour
{
    public float walkSpeed = 10f;

    private CrowdSpawner originSpawner;
    private CrowdSpawner destinationSpawner;
    private NavMeshAgent agent;

    public void Initialize(CrowdSpawner originSpawner, CrowdSpawner destinationSpawner, Vector3 walkTarget)
    {
        this.originSpawner = originSpawner;
        this.destinationSpawner = destinationSpawner;
        agent = GetComponent<NavMeshAgent>();

        // agent.Warp(transform.position);
        agent.speed = walkSpeed;
        agent.SetDestination(walkTarget);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (destinationSpawner != null && other.gameObject == destinationSpawner.gameObject)
        {
            Despawn();
        }
    }

    private void Despawn()
    {
        originSpawner.OnPedestrianRemoved(gameObject);
        Destroy(gameObject);
    }
}
