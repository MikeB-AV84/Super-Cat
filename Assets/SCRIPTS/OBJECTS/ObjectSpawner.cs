using UnityEngine;
using System.Collections; // For IEnumerator

public class ObjectSpawner : MonoBehaviour
{
    public static ObjectSpawner Instance { get; private set; }

    public GameObject heartPrefab;
    public GameObject burgerPrefab;
    public GameObject damageObstaclePrefab;

    public float heartSpawnInterval = 45f;
    public Vector2 burgerSpawnRateRange = new Vector2(3f, 8f);     // Min/Max time between burger spawns
    public Vector2 obstacleSpawnRateRange = new Vector2(1f, 4f);  // Min/Max time between obstacle spawns

    public Transform[] spawnPoints; // Empty GameObjects defining where items can spawn (usually just X, Z fixed, Y varies by lane)
                                   // For this game, we only need one spawn point X,Z and vary Y based on lanes.
    public float spawnXPosition = 15f; // X position where objects will spawn (off-screen right)

    private Coroutine _heartSpawnCoroutine;
    private Coroutine _burgerSpawnCoroutine;
    private Coroutine _obstacleSpawnCoroutine;
    private bool _isSpawning = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartSpawning()
    {
        if (_isSpawning) return;
        _isSpawning = true;

        _heartSpawnCoroutine = StartCoroutine(SpawnHeartRoutine());
        _burgerSpawnCoroutine = StartCoroutine(SpawnBurgerRoutine());
        _obstacleSpawnCoroutine = StartCoroutine(SpawnObstacleRoutine());
    }

    public void StopSpawning()
    {
        _isSpawning = false;
        if (_heartSpawnCoroutine != null) StopCoroutine(_heartSpawnCoroutine);
        if (_burgerSpawnCoroutine != null) StopCoroutine(_burgerSpawnCoroutine);
        if (_obstacleSpawnCoroutine != null) StopCoroutine(_obstacleSpawnCoroutine);
    }

    IEnumerator SpawnHeartRoutine()
    {
        while (_isSpawning)
        {
            yield return new WaitForSeconds(heartSpawnInterval);
            if (_isSpawning) SpawnObject(heartPrefab);
        }
    }

    IEnumerator SpawnBurgerRoutine()
    {
        while (_isSpawning)
        {
            float waitTime = Random.Range(burgerSpawnRateRange.x, burgerSpawnRateRange.y);
            yield return new WaitForSeconds(waitTime);
            if (_isSpawning) SpawnObject(burgerPrefab);
        }
    }

    IEnumerator SpawnObstacleRoutine()
    {
        while (_isSpawning)
        {
            float waitTime = Random.Range(obstacleSpawnRateRange.x, obstacleSpawnRateRange.y);
            yield return new WaitForSeconds(waitTime);
            if (_isSpawning) SpawnObject(damageObstaclePrefab);
        }
    }

    void SpawnObject(GameObject prefabToSpawn)
    {
        if (prefabToSpawn == null || LaneManager.Instance == null)
        {
            Debug.LogWarning("Prefab or LaneManager is null. Cannot spawn.");
            return;
        }

        int randomLaneIndex = Random.Range(0, 3); // 0, 1, or 2
        float spawnYPosition = LaneManager.Instance.GetLaneYPosition(randomLaneIndex);

        Vector3 spawnPosition = new Vector3(spawnXPosition, spawnYPosition, 0f); // Assuming Z is 0 for 2.5D

        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, transform); // Spawn as child of spawner
    }
}