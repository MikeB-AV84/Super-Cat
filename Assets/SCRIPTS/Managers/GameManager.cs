using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Speed Settings")]
    public float initialGameSpeed = 5f;
    public float speedIncrement = 0.5f;
    public int scoreToIncreaseSpeed = 500;
    public float maxGameSpeed = 20f;

    [Header("Scene Navigation")]
    public string mainMenuSceneName = "MainMenuScene";

    private float _currentGameSpeed;
    public float CurrentGameSpeed => _currentGameSpeed;

    private bool _isGameOver = false;
    private bool _isPaused = false;
    public bool IsPaused => _isPaused;

    private int _lastSpeedIncreaseScore = 0;

    public UIManager uiManager;
    // No need for public SimpleAudioManager reference if using Singleton

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (uiManager == null) Debug.LogError("GameManager: UIManager not assigned in Inspector!", this.gameObject);
        if (ScoreManager.Instance == null) Debug.LogError("GameManager: ScoreManager not found!", this.gameObject);
        if (HealthManager.Instance == null) Debug.LogError("GameManager: HealthManager not found!", this.gameObject);

        if (ScoreManager.Instance != null) ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
        if (HealthManager.Instance != null) HealthManager.Instance.OnPlayerDied += HandlePlayerDied;

        Time.timeScale = 1f;
        _isPaused = false;
        uiManager?.SetPauseMenu(false);

        // Start music when the game starts
        SimpleAudioManager.Instance?.PlayMusic();

        StartGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_isGameOver)
            {
                TogglePauseGame();
            }
        }

        if (_isGameOver && Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    void OnDestroy()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
        if (HealthManager.Instance != null) HealthManager.Instance.OnPlayerDied -= HandlePlayerDied;
    }

    public void StartGame()
    {
        _isGameOver = false;
        _currentGameSpeed = initialGameSpeed;
        _lastSpeedIncreaseScore = 0;

        Time.timeScale = 1f;

        HealthManager.Instance?.ResetHealth();
        ScoreManager.Instance?.ResetScore();

        uiManager?.HideGameOverScreen();
        uiManager?.SetPauseMenu(false);
        if (ScoreManager.Instance != null) uiManager?.UpdateScore(ScoreManager.Instance.CurrentScore);
        if (HealthManager.Instance != null) uiManager?.UpdateHearts(HealthManager.Instance.maxHealth);

        ObjectSpawner.Instance?.StartSpawning();

        // If restarting, ensure music plays (if it was stopped on game over)
        // SimpleAudioManager.Instance?.PlayMusic(); // Already handled in Start() for initial load.
                                                  // If AudioManager is DontDestroyOnLoad, this might be needed.
                                                  // Since it reloads with scene, its own Start will call PlayMusic.
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
    }

    void HandlePlayerDied()
    {
        _isGameOver = true;
        Time.timeScale = 0f;
        uiManager?.ShowGameOverScreen(ScoreManager.Instance.CurrentScore);
        ObjectSpawner.Instance?.StopSpawning();

        // Stop the music
        SimpleAudioManager.Instance?.StopMusic();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        // Music will restart automatically because SimpleAudioManager's Start will run on scene reload
        // (if it's not DontDestroyOnLoad and is part of the reloaded scene).
        // If you want music to start *before* this scene fully loads for the next session,
        // you might call PlayMusic() here, but it's usually handled by the AudioManager's own lifecycle.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TogglePauseGame()
    {
        _isPaused = !_isPaused;
        if (_isPaused)
        {
            Time.timeScale = 0f;
            uiManager?.SetPauseMenu(true);
            SimpleAudioManager.Instance?.audioSource.Pause(); // Pause AudioSource directly
        }
        else
        {
            Time.timeScale = 1f;
            uiManager?.SetPauseMenu(false);
            SimpleAudioManager.Instance?.audioSource.UnPause(); // UnPause AudioSource
        }
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        uiManager?.SetPauseMenu(false);
        SimpleAudioManager.Instance?.audioSource.UnPause(); // Ensure music resumes from pause
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        // Optionally stop music here if the main menu has its own,
        // or if AudioManager is DontDestroyOnLoad and you want a clean slate.
        // SimpleAudioManager.Instance?.StopMusic();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public bool IsGameOver() => _isGameOver;
}
