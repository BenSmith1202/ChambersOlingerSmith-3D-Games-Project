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

            // send a raycast downward to hit the floor (groundlayer) with a max range of 8 to get that location
            RaycastHit hit;
            Vector3 spawnPos = context.target.transform.position;
            if (Physics.Raycast(context.target.transform.position, Vector3.down, out hit, 8f, LayerMask.GetMask("GroundLayer")))
            {
                // If the raycast hits the ground layer, set the position to the hit point
                spawnPos = hit.point;
            }
            
            WorldHazardScript whs = Instantiate(beamPrefab, spawnPos, Quaternion.identity).GetComponent<WorldHazardScript>();
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
