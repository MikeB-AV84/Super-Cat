using UnityEngine;

public class Rotation2 : MonoBehaviour
{
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 30f;

    void Update()
    {
        // Rotate around the Z-axis at a constant speed
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}