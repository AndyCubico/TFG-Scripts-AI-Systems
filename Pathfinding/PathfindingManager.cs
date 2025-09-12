using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static PathfindingJob;

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance { private set; get; }

    public void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Create a path with a given starting and ending position, then store it in NativeList Path.
    /// </summary>
    /// <param name="path"> List to store the path</param>
    /// <param name="start"> Starting position of path. </param>
    /// <param name="end"> Ending position of path. </param>
    public void StartPathfinding(NativeList<int2> path, int2 start, int2 end)
    {
        FindPathJob findPathJob = new FindPathJob
        {
            startPosition = start,
            endPosition = end,
            resultPath = path,
            gridSize = new int2(GridManager.Instance.grid.GetWidth(), GridManager.Instance.grid.GetHeight()),
            pathNodeArray = GetPathNodeArray(),
        };
        JobHandle handle = findPathJob.Schedule();
        handle.Complete();

        foreach (int2 pathPosition in path)
        {
            //Debug.Log(pathPosition);
        }
    }

    [BurstCompile]
    private struct FindPathJob : IJob 
    {
        public int2 startPosition;
        public int2 endPosition;

        public NativeList<int2> resultPath;

        [DeallocateOnJobCompletion]
        public NativeArray<PathNode> pathNodeArray;

        public int2 gridSize;

        // Movement costs
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        public void Execute()
        {
            // Set up grid
            for (int i = 0; i < pathNodeArray.Length; i++)
            {
                PathNode pathNode = pathNodeArray[i];
                pathNode.hCost = CalculateDistanceCost(new int2(pathNode.x, pathNode.y), endPosition);
                pathNode.previousNodeIndex = -1; // Invalid value, just to initialize the path node values.

                pathNodeArray[i] = pathNode;
            }

            // A* algorithm

            // Neighbour offsets, where can an agent move from a given node.
            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
            neighbourOffsetArray[0] = new int2(-1, 0); // Left;
            neighbourOffsetArray[1] = new int2(1, 0); // Right;
            neighbourOffsetArray[2] = new int2(0, 1); // Up;
            neighbourOffsetArray[3] = new int2(0, -1); // Down;
            neighbourOffsetArray[4] = new int2(-1, -1); //  Left Down;
            neighbourOffsetArray[5] = new int2(-1, 1); // Left Up;
            neighbourOffsetArray[6] = new int2(1, -1); // Right Down;
            neighbourOffsetArray[7] = new int2(1, 1); // Right Up;

            // Calculate the last node index to know if the destination is reached.
            int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);

            // Start
            PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[startNode.index] = startNode;

            // Lists of indexes to manage the algorithm.
            NativeList<int> openList = new NativeList<int>(Allocator.Temp); // Indexes of the nodes not visited visited.
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp); // Indexes of the nodes already visited.

            openList.Add(startNode.index);

            while (openList.Length > 0)
            {
                // Check which is the current node to check for the path to follow.
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
                PathNode currentNode = pathNodeArray[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex)
                {
                    // Reached goal.
                    break;
                }

                // Remove current node from the open list.
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAt(i);
                        break;
                    }
                }

                closedList.Add(currentNodeIndex);

                // Cycle through the neighbours of the current node.
                for (int i = 0; i < neighbourOffsetArray.Length; i++)
                {
                    int2 neighbourOffset = neighbourOffsetArray[i];
                    int2 neighbourPoistion = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                    // Check if the neighbour is inside the grid.
                    if (!IsPositionInGrid(neighbourPoistion, gridSize))
                    {
                        // Not a valid neighbour.
                        continue;
                    }

                    int neighbourNodeIndex = CalculateIndex(neighbourPoistion.x, neighbourPoistion.y, gridSize.x);

                    if (closedList.Contains(neighbourNodeIndex))
                    {
                        // Altready visited this node.
                        continue;
                    }

                    PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];

                    if (!neighbourNode.isWalkable)
                    {
                        // Invalid node, not walkable.
                        continue;
                    }

                    // We have a valid node, let's compare costs to see if the neighbour is valid.
                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);
                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPoistion);

                    if (tentativeGCost <= neighbourNode.gCost) // If it's only "<" it can lead to a suboptimal path.
                    {
                        neighbourNode.previousNodeIndex = currentNodeIndex;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.CalculateFCost();
                        pathNodeArray[neighbourNodeIndex] = neighbourNode;

                        if (!openList.Contains(neighbourNode.index))
                        {
                            openList.Add(neighbourNode.index);
                        }
                    }
                }
            }

            PathNode endNode = pathNodeArray[endNodeIndex];
            if (endNode.previousNodeIndex == -1)
            {
                // No path was found.
                Debug.Log("No path was found");
            }
            else
            {
                // Found a path.
                NativeList<int2> path = CalculatePath(pathNodeArray, endNode);

                for (int i = 0; i < path.Length; i++)
                {
                    resultPath.Add(path[i]);
                }

                path.Dispose();
            }

            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();
        }

        /// <summary>
        /// Create the path from a node array and an end point.
        /// </summary>
        /// <param name="pathNodeArray"> Array with all the nodes of the path. </param>
        /// <param name="endNode"> End node of the array. </param>
        /// <returns> NativeList<int2> of all the indexes. </returns>
        private static NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.previousNodeIndex == -1)
            {
                // No path is found
                return new NativeList<int2>(Allocator.Temp);
            }
            else
            {
                // Found a path
                NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
                path.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;
                while (currentNode.previousNodeIndex != -1)
                {
                    PathNode previousNode = pathNodeArray[currentNode.previousNodeIndex];
                    path.Add(new int2(previousNode.x, previousNode.y));
                    currentNode = previousNode;
                }

                return path;
            }
        }

        /// <summary>
        /// Check if a given position fits inside a grid.
        /// </summary>
        /// <param name="gridPosition"> Position to check. </param>
        /// <param name="gridSize"> Grid that sets the limits. </param>
        /// <returns> True if the gridPosition is valid within the grid. </returns>
        private static bool IsPositionInGrid(int2 gridPosition, int2 gridSize)
        {
            return
                gridPosition.x >= 0 &&
                gridPosition.y >= 0 &&
                gridPosition.x < gridSize.x &&
                gridPosition.y < gridSize.y;
        }

        /// <summary>
        /// Convert an X and Y position into a flat index.
        /// </summary>
        /// <param name="x"> X position of the node int the graph. </param>
        /// <param name="y"> Y position of the node int the graph. </param>
        /// <param name="gridWidth"> Size of the graph widthwise. </param>
        /// <returns> Index of the X,Y position of a node in a graph. </returns>
        private static int CalculateIndex(int x, int y, int gridWidth)
        {
            return x + y * gridWidth;
        }

        /// <summary>
        /// Calculate distance cost from point a to b
        /// </summary>
        /// <param name="aPosition"> int2 of a position. </param>
        /// <param name="bPosition"> int2 of b position. </param>
        /// <returns> The cost from point a to b with the defined diagonal and straight costs. </returns>
        private static int CalculateDistanceCost(int2 aPosition, int2 bPosition)
        {
            int xDistance = math.abs(aPosition.x - bPosition.x);
            int yDistance = math.abs(aPosition.y - bPosition.y);
            int remaining = math.abs(xDistance - yDistance);
            return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        /// <summary>
        /// Get the node with lowest F value of all the nodes to visit
        /// </summary>
        /// <param name="openList"> List of nodes not visited. </param>
        /// <param name="pathNodeArray"> List of all nodes. </param>
        /// <returns> Index of the node with the lowest F value. </returns>
        private static int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            PathNode lowestCostPathNode = pathNodeArray[openList[0]];

            for (int i = 1; i < openList.Length; i++)
            {
                PathNode testPathNode = pathNodeArray[openList[i]];
                if (testPathNode.fCost < lowestCostPathNode.fCost)
                {
                    lowestCostPathNode = testPathNode;
                }
            }

            return lowestCostPathNode.index;
        }
    }

    private static NativeArray<PathNode> GetPathNodeArray()
    {
        Grid<GridNode> grid = GridManager.Instance.grid;
        int2 gridSize = new int2(GridManager.Instance.grid.GetWidth(), GridManager.Instance.grid.GetHeight());

        NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(
            gridSize.x * gridSize.y,
            Allocator.TempJob
        );

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                PathNode pathNode = new PathNode();
                pathNode.x = x;
                pathNode.y = y;

                pathNode.index = CalculateIndex(x, y, gridSize.x);

                pathNode.gCost = int.MaxValue;

                pathNode.isWalkable = grid.GetValue(x, y).IsWalkable();
                pathNode.previousNodeIndex = -1;

                pathNodeArray[pathNode.index] = pathNode; // Fill the native array with the different nodes.
            }
        }

        return pathNodeArray;
    }

    // Struct to manage each individual node in the grid.
    public struct PathNode
    {
        // Position
        public int x;
        public int y;

        // Indexes
        public int index; // Node's index.
        public int previousNodeIndex; // Node's index previous to this node when calculating the path.

        // A* values
        public int gCost; // Move cost from start node to this node.
        public int hCost; // Move cost from this node to the end node.
        public int fCost; // gCost + hCost.

        public bool isWalkable;

        /// <summary>
        /// Calculate F cost value of the node.
        /// </summary>
        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }

        /// <summary>
        /// Set the node to be walkable or not.
        /// </summary>
        /// <param name="isWalkable"></param>
        public void SetIsWalkable(bool isWalkable)
        {
            this.isWalkable = isWalkable;
        }
    }
}
