using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
public class WaitingQueue
{
    public Vector3 originPosition;
    public Vector3 lineDirection;
    public int stationId;

    public float slotSpacing = 3f;

    public int maxSlot = 1000;

    private LinkedList<Pedestrian> queue;

    private LinkedListNode<Pedestrian> onboardCandidate;

    public int Count => queue.Count;

    public void Initialize(Vector3 originPosition, Vector3 lineDirection)
    {
        this.originPosition = originPosition;
        this.lineDirection = lineDirection.normalized;
        queue = new LinkedList<Pedestrian>();
    }

    public bool Enqueue(Pedestrian pedestrian, out Vector3 slotPosition)
    {
        slotPosition = Vector3.zero;
        if (IsFull())
        {
            return false;
        }

        slotPosition = GetSlotPosition(queue.Count);
        queue.AddLast(pedestrian);

        if (onboardCandidate == null)
        {
            onboardCandidate = queue.Last;
        }
        return true;
    }

    public bool Dequeue(Pedestrian pedestrian)
    {
        if (onboardCandidate == null || onboardCandidate.Value != pedestrian)
        {
            return false;
        }
        LinkedListNode<Pedestrian> nextCandidate = (onboardCandidate.Previous != null) ? onboardCandidate.Previous : onboardCandidate.Next;
        queue.Remove(onboardCandidate);
        onboardCandidate = nextCandidate;
        AdvanceAll();
        return true;
    }

    private void AdvanceAll()
    {
        if (IsFull())
        {
            return;
        }

        int newSlot = 0;
        foreach (Pedestrian pedestrian in queue)
        {
            Vector3 newSlotPosition = GetSlotPosition(newSlot);

            pedestrian.AdvanceInQueue(newSlotPosition);
            newSlot++;
        }
    }

    public bool IsFull()
    {
        return queue.Count >= maxSlot;
    }

    public bool CanOnboard(Pedestrian pedestrain)
    {
        if (onboardCandidate == null)
        {
            return false;
        }
        return onboardCandidate.Value == pedestrain;
    }

    public Vector3 GetSlotPosition(int slot)
    {
        return originPosition + lineDirection * slot * slotSpacing;
    }

    public void HandleTrainArrivedAtStation(Station station)
    {
        onboardCandidate = queue.First;
    }

    public void AdvanceCandidate()
    {
        if (onboardCandidate == null)
        {
            return;
        }
        onboardCandidate = onboardCandidate.Next;
    }
}
