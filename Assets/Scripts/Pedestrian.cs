using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Pedestrian : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float stoppingDistance = 0.3f;
    public float stuckTimeout = 3f;

    private enum State
    {
        WalkingToDestination,
        WalkingToQueue,
        WaitingInQueue,
        WaitingForTrain,
        Boarding,
    }

    private static Train train;

    private NavMeshAgent agent;
    private PlatformEnd originSpawner;
    private WaitingQueue waitingQueue;
    private State state;
    private Utils.PlatformEndId destinationId;

    public void Initialize(PlatformEnd originSpawner, Utils.PlatformEndId destinationId, Vector3 destination)
    {
        this.originSpawner = originSpawner;
        this.destinationId = destinationId;
        SetupAgent();

        state = State.WalkingToDestination;
        agent.SetDestination(destination);
    }

    public void InitializeWithQueue(PlatformEnd originSpawner, Utils.PlatformEndId destinationId, WaitingQueue waitingQueue, Vector3 destination)
    {
        this.originSpawner = originSpawner;
        this.destinationId = destinationId;
        this.waitingQueue = waitingQueue;
        SetupAgent();

        if (train == null)
        {
            train = FindFirstObjectByType<Train>();
        }

        waitingQueue.Enqueue(gameObject, out Vector3 slotPosition);
        state = State.WalkingToQueue;
        agent.SetDestination(slotPosition);
    }

    public void AdvanceInQueue(Vector3 newSlotPosition)
    {
        state = State.WalkingToQueue;
        agent.SetDestination(newSlotPosition);
    }

    private void SetupAgent()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = walkSpeed;
        agent.stoppingDistance = stoppingDistance;

        // Identical avoidance priority across every pedestrian lets NavMeshAgent's local avoidance
        // deadlock when several converge on a tight single-file queue; stagger it so ties don't happen.
        agent.avoidancePriority = Random.Range(1, 99);
    }

    private bool HasArrived()
    {
        if (agent.pathPending)
        {
            return false;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
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
            if (train != null && waitingQueue.IsFirst(gameObject) && train.IsAtStation(waitingQueue.stationId))
            {
                TrainSeat seat = train.ReserveSeat();
                if (seat != null)
                {
                    waitingQueue.Dequeue(gameObject);
                    state = State.Boarding;
                    agent.SetDestination(seat.transform.position);
                }
            }
        }
        else if (state == State.Boarding)
        {
            if (HasArrived())
            {
                Despawn();
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
