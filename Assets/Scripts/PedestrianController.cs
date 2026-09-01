using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PedestrianController : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float queueWaitSeconds = 5f;

    private enum State
    {
        WalkingToDestination,
        WalkingToQueue,
        Waiting,
    }

    private CrowdSpawner originSpawner;
    private CrowdSpawner destinationSpawner;
    private NavMeshAgent agent;
    private State state;
    private Vector3 destination;
    private float waitTimer;
    private int queueSlot;
    private bool holdingQueueSlot;

    // Goal 1: walk straight to the opposite end.
    public void Initialize(CrowdSpawner originSpawner, CrowdSpawner destinationSpawner, Vector3 destination)
    {
        this.originSpawner = originSpawner;
        this.destinationSpawner = destinationSpawner;
        this.destination = destination;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;

        state = State.WalkingToDestination;
        agent.SetDestination(destination);
    }

    // Goal 2 (partial): wait at the queue spot for a while, then continue to the opposite end.
    public void InitializeWithQueue(CrowdSpawner originSpawner, CrowdSpawner destinationSpawner, Vector3 queuePoint, Vector3 destination, int queueSlot)
    {
        this.originSpawner = originSpawner;
        this.destinationSpawner = destinationSpawner;
        this.destination = destination;
        this.queueSlot = queueSlot;
        holdingQueueSlot = true;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;

        state = State.WalkingToQueue;
        agent.SetDestination(queuePoint);
    }

    private bool HasArrived()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && agent.velocity.sqrMagnitude < 0.01f;
    }

    void Update()
    {
        if (state == State.WalkingToQueue)
        {
            if (HasArrived())
            {
                state = State.Waiting;
                waitTimer = queueWaitSeconds;
            }
        }
        else if (state == State.Waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                ReleaseQueueSlot();
                state = State.WalkingToDestination;
                agent.SetDestination(destination);
            }
        }
    }

    private void ReleaseQueueSlot()
    {
        if (holdingQueueSlot)
        {
            originSpawner.ReleaseQueueSlot(queueSlot);
            holdingQueueSlot = false;
        }
    }

    private void OnDestroy()
    {
        ReleaseQueueSlot();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (state != State.WalkingToDestination)
        {
            return;
        }

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
