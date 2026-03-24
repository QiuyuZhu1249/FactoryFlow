using UnityEngine;

public enum GameState
{
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("References")]
    [SerializeField] private GridSystem _gridSystem;
    [SerializeField] private ResourceManager _resourceManager;
    [SerializeField] private ResourceFactory _resourceFactory;
    [SerializeField] private MoneyManager _moneyManager;
    [SerializeField] private TilemapReader _tilemapReader;
    [SerializeField] private BuildingPlacer _buildingPlacer;

    public GridSystem GridSystem => _gridSystem;
    public ResourceManager ResourceManager => _resourceManager;
    public ResourceFactory ResourceFactory => _resourceFactory;
    public MoneyManager MoneyManager => _moneyManager;
    public TilemapReader TilemapReader => _tilemapReader;
    public BuildingPlacer BuildingPlacer => _buildingPlacer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SpawnResourceCollector();
    }

    /// <summary>
    /// Spawns the resource collector at the center of the grid.
    /// </summary>
    private void SpawnResourceCollector()
    {
        int centerX = _gridSystem.gridWidth / 2;
        int centerY = _gridSystem.gridHeight / 2;
        Vector2Int centerPos = new Vector2Int(centerX, centerY);

        GameObject collectorObj = new GameObject("ResourceCollector");
        SpriteRenderer sr = collectorObj.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.GetCollector();
        sr.sortingOrder = 5;

        ResourceCollector collector = collectorObj.AddComponent<ResourceCollector>();
        collector.Initialize(centerPos, Direction.Up);

        Debug.Log($"[GameManager] Resource Collector spawned at grid {centerPos}.");
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] State changed to: {newState}");

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
        }
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
            SetState(GameState.Paused);
        else if (CurrentState == GameState.Paused)
            SetState(GameState.Playing);
    }

    public void ResetGame()
    {
        BuildingRegistry.Clear();
        SetState(GameState.Playing);
        Debug.Log("[GameManager] Game reset.");
    }
}
