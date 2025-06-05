using UnityEngine;
using UnityEngine.UI;     // For Image/RawImage, Button
using UnityEngine.Video;  // Required for VideoPlayer
using TMPro;              // For TextMeshPro elements
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [Header("Game UI Elements")]
    public TextMeshProUGUI scoreText;
    public Image[] heartIcons;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    [Tooltip("The RawImage UI element used to display the game over video.")]
    public RawImage gameOverVideoDisplay; // Assign the RawImage for video output
    [Tooltip("The VideoPlayer component that will play the game over video.")]
    public VideoPlayer gameOverVideoPlayer; // Assign the VideoPlayer component

    [Header("Pause Menu UI")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button quitToMenuButton;

    private Vector3 initialResumeButtonScale;
    private Vector3 initialQuitToMenuButtonScale;
    public float hoverScaleMultiplier = 1.1f;

    void Awake()
    {
        // Null checks for existing UI elements...
        if (scoreText == null) Debug.LogError("UIManager: ScoreText not assigned!", this.gameObject);
        if (gameOverPanel == null) Debug.LogError("UIManager: GameOverPanel not assigned!", this.gameObject);
        if (finalScoreText == null) Debug.LogError("UIManager: FinalScoreText not assigned!", this.gameObject);
        if (heartIcons == null || heartIcons.Length == 0)
        {
            Debug.LogWarning("UIManager: HeartIcons array is not assigned or is empty.", this.gameObject);
        }
        if (pauseMenuPanel == null) Debug.LogError("UIManager: PauseMenuPanel not assigned!", this.gameObject);
        if (resumeButton == null) Debug.LogError("UIManager: ResumeButton not assigned!", this.gameObject);
        if (quitToMenuButton == null) Debug.LogError("UIManager: QuitToMenuButton not assigned!", this.gameObject);

        // Null checks for new video elements
        if (gameOverVideoDisplay == null) Debug.LogError("UIManager: GameOverVideoDisplay (RawImage) not assigned!", this.gameObject);
        if (gameOverVideoPlayer == null) Debug.LogError("UIManager: GameOverVideoPlayer not assigned!", this.gameObject);
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        // Ensure video is not playing and its display is off if the panel is initially hidden
        if (gameOverVideoPlayer != null && gameOverVideoPlayer.isPlaying)
        {
            gameOverVideoPlayer.Stop();
        }
        if (gameOverVideoDisplay != null)
        {
            // Hide the RawImage if the VideoPlayer isn't ready or panel is hidden
            // You might want to control its enabled state along with the gameOverPanel
             gameOverVideoDisplay.enabled = false;
        }


        if (resumeButton != null && resumeButton.transform is RectTransform)
        {
            initialResumeButtonScale = resumeButton.transform.localScale;
        }
        if (quitToMenuButton != null && quitToMenuButton.transform is RectTransform)
        {
            initialQuitToMenuButtonScale = quitToMenuButton.transform.localScale;
        }

        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnHealthChanged += UpdateHearts;
            UpdateHearts(HealthManager.Instance.CurrentHealth);
        }
        else
        {
            Debug.LogError("UIManager: HealthManager.Instance is null at Start!", this.gameObject);
        }

        if (ScoreManager.Instance != null && scoreText != null) {
             UpdateScore(ScoreManager.Instance.CurrentScore);
        }
    }

    void OnDestroy()
    {
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnHealthChanged -= UpdateHearts;
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void UpdateHearts(int currentHealth)
    {
        if (heartIcons != null)
        {
            for (int i = 0; i < heartIcons.Length; i++)
            {
                if (heartIcons[i] != null)
                {
                    heartIcons[i].enabled = (i < currentHealth);
                }
            }
        }
    }

    public void ShowGameOverScreen(int finalScore)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null) finalScoreText.text = "Final Score: " + finalScore;

            // Handle Video Playback
            if (gameOverVideoPlayer != null && gameOverVideoDisplay != null)
            {
                if (gameOverVideoPlayer.clip != null)
                {
                    gameOverVideoDisplay.enabled = true; // Make sure RawImage is visible
                    gameOverVideoPlayer.Prepare(); // Prepare the video
                    gameOverVideoPlayer.prepareCompleted += OnGameOverVideoPrepared; // Play when prepared
                }
                else
                {
                    Debug.LogWarning("UIManager: No Video Clip assigned to GameOverVideoPlayer.", gameOverVideoPlayer);
                    gameOverVideoDisplay.enabled = false; // Hide if no clip
                }
            }
        }
    }

    private void OnGameOverVideoPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnGameOverVideoPrepared; // Unsubscribe
        if (gameOverPanel.activeSelf) // Only play if panel is still supposed to be active
        {
            vp.Play();
            Debug.Log("Game Over video started playing.");
        }
    }

    public void HideGameOverScreen() // Called by GameManager on RestartGame for example
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Stop and hide video elements
        if (gameOverVideoPlayer != null)
        {
            gameOverVideoPlayer.Stop();
            // Unsubscribe in case it was preparing but didn't finish
            gameOverVideoPlayer.prepareCompleted -= OnGameOverVideoPrepared;
        }
        if (gameOverVideoDisplay != null)
        {
            gameOverVideoDisplay.enabled = false;
        }
    }

    // --- Pause Menu Methods ---
    public void SetPauseMenu(bool isActive)
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isActive);
        }
    }

    public void OnResumeButtonPressed()
    {
        GameManager.Instance?.ResumeGame();
    }

    public void OnQuitToMenuButtonPressed()
    {
        GameManager.Instance?.QuitToMainMenu();
    }

    // --- Pause Menu Button Hover Effects ---
    public void OnResumeButtonHoverEnter()
    {
        if (resumeButton != null && resumeButton.transform is RectTransform)
        {
            resumeButton.transform.localScale = initialResumeButtonScale * hoverScaleMultiplier;
        }
    }
    public void OnResumeButtonHoverExit()
    {
        if (resumeButton != null && resumeButton.transform is RectTransform)
        {
            resumeButton.transform.localScale = initialResumeButtonScale;
        }
    }
    public void OnQuitToMenuButtonHoverEnter()
    {
        if (quitToMenuButton != null && quitToMenuButton.transform is RectTransform)
        {
            quitToMenuButton.transform.localScale = initialQuitToMenuButtonScale * hoverScaleMultiplier;
        }
    }
    public void OnQuitToMenuButtonHoverExit()
    {
        if (quitToMenuButton != null && quitToMenuButton.transform is RectTransform)
        {
            quitToMenuButton.transform.localScale = initialQuitToMenuButtonScale;
        }
    }
}
