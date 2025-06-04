using UnityEngine;

/// <summary>
/// Rotates the GameObject this script is attached to horizontally around its Y-axis.
/// Allows for adjustable speed and direction via the Inspector.
/// </summary>
public class HorizontalRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Speed of the rotation in degrees per second.")]
    public float rotationSpeed = 50f;

    [Tooltip("Determines the direction of rotation. True for clockwise, False for counter-clockwise.")]
    public bool rotateClockwise = true;

    // Internal variable to store the direction multiplier
    private float _directionMultiplier = 1f;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Used here to set the initial direction multiplier based on the inspector setting.
    /// </summary>
    void Awake()
    {
        UpdateDirection();
    }

    /// <summary>
    /// Update is called once per frame.
    /// Handles the continuous rotation of the object.
    /// </summary>
    void Update()
    {
        // Calculate the rotation amount for this frame
        // Time.deltaTime ensures the rotation is smooth and frame-rate independent
        float rotationAmount = rotationSpeed * _directionMultiplier * Time.deltaTime;

        // Apply the rotation around the Y-axis
        // Vector3.up is a shorthand for new Vector3(0, 1, 0)
        transform.Rotate(Vector3.up, rotationAmount);
    }

    /// <summary>
    /// This method is called by Unity when a value is changed in the Inspector.
    /// It's good practice to update internal states if public variables they depend on are changed.
    /// </summary>
    void OnValidate()
    {
        UpdateDirection();
    }

    /// <summary>
    /// Updates the internal direction multiplier based on the public 'rotateClockwise' boolean.
    /// </summary>
    private void UpdateDirection()
    {
        _directionMultiplier = rotateClockwise ? 1f : -1f;
    }

    // Example of how you might want to change direction via script (optional)
    /*
    public void SetDirection(bool clockwise)
    {
        rotateClockwise = clockwise;
        UpdateDirection();
    }

    public void ToggleDirection()
    {
        rotateClockwise = !rotateClockwise;
        UpdateDirection();
    }
    */
}