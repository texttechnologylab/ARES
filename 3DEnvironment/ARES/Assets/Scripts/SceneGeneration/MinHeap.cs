using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinHeap : MonoBehaviour
{
    public class Element
    {
        public Vector3 value;
        public float priority;
        public float currMaxDistance;
        public Vector3 startVec;
        public Element(Vector3 value, float priority, float currMaxDistance, Vector3 startVec)
        {
            this.value = value;
            this.priority = priority;
            this.currMaxDistance = currMaxDistance;
            this.startVec = startVec;
        }
    }
    public List<Element> heapList;

    public MinHeap()
    {
        heapList = new List<Element>();
    }

    private bool indexExists(int index)
    {
        return index >= 0 && index < heapList.Count;
    }

    public int getParentIndex(int index)
    {
        int parentIndex = Mathf.Min(index/2);
        if (indexExists(parentIndex))
        {
            return parentIndex;
        }
        else
        {
            return -1;
        }
    }

    public void addElement(Vector3 value, float priority, float currMaxDistance, Vector3 startVec)
    {
        Element newElement = new Element(value, priority, currMaxDistance, startVec);
        int insertAtIndex = heapList.Count;
        heapList.Add(newElement);
        repairUp(insertAtIndex);
    }

    public void swapElements(int index1, int index2)
    {
        Element tmp = heapList[index1];
        heapList[index1] = heapList[index2];
        heapList[index2] = tmp;
    }

    public void repairUp(int index)
    {
        int parentIndex = getParentIndex(index);
        if (parentIndex != -1 &&
            heapList[parentIndex].priority > heapList[index].priority)
        {
            swapElements(index, parentIndex);
            repairUp(parentIndex);
        }
    }

    public void repairDown(int index)
    {
        int leftChildIndex = index*2 + 1;
        int rightChildIndex = index*2 + 2;
        if (!indexExists(rightChildIndex))
        {
            if (indexExists(leftChildIndex))
            {
                if (heapList[leftChildIndex].priority < heapList[index].priority)
                {
                    swapElements(leftChildIndex, index);
                }
            }
        }
        else if (heapList[leftChildIndex].priority <= heapList[rightChildIndex].priority)
        {
            if (heapList[leftChildIndex].priority < heapList[index].priority)
            {
                swapElements(leftChildIndex, index);
                repairDown(leftChildIndex);
            }
        }
        else if (heapList[rightChildIndex].priority < heapList[leftChildIndex].priority)
        {
            if (heapList[rightChildIndex].priority < heapList[index].priority)
            {
                swapElements(rightChildIndex, index);
                repairDown(rightChildIndex);
            }
        }
    }

    public Element extractMin()
    {
        if (heapList.Count >= 1)
        {
            Element tmp = heapList[0];
            int indexEnd = heapList.Count - 1;
            heapList[0] = heapList[indexEnd];
            heapList.RemoveAt(indexEnd);
            repairDown(0);
            return tmp;
        }
        return null;
    }
}
