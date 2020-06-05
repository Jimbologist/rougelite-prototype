using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode<TNodeObj>
{
    public bool walkable;
    public Vector2 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;

    public PathNode<TNodeObj> parent;

    //Object on this node at grid location (x, y)
    public TNodeObj nodeObj;

    public int fCost { get { return gCost + hCost; } }

    //Walkable is true by default.
    public PathNode(Vector2 _worldPos, int _gridX, int _gridY)
    {
        
        this.worldPosition = _worldPos;
        this.gridX = _gridX;
        this.gridY = _gridY;
        walkable = true;
    }
}
