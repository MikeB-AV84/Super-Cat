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

    public float spawnXPosition = 15f; // X position where objects will spawn (off-screen right)

    private Coroutine _heartSpawnCoroutine;
    private Coroutine _burgerSpawnCoroutine;
    private Coroutine _obstacleSpawnCoroutine;
    private bool _isSpawning = false;

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
    }

    public void StartSpawning()
    {
        if (_isSpawning) return;
        _isSpawning = true;

        if (LaneManager.Instance == null)
        {
            Debug.LogError("ObjectSpawner: LaneManager not found! Cannot determine spawn Y positions.");
            _isSpawning = false; // Prevent spawning if LaneManager is missing
            return;
        }
        if (LaneManager.Instance.lanePositionsY == null || LaneManager.Instance.lanePositionsY.Length == 0)
        {
            Debug.LogError("ObjectSpawner: LaneManager.lanePositionsY is not set up! Cannot determine spawn Y positions.");
            _isSpawning = false; // Prevent spawning if lanes aren't defined
            return;
        }


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
        if (prefabToSpawn == null) // LaneManager presence is checked in StartSpawning
        {
            Debug.LogWarning("Prefab is null. Cannot spawn.");
            return;
        }

        // Get a random lane index
        int randomLaneIndex = Random.Range(0, LaneManager.Instance.lanePositionsY.Length);

        // Get the Y position for that lane directly from LaneManager
        float spawnYPosition = LaneManager.Instance.GetLaneYPosition(randomLaneIndex);

        // Combine with the fixed spawnXPosition and a Z of 0
        Vector3 spawnPosition = new Vector3(spawnXPosition, spawnYPosition, 0f);

        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, transform); // Spawn as child of this spawner object
    }
}