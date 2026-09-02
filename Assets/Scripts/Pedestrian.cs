using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Pedestrian : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float queueWaitSeconds = 5f;
    public float stoppingDistance = 0.3f;
    public float stuckTimeout = 3f;

    private enum State
    {
        WalkingToDestination,
        WalkingToQueue,
        WaitingInQueue,
        WaitingForTrain,
    }

    private PlatformEnd originSpawner;
    private WaitingQueue waitingQueue;
    private NavMeshAgent agent;
    private State state;
    private Vector3 destination;
    private float waitTimer = 5f;
    private float arrivalStallTimer;

    // Goal 1: walk straight to the opposite end.
    public void Initialize(PlatformEnd originSpawner, Vector3 destination)
    {
        this.originSpawner = originSpawner;
        this.destination = destination;
        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;
        agent.stoppingDistance = stoppingDistance;

        state = State.WalkingToDestination;
        agent.SetDestination(destination);
    }

    public void InitializeWithQueue(PlatformEnd originSpawner, WaitingQueue waitingQueue, Vector3 destination)
    {
        this.originSpawner = originSpawner;
        this.waitingQueue = waitingQueue;
        this.destination = destination;

        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;
        agent.stoppingDistance = stoppingDistance;

        // TODO: logic messy, check success
        waitingQueue.Enqueue(gameObject, out Vector3 slotPosition);
        state = State.WalkingToQueue;
        agent.SetDestination(slotPosition);
    }

    public void AdvanceInQueue(Vector3 newSlotPosition)
    {
        state = State.WalkingToQueue;
        agent.SetDestination(newSlotPosition);
    }

    private bool HasArrived()
    {
        if (agent.pathPending)
        {
            return false;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            arrivalStallTimer = 0f;
            return true;
        }

        // prevent stuck
        arrivalStallTimer += Time.deltaTime;
        if (arrivalStallTimer >= stuckTimeout && agent.velocity.sqrMagnitude < 0.01f)
        {
            arrivalStallTimer = 0f;
            return true;
        }

        return false;
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
        else if (state == State.WalkingToDestination)
        {
            if (HasArrived())
            {
                Despawn();
            }
        }
    }

    private void Despawn()
    {
        originSpawner.OnPedestrianRemoved(this);
        Destroy(gameObject);
    }
}
