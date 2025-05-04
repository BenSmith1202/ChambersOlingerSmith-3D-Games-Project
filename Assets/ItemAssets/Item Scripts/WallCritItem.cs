using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "WallCritItem", menuName = "Items/OnHit/WallCrit")]
public class WallCritItem : ItemInstance
{
    public float critChanceMod = 1.5f;

    public override TriggerType TriggerCategory => TriggerType.OnHit;

    public override void OnTrigger(EntityStats stats, TriggerContext context)
    {
        GameObject target = context.target;
        Attack atk = context.atk;
        GameObject myself = atk.owner;

        // if player is wallrunning, multiply crit chance by [critChanceMod].
        PlayerControllerScript pcs = myself.GetComponent<PlayerControllerScript>();
        if (pcs == null)
        {
            return;
        }
        // if wallrunning
        if (pcs.movementState == PlayerControllerScript.MovementState.wallrunning)
        {
            atk.critChance *= critChanceMod;
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
