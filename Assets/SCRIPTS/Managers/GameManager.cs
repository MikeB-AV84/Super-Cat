using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Speed Settings")]
    public float initialGameSpeed = 5f;
    public float speedIncrement = 0.5f;
    public int scoreToIncreaseSpeed = 500;
    public float maxGameSpeed = 20f;

    [Header("Scene Navigation")]
    public string mainMenuSceneName = "MainMenuScene"; // Ensure this scene is in Build Settings

    private float _currentGameSpeed;
    public float CurrentGameSpeed => _currentGameSpeed;

    private bool _isGameOver = false;
    private bool _isPaused = false; // New state for pausing
    public bool IsPaused => _isPaused; // Public getter

    private int _lastSpeedIncreaseScore = 0;

    public UIManager uiManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional: if GameManager persists across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (uiManager == null) Debug.LogError("GameManager: UIManager not assigned in Inspector!", this.gameObject);
        // Initialize other managers and subscribe to events (as before)
        if (ScoreManager.Instance == null) Debug.LogError("GameManager: ScoreManager not found!", this.gameObject);
        if (HealthManager.Instance == null) Debug.LogError("GameManager: HealthManager not found!", this.gameObject);

        if (ScoreManager.Instance != null) ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
        if (HealthManager.Instance != null) HealthManager.Instance.OnPlayerDied += HandlePlayerDied;

        // Ensure game is not paused at start (Time.timeScale might persist from editor or previous scene if not reset)
        Time.timeScale = 1f;
        _isPaused = false; // Explicitly set paused to false
        uiManager?.SetPauseMenu(false); // Ensure pause menu is hidden

        StartGame();
    }

    void Update()
    {
        // Handle Pause Input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Don't allow pausing if game is already over
            if (!_isGameOver)
            {
                TogglePauseGame();
            }
        }

        // Handle Restart Input (only if game is over)
        if (_isGameOver && Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (ScoreManager.Instance != null) ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
        if (HealthManager.Instance != null) HealthManager.Instance.OnPlayerDied -= HandlePlayerDied;
    }

    public void StartGame()
    {
        _isGameOver = false;
        // _isPaused should already be false here from Start() or TogglePauseGame()
        _currentGameSpeed = initialGameSpeed;
        _lastSpeedIncreaseScore = 0;

        Time.timeScale = 1f; // Ensure game is running

        HealthManager.Instance?.ResetHealth();
        ScoreManager.Instance?.ResetScore();

        uiManager?.HideGameOverScreen();
        uiManager?.SetPauseMenu(false); // Ensure pause menu is hidden at game start/restart
        if (ScoreManager.Instance != null) uiManager?.UpdateScore(ScoreManager.Instance.CurrentScore);
        if (HealthManager.Instance != null) uiManager?.UpdateHearts(HealthManager.Instance.maxHealth);

        ObjectSpawner.Instance?.StartSpawning();
    }

    void HandleScoreChanged(int newScore)
    {
        if (uiManager != null) uiManager.UpdateScore(newScore);
        if (newScore >= _lastSpeedIncreaseScore + scoreToIncreaseSpeed)
        {
            _lastSpeedIncreaseScore += scoreToIncreaseSpeed;
            IncreaseGameSpeed();
        }
    }

    void IncreaseGameSpeed()
    {
        _currentGameSpeed += speedIncrement;
        _currentGameSpeed = Mathf.Min(_currentGameSpeed, maxGameSpeed);
        // Debug.Log("Speed Increased to: " + _currentGameSpeed);
    }

    void HandlePlayerDied()
    {
        _isGameOver = true;
        Time.timeScale = 0f; // Pause game on death
        uiManager?.ShowGameOverScreen(ScoreManager.Instance.CurrentScore);
        ObjectSpawner.Instance?.StopSpawning();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // --- Pause Logic ---
    public void TogglePauseGame()
    {
        _isPaused = !_isPaused;

        if (_isPaused)
        {
            Time.timeScale = 0f; // Freeze game time
            uiManager?.SetPauseMenu(true);
            // Optionally disable player input here if not handled by Time.timeScale
        }
        else
        {
            ResumeGame(); // Call ResumeGame to ensure UI is also handled
        }
    }

    public void ResumeGame() // Public method for Resume button
    {
        _isPaused = false;
        Time.timeScale = 1f; // Unfreeze game time
        uiManager?.SetPauseMenu(false);
        // Optionally re-enable player input if disabled separately
    }

    public void QuitToMainMenu() // Public method for Quit button
    {
        Time.timeScale = 1f; // IMPORTANT: Always reset time scale before loading a new scene
        _isPaused = false; // Reset pause state
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public bool IsGameOver() => _isGameOver;
}
