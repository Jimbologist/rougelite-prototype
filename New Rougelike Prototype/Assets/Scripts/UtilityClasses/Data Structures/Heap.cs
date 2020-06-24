using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Heap class specifically for pathfinding with nodes using a Priority Queue
//If a heap is required as a data structure for some other part of the
//program, it will use a different, more robust script. This has to use PathNodes so
//that its a good middle ground between a simple and fast implementation.

//Basically, I'm too lazy to make a Heap class that works for any type. Fuck that.
//I just want some good ass pathfinding, and that's likely the only thing in this
//entire game that needs a priority queue to be optimized. I know it's lazy fuck you.
public class PathHeap<T> where T : IHeapItem<T>
{
    T[] items;
    int currentItemCount;

    public int Count { get { return currentItemCount; } }

    public PathHeap(int maxSize)
    {
        items = new T[maxSize];
    }

    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    public T RemoveFirst()
    {
        T firstItem = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    public void UpdateNode (T item)
    {
        SortUp(item);
    }

    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    //Sorts item downward in heap to correct position
    private void SortDown(T item)
    {
        while(true)
        {
            int childIndexLeft = (item.HeapIndex * 2) + 1;
            int childIndexRight = (item.HeapIndex * 2) + 2;
            int swapIndex = 0;

            if (childIndexLeft < currentItemCount)
            {
                //Swap with left child if it's less than current, unless right is also less.
                //In that case, compare the two nodes and compare the two. Swap with lowest one.
                swapIndex = childIndexLeft;
                if (childIndexRight < currentItemCount)
                {
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                        swapIndex = childIndexRight;
                }

                if (item.CompareTo(items[swapIndex] ) < 0)
                    Swap(item, items[swapIndex]);
                else
                    return;
            }
            else
                return;
        }
    }

    private void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;
        
        while(true)
        {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
                Swap(item, parentItem);
            else
                break;

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    private void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        int tempIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = tempIndex;
    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex { get; set; }
}
