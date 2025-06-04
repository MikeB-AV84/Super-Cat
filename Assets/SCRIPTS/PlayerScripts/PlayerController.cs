using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float laneChangeSpeed = 10f; // Speed of snapping to the new lane
    private int _currentLaneIndex = 1; // 0 = Top, 1 = Middle, 2 = Bottom
    private Vector3 _targetPosition;

    void Start()
    {
        if (LaneManager.Instance == null)
        {
            Debug.LogError("LaneManager not found in scene!");
            enabled = false; // Disable script if LaneManager is missing
            return;
        }
        // Set initial position based on LaneManager
        _currentLaneIndex = 1; // Start in the middle lane
        transform.position = new Vector3(transform.position.x, LaneManager.Instance.GetLaneYPosition(_currentLaneIndex), transform.position.z);
        _targetPosition = transform.position;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
        {
            return; // Don't allow movement if game is over
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveLane(1);
        }

        // Smoothly move to target lane position
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, laneChangeSpeed * Time.deltaTime);
    }

    void MoveLane(int direction) // -1 for up, 1 for down
    {
        int newLaneIndex = _currentLaneIndex + direction;

        // Clamp newLaneIndex to be within 0 and 2
        newLaneIndex = Mathf.Clamp(newLaneIndex, 0, 2);

        if (newLaneIndex != _currentLaneIndex)
        {
            _currentLaneIndex = newLaneIndex;
            _targetPosition = new Vector3(transform.position.x, LaneManager.Instance.GetLaneYPosition(_currentLaneIndex), transform.position.z);
            // Optional: Play lane switch sound
        }
    }

    // Collision detection will be handled by item/obstacle scripts via OnTriggerEnter
}