using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bottom toolbar with 10 building slots.
/// First 5 mapped to Miner/Conveyor/Splitter/Merger/ProductionStation.
/// Keyboard 1-5 or mouse click to select.
/// </summary>
public class ToolbarUI : MonoBehaviour
{
    [Header("Slot Images")]
    [SerializeField] private Image[] _slotBackgrounds = new Image[10];
    [SerializeField] private Image[] _slotIcons = new Image[10];
    [SerializeField] private Text[] _slotLabels = new Text[10];

    private Color _normalColor = new Color(0.2f, 0.2f, 0.25f, 0.8f);
    private Color _selectedColor = new Color(0.9f, 0.7f, 0.1f, 0.9f);
    private Color _emptyColor = new Color(0.15f, 0.15f, 0.18f, 0.5f);

    private string[] _slotNames = {
        "Miner", "Conveyor", "Splitter", "Merger", "Station",
        "", "", "", "", ""
    };

    private Color[] _slotColors = {
        new Color(0.6f, 0.4f, 0.2f),   // Miner - brown
        new Color(0.4f, 0.4f, 0.45f),  // Conveyor - gray
        new Color(0.2f, 0.4f, 0.7f),   // Splitter - blue
        new Color(0.2f, 0.6f, 0.3f),   // Merger - green
        new Color(0.7f, 0.5f, 0.1f),   // Production - yellow
        Color.clear, Color.clear, Color.clear, Color.clear, Color.clear
    };

    private void Update()
    {
        UpdateSlotHighlights();
    }

    private void UpdateSlotHighlights()
    {
        BuildingPlacer placer = BuildingPlacer.Instance;
        if (placer == null) return;

        for (int i = 0; i < 10; i++)
        {
            if (_slotBackgrounds[i] == null) continue;

            bool isSelected = false;
            if (!placer.RemoveMode && placer.SelectedType.HasValue)
            {
                isSelected = (int)placer.SelectedType.Value == i;
            }

            if (i < 5)
                _slotBackgrounds[i].color = isSelected ? _selectedColor : _normalColor;
            else
                _slotBackgrounds[i].color = _emptyColor;
        }
    }

    public void OnSlotClicked(int index)
    {
        if (index < 0 || index > 4) return;
        BuildingPlacer.Instance?.SelectBuilding((BuildingType)index);
    }
}
