using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Grid<GridNode> grid;
    [SerializeField] private int width, height;
    [SerializeField] private float cellSize;
    [SerializeField] private Vector2 origin;

    [SerializeField] private LayerMask m_NotWalkable; 

    [SerializeField] private bool showDebug = false;

    public static GridManager Instance { private set; get; }

    public void Awake()
    {
        Instance = this;

        grid = new Grid<GridNode>(width, height, cellSize, origin, (Grid<GridNode> g, int x, int y) => new GridNode(g, x, y), showDebug);

        SetGridWalkability();
    }

    private void SetGridWalkability()
    {
        // --- Set grid walkability ---

        // First check for colliders that are blocked terrain.
        bool[,] blockedNodes = new bool[grid.GetWidth(), grid.GetHeight()]; // Store grid nodes that have terrain.

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                Vector2 worldPos = origin + new Vector2(x + 0.5f, y + 0.5f) * cellSize;
                Collider2D hit = Physics2D.OverlapBox(worldPos, Vector2.one * cellSize * 0.8f, 0, m_NotWalkable); // * 0.8f to avoid false positives.
                blockedNodes[x, y] = hit != null;
            }
        }

        // Check which nodes have ground just beneath, they are walkable.
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 1; y < grid.GetHeight(); y++) // Start from y = 1 to allow y - 1.
            {
                // Nodes that are walkable above ground (node below is blocked, current cell is empty).
                if (blockedNodes[x, y - 1] && !blockedNodes[x, y])
                {
                    grid.GetValue(x, y)?.SetIsWalkable(true);
                }
            }
        }

        // In this last step, check if there are any cliffs, and if there are, make all nodes below it walkable until reaching ground.
        // Cliffs are walkable zones that tell the agent to jump instead of to just go walking.
        HashSet<int2> cliffNodes = new HashSet<int2>(new Helper.Int2Comparer()); // Store grid nodes that are cliffs.

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                // Not walkable but not blocked by ground.
                if (!blockedNodes[x, y] && grid.GetValue(x, y)?.IsWalkable() == false)
                {
                    // Check if right or left nodes are walkable and have not been previously checked for being a cliff.
                    bool leftIsWalkable = x > 0 && grid.GetValue(x - 1, y)?.IsWalkable() == true && !cliffNodes.Contains(new int2(x - 1, y));
                    bool rightIsWalkable = x < grid.GetWidth() - 1 && grid.GetValue(x + 1, y)?.IsWalkable() == true && !cliffNodes.Contains(new int2(x + 1, y));

                    // If true, the current node being checked is considered a cliff.
                    if (leftIsWalkable || rightIsWalkable)
                    {
                        // Mark this node as walkable and as a cliff.
                        grid.GetValue(x, y)?.SetIsWalkable(true);
                        grid.GetValue(x, y)?.SetIsCliff(true);
                        cliffNodes.Add(new int2(x, y));

                        //// Mark all cells below as walkable until a ground node.
                        for (int z = y - 1; z >= 0; z--)
                        {
                            if (blockedNodes[x, z]) break;
                            grid.GetValue(x, z)?.SetIsWalkable(true);
                            cliffNodes.Add(new int2(x, z));
                        }
                    }
                }
            }
        }

        // Fill the gap between cliffs.
        List<int2> allCliffs = new List<int2>();
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                var node = grid.GetValue(x, y);
                if (node != null && node.IsCliff())
                {
                    allCliffs.Add(new int2(x, y));
                }
            }
        }

        // Sort cliffs left-to-right.
        allCliffs.Sort((a, b) => a.x.CompareTo(b.x));

        for (int i = 0; i < allCliffs.Count - 1; i++)
        {
            int2 left = allCliffs[i];
            int2 right = allCliffs[i + 1];

            int minX = left.x + 1;
            int maxX = right.x - 1;

            if (minX > maxX) continue;

            int minY = Mathf.Min(left.y, right.y);
            int maxY = Mathf.Max(left.y, right.y);

            bool validGap = true;

            // Check if everything between cliffs is empty air.
            for (int xGap = minX; xGap <= maxX && validGap; xGap++)
            {
                for (int yGap = minY; yGap <= maxY && validGap; yGap++)
                {
                    if (blockedNodes[xGap, yGap]) validGap = false;

                    if (yGap > 0)
                    {
                        if (blockedNodes[xGap, yGap - 1] || grid.GetValue(xGap, yGap - 1)?.IsWalkable() == true)
                        {
                            validGap = false;
                        }
                    }
                }
            }

            if (!validGap) continue;

            // Fill the gap with walkable cliff nodes.
            for (int xGap = minX; xGap <= maxX; xGap++)
            {
                for (int yGap = minY; yGap <= maxY; yGap++)
                {
                    var node = grid.GetValue(xGap, yGap);
                    if (node != null && !blockedNodes[xGap, yGap])
                    {
                        node.SetIsWalkable(true);
                        cliffNodes.Add(new int2(xGap, yGap));

                        // Fill downward until hitting ground.
                        for (int z = yGap - 1; z >= 0; z--)
                        {
                            if (blockedNodes[xGap, z]) break;
                            grid.GetValue(xGap, z)?.SetIsWalkable(true);
                            cliffNodes.Add(new int2(xGap, z));
                        }
                    }
                }
            }
        }

        cliffNodes.Clear();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                if (grid.GetValue(x, y).IsCliff())
                {
                    cliffNodes.Add(new int2(x, y));
                }
            }
        }

        // Mark only the first walkable node directly below each cliff as a cliff, avoid errors when jumping from a node to a lower one.
        foreach (int2 cliff in cliffNodes)
        {
            if (cliff.y - 1 >= 0)
            {
                var belowNode = grid.GetValue(cliff.x, cliff.y - 1);

                if (belowNode != null && belowNode.IsWalkable() && !belowNode.IsCliff())
                {
                    belowNode.SetIsCliff(true);
                }
            }
        }
    }
}

