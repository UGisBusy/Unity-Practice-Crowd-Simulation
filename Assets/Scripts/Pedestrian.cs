using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Pedestrian : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float stoppingDistance = 0.3f;
    public float stuckTimeout = 3f;

    public enum State
    {
        WalkingToDestination,
        WalkingToQueue,
        WaitingInQueue,
        Boarding,
        Onboard,
        Disembarking,
    }

    private static Train train;
    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;
    private PlatformEnd originSpawner;
    private WaitingQueue waitingQueue;

    // TODO: debug
    public State state;
    private Utils.PlatformEndId destinationId;
    private TrainSeat reservedSeat;

    // TODO: find better way
    private Vector3 finalPosition;

    public void Initialize(PlatformEnd originSpawner, Utils.PlatformEndId destinationId, Vector3 destination)
    {
        this.originSpawner = originSpawner;
        this.destinationId = destinationId;
        SetupAgent();

        state = State.WalkingToDestination;
        agent.SetDestination(destination);
    }

    public void InitializePassenger(PlatformEnd originSpawner, Utils.PlatformEndId destinationId, WaitingQueue waitingQueue)
    {
        this.originSpawner = originSpawner;
        this.destinationId = destinationId;
        this.waitingQueue = waitingQueue;
        SetupAgent();

        if (train == null)
        {
            train = FindFirstObjectByType<Train>();
        }

        waitingQueue.Enqueue(this, out Vector3 slotPosition);
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

        // prevent pedestrians from getting stuck on each other by randomizing avoidance priority
        agent.avoidancePriority = Random.Range(1, 99);

        Renderer pedestrianRenderer = GetComponent<Renderer>();
        if (pedestrianRenderer != null)
        {
            pedestrianRenderer.material.color = Utils.GetPlatformColor(destinationId);
        }
    }

    private bool HasArrived(bool soft = false)
    {
        if (agent.pathPending)
        {
            return false;
        }

        float factor = soft ? 2f : 1f;
        if (agent.remainingDistance <= agent.stoppingDistance * factor)
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
                state = State.WaitingInQueue;
            }
        }
        else if (state == State.WaitingInQueue)
        {
            if (train != null && waitingQueue.CanOnboard(this) && train.IsAtStation(waitingQueue.stationId))
            {
                int currentStationId = waitingQueue.stationId;
                int destinationStationId = Utils.GetStationId(destinationId);
                int direction = (destinationStationId > currentStationId) ? 1 : -1;

                if (direction != train.direction)
                {
                    waitingQueue.AdvanceCandidate();
                }
                else
                {
                    TrainSeat seat = train.ReserveSeat();
                    if (seat != null)
                    {
                        train.waitToken++;
                        waitingQueue.Dequeue(this);
                        reservedSeat = seat;
                        state = State.Boarding;
                        agent.SetDestination(seat.transform.position);
                    }
                }
            }
        }
        else if (state == State.Boarding)
        {
            if (HasArrived())
            {
                state = State.Onboard;
                train.waitToken--;
                transform.SetParent(train.transform, true);

                agent.enabled = false;
                if (obstacle == null)
                {
                    obstacle = gameObject.AddComponent<NavMeshObstacle>();
                    obstacle.shape = NavMeshObstacleShape.Capsule;
                    obstacle.radius = agent.radius;
                    obstacle.height = agent.height;
                    obstacle.carving = true;
                }
                obstacle.enabled = true;

                train.OnArrivedAtStation += HandleTrainArrivedAtStation;
            }
        }
        else if (state == State.Disembarking)
        {
            if (HasArrived(true))
            {
                train.waitToken--;
                state = State.WalkingToDestination;
                agent.SetDestination(finalPosition);
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

    private void HandleTrainArrivedAtStation(Station station)
    {
        if (station.id != Utils.GetStationId(destinationId))
        {
            return;
        }

        train.OnArrivedAtStation -= HandleTrainArrivedAtStation;
        reservedSeat.IsOccupied = false;
        reservedSeat = null;

        transform.SetParent(null, true);
        obstacle.enabled = false;
        agent.enabled = true;

        train.waitToken++;

        state = State.Disembarking;
        agent.SetDestination(station.gatePosition);
        finalPosition = station.GetRandomPositionAtPlatform(destinationId);
    }

    void OnDestroy()
    {
        if (state == State.Onboard && train != null)
        {
            train.OnArrivedAtStation -= HandleTrainArrivedAtStation;
        }
    }

    private void Despawn()
    {
        originSpawner.OnPedestrianRemoved(this);
        Destroy(gameObject);
    }
}
