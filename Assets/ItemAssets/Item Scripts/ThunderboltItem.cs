using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "ThunderboltItem", menuName = "Items/OnHit/Thunderbolt")]
public class ThunderboltItem : ItemInstance
{
    
    public float chance = 0.3f; // chance of triggering
    public float boltFalloff = 0.8f; // falloff for damage and proc chance
    public int maxBounces = 3; // max bounces for the chain lightning
    public float maxBounceDistance = 5f; // max distance for the chain lightning to bounce

    public GameObject chainLightningPrefab; // Prefab for the lightning effect

    public override TriggerType TriggerCategory => TriggerType.OnHit;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        GameObject target = context.target;
        Attack atk = context.atk;
        GameObject myself = atk.owner;
        EntityStats targetStats = target.GetComponent<EntityStats>();
        //play sound?

        
        if (Random.value < chance) // If the random value is greater than the chance, do not proc
        {
            Vector3 spawnPos = target.transform.position;
            ChainLightning chainStart = Instantiate(chainLightningPrefab, spawnPos, Quaternion.identity, target.transform).GetComponent<ChainLightning>();
            chainStart.owner = myself; // Set the owner of the chain lightning to the player
            chainStart.firstTarget = target; // Set the first target to the one that was hit
            chainStart.damage = Mathf.FloorToInt(atk.damage * boltFalloff); // Set the damage for the chain lightning
            chainStart.bouncesLeft = maxBounces; // Set the number of bounces
            chainStart.bounceDistance = maxBounceDistance; // Set the bounce distance
            chainStart.damageFalloff = boltFalloff; // Set the falloff
            Debug.Log("Thunderbolt proc'd on " + target.name + " for " + atk.damage + " damage.");
        }
        
    }

    public override void OnAcquire(EntityStats stats)
    {

    }

    // does the inverse of the method above
    public override void OnRemove(EntityStats stats)
    {

    }
}
