using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicObjectiveScript : MonoBehaviour
{
    LogicManager logicManager;

    [Header("Spawn Settings")]
    [Tooltip("Multiplier applied to each room's starting credits when triggered")]
    public float creditMultiplier = 1f;

    void Start()
    {
        logicManager = GameObject.FindWithTag("LogicManager").GetComponent<LogicManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerControllerScript>() != null)
        {
            logicManager.objectiveComplete = true;
            Debug.Log("MacGuffin obtained!");


            // Trigger all room spawners
            TriggerAllRoomSpawners();

            // Play sounds or particle effects and enrage enemies
            Destroy(gameObject);
        }
    }

    private void TriggerAllRoomSpawners()
    {
        // Find all RoomSpawner components in the scene
        RoomSpawner[] allSpawners = FindObjectsOfType<RoomSpawner>();
        SpawnDirector director = GameObject.FindWithTag("SpawnDirector")?.GetComponent<SpawnDirector>();

        if (director == null)
        {
            Debug.LogWarning("No SpawnDirector found in scene!");
            return;
        }

        foreach (RoomSpawner spawner in allSpawners)
        {
            // Use each spawner's StartingCredits with our multiplier
            float creditsToSpend = spawner.StartingCredits * creditMultiplier;
            if (creditsToSpend > 0)
            {
                spawner.AttemptSpawns(director, creditsToSpend);
            }
        }
    }
}