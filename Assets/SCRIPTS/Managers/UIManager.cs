using UnityEngine;
using UnityEngine.UI;     // Still needed for Image components (like hearts)
using TMPro;              // Required for TextMeshPro elements
using System.Collections.Generic; // For List if using individual heart images

public class UIManager : MonoBehaviour
{
    // Assign these in the Inspector
    public TextMeshProUGUI scoreText;         // Changed from Text to TextMeshProUGUI
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;    // Changed from Text to TextMeshProUGUI

    // Hearts Display (Option 1: Array of Images - using UnityEngine.UI.Image)
    public Image[] heartIcons; // Assign 3 heart UI Image elements

    // Hearts Display (Option 2: TextMeshPro based)
    // public TextMeshProUGUI heartsText;     // Changed from Text to TextMeshProUGUI

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
        // Option 1: Using an array of heart images (UnityEngine.UI.Image)
        if (heartIcons != null && heartIcons.Length > 0) // Check if array is assigned and not empty
        {
            for (int i = 0; i < heartIcons.Length; i++)
            {
                if (heartIcons[i] != null) // Check if individual image element is assigned
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
        }

        // Option 2: Using TextMeshPro (e.g., "Hearts: ❤️❤️❤️")
        // if (heartsText != null)
        // {
        //     string heartDisplay = "Hearts: ";
        //     for(int i = 0; i < currentHealth; i++) heartDisplay += "❤️"; // Note: Ensure your TMP font asset supports these characters
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