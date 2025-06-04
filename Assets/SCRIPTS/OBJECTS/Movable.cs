using UnityEngine;

public class Movable : MonoBehaviour
{
    public float offScreenX = -15f; // X position to destroy object

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver()) return;

        transform.Translate(Vector3.left * GameManager.Instance.CurrentGameSpeed * Time.deltaTime);

        if (transform.position.x < offScreenX)
        {
            Destroy(gameObject);
        }
    }
}