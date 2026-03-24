using UnityEngine;

public class GridTester : MonoBehaviour
{
    private Camera mainCamera;
    private GridSystem grid;

    void Start()
    {
        mainCamera = Camera.main;
        grid = GridSystem.Instance;

        if (grid == null)
        {
            Debug.LogError("Error!");
        }
    }

    void Update()
    {
        // Use the left mouse button to occupy the squares in red
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            Vector2Int gridPos = grid.WorldToGrid(mousePos);

            if (grid.IsCellAvailable(gridPos))
            {
                grid.OccupyCell(gridPos);
                Debug.Log($"Occupying the grid cells: [{gridPos.x}, {gridPos.y}]");
            }
            else
            {
                Debug.Log($"The grid [{gridPos.x}, {gridPos.y}] is already occupied.");
            }
        }

        // Right-click to cancel the previous occupation
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            Vector2Int gridPos = grid.WorldToGrid(mousePos);

            if (grid.IsCellInBounds(gridPos))
            {
                grid.FreeCell(gridPos);
                Debug.Log($"Cancel the occupation: [{gridPos.x}, {gridPos.y}]");
            }
        }
    }

    // Display a cross in the grid that the mouse has moved over
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || grid == null) return;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector2Int gridPos = grid.WorldToGrid(mousePos);
        Vector3 worldCenter = grid.GridToWorld(gridPos);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(worldCenter + Vector3.left * 0.3f, worldCenter + Vector3.right * 0.3f);
        Gizmos.DrawLine(worldCenter + Vector3.down * 0.3f, worldCenter + Vector3.up * 0.3f);
    }
}
