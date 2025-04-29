using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "Macrowave", menuName = "Items/OnKill/Macrowave")]
public class MacrowaveItem : ItemInstance
{
    public float chance = 0.05f; // 5% chance to trigger
    public GameObject beamPrefab; // The black hole prefab to spawn

    public override TriggerType TriggerCategory => TriggerType.OnKill;

    public override void OnTrigger(EntityStats myStats, TriggerContext context)
    {
        if (Random.value < chance)
        {
            //get a reference to the lethal attack
            Attack atk = context.atk;
            
            //make black hole
            WorldHazardScript whs = Instantiate(beamPrefab, context.target.transform.position, Quaternion.identity).GetComponent<WorldHazardScript>();
            whs.owner = atk.owner; // Set the owner of the black hole to the owner of the attack
            whs.damagePerTick = atk.damage / 2; // Set the damage per tick to the attack damage
            whs.procCoefficient = atk.procCoef / 10; // Set the proc coefficient to half the attack proc coefficient

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
