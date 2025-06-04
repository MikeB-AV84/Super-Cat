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
        // Initialization of _laneCooldownEndTime array is moved to Start()
    }

    void Start()
    {
        // Initialize _laneCooldownEndTime here, as LaneManager.Instance and its properties should be set.
        if (LaneManager.Instance != null &&
            LaneManager.Instance.lanePositionsY != null &&
            LaneManager.Instance.lanePositionsY.Length > 0)
        {
            _laneCooldownEndTime = new float[LaneManager.Instance.lanePositionsY.Length];
        }
        else
        {
            // This log indicates a problem with LaneManager setup or access timing.
            Debug.LogError("ObjectSpawner (Start): LaneManager or its lanePositionsY not initialized correctly! Spacing logic might be impaired. Defaulting to 3 lanes for cooldown array.", this.gameObject);
            _laneCooldownEndTime = new float[3]; // Fallback if LaneManager isn't ready
        }

        // Note: StartSpawning() is typically called by GameManager after this Start() method.
        // The actual values in _laneCooldownEndTime are set within StartSpawning().
    }

    public void StartSpawning()
    {
        if (_isSpawning) return;
        

        if (LaneManager.Instance == null ||
            LaneManager.Instance.lanePositionsY == null ||
            LaneManager.Instance.lanePositionsY.Length == 0)
        {
            Debug.LogError("ObjectSpawner (StartSpawning): LaneManager critical error. Cannot start spawning.", this.gameObject);
            _isSpawning = false; // Ensure spawning is flagged as off
            return;
        }
        
        // Ensure _laneCooldownEndTime array is initialized (it should be by Start(), but double-check size)
        if (_laneCooldownEndTime == null || _laneCooldownEndTime.Length != LaneManager.Instance.lanePositionsY.Length)
        {
            Debug.LogWarning("ObjectSpawner (StartSpawning): _laneCooldownEndTime was not correctly sized. Re-initializing array size.", this.gameObject);
            _laneCooldownEndTime = new float[LaneManager.Instance.lanePositionsY.Length];
        }

        _isSpawning = true; // Set true only if checks pass
        _lastGlobalSpawnTime = Time.time - minTimeBetweenAnySpawn; // Ensure first spawn isn't blocked
        for (int i = 0; i < _laneCooldownEndTime.Length; i++)
        {
            // Ensure lanes are initially free
            _laneCooldownEndTime[i] = Time.time - laneSpecificCooldownDuration;
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
        if (!_isSpawning) return; // Added check here

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("ObjectSpawner: Prefab to spawn is null.", this.gameObject);
            return;
        }

        if (LaneManager.Instance == null || LaneManager.Instance.lanePositionsY == null) {
            Debug.LogError("ObjectSpawner: LaneManager not available in TrySpawnObject. Cannot spawn.", this.gameObject);
            return;
        }


        if (Time.time < _lastGlobalSpawnTime + minTimeBetweenAnySpawn)
        {
            return; 
        }

        int randomLaneIndex = Random.Range(0, LaneManager.Instance.lanePositionsY.Length);
        
        // Check if _laneCooldownEndTime has been initialized correctly
        if (_laneCooldownEndTime == null || randomLaneIndex >= _laneCooldownEndTime.Length) {
            Debug.LogError($"ObjectSpawner: _laneCooldownEndTime not initialized or index out of bounds. LaneIndex: {randomLaneIndex}, ArrayLength: {(_laneCooldownEndTime == null ? "null" : _laneCooldownEndTime.Length.ToString())}", this.gameObject);
            return;
        }

        if (Time.time < _laneCooldownEndTime[randomLaneIndex])
        {
            return; 
        }

        float spawnYPosition = LaneManager.Instance.GetLaneYPosition(randomLaneIndex);
        Vector3 spawnPosition = new Vector3(spawnXPosition, spawnYPosition, 0f);
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, transform);

        _lastGlobalSpawnTime = Time.time;
        _laneCooldownEndTime[randomLaneIndex] = Time.time + laneSpecificCooldownDuration;
    }
}