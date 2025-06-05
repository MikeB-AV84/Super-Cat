using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes
using UnityEngine.UI; // Often used with UI elements, though not strictly for this basic script
using UnityEngine.EventSystems; // For more advanced event handling like IPointerEnterHandler

// It's good practice to place your scripts within a namespace
namespace YourGameName.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Navigation")]
        [Tooltip("The name of the scene to load when the Play button is clicked.")]
        public string gameSceneName = "GameScene"; // Make sure this matches the exact name of your game scene

        [Header("Button References (Optional - for script-based hover)")]
        [Tooltip("Reference to the Play Button's RectTransform for script-based scaling.")]
        public RectTransform playButtonRect;
        [Tooltip("Reference to the Quit Button's RectTransform for script-based scaling.")]
        public RectTransform quitButtonRect;

        private Vector3 initialPlayButtonScale;
        private Vector3 initialQuitButtonScale;
        public float hoverScaleMultiplier = 1.1f; // How much bigger the button gets on hover

        void Start()
        {
            // Store initial scales if RectTransforms are assigned (for script-based hover)
            if (playButtonRect != null)
            {
                initialPlayButtonScale = playButtonRect.localScale;
            }
            if (quitButtonRect != null)
            {
                initialQuitButtonScale = quitButtonRect.localScale;
            }
        }

        // Public method to be called by the Play button's OnClick() event
        public void PlayGame()
        {
            if (string.IsNullOrEmpty(gameSceneName))
            {
                Debug.LogError("Game Scene Name is not set in the MainMenuController script on " + gameObject.name);
                return;
            }
            Debug.Log("Play button clicked. Loading scene: " + gameSceneName);
            SceneManager.LoadScene(gameSceneName);
        }

        // Public method to be called by the Quit button's OnClick() event
        public void QuitGame()
        {
            Debug.Log("Quit button clicked. Quitting application...");

            // If running in the Unity Editor
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            // If running in a built game
            Application.Quit();
            #endif
        }

        // --- Script-based Hover Effects ---
        // These methods can be linked to Event Trigger components on your UI Buttons.
        // Add an Event Trigger component to your Button.
        // Add two new event types: PointerEnter and PointerExit.
        // For PointerEnter, drag the GameObject with this script to the object field,
        // then select MainMenuController -> OnButtonHoverEnter (or specific button hover).
        // For PointerExit, select MainMenuController -> OnButtonHoverExit (or specific button hover).

        public void OnPlayButtonHoverEnter()
        {
            if (playButtonRect != null)
            {
                playButtonRect.localScale = initialPlayButtonScale * hoverScaleMultiplier;
            }
        }

        public void OnPlayButtonHoverExit()
        {
            if (playButtonRect != null)
            {
                playButtonRect.localScale = initialPlayButtonScale;
            }
        }

        public void OnQuitButtonHoverEnter()
        {
            if (quitButtonRect != null)
            {
                quitButtonRect.localScale = initialQuitButtonScale * hoverScaleMultiplier;
            }
        }

        public void OnQuitButtonHoverExit()
        {
            if (quitButtonRect != null)
            {
                quitButtonRect.localScale = initialQuitButtonScale;
            }
        }
    }
}
