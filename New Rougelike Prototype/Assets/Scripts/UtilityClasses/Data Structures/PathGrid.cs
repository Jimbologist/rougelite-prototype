using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Find a good way to update if a node is walkable.
// Grid that can be used as a data structure and/or pathfinding.
// Can retrieve nodes from world space or direct indeces.
public class PathGrid<T>
{
    public Vector2 gridOrigin;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public PathNode<T>[,] grid;

    private float nodeDiameter;
    private Vector2Int gridSize;
    private bool initialized = false;

    public Vector2Int GridSize { get => gridSize; }
    public int GridArea { get => gridSize.x * gridSize.y; }

    public PathGrid(Vector2 _gridWorldSize, float nodeRadius)
    {
        this.gridWorldSize = _gridWorldSize;
        nodeDiameter = nodeRadius * 2;
        gridSize.x = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSize.y = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
    }

    //Use this constructor if grid is not really in world space!
    public PathGrid(Vector2 _gridWorldSize)
    {
        this.gridWorldSize = _gridWorldSize;
        nodeRadius = 0.5f;
        nodeDiameter = 1.0f;
        gridSize.x = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSize.y = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
    }

    public PathNode<T> GetNodeDirect(int x, int y)
    {
        return grid[x, y];
    }

    public PathNode<T> NodeFromWorldPoint(Vector2 worldPosition)
    {

        int x = Mathf.FloorToInt(Mathf.Clamp(worldPosition.x, gridOrigin.x, gridOrigin.x + gridSize.x - 0.0001f));
        int y = Mathf.FloorToInt(Mathf.Clamp(worldPosition.y, gridOrigin.y, gridOrigin.y + gridSize.y - 0.0001f));

        return grid[x, y];
    }

    //Get neighbors of a node, including diagonals
    public List<PathNode<T>> GetNeighboursDiag(PathNode<T> node)
    {
        List<PathNode<T>> neighbors = new List<PathNode<T>>();

        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                //If neighbor coordinates are in grid bounds, add neighbor.
                if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
                    neighbors.Add(grid[checkX, checkY]);
            }
        }

        return neighbors;
    }

    //Get neighbors of a node, NOT including diagonals.
    //Only up, down, left, and right.
    public List<PathNode<T>> GetNeighbours(PathNode<T> node)
    {
        List<PathNode<T>> neighbors = new List<PathNode<T>>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x != 0 && y != 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                //If neighbor coordinates are in grid bounds, add neighbor.
                if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
                    neighbors.Add(grid[checkX, checkY]);
            }
        }

        return neighbors;
    }

    // Create grid with given world position of bottom left point.
    // Can be an object's position (ex. a room's origin point) or whatver is needed.
    // All positions are walkable by default.
    public void MakeNewGridData(Vector2 originPos)
    {
        gridOrigin = originPos;
        initialized = true;
        grid = new PathNode<T>[gridSize.x, gridSize.y];

        for(int x = 0; x < gridSize.x; x++)
        {
            for(int y = 0; y < gridSize.y; y++)
            {
                Vector2 worldPoint = gridOrigin + (Vector2.right * (x * nodeDiameter)) + (Vector2.up * (y * nodeDiameter));
                grid[x, y] = new PathNode<T>(worldPoint, x, y);
            }
        }
    }

    public List<PathNode<T>> FindPathAStar(Vector2 startPos, Vector2 targetPos, bool includeDiagonals)
    {
        if (!initialized)
        {
            Debug.Log("Cannot find path on grid with no data");
            return null;
        }
        PathNode<T> startNode = NodeFromWorldPoint(startPos);
        PathNode<T> targetNode = NodeFromWorldPoint(targetPos);

        List<PathNode<T>> neighbors;
        PathHeap<PathNode<T>> openSet = new PathHeap<PathNode<T>>(GridArea);
        HashSet<PathNode<T>> closedSet = new HashSet<PathNode<T>>();
        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            PathNode<T> current = openSet.RemoveFirst();
            closedSet.Add(current);

            if (current == targetNode)
                return RetracePath(startNode, targetNode);

            if (includeDiagonals)
                neighbors = GetNeighboursDiag(current);
            else
                neighbors = GetNeighbours(current);

            foreach (PathNode<T> neighbor in neighbors)
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newMovementCost = current.gCost + GetDistance(current, neighbor);
                if(newMovementCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return null;
    }

    private List<PathNode<T>> RetracePath(PathNode<T> startNode, PathNode<T> endNode)
    {
        List<PathNode<T>> path = new List<PathNode<T>>();
        PathNode<T> current = endNode;

        while (current != startNode)
        {
            path.Add(current);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }

    public int GetDistance(PathNode<T> nodeA, PathNode<T> nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return 14 * Mathf.Min(distX, distY) + 10 * Mathf.Abs(distX - distY);
    }
}
