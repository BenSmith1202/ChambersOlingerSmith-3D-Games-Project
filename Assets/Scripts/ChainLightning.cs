using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ChainLightning : MonoBehaviour
{
    public GameObject lightningBoltPrefab;
    public GameObject owner; // The entity that owns this chain lightning effect
    public GameObject firstTarget;
    GameObject secondTarget;
    public int bouncesLeft = 1; // Number of bounces left before stopping the chain lightning
    List<GameObject> hitTargets;
    public float bounceDistance = 5f; // Maximum distance for the next target to be hit
    public int damage = 10; // Damage dealt by the lightning bolt
    public float damageFalloff = 0.8f; // Damage reduction for each bounce
    float procChanceMultiplier = 1f; // percent of owners proc chance

    // Start is called before the first frame update
    void Start()
    {
        hitTargets = new List<GameObject>();
        if (firstTarget == null)
        {
            Debug.LogError("First target is not set. Please assign a target in the inspector.");
            Destroy(gameObject); // Destroy this object if no target is assigned
        }
        
        if (bouncesLeft > 0)
        {
            TriggerChainLightning();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerChainLightning()
    {
        if (bouncesLeft <= 0 || damage <= 0)
        {
            Destroy(gameObject); // Destroy this object if no bounces left or damage is zero
            return; // No more bounces left

        }

        Collider[] targetCandidates = Physics.OverlapSphere(firstTarget.transform.position, bounceDistance);
        foreach (Collider col in targetCandidates)
        {
            // Check if the hit object is a valid target (has not already been hit)
            if (!hitTargets.Contains(col.gameObject))
            {
                float distance = bounceDistance;
                EntityStats stats = col.gameObject.GetComponent<EntityStats>();
                if (stats == null || stats.isDead) continue; // Skip if the target is null or dead
                if (!ShouldAffectTarget(stats)) continue; // Skip if the target should not be affected

                //otherwise, see if its the closest target
                float targetDistance = Vector3.Distance(firstTarget.transform.position, col.gameObject.transform.position);
                if (targetDistance <= distance)
                {
                    // if so set it as the second target
                    secondTarget = col.gameObject;
                    distance = targetDistance;
                }
            }
        }

        if (secondTarget != null)
        {
            // secondTarget now points to the closest valid lightning target

            // create lightning effect
            LightningBolt lightningBolt = Instantiate(lightningBoltPrefab, firstTarget.transform.position, Quaternion.identity).GetComponent<LightningBolt>();
            lightningBolt.Initialize(firstTarget.transform.position, secondTarget.transform.position); // Initialize the lightning bolt with start and end points
            // lightning bolt deletes itself after a short time

            // deal damage
            EntityStats oStats = owner.GetComponent<EntityStats>();
            Attack attack = new Attack(owner, damage, oStats.getCritChance(), 0, procChanceMultiplier); // Create an attack object with the specified damage
            secondTarget.GetComponent<EntityStats>().TakeHit(attack); // Apply the attack to the target

            //change parameter values for the chain values before calculating next bounce.
            procChanceMultiplier = procChanceMultiplier * damageFalloff; // Apply proc chance falloff
            hitTargets.Add(secondTarget); // Add the target to the hit list
            firstTarget = secondTarget; // Set the new first target for the next bounce
            bouncesLeft --; // Decrease the number of bounces left
            damage = Mathf.RoundToInt(damage * damageFalloff); // Apply damage falloff
            secondTarget = null; // Reset second target for the next bounce

            TriggerChainLightning(); // Recursively call to trigger the next bounce
        }

        else
        {
            //otherwise, no valid targets, so lightning stops.
            Destroy(gameObject); // Destroy this object if no valid targets found
        }
    }

    public bool ShouldAffectTarget(EntityStats targetStats)
    {

        GameObject targetObject = targetStats.gameObject;

        // Case 1: No owner - Hazard affects everyone
        if (owner == null)
        {
            return true;
        }

        // --- Owner exists, check teams ---

        // Check if the target is the owner - owners never hit themselves unless FF is on
        if (targetObject == owner)
        {
            return false;
        }


        bool ownerIsPlayer = owner.GetComponent<PlayerControllerScript>() != null; // Assuming PlayerControllerScript marks the player
        bool targetIsPlayer = targetObject.CompareTag("Player") && targetObject.GetComponent<PlayerControllerScript>() != null; // Check tag and script

        // Case 2: Owner is an Enemy (No PlayerControllerScript) - Affects only the Player
        if (!ownerIsPlayer)
        {
            return targetIsPlayer;
        }

        // Case 3: Owner is the Player - Affects only non-Players
        if (ownerIsPlayer)
        {
            return !targetIsPlayer;
        }

        // Default case (shouldn't be reached if logic is sound)
        return true;
    }
}
