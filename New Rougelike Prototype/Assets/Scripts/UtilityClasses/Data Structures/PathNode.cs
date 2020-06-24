using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Nodes used for PathFinding in the PathGrid class. Implements the
//IHeapItem interface to allow use of a priority queue in PathGrid
//pathfinding. Can also contain a reference to an object that is
//currently in the Nodes occupied space.
public class PathNode<T> : IHeapItem<PathNode<T>>
{
    public PathNode<T> parent;

    //Object on this node at grid location (x, y)
    public T nodeObj;

    public bool walkable;
    public Vector2 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;

    private int heapIndex;

    public int fCost { get { return gCost + hCost; } }
    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    public int CompareTo(PathNode<T> nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
            compare = hCost.CompareTo(nodeToCompare.hCost);
        //We return 1 if node is lower in our case, so return -compare.
        return -compare;
    }

    //Walkable is true by default.
    public PathNode(Vector2 _worldPos, int _gridX, int _gridY)
    {
        
        this.worldPosition = _worldPos;
        this.gridX = _gridX;
        this.gridY = _gridY;
        walkable = true;
    }
}
