using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "Matter Condenser", menuName = "Items/OnKill/MatterCondenser")]
public class MatterCondenserItem : ItemInstance
{
    public float chance = 0.1f; // 10% chance to trigger
    public GameObject blackHolePrefab; // The black hole prefab to spawn

    public override TriggerType TriggerCategory => TriggerType.OnKill;

    public override void OnTrigger(EntityStats myStats, TriggerContext context)
    {
        if (Random.value < chance)
        {
            //get a reference to the lethal attack
            Attack atk = context.atk;

            if (atk.critLevel > 1) // On hypercrit
            {
                //make black hole
                WorldHazardScript whs = Instantiate(blackHolePrefab, context.target.transform.position, Quaternion.identity).GetComponent<WorldHazardScript>();
                whs.owner = atk.owner; // Set the owner of the black hole to the owner of the attack
            }
            
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
