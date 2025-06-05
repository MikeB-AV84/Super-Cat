using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float laneChangeSpeed = 10f;
    private int _currentLaneIndex = 1; // 0 = Top, 1 = Middle, 2 = Bottom
    private Vector3 _targetPosition;

    private Animator _animator; // Reference to the Animator component
    private bool _isMovingLane = false; // To track if currently changing lanes

    // Animator parameter hashes (for efficiency)
    private static readonly int MoveDirectionHash = Animator.StringToHash("MoveDirection");
    private static readonly int LandedOnLaneHash = Animator.StringToHash("LandedOnLane");

    void Start()
    {
        _animator = GetComponent<Animator>(); // Get the Animator component
        if (_animator == null)
        {
            Debug.LogError("PlayerController: Animator component not found on Player!", this.gameObject);
        }

        if (LaneManager.Instance == null)
        {
            Debug.LogError("LaneManager not found in scene!", this.gameObject);
            enabled = false;
            return;
        }
        _currentLaneIndex = 1;
        transform.position = new Vector3(transform.position.x, LaneManager.Instance.GetLaneYPosition(_currentLaneIndex), transform.position.z);
        _targetPosition = transform.position;
        SetIdleAnimation(); // Start in idle
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver())
        {
            // Optionally set a "dead" animation here if you have one
            return;
        }

        // Handle input only if not currently moving between lanes
        if (!_isMovingLane)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveLane(-1); // -1 indicates moving up
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveLane(1); // 1 indicates moving down
            }
        }

        // Smoothly move to target lane position
        if (Vector3.Distance(transform.position, _targetPosition) > 0.01f)
        {
            _isMovingLane = true; // Still moving
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, laneChangeSpeed * Time.deltaTime);
        }
        else // Reached the target lane
        {
            if (_isMovingLane) // Was moving and just arrived
            {
                transform.position = _targetPosition; // Snap to exact position
                _isMovingLane = false;
                if (_animator != null)
                {
                    _animator.SetTrigger(LandedOnLaneHash); // Tell animator we've "landed"
                    SetIdleAnimation(); // Set parameter back to idle
                }
            }
        }
    }

    void MoveLane(int direction) // direction: -1 for up, 1 for down from current
    {
        int newLaneIndex = _currentLaneIndex + direction;
        newLaneIndex = Mathf.Clamp(newLaneIndex, 0, 2); // 0=Top, 1=Middle, 2=Bottom

        if (newLaneIndex != _currentLaneIndex)
        {
            _currentLaneIndex = newLaneIndex;
            _targetPosition = new Vector3(transform.position.x, LaneManager.Instance.GetLaneYPosition(_currentLaneIndex), transform.position.z);
            _isMovingLane = true;

            if (_animator != null)
            {
                if (direction < 0) // Moving Up
                {
                    _animator.SetInteger(MoveDirectionHash, 1); // Set MoveDirection to 1 (Up)
                }
                else if (direction > 0) // Moving Down
                {
                    _animator.SetInteger(MoveDirectionHash, -1); // Set MoveDirection to -1 (Down)
                }
            }
        }
    }

    void SetIdleAnimation()
    {
        if (_animator != null)
        {
            _animator.SetInteger(MoveDirectionHash, 0); // Set MoveDirection to 0 (Idle)
        }
    }
}