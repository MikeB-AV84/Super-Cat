using UnityEngine;
using UnityEngine.SceneManagement; // Required for restarting the scene

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float initialGameSpeed = 5f;
    public float speedIncrement = 0.5f;
    public int scoreToIncreaseSpeed = 500;
    public float maxGameSpeed = 20f; // Optional: Cap the speed

    private float _currentGameSpeed;
    public float CurrentGameSpeed => _currentGameSpeed;

    private bool _isGameOver = false;
    private int _lastSpeedIncreaseScore = 0;

    public UIManager uiManager; // Assign in Inspector

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (uiManager == null) Debug.LogError("UIManager not assigned to GameManager!");
        if (ScoreManager.Instance == null) Debug.LogError("ScoreManager not found!");
        if (HealthManager.Instance == null) Debug.LogError("HealthManager not found!");

        // Subscribe to events
        ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
        HealthManager.Instance.OnPlayerDied += HandlePlayerDied;

        StartGame();
    }

    void OnDestroy() // Unsubscribe when object is destroyed
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
        if (HealthManager.Instance != null) HealthManager.Instance.OnPlayerDied -= HandlePlayerDied;
    }


    public void StartGame()
    {
        _isGameOver = false;
        _currentGameSpeed = initialGameSpeed;
        _lastSpeedIncreaseScore = 0;

        Time.timeScale = 1f; // Ensure game is running

        // Reset managers (they should have their own reset methods)
        HealthManager.Instance?.ResetHealth();
        ScoreManager.Instance?.ResetScore();

        uiManager?.HideGameOverScreen();
        uiManager?.UpdateScore(0); // Ensure UI is correct at start
        uiManager?.UpdateHearts(HealthManager.Instance.maxHealth);

        // Tell Spawner to start spawning (Spawner script will handle its own logic)
        ObjectSpawner.Instance?.StartSpawning();
    }

    void Update()
    {
        if (_isGameOver && Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    void HandleScoreChanged(int newScore)
    {
        uiManager?.UpdateScore(newScore);
        // Difficulty Scaling
        if (newScore >= _lastSpeedIncreaseScore + scoreToIncreaseSpeed)
        {
            _lastSpeedIncreaseScore += scoreToIncreaseSpeed;
            IncreaseGameSpeed();
        }
    }

    void IncreaseGameSpeed()
    {
        _currentGameSpeed += speedIncrement;
        _currentGameSpeed = Mathf.Min(_currentGameSpeed, maxGameSpeed); // Clamp to max speed
        Debug.Log("Speed Increased to: " + _currentGameSpeed);
        // Optional: Play sound/visual cue for speed increase
    }

    void HandlePlayerDied()
    {
        _isGameOver = true;
        Time.timeScale = 0f; // Pause the game
        uiManager?.ShowGameOverScreen(ScoreManager.Instance.CurrentScore);
        Debug.Log("Game Over! Final Score: " + ScoreManager.Instance.CurrentScore);
        ObjectSpawner.Instance?.StopSpawning();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Important: Reset time scale before loading scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // Note: A more robust restart might involve resetting states without reloading the scene,
        // but scene reload is simpler for a basic setup.
    }

    public bool IsGameOver()
    {
        return _isGameOver;
    }
}