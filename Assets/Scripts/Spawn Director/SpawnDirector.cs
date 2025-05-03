using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnDirector : MonoBehaviour
{
    // =============================================
    // VARIABLES
    // =============================================

    [Header("Monster Pool Settings")]
    public GameObject[] monsterPrefabs;
    private List<Queue<GameObject>> monsterPools = new List<Queue<GameObject>>();

    [Header("Credit System")]
    public float currentCredits;
    public float creditMultiplier = 1f;
    public float difficultyCoefficient = 1f;

    [Header("Spawn Loop")]
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 5f;
    private float spawnTimer;
    public int maxEnemies;

    [Header("Room Management")]
    private List<RoomSpawner> roomSpawners = new List<RoomSpawner>();

    [Header("Other")]
    private LogicManager logicMan;
    private GameObject player;

    // =============================================
    // INITIALIZATION
    // =============================================

    void Start()
    {

        player = GameObject.FindWithTag("Player");

        logicMan = GameObject.FindWithTag("LogicManager").GetComponent<LogicManager>();
        InitializeObjectPools();
        FindAllRoomSpawners();
        spawnTimer = GetRandomSpawnInterval();
    }

    void Update()
    {
        difficultyCoefficient = 1 + (logicMan.difficultyLevel / 10);
        AccumulateCreditsOverTime();

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            TriggerRoomSpawning();
            spawnTimer = GetRandomSpawnInterval();
        }
    }

    public int poolSizes = 50;
    void InitializeObjectPools()
    {
        foreach (GameObject prefab in monsterPrefabs)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int j = 0; j < poolSizes; j++)
            {
                GameObject monster = Instantiate(prefab);
                monster.SetActive(false);
                pool.Enqueue(monster);
            }
            monsterPools.Add(pool);
        }
    }

    void FindAllRoomSpawners()
    {
        roomSpawners = new List<RoomSpawner>(FindObjectsOfType<RoomSpawner>());
        Debug.Log("Found " + roomSpawners.Count + " room spawners.");
    }

    // =============================================
    // CREDIT SYSTEM
    // =============================================

    public float EscapeCreditIncreaseCoefficient = 1.5f;

    void AccumulateCreditsOverTime()
    {
        float creditsPerSecond = creditMultiplier * (1 + 0.4f * difficultyCoefficient);
        if (logicMan.objectiveComplete)
        {
            creditsPerSecond *= EscapeCreditIncreaseCoefficient;
        }
        currentCredits += creditsPerSecond * Time.deltaTime;
    }

    // =============================================
    // SPAWNING COORDINATION
    // =============================================

    float GetRandomSpawnInterval()
    {
        float interval = Random.Range(minSpawnInterval, maxSpawnInterval);
        interval = interval - (difficultyCoefficient - 1f);
        if (logicMan.objectiveComplete) interval /= 1.5f;
        return Mathf.Clamp(interval, 1, maxSpawnInterval);
    }

    void TriggerRoomSpawning()
    {
        if (currentCredits <= 0 || monsterPools.Count == 0) return;

        RoomSpawner closestRoom = GetClosestRoomSpawner();
        if (closestRoom != null)
        {
            float creditsSpent = closestRoom.AttemptSpawns(this, currentCredits);
            currentCredits -= creditsSpent; // Subtract the spent credits
        }
    }

    RoomSpawner GetClosestRoomSpawner()
    {
        RoomSpawner closest = null;
        float closestDistance = Mathf.Infinity;
        Vector3 playerPos = player.transform.position;

        foreach (RoomSpawner spawner in roomSpawners)
        {
            float distance = Vector3.Distance(spawner.transform.position, playerPos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = spawner;
            }
        }
        return closest;
    }

    // =============================================
    // POOL MANAGEMENT
    // =============================================

    public GameObject GetMonsterFromPool(int prefabIndex)
    {
        if (prefabIndex >= 0 && prefabIndex < monsterPools.Count && monsterPools[prefabIndex].Count > 0)
        {
            return monsterPools[prefabIndex].Dequeue();
        }
        return null;
    }

    /// <summary>
    /// Registers a monster death and returns it to the pool
    /// </summary>
    public void RegisterKill(GameObject monster, int prefabIndex)
    {
        monster.SetActive(false);
        print("gone");

        if (prefabIndex >= 0 && prefabIndex < monsterPools.Count)
        {
            print("hi");
            monsterPools[prefabIndex].Enqueue(monster);
        }
    }
}