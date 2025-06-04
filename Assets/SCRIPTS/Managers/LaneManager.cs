using UnityEngine;

public class LaneManager : MonoBehaviour
{
    public static LaneManager Instance { get; private set; }

    public float[] lanePositionsY; // Y-coordinates for Top, Middle, Bottom lanes
                                    // Example: Top = 2.0f, Middle = 0.0f, Bottom = -2.0f

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (lanePositionsY == null || lanePositionsY.Length != 3)
        {
            Debug.LogWarning("LaneManager: lanePositionsY not set up correctly. Defaulting to 2, 0, -2.");
            lanePositionsY = new float[] { 2f, 0f, -2f };
        }
    }

    public float GetLaneYPosition(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= lanePositionsY.Length)
        {
            Debug.LogError("Invalid lane index: " + laneIndex);
            return 0f; // Default to middle lane Y if error
        }
        return lanePositionsY[laneIndex];
    }
}