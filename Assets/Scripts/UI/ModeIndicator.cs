using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows current mode (BUILD/REMOVE) and selected building in top-left corner.
/// </summary>
public class ModeIndicator : MonoBehaviour
{
    [SerializeField] private Text _modeText;

    private void Update()
    {
        if (_modeText == null) return;

        BuildingPlacer placer = BuildingPlacer.Instance;
        if (placer == null) return;

        if (placer.RemoveMode)
        {
            _modeText.text = "REMOVE MODE [X]";
            _modeText.color = new Color(1f, 0.3f, 0.3f);
        }
        else if (placer.SelectedType.HasValue)
        {
            string name = placer.SelectedType.Value.ToString();
            string facing = placer.CurrentFacing.ToString();
            _modeText.text = $"BUILD: {name} [{facing}] [R=Rotate]";
            _modeText.color = new Color(0.3f, 1f, 0.3f);
        }
        else
        {
            _modeText.text = "SELECT [1-5] or [X=Remove]";
            _modeText.color = Color.white;
        }
    }
}
