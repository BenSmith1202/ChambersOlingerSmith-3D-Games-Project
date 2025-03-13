using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnDirector : MonoBehaviour
{
    // VARIABLES

    #region

    [Header("Spawning Settings")]
    public GameObject[] monsterPrefabs; // Array of monster types
    private List<GameObject> allMonsters = new List<GameObject>();

    public Transform player; // Reference to the player object
    public float innerSpawnRadius = 5f; // Minimum spawn distance from player
    public float outerSpawnRadius = 20f; // Maximum spawn distance from player
    public LayerMask groundLayer; // LayerMask to detect the ground

    [Header("Object Pools")]
    private List<Queue<GameObject>> monsterPools = new List<Queue<GameObject>>();

    [Header("Credit System")]
    public float currentCredits; // Current credits available for spawning
    public float creditMultiplier = 1f; // Multiplier for credit income
    public float difficultyCoefficient = 1f; // Difficulty scaling factor

    [Header("Spawn Loop")]
    public float minSpawnInterval = 1f; // Minimum time between spawns
    public float maxSpawnInterval = 5f; // Maximum time between spawns
    private float spawnTimer;

    [Header("Spawn Cards")]
    private List<SpawnCard> spawnCards = new List<SpawnCard>(); // List of all spawn cards from monster prefabs


    [Header("Other")]
    private LogicManager logicMan;



    #endregion






    // START + UPDATE + INITIALIZE POOLS

    #region

    void Start()
    {
        logicMan = GameObject.FindWithTag("LogicManager").GetComponent<LogicManager>();

        InitializeObjectPools();
        InitializeSpawnCards(); // Grab all SpawnCard scripts from monster prefabs
        spawnTimer = GetRandomSpawnInterval(); // Initialize the first spawn timer
    }

    void Update()
    {
        // Handle continuous credit income for continuous Directors
        AccumulateCreditsOverTime();

        // Handle spawn loop
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            AttemptSpawn();
            spawnTimer = GetRandomSpawnInterval(); // Reset timer with a new random interval
        }
    }



    // Initialize object pools for each monster type
    void InitializeObjectPools()
    {
        for (int i = 0; i < monsterPrefabs.Length; i++)
        {
            Queue<GameObject> newPool = new Queue<GameObject>();
            for (int j = 0; j < 50; j++) // Each pool gets 50 monsters
            {
                GameObject monster = Instantiate(monsterPrefabs[i]);
                monster.SetActive(false);
                newPool.Enqueue(monster);
            }
            monsterPools.Add(newPool);
        }
        Debug.Log("Initialized object pools for " + monsterPrefabs.Length + " monster types.");
    }



    #endregion


















    // CREDITS

    #region

    




    // Accumulate credits over time based on difficulty and credit multiplier
    void AccumulateCreditsOverTime()
    {
        float creditsPerSecond = creditMultiplier * (1 + 0.4f * difficultyCoefficient);
        currentCredits += creditsPerSecond * Time.deltaTime;
    }

    // Spend credits when spawning a monster
    void SpendCredits(float amount)
    {
        if (currentCredits >= amount)
        {
            currentCredits -= amount;
            Debug.Log("Spent " + amount + " credits. Remaining: " + currentCredits);
        }
        else
        {
            Debug.Log("Not enough credits to spawn monster. Required: " + amount + ", Available: " + currentCredits);
        }
    }


    #endregion


















    // SPAWNING

    #region


    // Grab all SpawnCard scripts from monster prefabs
    void InitializeSpawnCards()
    {
        foreach (GameObject monsterPrefab in monsterPrefabs)
        {
            SpawnCard spawnCard = monsterPrefab.GetComponent<SpawnCard>();
            if (spawnCard != null)
            {
                spawnCards.Add(spawnCard);
            }
        }
        Debug.Log("Loaded " + spawnCards.Count + " spawn cards.");
    }






    // Select a spawn card based on weighted probability
    SpawnCard SelectSpawnCard()
    {
        float totalWeight = 0f;
        foreach (SpawnCard card in spawnCards)
        {
            if (IsSpawnCardValid(card))
            {
                totalWeight += card.weight;
            }
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (SpawnCard card in spawnCards)
        {
            if (IsSpawnCardValid(card))
            {
                cumulativeWeight += card.weight;
                if (randomValue <= cumulativeWeight)
                {
                    return card;
                }
            }
        }

        return null; // No valid spawn card found
    }







    // Check if a spawn card is valid for the current stage and credits
    bool IsSpawnCardValid(SpawnCard card)
    {
        return card.minStage <= logicMan.currentStage && card.creditCost <= currentCredits;
    }




    // Calculate a random spawn interval within bounds influenced by difficulty
    float GetRandomSpawnInterval()
    {
        float interval = Random.Range(minSpawnInterval, maxSpawnInterval);
        interval /= difficultyCoefficient; // Decrease interval as difficulty increases
        return Mathf.Clamp(interval, minSpawnInterval, maxSpawnInterval); // Ensure it stays within bounds
    }



    // Attempt to spawn a monster
    void AttemptSpawn()
    {
        if (allMonsters.Count >= 40)
        {
            Debug.Log("Overcrowded! Cannot spawn more monsters.");
            return;
        }

        SpawnCard selectedCard = SelectSpawnCard();
        if (selectedCard == null)
        {
            Debug.Log("No valid spawn card found.");
            return;
        }

        // Keep spawning monsters using the selected card until it's no longer valid
        while (IsSpawnCardValid(selectedCard) && allMonsters.Count < 40)
        {
            SpawnMonster(selectedCard);
        }

        Debug.Log("Finished spawning with " + selectedCard.name);
    }







    // HARDEST THING IS THIS
    // AHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH

    // Spawn a monster using the selected spawn card
    void SpawnMonster(SpawnCard card)
    {
        GameObject monster = GetMonsterFromPool(card);
        if (monster == null)
        {
            Debug.Log("No available monsters in pool.");
            return;
        }

        Vector3 spawnPosition = GetRandomSpawnPosition(card);

        monster.transform.position = spawnPosition;
        monster.transform.rotation = Quaternion.identity;


        monster.SetActive(true);
        allMonsters.Add(monster);


        SpendCredits(card.creditCost);
        Debug.Log("Spawned " + monster.name + " for " + card.creditCost + " credits.");


    }






    private bool noGround;


    // Get a random spawn position that is valid (on the ground and surrounded by GroundLayer objects)
    Vector3 GetRandomSpawnPosition(SpawnCard card)
    {
        Vector3 randomPosition;
        bool isValidPosition = false;
        int maxAttempts = 20; // Maximum attempts to find a valid position
        int attempts = 0;

        do
        {
            // Generate a random position within the spawn radius
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(innerSpawnRadius, outerSpawnRadius);
            randomPosition = new Vector3(player.position.x + randomCircle.x, player.position.y + (Mathf.Abs( randomCircle.y) / 2), player.position.z + randomCircle.y);




            // Ensure the position is within the spawn radius
            float distance = Vector3.Distance(player.position, randomPosition);
            if (distance < innerSpawnRadius)
            {
                continue; // Skip this position if it's too close to the player
            }


            if (!card.isFlyer)
            {
                if (Physics.Raycast(randomPosition, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
                {
                    randomPosition = hit.point; // Adjust the position to the ground level
                    noGround = false;
                }
                else
                {
                    noGround = true;
                    print("tried to spawn over void");
                }
            }


            // Check if the position is surrounded by GroundLayer objects
            isValidPosition = IsPositionSurroundedByGround(randomPosition);


            attempts++;
        } while (!isValidPosition && attempts < maxAttempts && noGround);

        if (!isValidPosition)
        {
            Debug.LogWarning("Failed to find a valid spawn position after " + maxAttempts + " attempts.");
            return player.position; // Fallback to player position if no valid position is found
        }

        return randomPosition;
    }














    // Check if a position is surrounded by GroundLayer objects using raycasts
    bool IsPositionSurroundedByGround(Vector3 position)
    {
        // Define directions to check (up, down, left, right, forward, backward)
        Vector3[] directions = new Vector3[]
        {
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back
        };

        int counter = 0;

        // Check each direction
        foreach (Vector3 direction in directions)
        {
            if (Physics.Raycast(position, direction, out RaycastHit hit, 3f, groundLayer))
            {
                counter++;
            }
        }

        if (counter < 5){
            print("spawn inside wall cancled");
        }


        // If all directions have ground, the position is valid
        return counter < 5 && counter > 1;
    }




















    #endregion























    // REGISTERING STUFF

    #region

    // Get a monster from the object pool based on the spawn card
    GameObject GetMonsterFromPool(SpawnCard card)
    {
        int index = System.Array.IndexOf(monsterPrefabs, card.gameObject);
        if (index >= 0 && index < monsterPools.Count && monsterPools[index].Count > 0)
        {
            return monsterPools[index].Dequeue();
        }
        return null; // No available monsters in the pool
    }





    // Register a monster kill and return it to the pool
    public void RegisterKill(GameObject monster, int type)
    {
        allMonsters.Remove(monster);
        StartCoroutine(MonsterDies(monster, type));
    }





    // Coroutine to deactivate and return a monster to the pool
    public IEnumerator MonsterDies(GameObject monster, int type)
    {
        yield return new WaitForSeconds(2f);
        monster.SetActive(false);
        if (allMonsters.Count < monsterPools.Count)
        {
            monsterPools[type].Enqueue(monster); // may have to alter for multiple scenes lol
        }
    }



    #endregion























}