using UnityEngine;
using System; // For Action

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int _currentScore;
    public int CurrentScore => _currentScore;

    public event Action<int> OnScoreChanged; // For UI and GameManager

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ResetScore();
    }

    public void ResetScore()
    {
        _currentScore = 0;
        OnScoreChanged?.Invoke(_currentScore);
    }

    public void AddScore(int amount)
    {
        _currentScore += amount;
        OnScoreChanged?.Invoke(_currentScore);
    }
}