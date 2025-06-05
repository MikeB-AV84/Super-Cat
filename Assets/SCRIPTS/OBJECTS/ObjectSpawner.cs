using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour
{
    public static ObjectSpawner Instance { get; private set; }

    public GameObject heartPrefab;
    public GameObject burgerPrefab;
    public GameObject damageObstaclePrefab;

    [Header("Spawn Timings")]
    public float heartSpawnInterval = 45f;
    public Vector2 burgerSpawnRateRange = new Vector2(3f, 8f);
    public Vector2 obstacleSpawnRateRange = new Vector2(1f, 4f);

    [Header("Spawn Positioning & Spacing")]
    public float spawnXPosition = 12f;
    public float minTimeBetweenAnySpawn = 0.2f;
    public float laneSpecificCooldownDuration = 0.5f;

    private Coroutine _heartSpawnCoroutine;
    private Coroutine _burgerSpawnCoroutine;
    private Coroutine _obstacleSpawnCoroutine;
    private bool _isSpawning = false;

    private float _lastGlobalSpawnTime = -10f;
    private float[] _laneCooldownEndTime; // Array to store cooldown end times for each lane

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
        // _laneCooldownEndTime's initial sizing is now handled in StartSpawning
    }

    void Start()
    {
        // This Start method can be used for other initializations for ObjectSpawner
        // but we are no longer sizing _laneCooldownEndTime here to avoid race conditions
        // with GameManager calling StartSpawning.
    }

    public void StartSpawning()
    {
        if (_isSpawning) return; // Don't start if already spawning

        // Critical check for LaneManager before proceeding
        if (LaneManager.Instance == null ||
            LaneManager.Instance.lanePositionsY == null ||
            LaneManager.Instance.lanePositionsY.Length == 0)
        {
            Debug.LogError("ObjectSpawner (StartSpawning): LaneManager not found or its lanePositionsY not initialized! Cannot start spawning.", this.gameObject);
            _isSpawning = false; // Ensure spawning is flagged as off
            return;
        }

        // Initialize or Re-initialize _laneCooldownEndTime array's size HERE.
        // This ensures the array is correctly sized before it's filled,
        // regardless of Start() execution order.
        int requiredLaneCount = LaneManager.Instance.lanePositionsY.Length;
        if (_laneCooldownEndTime == null || _laneCooldownEndTime.Length != requiredLaneCount)
        {
            // This is no longer a "warning" of incorrect sizing, but the point of actual sizing.
            // It's expected to be null on the very first call.
            // Debug.Log($"ObjectSpawner (StartSpawning): Initializing/Resizing _laneCooldownEndTime array to size {requiredLaneCount}.", this.gameObject);
            _laneCooldownEndTime = new float[requiredLaneCount];
        }

        _isSpawning = true; // Set spawning to true only if all checks pass and initialization is done

        // Reset cooldowns to ensure lanes are initially free
        _lastGlobalSpawnTime = Time.time - minTimeBetweenAnySpawn; // Ensure first spawn isn't immediately blocked
        for (int i = 0; i < _laneCooldownEndTime.Length; i++)
        {
            _laneCooldownEndTime[i] = Time.time - laneSpecificCooldownDuration; // Ensure lanes are initially free
        }

        // Start the spawning coroutines
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
            if (_isSpawning) TrySpawnObject(heartPrefab);
        }
    }

    IEnumerator SpawnBurgerRoutine()
    {
        while (_isSpawning)
        {
            float waitTime = Random.Range(burgerSpawnRateRange.x, burgerSpawnRateRange.y);
            yield return new WaitForSeconds(waitTime);
            if (_isSpawning) TrySpawnObject(burgerPrefab);
        }
    }

    IEnumerator SpawnObstacleRoutine()
    {
        while (_isSpawning)
        {
            float waitTime = Random.Range(obstacleSpawnRateRange.x, obstacleSpawnRateRange.y);
            yield return new WaitForSeconds(waitTime);
            if (_isSpawning) TrySpawnObject(damageObstaclePrefab);
        }
    }

    void TrySpawnObject(GameObject prefabToSpawn)
    {
        // Ensure spawning is active and dependencies are met
        if (!_isSpawning) return;

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("ObjectSpawner: Prefab to spawn is null.", this.gameObject);
            return;
        }

        // These checks should ideally not fail here if StartSpawning was successful,
        // but are good safety measures.
        if (LaneManager.Instance == null || LaneManager.Instance.lanePositionsY == null) {
            Debug.LogError("ObjectSpawner (TrySpawnObject): LaneManager not available. Cannot spawn.", this.gameObject);
            return;
        }
        if (_laneCooldownEndTime == null) {
            Debug.LogError("ObjectSpawner (TrySpawnObject): _laneCooldownEndTime array is null. Cannot check lane cooldowns.", this.gameObject);
            return;
        }


        // 1. Global Cooldown Check
        if (Time.time < _lastGlobalSpawnTime + minTimeBetweenAnySpawn)
        {
            // Debug.Log($"Global spawn cooldown active. Skipping spawn for {prefabToSpawn.name}");
            return; // Too soon since the last global spawn
        }

        int randomLaneIndex = Random.Range(0, LaneManager.Instance.lanePositionsY.Length);
        
        // Defensively check array bounds, though length should match lanePositionsY.Length
        if (randomLaneIndex >= _laneCooldownEndTime.Length) {
            Debug.LogError($"ObjectSpawner (TrySpawnObject): randomLaneIndex ({randomLaneIndex}) is out of bounds for _laneCooldownEndTime (Length: {_laneCooldownEndTime.Length}). This indicates a mismatch.", this.gameObject);
            return;
        }

        // 2. Lane Specific Cooldown Check
        if (Time.time < _laneCooldownEndTime[randomLaneIndex])
        {
            // Debug.Log($"Lane {randomLaneIndex} is on cooldown. Skipping spawn for {prefabToSpawn.name}");
            return; // This specific lane is "busy"
        }

        // All checks passed, proceed to spawn
        float spawnYPosition = LaneManager.Instance.GetLaneYPosition(randomLaneIndex);
        Vector3 spawnPosition = new Vector3(spawnXPosition, spawnYPosition, 0f);
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, transform); // Spawn as child of spawner

        // Update cooldown timers
        _lastGlobalSpawnTime = Time.time;
        _laneCooldownEndTime[randomLaneIndex] = Time.time + laneSpecificCooldownDuration;
    }
}
