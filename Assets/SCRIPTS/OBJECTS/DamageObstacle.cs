using UnityEngine;

public class DamageObstacle : MonoBehaviour
{
    public int damageAmount = 1;

    void OnTriggerEnter2D(Collider2D other) // Or OnTriggerEnter2D
    {
        if (other.CompareTag("Player"))
        {
            HealthManager.Instance?.TakeDamage(damageAmount);
            // Optional: Play impact sound, visual effect, maybe destroy obstacle
            // For this example, we'll let it pass through or be destroyed by Movable script
            // If you want it to destroy on impact:
            Destroy(gameObject);
        }
    }
}