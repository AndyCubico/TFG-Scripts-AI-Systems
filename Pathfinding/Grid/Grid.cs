using System;
using UnityEngine;

public class Grid<T>
{
    // Event to adjust a cell of the grid at any given time, mostly for debug. Example: make a node not walkable.
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;

    public class OnGridObjectChangedEventArgs
    {
        public int x, y;
    }

    private int m_Width, m_Height;
    private float m_CellSize;
    private Vector2 m_OriginPosition; // Starting point of the grid.
    private T[,] m_GridArray; // Array that stores each <T> node (almost always will be a GridNode)

    // Debug draw cell position in the grid.
    private TextMesh[,] m_DebugTextArray;
    private bool m_IsDebug;

    /// <summary>
    /// Contructor of a grid of <T> nodes.
    /// </summary>
    /// <param name="width"> Width of grid (x). </param>
    /// <param name="height"> Height of grid (y). </param>
    /// <param name="cellSize"> Width and height of each cell (squares). </param>
    /// <param name="originPosition"> Starting position in the world from which to start drawing the grid. </param>
    /// <param name="createGridObject"> Delegate to create the grid object in each cell. </param>
    public Grid(int width, int height, float cellSize, Vector2 originPosition, Func<Grid<T>, int, int, T> createGridObject, bool isDebug = false)
    {
        m_Width = width;
        m_Height = height;
        m_CellSize = cellSize;
        m_OriginPosition = originPosition;

        m_GridArray = new T[width, height];

        m_DebugTextArray = new TextMesh[width, height];

        m_IsDebug = isDebug;

        GameObject debugTextContainer = new GameObject("Grid Debug Text Container");

        for (int x = 0; x < m_GridArray.GetLength(0); x++)
        {
            for (int y = 0; y < m_GridArray.GetLength(1); y++)
            {
                // Create and assign the grid object first
                m_GridArray[x, y] = createGridObject(this, x, y);

                if (m_IsDebug)
                {
                    m_DebugTextArray[x, y] = CreateDisplayText($"{x}, {y}", debugTextContainer.transform, GetWorldPosition(x, y) + new Vector2(cellSize, cellSize) * .5f,
                                                                20, WalkableDebugColor(m_GridArray[x, y]));

                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }
        }

        if (m_IsDebug)
        {
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
        }
    }

    /// <summary>
    /// Event that modifies a given cell of the grid.
    /// </summary>
    /// <param name="x"> </param>
    /// <param name="y"></param>
    public void TriggerGridObjectChanged(int x, int y)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });

        if (m_IsDebug)
        {
            if (x >= 0 && y >= 0 && x < m_Width && y < m_Height)
            {
                m_DebugTextArray[x, y].text = $"{x}, {y}";
                m_DebugTextArray[x, y].color = WalkableDebugColor(m_GridArray[x, y]);
            }
        }
    }

    private Vector2 GetWorldPosition(int x, int y)
    {
        return new Vector2(x, y) * m_CellSize + m_OriginPosition;
    }

    public void GetXYPosition(Vector2 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - m_OriginPosition).x / m_CellSize);
        y = Mathf.FloorToInt((worldPosition - m_OriginPosition).y / m_CellSize);
    }

    public T GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < m_Width && y < m_Height)
        {
            return m_GridArray[x, y];
        }
        else
        {
            return default;
        }
    }

    public T GetValue(Vector2 worldPosition)
    {
        int x, y;
        GetXYPosition(worldPosition, out x, out y);
        return GetValue(x, y);
    }

    public int GetWidth()
    {
        return m_Width;
    }

    public int GetHeight()
    {
        return m_Height;
    }

    // Debug grid cell values
    private static TextMesh CreateDisplayText(string text = "", Transform parent = null, Vector2 localPosition = default, int fontSize = 10, Color color = default,
                                                TextAnchor textAnchor = TextAnchor.MiddleCenter, int sortingOrder = 500, TextAlignment textAlignment = TextAlignment.Center)
    {
        GameObject go = new GameObject("Display Text", typeof(TextMesh));
        Transform transform = go.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;

        transform.localScale = Vector2.one * 0.1f; // This makes the text readable at small sizes

        TextMesh textMesh = go.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.fontSize = fontSize;
        textMesh.text = text;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }

    // Change colour of the cell's text, white if walkbale black otherwise.
    private Color WalkableDebugColor(T gridObject)
    {
        if (gridObject is GridNode node)
        {
            return node.IsCliff() ? Color.red : node.IsWalkable() ? Color.white : Color.black;
        }

        return Color.white;
    }
}

public class GridNode
{
    private Grid<GridNode> m_Grid;
    private int m_X;
    private int m_Y;

    private bool m_IsWalkable;
    private bool m_IsCliff;

    public GridNode(Grid<GridNode> grid, int x, int y)
    {
        m_Grid = grid;
        m_X = x;
        m_Y = y;
        m_IsWalkable = false; // All nodes are not walkable from the start.
        m_IsCliff = false; // Flag to indicate nodes that are walkable but have to be jumped.
    }

    public bool IsWalkable()
    {
        return m_IsWalkable;
    }

    public void SetIsWalkable(bool isWalkable)
    {
        m_IsWalkable = isWalkable;
        m_Grid.TriggerGridObjectChanged(m_X, m_Y);
    }

    public bool IsCliff()
    {
        return m_IsCliff;
    }

    public void SetIsCliff(bool isCliff)
    {
        m_IsCliff = isCliff;
        m_Grid.TriggerGridObjectChanged(m_X, m_Y);
    }
}
