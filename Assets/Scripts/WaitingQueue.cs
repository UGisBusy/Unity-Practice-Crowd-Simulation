using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
public class WaitingQueue
{
    public Vector3 originPosition;
    public Vector3 lineDirection;

    public float slotSpacing = 1f;

    public int maxSlot = 1000;

    private Queue<GameObject> queue = new Queue<GameObject>();

    public void Initialize(Vector3 originPosition, Vector3 lineDirection)
    {
        this.originPosition = originPosition;
        this.lineDirection = lineDirection.normalized;
    }

    public bool Enqueue(GameObject pedestrian, out Vector3 slotPosition)
    {
        slotPosition = Vector3.zero;
        if (IsFull())
        {
            return false;
        }

        slotPosition = GetSlotPosition(queue.Count);
        queue.Enqueue(pedestrian);

        return true;
    }

    public bool Dequeue(GameObject pedestrian)
    {
        if (queue.Count == 0 || queue.First() != pedestrian)
        {
            return false;
        }

        queue.Dequeue();
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
        foreach (GameObject pedestrian in queue)
        {
            Vector3 newSlotPoisition = GetSlotPosition(newSlot);
            PedestrianController controller = pedestrian.GetComponent<PedestrianController>();
            controller.AdvenceInQueue(newSlotPoisition);
            newSlot++;
        }
    }

    public bool IsFull()
    {
        return queue.Count >= maxSlot;
    }

    public bool IsFirst(GameObject pedestrian)
    {
        return queue.Count > 0 && queue.Peek() == pedestrian;
    }

    public int GetCount()
    {
        return queue.Count;
    }

    public Vector3 GetSlotPosition(int slot)
    {
        return originPosition + lineDirection * slot * slotSpacing;
    }

}