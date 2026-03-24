using UnityEngine;

public class DebugUI : MonoBehaviour
{
    private ResourceManager _resourceManager;

    private void Start()
    {
        _resourceManager = GameManager.Instance.ResourceManager;
    }

    private void Update()
    {
        // Press 1-4 to add resources for testing
        if (Input.GetKeyDown(KeyCode.Alpha1))
            _resourceManager.AddResource(ItemType.IronOre, 1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            _resourceManager.AddResource(ItemType.CopperOre, 1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            _resourceManager.AddResource(ItemType.Coal, 1);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            _resourceManager.AddResource(ItemType.Stone, 1);

        // Press P to toggle pause
        if (Input.GetKeyDown(KeyCode.P))
            GameManager.Instance.TogglePause();

        // Press R to reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Instance.ResetGame();
        }
    }

    private void OnGUI()
    {
        if (_resourceManager == null) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;

        float x = 10f;
        float y = 10f;
        float lineHeight = 25f;

        // Game state
        GUI.Label(new Rect(x, y, 400, lineHeight),
            $"State: {GameManager.Instance.CurrentState}", style);
        y += lineHeight;

        // Resource counts
        GUI.Label(new Rect(x, y, 400, lineHeight),
            $"Iron: {_resourceManager.GetResourceCount(ItemType.IronOre)}", style);
        y += lineHeight;
        GUI.Label(new Rect(x, y, 400, lineHeight),
            $"Copper: {_resourceManager.GetResourceCount(ItemType.CopperOre)}", style);
        y += lineHeight;
        GUI.Label(new Rect(x, y, 400, lineHeight),
            $"Coal: {_resourceManager.GetResourceCount(ItemType.Coal)}", style);
        y += lineHeight;
        GUI.Label(new Rect(x, y, 400, lineHeight),
            $"Stone: {_resourceManager.GetResourceCount(ItemType.Stone)}", style);
        y += lineHeight * 1.5f;

        // Controls hint
        style.fontSize = 14;
        style.fontStyle = FontStyle.Normal;
        style.normal.textColor = Color.yellow;
        GUI.Label(new Rect(x, y, 400, lineHeight),
            "[1-4] Add Iron/Copper/Coal/Stone", style);
        y += lineHeight;
        GUI.Label(new Rect(x, y, 400, lineHeight),
            "[P] Pause/Resume  [R] Reset", style);
        y += lineHeight;
        GUI.Label(new Rect(x, y, 400, lineHeight),
            "[Left Click] Occupy Grid  [Right Click] Free Grid", style);
    }
}
