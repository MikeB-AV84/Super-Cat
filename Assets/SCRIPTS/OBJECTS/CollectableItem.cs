using UnityEngine;

public enum ItemType { Heart, Burger }

public class CollectableItem : MonoBehaviour
{
    public ItemType itemType;
    public int value = 1; // For hearts, it's 1 health. For burgers, points (e.g., 50)

    void OnTriggerEnter(Collider other) // Or OnTriggerEnter2D if using 2D colliders
    {
        if (other.CompareTag("Player"))
        {
            Collect(other.gameObject);
        }
    }

    protected virtual void Collect(GameObject player)
    {
        // Default behavior, can be overridden
        Debug.Log(itemType + " collected by " + player.name);

        if (itemType == ItemType.Heart)
        {
            HealthManager.Instance?.Heal(value);
        }
        else if (itemType == ItemType.Burger)
        {
            ScoreManager.Instance?.AddScore(value);
        }

        // Optional: Play collection sound
        Destroy(gameObject); // Remove item from scene
    }
}