using UnityEngine;
using UnityEngine.UI;     // For Image/RawImage if used for hearts, and Button
using TMPro;              // For TextMeshPro elements
using UnityEngine.EventSystems; // For Event Triggers (hover effects)

public class UIManager : MonoBehaviour
{
    [Header("Game UI Elements")]
    public TextMeshProUGUI scoreText;
    public Image[] heartIcons; // Or RawImage[] if you used that for 3D hearts

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    [Header("Pause Menu UI")]
    public GameObject pauseMenuPanel;
    public Button resumeButton; // Assign in Inspector
    public Button quitToMenuButton; // Assign in Inspector

    private Vector3 initialResumeButtonScale;
    private Vector3 initialQuitToMenuButtonScale;
    public float hoverScaleMultiplier = 1.1f;

    void Awake()
    {
        if (scoreText == null) Debug.LogError("UIManager: ScoreText not assigned!", this.gameObject);
        if (gameOverPanel == null) Debug.LogError("UIManager: GameOverPanel not assigned!", this.gameObject);
        if (finalScoreText == null) Debug.LogError("UIManager: FinalScoreText not assigned!", this.gameObject);
        if (heartIcons == null || heartIcons.Length == 0)
        {
            Debug.LogWarning("UIManager: HeartIcons array is not assigned or is empty.", this.gameObject);
        }

        // Pause Menu UI Checks
        if (pauseMenuPanel == null) Debug.LogError("UIManager: PauseMenuPanel not assigned!", this.gameObject);
        if (resumeButton == null) Debug.LogError("UIManager: ResumeButton not assigned!", this.gameObject);
        if (quitToMenuButton == null) Debug.LogError("UIManager: QuitToMenuButton not assigned!", this.gameObject);
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false); // Ensure pause menu is hidden at start

        // Store initial scales for pause menu buttons
        if (resumeButton != null && resumeButton.transform is RectTransform)
        {
            initialResumeButtonScale = resumeButton.transform.localScale;
        }
        if (quitToMenuButton != null && quitToMenuButton.transform is RectTransform)
        {
            initialQuitToMenuButtonScale = quitToMenuButton.transform.localScale;
        }

        // Subscribe to health changes from HealthManager
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnHealthChanged += UpdateHearts;
            UpdateHearts(HealthManager.Instance.CurrentHealth);
        }
        else
        {
            Debug.LogError("UIManager: HealthManager.Instance is null at Start!", this.gameObject);
        }

        // Initialize score display
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
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Final Score: " + finalScore;
    }

    public void HideGameOverScreen()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    // --- Pause Menu Methods ---
    public void SetPauseMenu(bool isActive)
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isActive);
        }
    }

    // Called by Resume Button's OnClick event (configured in Inspector)
    public void OnResumeButtonPressed()
    {
        GameManager.Instance?.ResumeGame();
    }

    // Called by Quit to Menu Button's OnClick event (configured in Inspector)
    public void OnQuitToMenuButtonPressed()
    {
        GameManager.Instance?.QuitToMainMenu();
    }

    // --- Pause Menu Button Hover Effects ---
    // Assign these to Event Triggers (PointerEnter, PointerExit) on the buttons
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
