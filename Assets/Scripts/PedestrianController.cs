using Unity.VisualScripting;
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
        WaitingInQueue,
        WaitingForTrain,
    }

    private CrowdSpawner originSpawner;
    private CrowdSpawner destinationSpawner;
    private WaitingQueue waitingQueue;
    private NavMeshAgent agent;
    private State state;
    private Vector3 destination;
    private float waitTimer = 5f;

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
    public void InitializeWithQueue(CrowdSpawner originSpawner, CrowdSpawner destinationSpawner, WaitingQueue waitingQueue, Vector3 destination)
    {
        this.originSpawner = originSpawner;
        this.destinationSpawner = destinationSpawner;
        this.waitingQueue = waitingQueue;
        this.destination = destination;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;

        // TODO: logic messy, check success 
        Vector3 slotPosition;
        waitingQueue.Enqueue(gameObject, out slotPosition);
        state = State.WalkingToQueue;
        agent.SetDestination(slotPosition);
    }

    public void AdvenceInQueue(Vector3 newSlotPosition)
    {
        state = State.WalkingToQueue;
        agent.SetDestination(newSlotPosition);
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
                if (waitingQueue.IsFirst(gameObject))
                {
                    state = State.WaitingForTrain;
                }
                else
                {
                    state = State.WaitingInQueue;
                }
            }
        }
        else if (state == State.WaitingForTrain)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                state = State.WalkingToDestination;
                agent.SetDestination(destination);
                waitingQueue.Dequeue(gameObject);
            }
        }
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
