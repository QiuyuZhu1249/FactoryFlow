using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays current money in the top-right corner.
/// </summary>
public class MoneyDisplay : MonoBehaviour
{
    [SerializeField] private Text _moneyText;

    private void Update()
    {
        if (_moneyText == null || MoneyManager.Instance == null) return;
        _moneyText.text = $"$ {MoneyManager.Instance.Money}";
    }
}
