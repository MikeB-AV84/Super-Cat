using UnityEngine;
using System; // For Action

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    public int maxHealth = 3;
    private int _currentHealth;

    public int CurrentHealth => _currentHealth; // Public getter

    public event Action<int> OnHealthChanged; // For UI updates
    public event Action OnPlayerDied;       // For GameManager

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        _currentHealth = maxHealth;
        OnHealthChanged?.Invoke(_currentHealth);
    }

    public void TakeDamage(int amount)
    {
        if (_currentHealth <= 0) return; // Already dead

        _currentHealth -= amount;
        _currentHealth = Mathf.Max(0, _currentHealth); // Don't go below 0
        OnHealthChanged?.Invoke(_currentHealth);
        // Optional: Play damage sound/visual effect

        if (_currentHealth <= 0)
        {
            OnPlayerDied?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (_currentHealth <= 0) return; // Cannot heal if dead

        _currentHealth += amount;
        _currentHealth = Mathf.Min(_currentHealth, maxHealth); // Don't exceed max health
        OnHealthChanged?.Invoke(_currentHealth);
        // Optional: Play heal sound/visual effect
    }
}