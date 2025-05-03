using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] localMonsterPrefabs;
    public Transform[] flyingEnemySpawns;
    public Transform[] rangedEnemySpawns;
    public Transform[] groundedEnemySpawns;
    public float spawnRadiusCheck = 1f;
    public LayerMask obstacleLayers;

    [Tooltip("Credits to spend on initial room spawn")]
    public float StartingCredits = 0f; // NEW: Added starting credits variable

    private List<SpawnCard> localSpawnCards = new List<SpawnCard>();
    private List<GameObject> activeMonsters = new List<GameObject>();

    void Start()
    {
        InitializeLocalSpawnCards();


        StartCoroutine(DelayedStart());
    }



    private IEnumerator DelayedStart()
    {
        yield return null; // Waits one frame, allowing all Start methods to complete

        // NEW: Initial spawn if starting credits > 0
        if (StartingCredits > 0f)
        {
            SpawnDirector director = GameObject.FindWithTag("SpawnDirector")?.GetComponent<SpawnDirector>();
            if (director != null)
            {
                AttemptSpawns(director, StartingCredits);
            }
        }
        // Your code here runs after all other Start methods
    }

    void InitializeLocalSpawnCards()
    {
        foreach (GameObject prefab in localMonsterPrefabs)
        {
            SpawnCard card = prefab.GetComponent<SpawnCard>();
            if (card != null) localSpawnCards.Add(card);
        }
    }

    public float CreditMultiplyer = 1;
    /// <summary>
    /// Attempts to spawn monsters and returns total credits spent
    /// </summary>
    public float AttemptSpawns(SpawnDirector director, float availableCredits)
    {
        print("SPAWNING");
        if (localSpawnCards.Count == 0) return 0f;

        availableCredits = availableCredits * CreditMultiplyer;

        float creditsSpent = 0f;
        SpawnCard selectedCard = SelectSpawnCard(availableCredits);

        while (selectedCard != null)
        {
            SpawnMonster(director, selectedCard);
            creditsSpent += selectedCard.creditCost;
            availableCredits -= selectedCard.creditCost;
            selectedCard = SelectSpawnCard(availableCredits);
        }

        return creditsSpent;
    }

    SpawnCard SelectSpawnCard(float availableCredits)
    {
        float totalWeight = 0f;
        List<SpawnCard> validCards = new List<SpawnCard>();

        foreach (SpawnCard card in localSpawnCards)
        {
            if (card.creditCost <= availableCredits)
            {
                validCards.Add(card);
                totalWeight += card.weight;
            }
        }

        if (validCards.Count == 0) return null;

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (SpawnCard card in validCards)
        {
            cumulativeWeight += card.weight;
            if (randomValue <= cumulativeWeight)
            {
                return card;
            }
        }

        return null;
    }

    void SpawnMonster(SpawnDirector director, SpawnCard card)
    {
        int prefabIndex = System.Array.IndexOf(director.monsterPrefabs, card.gameObject);
        if (prefabIndex < 0) return;

        GameObject monster = director.GetMonsterFromPool(prefabIndex);
        if (monster == null) return;

        Transform spawnPoint = GetValidSpawnPoint(card);
        if (spawnPoint != null)
        {
            monster.transform.position = spawnPoint.position;
            monster.transform.rotation = spawnPoint.rotation;
            monster.SetActive(true);
            if (monster.GetComponent<SpawnInScript>())
            {
                monster.GetComponent<SpawnInScript>().StartSpawnSequence();
            }
            activeMonsters.Add(monster);
        }
        else
        {
            director.RegisterKill(monster, prefabIndex);
        }
    }

    Transform GetValidSpawnPoint(SpawnCard card)
    {
        Transform[] possibleSpawns = GetSpawnCategory(card);
        if (possibleSpawns == null || possibleSpawns.Length == 0) return null;

        for (int i = 0; i < possibleSpawns.Length; i++)
        {
            int randomIndex = Random.Range(0, possibleSpawns.Length);
            Transform spawnPoint = possibleSpawns[randomIndex];

            if (!Physics.CheckSphere(spawnPoint.position, spawnRadiusCheck, obstacleLayers))
            {
                return spawnPoint;
            }
        }

        return possibleSpawns.Length > 0 ? possibleSpawns[0] : null;
    }

    Transform[] GetSpawnCategory(SpawnCard card)
    {
        if (card.isFlyer && flyingEnemySpawns != null && flyingEnemySpawns.Length > 0)
            return flyingEnemySpawns;
        if (card.isRanged && rangedEnemySpawns != null && rangedEnemySpawns.Length > 0)
            return rangedEnemySpawns;
        if (groundedEnemySpawns != null && groundedEnemySpawns.Length > 0)
            return groundedEnemySpawns;
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        // Only draw gizmos for non-empty arrays
        if (flyingEnemySpawns != null && flyingEnemySpawns.Length > 0)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in flyingEnemySpawns)
            {
                if (point != null) Gizmos.DrawSphere(point.position, 0.5f);
            }
        }

        if (rangedEnemySpawns != null && rangedEnemySpawns.Length > 0)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in rangedEnemySpawns)
            {
                if (point != null) Gizmos.DrawSphere(point.position, 0.5f);
            }
        }

        if (groundedEnemySpawns != null && groundedEnemySpawns.Length > 0)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in groundedEnemySpawns)
            {
                if (point != null) Gizmos.DrawSphere(point.position, 0.5f);
            }
        }
    }
}