using UnityEngine;

public class Station : MonoBehaviour
{
    [Tooltip("Distance from the line between the two PlatformEnds out to the gate, i.e. the platform's half-width.")]
    public float platformDepth = 20f;

    public Vector3 gatePosition;

    private PlatformEnd platformEndA;
    private PlatformEnd platformEndB;
    private WaitingQueue queueA;
    private WaitingQueue queueB;

    private float gateWidth;
    private int id;

    public void Initialize(int id, float gateWidth)
    {
        this.id = id;
        this.gateWidth = gateWidth;

        PlatformEnd[] platformEnds = GetComponentsInChildren<PlatformEnd>();
        if (platformEnds.Length != 2)
        {
            Debug.LogError($"Station {name} must have 2 PlatformEnd");
            return;
        }
        platformEndA = platformEnds[0];
        platformEndB = platformEnds[1];

        ComputeGatePosition();

        queueA = BuildWaitingQueue(platformEndA, platformEndB);
        queueB = BuildWaitingQueue(platformEndB, platformEndA);

        InitializePlatormEnds();
    }

    private void ComputeGatePosition()
    {
        Vector3 centerline = (platformEndA.transform.position + platformEndB.transform.position) / 2f;
        Vector3 topDirection = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        gatePosition = centerline + topDirection * platformDepth;
    }

    private void InitializePlatormEnds()
    {
        platformEndA.Initialize(platformEndB, (Utils.PlatformEndId)(id * 2), this);
        platformEndB.Initialize(platformEndA, (Utils.PlatformEndId)(id * 2 + 1), this);
    }

    private WaitingQueue BuildWaitingQueue(PlatformEnd platformEnd, PlatformEnd otherEnd)
    {
        WaitingQueue waitingQueue = new WaitingQueue();
        waitingQueue.stationId = id;
        Vector3 alongPlatform = platformEnd.transform.position - otherEnd.transform.position;
        Vector3 lineDirection = new Vector3(alongPlatform.x, 0f, alongPlatform.z).normalized;

        float firstSlotOffset = gateWidth / 2f + waitingQueue.slotSpacing;
        waitingQueue.Initialize(gatePosition + lineDirection * firstSlotOffset, lineDirection);
        return waitingQueue;
    }

    public WaitingQueue ChooseQueue(Vector3 fromPosition)
    {
        if (queueA.Count == queueB.Count)
        {
            Vector3 nextSlotA = queueA.GetSlotPosition(queueA.Count);
            Vector3 nextSlotB = queueB.GetSlotPosition(queueB.Count);

            float sqrDistA = (nextSlotA - fromPosition).sqrMagnitude;
            float sqrDistB = (nextSlotB - fromPosition).sqrMagnitude;
            return sqrDistA < sqrDistB ? queueA : queueB;
        }

        return queueA.Count < queueB.Count ? queueA : queueB;
    }

    public Vector3 GetRandomPositionAtPlatform(Utils.PlatformEndId id)
    {
        PlatformEnd platformEnd = ((int)id % 2 == 0) ? platformEndA : platformEndB;
        return platformEnd.GetRandomPosition(null);
    }

    void Start()
    {

    }

    void Update()
    {

    }

}
