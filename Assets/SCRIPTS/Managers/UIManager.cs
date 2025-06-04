using UnityEngine;
using UnityEngine.UI;     // Required for Image components
using TMPro;              // Required for TextMeshPro elements

public class UIManager : MonoBehaviour
{
    // Assign these in the Inspector
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    // Hearts Display: Using an array of 2D Image UI elements
    public Image[] heartIcons; // Assign your 3 (or more) heart UI Image elements here

    void Awake()
    {
        // It's good practice to null-check critical references if they are assigned in Inspector
        if (scoreText == null) Debug.LogError("UIManager: ScoreText not assigned!", this.gameObject);
        if (gameOverPanel == null) Debug.LogError("UIManager: GameOverPanel not assigned!", this.gameObject);
        if (finalScoreText == null) Debug.LogError("UIManager: FinalScoreText not assigned!", this.gameObject);
        if (heartIcons == null || heartIcons.Length == 0)
        {
            Debug.LogWarning("UIManager: HeartIcons array is not assigned or is empty. Heart display will not work.", this.gameObject);
        }
    }

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Subscribe to health changes from HealthManager
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnHealthChanged += UpdateHearts;

            // Initialize hearts display based on current health at start
            // This relies on HealthManager having initialized its CurrentHealth by its own Start/Awake
            UpdateHearts(HealthManager.Instance.CurrentHealth);
        }
        else
        {
            Debug.LogError("UIManager: HealthManager.Instance is null at Start! UI cannot subscribe to health changes or initialize hearts.", this.gameObject);
        }

        // Initialize score display
        if (ScoreManager.Instance != null && scoreText != null) {
             UpdateScore(ScoreManager.Instance.CurrentScore);
        } else if (scoreText == null) {
            Debug.LogError("UIManager: ScoreText is null. Cannot display score.", this.gameObject);
        } else {
            Debug.LogWarning("UIManager: ScoreManager.Instance is null at Start. Cannot initialize score.", this.gameObject);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent errors if UIManager is destroyed before HealthManager
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnHealthChanged -= UpdateHearts;
        }
    }

    // This method is typically called by GameManager when score changes (or UIManager can subscribe directly)
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    // This method is called when HealthManager.OnHealthChanged event fires
    public void UpdateHearts(int currentHealth)
    {
        // Debug.Log($"UIManager: Updating hearts. Current Health: {currentHealth}"); // For testing
        if (heartIcons != null)
        {
            for (int i = 0; i < heartIcons.Length; i++)
            {
                if (heartIcons[i] != null)
                {
                    // Enable the heart image if 'i' (0-indexed) is less than currentHealth
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
        }
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore;
        }
    }

    public void HideGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
}