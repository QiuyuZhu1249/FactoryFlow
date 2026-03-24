using UnityEngine;

/// <summary>
/// Tracks player money. Ore collected = $1 each.
/// </summary>
public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    public int Money { get; private set; } = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        Money += amount;
        Debug.Log($"[MoneyManager] +${amount}. Total: ${Money}");
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return false;
        if (Money < amount)
        {
            Debug.LogWarning($"[MoneyManager] Not enough money. Have: ${Money}, Need: ${amount}");
            return false;
        }
        Money -= amount;
        Debug.Log($"[MoneyManager] -${amount}. Remaining: ${Money}");
        return true;
    }

    public void ResetMoney()
    {
        Money = 0;
    }
}
