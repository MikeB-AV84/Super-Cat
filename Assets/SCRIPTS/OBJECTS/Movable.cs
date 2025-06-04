using UnityEngine;

public class Movable : MonoBehaviour
{
    public float offScreenX = -12f; // Should be set to -12 on your prefabs

    // Use this flag to log the speed issue only once per object, if needed
    private bool hasLoggedSpeedIssue = false;

    void Update()
    {
        if (GameManager.Instance == null)
        {
            // This would be a problem, but usually other things break first.
            // To avoid spamming console: Debug.LogWarningOnce("Movable: GameManager.Instance is null!");
            return;
        }

        if (GameManager.Instance.IsGameOver())
        {
            // Objects stop moving when the game is over.
            return;
        }

        float currentSpeed = GameManager.Instance.CurrentGameSpeed;

        // --- Debugging Speed ---
        if (!hasLoggedSpeedIssue)
        {
            if (currentSpeed <= 0)
            {
                Debug.LogWarning($"Movable script on '{gameObject.name}': CurrentGameSpeed is {currentSpeed}. Object will not move correctly. Check GameManager's initialGameSpeed.", this.gameObject);
                hasLoggedSpeedIssue = true; // Log only once per object instance
            }
        }
        // --- End Debugging Speed ---

        // Explicitly use Space.World to ensure movement along the global X-axis
        // Vector3.left is (-1, 0, 0)
        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime, Space.World);

        // --- Debugging Position (Enable temporarily if objects disappear or behave strangely) ---
        // Debug.Log($"Object: {gameObject.name}, X Position: {transform.position.x}, Target Destroy X: {offScreenX}, Speed: {currentSpeed}");
        // --- End Debugging Position ---

        if (transform.position.x < offScreenX)
        {
            Destroy(gameObject);
        }
    }
}