using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public static GridSystem Instance;

    [Header("Grid set")]
    public float cellSize = 1f;
    public int gridWidth = 50;
    public int gridHeight = 50;
    public bool showGrid = true;

    private bool[,] occupiedCells;
    private Vector3 origin;

    void Awake()
    {
        Instance = this;
        occupiedCells = new bool[gridWidth, gridHeight];

        float startX = -(gridWidth * cellSize) / 2f;
        float startY = -(gridHeight * cellSize) / 2f;
        origin = new Vector3(startX, startY, 0);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float offsetX = worldPos.x - origin.x;
        float offsetY = worldPos.y - origin.y;

        int x = Mathf.FloorToInt(offsetX / cellSize);
        int y = Mathf.FloorToInt(offsetY / cellSize);

        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        float x = origin.x + (gridPos.x * cellSize) + cellSize / 2f;
        float y = origin.y + (gridPos.y * cellSize) + cellSize / 2f;
        return new Vector3(x, y, 0);
    }

    public Vector3 GridToWorldCorner(Vector2Int gridPos)
    {
        float x = origin.x + (gridPos.x * cellSize);
        float y = origin.y + (gridPos.y * cellSize);
        return new Vector3(x, y, 0);
    }

    // Check if the grid is usable
    public bool IsCellAvailable(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= gridWidth ||
            gridPos.y < 0 || gridPos.y >= gridHeight)
        {
            return false;
        }
        return !occupiedCells[gridPos.x, gridPos.y];
    }

    // Check if the grid is within the grid boundaries
    public bool IsCellInBounds(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth &&
               gridPos.y >= 0 && gridPos.y < gridHeight;
    }

    // Occupied squares
    public void OccupyCell(Vector2Int gridPos)
    {
        if (IsCellInBounds(gridPos))
        {
            occupiedCells[gridPos.x, gridPos.y] = true;
        }
    }

    // Cancel the occupation
    public void FreeCell(Vector2Int gridPos)
    {
        if (IsCellInBounds(gridPos))
        {
            occupiedCells[gridPos.x, gridPos.y] = false;
        }
    }

    // Visualized grid
    void OnDrawGizmos()
    {
        if (!showGrid || !Application.isPlaying)
            return;

        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        for (int x = 0; x <= gridWidth; x++)
        {
            for (int y = 0; y <= gridHeight; y++)
            {
                Vector3 corner = GridToWorldCorner(new Vector2Int(x, y));

                if (y < gridHeight)
                {
                    Vector3 end = GridToWorldCorner(new Vector2Int(x, y + 1));
                    Gizmos.DrawLine(corner, end);
                }

                if (x < gridWidth)
                {
                    Vector3 end = GridToWorldCorner(new Vector2Int(x + 1, y));
                    Gizmos.DrawLine(corner, end);
                }
            }
        }

        // Mark the occupied squares
        if (occupiedCells != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (occupiedCells[x, y])
                    {
                        Vector3 center = GridToWorld(new Vector2Int(x, y));
                        Gizmos.color = new Color(1, 0, 0, 0.3f);
                        Gizmos.DrawCube(center, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0));
                    }
                }
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(origin, 0.2f);
    }
}
