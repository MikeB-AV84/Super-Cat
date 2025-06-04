using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Text, Image
using System.Collections.Generic; // For List if using individual heart images

public class UIManager : MonoBehaviour
{
    // Assign these in the Inspector
    public Text scoreText;
    public GameObject gameOverPanel;
    public Text finalScoreText;

    // Hearts Display (Option 1: Array of Images)
    public Image[] heartIcons; // Assign 3 heart UI Image elements

    // Hearts Display (Option 2: Text based)
    // public Text heartsText;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        // Initial UI setup will be handled by GameManager calling these methods
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
        // Option 1: Using an array of heart images
        if (heartIcons != null)
        {
            for (int i = 0; i < heartIcons.Length; i++)
            {
                if (i < currentHealth)
                {
                    heartIcons[i].enabled = true;
                }
                else
                {
                    heartIcons[i].enabled = false;
                }
            }
        }

        // Option 2: Using text (e.g., "Hearts: ❤️❤️❤️")
        // if (heartsText != null)
        // {
        //     string heartDisplay = "Hearts: ";
        //     for(int i = 0; i < currentHealth; i++) heartDisplay += "❤️";
        //     heartsText.text = heartDisplay;
        // }
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