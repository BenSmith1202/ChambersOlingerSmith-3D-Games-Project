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
    public int maxEnemies;

    [Header("Spawn Cards")]
    private List<SpawnCard> spawnCards = new List<SpawnCard>(); // List of all spawn cards from monster prefabs


    [Header("Other")]
    private LogicManager logicMan;
    private bool noGround;
    public float offset;



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
    bool doItOnce = false;
    void Update()
    {
        if (logicMan.objectiveComplete)
        {
            if(doItOnce == false)
            {
                Light[] allLights = FindObjectsOfType<Light>();

                // Iterate through each light and check if it's a Point Light
                foreach (Light light in allLights)
                {
                    if (light.type == LightType.Point) // Ensure it's a Point Light
                    {
                        light.color = Color.red; // Change color to red
                    }
                }
            }
        }

        difficultyCoefficient = 1 + (logicMan.difficultyLevel / 10);


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
            for (int j = 0; j < 200; j++) // j = how many monsters each pool will have
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

    public float EscapeCreditIncreaseCoefficient = 1.5f;


    // Accumulate credits over time based on difficulty and credit multiplier
    void AccumulateCreditsOverTime()
    {
        float creditsPerSecond = creditMultiplier * (1 + 0.4f * difficultyCoefficient);
        if (logicMan.objectiveComplete)
        {
            creditsPerSecond = creditsPerSecond * EscapeCreditIncreaseCoefficient;
        }
        currentCredits += creditsPerSecond * Time.deltaTime;
    }

    // Spend credits when spawning a monster
    void SpendCredits(float amount)
    {
        if (currentCredits >= amount)
        {
            currentCredits -= amount;
           // Debug.Log("Spent " + amount + " credits. Remaining: " + currentCredits);
        }
        else
        {
          //  Debug.Log("Not enough credits to spawn monster. Required: " + amount + ", Available: " + currentCredits);
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


    bool IsSpawnCardValid(SpawnCard card)
    {
        return card.minStage <= logicMan.currentStage && card.creditCost <= currentCredits;
    }


    public float EscapeSpawnIntervalCoefficient = 1.5f;

    float GetRandomSpawnInterval()
    {
        float interval = Random.Range(minSpawnInterval, maxSpawnInterval);
        interval = interval - (difficultyCoefficient - 1f);
        if (logicMan.objectiveComplete)
        {
            interval = interval / EscapeSpawnIntervalCoefficient;
        }
        return Mathf.Clamp(interval, 1, maxSpawnInterval); // Ensure it stays within bounds
    }


    
    void AttemptSpawn()
    {
        if (allMonsters.Count >= maxEnemies)
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
        while (IsSpawnCardValid(selectedCard) && allMonsters.Count < maxEnemies)
        {
            SpawnMonster(selectedCard);
        }

        Debug.Log("Finished spawning with " + selectedCard.name);
    }


    void SpawnMonster(SpawnCard card)
    {
        GameObject monster = GetMonsterFromPool(card);
        if (monster == null)
        {
            Debug.Log("No available monsters in pool.");
            return;
        }

        Vector3 spawnPosition = GetRandomSpawnPosition(card, monster);

        monster.transform.position = spawnPosition;
        monster.transform.rotation = Quaternion.identity;


        monster.SetActive(true);
        allMonsters.Add(monster);
        print(allMonsters.Count);


        SpendCredits(card.creditCost);
        Debug.Log("Spawned " + monster.name + " for " + card.creditCost + " credits.");


    }

    public int maxAttempts = 20; // Maximum attempts to find a valid position

    Vector3 GetRandomSpawnPosition(SpawnCard card, GameObject monster)
    {
        Vector3 randomPosition;
        bool isValidPosition = false;
        int attempts = 0;

        do
        {
            // Generate a random position within the spawn radius
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(card.innerSpawnRadius, card.outerSpawnRadius);
            randomPosition = new Vector3(player.position.x + randomCircle.x, player.position.y + Mathf.Abs( Random.Range(-5 , 25)), player.position.z + randomCircle.y);

            //might have to add to the player position thingy




            // Ensure the position is within the spawn radius
            float distance = Vector3.Distance(player.position, randomPosition);
            if (distance < card.innerSpawnRadius)
            {
                continue; // Skip this position if it's too close to the player
            }


            //Check if the position is surrounded by GroundLayer objects
            isValidPosition = IsPositionSurroundedByGround(randomPosition);


            attempts++;
        } while (!isValidPosition && attempts < maxAttempts);



        if (Physics.Raycast(randomPosition, Vector3.down, out RaycastHit hit, float.PositiveInfinity, groundLayer))
        {
            randomPosition = hit.point;
            print(randomPosition);

        }




        // move it up slightly

        //print(randomPosition);

        //if (monster.GetComponent<Collider>())
        //{
        //    float distanceToGround = monster.GetComponent<Collider>().bounds.extents.y;
        //    randomPosition = new Vector3(randomPosition.x, randomPosition.y + distanceToGround, randomPosition.z);
        //}
        //else
        //{
        //    randomPosition = new Vector3(randomPosition.x, randomPosition.y + offset, randomPosition.z);
        //}

        randomPosition = new Vector3(randomPosition.x, randomPosition.y + offset, randomPosition.z);


        ////print(randomPosition);

        //Vector3 diff = randomPosition - player.transform.position;

        //if(diff.x > 0)
        //{
        //    //move left
        //    randomPosition.x -= spawnOffset2;
        //}
        //else
        //{
        //    // move right
        //    randomPosition.x += spawnOffset2;
        //}


        //if(diff.z > 0)
        //{
        //    //move back
        //    randomPosition.z -= spawnOffset2;
        //}
        //else
        //{
        //    //move foward
        //    randomPosition.z += spawnOffset2;

        //}

        //print(randomPosition);

        if (!isValidPosition)
        {
            Debug.LogWarning("Failed to find a valid spawn position after " + maxAttempts + " attempts.");
        }



        


        return randomPosition;
    }


    public float spawnOffset2;


    bool IsPositionSurroundedByGround(Vector3 position)
    {
       

        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, float.PositiveInfinity, groundLayer))
        {
            print("YAY");
            return true;

        }


        return false;
    }




    #endregion




    // REGISTERING STUFF

    #region

    // Get a monster from the object pool based on the spawn card
    GameObject GetMonsterFromPool(SpawnCard card)
    {
        int index = System.Array.IndexOf(monsterPrefabs, card.gameObject);

        int attempts = 0;
        do
        {
            if (index >= 0 && index < monsterPools.Count && monsterPools[index].Count > 0)
            {
                GameObject monster = monsterPools[index].Dequeue();
                return monster;
                if (monster.activeInHierarchy)
                {
                    monsterPools[index].Enqueue(monster);
                    attempts++;
                }
                else
                {
                    return monster;
                }


            }
        }
        while (attempts < 20);
        
        return null; // No available monsters in the pool
    }





    // Register a monster kill and return it to the pool
    public void RegisterKill(GameObject monster, int type)
    {

        //  Destroy(monster); //quick fix for now       

       


        StartCoroutine(MonsterDies(monster, type));
    }





    // Coroutine to deactivate and return a monster to the pool
    public IEnumerator MonsterDies(GameObject monster, int type)
    {
        // yield return new WaitForSeconds(0.01f); // if we had die animation, make this longer



        //  print(allMonsters.Count);
        allMonsters.Remove(monster);
        // print("AHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH");
        // print(allMonsters.Count);


        monster.SetActive(false);
       // print("DEAD MONSTER");
        monsterPools[type - 1].Enqueue(monster); // may have to alter for multiple scenes lol
       


        yield return new WaitForEndOfFrame();

    }



    #endregion





}