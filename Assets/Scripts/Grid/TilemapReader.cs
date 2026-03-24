using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Reads the Ground tilemap to determine ore type at grid positions.
/// Converts between GridSystem coordinates and Tilemap coordinates.
/// </summary>
public class TilemapReader : MonoBehaviour
{
    public static TilemapReader Instance { get; private set; }

    [Header("Tilemap Reference")]
    [SerializeField] private Tilemap _groundTilemap;

    [Header("Tile Assets")]
    [SerializeField] private TileBase _ironOreTile;
    [SerializeField] private TileBase _copperOreTile;
    [SerializeField] private TileBase _coalTile;
    [SerializeField] private TileBase _stoneTile;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Converts GridSystem coordinates to Tilemap cell coordinates.
    /// GridSystem origin at (-gridWidth/2, -gridHeight/2), cellSize 1.
    /// For 30x20 grid: grid(0,0) = world(-14.5, -9.5) = tilemap(-15, -10).
    /// </summary>
    private Vector3Int GridToTilemapPos(Vector2Int gridPos)
    {
        int gridWidth = GridSystem.Instance.gridWidth;
        int gridHeight = GridSystem.Instance.gridHeight;
        int offsetX = gridWidth / 2;
        int offsetY = gridHeight / 2;
        return new Vector3Int(gridPos.x - offsetX, gridPos.y - offsetY, 0);
    }

    /// <summary>
    /// Returns the ore type at the given grid position.
    /// Returns ItemType.None if no ore tile exists.
    /// </summary>
    public ItemType GetOreTypeAt(Vector2Int gridPos)
    {
        if (_groundTilemap == null)
        {
            Debug.LogWarning("[TilemapReader] Ground tilemap not assigned.");
            return ItemType.None;
        }

        Vector3Int tilemapPos = GridToTilemapPos(gridPos);
        TileBase tile = _groundTilemap.GetTile(tilemapPos);

        if (tile == null) return ItemType.None;
        if (tile == _ironOreTile)   return ItemType.IronOre;
        if (tile == _copperOreTile) return ItemType.CopperOre;
        if (tile == _coalTile)      return ItemType.Coal;
        if (tile == _stoneTile)     return ItemType.Stone;

        return ItemType.None;
    }

    /// <summary>
    /// Returns true if there is any ore tile at the given grid position.
    /// </summary>
    public bool HasOre(Vector2Int gridPos)
    {
        return GetOreTypeAt(gridPos) != ItemType.None;
    }
}
